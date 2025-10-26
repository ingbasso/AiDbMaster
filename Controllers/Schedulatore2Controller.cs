using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using AiDbMaster.Data;
using AiDbMaster.Models;

namespace AiDbMaster.Controllers
{
    /// <summary>
    /// Controller per lo Schedulatore2 - visualizza ordini di produzione da ListaOP
    /// con avanzamento produzione e colori per stati
    /// </summary>
    [Authorize]
    public class Schedulatore2Controller : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<Schedulatore2Controller> _logger;

        public Schedulatore2Controller(ApplicationDbContext context, ILogger<Schedulatore2Controller> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Pagina principale Schedulatore2
        /// </summary>
        public IActionResult Index()
        {
            ViewBag.Title = "Schedulatore Ordini di Produzione";
            return View();
        }

        /// <summary>
        /// API per ottenere i centri di lavoro (rooms per lo scheduler)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetCentriLavoro()
        {
            try
            {
                _logger.LogInformation("Caricamento centri di lavoro per Schedulatore2");
                
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

                var centriLavoro = centriFromDb.Select(c => new
                {
                    Id = c.Id,
                    Name = c.Name,
                    Color = GetCentroLavoroColor(c.Id),
                    Capacity = c.Capacity
                }).ToList();

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
        /// API per ottenere gli ordini di produzione da ListaOP
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetOrdiniProduzione(DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                _logger.LogInformation("Caricamento ordini di produzione da ListaOP");
                
                // Range di default (30 giorni prima e dopo oggi)
                var start = startDate ?? DateTime.Today.AddDays(-30);
                var end = endDate ?? DateTime.Today.AddDays(90);

                _logger.LogInformation($"Range date: {start:yyyy-MM-dd} - {end:yyyy-MM-dd}");

                // Recupera gli ordini dal database con Include per le relazioni
                var ordiniFromDb = await _context.ListaOP
                    .Include(o => o.Stato)
                    .Include(o => o.CentroLavoro)
                    .Where(o => o.DataInizioOP >= start && o.DataInizioOP <= end)
                    .ToListAsync();

                // Trasforma in formato per il calendario
                var ordiniList = ordiniFromDb.Select(o => new
                {
                    Id = o.IdListaOP,
                    // Subject con formato: "Ord. T-2025-OP-000001 Qta: 150"
                    Subject = $"Ord. {o.TipoOrdine}-{o.AnnoOrdine}-{o.SerieOrdine}-{o.NumeroOrdine:D6} Qta: {Math.Floor(o.Quantita)}",
                    Description = o.DescrOrdine ?? "",
                    StartTime = o.DataInizioOP,
                    // EndTime = DataInizioOP + (Quantita * TempoCiclo) secondi
                    EndTime = o.DataInizioOP.AddSeconds((double)(o.Quantita * (decimal)o.TempoCiclo)),
                    RoomId = o.CodiceCentro,
                    // Colore in base allo stato
                    CategoryColor = GetStatoColor(o.IdStato),
                    IsAllDay = false,
                    RecurrenceRule = "",
                    // Dati aggiuntivi
                    CodiceArticolo = o.CodiceArticolo,
                    DescrizioneArticolo = o.DescrizioneArticolo,
                    Quantita = o.Quantita,
                    QuantitaProdotta = o.QuantitaProdotta,
                    IdStato = o.IdStato,
                    StatoDescrizione = o.Stato?.DescrizioneStato ?? "",
                    CentroLavoro = o.CentroLavoro?.DescrizioneCentro ?? "",
                    CodiceCentro = o.CodiceCentro,
                    Priorita = o.Priorita ?? 2,
                    // Percentuale di completamento per la barra progressiva
                    PercentualeCompletamento = o.Quantita > 0 ? Math.Round((o.QuantitaProdotta / o.Quantita) * 100, 2) : 0,
                    TipoOrdine = o.TipoOrdine,
                    AnnoOrdine = o.AnnoOrdine,
                    SerieOrdine = o.SerieOrdine,
                    NumeroOrdine = o.NumeroOrdine,
                    Note = o.Note ?? "",
                    TempoCiclo = o.TempoCiclo
                }).ToList();

                _logger.LogInformation($"Caricati {ordiniList.Count} ordini di produzione");
                return Json(ordiniList);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore nel recupero degli ordini di produzione");
                return Json(new { error = true, message = ex.Message });
            }
        }

        /// <summary>
        /// API per aggiornare un ordine (drag & drop o resize)
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

                if (request.IsResize)
                {
                    // RESIZE: Ricalcola la quantità in base alla nuova durata
                    var durataSecondi = (decimal)(request.EndTime - request.StartTime).TotalSeconds;
                    
                    if (ordine.TempoCiclo > 0)
                    {
                        ordine.Quantita = Math.Round(durataSecondi / (decimal)ordine.TempoCiclo, 3);
                        _logger.LogInformation($"Ordine {ordine.IdListaOP} RESIZE: nuova Quantità={ordine.Quantita}");
                    }
                }

                // Imposta il flag Modificato
                ordine.Modificato = true;

                await _context.SaveChangesAsync();

                _logger.LogInformation($"Ordine {ordine.IdListaOP} aggiornato con successo");

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
        /// API per ottenere gli stati ordini
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetStatiOrdini()
        {
            try
            {
                var stati = await _context.StatiOP
                    .Where(s => s.Attivo)
                    .OrderBy(s => s.Ordine)
                    .Select(s => new
                    {
                        Id = s.IdStato,
                        Codice = s.CodiceStato,
                        Descrizione = s.DescrizioneStato,
                        Colore = GetStatoColor(s.IdStato)
                    })
                    .ToListAsync();

                return Json(stati);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore nel recupero stati ordini");
                return Json(new { error = true, message = ex.Message });
            }
        }

        /// <summary>
        /// Ottiene il colore per uno stato specifico secondo le specifiche:
        /// 1=Arancio, 2=Blu chiaro, 3=Viola, 4=Verde
        /// </summary>
        private static string GetStatoColor(int idStato)
        {
            return idStato switch
            {
                1 => "#FFA500", // Stato 1 - Arancio
                2 => "#87CEEB", // Stato 2 - Blu chiaro
                3 => "#9370DB", // Stato 3 - Viola
                4 => "#32CD32", // Stato 4 - Verde
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
    }
}

