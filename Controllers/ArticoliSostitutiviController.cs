using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using AiDbMaster.Data;
using AiDbMaster.Models;

namespace AiDbMaster.Controllers
{
    /// <summary>
    /// Controller per la gestione degli Articoli Sostitutivi
    /// Fornisce funzionalità di visualizzazione delle relazioni di sostituzione tra articoli
    /// </summary>
    [Authorize]
    public class ArticoliSostitutiviController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ArticoliSostitutiviController> _logger;

        public ArticoliSostitutiviController(
            ApplicationDbContext context,
            ILogger<ArticoliSostitutiviController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Visualizza l'elenco di tutte le relazioni di sostituzione tra articoli
        /// GET: ArticoliSostitutivi
        /// </summary>
        /// <param name="search">Termine di ricerca per filtrare le relazioni</param>
        /// <param name="codiceArticolo">Filtro per codice articolo principale</param>
        /// <param name="codiceArticoloSostitutivo">Filtro per codice articolo sostitutivo</param>
        /// <param name="sortOrder">Ordinamento dei risultati</param>
        /// <param name="page">Numero di pagina per la paginazione</param>
        /// <param name="pageSize">Numero di elementi per pagina</param>
        /// <returns>Vista con l'elenco delle relazioni di sostituzione</returns>
        public async Task<IActionResult> Index(
            string? search,
            string? codiceArticolo,
            string? codiceArticoloSostitutivo,
            string sortOrder = "articolo",
            int page = 1,
            int pageSize = 50)
        {
            try
            {
                _logger.LogInformation("Caricamento articoli sostitutivi - Pagina: {Page}, Ricerca: {Search}", page, search);

                // Query base
                var query = _context.ArticoliSostitutivi.AsQueryable();

                // Filtro per ricerca testuale
                if (!string.IsNullOrEmpty(search))
                {
                    query = query.Where(a => 
                        a.CodiceArticolo.Contains(search) ||
                        a.CodiceArticoloSostitutivo.Contains(search) ||
                        (a.Note != null && a.Note.Contains(search)));
                }

                // Filtro per codice articolo principale
                if (!string.IsNullOrEmpty(codiceArticolo))
                {
                    query = query.Where(a => a.CodiceArticolo.Contains(codiceArticolo));
                }

                // Filtro per codice articolo sostitutivo
                if (!string.IsNullOrEmpty(codiceArticoloSostitutivo))
                {
                    query = query.Where(a => a.CodiceArticoloSostitutivo.Contains(codiceArticoloSostitutivo));
                }

                // Ordinamento
                query = sortOrder switch
                {
                    "articolo_desc" => query.OrderByDescending(a => a.CodiceArticolo),
                    "sostitutivo" => query.OrderBy(a => a.CodiceArticoloSostitutivo),
                    "sostitutivo_desc" => query.OrderByDescending(a => a.CodiceArticoloSostitutivo),
                    "note" => query.OrderBy(a => a.Note),
                    "note_desc" => query.OrderByDescending(a => a.Note),
                    _ => query.OrderBy(a => a.CodiceArticolo).ThenBy(a => a.CodiceArticoloSostitutivo)
                };

                // Conteggio totale per la paginazione
                var totalCount = await query.CountAsync();

                // Paginazione
                var articoliSostitutivi = await query
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                // Preparazione dei dati per la vista
                ViewBag.CurrentSort = sortOrder;
                ViewBag.CurrentSearch = search;
                ViewBag.CurrentCodiceArticolo = codiceArticolo;
                ViewBag.CurrentCodiceArticoloSostitutivo = codiceArticoloSostitutivo;
                ViewBag.CurrentPage = page;
                ViewBag.PageSize = pageSize;
                ViewBag.TotalCount = totalCount;
                ViewBag.TotalPages = (int)Math.Ceiling((double)totalCount / pageSize);

                // Dati per i filtri dropdown
                ViewBag.CodiciArticolo = await _context.ArticoliSostitutivi
                    .Select(a => a.CodiceArticolo)
                    .Distinct()
                    .OrderBy(c => c)
                    .Take(100) // Limito per performance
                    .ToListAsync();

                ViewBag.CodiciArticoloSostitutivo = await _context.ArticoliSostitutivi
                    .Select(a => a.CodiceArticoloSostitutivo)
                    .Distinct()
                    .OrderBy(c => c)
                    .Take(100) // Limito per performance
                    .ToListAsync();

                // Statistiche aggiuntive
                ViewBag.TotalArticoliPrincipali = await _context.ArticoliSostitutivi
                    .Select(a => a.CodiceArticolo)
                    .Distinct()
                    .CountAsync();

                ViewBag.TotalArticoliSostitutivi = await _context.ArticoliSostitutivi
                    .Select(a => a.CodiceArticoloSostitutivo)
                    .Distinct()
                    .CountAsync();

                ViewBag.TotalConNote = await _context.ArticoliSostitutivi
                    .Where(a => !string.IsNullOrEmpty(a.Note))
                    .CountAsync();

                _logger.LogInformation("Caricati {Count} articoli sostitutivi su {Total} totali", articoliSostitutivi.Count, totalCount);

                return View(articoliSostitutivi);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante il caricamento degli articoli sostitutivi");
                TempData["ErrorMessage"] = "Si è verificato un errore durante il caricamento degli articoli sostitutivi.";
                return View(new List<ArticoliSostitutivi>());
            }
        }

        /// <summary>
        /// Visualizza i dettagli di una relazione di sostituzione specifica
        /// GET: ArticoliSostitutivi/Details/codiceArticolo/codiceArticoloSostitutivo
        /// </summary>
        /// <param name="codiceArticolo">Codice dell'articolo principale</param>
        /// <param name="codiceArticoloSostitutivo">Codice dell'articolo sostitutivo</param>
        /// <returns>Vista con i dettagli della relazione di sostituzione</returns>
        public async Task<IActionResult> Details(string? codiceArticolo, string? codiceArticoloSostitutivo)
        {
            if (string.IsNullOrEmpty(codiceArticolo) || string.IsNullOrEmpty(codiceArticoloSostitutivo))
            {
                _logger.LogWarning("Tentativo di accesso ai dettagli articolo sostitutivo senza codici completi");
                return NotFound();
            }

            try
            {
                var articoloSostitutivo = await _context.ArticoliSostitutivi
                    .FirstOrDefaultAsync(a => a.CodiceArticolo == codiceArticolo && 
                                            a.CodiceArticoloSostitutivo == codiceArticoloSostitutivo);

                if (articoloSostitutivo == null)
                {
                    _logger.LogWarning("Relazione di sostituzione non trovata: {CodiceArticolo} -> {CodiceArticoloSostitutivo}", 
                        codiceArticolo, codiceArticoloSostitutivo);
                    return NotFound();
                }

                // Cerco relazioni correlate
                ViewBag.AltreRelazioni = await _context.ArticoliSostitutivi
                    .Where(a => a.CodiceArticolo == codiceArticolo && 
                               a.CodiceArticoloSostitutivo != codiceArticoloSostitutivo)
                    .OrderBy(a => a.CodiceArticoloSostitutivo)
                    .ToListAsync();

                ViewBag.RelazioniInverse = await _context.ArticoliSostitutivi
                    .Where(a => a.CodiceArticoloSostitutivo == codiceArticolo)
                    .OrderBy(a => a.CodiceArticolo)
                    .ToListAsync();

                _logger.LogInformation("Visualizzazione dettagli relazione sostituzione: {CodiceArticolo} -> {CodiceArticoloSostitutivo}", 
                    codiceArticolo, codiceArticoloSostitutivo);

                return View(articoloSostitutivo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante il caricamento dei dettagli della relazione di sostituzione {CodiceArticolo} -> {CodiceArticoloSostitutivo}", 
                    codiceArticolo, codiceArticoloSostitutivo);
                TempData["ErrorMessage"] = "Si è verificato un errore durante il caricamento dei dettagli della relazione di sostituzione.";
                return RedirectToAction(nameof(Index));
            }
        }

        /// <summary>
        /// API per ottenere i dati delle relazioni di sostituzione in formato JSON
        /// GET: ArticoliSostitutivi/GetArticoliSostitutiviJson
        /// </summary>
        /// <returns>Dati delle relazioni di sostituzione in formato JSON</returns>
        [HttpGet]
        public async Task<IActionResult> GetArticoliSostitutiviJson()
        {
            try
            {
                var articoliSostitutivi = await _context.ArticoliSostitutivi
                    .OrderBy(a => a.CodiceArticolo)
                    .ThenBy(a => a.CodiceArticoloSostitutivo)
                    .Select(a => new
                    {
                        a.CodiceArticolo,
                        a.CodiceArticoloSostitutivo,
                        a.Note,
                        DescrizioneCompleta = a.DescrizioneCompleta,
                        TipoRelazione = a.TipoRelazione,
                        HasNote = a.HasNote,
                        ChiaveComposta = a.ChiaveComposta
                    })
                    .ToListAsync();

                return Json(new { data = articoliSostitutivi });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante il caricamento dei dati JSON degli articoli sostitutivi");
                return Json(new { error = "Errore durante il caricamento dei dati" });
            }
        }

        /// <summary>
        /// API per cercare articoli per autocompletamento
        /// GET: ArticoliSostitutivi/SearchArticoli
        /// </summary>
        /// <param name="term">Termine di ricerca</param>
        /// <returns>Lista di articoli che corrispondono al termine di ricerca</returns>
        [HttpGet]
        public async Task<IActionResult> SearchArticoli(string term)
        {
            try
            {
                if (string.IsNullOrEmpty(term) || term.Length < 2)
                {
                    return Json(new List<object>());
                }

                var articoli = await _context.ArticoliSostitutivi
                    .Where(a => a.CodiceArticolo.Contains(term) || a.CodiceArticoloSostitutivo.Contains(term))
                    .SelectMany(a => new[] { a.CodiceArticolo, a.CodiceArticoloSostitutivo })
                    .Distinct()
                    .Where(c => c.Contains(term))
                    .OrderBy(c => c)
                    .Take(20)
                    .Select(c => new { value = c, label = c })
                    .ToListAsync();

                return Json(articoli);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante la ricerca articoli per termine: {Term}", term);
                return Json(new List<object>());
            }
        }
    }
}
