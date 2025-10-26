using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using AiDbMaster.Data;
using AiDbMaster.Models;
using System.Globalization;

namespace AiDbMaster.Controllers
{
    /// <summary>
    /// Controller per la gestione dello Schedulatore di Produzione
    /// Utilizza Syncfusion Room Scheduler per visualizzare gli ordini di produzione per centro di lavoro
    /// </summary>
    [Authorize]
    public class SchedulatoreController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<SchedulatoreController> _logger;

        public SchedulatoreController(ApplicationDbContext context, ILogger<SchedulatoreController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Pagina principale dello Schedulatore
        /// </summary>
        public IActionResult Index()
        {
            ViewBag.Title = "Schedulatore di Produzione";
            return View();
        }

        /// <summary>
        /// Endpoint di test per verificare il funzionamento del controller
        /// </summary>
        [HttpGet]
        public IActionResult Test()
        {
            try
            {
                _logger.LogInformation("Test endpoint chiamato");
                
                var result = new
                {
                    success = true,
                    message = "Controller funzionante",
                    timestamp = DateTime.Now,
                    database = _context.Database.CanConnect(),
                    centriCount = _context.CentriLavoro.Count(),
                    ordiniCount = _context.ListaOP.Count()
                };
                
                return Json(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore nel test endpoint");
                return Json(new { 
                    success = false, 
                    message = ex.Message,
                    timestamp = DateTime.Now
                });
            }
        }

        /// <summary>
        /// API per ottenere i centri di lavoro per il dropdown del popup
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetCentriLavoroDropdown()
        {
            try
            {
                var centri = await _context.CentriLavoro
                    .Where(c => c.Attivo)
                    .OrderBy(c => c.DescrizioneCentro)
                    .Select(c => new
                    {
                        Id = c.CodiceCentro,
                        Nome = c.DescrizioneCentro
                    })
                    .ToListAsync();

                return Json(centri);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore nel caricamento centri di lavoro per dropdown");
                return Json(new { error = ex.Message });
            }
        }

        /// <summary>
        /// API per ottenere i centri di lavoro (rooms per lo scheduler)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetCentriLavoro()
        {
            try
            {
                _logger.LogInformation("Inizio caricamento centri di lavoro");
                
                // Prima recuperiamo i dati dal database
                var centriFromDb = await _context.CentriLavoro
                    .Where(c => c.Attivo)
                    .OrderBy(c => c.DescrizioneCentro)
                    .Select(c => new
                    {
                        Id = c.CodiceCentro,
                        Name = c.DescrizioneCentro,
                        Capacity = c.CapacitaOraria ?? 1
                    })
                    .ToListAsync();

                // Poi aggiungiamo i colori in memoria
                var centriLavoro = centriFromDb.Select(c => new
                {
                    Id = c.Id,
                    Name = c.Name,
                    Color = GetCentroLavoroColor(c.Id),
                    Capacity = c.Capacity
                }).ToList();

                _logger.LogInformation($"Primi 3 centri: {string.Join(", ", centriLavoro.Take(3).Select(c => $"Id:{c.Id}, Name:{c.Name}"))}");

                _logger.LogInformation($"Caricati {centriLavoro.Count} centri di lavoro");
                return Json(centriLavoro);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore nel recupero dei centri di lavoro");
                return Json(new { error = true, message = ex.Message });
            }
        }

        /// <summary>
        /// API per ottenere gli ordini di produzione (eventi per lo scheduler)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetOrdiniProduzione(DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                _logger.LogInformation("Inizio caricamento ordini di produzione");
                
                // Se non specificate, usa un range di default (30 giorni prima e dopo oggi)
                var start = startDate ?? DateTime.Today.AddDays(-30);
                var end = endDate ?? DateTime.Today.AddDays(30);

                _logger.LogInformation($"Range date: {start:yyyy-MM-dd} - {end:yyyy-MM-dd}");

                // Prima recuperiamo i dati dal database
                var ordiniFromDb = await _context.ListaOP
                    .Include(o => o.Stato)
                    .Include(o => o.CentroLavoro)
                    .Where(o => o.DataInizioOP >= start && 
                               (o.DataFineOP <= end || o.DataFinePrevista <= end || 
                                (o.DataFineOP == null && o.DataFinePrevista == null)))
                    .Select(o => new
                    {
                        Id = o.IdListaOP,
                        CodiceArticolo = o.CodiceArticolo,
                        DescrizioneArticolo = o.DescrizioneArticolo,
                        Quantita = o.Quantita,
                        QuantitaProdotta = o.QuantitaProdotta,
                        DescrOrdine = o.DescrOrdine ?? "",
                        DataInizioOP = o.DataInizioOP,
                        DataFineOP = o.DataFineOP,
                        DataFinePrevista = o.DataFinePrevista,
                        CodiceCentro = o.CodiceCentro,
                        IdStato = o.IdStato,
                        StatoDescrizione = o.Stato != null ? o.Stato.DescrizioneStato : "",
                        CentroLavoroDescrizione = o.CentroLavoro != null ? o.CentroLavoro.DescrizioneCentro : "",
                        Priorita = o.Priorita ?? 2,
                        TipoOrdine = o.TipoOrdine,
                        AnnoOrdine = o.AnnoOrdine,
                        SerieOrdine = o.SerieOrdine,
                        NumeroOrdine = o.NumeroOrdine,
                        Note = o.Note ?? "",
                        TempoCiclo = o.TempoCiclo,
                        TempoSetup = o.TempoSetup ?? 0
                    })
                    .ToListAsync();

                // Poi trasformiamo i dati calcolando l'EndTime con i fermi
                var ordiniList = new List<object>();
                
                foreach (var o in ordiniFromDb)
                {
                    DateTime endTime;
                    
                    if (o.IdStato == 3) // Chiuso: usa DataFineOP
                    {
                        endTime = o.DataFineOP.Value;
                    }
                    else if (o.IdStato == 2) // In Produzione: usa DataFineOP
                    {
                        endTime = o.DataFineOP.Value;
                    }
                    else // Altri stati: calcola al volo + allunga per fermi
                    {
                        // Calcola EndTime teorico
                        var endTimeTeorico = o.DataInizioOP.AddSeconds((double)(o.Quantita * (decimal)o.TempoCiclo));
                        
                        // Calcola sovrapposizioni con fermi
                        var durataSovrapposizioneFermi = await CalcolaDurataSovrapposizioneFermi(
                            o.CodiceCentro, 
                            o.DataInizioOP, 
                            endTimeTeorico
                        );
                        
                        // Allunga l'EndTime per compensare i fermi
                        endTime = endTimeTeorico.Add(durataSovrapposizioneFermi);
                        
                        if (durataSovrapposizioneFermi > TimeSpan.Zero)
                        {
                            _logger.LogInformation($"Ordine {o.Id}: EndTime allungato di {durataSovrapposizioneFermi.TotalMinutes} minuti per fermi");
                        }
                    }
                    
                    ordiniList.Add(new
                    {
                        Id = o.Id,
                        Subject = $"Ord. {o.TipoOrdine}-{o.AnnoOrdine}-{o.SerieOrdine}-{o.NumeroOrdine} Qta: {Math.Floor(o.Quantita)}",
                        Description = o.DescrOrdine,
                        StartTime = o.DataInizioOP,
                        EndTime = endTime,
                        RoomId = o.CodiceCentro,
                        CategoryColor = GetStatoColor(o.IdStato),
                        IsAllDay = false,
                        RecurrenceRule = "",
                        // Dati aggiuntivi per il tooltip e la modifica
                        CodiceArticolo = o.CodiceArticolo,
                        DescrizioneArticolo = o.DescrizioneArticolo,
                        Quantita = o.Quantita,
                        QuantitaProdotta = o.QuantitaProdotta,
                        IdStato = o.IdStato,
                        StatoDescrizione = o.StatoDescrizione,
                        CentroLavoro = o.CentroLavoroDescrizione,
                        CodiceCentro = o.CodiceCentro,
                        Priorita = o.Priorita,
                        PercentualeCompletamento = o.Quantita > 0 ? Math.Round((o.QuantitaProdotta / o.Quantita) * 100, 2) : 0,
                        TipoOrdine = o.TipoOrdine,
                        AnnoOrdine = o.AnnoOrdine,
                        SerieOrdine = o.SerieOrdine,
                        NumeroOrdine = o.NumeroOrdine,
                        Note = o.Note,
                        TempoCiclo = o.TempoCiclo,
                        TempoSetup = o.TempoSetup
                    });
                }

                _logger.LogInformation($"Caricati {ordiniList.Count} ordini di produzione con calcolo fermi");
                return Json(ordiniList);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore nel recupero degli ordini di produzione");
                return Json(new { error = true, message = ex.Message });
            }
        }

        /// <summary>
        /// API per aggiornare la programmazione di un ordine (drag & drop)
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> UpdateOrdine([FromBody] UpdateOrdineRequest request)
        {
            try
            {
                var ordine = await _context.ListaOP.FindAsync(request.Id);
                if (ordine == null)
                {
                    return NotFound("Ordine non trovato");
                }

                // Aggiorna la data di inizio
                ordine.DataInizioOP = request.StartTime;

                // LOGICA DRAG & DROP vs RESIZE
                if (request.IsResize)
                {
                    // RESIZE: Ricalcola la quantità in base alla nuova durata NETTA (senza fermi)
                    // L'utente ha fatto resize sull'evento VISUALE che include i fermi
                    // Dobbiamo sottrarre i fermi per ottenere la durata di produzione effettiva
                    
                    var durataVisualeSecondi = (decimal)(request.EndTime - request.StartTime).TotalSeconds;
                    
                    // Trova i fermi sovrapposti nel periodo visuale
                    var durataSovrapposizioneFermiVisuale = await CalcolaDurataSovrapposizioneFermi(
                        ordine.CodiceCentro,
                        request.StartTime,
                        request.EndTime
                    );
                    
                    // Calcola la durata NETTA (senza fermi)
                    var durataNettatSecondi = durataVisualeSecondi - (decimal)durataSovrapposizioneFermiVisuale.TotalSeconds;
                    
                    _logger.LogInformation($"Ordine {ordine.IdListaOP} RESIZE: " +
                                         $"durata visuale={durataVisualeSecondi}s, " +
                                         $"fermi={durataSovrapposizioneFermiVisuale.TotalSeconds}s, " +
                                         $"durata netta={durataNettatSecondi}s");
                    
                    if (ordine.TempoCiclo > 0 && durataNettatSecondi > 0)
                    {
                        ordine.Quantita = Math.Round(durataNettatSecondi / (decimal)ordine.TempoCiclo, 2);
                        _logger.LogInformation($"Nuova Quantità calcolata: {ordine.Quantita} " +
                                             $"(TempoCiclo: {ordine.TempoCiclo}s)");
                    }
                    else if (ordine.TempoCiclo <= 0)
                    {
                        _logger.LogWarning($"Ordine {ordine.IdListaOP}: TempoCiclo = 0, impossibile ricalcolare Quantità");
                    }
                    else
                    {
                        _logger.LogWarning($"Ordine {ordine.IdListaOP}: durata netta <= 0, l'ordine è completamente coperto da fermi");
                        ordine.Quantita = 0;
                    }
                    
                    // Aggiorna la data fine in base allo stato
                    if (ordine.IdStato == 3) // Chiuso - non dovrebbe mai arrivare qui (bloccato nel frontend)
                    {
                        return BadRequest("Gli ordini chiusi non possono essere modificati");
                    }
                    else if (ordine.IdStato == 2) // In Produzione
                    {
                        // Verifica che la nuova data fine non sia nel passato
                        if (request.EndTime < DateTime.Now)
                        {
                            return BadRequest("Non puoi ridurre l'ordine a una data passata");
                        }
                        ordine.DataFineOP = request.EndTime;
                        _logger.LogInformation($"Ordine {ordine.IdListaOP} in produzione: DataFineOP aggiornata a {request.EndTime:yyyy-MM-dd HH:mm}");
                    }
                    else // Altri stati (Emesso, Sospeso, Urgente...)
                    {
                        // Per il resize: DataFinePrevista = EndTime impostato dall'utente
                        // L'utente ha già posizionato l'evento tenendo conto dei fermi visualizzati
                        // NON ricalcolare i fermi, altrimenti verrebbero contati due volte
                        ordine.DataFinePrevista = request.EndTime;
                        _logger.LogInformation($"RESIZE - DataFinePrevista impostata a {ordine.DataFinePrevista:yyyy-MM-dd HH:mm} (come indicato dall'utente)");
                    }
                }
                else
                {
                    // DRAG & DROP: NON modificare il centro, NON modificare la quantità
                    // La durata rimane invariata, quindi EndTime si ricalcola automaticamente dalla vista
                    _logger.LogInformation($"Ordine {ordine.IdListaOP} DRAG&DROP: " +
                                         $"nuovo inizio {ordine.DataInizioOP:yyyy-MM-dd HH:mm}, " +
                                         $"centro {ordine.CodiceCentro} (invariato), Quantità {ordine.Quantita} (invariata)");
                    
                    // Per gli stati != 2 e != 3, aggiorna DataFinePrevista considerando i fermi
                    if (ordine.IdStato != 2 && ordine.IdStato != 3)
                    {
                        // Calcola EndTime teorico
                        var endTimeTeorico = request.StartTime.AddSeconds((double)(ordine.Quantita * (decimal)ordine.TempoCiclo));
                        
                        // Calcola sovrapposizioni con fermi nel nuovo periodo
                        var durataSovrapposizioneFermi = await CalcolaDurataSovrapposizioneFermi(
                            ordine.CodiceCentro, 
                            request.StartTime, 
                            endTimeTeorico
                        );
                        
                        // Allunga per compensare i fermi
                        ordine.DataFinePrevista = endTimeTeorico.Add(durataSovrapposizioneFermi);
                        
                        if (durataSovrapposizioneFermi > TimeSpan.Zero)
                        {
                            _logger.LogInformation($"DataFinePrevista allungata di {durataSovrapposizioneFermi.TotalMinutes} minuti per fermi: {ordine.DataFinePrevista:yyyy-MM-dd HH:mm}");
                        }
                        else
                        {
                            _logger.LogInformation($"DataFinePrevista ricalcolata (nessun fermo): {ordine.DataFinePrevista:yyyy-MM-dd HH:mm}");
                        }
                    }
                }

                // Imposta il flag Modificato per entrambi i casi
                ordine.Modificato = true;

                await _context.SaveChangesAsync();

                _logger.LogInformation($"Ordine {ordine.IdListaOP} aggiornato con successo (Modificato=1)");

                return Ok(new { 
                    success = true, 
                    message = "Ordine aggiornato con successo",
                    nuovaQuantita = ordine.Quantita
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Errore nell'aggiornamento dell'ordine {request.Id}");
                return StatusCode(500, "Errore nell'aggiornamento dell'ordine");
            }
        }

        /// <summary>
        /// API per ottenere i fermi dei centri di lavoro
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetFermiCentriLavoro(DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                var start = startDate ?? DateTime.Today.AddDays(-30);
                var end = endDate ?? DateTime.Today.AddDays(90);

                var fermi = await _context.CalendarioFermiCentriLavoro
                    .Where(f => f.DataInizioFermo >= start && f.DataFineFermo <= end && 
                               (f.CodiceCentro != null && f.CodiceCentro != ""))
                    .Select(f => new
                    {
                        Id = f.Id,
                        CodiceCentro = f.CodiceCentro,
                        DataInizio = f.DataInizioFermo,
                        DataFine = f.DataFineFermo,
                        Descrizione = f.Motivo ?? "",
                        TipoFermo = f.TipoFermo.ToString()
                    })
                    .ToListAsync();

                _logger.LogInformation($"Caricati {fermi.Count} fermi centri lavoro");

                return Ok(fermi);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore nel caricamento fermi centri lavoro");
                return StatusCode(500, "Errore nel caricamento dei fermi");
            }
        }

        /// <summary>
        /// Calcola la durata totale delle sovrapposizioni con i fermi per un ordine
        /// </summary>
        private async Task<TimeSpan> CalcolaDurataSovrapposizioneFermi(
            string codiceCentro, 
            DateTime dataInizio, 
            DateTime dataFine)
        {
            try
            {
                // Recupera i fermi del centro che si sovrappongono al periodo dell'ordine
                var fermiSovrapposti = await _context.CalendarioFermiCentriLavoro
                    .Where(f => f.CodiceCentro == codiceCentro &&
                               f.DataInizioFermo < dataFine &&
                               f.DataFineFermo > dataInizio)
                    .ToListAsync();

                if (!fermiSovrapposti.Any())
                {
                    return TimeSpan.Zero;
                }

                TimeSpan durataTotale = TimeSpan.Zero;

                foreach (var fermo in fermiSovrapposti)
                {
                    // Calcola l'intersezione tra ordine e fermo
                    var inizioSovrapposizione = dataInizio > fermo.DataInizioFermo ? dataInizio : fermo.DataInizioFermo;
                    var fineSovrapposizione = dataFine < fermo.DataFineFermo ? dataFine : fermo.DataFineFermo.Value;

                    if (fineSovrapposizione > inizioSovrapposizione)
                    {
                        var durataSovrapposizione = fineSovrapposizione - inizioSovrapposizione;
                        durataTotale += durataSovrapposizione;

                        _logger.LogDebug($"Fermo sovrapposto: {fermo.Id}, durata: {durataSovrapposizione.TotalMinutes} minuti");
                    }
                }

                _logger.LogInformation($"Centro {codiceCentro}: durata totale sovrapposizione fermi = {durataTotale.TotalMinutes} minuti");
                return durataTotale;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Errore nel calcolo sovrapposizioni fermi per centro {codiceCentro}");
                return TimeSpan.Zero;
            }
        }

        /// <summary>
        /// Ottiene il colore per uno stato specifico
        /// </summary>
        private static string GetStatoColor(int idStato)
        {
            return idStato switch
            {
                1 => "#FFA500", // Emesso - Arancione
                2 => "#1E90FF", // In Produzione - Blu
                3 => "#32CD32", // Chiuso - Verde
                4 => "#FF6347", // Sospeso - Rosso
                5 => "#9932CC", // Urgente - Viola
                _ => "#808080"  // Default - Grigio
            };
        }

        /// <summary>
        /// Ottiene un colore per il centro di lavoro (per differenziare visivamente)
        /// </summary>
        private static string GetCentroLavoroColor(string codiceCentro)
        {
            var colors = new[] { "#E3F2FD", "#F3E5F5", "#E8F5E8", "#FFF3E0", "#FCE4EC", "#F1F8E9" };
            return colors[Math.Abs(codiceCentro.GetHashCode()) % colors.Length];
        }

        /// <summary>
        /// API per salvare le modifiche di un ordine dal popup
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> SaveOrderDetails([FromBody] SaveOrderDetailsRequest request)
        {
            try
            {
                var ordine = await _context.ListaOP.FindAsync(request.Id);
                if (ordine == null)
                {
                    return NotFound("Ordine non trovato");
                }

                // Aggiorna solo i campi forniti (non null)
                if (request.Quantita.HasValue)
                    ordine.Quantita = request.Quantita.Value;
                
                if (request.QuantitaProdotta.HasValue)
                    ordine.QuantitaProdotta = request.QuantitaProdotta.Value;
                
                // IdStato viene sempre aggiornato
                ordine.IdStato = request.IdStato;
                
                if (!string.IsNullOrEmpty(request.CodiceCentro))
                    ordine.CodiceCentro = request.CodiceCentro;
                
                if (request.Note != null)
                    ordine.Note = request.Note;

                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Modifiche salvate con successo" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore nel salvataggio delle modifiche ordine {Id}", request.Id);
                return Json(new { success = false, message = ex.Message });
            }
        }
    }

    /// <summary>
    /// Modello per la richiesta di aggiornamento ordine
    /// </summary>
    public class UpdateOrdineRequest
    {
        public int Id { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string? RoomId { get; set; }
        public bool IsResize { get; set; } // true = resize, false = drag&drop
    }

    /// <summary>
    /// Modello per la richiesta di salvataggio dettagli ordine
    /// </summary>
    public class SaveOrderDetailsRequest
    {
        public int Id { get; set; }
        public decimal? Quantita { get; set; }
        public decimal? QuantitaProdotta { get; set; }
        public int IdStato { get; set; }
        public string? CodiceCentro { get; set; }
        public string? Note { get; set; }
    }
}
