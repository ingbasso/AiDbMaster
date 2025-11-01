using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AiDbMaster.Data;
using AiDbMaster.Models;
using AiDbMaster.ViewModels;

namespace AiDbMaster.Controllers
{
    [Authorize(Roles = "Admin,Manager")]
    public class CalendarioFermiController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<CalendarioFermiController> _logger;

        // Colori per i centri di lavoro (massimo 10 centri)
        private readonly string[] _coloriCentri = new[]
        {
            "#e74c3c", // Rosso
            "#3498db", // Blu
            "#2ecc71", // Verde
            "#f39c12", // Arancione
            "#9b59b6", // Viola
            "#1abc9c", // Turchese
            "#e67e22", // Arancione scuro
            "#34495e", // Grigio scuro
            "#16a085", // Verde acqua
            "#c0392b"  // Rosso scuro
        };

        public CalendarioFermiController(
            ApplicationDbContext context,
            ILogger<CalendarioFermiController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Pagina principale con Syncfusion Scheduler
        /// </summary>
        public async Task<IActionResult> Index()
        {
            try
            {
                _logger.LogInformation("Caricamento pagina Calendario Fermi");
                
                // Carica lista centri ATTIVI per dropdown
                var centri = await _context.CentriLavoro
                    .Where(c => c.Attivo == true) // Solo centri attivi
                    .OrderBy(c => c.CodiceCentro)
                    .Select(c => new { c.CodiceCentro, c.DescrizioneCentro, c.Attivo })
                    .ToListAsync();

                _logger.LogInformation($"Centri attivi caricati per ViewBag: {centri.Count}");
                
                ViewBag.CentriLavoro = centri;
                
                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore nel caricamento della pagina Calendario Fermi");
                ViewBag.CentriLavoro = new List<object>();
                return View();
            }
        }

        /// <summary>
        /// API per ottenere i centri di lavoro (resources per lo scheduler timeline)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetCentriLavoro()
        {
            try
            {
                _logger.LogInformation("Inizio caricamento centri di lavoro per timeline");
                
                // Prima verifica: quanti centri ci sono in totale
                var tuttiCentri = await _context.CentriLavoro.CountAsync();
                _logger.LogInformation($"Totale centri in database: {tuttiCentri}");
                
                // Seconda verifica: quanti centri attivi
                var centriAttivi = await _context.CentriLavoro.Where(c => c.Attivo).CountAsync();
                _logger.LogInformation($"Centri attivi (Attivo=1): {centriAttivi}");
                
                var centriFromDb = await _context.CentriLavoro
                    .Where(c => c.Attivo == true) // Esplicito: Attivo = 1
                    .OrderBy(c => c.DescrizioneCentro)
                    .Select(c => new
                    {
                        Id = c.CodiceCentro,
                        Name = c.DescrizioneCentro,
                        Capacity = c.CapacitaOraria ?? 1,
                        Attivo = c.Attivo
                    })
                    .ToListAsync();

                _logger.LogInformation($"Query eseguita, centri trovati: {centriFromDb.Count}");
                
                // Log dettaglio dei centri
                foreach (var centro in centriFromDb.Take(5))
                {
                    _logger.LogInformation($"Centro: {centro.Id} - {centro.Name} - Attivo: {centro.Attivo}");
                }

                // Aggiungi colori in memoria
                var centriLavoro = centriFromDb.Select((c, index) => new
                {
                    Id = c.Id,
                    Name = c.Name,
                    Color = _coloriCentri[index % _coloriCentri.Length],
                    Capacity = c.Capacity
                }).ToList();

                _logger.LogInformation($"Caricati {centriLavoro.Count} centri di lavoro per timeline");
                
                if (centriLavoro.Count == 0)
                {
                    _logger.LogWarning("ATTENZIONE: Nessun centro di lavoro attivo trovato!");
                    
                    // Restituisci comunque un array vuoto, non un errore
                    return Json(new List<object>());
                }
                
                return Json(centriLavoro);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore nel recupero dei centri di lavoro");
                return StatusCode(500, new { error = true, message = ex.Message });
            }
        }

        /// <summary>
        /// API: Ottiene tutti i fermi per il calendario (formato Syncfusion)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetFermi(
            string? codiceCentro = null,
            DateTime? dataInizio = null,
            DateTime? dataFine = null)
        {
            try
            {
                var query = _context.CalendarioFermiCentriLavoro
                    .Include(f => f.CentroLavoro)
                    .AsQueryable();

                // Filtro per centro (se specificato)
                if (!string.IsNullOrEmpty(codiceCentro))
                {
                    query = query.Where(f => f.CodiceCentro == codiceCentro);
                }

                // Filtro per periodo (se specificato)
                if (dataInizio.HasValue)
                {
                    query = query.Where(f => f.DataFineFermo == null || f.DataFineFermo >= dataInizio.Value);
                }

                if (dataFine.HasValue)
                {
                    query = query.Where(f => f.DataInizioFermo <= dataFine.Value);
                }

                var fermi = await query.ToListAsync();

                // Usa sempre il colore rosso della FRIMA (#e74c3c - primo colore dell'array)
                const string coloreFermi = "#e74c3c"; // Rosso della FRIMA

                // Mappa a SchedulerEventViewModel
                var eventi = fermi.Select(f => new SchedulerEventViewModel
                {
                    Id = f.Id,
                    Subject = $"{f.CentroLavoro?.DescrizioneCentro ?? f.CodiceCentro}: {f.Motivo ?? f.TipoFermo.ToString()}",
                    StartTime = f.DataInizioFermo,
                    EndTime = f.DataFineFermo ?? f.DataInizioFermo.AddHours(8), // Default 8h se non specificato
                    Description = f.Note,
                    IsAllDay = false,
                    CategoryColor = coloreFermi, // Usa sempre lo stesso colore rosso
                    CodiceCentro = f.CodiceCentro,
                    DescrizioneCentro = f.CentroLavoro?.DescrizioneCentro ?? "",
                    TipoFermo = f.TipoFermo.ToString(),
                    IsPianificato = f.IsPianificato,
                    StatoFermo = f.StatoFermo
                }).ToList();

                return Json(eventi);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante il caricamento dei fermi");
                return StatusCode(500, new { success = false, message = "Errore durante il caricamento dei fermi" });
            }
        }

        /// <summary>
        /// API: Crea un nuovo fermo
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateFermo([FromBody] CalendarioFermoViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new { success = false, message = "Dati non validi", errors = ModelState });
                }

                var fermiCreati = new List<CalendarioFermiCentriLavoro>();

                // Se "Applica a tutti i centri" è true
                if (model.ApplicaATuttiICentri)
                {
                    var tuttiCentri = await _context.CentriLavoro.Select(c => c.CodiceCentro).ToListAsync();
                    
                    foreach (var centro in tuttiCentri)
                    {
                        var fermo = new CalendarioFermiCentriLavoro
                        {
                            CodiceCentro = centro,
                            DataInizioFermo = model.DataInizioFermo,
                            DataFineFermo = model.DataFineFermo,
                            TipoFermo = model.TipoFermo,
                            Motivo = model.Motivo,
                            Note = model.Note,
                            IsPianificato = model.IsPianificato,
                            DataCreazione = DateTime.Now
                        };
                        _context.CalendarioFermiCentriLavoro.Add(fermo);
                        fermiCreati.Add(fermo);
                    }
                }
                else
                {
                    // Crea fermo per singolo centro
                    var fermo = new CalendarioFermiCentriLavoro
                    {
                        CodiceCentro = model.CodiceCentro,
                        DataInizioFermo = model.DataInizioFermo,
                        DataFineFermo = model.DataFineFermo,
                        TipoFermo = model.TipoFermo,
                        Motivo = model.Motivo,
                        Note = model.Note,
                        IsPianificato = model.IsPianificato,
                        DataCreazione = DateTime.Now
                    };
                    _context.CalendarioFermiCentriLavoro.Add(fermo);
                    fermiCreati.Add(fermo);
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation("Creati {Count} fermi", fermiCreati.Count);

                return Ok(new { success = true, message = $"Fermo creato con successo ({fermiCreati.Count} centro/i)" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante la creazione del fermo");
                return StatusCode(500, new { success = false, message = "Errore durante la creazione del fermo" });
            }
        }

        /// <summary>
        /// API: Aggiorna un fermo esistente
        /// </summary>
        [HttpPut]
        [Route("CalendarioFermi/UpdateFermo/{id}")]
        public async Task<IActionResult> UpdateFermo(int id, [FromBody] CalendarioFermoViewModel model)
        {
            try
            {
                _logger.LogInformation($"Aggiornamento fermo ID {id}: {model.CodiceCentro} dal {model.DataInizioFermo:dd/MM/yyyy HH:mm} al {model.DataFineFermo:dd/MM/yyyy HH:mm}");
                
                var fermo = await _context.CalendarioFermiCentriLavoro.FindAsync(id);
                
                if (fermo == null)
                {
                    return NotFound(new { success = false, message = "Fermo non trovato" });
                }

                fermo.CodiceCentro = model.CodiceCentro;
                fermo.DataInizioFermo = model.DataInizioFermo;
                fermo.DataFineFermo = model.DataFineFermo;
                fermo.TipoFermo = model.TipoFermo;
                fermo.Motivo = model.Motivo;
                fermo.Note = model.Note;
                fermo.IsPianificato = model.IsPianificato;
                fermo.DataUltimaModifica = DateTime.Now;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Fermo {Id} aggiornato", id);

                return Ok(new { success = true, message = "Fermo aggiornato con successo" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante l'aggiornamento del fermo {Id}", id);
                return StatusCode(500, new { success = false, message = "Errore durante l'aggiornamento del fermo" });
            }
        }

        /// <summary>
        /// API: Elimina un fermo
        /// </summary>
        [HttpDelete]
        [Route("CalendarioFermi/DeleteFermo/{id}")]
        public async Task<IActionResult> DeleteFermo(int id)
        {
            try
            {
                var fermo = await _context.CalendarioFermiCentriLavoro.FindAsync(id);
                
                if (fermo == null)
                {
                    return NotFound(new { success = false, message = "Fermo non trovato" });
                }

                _context.CalendarioFermiCentriLavoro.Remove(fermo);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Fermo {Id} eliminato", id);

                return Ok(new { success = true, message = "Fermo eliminato con successo" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante l'eliminazione del fermo {Id}", id);
                return StatusCode(500, new { success = false, message = "Errore durante l'eliminazione del fermo" });
            }
        }

        /// <summary>
        /// API: Genera weekend automaticamente per un periodo
        /// Crea UN UNICO evento per ogni periodo consecutivo di weekend (sabato-domenica)
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> GeneraWeekend([FromBody] GeneraWeekendViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new { success = false, message = "Dati non validi" });
                }

                // Carica solo i centri ATTIVI
                var centri = model.ApplicaATutti
                    ? await _context.CentriLavoro
                        .Where(c => c.Attivo == true)
                        .Select(c => c.CodiceCentro)
                        .ToListAsync()
                    : model.CentriSelezionati;

                if (!centri.Any())
                {
                    return BadRequest(new { success = false, message = "Seleziona almeno un centro o non ci sono centri attivi" });
                }

                var fermiCreati = 0;
                
                // Raggruppa i weekend consecutivi
                var periodiWeekend = new List<(DateTime Inizio, DateTime Fine)>();
                var dataCorrente = model.DataInizio.Date;
                DateTime? inizioWeekendCorrente = null;
                DateTime? fineWeekendCorrente = null;

                while (dataCorrente <= model.DataFine.Date)
                {
                    // Controlla se è sabato o domenica
                    if (dataCorrente.DayOfWeek == DayOfWeek.Saturday || 
                        dataCorrente.DayOfWeek == DayOfWeek.Sunday)
                    {
                        if (inizioWeekendCorrente == null)
                        {
                            // Inizio di un nuovo weekend
                            inizioWeekendCorrente = dataCorrente;
                        }
                        fineWeekendCorrente = dataCorrente;
                    }
                    else
                    {
                        // Non è weekend, salva il periodo precedente se esiste
                        if (inizioWeekendCorrente.HasValue && fineWeekendCorrente.HasValue)
                        {
                            periodiWeekend.Add((inizioWeekendCorrente.Value, fineWeekendCorrente.Value));
                            inizioWeekendCorrente = null;
                            fineWeekendCorrente = null;
                        }
                    }

                    dataCorrente = dataCorrente.AddDays(1);
                }

                // Salva l'ultimo periodo se il range termina in un weekend
                if (inizioWeekendCorrente.HasValue && fineWeekendCorrente.HasValue)
                {
                    periodiWeekend.Add((inizioWeekendCorrente.Value, fineWeekendCorrente.Value));
                }

                _logger.LogInformation($"Trovati {periodiWeekend.Count} periodi di weekend nel range selezionato");

                // Crea UN evento per ogni periodo di weekend per ogni centro
                foreach (var periodo in periodiWeekend)
                {
                    foreach (var centro in centri)
                    {
                        // Verifica se esiste già un fermo che copre questo periodo
                        var esisteFermoSovrapposizione = await _context.CalendarioFermiCentriLavoro
                            .AnyAsync(f => f.CodiceCentro == centro &&
                                         f.TipoFermo == TipoFermo.WeekEnd &&
                                         ((f.DataInizioFermo.Date <= periodo.Inizio && 
                                           (f.DataFineFermo == null || f.DataFineFermo.Value.Date >= periodo.Inizio)) ||
                                          (f.DataInizioFermo.Date <= periodo.Fine && 
                                           (f.DataFineFermo == null || f.DataFineFermo.Value.Date >= periodo.Fine))));

                        if (!esisteFermoSovrapposizione)
                        {
                            var fermo = new CalendarioFermiCentriLavoro
                            {
                                CodiceCentro = centro,
                                DataInizioFermo = periodo.Inizio.Date, // Inizio giornata (00:00)
                                DataFineFermo = periodo.Fine.Date.AddHours(23).AddMinutes(59), // Fine giornata (23:59)
                                TipoFermo = TipoFermo.WeekEnd,
                                Motivo = model.Motivo ?? "Weekend",
                                IsPianificato = true,
                                DataCreazione = DateTime.Now
                            };
                            _context.CalendarioFermiCentriLavoro.Add(fermo);
                            fermiCreati++;
                            
                            _logger.LogInformation(
                                $"Creato fermo weekend per centro {centro}: {periodo.Inizio:dd/MM/yyyy} - {periodo.Fine:dd/MM/yyyy}");
                        }
                    }
                }

                if (fermiCreati > 0)
                {
                    await _context.SaveChangesAsync();
                }

                _logger.LogInformation($"Generati {fermiCreati} fermi per {periodiWeekend.Count} periodi di weekend su {centri.Count} centri");

                return Ok(new 
                { 
                    success = true, 
                    message = $"Generati {fermiCreati} fermi weekend per {centri.Count} centro/i ({periodiWeekend.Count} periodo/i)", 
                    count = fermiCreati,
                    periodi = periodiWeekend.Count,
                    centri = centri.Count
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante la generazione dei weekend");
                return StatusCode(500, new { success = false, message = "Errore durante la generazione dei weekend: " + ex.Message });
            }
        }

        /// <summary>
        /// API: Genera fermi settimanali (turni notturni + weekend) per un range di settimane
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> GeneraFermiSettimanali([FromBody] GeneraFermiSettimanaliViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new { success = false, message = "Dati non validi" });
                }

                // Carica centri
                List<string> centri;
                
                if (model.ApplicaATutti)
                {
                    // Applica a tutti i centri attivi
                    centri = await _context.CentriLavoro
                        .Where(c => c.Attivo == true)
                        .Select(c => c.CodiceCentro)
                        .ToListAsync();
                }
                else if (!string.IsNullOrEmpty(model.CodiceCentro))
                {
                    // Applica solo al centro selezionato
                    var centroEsiste = await _context.CentriLavoro
                        .AnyAsync(c => c.CodiceCentro == model.CodiceCentro && c.Attivo == true);
                    
                    if (!centroEsiste)
                    {
                        return BadRequest(new { success = false, message = "Centro di lavoro non trovato o non attivo" });
                    }
                    
                    centri = new List<string> { model.CodiceCentro };
                }
                else
                {
                    return BadRequest(new { success = false, message = "Seleziona un centro o applica a tutti" });
                }

                if (!centri.Any())
                {
                    return BadRequest(new { success = false, message = "Nessun centro attivo trovato" });
                }

                var fermiCreati = 0;

                // Per ogni settimana nel range
                for (int numSettimana = model.DaSettimana; numSettimana <= model.ASettimana; numSettimana++)
                {
                    // Calcola il lunedì della settimana
                    var lunedi = GetMondayOfWeek(model.Anno, numSettimana);
                    _logger.LogInformation($"Settimana {numSettimana}/{model.Anno}: Lunedì = {lunedi:dd/MM/yyyy}");

                    foreach (var centro in centri)
                    {
                        // 1. Lun 20:00 - Mar 06:00 (Turno Notturno)
                        CreaFermoNotturno(lunedi, DayOfWeek.Monday, centro, model.Motivo);
                        fermiCreati++;

                        // 2. Mar 20:00 - Mer 06:00 (Turno Notturno)
                        CreaFermoNotturno(lunedi, DayOfWeek.Tuesday, centro, model.Motivo);
                        fermiCreati++;

                        // 3. Mer 20:00 - Gio 06:00 (Turno Notturno)
                        CreaFermoNotturno(lunedi, DayOfWeek.Wednesday, centro, model.Motivo);
                        fermiCreati++;

                        // 4. Gio 20:00 - Ven 06:00 (Turno Notturno)
                        CreaFermoNotturno(lunedi, DayOfWeek.Thursday, centro, model.Motivo);
                        fermiCreati++;

                        // 5. Weekend: Ven 20:00 - Lun 06:00 (settimana successiva)
                        var venerdi = lunedi.AddDays(4); // Venerdì della settimana
                        var lunediSuccessivo = lunedi.AddDays(7); // Lunedì settimana successiva
                        
                        _logger.LogInformation($"  Weekend: {venerdi:dd/MM/yyyy} 20:00 → {lunediSuccessivo:dd/MM/yyyy} 06:00");
                        
                        var fermoWeekend = new CalendarioFermiCentriLavoro
                        {
                            CodiceCentro = centro,
                            DataInizioFermo = venerdi.Date.AddHours(20),
                            DataFineFermo = lunediSuccessivo.Date.AddHours(6),
                            TipoFermo = TipoFermo.WeekEnd,
                            Motivo = model.Motivo ?? "Fermo programmato",
                            IsPianificato = true,
                            DataCreazione = DateTime.Now
                        };
                        _context.CalendarioFermiCentriLavoro.Add(fermoWeekend);
                        fermiCreati++;
                    }
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation($"Generati {fermiCreati} fermi settimanali per {centri.Count} centri");

                return Ok(new 
                { 
                    success = true, 
                    message = $"Generati {fermiCreati} fermi per {centri.Count} centro/i", 
                    count = fermiCreati
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante la generazione dei fermi settimanali");
                return StatusCode(500, new { success = false, message = "Errore: " + ex.Message });
            }
        }

        /// <summary>
        /// Helper: Crea fermo turno notturno (giorno 20:00 - giorno+1 06:00)
        /// </summary>
        private void CreaFermoNotturno(DateTime lunediSettimana, DayOfWeek giorno, string centro, string? motivo)
        {
            var giornoInizio = lunediSettimana.AddDays((int)giorno - (int)DayOfWeek.Monday);
            var giornoFine = giornoInizio.AddDays(1);

            var fermo = new CalendarioFermiCentriLavoro
            {
                CodiceCentro = centro,
                DataInizioFermo = giornoInizio.Date.AddHours(20),
                DataFineFermo = giornoFine.Date.AddHours(6),
                TipoFermo = TipoFermo.TurnoNotturno,
                Motivo = motivo ?? "Fermo programmato",
                IsPianificato = true,
                DataCreazione = DateTime.Now
            };
            _context.CalendarioFermiCentriLavoro.Add(fermo);
        }

        /// <summary>
        /// Helper: Ottiene il lunedì di una settimana specifica dell'anno (ISO 8601)
        /// </summary>
        private DateTime GetMondayOfWeek(int anno, int numeroSettimana)
        {
            // Usa ISOWeek di .NET per calcolare correttamente secondo lo standard ISO 8601
            return System.Globalization.ISOWeek.ToDateTime(anno, numeroSettimana, DayOfWeek.Monday);
        }
    }
}

