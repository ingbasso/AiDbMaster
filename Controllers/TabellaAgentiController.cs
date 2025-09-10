using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using AiDbMaster.Data;
using AiDbMaster.Models;

namespace AiDbMaster.Controllers
{
    /// <summary>
    /// Controller per la gestione della Tabella Agenti
    /// Fornisce funzionalità di visualizzazione degli agenti di vendita
    /// </summary>
    [Authorize]
    public class TabellaAgentiController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<TabellaAgentiController> _logger;

        public TabellaAgentiController(
            ApplicationDbContext context,
            ILogger<TabellaAgentiController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Visualizza l'elenco di tutti gli agenti
        /// GET: TabellaAgenti
        /// </summary>
        /// <param name="search">Termine di ricerca per filtrare gli agenti</param>
        /// <param name="provincia">Filtro per provincia</param>
        /// <param name="attivo">Filtro per stato attivo</param>
        /// <param name="sortOrder">Ordinamento dei risultati</param>
        /// <param name="page">Numero di pagina per la paginazione</param>
        /// <param name="pageSize">Numero di elementi per pagina</param>
        /// <returns>Vista con l'elenco degli agenti</returns>
        public async Task<IActionResult> Index(
            string? search,
            string? provincia,
            bool? attivo,
            string sortOrder = "codice",
            int page = 1,
            int pageSize = 50)
        {
            try
            {
                _logger.LogInformation("Caricamento tabella agenti - Pagina: {Page}, Ricerca: {Search}", page, search);

                // Query base
                var query = _context.TabellaAgenti.AsQueryable();

                // Filtro per ricerca testuale
                if (!string.IsNullOrEmpty(search))
                {
                    query = query.Where(a => 
                        a.CodiceAgente.ToString().Contains(search) ||
                        (a.DescrizioneAgente != null && a.DescrizioneAgente.Contains(search)) ||
                        (a.CittaAgente != null && a.CittaAgente.Contains(search)) ||
                        (a.ProvinciaAgente != null && a.ProvinciaAgente.Contains(search)) ||
                        (a.IndirizzoAgente != null && a.IndirizzoAgente.Contains(search)));
                }

                // Filtro per provincia
                if (!string.IsNullOrEmpty(provincia))
                {
                    query = query.Where(a => a.ProvinciaAgente == provincia);
                }

                // Filtro per stato attivo
                if (attivo.HasValue)
                {
                    query = query.Where(a => a.Attivo == attivo.Value);
                }

                // Ordinamento
                query = sortOrder switch
                {
                    "codice_desc" => query.OrderByDescending(a => a.CodiceAgente),
                    "nome" => query.OrderBy(a => a.DescrizioneAgente),
                    "nome_desc" => query.OrderByDescending(a => a.DescrizioneAgente),
                    "citta" => query.OrderBy(a => a.CittaAgente),
                    "citta_desc" => query.OrderByDescending(a => a.CittaAgente),
                    "provincia" => query.OrderBy(a => a.ProvinciaAgente),
                    "provincia_desc" => query.OrderByDescending(a => a.ProvinciaAgente),
                    "stato" => query.OrderBy(a => a.Attivo),
                    "stato_desc" => query.OrderByDescending(a => a.Attivo),
                    _ => query.OrderBy(a => a.CodiceAgente)
                };

                // Conteggio totale per la paginazione
                var totalCount = await query.CountAsync();

                // Paginazione
                var agenti = await query
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                // Preparazione dei dati per la vista
                ViewBag.CurrentSort = sortOrder;
                ViewBag.CurrentSearch = search;
                ViewBag.CurrentProvincia = provincia;
                ViewBag.CurrentAttivo = attivo;
                ViewBag.CurrentPage = page;
                ViewBag.PageSize = pageSize;
                ViewBag.TotalCount = totalCount;
                ViewBag.TotalPages = (int)Math.Ceiling((double)totalCount / pageSize);

                // Dati per i filtri dropdown
                ViewBag.Province = await _context.TabellaAgenti
                    .Where(a => !string.IsNullOrEmpty(a.ProvinciaAgente))
                    .Select(a => a.ProvinciaAgente)
                    .Distinct()
                    .OrderBy(p => p)
                    .ToListAsync();

                // Statistiche aggiuntive
                ViewBag.TotaleAttivi = await _context.TabellaAgenti
                    .CountAsync(a => a.Attivo);

                ViewBag.TotaleInattivi = await _context.TabellaAgenti
                    .CountAsync(a => !a.Attivo);

                ViewBag.TotaleConIndirizzo = await _context.TabellaAgenti
                    .CountAsync(a => !string.IsNullOrEmpty(a.IndirizzoAgente) && !string.IsNullOrEmpty(a.CittaAgente));

                ViewBag.ProvinceDistinte = await _context.TabellaAgenti
                    .Where(a => !string.IsNullOrEmpty(a.ProvinciaAgente))
                    .Select(a => a.ProvinciaAgente)
                    .Distinct()
                    .CountAsync();

                _logger.LogInformation("Caricati {Count} agenti su {Total} totali", agenti.Count, totalCount);

                return View(agenti);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante il caricamento della tabella agenti");
                TempData["ErrorMessage"] = "Si è verificato un errore durante il caricamento degli agenti.";
                return View(new List<TabellaAgenti>());
            }
        }

        /// <summary>
        /// Visualizza i dettagli di un agente specifico
        /// GET: TabellaAgenti/Details/5
        /// </summary>
        /// <param name="id">Codice dell'agente da visualizzare</param>
        /// <returns>Vista con i dettagli dell'agente</returns>
        public async Task<IActionResult> Details(short? id)
        {
            if (id == null)
            {
                _logger.LogWarning("Tentativo di accesso ai dettagli agente senza codice");
                return NotFound();
            }

            try
            {
                var agente = await _context.TabellaAgenti
                    .FirstOrDefaultAsync(a => a.CodiceAgente == id);

                if (agente == null)
                {
                    _logger.LogWarning("Agente con codice {CodiceAgente} non trovato", id);
                    return NotFound();
                }

                // Cerco altri agenti nella stessa provincia
                ViewBag.AltriAgentiProvincia = await _context.TabellaAgenti
                    .Where(a => a.ProvinciaAgente == agente.ProvinciaAgente && 
                               a.CodiceAgente != agente.CodiceAgente &&
                               !string.IsNullOrEmpty(a.ProvinciaAgente))
                    .OrderBy(a => a.CodiceAgente)
                    .ToListAsync();

                // Cerco altri agenti nella stessa città
                ViewBag.AltriAgentiCitta = await _context.TabellaAgenti
                    .Where(a => a.CittaAgente == agente.CittaAgente && 
                               a.CodiceAgente != agente.CodiceAgente &&
                               !string.IsNullOrEmpty(a.CittaAgente))
                    .OrderBy(a => a.CodiceAgente)
                    .ToListAsync();

                // Statistiche per la provincia dell'agente
                if (!string.IsNullOrEmpty(agente.ProvinciaAgente))
                {
                    var statisticheProvincia = await _context.TabellaAgenti
                        .Where(a => a.ProvinciaAgente == agente.ProvinciaAgente)
                        .GroupBy(a => 1)
                        .Select(g => new
                        {
                            TotaleAgenti = g.Count(),
                            AgentiAttivi = g.Count(a => a.Attivo),
                            AgentiInattivi = g.Count(a => !a.Attivo),
                            CittaDistinte = g.Select(a => a.CittaAgente).Where(c => !string.IsNullOrEmpty(c)).Distinct().Count()
                        })
                        .FirstOrDefaultAsync();

                    ViewBag.StatisticheProvincia = statisticheProvincia;
                }

                _logger.LogInformation("Visualizzazione dettagli agente: {CodiceAgente} - {DescrizioneAgente}", 
                    agente.CodiceAgente, agente.DescrizioneAgente);

                return View(agente);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante il caricamento dei dettagli dell'agente {CodiceAgente}", id);
                TempData["ErrorMessage"] = "Si è verificato un errore durante il caricamento dei dettagli dell'agente.";
                return RedirectToAction(nameof(Index));
            }
        }

        /// <summary>
        /// API per ottenere i dati degli agenti in formato JSON
        /// GET: TabellaAgenti/GetAgentiJson
        /// </summary>
        /// <returns>Dati degli agenti in formato JSON</returns>
        [HttpGet]
        public async Task<IActionResult> GetAgentiJson()
        {
            try
            {
                var agenti = await _context.TabellaAgenti
                    .OrderBy(a => a.CodiceAgente)
                    .Select(a => new
                    {
                        a.CodiceAgente,
                        a.DescrizioneAgente,
                        a.IndirizzoAgente,
                        a.CapAgente,
                        a.CittaAgente,
                        a.ProvinciaAgente,
                        a.Attivo,
                        IndirizzoCompleto = a.IndirizzoCompleto,
                        NomeCompleto = a.NomeCompleto,
                        StatoAgente = a.StatoAgente,
                        DescrizioneCompleta = a.DescrizioneCompleta,
                        InfoLocalizzazione = a.InfoLocalizzazione,
                        HasIndirizzoCompleto = a.HasIndirizzoCompleto
                    })
                    .ToListAsync();

                return Json(new { data = agenti });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante il caricamento dei dati JSON degli agenti");
                return Json(new { error = "Errore durante il caricamento dei dati" });
            }
        }

        /// <summary>
        /// API per ottenere statistiche aggregate degli agenti
        /// GET: TabellaAgenti/GetStatistiche
        /// </summary>
        /// <returns>Statistiche degli agenti in formato JSON</returns>
        [HttpGet]
        public async Task<IActionResult> GetStatistiche()
        {
            try
            {
                var statistiche = await _context.TabellaAgenti
                    .GroupBy(a => 1)
                    .Select(g => new
                    {
                        TotaleAgenti = g.Count(),
                        AgentiAttivi = g.Count(a => a.Attivo),
                        AgentiInattivi = g.Count(a => !a.Attivo),
                        AgentiConIndirizzo = g.Count(a => !string.IsNullOrEmpty(a.IndirizzoAgente) && !string.IsNullOrEmpty(a.CittaAgente)),
                        ProvinceDistinte = g.Select(a => a.ProvinciaAgente).Where(p => !string.IsNullOrEmpty(p)).Distinct().Count(),
                        CittaDistinte = g.Select(a => a.CittaAgente).Where(c => !string.IsNullOrEmpty(c)).Distinct().Count(),
                        PercentualeAttivi = g.Count() > 0 ? Math.Round((double)g.Count(a => a.Attivo) / g.Count() * 100, 1) : 0
                    })
                    .FirstOrDefaultAsync();

                return Json(statistiche);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante il caricamento delle statistiche degli agenti");
                return Json(new { error = "Errore durante il caricamento delle statistiche" });
            }
        }

        /// <summary>
        /// API per cercare agenti per autocompletamento
        /// GET: TabellaAgenti/SearchAgenti
        /// </summary>
        /// <param name="term">Termine di ricerca</param>
        /// <returns>Lista di agenti che corrispondono al termine di ricerca</returns>
        [HttpGet]
        public async Task<IActionResult> SearchAgenti(string term)
        {
            try
            {
                if (string.IsNullOrEmpty(term) || term.Length < 1)
                {
                    return Json(new List<object>());
                }

                var agenti = await _context.TabellaAgenti
                    .Where(a => a.CodiceAgente.ToString().Contains(term) ||
                               (a.DescrizioneAgente != null && a.DescrizioneAgente.Contains(term)))
                    .OrderBy(a => a.CodiceAgente)
                    .Take(20)
                    .Select(a => new 
                    { 
                        value = a.CodiceAgente, 
                        label = a.NomeCompleto,
                        attivo = a.Attivo
                    })
                    .ToListAsync();

                return Json(agenti);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante la ricerca agenti per termine: {Term}", term);
                return Json(new List<object>());
            }
        }
    }
}
