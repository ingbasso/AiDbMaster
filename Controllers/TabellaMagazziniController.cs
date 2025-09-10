using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using AiDbMaster.Data;
using AiDbMaster.Models;

namespace AiDbMaster.Controllers
{
    /// <summary>
    /// Controller per la gestione della Tabella Magazzini
    /// Fornisce funzionalità di visualizzazione dei magazzini configurati nel sistema
    /// </summary>
    [Authorize]
    public class TabellaMagazziniController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<TabellaMagazziniController> _logger;

        public TabellaMagazziniController(
            ApplicationDbContext context,
            ILogger<TabellaMagazziniController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Visualizza l'elenco di tutti i magazzini
        /// GET: TabellaMagazzini
        /// </summary>
        /// <param name="search">Termine di ricerca per filtrare i magazzini</param>
        /// <param name="hasDescrizione">Filtro per magazzini con/senza descrizione</param>
        /// <param name="sortOrder">Ordinamento dei risultati</param>
        /// <param name="page">Numero di pagina per la paginazione</param>
        /// <param name="pageSize">Numero di elementi per pagina</param>
        /// <returns>Vista con l'elenco dei magazzini</returns>
        public async Task<IActionResult> Index(
            string? search,
            bool? hasDescrizione,
            string sortOrder = "codice",
            int page = 1,
            int pageSize = 50)
        {
            try
            {
                _logger.LogInformation("Caricamento tabella magazzini - Pagina: {Page}, Ricerca: {Search}", page, search);

                // Query base
                var query = _context.TabellaMagazzini.AsQueryable();

                // Filtro per ricerca testuale
                if (!string.IsNullOrEmpty(search))
                {
                    query = query.Where(m => 
                        m.CodiceMagazzino.ToString().Contains(search) ||
                        (m.DescrizioneMagazzino != null && m.DescrizioneMagazzino.Contains(search)));
                }

                // Filtro per presenza descrizione
                if (hasDescrizione.HasValue)
                {
                    if (hasDescrizione.Value)
                    {
                        query = query.Where(m => !string.IsNullOrEmpty(m.DescrizioneMagazzino));
                    }
                    else
                    {
                        query = query.Where(m => string.IsNullOrEmpty(m.DescrizioneMagazzino));
                    }
                }

                // Ordinamento
                query = sortOrder switch
                {
                    "codice_desc" => query.OrderByDescending(m => m.CodiceMagazzino),
                    "descrizione" => query.OrderBy(m => m.DescrizioneMagazzino),
                    "descrizione_desc" => query.OrderByDescending(m => m.DescrizioneMagazzino),
                    "id" => query.OrderBy(m => m.Id),
                    "id_desc" => query.OrderByDescending(m => m.Id),
                    _ => query.OrderBy(m => m.CodiceMagazzino)
                };

                // Conteggio totale per la paginazione
                var totalCount = await query.CountAsync();

                // Paginazione
                var magazzini = await query
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                // Preparazione dei dati per la vista
                ViewBag.CurrentSort = sortOrder;
                ViewBag.CurrentSearch = search;
                ViewBag.CurrentHasDescrizione = hasDescrizione;
                ViewBag.CurrentPage = page;
                ViewBag.PageSize = pageSize;
                ViewBag.TotalCount = totalCount;
                ViewBag.TotalPages = (int)Math.Ceiling((double)totalCount / pageSize);

                // Statistiche aggiuntive
                ViewBag.TotaleConDescrizione = await _context.TabellaMagazzini
                    .CountAsync(m => !string.IsNullOrEmpty(m.DescrizioneMagazzino));

                ViewBag.TotaleSenzaDescrizione = await _context.TabellaMagazzini
                    .CountAsync(m => string.IsNullOrEmpty(m.DescrizioneMagazzino));

                ViewBag.CodiceMinimo = await _context.TabellaMagazzini
                    .MinAsync(m => (short?)m.CodiceMagazzino) ?? 0;

                ViewBag.CodiceMassimo = await _context.TabellaMagazzini
                    .MaxAsync(m => (short?)m.CodiceMagazzino) ?? 0;

                // Verifica se ci sono progressivi articoli collegati
                var magazziniConProgressivi = await _context.ProgressiviArticoli
                    .Select(p => p.CodiceMagazzino)
                    .Distinct()
                    .CountAsync();

                ViewBag.MagazziniConProgressivi = magazziniConProgressivi;

                _logger.LogInformation("Caricati {Count} magazzini su {Total} totali", magazzini.Count, totalCount);

                return View(magazzini);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante il caricamento della tabella magazzini");
                TempData["ErrorMessage"] = "Si è verificato un errore durante il caricamento dei magazzini.";
                return View(new List<TabellaMagazzini>());
            }
        }

        /// <summary>
        /// Visualizza i dettagli di un magazzino specifico
        /// GET: TabellaMagazzini/Details/5
        /// </summary>
        /// <param name="id">ID del magazzino da visualizzare</param>
        /// <returns>Vista con i dettagli del magazzino</returns>
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                _logger.LogWarning("Tentativo di accesso ai dettagli magazzino senza ID");
                return NotFound();
            }

            try
            {
                var magazzino = await _context.TabellaMagazzini
                    .FirstOrDefaultAsync(m => m.Id == id);

                if (magazzino == null)
                {
                    _logger.LogWarning("Magazzino con ID {Id} non trovato", id);
                    return NotFound();
                }

                // Cerco altri magazzini per confronto
                ViewBag.AltriMagazzini = await _context.TabellaMagazzini
                    .Where(m => m.Id != magazzino.Id)
                    .OrderBy(m => m.CodiceMagazzino)
                    .ToListAsync();

                // Statistiche sui progressivi articoli per questo magazzino
                var statisticheProgressivi = await _context.ProgressiviArticoli
                    .Where(p => p.CodiceMagazzino == magazzino.CodiceMagazzino)
                    .GroupBy(p => 1)
                    .Select(g => new
                    {
                        TotaleArticoli = g.Count(),
                        TotaleEsistenza = g.Sum(p => p.Esistenza),
                        TotaleDisponibile = g.Sum(p => p.Esistenza - p.Impegnato - p.Prenotato),
                        TotaleImpegnato = g.Sum(p => p.Impegnato),
                        TotaleOrdinato = g.Sum(p => p.Ordinato),
                        ArticoliEsauriti = g.Count(p => p.Esistenza <= 0),
                        ArticoliDisponibili = g.Count(p => (p.Esistenza - p.Impegnato - p.Prenotato) > 0)
                    })
                    .FirstOrDefaultAsync();

                ViewBag.StatisticheProgressivi = statisticheProgressivi;

                // Articoli con maggiore esistenza in questo magazzino
                ViewBag.TopArticoli = await _context.ProgressiviArticoli
                    .Where(p => p.CodiceMagazzino == magazzino.CodiceMagazzino)
                    .OrderByDescending(p => p.Esistenza)
                    .Take(10)
                    .ToListAsync();

                // Articoli esauriti in questo magazzino
                ViewBag.ArticoliEsauriti = await _context.ProgressiviArticoli
                    .Where(p => p.CodiceMagazzino == magazzino.CodiceMagazzino && p.Esistenza <= 0)
                    .OrderBy(p => p.CodiceArticolo)
                    .Take(10)
                    .ToListAsync();

                _logger.LogInformation("Visualizzazione dettagli magazzino: {CodiceMagazzino} - {DescrizioneMagazzino}", 
                    magazzino.CodiceMagazzino, magazzino.DescrizioneMagazzino);

                return View(magazzino);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante il caricamento dei dettagli del magazzino {Id}", id);
                TempData["ErrorMessage"] = "Si è verificato un errore durante il caricamento dei dettagli del magazzino.";
                return RedirectToAction(nameof(Index));
            }
        }

        /// <summary>
        /// API per ottenere i dati dei magazzini in formato JSON
        /// GET: TabellaMagazzini/GetMagazziniJson
        /// </summary>
        /// <returns>Dati dei magazzini in formato JSON</returns>
        [HttpGet]
        public async Task<IActionResult> GetMagazziniJson()
        {
            try
            {
                var magazzini = await _context.TabellaMagazzini
                    .OrderBy(m => m.CodiceMagazzino)
                    .Select(m => new
                    {
                        m.Id,
                        m.CodiceMagazzino,
                        m.DescrizioneMagazzino,
                        NomeCompleto = m.NomeCompleto,
                        DescrizioneCompleta = m.DescrizioneCompleta,
                        HasDescrizione = m.HasDescrizione,
                        BadgeText = m.BadgeText,
                        DescrizioneBreve = m.DescrizioneBreve,
                        InfoExport = m.InfoExport
                    })
                    .ToListAsync();

                return Json(new { data = magazzini });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante il caricamento dei dati JSON dei magazzini");
                return Json(new { error = "Errore durante il caricamento dei dati" });
            }
        }

        /// <summary>
        /// API per ottenere statistiche aggregate dei magazzini
        /// GET: TabellaMagazzini/GetStatistiche
        /// </summary>
        /// <returns>Statistiche dei magazzini in formato JSON</returns>
        [HttpGet]
        public async Task<IActionResult> GetStatistiche()
        {
            try
            {
                var statistiche = await _context.TabellaMagazzini
                    .GroupBy(m => 1)
                    .Select(g => new
                    {
                        TotaleMagazzini = g.Count(),
                        MagazziniConDescrizione = g.Count(m => !string.IsNullOrEmpty(m.DescrizioneMagazzino)),
                        MagazziniSenzaDescrizione = g.Count(m => string.IsNullOrEmpty(m.DescrizioneMagazzino)),
                        CodiceMinimo = g.Min(m => m.CodiceMagazzino),
                        CodiceMassimo = g.Max(m => m.CodiceMagazzino),
                        PercentualeConDescrizione = g.Count() > 0 ? Math.Round((double)g.Count(m => !string.IsNullOrEmpty(m.DescrizioneMagazzino)) / g.Count() * 100, 1) : 0
                    })
                    .FirstOrDefaultAsync();

                // Aggiungo statistiche sui progressivi articoli
                var statisticheProgressivi = await _context.ProgressiviArticoli
                    .GroupBy(p => 1)
                    .Select(g => new
                    {
                        MagazziniConArticoli = g.Select(p => p.CodiceMagazzino).Distinct().Count(),
                        TotaleArticoliInMagazzini = g.Count()
                    })
                    .FirstOrDefaultAsync();

                var risultato = new
                {
                    Magazzini = statistiche,
                    Progressivi = statisticheProgressivi
                };

                return Json(risultato);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante il caricamento delle statistiche dei magazzini");
                return Json(new { error = "Errore durante il caricamento delle statistiche" });
            }
        }

        /// <summary>
        /// API per cercare magazzini per autocompletamento
        /// GET: TabellaMagazzini/SearchMagazzini
        /// </summary>
        /// <param name="term">Termine di ricerca</param>
        /// <returns>Lista di magazzini che corrispondono al termine di ricerca</returns>
        [HttpGet]
        public async Task<IActionResult> SearchMagazzini(string term)
        {
            try
            {
                if (string.IsNullOrEmpty(term) || term.Length < 1)
                {
                    return Json(new List<object>());
                }

                var magazzini = await _context.TabellaMagazzini
                    .Where(m => m.CodiceMagazzino.ToString().Contains(term) ||
                               (m.DescrizioneMagazzino != null && m.DescrizioneMagazzino.Contains(term)))
                    .OrderBy(m => m.CodiceMagazzino)
                    .Take(20)
                    .Select(m => new 
                    { 
                        value = m.CodiceMagazzino, 
                        label = m.NomeCompleto,
                        descrizione = m.DescrizioneMagazzino,
                        hasDescrizione = m.HasDescrizione
                    })
                    .ToListAsync();

                return Json(magazzini);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante la ricerca magazzini per termine: {Term}", term);
                return Json(new List<object>());
            }
        }
    }
}
