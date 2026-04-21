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
    /// Controller per la gestione delle Rettifiche del Conto Economico
    /// </summary>
    [Authorize]
    public class PstreeListaRettificheController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<PstreeListaRettificheController> _logger;

        public PstreeListaRettificheController(ApplicationDbContext context, ILogger<PstreeListaRettificheController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: ListaRettifiche
        public async Task<IActionResult> Index(
            int? filterIdCodiceConto,
            int? filterMese,
            int? filterAnno,
            int? filterIdFamiglia,
            int? filterIdSede)
        {
            // Default: anno corrente
            var annoDefault = filterAnno ?? DateTime.Now.Year;
            
            // Default: sede con id più piccolo
            int? sedeDefault = filterIdSede;
            if (!filterIdSede.HasValue)
            {
                var primaSede = await _context.PstreeListaSedi.OrderBy(s => s.Id).FirstOrDefaultAsync();
                sedeDefault = primaSede?.Id;
            }
            
            // Default: famiglia Analitica (Id = 0)
            var famigliaDefault = filterIdFamiglia ?? 0;
            
            var query = _context.PstreeListaRettifiche.AsQueryable();

            // Applica filtri
            if (filterIdCodiceConto.HasValue)
            {
                query = query.Where(r => r.IdCodiceConto == filterIdCodiceConto.Value);
            }

            if (filterMese.HasValue)
            {
                query = query.Where(r => r.Mese == filterMese.Value);
            }

            // Applica sempre filtro anno (default = anno corrente)
            query = query.Where(r => r.Anno == annoDefault);

            // Applica sempre filtro famiglia (default = Analitica)
            query = query.Where(r => r.IdFamiglia == famigliaDefault);

            // Applica sempre filtro sede se disponibile
            if (sedeDefault.HasValue)
            {
                query = query.Where(r => r.IdSede == sedeDefault.Value);
            }

            var rettifiche = await query
                .OrderByDescending(r => r.Anno)
                .ThenByDescending(r => r.Mese)
                .ThenBy(r => r.IdCodiceConto)
                .ToListAsync();

            // Calcola totali
            ViewBag.TotaleDare = rettifiche.Sum(r => r.Dare);
            ViewBag.TotaleAvere = rettifiche.Sum(r => r.Avere);
            ViewBag.TotaleSaldo = rettifiche.Sum(r => r.Saldo);

            // Carica i nomi delle famiglie e sedi per la visualizzazione
            var idFamiglie = rettifiche.Select(r => r.IdFamiglia).Distinct().ToList();
            var famiglie = await _context.PstreeListaFamiglie
                .Where(f => idFamiglie.Contains(f.Id))
                .ToDictionaryAsync(f => f.Id, f => f.NomeFamiglia);
            ViewBag.NomiFamiglie = famiglie;

            var idSedi = rettifiche.Select(r => r.IdSede).Distinct().ToList();
            var sedi = await _context.PstreeListaSedi
                .Where(s => idSedi.Contains(s.Id))
                .ToDictionaryAsync(s => s.Id, s => s.Sede);
            ViewBag.NomiSedi = sedi;

            // Carica le descrizioni dei conti economici
            var idConti = rettifiche.Select(r => r.IdCodiceConto).Distinct().ToList();
            var conti = await _context.PstreeStrutturaContoEconomico
                .Where(c => idConti.Contains(c.IdCodiceConto))
                .ToDictionaryAsync(c => c.IdCodiceConto, c => c.DescrizioneConto);
            ViewBag.DescrizioniConti = conti;

            // Filtri correnti (usa i default se non specificati)
            ViewBag.FilterIdCodiceConto = filterIdCodiceConto;
            ViewBag.FilterMese = filterMese;
            ViewBag.FilterAnno = annoDefault;
            ViewBag.FilterIdFamiglia = famigliaDefault;
            ViewBag.FilterIdSede = sedeDefault;

            // Popola dropdown filtri (solo voci con VoceRettifica = true)
            await PopulateContiEconomiciRettificaFiltroDropdownAsync(filterIdCodiceConto);
            PopulateMesiDropdown(filterMese);
            await PopulateAnniDropdownAsync(annoDefault);
            await PopulateFamiglieDropdownAsync(famigliaDefault);
            await PopulateSediDropdownAsync(sedeDefault);

            return View(rettifiche);
        }

        // GET: ListaRettifiche/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var rettifica = await _context.PstreeListaRettifiche
                .FirstOrDefaultAsync(r => r.Id == id);

            if (rettifica == null)
            {
                return NotFound();
            }

            // Carica il conto economico correlato
            var conto = await _context.PstreeStrutturaContoEconomico
                .FirstOrDefaultAsync(s => s.IdCodiceConto == rettifica.IdCodiceConto);
            ViewBag.ContoEconomico = conto;

            // Carica famiglia e sede
            var famiglia = await _context.PstreeListaFamiglie.FirstOrDefaultAsync(f => f.Id == rettifica.IdFamiglia);
            ViewBag.Famiglia = famiglia;

            var sede = await _context.PstreeListaSedi.FirstOrDefaultAsync(s => s.Id == rettifica.IdSede);
            ViewBag.Sede = sede;

            return View(rettifica);
        }

        // GET: ListaRettifiche/Create
        public async Task<IActionResult> Create()
        {
            // Sede di default: quella con Id più piccolo
            var sedeDefault = await _context.PstreeListaSedi.OrderBy(s => s.Id).FirstOrDefaultAsync();
            var sedeDefaultId = sedeDefault?.Id;
            
            await PopulateContiEconomiciRettificaDropdownAsync();
            PopulateMesiDropdown(DateTime.Now.Month);
            await PopulateFamiglieDropdownAsync();
            await PopulateSediDropdownAsync(sedeDefaultId);

            var model = new PstreeListaRettifiche
            {
                Anno = DateTime.Now.Year,
                Mese = DateTime.Now.Month,
                Dare = 0,
                Avere = 0,
                IdSede = sedeDefaultId ?? 0
            };

            return View(model);
        }

        // POST: ListaRettifiche/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,IdCodiceConto,Dare,Avere,Mese,Anno,IdFamiglia,IdSede")] PstreeListaRettifiche rettifica)
        {
            if (ModelState.IsValid)
            {
                _context.Add(rettifica);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Rettifica per conto {rettifica.IdCodiceConto} creata con successo!";
                _logger.LogInformation("Rettifica {Id} creata per conto {IdCodiceConto}", rettifica.Id, rettifica.IdCodiceConto);

                return RedirectToAction(nameof(Index));
            }

            await PopulateContiEconomiciRettificaDropdownAsync(rettifica.IdCodiceConto);
            PopulateMesiDropdown(rettifica.Mese);
            await PopulateFamiglieDropdownAsync(rettifica.IdFamiglia);
            await PopulateSediDropdownAsync(rettifica.IdSede);
            return View(rettifica);
        }

        // POST: ListaRettifiche/CreateForAllMonths
        // Crea la stessa rettifica per tutti i 12 mesi dell'anno
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateForAllMonths(int IdCodiceConto, decimal Dare, decimal Avere, int Anno, int IdFamiglia, int IdSede)
        {
            try
            {
                // Trova e cancella rettifiche esistenti per questo conto/anno/famiglia/sede
                var rettificheEsistenti = await _context.PstreeListaRettifiche
                    .Where(r => r.IdCodiceConto == IdCodiceConto 
                             && r.Anno == Anno 
                             && r.IdFamiglia == IdFamiglia 
                             && r.IdSede == IdSede)
                    .ToListAsync();
                
                int eliminati = rettificheEsistenti.Count;
                
                if (eliminati > 0)
                {
                    _context.PstreeListaRettifiche.RemoveRange(rettificheEsistenti);
                    _logger.LogInformation("Eliminate {Count} rettifiche esistenti per conto {Conto}, anno {Anno}, famiglia {Fam}, sede {Sede}",
                        eliminati, IdCodiceConto, Anno, IdFamiglia, IdSede);
                }
                
                // Crea 12 nuove rettifiche (una per ogni mese)
                for (int mese = 1; mese <= 12; mese++)
                {
                    var nuovaRettifica = new PstreeListaRettifiche
                    {
                        IdCodiceConto = IdCodiceConto,
                        Dare = Dare,
                        Avere = Avere,
                        Mese = mese,
                        Anno = Anno,
                        IdFamiglia = IdFamiglia,
                        IdSede = IdSede
                    };
                    _context.PstreeListaRettifiche.Add(nuovaRettifica);
                }
                
                await _context.SaveChangesAsync();
                
                var messaggio = $"Create 12 rettifiche per tutti i mesi del {Anno} per il conto {IdCodiceConto}!";
                if (eliminati > 0)
                {
                    messaggio += $" (Eliminate {eliminati} rettifiche precedenti)";
                }
                
                TempData["SuccessMessage"] = messaggio;
                _logger.LogInformation("Create 12 rettifiche per conto {Conto}, anno {Anno}", IdCodiceConto, Anno);
                
                return RedirectToAction(nameof(Index), new { filterAnno = Anno, filterIdCodiceConto = IdCodiceConto, filterIdFamiglia = IdFamiglia, filterIdSede = IdSede });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante la creazione delle rettifiche per tutti i mesi");
                TempData["ErrorMessage"] = "Errore durante la creazione: " + ex.Message;
                return RedirectToAction(nameof(Create));
            }
        }

        // GET: ListaRettifiche/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var rettifica = await _context.PstreeListaRettifiche.FindAsync(id);
            if (rettifica == null)
            {
                return NotFound();
            }

            // Carica la descrizione del conto per la visualizzazione
            var conto = await _context.PstreeStrutturaContoEconomico
                .FirstOrDefaultAsync(s => s.IdCodiceConto == rettifica.IdCodiceConto);
            ViewBag.DescrizioneConto = conto?.DescrizioneConto ?? "";

            await PopulateContiEconomiciRettificaDropdownAsync(rettifica.IdCodiceConto);
            PopulateMesiDropdown(rettifica.Mese);
            await PopulateFamiglieDropdownAsync(rettifica.IdFamiglia);
            await PopulateSediDropdownAsync(rettifica.IdSede);

            return View(rettifica);
        }

        // POST: ListaRettifiche/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,IdCodiceConto,Dare,Avere,Mese,Anno,IdFamiglia,IdSede")] PstreeListaRettifiche rettifica)
        {
            if (id != rettifica.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(rettifica);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = $"Rettifica per conto {rettifica.IdCodiceConto} modificata con successo!";
                    _logger.LogInformation("Rettifica {Id} modificata", rettifica.Id);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!RettificaExists(rettifica.Id))
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

            await PopulateContiEconomiciRettificaDropdownAsync(rettifica.IdCodiceConto);
            PopulateMesiDropdown(rettifica.Mese);
            await PopulateFamiglieDropdownAsync(rettifica.IdFamiglia);
            await PopulateSediDropdownAsync(rettifica.IdSede);
            return View(rettifica);
        }

        // GET: ListaRettifiche/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var rettifica = await _context.PstreeListaRettifiche
                .FirstOrDefaultAsync(r => r.Id == id);

            if (rettifica == null)
            {
                return NotFound();
            }

            // Carica famiglia e sede
            var famiglia = await _context.PstreeListaFamiglie.FirstOrDefaultAsync(f => f.Id == rettifica.IdFamiglia);
            ViewBag.Famiglia = famiglia;

            var sede = await _context.PstreeListaSedi.FirstOrDefaultAsync(s => s.Id == rettifica.IdSede);
            ViewBag.Sede = sede;

            // Carica la descrizione del conto
            var conto = await _context.PstreeStrutturaContoEconomico
                .FirstOrDefaultAsync(s => s.IdCodiceConto == rettifica.IdCodiceConto);
            ViewBag.DescrizioneConto = conto?.DescrizioneConto ?? "N/D";

            return View(rettifica);
        }

        // POST: ListaRettifiche/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var rettifica = await _context.PstreeListaRettifiche.FindAsync(id);
            if (rettifica == null)
            {
                return NotFound();
            }

            var idConto = rettifica.IdCodiceConto;
            _context.PstreeListaRettifiche.Remove(rettifica);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Rettifica per conto {idConto} eliminata con successo!";
            _logger.LogInformation("Rettifica {Id} eliminata", id);

            return RedirectToAction(nameof(Index));
        }

        // ========================================
        // AJAX: Ottieni descrizione conto
        // ========================================
        [HttpGet]
        public async Task<IActionResult> GetDescrizioneConto(int idCodiceConto)
        {
            var conto = await _context.PstreeStrutturaContoEconomico
                .FirstOrDefaultAsync(s => s.IdCodiceConto == idCodiceConto);

            if (conto != null)
            {
                return Json(new { descrizione = conto.DescrizioneConto });
            }

            return Json(new { descrizione = "" });
        }

        // ========================================
        // METODI HELPER
        // ========================================

        private bool RettificaExists(int id)
        {
            return _context.PstreeListaRettifiche.Any(r => r.Id == id);
        }

        /// <summary>
        /// Popola dropdown con solo i conti VoceRettifica = true (per filtro Index)
        /// </summary>
        private async Task PopulateContiEconomiciRettificaFiltroDropdownAsync(int? selectedValue = null)
        {
            var conti = await _context.PstreeStrutturaContoEconomico
                .Where(s => s.VoceRettifica == true)
                .OrderBy(s => s.IdCodiceConto)
                .ToListAsync();

            var items = conti.Select(c => new SelectListItem
            {
                Value = c.IdCodiceConto.ToString(),
                Text = c.DescrizioneCompleta,
                Selected = selectedValue.HasValue && c.IdCodiceConto == selectedValue.Value
            }).ToList();

            items.Insert(0, new SelectListItem { Value = "", Text = "-- Tutti --" });

            ViewBag.ContiEconomici = items;
        }

        /// <summary>
        /// Popola dropdown con solo i conti che hanno VoceRettifica = true (per creazione/modifica)
        /// </summary>
        private async Task PopulateContiEconomiciRettificaDropdownAsync(int? selectedValue = null)
        {
            var conti = await _context.PstreeStrutturaContoEconomico
                .Where(s => s.VoceRettifica == true)
                .OrderBy(s => s.IdCodiceConto)
                .ToListAsync();

            var items = conti.Select(c => new SelectListItem
            {
                Value = c.IdCodiceConto.ToString(),
                Text = c.DescrizioneCompleta,
                Selected = selectedValue.HasValue && c.IdCodiceConto == selectedValue.Value
            }).ToList();

            items.Insert(0, new SelectListItem { Value = "", Text = "-- Seleziona un conto --" });

            ViewBag.ContiEconomiciRettifica = items;
        }

        private void PopulateMesiDropdown(int? selectedValue = null)
        {
            var mesi = new List<SelectListItem>
            {
                new SelectListItem { Value = "1", Text = "Gennaio", Selected = selectedValue == 1 },
                new SelectListItem { Value = "2", Text = "Febbraio", Selected = selectedValue == 2 },
                new SelectListItem { Value = "3", Text = "Marzo", Selected = selectedValue == 3 },
                new SelectListItem { Value = "4", Text = "Aprile", Selected = selectedValue == 4 },
                new SelectListItem { Value = "5", Text = "Maggio", Selected = selectedValue == 5 },
                new SelectListItem { Value = "6", Text = "Giugno", Selected = selectedValue == 6 },
                new SelectListItem { Value = "7", Text = "Luglio", Selected = selectedValue == 7 },
                new SelectListItem { Value = "8", Text = "Agosto", Selected = selectedValue == 8 },
                new SelectListItem { Value = "9", Text = "Settembre", Selected = selectedValue == 9 },
                new SelectListItem { Value = "10", Text = "Ottobre", Selected = selectedValue == 10 },
                new SelectListItem { Value = "11", Text = "Novembre", Selected = selectedValue == 11 },
                new SelectListItem { Value = "12", Text = "Dicembre", Selected = selectedValue == 12 }
            };

            ViewBag.Mesi = mesi;
        }

        private async Task PopulateAnniDropdownAsync(int? selectedValue = null)
        {
            // Ottieni anni dalle rettifiche esistenti + anno corrente + anno precedente/successivo
            var anniDb = await _context.PstreeListaRettifiche
                .Select(r => r.Anno)
                .Distinct()
                .ToListAsync();

            var annoCorrente = DateTime.Now.Year;
            var anni = anniDb
                .Union(new[] { annoCorrente - 1, annoCorrente, annoCorrente + 1 })
                .Distinct()
                .OrderByDescending(a => a)
                .ToList();

            var items = anni.Select(a => new SelectListItem
            {
                Value = a.ToString(),
                Text = a.ToString(),
                Selected = selectedValue.HasValue && a == selectedValue.Value
            }).ToList();

            items.Insert(0, new SelectListItem { Value = "", Text = "-- Tutti --" });

            ViewBag.Anni = items;
        }

        private async Task PopulateFamiglieDropdownAsync(int? selectedValue = null)
        {
            // Ottieni le famiglie dalla tabella ListaFamiglie
            var famiglie = await _context.PstreeListaFamiglie
                .OrderBy(f => f.Id)
                .ToListAsync();

            var items = famiglie.Select(f => new SelectListItem
            {
                Value = f.Id.ToString(),
                Text = $"{f.Id} - {f.NomeFamiglia}",
                Selected = selectedValue.HasValue && f.Id == selectedValue.Value
            }).ToList();

            items.Insert(0, new SelectListItem { Value = "", Text = "-- Seleziona --" });

            ViewBag.Famiglie = items;
        }

        private async Task PopulateSediDropdownAsync(int? selectedValue = null)
        {
            // Ottieni le sedi dalla tabella ListaSedi
            var sedi = await _context.PstreeListaSedi
                .OrderBy(s => s.Id)
                .ToListAsync();

            var items = sedi.Select(s => new SelectListItem
            {
                Value = s.Id.ToString(),
                Text = $"{s.Id} - {s.Sede}",
                Selected = selectedValue.HasValue && s.Id == selectedValue.Value
            }).ToList();

            items.Insert(0, new SelectListItem { Value = "", Text = "-- Seleziona --" });

            ViewBag.Sedi = items;
        }
    }
}
