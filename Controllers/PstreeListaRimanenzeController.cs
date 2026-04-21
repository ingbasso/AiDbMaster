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
    /// Controller per la gestione delle Rimanenze finali mensili di magazzino
    /// </summary>
    [Authorize]
    public class PstreeListaRimanenzeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<PstreeListaRimanenzeController> _logger;

        public PstreeListaRimanenzeController(ApplicationDbContext context, ILogger<PstreeListaRimanenzeController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: ListaRimanenze
        public async Task<IActionResult> Index(int? idFamiglia, int? idSede, int? mese, int? anno)
        {
            // Default anno corrente se non specificato
            var annoDefault = anno ?? DateTime.Now.Year;
            
            // Default sede con id più piccolo se non specificata
            int? sedeDefault = idSede;
            if (!idSede.HasValue)
            {
                var primaSede = await _context.PstreeListaSedi.OrderBy(s => s.Id).FirstOrDefaultAsync();
                sedeDefault = primaSede?.Id;
            }
            
            var query = _context.PstreeListaRimanenze.AsQueryable();

            // Applica filtri
            if (idFamiglia.HasValue)
            {
                query = query.Where(r => r.IdFamiglia == idFamiglia.Value);
            }
            // Applica sempre filtro sede (default = sede con id più piccolo)
            if (sedeDefault.HasValue)
            {
                query = query.Where(r => r.IdSede == sedeDefault.Value);
            }
            if (mese.HasValue)
            {
                query = query.Where(r => r.Mese == mese.Value);
            }
            // Applica sempre filtro anno (default = anno corrente)
            query = query.Where(r => r.Anno == annoDefault);

            var rimanenze = await query
                .OrderByDescending(r => r.Anno)
                .ThenByDescending(r => r.Mese)
                .ThenBy(r => r.IdFamiglia)
                .ToListAsync();

            // Carica dizionari per mostrare descrizioni
            var famiglieDict = await _context.PstreeListaFamiglie
                .ToDictionaryAsync(f => f.Id, f => f.NomeFamiglia);
            ViewBag.FamiglieDescrizioni = famiglieDict;

            var sediDict = await _context.PstreeListaSedi
                .ToDictionaryAsync(s => s.Id, s => s.Sede);
            ViewBag.SediDescrizioni = sediDict;

            // === CALCOLA WARNING RIMANENZE MANCANTI (per sede e anno filtrati) ===
            var warnings = new List<Pstree_RimanenzeWarning>();
            var meseCorrente = DateTime.Now.Month;
            
            // Usa la sede e l'anno selezionati nei filtri
            if (sedeDefault.HasValue)
            {
                // Carica famiglie (esclusa Analitica)
                var famiglie = await _context.PstreeListaFamiglie.Where(f => f.Id != 0).ToListAsync();
                var numFamiglie = famiglie.Count;
                
                // Carica rimanenze per la sede e anno filtrati
                var rimanenzeAnno = await _context.PstreeListaRimanenze
                    .Where(r => r.Anno == annoDefault && r.IdSede == sedeDefault.Value)
                    .ToListAsync();
                
                // Se l'anno filtrato è l'anno corrente, controlla fino al mese corrente
                // Altrimenti controlla tutti i 12 mesi
                int maxMese = (annoDefault == DateTime.Now.Year) ? meseCorrente : 12;
                
                // Per ogni mese
                for (int m = 1; m <= maxMese; m++)
                {
                    var rimanenzeMese = rimanenzeAnno.Where(r => r.Mese == m).ToList();
                    
                    if (!rimanenzeMese.Any())
                    {
                        // Mese completamente mancante
                        warnings.Add(new Pstree_RimanenzeWarning
                        {
                            Tipo = "danger",
                            Icona = "fa-exclamation-circle",
                            Messaggio = GetNomeMese(m),
                            Sede = sedeDefault.Value,
                            Anno = annoDefault,
                            Mese = m
                        });
                    }
                    else if (rimanenzeMese.Count < numFamiglie)
                    {
                        // Alcune famiglie mancanti
                        var famiglieMancanti = numFamiglie - rimanenzeMese.Count;
                        warnings.Add(new Pstree_RimanenzeWarning
                        {
                            Tipo = "warning",
                            Icona = "fa-exclamation-triangle",
                            Messaggio = $"{GetNomeMese(m)} ({famiglieMancanti} famiglie mancanti)",
                            Sede = sedeDefault.Value,
                            Anno = annoDefault,
                            Mese = m
                        });
                    }
                }
            }
            
            ViewBag.Warnings = warnings.OrderBy(w => w.Mese).ToList();

            // Popola dropdown filtri (usa i default)
            await PopulateFamiglieDropdownAsync(idFamiglia);
            await PopulateSediDropdownAsync(sedeDefault);
            PopulateMesiDropdown(mese);
            await PopulateAnniDropdownAsync(annoDefault);

            // Salva filtri correnti (usa i default)
            ViewBag.CurrentFamiglia = idFamiglia;
            ViewBag.CurrentSede = sedeDefault;
            ViewBag.CurrentMese = mese;
            ViewBag.CurrentAnno = annoDefault;

            return View(rimanenze);
        }

        // GET: ListaRimanenze/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var rimanenza = await _context.PstreeListaRimanenze
                .FirstOrDefaultAsync(r => r.Id == id);

            if (rimanenza == null)
            {
                return NotFound();
            }

            // Carica famiglia e sede
            ViewBag.Famiglia = await _context.PstreeListaFamiglie
                .FirstOrDefaultAsync(f => f.Id == rimanenza.IdFamiglia);
            ViewBag.Sede = await _context.PstreeListaSedi
                .FirstOrDefaultAsync(s => s.Id == rimanenza.IdSede);

            return View(rimanenza);
        }

        // GET: ListaRimanenze/Create
        public async Task<IActionResult> Create()
        {
            var model = new PstreeListaRimanenze
            {
                Anno = DateTime.Now.Year,
                Mese = DateTime.Now.Month
            };

            await PopulateFamiglieDropdownAsync();
            await PopulateSediDropdownAsync();
            PopulateMesiDropdown();

            return View(model);
        }

        // POST: ListaRimanenze/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Valore,Mese,Anno,IdFamiglia,IdSede")] PstreeListaRimanenze rimanenza)
        {
            if (ModelState.IsValid)
            {
                // Verifica se esiste già una rimanenza per stessa famiglia/sede/mese/anno
                var exists = await _context.PstreeListaRimanenze
                    .AnyAsync(r => r.IdFamiglia == rimanenza.IdFamiglia 
                        && r.IdSede == rimanenza.IdSede 
                        && r.Mese == rimanenza.Mese 
                        && r.Anno == rimanenza.Anno);

                if (exists)
                {
                    ModelState.AddModelError("", "Esiste già una rimanenza per questa combinazione di Famiglia/Sede/Mese/Anno.");
                    await PopulateFamiglieDropdownAsync(rimanenza.IdFamiglia);
                    await PopulateSediDropdownAsync(rimanenza.IdSede);
                    PopulateMesiDropdown();
                    return View(rimanenza);
                }

                _context.Add(rimanenza);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Rimanenza creata con successo!";
                _logger.LogInformation("Rimanenza {Id} creata per Famiglia {IdFamiglia}, Sede {IdSede}, {Mese}/{Anno}", 
                    rimanenza.Id, rimanenza.IdFamiglia, rimanenza.IdSede, rimanenza.Mese, rimanenza.Anno);

                return RedirectToAction(nameof(Index));
            }

            await PopulateFamiglieDropdownAsync(rimanenza.IdFamiglia);
            await PopulateSediDropdownAsync(rimanenza.IdSede);
            PopulateMesiDropdown();
            return View(rimanenza);
        }

        // GET: ListaRimanenze/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var rimanenza = await _context.PstreeListaRimanenze.FindAsync(id);
            if (rimanenza == null)
            {
                return NotFound();
            }

            await PopulateFamiglieDropdownAsync(rimanenza.IdFamiglia);
            await PopulateSediDropdownAsync(rimanenza.IdSede);
            PopulateMesiDropdown();

            return View(rimanenza);
        }

        // POST: ListaRimanenze/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Valore,Mese,Anno,IdFamiglia,IdSede,RettificaValore,NoteRettifica")] PstreeListaRimanenze rimanenza)
        {
            if (id != rimanenza.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Verifica duplicato (escludendo il record corrente)
                    var exists = await _context.PstreeListaRimanenze
                        .AnyAsync(r => r.IdFamiglia == rimanenza.IdFamiglia 
                            && r.IdSede == rimanenza.IdSede 
                            && r.Mese == rimanenza.Mese 
                            && r.Anno == rimanenza.Anno
                            && r.Id != rimanenza.Id);

                    if (exists)
                    {
                        ModelState.AddModelError("", "Esiste già una rimanenza per questa combinazione di Famiglia/Sede/Mese/Anno.");
                        await PopulateFamiglieDropdownAsync(rimanenza.IdFamiglia);
                        await PopulateSediDropdownAsync(rimanenza.IdSede);
                        PopulateMesiDropdown();
                        return View(rimanenza);
                    }

                    _context.Update(rimanenza);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Rimanenza modificata con successo!";
                    _logger.LogInformation("Rimanenza {Id} modificata", rimanenza.Id);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!RimanenzaExists(rimanenza.Id))
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

            await PopulateFamiglieDropdownAsync(rimanenza.IdFamiglia);
            await PopulateSediDropdownAsync(rimanenza.IdSede);
            PopulateMesiDropdown();
            return View(rimanenza);
        }

        // GET: ListaRimanenze/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var rimanenza = await _context.PstreeListaRimanenze
                .FirstOrDefaultAsync(r => r.Id == id);

            if (rimanenza == null)
            {
                return NotFound();
            }

            // Carica famiglia e sede
            ViewBag.Famiglia = await _context.PstreeListaFamiglie
                .FirstOrDefaultAsync(f => f.Id == rimanenza.IdFamiglia);
            ViewBag.Sede = await _context.PstreeListaSedi
                .FirstOrDefaultAsync(s => s.Id == rimanenza.IdSede);

            return View(rimanenza);
        }

        // POST: ListaRimanenze/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var rimanenza = await _context.PstreeListaRimanenze.FindAsync(id);
            if (rimanenza == null)
            {
                return NotFound();
            }

            _context.PstreeListaRimanenze.Remove(rimanenza);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Rimanenza eliminata con successo!";
            _logger.LogInformation("Rimanenza {Id} eliminata", id);

            return RedirectToAction(nameof(Index));
        }

        // ==========================================
        // === NUOVA MODALITÀ GRIGLIA ===
        // ==========================================

        // GET: ListaRimanenze/EditGriglia
        public async Task<IActionResult> EditGriglia(int? anno, int? mese, int? sede)
        {
            var annoCorrente = DateTime.Now.Year;
            var meseCorrente = DateTime.Now.Month;
            
            // Carica sedi ordinate per ID
            var sediList = await _context.PstreeListaSedi
                .OrderBy(s => s.Id)
                .Select(s => new Pstree_SedeDropdownItem { Id = s.Id, Nome = s.Sede })
                .ToListAsync();
            
            // Default sede: quella con ID più piccolo
            var sedeDefault = sede ?? sediList.FirstOrDefault()?.Id ?? 0;

            // Se non specificati, chiedi di selezionare
            if (!anno.HasValue || !mese.HasValue || !sede.HasValue)
            {
                var viewModel = new Pstree_RimanenzeSelezionaViewModel
                {
                    Anno = anno ?? annoCorrente,
                    Mese = mese ?? meseCorrente,
                    IdSede = sedeDefault
                };

                viewModel.AnniDisponibili = Enumerable.Range(annoCorrente - 2, 5).OrderByDescending(a => a).ToList();
                viewModel.Sedi = sediList;

                return View("SelezionaGriglia", viewModel);
            }
            
            // Carica o crea i record per la griglia
            var griglia = await CaricaOCreaGrigliaAsync(anno.Value, mese.Value, sede.Value);
            
            return View("EditGriglia", griglia);
        }

        // POST: ListaRimanenze/SaveGriglia
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveGriglia(Pstree_RimanenzeSaveModel model)
        {
            try
            {
                // === GESTIONE GENNAIO: Crea record Dicembre anno precedente se necessario ===
                bool creatoRecordDicembre = false;
                if (model.Mese == 1)
                {
                    // Verifica se esistono già record di Dicembre anno precedente
                    var esisteDicembre = await _context.PstreeListaRimanenze
                        .AnyAsync(r => r.Anno == model.Anno - 1 && r.Mese == 12 && r.IdSede == model.IdSede);
                    
                    if (!esisteDicembre)
                    {
                        // Crea i record di Dicembre anno precedente con le rimanenze iniziali
                        foreach (var item in model.Rimanenze)
                        {
                            var valoreIniziale = item.GetRimanenzaInizialeValue();
                            if (valoreIniziale > 0)
                            {
                                _context.PstreeListaRimanenze.Add(new PstreeListaRimanenze
                                {
                                    Anno = model.Anno - 1,
                                    Mese = 12,
                                    IdSede = model.IdSede,
                                    IdFamiglia = item.IdFamiglia,
                                    Valore = valoreIniziale
                                });
                                creatoRecordDicembre = true;
                            }
                        }
                    }
                }
                
                // Salva le rimanenze del mese corrente
                foreach (var item in model.Rimanenze)
                {
                    var valore = item.GetRimanenzaFinaleValue();
                    var rettifica = item.GetRettificaValoreValue();

                    if (item.Id > 0)
                    {
                        // Aggiorna record esistente
                        var rimanenza = await _context.PstreeListaRimanenze.FindAsync(item.Id);
                        if (rimanenza != null)
                        {
                            rimanenza.Valore = valore;
                            rimanenza.RettificaValore = rettifica;
                            rimanenza.NoteRettifica = item.NoteRettifica;
                            _context.Update(rimanenza);
                        }
                    }
                    else
                    {
                        // Crea nuovo record
                        _context.PstreeListaRimanenze.Add(new PstreeListaRimanenze
                        {
                            Anno = model.Anno,
                            Mese = model.Mese,
                            IdSede = model.IdSede,
                            IdFamiglia = item.IdFamiglia,
                            Valore = valore,
                            RettificaValore = rettifica,
                            NoteRettifica = item.NoteRettifica
                        });
                    }
                }

                await _context.SaveChangesAsync();

                var messaggio = "Rimanenze salvate con successo!";
                if (creatoRecordDicembre)
                {
                    messaggio += $" Creati anche i record di Dicembre {model.Anno - 1} come rimanenze iniziali.";
                }
                TempData["SuccessMessage"] = messaggio;
                _logger.LogInformation("Rimanenze salvate per {Mese}/{Anno} sede {Sede}. Creato Dicembre: {CreatoD}",
                    model.Mese, model.Anno, model.IdSede, creatoRecordDicembre);

                return RedirectToAction(nameof(EditGriglia), new { anno = model.Anno, mese = model.Mese, sede = model.IdSede });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante il salvataggio delle rimanenze");
                TempData["ErrorMessage"] = "Errore durante il salvataggio: " + ex.Message;
                return RedirectToAction(nameof(EditGriglia), new { anno = model.Anno, mese = model.Mese, sede = model.IdSede });
            }
        }

        // POST: ListaRimanenze/CopyFromPreviousMonth
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CopyFromPreviousMonth(int anno, int mese, int sede)
        {
            try
            {
                // Calcola mese precedente
                int mesePrecedente, annoPrecedente;
                if (mese == 1)
                {
                    mesePrecedente = 12;
                    annoPrecedente = anno - 1;
                }
                else
                {
                    mesePrecedente = mese - 1;
                    annoPrecedente = anno;
                }
                
                // Carica rimanenze del mese precedente
                var rimanenzePrecedenti = await _context.PstreeListaRimanenze
                    .Where(r => r.Anno == annoPrecedente && r.Mese == mesePrecedente && r.IdSede == sede)
                    .ToDictionaryAsync(r => r.IdFamiglia, r => r.Valore);
                
                if (!rimanenzePrecedenti.Any())
                {
                    TempData["WarningMessage"] = $"Nessuna rimanenza trovata per {Pstree_RimanenzeGrigliaViewModel.NomiMesi[mesePrecedente]} {annoPrecedente}.";
                    return RedirectToAction(nameof(EditGriglia), new { anno, mese, sede });
                }
                
                // Aggiorna o crea i record del mese corrente
                var rimanenzeAttuali = await _context.PstreeListaRimanenze
                    .Where(r => r.Anno == anno && r.Mese == mese && r.IdSede == sede)
                    .ToListAsync();
                
                // Famiglie (esclusa Analitica Id=0)
                var famiglie = await _context.PstreeListaFamiglie
                    .Where(f => f.Id != 0)
                    .ToListAsync();
                
                foreach (var famiglia in famiglie)
                {
                    var valorePrecedente = rimanenzePrecedenti.GetValueOrDefault(famiglia.Id, 0);
                    var rimanenzaAttuale = rimanenzeAttuali.FirstOrDefault(r => r.IdFamiglia == famiglia.Id);
                    
                    if (rimanenzaAttuale != null)
                    {
                        rimanenzaAttuale.Valore = valorePrecedente;
                        _context.Update(rimanenzaAttuale);
                    }
                    else
                    {
                        _context.PstreeListaRimanenze.Add(new PstreeListaRimanenze
                        {
                            Anno = anno,
                            Mese = mese,
                            IdSede = sede,
                            IdFamiglia = famiglia.Id,
                            Valore = valorePrecedente
                        });
                    }
                }
                
                await _context.SaveChangesAsync();
                
                TempData["SuccessMessage"] = $"Valori copiati da {Pstree_RimanenzeGrigliaViewModel.NomiMesi[mesePrecedente]} {annoPrecedente}!";
                _logger.LogInformation("Rimanenze copiate da {MesePrecedente}/{AnnoPrecedente} a {Mese}/{Anno} sede {Sede}", 
                    mesePrecedente, annoPrecedente, mese, anno, sede);
                
                return RedirectToAction(nameof(EditGriglia), new { anno, mese, sede });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante la copia delle rimanenze");
                TempData["ErrorMessage"] = "Errore durante la copia: " + ex.Message;
                return RedirectToAction(nameof(EditGriglia), new { anno, mese, sede });
            }
        }

        // POST: ListaRimanenze/DeleteAllGriglia
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteAllGriglia(int anno, int mese, int sede)
        {
            try
            {
                var rimanenze = await _context.PstreeListaRimanenze
                    .Where(r => r.Anno == anno && r.Mese == mese && r.IdSede == sede)
                    .ToListAsync();
                
                if (!rimanenze.Any())
                {
                    TempData["WarningMessage"] = "Nessuna rimanenza da eliminare.";
                    return RedirectToAction(nameof(Index));
                }
                
                _context.PstreeListaRimanenze.RemoveRange(rimanenze);
                await _context.SaveChangesAsync();
                
                TempData["SuccessMessage"] = $"Eliminate {rimanenze.Count} rimanenze per {Pstree_RimanenzeGrigliaViewModel.NomiMesi[mese]} {anno}!";
                _logger.LogInformation("Eliminate {Count} rimanenze per {Mese}/{Anno} sede {Sede}", 
                    rimanenze.Count, mese, anno, sede);
                
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante l'eliminazione delle rimanenze");
                TempData["ErrorMessage"] = "Errore durante l'eliminazione: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        // === HELPER PER GRIGLIA ===
        
        private async Task<Pstree_RimanenzeGrigliaViewModel> CaricaOCreaGrigliaAsync(int anno, int mese, int sede)
        {
            var viewModel = new Pstree_RimanenzeGrigliaViewModel
            {
                Anno = anno,
                Mese = mese,
                IdSede = sede,
                NomeMese = Pstree_RimanenzeGrigliaViewModel.NomiMesi[mese]
            };
            
            // Carica nome sede
            var sedeEntity = await _context.PstreeListaSedi.FirstOrDefaultAsync(s => s.Id == sede);
            viewModel.NomeSede = sedeEntity?.Sede ?? sede.ToString();
            
            // Carica anni disponibili
            var annoCorrente = DateTime.Now.Year;
            viewModel.AnniDisponibili = Enumerable.Range(annoCorrente - 2, 5).OrderByDescending(a => a).ToList();
            
            // Carica sedi
            viewModel.Sedi = await _context.PstreeListaSedi
                .OrderBy(s => s.Id)
                .Select(s => new Pstree_SedeDropdownItem { Id = s.Id, Nome = s.Sede })
                .ToListAsync();
            
            // Carica famiglie (esclusa Analitica Id=0)
            var famiglie = await _context.PstreeListaFamiglie
                .Where(f => f.Id != 0)
                .OrderBy(f => f.Id)
                .ToListAsync();
            
            // Carica rimanenze esistenti per questo mese/anno/sede
            var rimanenzeAttuali = await _context.PstreeListaRimanenze
                .Where(r => r.Anno == anno && r.Mese == mese && r.IdSede == sede)
                .ToDictionaryAsync(r => r.IdFamiglia, r => r);
            
            viewModel.RecordEsistenti = rimanenzeAttuali.Any();
            
            // Calcola mese precedente per rimanenza iniziale
            int mesePrecedente, annoPrecedente;
            if (mese == 1)
            {
                mesePrecedente = 12;
                annoPrecedente = anno - 1;
            }
            else
            {
                mesePrecedente = mese - 1;
                annoPrecedente = anno;
            }
            
            // Carica rimanenze del mese precedente (= rimanenza iniziale)
            // Usa ValoreEffettivo (Valore + Rettifica) per la rimanenza iniziale
            var rimanenzePrecedenti = await _context.PstreeListaRimanenze
                .Where(r => r.Anno == annoPrecedente && r.Mese == mesePrecedente && r.IdSede == sede)
                .ToDictionaryAsync(r => r.IdFamiglia, r => r.Valore + r.RettificaValore);
            
            // === GESTIONE GENNAIO SENZA DICEMBRE ===
            // Se siamo a Gennaio e non esistono rimanenze di Dicembre precedente
            viewModel.IsGennaioSenzaDicembre = (mese == 1 && !rimanenzePrecedenti.Any());

            // Costruisci le righe
            foreach (var famiglia in famiglie)
            {
                var riga = new Pstree_RimanenzaRigaViewModel
                {
                    IdFamiglia = famiglia.Id,
                    NomeFamiglia = famiglia.NomeFamiglia,
                    RimanenzaIniziale = rimanenzePrecedenti.GetValueOrDefault(famiglia.Id, 0)
                };

                if (rimanenzeAttuali.TryGetValue(famiglia.Id, out var rimanenza))
                {
                    riga.Id = rimanenza.Id;
                    riga.RimanenzaFinale = rimanenza.Valore;
                    riga.RettificaValore = rimanenza.RettificaValore;
                    riga.NoteRettifica = rimanenza.NoteRettifica;
                }
                else
                {
                    riga.Id = 0;
                    riga.RimanenzaFinale = 0;
                    riga.RettificaValore = 0;
                    riga.NoteRettifica = null;
                }

                viewModel.Righe.Add(riga);
            }

            return viewModel;
        }

        // === METODI HELPER ===

        private bool RimanenzaExists(int id)
        {
            return _context.PstreeListaRimanenze.Any(r => r.Id == id);
        }

        private async Task PopulateFamiglieDropdownAsync(int? selectedId = null)
        {
            var famiglie = await _context.PstreeListaFamiglie
                .OrderBy(f => f.Id)
                .Select(f => new SelectListItem
                {
                    Value = f.Id.ToString(),
                    Text = $"{f.Id} - {f.NomeFamiglia}",
                    Selected = selectedId.HasValue && f.Id == selectedId.Value
                })
                .ToListAsync();

            ViewBag.Famiglie = famiglie;
        }

        private async Task PopulateSediDropdownAsync(int? selectedId = null)
        {
            var sedi = await _context.PstreeListaSedi
                .OrderBy(s => s.Id)
                .Select(s => new SelectListItem
                {
                    Value = s.Id.ToString(),
                    Text = $"{s.Id} - {s.Sede}",
                    Selected = selectedId.HasValue && s.Id == selectedId.Value
                })
                .ToListAsync();

            ViewBag.Sedi = sedi;
        }

        private void PopulateMesiDropdown(int? selectedMese = null)
        {
            var mesi = new List<SelectListItem>
            {
                new SelectListItem { Value = "1", Text = "Gennaio", Selected = selectedMese == 1 },
                new SelectListItem { Value = "2", Text = "Febbraio", Selected = selectedMese == 2 },
                new SelectListItem { Value = "3", Text = "Marzo", Selected = selectedMese == 3 },
                new SelectListItem { Value = "4", Text = "Aprile", Selected = selectedMese == 4 },
                new SelectListItem { Value = "5", Text = "Maggio", Selected = selectedMese == 5 },
                new SelectListItem { Value = "6", Text = "Giugno", Selected = selectedMese == 6 },
                new SelectListItem { Value = "7", Text = "Luglio", Selected = selectedMese == 7 },
                new SelectListItem { Value = "8", Text = "Agosto", Selected = selectedMese == 8 },
                new SelectListItem { Value = "9", Text = "Settembre", Selected = selectedMese == 9 },
                new SelectListItem { Value = "10", Text = "Ottobre", Selected = selectedMese == 10 },
                new SelectListItem { Value = "11", Text = "Novembre", Selected = selectedMese == 11 },
                new SelectListItem { Value = "12", Text = "Dicembre", Selected = selectedMese == 12 }
            };

            ViewBag.Mesi = mesi;
        }

        private async Task PopulateAnniDropdownAsync(int? selectedAnno = null)
        {
            var anniDb = await _context.PstreeListaRimanenze
                .Select(r => r.Anno)
                .Distinct()
                .ToListAsync();

            var currentYear = DateTime.Now.Year;
            var anni = anniDb.Union(new[] { currentYear, currentYear - 1, currentYear + 1 })
                .Distinct()
                .OrderByDescending(a => a)
                .Select(a => new SelectListItem
                {
                    Value = a.ToString(),
                    Text = a.ToString(),
                    Selected = selectedAnno.HasValue && a == selectedAnno.Value
                })
                .ToList();

            ViewBag.Anni = anni;
        }

        private static string GetNomeMese(int mese)
        {
            var nomi = new[] { "", "Gennaio", "Febbraio", "Marzo", "Aprile", "Maggio", "Giugno",
                              "Luglio", "Agosto", "Settembre", "Ottobre", "Novembre", "Dicembre" };
            return mese >= 1 && mese <= 12 ? nomi[mese] : mese.ToString();
        }
    }
}
