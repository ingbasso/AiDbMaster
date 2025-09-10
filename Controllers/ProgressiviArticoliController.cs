using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using AiDbMaster.Data;
using AiDbMaster.Models;

namespace AiDbMaster.Controllers
{
    /// <summary>
    /// Controller per la gestione dei Progressivi Articoli
    /// Fornisce funzionalità di visualizzazione delle giacenze e movimenti degli articoli per magazzino
    /// </summary>
    [Authorize]
    public class ProgressiviArticoliController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ProgressiviArticoliController> _logger;

        public ProgressiviArticoliController(
            ApplicationDbContext context,
            ILogger<ProgressiviArticoliController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Visualizza l'elenco di tutti i progressivi articoli
        /// GET: ProgressiviArticoli
        /// </summary>
        /// <param name="search">Termine di ricerca per filtrare i progressivi</param>
        /// <param name="codiceArticolo">Filtro per codice articolo</param>
        /// <param name="codiceMagazzino">Filtro per codice magazzino</param>
        /// <param name="statoGiacenza">Filtro per stato giacenza</param>
        /// <param name="soloConMovimenti">Mostra solo articoli con movimenti</param>
        /// <param name="sortOrder">Ordinamento dei risultati</param>
        /// <param name="page">Numero di pagina per la paginazione</param>
        /// <param name="pageSize">Numero di elementi per pagina</param>
        /// <returns>Vista con l'elenco dei progressivi articoli</returns>
        public async Task<IActionResult> Index(
            string? search,
            string? codiceArticolo,
            short? codiceMagazzino,
            string? statoGiacenza,
            bool soloConMovimenti = false,
            string sortOrder = "articolo",
            int page = 1,
            int pageSize = 50)
        {
            try
            {
                _logger.LogInformation("Caricamento progressivi articoli - Pagina: {Page}, Ricerca: {Search}", page, search);

                // Query base
                var query = _context.ProgressiviArticoli.AsQueryable();

                // Filtro per ricerca testuale
                if (!string.IsNullOrEmpty(search))
                {
                    query = query.Where(p => 
                        p.CodiceArticolo.Contains(search) ||
                        p.CodiceMagazzino.ToString().Contains(search));
                }

                // Filtro per codice articolo
                if (!string.IsNullOrEmpty(codiceArticolo))
                {
                    query = query.Where(p => p.CodiceArticolo.Contains(codiceArticolo));
                }

                // Filtro per codice magazzino
                if (codiceMagazzino.HasValue)
                {
                    query = query.Where(p => p.CodiceMagazzino == codiceMagazzino.Value);
                }

                // Filtro per stato giacenza
                if (!string.IsNullOrEmpty(statoGiacenza))
                {
                    query = statoGiacenza switch
                    {
                        "Esaurito" => query.Where(p => p.Esistenza <= 0),
                        "Non Disponibile" => query.Where(p => p.Esistenza > 0 && (p.Esistenza - p.Impegnato - p.Prenotato) <= 0),
                        "Scorta Bassa" => query.Where(p => p.Esistenza > 0 && (p.Esistenza - p.Impegnato - p.Prenotato) > 0 && (p.Esistenza - p.Impegnato - p.Prenotato) < (p.Esistenza * 0.2m)),
                        "Disponibile" => query.Where(p => (p.Esistenza - p.Impegnato - p.Prenotato) >= (p.Esistenza * 0.2m)),
                        _ => query
                    };
                }

                // Filtro solo con movimenti
                if (soloConMovimenti)
                {
                    query = query.Where(p => p.Ordinato > 0 || p.Impegnato > 0 || p.Prenotato > 0);
                }

                // Ordinamento
                query = sortOrder switch
                {
                    "articolo_desc" => query.OrderByDescending(p => p.CodiceArticolo),
                    "magazzino" => query.OrderBy(p => p.CodiceMagazzino),
                    "magazzino_desc" => query.OrderByDescending(p => p.CodiceMagazzino),
                    "esistenza" => query.OrderBy(p => p.Esistenza),
                    "esistenza_desc" => query.OrderByDescending(p => p.Esistenza),
                    "disponibile" => query.OrderBy(p => p.Esistenza - p.Impegnato - p.Prenotato),
                    "disponibile_desc" => query.OrderByDescending(p => p.Esistenza - p.Impegnato - p.Prenotato),
                    "ordinato" => query.OrderBy(p => p.Ordinato),
                    "ordinato_desc" => query.OrderByDescending(p => p.Ordinato),
                    "impegnato" => query.OrderBy(p => p.Impegnato),
                    "impegnato_desc" => query.OrderByDescending(p => p.Impegnato),
                    _ => query.OrderBy(p => p.CodiceArticolo).ThenBy(p => p.CodiceMagazzino)
                };

                // Conteggio totale per la paginazione
                var totalCount = await query.CountAsync();

                // Paginazione
                var progressiviArticoli = await query
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                // Preparazione dei dati per la vista
                ViewBag.CurrentSort = sortOrder;
                ViewBag.CurrentSearch = search;
                ViewBag.CurrentCodiceArticolo = codiceArticolo;
                ViewBag.CurrentCodiceMagazzino = codiceMagazzino;
                ViewBag.CurrentStatoGiacenza = statoGiacenza;
                ViewBag.CurrentSoloConMovimenti = soloConMovimenti;
                ViewBag.CurrentPage = page;
                ViewBag.PageSize = pageSize;
                ViewBag.TotalCount = totalCount;
                ViewBag.TotalPages = (int)Math.Ceiling((double)totalCount / pageSize);

                // Dati per i filtri dropdown
                ViewBag.CodiciArticolo = await _context.ProgressiviArticoli
                    .Select(p => p.CodiceArticolo)
                    .Distinct()
                    .OrderBy(c => c)
                    .Take(100) // Limito per performance
                    .ToListAsync();

                ViewBag.CodiciMagazzino = await _context.ProgressiviArticoli
                    .Select(p => p.CodiceMagazzino)
                    .Distinct()
                    .OrderBy(c => c)
                    .ToListAsync();

                ViewBag.StatiGiacenza = new List<string> { "Esaurito", "Non Disponibile", "Scorta Bassa", "Disponibile" };

                // Statistiche aggregate
                var statistiche = await query.GroupBy(p => 1).Select(g => new
                {
                    TotaleEsistenza = g.Sum(p => p.Esistenza),
                    TotaleOrdinato = g.Sum(p => p.Ordinato),
                    TotaleImpegnato = g.Sum(p => p.Impegnato),
                    TotalePrenotato = g.Sum(p => p.Prenotato),
                    TotaleDisponibile = g.Sum(p => p.Esistenza - p.Impegnato - p.Prenotato),
                    TotaleMagazzini = g.Select(p => p.CodiceMagazzino).Distinct().Count(),
                    TotaleArticoli = g.Select(p => p.CodiceArticolo).Distinct().Count()
                }).FirstOrDefaultAsync();

                ViewBag.Statistiche = statistiche ?? new
                {
                    TotaleEsistenza = 0m,
                    TotaleOrdinato = 0m,
                    TotaleImpegnato = 0m,
                    TotalePrenotato = 0m,
                    TotaleDisponibile = 0m,
                    TotaleMagazzini = 0,
                    TotaleArticoli = 0
                };

                _logger.LogInformation("Caricati {Count} progressivi articoli su {Total} totali", progressiviArticoli.Count, totalCount);

                return View(progressiviArticoli);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante il caricamento dei progressivi articoli");
                TempData["ErrorMessage"] = "Si è verificato un errore durante il caricamento dei progressivi articoli.";
                return View(new List<ProgressiviArticoli>());
            }
        }

        /// <summary>
        /// Visualizza i dettagli di un progressivo articolo specifico
        /// GET: ProgressiviArticoli/Details/5
        /// </summary>
        /// <param name="id">ID del progressivo articolo da visualizzare</param>
        /// <returns>Vista con i dettagli del progressivo articolo</returns>
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                _logger.LogWarning("Tentativo di accesso ai dettagli progressivo articolo senza ID");
                return NotFound();
            }

            try
            {
                var progressivoArticolo = await _context.ProgressiviArticoli
                    .FirstOrDefaultAsync(p => p.Id == id);

                if (progressivoArticolo == null)
                {
                    _logger.LogWarning("Progressivo articolo con ID {Id} non trovato", id);
                    return NotFound();
                }

                // Cerco altri progressivi dello stesso articolo in magazzini diversi
                ViewBag.AltriMagazzini = await _context.ProgressiviArticoli
                    .Where(p => p.CodiceArticolo == progressivoArticolo.CodiceArticolo && 
                               p.CodiceMagazzino != progressivoArticolo.CodiceMagazzino)
                    .OrderBy(p => p.CodiceMagazzino)
                    .ToListAsync();

                // Cerco altri articoli nello stesso magazzino
                ViewBag.AltriArticoliMagazzino = await _context.ProgressiviArticoli
                    .Where(p => p.CodiceMagazzino == progressivoArticolo.CodiceMagazzino && 
                               p.CodiceArticolo != progressivoArticolo.CodiceArticolo)
                    .OrderBy(p => p.CodiceArticolo)
                    .Take(10) // Limito per performance
                    .ToListAsync();

                // Statistiche aggregate per l'articolo
                var statisticheArticolo = await _context.ProgressiviArticoli
                    .Where(p => p.CodiceArticolo == progressivoArticolo.CodiceArticolo)
                    .GroupBy(p => 1)
                    .Select(g => new
                    {
                        TotaleEsistenza = g.Sum(p => p.Esistenza),
                        TotaleOrdinato = g.Sum(p => p.Ordinato),
                        TotaleImpegnato = g.Sum(p => p.Impegnato),
                        TotalePrenotato = g.Sum(p => p.Prenotato),
                        TotaleDisponibile = g.Sum(p => p.Esistenza - p.Impegnato - p.Prenotato),
                        NumeroMagazzini = g.Count()
                    })
                    .FirstOrDefaultAsync();

                ViewBag.StatisticheArticolo = statisticheArticolo;

                _logger.LogInformation("Visualizzazione dettagli progressivo articolo: {CodiceArticolo} - Magazzino: {CodiceMagazzino}", 
                    progressivoArticolo.CodiceArticolo, progressivoArticolo.CodiceMagazzino);

                return View(progressivoArticolo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante il caricamento dei dettagli del progressivo articolo {Id}", id);
                TempData["ErrorMessage"] = "Si è verificato un errore durante il caricamento dei dettagli del progressivo articolo.";
                return RedirectToAction(nameof(Index));
            }
        }

        /// <summary>
        /// API per ottenere i dati dei progressivi articoli in formato JSON
        /// GET: ProgressiviArticoli/GetProgressiviArticoliJson
        /// </summary>
        /// <returns>Dati dei progressivi articoli in formato JSON</returns>
        [HttpGet]
        public async Task<IActionResult> GetProgressiviArticoliJson()
        {
            try
            {
                var progressiviArticoli = await _context.ProgressiviArticoli
                    .OrderBy(p => p.CodiceArticolo)
                    .ThenBy(p => p.CodiceMagazzino)
                    .Select(p => new
                    {
                        p.Id,
                        p.CodiceArticolo,
                        p.CodiceMagazzino,
                        p.Esistenza,
                        p.Ordinato,
                        p.Impegnato,
                        p.Prenotato,
                        Disponibile = p.Disponibile,
                        TotalePrevisto = p.TotalePrevisto,
                        PercentualeImpegno = p.PercentualeImpegno,
                        StatoGiacenza = p.StatoGiacenza,
                        DescrizioneCompleta = p.DescrizioneCompleta,
                        RiepilogoQuantita = p.RiepilogoQuantita,
                        HasMovimenti = p.HasMovimenti
                    })
                    .ToListAsync();

                return Json(new { data = progressiviArticoli });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante il caricamento dei dati JSON dei progressivi articoli");
                return Json(new { error = "Errore durante il caricamento dei dati" });
            }
        }

        /// <summary>
        /// API per ottenere statistiche aggregate per dashboard
        /// GET: ProgressiviArticoli/GetStatistiche
        /// </summary>
        /// <returns>Statistiche aggregate in formato JSON</returns>
        [HttpGet]
        public async Task<IActionResult> GetStatistiche()
        {
            try
            {
                var statistiche = await _context.ProgressiviArticoli
                    .GroupBy(p => 1)
                    .Select(g => new
                    {
                        TotaleRecords = g.Count(),
                        TotaleArticoli = g.Select(p => p.CodiceArticolo).Distinct().Count(),
                        TotaleMagazzini = g.Select(p => p.CodiceMagazzino).Distinct().Count(),
                        TotaleEsistenza = g.Sum(p => p.Esistenza),
                        TotaleOrdinato = g.Sum(p => p.Ordinato),
                        TotaleImpegnato = g.Sum(p => p.Impegnato),
                        TotalePrenotato = g.Sum(p => p.Prenotato),
                        TotaleDisponibile = g.Sum(p => p.Esistenza - p.Impegnato - p.Prenotato),
                        ArticoliEsauriti = g.Count(p => p.Esistenza <= 0),
                        ArticoliNonDisponibili = g.Count(p => p.Esistenza > 0 && (p.Esistenza - p.Impegnato - p.Prenotato) <= 0),
                        ArticoliScortaBassa = g.Count(p => p.Esistenza > 0 && (p.Esistenza - p.Impegnato - p.Prenotato) > 0 && (p.Esistenza - p.Impegnato - p.Prenotato) < (p.Esistenza * 0.2m)),
                        ArticoliConMovimenti = g.Count(p => p.Ordinato > 0 || p.Impegnato > 0 || p.Prenotato > 0)
                    })
                    .FirstOrDefaultAsync();

                return Json(statistiche);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante il caricamento delle statistiche dei progressivi articoli");
                return Json(new { error = "Errore durante il caricamento delle statistiche" });
            }
        }
    }
}
