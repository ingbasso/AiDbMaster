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
                        Id = c.IdCentroLavoro,
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
                        Id = c.IdCentroLavoro,
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
                        IdCentroLavoro = o.IdCentroLavoro,
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

                // Poi trasformiamo i dati in memoria
                var ordini = ordiniFromDb.Select(o => new
                {
                    Id = o.Id,
                    Subject = $"{o.CodiceArticolo} Qta: {o.Quantita}",
                    Description = o.DescrOrdine,
                    StartTime = o.DataInizioOP,
                    EndTime = o.IdStato == 3 ? o.DataFineOP : o.DataFinePrevista,
                    RoomId = o.IdCentroLavoro,
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
                    IdCentroLavoro = o.IdCentroLavoro,
                    Priorita = o.Priorita,
                    PercentualeCompletamento = o.Quantita > 0 ? Math.Round((o.QuantitaProdotta / o.Quantita) * 100, 2) : 0,
                    TipoOrdine = o.TipoOrdine,
                    AnnoOrdine = o.AnnoOrdine,
                    SerieOrdine = o.SerieOrdine,
                    NumeroOrdine = o.NumeroOrdine,
                    Note = o.Note,
                    TempoCiclo = o.TempoCiclo,
                    TempoSetup = o.TempoSetup
                }).ToList();

                _logger.LogInformation($"Primi 3 ordini: {string.Join(", ", ordini.Take(3).Select(o => $"Id:{o.Id}, RoomId:{o.RoomId}, Subject:{o.Subject}"))}");
                _logger.LogInformation($"Caricati {ordini.Count} ordini di produzione");
                return Json(ordini);
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

                // Aggiorna le date
                ordine.DataInizioOP = request.StartTime;
                
                // Aggiorna la data fine in base allo stato
                if (ordine.IdStato == 3) // Chiuso
                {
                    ordine.DataFineOP = request.EndTime;
                }
                else
                {
                    ordine.DataFinePrevista = request.EndTime;
                }

                // Aggiorna il centro di lavoro se cambiato
                if (request.RoomId.HasValue)
                {
                    ordine.IdCentroLavoro = request.RoomId.Value;
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation($"Ordine {ordine.IdListaOP} aggiornato: nuovo centro {ordine.IdCentroLavoro}, " +
                                     $"inizio {ordine.DataInizioOP:yyyy-MM-dd HH:mm}, " +
                                     $"fine {(ordine.IdStato == 3 ? ordine.DataFineOP : ordine.DataFinePrevista):yyyy-MM-dd HH:mm}");

                return Ok(new { success = true, message = "Ordine aggiornato con successo" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Errore nell'aggiornamento dell'ordine {request.Id}");
                return StatusCode(500, "Errore nell'aggiornamento dell'ordine");
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
        private static string GetCentroLavoroColor(int idCentroLavoro)
        {
            var colors = new[] { "#E3F2FD", "#F3E5F5", "#E8F5E8", "#FFF3E0", "#FCE4EC", "#F1F8E9" };
            return colors[idCentroLavoro % colors.Length];
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

                // Aggiorna i campi modificabili
                ordine.Quantita = request.Quantita;
                ordine.QuantitaProdotta = request.QuantitaProdotta;
                ordine.IdStato = request.IdStato;
                ordine.IdCentroLavoro = request.IdCentroLavoro;
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
        public int? RoomId { get; set; }
    }

    /// <summary>
    /// Modello per la richiesta di salvataggio dettagli ordine
    /// </summary>
    public class SaveOrderDetailsRequest
    {
        public int Id { get; set; }
        public decimal Quantita { get; set; }
        public decimal QuantitaProdotta { get; set; }
        public int IdStato { get; set; }
        public int IdCentroLavoro { get; set; }
        public string Note { get; set; } = string.Empty;
    }
}
