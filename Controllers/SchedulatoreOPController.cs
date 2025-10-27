using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AiDbMaster.Data;
using AiDbMaster.Models;

namespace AiDbMaster.Controllers
{
    [Authorize]
    public class SchedulatoreOPController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<SchedulatoreOPController> _logger;

        public SchedulatoreOPController(
            ApplicationDbContext context,
            ILogger<SchedulatoreOPController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Pagina principale Schedulatore Ordini di Produzione
        /// </summary>
        public IActionResult Index()
        {
            ViewBag.Title = "Schedulatore Ordini di Produzione";
            ViewBag.UseFluidContainer = true;
            return View();
        }

        /// <summary>
        /// API: Ottiene i centri di lavoro per le risorse del timeline
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetCentriLavoro()
        {
            try
            {
                _logger.LogInformation("Caricamento centri di lavoro per SchedulatoreOP");

                var centri = await _context.CentriLavoro
                    .Where(c => c.Attivo)
                    .OrderBy(c => c.DescrizioneCentro)
                    .Select(c => new
                    {
                        Id = c.CodiceCentro,
                        Name = c.DescrizioneCentro,
                        Capacity = c.CapacitaOraria ?? 1
                    })
                    .ToListAsync();

                _logger.LogInformation($"Caricati {centri.Count} centri di lavoro");
                return Json(centri);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore nel recupero dei centri di lavoro");
                return StatusCode(500, new { error = true, message = ex.Message });
            }
        }

        /// <summary>
        /// API: Ottiene gli ordini di produzione per il calendario
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetOrdiniProduzione(
            DateTime? dataInizio = null,
            DateTime? dataFine = null)
        {
            try
            {
                // Range default: 2 mesi (1 mese prima e 1 mese dopo oggi)
                var start = dataInizio ?? DateTime.Today.AddMonths(-1);
                var end = dataFine ?? DateTime.Today.AddMonths(1);

                _logger.LogInformation($"Caricamento ordini produzione: {start:yyyy-MM-dd} - {end:yyyy-MM-dd}");

                // Prima conta TUTTI gli ordini
                var totaleOrdini = await _context.ListaOP.CountAsync();
                _logger.LogInformation($"Totale ordini in ListaOP: {totaleOrdini}");

                // Poi filtra per date
                var ordini = await _context.ListaOP
                    .Include(o => o.Stato)
                    .Include(o => o.CentroLavoro)
                    .Where(o => o.DataInizioOP >= start && o.DataInizioOP <= end)
                    .ToListAsync();
                
                _logger.LogInformation($"Ordini nel range {start:yyyy-MM-dd} - {end:yyyy-MM-dd}: {ordini.Count}");
                
                if (ordini.Count == 0)
                {
                    // Log delle date effettive degli ordini per debug
                    var dateOrdini = await _context.ListaOP
                        .OrderBy(o => o.DataInizioOP)
                        .Select(o => o.DataInizioOP)
                        .Take(5)
                        .ToListAsync();
                    
                    _logger.LogWarning($"Nessun ordine nel range. Prime 5 date in DB: {string.Join(", ", dateOrdini.Select(d => d.ToString("yyyy-MM-dd")))}");
                }

                // Mappa gli ordini in eventi per Syncfusion
                var eventi = ordini.Select(o =>
                {
                    // Calcolo EndTime con protezione da valori negativi
                    var durataSecondi = (double)(o.Quantita * (decimal)o.TempoCiclo);
                    var endTime = durataSecondi > 0 
                        ? o.DataInizioOP.AddSeconds(durataSecondi)
                        : o.DataInizioOP.AddHours(1); // Default 1 ora se calcolo non valido
                    
                    return new
                    {
                        // Dati per Syncfusion Schedule
                        Id = o.IdListaOP,
                        Subject = $"Ord. {o.AnnoOrdine}-{o.NumeroOrdine} Qta: {Math.Floor(o.Quantita)}",
                        StartTime = o.DataInizioOP,
                        EndTime = endTime,
                        RoomId = o.CodiceCentro, // Per associare alla risorsa (centro di lavoro)
                        // Colore basato su IdStato
                        CategoryColor = GetColorByStato(o.IdStato),
                        IsAllDay = false,
                    
                    // TUTTI i campi di ListaOP per il popup
                    TipoOrdine = o.TipoOrdine,
                    AnnoOrdine = o.AnnoOrdine,
                    SerieOrdine = o.SerieOrdine,
                    NumeroOrdine = o.NumeroOrdine,
                    RigaOrdine = o.RigaOrdine,
                    DescrOrdine = o.DescrOrdine,
                    CodiceArticolo = o.CodiceArticolo,
                    DescrizioneArticolo = o.DescrizioneArticolo,
                    UnitaMisura = o.UnitaMisura,
                    Quantita = o.Quantita,
                    QuantitaProdotta = o.QuantitaProdotta,
                    DataInizioOP = o.DataInizioOP,
                    TempoCiclo = o.TempoCiclo,
                    DataInizioSetup = o.DataInizioSetup,
                    TempoSetup = o.TempoSetup,
                    IdStato = o.IdStato,
                    StatoDescrizione = o.Stato?.DescrizioneStato ?? "",
                    CodiceCentro = o.CodiceCentro,
                    CentroLavoroDescrizione = o.CentroLavoro?.DescrizioneCentro ?? "",
                    CodiceLavorazione = o.CodiceLavorazione,
                    Note = o.Note,
                    DataFineOP = o.DataFineOP,
                    DataFinePrevista = o.DataFinePrevista,
                    Priorita = o.Priorita,
                    IdOperatore = o.IdOperatore,
                    CostoOrario = o.CostoOrario,
                    TempoEffettivo = o.TempoEffettivo,
                    Modificato = o.Modificato,
                    PercentualeCompletamento = o.Quantita > 0 ? Math.Round((o.QuantitaProdotta / o.Quantita) * 100, 2) : 0
                    };
                }).ToList();

                _logger.LogInformation($"✅ Restituiti {eventi.Count} ordini di produzione per il calendario");
                
                // Log dei primi 3 eventi per debug
                if (eventi.Count > 0)
                {
                    _logger.LogInformation($"Primi eventi: {string.Join(", ", eventi.Take(3).Select(e => $"{e.Subject} [{e.StartTime:yyyy-MM-dd HH:mm} - {e.EndTime:yyyy-MM-dd HH:mm}]"))}");
                }
                
                return Json(eventi);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore nel recupero degli ordini di produzione");
                return StatusCode(500, new { error = true, message = ex.Message });
            }
        }

        /// <summary>
        /// Ottiene il colore in base allo stato dell'ordine
        /// </summary>
        private static string GetColorByStato(int idStato)
        {
            return idStato switch
            {
                1 => "#FFA500", // Arancione
                2 => "#1E90FF", // Blu
                3 => "#9370DB", // Viola
                4 => "#32CD32", // Verde
                _ => "#808080"  // Grigio (default)
            };
        }
    }
}

