using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using AiDbMaster.Data;
using AiDbMaster.Models;
using AiDbMaster.ViewModels;
using Microsoft.AspNetCore.Authorization;

namespace AiDbMaster.Controllers
{
    /// <summary>
    /// Controller per la gestione delle Associazioni tra Piano dei Conti e StrutturaContoEconomico
    /// </summary>
    [Authorize]
    public class PstreeAssociazioniCEController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<PstreeAssociazioniCEController> _logger;

        public PstreeAssociazioniCEController(ApplicationDbContext context, ILogger<PstreeAssociazioniCEController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: AssociazioniCE
        public async Task<IActionResult> Index(string? searchCodicePdC, int? filterIdCodiceConto)
        {
            // ========================================
            // STATISTICHE PER LE CARD RIEPILOGATIVE
            // (Solo conti di tipo 'E' - Economico)
            // ========================================

            // Tutti i codici PdC UNICI di tipo ECONOMICO presenti nel Piano dei Conti
            // Esclude i conti che iniziano per '60' (non devono essere associati)
            var tuttiCodiciPdC = await _context.PstreeListaPianoDeiConti
                .Where(p => p.TipoPdC == "E" && !p.CodicePdC.StartsWith("60"))
                .Select(p => p.CodicePdC)
                .ToListAsync();

            var totalePdCUnici = tuttiCodiciPdC.Count;

            // Codici PdC già associati (solo quelli di tipo E)
            var codiciPdCAssociati = await _context.PstreeAssociazioniCE
                .Where(a => _context.PstreeListaPianoDeiConti.Any(p => p.CodicePdC == a.CodicePdC && p.TipoPdC == "E"))
                .Select(a => a.CodicePdC)
                .Distinct()
                .ToListAsync();

            var pdcAssociati = codiciPdCAssociati.Count;

            // Codici PdC con flag "Non Associare" = true (da escludere dalla lista non associati)
            var codiciNonAssociare = await _context.PstreeListaPianoDeiConti
                .Where(p => p.TipoPdC == "E" && p.NonAssociare)
                .Select(p => p.CodicePdC)
                .ToListAsync();

            // Codici PdC NON associati (solo tipo E, esclusi quelli con NonAssociare = true)
            var codiciPdCNonAssociati = tuttiCodiciPdC
                .Where(cs => !codiciPdCAssociati.Contains(cs) && !codiciNonAssociare.Contains(cs))
                .OrderBy(cs => cs)
                .ToList();

            var pdcNonAssociati = codiciPdCNonAssociati.Count;

            // Percentuale di copertura (esclude i conti "Non Associare" dal calcolo)
            var contiDaAssociare = totalePdCUnici - codiciNonAssociare.Count;
            var percentualeCopertura = contiDaAssociare > 0 
                ? Math.Round((double)pdcAssociati / contiDaAssociare * 100, 1) 
                : 0;

            // Passa le statistiche alla View
            ViewBag.TotalePdCUnici = totalePdCUnici;
            ViewBag.PdCAssociati = pdcAssociati;
            ViewBag.PdCNonAssociati = pdcNonAssociati;
            ViewBag.PercentualeCopertura = percentualeCopertura;
            ViewBag.CodiciPdCNonAssociati = codiciPdCNonAssociati;

            // Statistiche e dati per i conti "Non Associare"
            ViewBag.ContiNonAssociare = codiciNonAssociare.Count;
            ViewBag.CodiciNonAssociare = codiciNonAssociare;

            // Carica le descrizioni dei PdC non associati (solo tipo E)
            var pdcNonAssociatiDict = await _context.PstreeListaPianoDeiConti
                .Where(p => codiciPdCNonAssociati.Contains(p.CodicePdC) && p.TipoPdC == "E")
                .ToDictionaryAsync(p => p.CodicePdC, p => p);
            ViewBag.PdCNonAssociatiDict = pdcNonAssociatiDict;

            // Carica le descrizioni dei PdC "Non Associare"
            var pdcNonAssociareDict = await _context.PstreeListaPianoDeiConti
                .Where(p => codiciNonAssociare.Contains(p.CodicePdC))
                .ToDictionaryAsync(p => p.CodicePdC, p => p);
            ViewBag.PdCNonAssociareDict = pdcNonAssociareDict;

            // ========================================
            // QUERY PRINCIPALE CON FILTRI
            // ========================================

            var query = _context.PstreeAssociazioniCE.AsQueryable();

            // Filtro per CodicePdC
            if (!string.IsNullOrEmpty(searchCodicePdC))
            {
                query = query.Where(a => a.CodicePdC.Contains(searchCodicePdC));
            }

            // Filtro per IdCodiceConto
            if (filterIdCodiceConto.HasValue)
            {
                query = query.Where(a => a.IdCodiceConto == filterIdCodiceConto.Value);
            }

            var associazioni = await query
                .OrderBy(a => a.CodicePdC)
                .ToListAsync();

            // Carica i dati correlati per mostrare le descrizioni
            var codiciConto = associazioni.Select(a => a.IdCodiceConto).Distinct().ToList();
            var contiEconomici = await _context.PstreeStrutturaContoEconomico
                .Where(s => codiciConto.Contains(s.IdCodiceConto))
                .ToDictionaryAsync(s => s.IdCodiceConto, s => s.DescrizioneCompleta);

            ViewBag.ContiEconomici = contiEconomici;

            // Carica le descrizioni dei PdC
            var codiciPdC = associazioni.Select(a => a.CodicePdC).Distinct().ToList();
            var descrizioniPdC = await _context.PstreeListaPianoDeiConti
                .Where(p => codiciPdC.Contains(p.CodicePdC))
                .ToDictionaryAsync(p => p.CodicePdC, p => p);

            ViewBag.DescrizioniPdC = descrizioniPdC;

            // Filtri per la view
            ViewBag.SearchCodicePdC = searchCodicePdC;
            ViewBag.FilterIdCodiceConto = filterIdCodiceConto;

            // Dropdown per filtro Conto Economico
            await PopulateContoEconomicoDropdownAsync(filterIdCodiceConto);

            return View(associazioni);
        }

        // GET: AssociazioniCE/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var associazione = await _context.PstreeAssociazioniCE
                .FirstOrDefaultAsync(a => a.Id == id);

            if (associazione == null)
            {
                return NotFound();
            }

            // Carica dati correlati
            await LoadRelatedDataAsync(associazione);

            return View(associazione);
        }

        // GET: AssociazioniCE/Create
        public async Task<IActionResult> Create(string? codicePdC = null)
        {
            await PopulateCodiciPdCDropdownAsync(codicePdC);
            await PopulateContoEconomicoDropdownAsync();

            var model = new PstreeAssociazioniCE();
            if (!string.IsNullOrEmpty(codicePdC))
            {
                model.CodicePdC = codicePdC;
            }

            return View(model);
        }

        // POST: AssociazioniCE/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,CodicePdC,IdCodiceConto")] PstreeAssociazioniCE associazione)
        {
            if (ModelState.IsValid)
            {
                // Verifica che non esista già un'associazione con lo stesso CodicePdC
                var esistente = await _context.PstreeAssociazioniCE
                    .AnyAsync(a => a.CodicePdC == associazione.CodicePdC);

                if (esistente)
                {
                    ModelState.AddModelError("CodicePdC", "Questo codice PdC è già associato a un conto economico.");
                    await PopulateCodiciPdCDropdownAsync(associazione.CodicePdC);
                    await PopulateContoEconomicoDropdownAsync(associazione.IdCodiceConto);
                    return View(associazione);
                }

                _context.Add(associazione);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Associazione per '{associazione.CodicePdC}' creata con successo!";
                _logger.LogInformation("Associazione creata: CodicePdC={CodicePdC}, IdCodiceConto={IdCodiceConto}", 
                    associazione.CodicePdC, associazione.IdCodiceConto);

                return RedirectToAction(nameof(Index));
            }

            await PopulateCodiciPdCDropdownAsync(associazione.CodicePdC);
            await PopulateContoEconomicoDropdownAsync(associazione.IdCodiceConto);
            return View(associazione);
        }

        // GET: AssociazioniCE/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var associazione = await _context.PstreeAssociazioniCE.FindAsync(id);
            if (associazione == null)
            {
                return NotFound();
            }

            await PopulateCodiciPdCDropdownAsync(associazione.CodicePdC);
            await PopulateContoEconomicoDropdownAsync(associazione.IdCodiceConto);
            return View(associazione);
        }

        // POST: AssociazioniCE/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,CodicePdC,IdCodiceConto")] PstreeAssociazioniCE associazione)
        {
            if (id != associazione.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Verifica che non esista già un'altra associazione con lo stesso CodicePdC
                    var esistente = await _context.PstreeAssociazioniCE
                        .AnyAsync(a => a.CodicePdC == associazione.CodicePdC && a.Id != associazione.Id);

                    if (esistente)
                    {
                        ModelState.AddModelError("CodicePdC", "Questo codice PdC è già associato a un altro conto economico.");
                        await PopulateCodiciPdCDropdownAsync(associazione.CodicePdC);
                        await PopulateContoEconomicoDropdownAsync(associazione.IdCodiceConto);
                        return View(associazione);
                    }

                    _context.Update(associazione);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = $"Associazione per '{associazione.CodicePdC}' modificata con successo!";
                    _logger.LogInformation("Associazione modificata: Id={Id}, CodicePdC={CodicePdC}, IdCodiceConto={IdCodiceConto}", 
                        associazione.Id, associazione.CodicePdC, associazione.IdCodiceConto);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AssociazioneExists(associazione.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }

            await PopulateCodiciPdCDropdownAsync(associazione.CodicePdC);
            await PopulateContoEconomicoDropdownAsync(associazione.IdCodiceConto);
            return View(associazione);
        }

        // GET: AssociazioniCE/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var associazione = await _context.PstreeAssociazioniCE
                .FirstOrDefaultAsync(a => a.Id == id);

            if (associazione == null)
            {
                return NotFound();
            }

            // Carica dati correlati
            await LoadRelatedDataAsync(associazione);

            return View(associazione);
        }

        // POST: AssociazioniCE/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var associazione = await _context.PstreeAssociazioniCE.FindAsync(id);
            if (associazione == null)
            {
                return NotFound();
            }

            var codicePdC = associazione.CodicePdC;
            _context.PstreeAssociazioniCE.Remove(associazione);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Associazione per '{codicePdC}' eliminata con successo!";
            _logger.LogInformation("Associazione eliminata: Id={Id}, CodicePdC={CodicePdC}", id, codicePdC);

            return RedirectToAction(nameof(Index));
        }

        // ========================================
        // AZIONE PER CREAZIONE RAPIDA DA LISTA NON ASSOCIATI
        // ========================================

        // GET: AssociazioniCE/CreateQuick?codicePdC=XXX
        public async Task<IActionResult> CreateQuick(string codicePdC)
        {
            if (string.IsNullOrEmpty(codicePdC))
            {
                return RedirectToAction(nameof(Index));
            }

            // Verifica che non sia già associato
            var esistente = await _context.PstreeAssociazioniCE
                .AnyAsync(a => a.CodicePdC == codicePdC);

            if (esistente)
            {
                TempData["ErrorMessage"] = $"Il codice PdC '{codicePdC}' è già associato.";
                return RedirectToAction(nameof(Index));
            }

            // Ottieni la descrizione dal Piano dei Conti
            var pdc = await _context.PstreeListaPianoDeiConti
                .FirstOrDefaultAsync(p => p.CodicePdC == codicePdC);

            ViewBag.PianoDeiConti = pdc;
            await PopulateContoEconomicoDropdownAsync();

            var model = new PstreeAssociazioniCE { CodicePdC = codicePdC };
            return View(model);
        }

        // POST: AssociazioniCE/CreateQuick
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateQuick([Bind("CodicePdC,IdCodiceConto")] PstreeAssociazioniCE associazione)
        {
            if (ModelState.IsValid)
            {
                // Verifica che non esista già
                var esistente = await _context.PstreeAssociazioniCE
                    .AnyAsync(a => a.CodicePdC == associazione.CodicePdC);

                if (esistente)
                {
                    TempData["ErrorMessage"] = $"Il codice PdC '{associazione.CodicePdC}' è già associato.";
                    return RedirectToAction(nameof(Index));
                }

                _context.Add(associazione);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Associazione per '{associazione.CodicePdC}' creata con successo!";
                return RedirectToAction(nameof(Index));
            }

            await PopulateContoEconomicoDropdownAsync(associazione.IdCodiceConto);
            return View(associazione);
        }

        // ========================================
        // METODI HELPER
        // ========================================

        private bool AssociazioneExists(int id)
        {
            return _context.PstreeAssociazioniCE.Any(a => a.Id == id);
        }

        private async Task PopulateCodiciPdCDropdownAsync(string? selectedValue = null)
        {
            // Prendi solo i codici PdC NON ancora associati + quello selezionato (se in modifica)
            // Esclude i conti che iniziano per '60' (non devono essere associati)
            var codiciGiaAssociati = await _context.PstreeAssociazioniCE
                .Select(a => a.CodicePdC)
                .ToListAsync();

            var codiciDisponibili = await _context.PstreeListaPianoDeiConti
                .Where(p => !p.CodicePdC.StartsWith("60") && 
                           (!codiciGiaAssociati.Contains(p.CodicePdC) || p.CodicePdC == selectedValue))
                .OrderBy(p => p.CodicePdC)
                .ToListAsync();

            var items = codiciDisponibili.Select(p => new SelectListItem
            {
                Value = p.CodicePdC,
                Text = $"{p.CodicePdC} - {p.DescrizionePdC} ({p.TipoDescrizione})",
                Selected = p.CodicePdC == selectedValue
            }).ToList();

            ViewBag.CodiciPdC = items;
        }

        private async Task PopulateContoEconomicoDropdownAsync(int? selectedValue = null)
        {
            var conti = await _context.PstreeStrutturaContoEconomico
                .OrderBy(s => s.IdCodiceConto)
                .ToListAsync();

            var items = conti.Select(c => new SelectListItem
            {
                Value = c.IdCodiceConto.ToString(),
                Text = c.DescrizioneCompleta,
                Selected = selectedValue.HasValue && c.IdCodiceConto == selectedValue.Value
            }).ToList();

            items.Insert(0, new SelectListItem { Value = "", Text = "-- Seleziona un conto --" });

            ViewBag.ContiEconomico = items;
        }

        private async Task LoadRelatedDataAsync(PstreeAssociazioniCE associazione)
        {
            // Carica il conto economico correlato
            var conto = await _context.PstreeStrutturaContoEconomico
                .FirstOrDefaultAsync(s => s.IdCodiceConto == associazione.IdCodiceConto);

            ViewBag.ContoEconomico = conto;

            // Carica il Piano dei Conti
            var pdc = await _context.PstreeListaPianoDeiConti
                .FirstOrDefaultAsync(p => p.CodicePdC == associazione.CodicePdC);

            ViewBag.PianoDeiConti = pdc;
        }
    }
}
