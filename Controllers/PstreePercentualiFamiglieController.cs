using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AiDbMaster.Data;
using AiDbMaster.Models;
using AiDbMaster.ViewModels;
using Microsoft.AspNetCore.Authorization;

namespace AiDbMaster.Controllers
{
    /// <summary>
    /// Controller per la gestione delle Percentuali per Famiglia
    /// Interfaccia a matrice Mesi x Famiglie
    /// </summary>
    [Authorize]
    public class PstreePercentualiFamiglieController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<PstreePercentualiFamiglieController> _logger;

        public PstreePercentualiFamiglieController(ApplicationDbContext context, ILogger<PstreePercentualiFamiglieController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: PercentualiFamiglie
        public async Task<IActionResult> Index(int? anno, int? sede)
        {
            var viewModel = new Pstree_PercentualiFamiglieIndexViewModel();
            
            var annoCorrente = DateTime.Now.Year;
            viewModel.Anno = anno ?? annoCorrente;
            viewModel.SedeSelezionata = sede;
            
            // Carica anni disponibili
            viewModel.AnniDisponibili = Enumerable.Range(annoCorrente - 2, 5).OrderByDescending(a => a).ToList();
            
            // Carica sedi
            var sedi = await _context.PstreeListaSedi.OrderBy(s => s.Id).ToListAsync();
            viewModel.Sedi = sedi.Select(s => new Pstree_SedeDropdownItem { Id = s.Id, Nome = s.Sede }).ToList();
            
            // Carica tutte le voci CE (solo foglie, non i totali)
            var vociCE = await _context.PstreeStrutturaContoEconomico
                .Where(v => v.TipoConto != "T" && v.TipoConto != "S") // Escludi totali e sottototali
                .OrderBy(v => v.Ordine)
                .ToListAsync();
            
            // Carica tutte le percentuali per l'anno e sede selezionati
            var percentualiQuery = _context.PstreePercentualiFamiglie
                .Where(p => p.Anno == viewModel.Anno && p.IdFamiglia != 0); // Escludi Analitica
            
            if (sede.HasValue)
            {
                percentualiQuery = percentualiQuery.Where(p => p.IdSede == sede.Value);
            }
            
            var percentuali = await percentualiQuery.ToListAsync();
            
            // Raggruppa per IdCodiceConto e Mese, calcola somma
            var percentualiPerVoce = percentuali
                .GroupBy(p => p.IdCodiceConto)
                .ToDictionary(
                    g => g.Key,
                    g => g.GroupBy(p => p.Mese)
                          .ToDictionary(m => m.Key, m => m.Sum(p => p.Percentuale))
                );
            
            // Costruisci la lista con lo stato
            viewModel.VociCE = vociCE.Select(v => 
            {
                var status = new Pstree_VoceCEPercentualeStatus
                {
                    IdCodiceConto = v.IdCodiceConto,
                    DescrizioneConto = v.DescrizioneConto,
                    TipoConto = v.TipoConto
                };
                
                if (percentualiPerVoce.TryGetValue(v.IdCodiceConto, out var mesiDict))
                {
                    foreach (var mese in mesiDict)
                    {
                        if (Math.Abs(mese.Value - 100.0) < 0.01)
                            status.MesiCompleti++;
                        else
                            status.MesiIncompleti++;
                    }
                }
                
                return status;
            }).ToList();
            
            return View(viewModel);
        }

        // GET: PercentualiFamiglie/Edit
        public async Task<IActionResult> Edit(int idCodiceConto, int? anno, int? sede)
        {
            var annoCorrente = DateTime.Now.Year;
            var annoSelezionato = anno ?? annoCorrente;
            
            // Se sede non specificata, prendi la prima disponibile
            var sedi = await _context.PstreeListaSedi.OrderBy(s => s.Id).ToListAsync();
            var sedeSelezionata = sede ?? sedi.FirstOrDefault()?.Id ?? 1;
            
            var voce = await _context.PstreeStrutturaContoEconomico
                .FirstOrDefaultAsync(v => v.IdCodiceConto == idCodiceConto);
            
            if (voce == null)
            {
                TempData["ErrorMessage"] = "Voce del conto economico non trovata.";
                return RedirectToAction(nameof(Index));
            }
            
            var sedeEntity = sedi.FirstOrDefault(s => s.Id == sedeSelezionata);
            
            var viewModel = new Pstree_PercentualiFamiglieEditViewModel
            {
                IdCodiceConto = idCodiceConto,
                DescrizioneConto = voce.DescrizioneConto,
                Anno = annoSelezionato,
                IdSede = sedeSelezionata,
                NomeSede = sedeEntity?.Sede ?? "N/D"
            };
            
            // Carica anni e sedi per dropdown
            viewModel.AnniDisponibili = Enumerable.Range(annoCorrente - 2, 5).OrderByDescending(a => a).ToList();
            viewModel.Sedi = sedi.Select(s => new Pstree_SedeDropdownItem { Id = s.Id, Nome = s.Sede }).ToList();
            
            // Carica voci CE per dropdown
            var vociCE = await _context.PstreeStrutturaContoEconomico
                .Where(v => v.TipoConto != "T" && v.TipoConto != "S")
                .OrderBy(v => v.Ordine)
                .ToListAsync();
            viewModel.VociCE = vociCE.Select(v => new Pstree_VoceCEDropdownItem 
            { 
                IdCodiceConto = v.IdCodiceConto, 
                Descrizione = v.DescrizioneConto 
            }).ToList();
            
            // Carica famiglie (esclusa Analitica Id=0)
            var famiglie = await _context.PstreeListaFamiglie
                .Where(f => f.Id != 0)
                .OrderBy(f => f.Id)
                .ToListAsync();
            viewModel.Famiglie = famiglie.Select(f => new Pstree_FamigliaColonna { Id = f.Id, Nome = f.NomeFamiglia }).ToList();
            
            // Carica percentuali esistenti
            var percentuali = await _context.PstreePercentualiFamiglie
                .Where(p => p.IdCodiceConto == idCodiceConto && 
                            p.Anno == annoSelezionato && 
                            p.IdSede == sedeSelezionata &&
                            p.IdFamiglia != 0)
                .ToListAsync();
            
            // Costruisci la matrice 12 mesi x N famiglie
            viewModel.RigheMesi = new List<Pstree_PercentualeRigaMese>();
            for (int mese = 1; mese <= 12; mese++)
            {
                var riga = new Pstree_PercentualeRigaMese
                {
                    Mese = mese,
                    NomeMese = Pstree_PercentualiFamiglieEditViewModel.NomiMesi[mese],
                    Percentuali = new Dictionary<int, double>()
                };
                
                // Inizializza tutte le famiglie a 0
                foreach (var fam in famiglie)
                {
                    var perc = percentuali.FirstOrDefault(p => p.Mese == mese && p.IdFamiglia == fam.Id);
                    riga.Percentuali[fam.Id] = perc?.Percentuale ?? 0;
                }
                
                viewModel.RigheMesi.Add(riga);
            }
            
            return View(viewModel);
        }

        // POST: PercentualiFamiglie/Save
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Save(Pstree_PercentualiFamiglieSaveModel model)
        {
            try
            {
                // Debug: log di tutti i form data ricevuti
                _logger.LogInformation("=== SAVE CHIAMATO ===");
                _logger.LogInformation("IdCodiceConto: {Id}, Anno: {Anno}, Sede: {Sede}", 
                    model.IdCodiceConto, model.Anno, model.IdSede);
                _logger.LogInformation("Percentuali count dal model: {Count}", model.Percentuali?.Count ?? 0);
                
                // Log alcuni form values per debug
                foreach (var key in Request.Form.Keys.Where(k => k.Contains("Percentuali")).Take(20))
                {
                    _logger.LogInformation("Form[{Key}] = {Value}", key, Request.Form[key]);
                }
                
                if (model.Percentuali == null || !model.Percentuali.Any())
                {
                    TempData["ErrorMessage"] = "Nessuna percentuale ricevuta dal form.";
                    return RedirectToAction(nameof(Edit), new { idCodiceConto = model.IdCodiceConto, anno = model.Anno, sede = model.IdSede });
                }
                
                // Validazione: ogni mese deve avere somma = 100%
                // Usa GetPercentualeValue() per parsing corretto con cultura invariante
                var percentualiPerMese = model.Percentuali
                    .GroupBy(p => p.Mese)
                    .ToDictionary(g => g.Key, g => g.Sum(p => p.GetPercentualeValue()));
                
                // Debug: log delle somme per mese
                foreach (var kv in percentualiPerMese)
                {
                    _logger.LogInformation("Mese {Mese}: somma = {Somma}", kv.Key, kv.Value);
                }
                
                // Mostra debug info su banner
                var debugInfo = string.Join(" | ", percentualiPerMese.Select(kv => $"Mese {kv.Key}: {kv.Value:F2}%"));
                TempData["DebugMessage"] = $"DEBUG - Ricevute {model.Percentuali.Count} percentuali. Somme: {debugInfo}";
                
                var mesiInvalidi = percentualiPerMese
                    .Where(m => Math.Abs(m.Value - 100.0) >= 0.5) // Aumentata tolleranza a 0.5
                    .Select(m => $"{Pstree_PercentualiFamiglieEditViewModel.NomiMesi[m.Key]} ({percentualiPerMese[m.Key]:F2}%)")
                    .ToList();
                
                if (mesiInvalidi.Any())
                {
                    TempData["ErrorMessage"] = $"La somma delle percentuali deve essere 100% per ogni mese. Mesi non validi: {string.Join(", ", mesiInvalidi)}";
                    return RedirectToAction(nameof(Edit), new { idCodiceConto = model.IdCodiceConto, anno = model.Anno, sede = model.IdSede });
                }
                
                // Elimina le percentuali esistenti per questa voce/anno/sede
                var esistenti = await _context.PstreePercentualiFamiglie
                    .Where(p => p.IdCodiceConto == model.IdCodiceConto && 
                                p.Anno == model.Anno && 
                                p.IdSede == model.IdSede)
                    .ToListAsync();
                
                _context.PstreePercentualiFamiglie.RemoveRange(esistenti);
                
                // Inserisci le nuove percentuali (solo quelle > 0)
                foreach (var item in model.Percentuali.Where(p => p.GetPercentualeValue() > 0))
                {
                    _context.PstreePercentualiFamiglie.Add(new PstreePercentualiFamiglie
                    {
                        IdCodiceConto = model.IdCodiceConto,
                        Anno = model.Anno,
                        IdSede = model.IdSede,
                        Mese = item.Mese,
                        IdFamiglia = item.IdFamiglia,
                        Percentuale = item.GetPercentualeValue()
                    });
                }
                
                await _context.SaveChangesAsync();
                
                TempData["SuccessMessage"] = "Percentuali salvate con successo!";
                _logger.LogInformation("Percentuali salvate per voce {IdCodiceConto}, anno {Anno}, sede {IdSede}", 
                    model.IdCodiceConto, model.Anno, model.IdSede);
                
                return RedirectToAction(nameof(Index), new { anno = model.Anno, sede = model.IdSede });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante il salvataggio delle percentuali");
                TempData["ErrorMessage"] = "Errore durante il salvataggio: " + ex.Message;
                return RedirectToAction(nameof(Edit), new { idCodiceConto = model.IdCodiceConto, anno = model.Anno, sede = model.IdSede });
            }
        }

        // POST: PercentualiFamiglie/DeleteAll
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteAll(int idCodiceConto, int anno, int sede)
        {
            try
            {
                var esistenti = await _context.PstreePercentualiFamiglie
                    .Where(p => p.IdCodiceConto == idCodiceConto && 
                                p.Anno == anno && 
                                p.IdSede == sede)
                    .ToListAsync();
                
                if (!esistenti.Any())
                {
                    TempData["WarningMessage"] = "Nessuna percentuale da eliminare.";
                    return RedirectToAction(nameof(Index), new { anno, sede });
                }
                
                _context.PstreePercentualiFamiglie.RemoveRange(esistenti);
                await _context.SaveChangesAsync();
                
                TempData["SuccessMessage"] = $"Eliminate {esistenti.Count} percentuali con successo!";
                _logger.LogInformation("Eliminate {Count} percentuali per voce {IdCodiceConto}, anno {Anno}, sede {IdSede}", 
                    esistenti.Count, idCodiceConto, anno, sede);
                
                return RedirectToAction(nameof(Index), new { anno, sede });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante l'eliminazione delle percentuali");
                TempData["ErrorMessage"] = "Errore durante l'eliminazione: " + ex.Message;
                return RedirectToAction(nameof(Index), new { anno, sede });
            }
        }

        // POST: PercentualiFamiglie/CopyMonth
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CopyMonth(int idCodiceConto, int anno, int sede, int meseOrigine, int meseDestinazione)
        {
            try
            {
                // Carica percentuali del mese origine
                var percentualiOrigine = await _context.PstreePercentualiFamiglie
                    .Where(p => p.IdCodiceConto == idCodiceConto && 
                                p.Anno == anno && 
                                p.IdSede == sede &&
                                p.Mese == meseOrigine)
                    .ToListAsync();
                
                if (!percentualiOrigine.Any())
                {
                    TempData["WarningMessage"] = "Nessuna percentuale da copiare nel mese origine.";
                    return RedirectToAction(nameof(Edit), new { idCodiceConto, anno, sede });
                }
                
                // Elimina percentuali esistenti nel mese destinazione
                var esistentiDest = await _context.PstreePercentualiFamiglie
                    .Where(p => p.IdCodiceConto == idCodiceConto && 
                                p.Anno == anno && 
                                p.IdSede == sede &&
                                p.Mese == meseDestinazione)
                    .ToListAsync();
                
                _context.PstreePercentualiFamiglie.RemoveRange(esistentiDest);
                
                // Copia le percentuali
                foreach (var perc in percentualiOrigine)
                {
                    _context.PstreePercentualiFamiglie.Add(new PstreePercentualiFamiglie
                    {
                        IdCodiceConto = perc.IdCodiceConto,
                        Anno = perc.Anno,
                        IdSede = perc.IdSede,
                        Mese = meseDestinazione,
                        IdFamiglia = perc.IdFamiglia,
                        Percentuale = perc.Percentuale
                    });
                }
                
                await _context.SaveChangesAsync();
                
                TempData["SuccessMessage"] = $"Percentuali copiate da {Pstree_PercentualiFamiglieEditViewModel.NomiMesi[meseOrigine]} a {Pstree_PercentualiFamiglieEditViewModel.NomiMesi[meseDestinazione]}!";
                
                return RedirectToAction(nameof(Edit), new { idCodiceConto, anno, sede });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante la copia delle percentuali");
                TempData["ErrorMessage"] = "Errore durante la copia: " + ex.Message;
                return RedirectToAction(nameof(Edit), new { idCodiceConto, anno, sede });
            }
        }
    }
}
