using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using AiDbMaster.Data;
using AiDbMaster.Models;
using AiDbMaster.ViewModels;

namespace AiDbMaster.Controllers
{
    /// <summary>
    /// Controller per la gestione delle ripartizioni di import saldi sul Conto Economico
    /// (Pstree_AssociazioniImportSaldiCE): per un conto del Piano dei Conti, in un dato
    /// anno/mese/sede, definisce su quali voci di CE va ripartito il saldo e con quale percentuale.
    /// </summary>
    [Authorize]
    public class PstreeAssociazioniImportSaldiCEController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<PstreeAssociazioniImportSaldiCEController> _logger;

        private static readonly string[] NomiMesi =
        {
            "", "Gennaio", "Febbraio", "Marzo", "Aprile", "Maggio", "Giugno",
            "Luglio", "Agosto", "Settembre", "Ottobre", "Novembre", "Dicembre"
        };

        public PstreeAssociazioniImportSaldiCEController(
            ApplicationDbContext context,
            ILogger<PstreeAssociazioniImportSaldiCEController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: PstreeAssociazioniImportSaldiCE
        public async Task<IActionResult> Index(int? anno, int? sede, string? searchCodicePdC)
        {
            var annoCorrente = DateTime.Now.Year;
            var annoSelezionato = anno ?? annoCorrente;

            var query = _context.PstreeAssociazioniImportSaldiCE
                .AsNoTracking()
                .Include(a => a.PianoDeiConti)
                .Include(a => a.ContoEconomico)
                .Include(a => a.Sede)
                .Where(a => a.Anno == annoSelezionato);

            if (sede.HasValue)
            {
                query = query.Where(a => a.IdSede == sede.Value);
            }

            if (!string.IsNullOrWhiteSpace(searchCodicePdC))
            {
                query = query.Where(a => a.CodicePdC.Contains(searchCodicePdC));
            }

            var lista = await query
                .OrderBy(a => a.CodicePdC)
                .ThenBy(a => a.Mese)
                .ThenBy(a => a.IdCodiceConto)
                .ToListAsync();

            // Totale percentuale per gruppo (CodicePdC + Sede + Anno + Mese): deve fare 100%
            var totaliGruppo = lista
                .GroupBy(a => (a.CodicePdC, a.IdSede, a.Anno, a.Mese))
                .ToDictionary(g => g.Key, g => g.Sum(a => a.Percentuale));

            ViewBag.TotaliGruppo = totaliGruppo;
            ViewBag.Anno = annoSelezionato;
            ViewBag.SedeSelezionata = sede;
            ViewBag.SearchCodicePdC = searchCodicePdC;
            ViewBag.AnniDisponibili = Enumerable.Range(annoCorrente - 3, 6).OrderByDescending(a => a).ToList();
            await PopolaSediAsync(sede);

            return View(lista);
        }

        // GET: PstreeAssociazioniImportSaldiCE/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var item = await _context.PstreeAssociazioniImportSaldiCE
                .AsNoTracking()
                .Include(a => a.PianoDeiConti)
                .Include(a => a.ContoEconomico)
                .Include(a => a.Sede)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (item == null) return NotFound();

            return View(item);
        }

        // GET: PstreeAssociazioniImportSaldiCE/Create
        public async Task<IActionResult> Create(string? codicePdC, int? anno, int? sede)
        {
            await PopolaCreateAsync();

            // Sede predefinita: se non specificata, usa "Favaro1" (se presente)
            var sedeDefault = sede ?? 0;
            if (sedeDefault <= 0)
            {
                sedeDefault = await _context.PstreeListaSedi
                    .Where(s => s.Sede.Replace(" ", "") == "Favaro1")
                    .Select(s => s.Id)
                    .FirstOrDefaultAsync();
            }

            var model = new RipartizioneImportSaldiCreateViewModel
            {
                CodicePdC = codicePdC ?? string.Empty,
                Anno = anno ?? DateTime.Now.Year,
                Mese = DateTime.Now.Month,
                IdSede = sedeDefault
            };

            return View(model);
        }

        // POST: PstreeAssociazioniImportSaldiCE/Create
        // azione = "mese" (solo il mese selezionato) oppure "anno" (tutti i mesi 1-12)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(RipartizioneImportSaldiCreateViewModel model, string azione)
        {
            var tuttiMesi = azione == "anno";

            // --- Validazioni di base ---
            if (string.IsNullOrWhiteSpace(model.CodicePdC))
                ModelState.AddModelError(nameof(model.CodicePdC), "Selezionare un codice del Piano dei Conti.");

            if (model.IdSede <= 0)
                ModelState.AddModelError(nameof(model.IdSede), "Selezionare una sede.");

            if (!tuttiMesi && (model.Mese < 1 || model.Mese > 12))
                ModelState.AddModelError(nameof(model.Mese), "Selezionare un mese valido.");

            // Solo voci valorizzate
            var voci = (model.Voci ?? new List<RipartizioneVoceItem>())
                .Where(v => v.IdCodiceConto > 0)
                .ToList();

            if (!voci.Any())
                ModelState.AddModelError(string.Empty, "Aggiungere almeno una voce di Conto Economico.");

            // Niente voci duplicate
            var vociDuplicate = voci
                .GroupBy(v => v.IdCodiceConto)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key)
                .ToList();
            if (vociDuplicate.Any())
                ModelState.AddModelError(string.Empty, "La stessa voce di Conto Economico è stata inserita più volte.");

            // Percentuali nel range e somma = 100%
            foreach (var v in voci)
            {
                var perc = v.GetPercentuale();
                if (perc < 0 || perc > 100)
                    ModelState.AddModelError(string.Empty, "Ogni percentuale deve essere compresa tra 0 e 100.");
            }

            var sommaPerc = voci.Sum(v => v.GetPercentuale());
            if (voci.Any() && Math.Abs(sommaPerc - 100.0) >= 0.5)
                ModelState.AddModelError(string.Empty,
                    $"La somma delle percentuali deve essere 100% (attuale: {sommaPerc:N2}%).");

            if (!ModelState.IsValid)
            {
                await PopolaCreateAsync();
                return View(model);
            }

            // --- Salvataggio: per ogni mese interessato sostituisce le righe esistenti ---
            var mesi = tuttiMesi ? Enumerable.Range(1, 12).ToList() : new List<int> { model.Mese };

            foreach (var mese in mesi)
            {
                var esistenti = await _context.PstreeAssociazioniImportSaldiCE
                    .Where(a => a.CodicePdC == model.CodicePdC &&
                                a.IdSede == model.IdSede &&
                                a.Anno == model.Anno &&
                                a.Mese == mese)
                    .ToListAsync();

                _context.PstreeAssociazioniImportSaldiCE.RemoveRange(esistenti);

                foreach (var v in voci)
                {
                    _context.PstreeAssociazioniImportSaldiCE.Add(new PstreeAssociazioniImportSaldiCE
                    {
                        CodicePdC = model.CodicePdC,
                        IdCodiceConto = v.IdCodiceConto,
                        IdSede = model.IdSede,
                        Anno = model.Anno,
                        Mese = mese,
                        Percentuale = v.GetPercentuale()
                    });
                }
            }

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = tuttiMesi
                ? $"Ripartizione salvata per tutti i mesi del {model.Anno}: {voci.Count} voci per il conto {model.CodicePdC}."
                : $"Ripartizione salvata per {model.CodicePdC} ({voci.Count} voci) nel mese {model.Mese}/{model.Anno}.";

            _logger.LogInformation(
                "Ripartizione import saldi salvata: CodicePdC={CodicePdC}, Sede={Sede}, Anno={Anno}, TuttiMesi={Tutti}, Voci={Voci}",
                model.CodicePdC, model.IdSede, model.Anno, tuttiMesi, voci.Count);

            return RedirectToAction(nameof(Index), new { anno = model.Anno, sede = model.IdSede });
        }

        // GET: PstreeAssociazioniImportSaldiCE/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var model = await _context.PstreeAssociazioniImportSaldiCE.FindAsync(id);
            if (model == null) return NotFound();

            await PopolaDropdownAsync(model);
            return View(model);
        }

        // POST: PstreeAssociazioniImportSaldiCE/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id,
            [Bind("Id,CodicePdC,IdCodiceConto,IdSede,Anno,Mese,Percentuale")] PstreeAssociazioniImportSaldiCE model)
        {
            if (id != model.Id) return NotFound();

            await ValidaModelloAsync(model, model.Id);

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(model);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] =
                        $"Ripartizione aggiornata: {model.CodicePdC} → voce {model.IdCodiceConto} ({model.Percentuale:N2}%).";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.PstreeAssociazioniImportSaldiCE.Any(a => a.Id == model.Id))
                        return NotFound();
                    throw;
                }

                return RedirectToAction(nameof(Index), new { anno = model.Anno, sede = model.IdSede });
            }

            await PopolaDropdownAsync(model);
            return View(model);
        }

        // GET: PstreeAssociazioniImportSaldiCE/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var item = await _context.PstreeAssociazioniImportSaldiCE
                .AsNoTracking()
                .Include(a => a.PianoDeiConti)
                .Include(a => a.ContoEconomico)
                .Include(a => a.Sede)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (item == null) return NotFound();

            return View(item);
        }

        // POST: PstreeAssociazioniImportSaldiCE/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var item = await _context.PstreeAssociazioniImportSaldiCE.FindAsync(id);
            if (item == null) return NotFound();

            _context.PstreeAssociazioniImportSaldiCE.Remove(item);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Ripartizione eliminata ({item.CodicePdC}).";
            _logger.LogInformation("Ripartizione import saldi eliminata: Id={Id}, CodicePdC={CodicePdC}", id, item.CodicePdC);

            return RedirectToAction(nameof(Index), new { anno = item.Anno, sede = item.IdSede });
        }

        // ========================================
        // METODI HELPER
        // ========================================

        /// <summary>
        /// Valida il modello: range percentuale e unicità della combinazione
        /// (CodicePdC + IdCodiceConto + IdSede + Anno + Mese).
        /// </summary>
        private async Task ValidaModelloAsync(PstreeAssociazioniImportSaldiCE model, int? idEsclusione)
        {
            if (model.Mese < 1 || model.Mese > 12)
            {
                ModelState.AddModelError(nameof(model.Mese), "Il mese deve essere compreso tra 1 e 12.");
            }

            if (model.Percentuale < 0 || model.Percentuale > 100)
            {
                ModelState.AddModelError(nameof(model.Percentuale), "La percentuale deve essere compresa tra 0 e 100.");
            }

            var duplicato = await _context.PstreeAssociazioniImportSaldiCE.AnyAsync(a =>
                a.CodicePdC == model.CodicePdC &&
                a.IdCodiceConto == model.IdCodiceConto &&
                a.IdSede == model.IdSede &&
                a.Anno == model.Anno &&
                a.Mese == model.Mese &&
                (!idEsclusione.HasValue || a.Id != idEsclusione.Value));

            if (duplicato)
            {
                ModelState.AddModelError(string.Empty,
                    "Esiste già una ripartizione per questo conto, voce, sede, anno e mese.");
            }
        }

        /// <summary>
        /// Prepara i dati per la pagina Create: sorgenti JSON per i ComboBox ricercabili
        /// (Piano dei Conti e voci CE) e i select di sede / anno / mese.
        /// </summary>
        private async Task PopolaCreateAsync()
        {
            var pdc = await _context.PstreeListaPianoDeiConti
                .Where(p => p.TipoPdC == "E")
                .OrderBy(p => p.CodicePdC)
                .Select(p => new { value = p.CodicePdC, text = p.CodicePdC + " - " + p.DescrizionePdC })
                .ToListAsync();
            ViewBag.PdcJson = JsonSerializer.Serialize(pdc);

            var voci = await _context.PstreeStrutturaContoEconomico
                .Where(v => v.TipoConto != "T" && v.TipoConto != "S")
                .OrderBy(v => v.Ordine)
                .Select(v => new { value = v.IdCodiceConto, text = v.IdCodiceConto + " - " + v.DescrizioneConto })
                .ToListAsync();
            ViewBag.VociJson = JsonSerializer.Serialize(voci);

            await PopolaSediAsync(null);

            var annoCorrente = DateTime.Now.Year;
            ViewBag.AnniDisponibili = Enumerable.Range(annoCorrente - 3, 6).OrderByDescending(a => a).ToList();

            ViewBag.Mesi = Enumerable.Range(1, 12).Select(m => new SelectListItem
            {
                Value = m.ToString(),
                Text = NomiMesi[m]
            }).ToList();
        }

        private async Task PopolaDropdownAsync(PstreeAssociazioniImportSaldiCE? model = null)
        {
            // Piano dei Conti (tipo Economico)
            var pdc = await _context.PstreeListaPianoDeiConti
                .Where(p => p.TipoPdC == "E")
                .OrderBy(p => p.CodicePdC)
                .ToListAsync();
            ViewBag.CodiciPdC = pdc.Select(p => new SelectListItem
            {
                Value = p.CodicePdC,
                Text = $"{p.CodicePdC} - {p.DescrizionePdC}",
                Selected = model != null && p.CodicePdC == model.CodicePdC
            }).ToList();

            // Voci CE (solo foglie: niente Totali/Sottototali)
            var vociCE = await _context.PstreeStrutturaContoEconomico
                .Where(v => v.TipoConto != "T" && v.TipoConto != "S")
                .OrderBy(v => v.Ordine)
                .ToListAsync();
            var vociItems = vociCE.Select(v => new SelectListItem
            {
                Value = v.IdCodiceConto.ToString(),
                Text = $"{v.IdCodiceConto} - {v.DescrizioneConto}",
                Selected = model != null && v.IdCodiceConto == model.IdCodiceConto
            }).ToList();
            vociItems.Insert(0, new SelectListItem { Value = "", Text = "-- Seleziona una voce --" });
            ViewBag.VociCE = vociItems;

            await PopolaSediAsync(model?.IdSede);

            var annoCorrente = DateTime.Now.Year;
            ViewBag.AnniDisponibili = Enumerable.Range(annoCorrente - 3, 6).OrderByDescending(a => a).ToList();

            ViewBag.Mesi = Enumerable.Range(1, 12).Select(m => new SelectListItem
            {
                Value = m.ToString(),
                Text = NomiMesi[m],
                Selected = model != null && m == model.Mese
            }).ToList();
        }

        private async Task PopolaSediAsync(int? selezionata)
        {
            var sedi = await _context.PstreeListaSedi.OrderBy(s => s.Id).ToListAsync();
            ViewBag.Sedi = sedi.Select(s => new SelectListItem
            {
                Value = s.Id.ToString(),
                Text = $"{s.Id} - {s.Sede}",
                Selected = selezionata.HasValue && s.Id == selezionata.Value
            }).ToList();
        }
    }
}
