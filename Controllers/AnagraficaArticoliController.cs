using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using AiDbMaster.Data;
using AiDbMaster.Models;

namespace AiDbMaster.Controllers
{
    /// <summary>
    /// Controller per la gestione dell'Anagrafica Articoli
    /// Fornisce funzionalità di visualizzazione degli articoli
    /// </summary>
    [Authorize]
    public class AnagraficaArticoliController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AnagraficaArticoliController> _logger;

        public AnagraficaArticoliController(
            ApplicationDbContext context,
            ILogger<AnagraficaArticoliController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Visualizza l'elenco di tutti gli articoli
        /// GET: AnagraficaArticoli
        /// </summary>
        /// <param name="search">Termine di ricerca per filtrare gli articoli</param>
        /// <param name="categoria">Filtro per categoria</param>
        /// <param name="attivo">Filtro per stato attivo/inattivo</param>
        /// <param name="sortOrder">Ordinamento dei risultati</param>
        /// <param name="page">Numero di pagina per la paginazione</param>
        /// <param name="pageSize">Numero di elementi per pagina</param>
        /// <returns>Vista con l'elenco degli articoli</returns>
        public async Task<IActionResult> Index(
            string? search,
            string? categoria,
            bool? attivo,
            string sortOrder = "codice",
            int page = 1,
            int pageSize = 50)
        {
            try
            {
                _logger.LogInformation("Caricamento anagrafica articoli - Pagina: {Page}, Ricerca: {Search}", page, search);

                // Query base
                var query = _context.AnagraficaArticoli.AsQueryable();

                // Filtro per ricerca testuale
                if (!string.IsNullOrEmpty(search))
                {
                    query = query.Where(a => 
                        a.CodiceArticolo.Contains(search) ||
                        a.Descrizione.Contains(search) ||
                        (a.CodiceAlternativo != null && a.CodiceAlternativo.Contains(search)) ||
                        (a.DescrizioneUlteriore != null && a.DescrizioneUlteriore.Contains(search)));
                }

                // Filtro per tipo articolo (uso come categoria)
                if (!string.IsNullOrEmpty(categoria))
                {
                    query = query.Where(a => a.TipoArticolo == categoria);
                }

                // Nota: La tabella non ha un campo "Attivo", quindi questo filtro è disabilitato
                // if (attivo.HasValue)
                // {
                //     query = query.Where(a => a.Attivo == attivo.Value);
                // }

                // Ordinamento
                query = sortOrder.ToLower() switch
                {
                    "codice" => query.OrderBy(a => a.CodiceArticolo),
                    "codice_desc" => query.OrderByDescending(a => a.CodiceArticolo),
                    "descrizione" => query.OrderBy(a => a.Descrizione),
                    "descrizione_desc" => query.OrderByDescending(a => a.Descrizione),
                    "tipo" => query.OrderBy(a => a.TipoArticolo).ThenBy(a => a.CodiceArticolo),
                    "tipo_desc" => query.OrderByDescending(a => a.TipoArticolo).ThenBy(a => a.CodiceArticolo),
                    "unita" => query.OrderBy(a => a.UnitaMisura).ThenBy(a => a.CodiceArticolo),
                    "unita_desc" => query.OrderByDescending(a => a.UnitaMisura).ThenBy(a => a.CodiceArticolo),
                    _ => query.OrderBy(a => a.CodiceArticolo)
                };

                // Conteggio totale per la paginazione
                var totalItems = await query.CountAsync();

                // Paginazione
                var articoli = await query
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                // Preparazione dati per la vista
                ViewBag.Search = search;
                ViewBag.Categoria = categoria;
                ViewBag.Attivo = attivo;
                ViewBag.SortOrder = sortOrder;
                ViewBag.CurrentPage = page;
                ViewBag.PageSize = pageSize;
                ViewBag.TotalItems = totalItems;
                ViewBag.TotalPages = (int)Math.Ceiling((double)totalItems / pageSize);

                // Lista dei tipi articolo per il filtro dropdown
                ViewBag.Categorie = await _context.AnagraficaArticoli
                    .Where(a => a.TipoArticolo != null && a.TipoArticolo != "")
                    .Select(a => a.TipoArticolo!)
                    .Distinct()
                    .OrderBy(c => c)
                    .ToListAsync();

                _logger.LogInformation("Caricati {Count} articoli su {Total} totali", articoli.Count, totalItems);

                return View(articoli);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante il caricamento dell'anagrafica articoli");
                TempData["ErrorMessage"] = "Si è verificato un errore durante il caricamento degli articoli.";
                return View(new List<AnagraficaArticoli>());
            }
        }

        /// <summary>
        /// Visualizza i dettagli di un articolo specifico
        /// GET: AnagraficaArticoli/Details/5
        /// </summary>
        /// <param name="id">ID dell'articolo da visualizzare</param>
        /// <returns>Vista con i dettagli dell'articolo</returns>
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var articolo = await _context.AnagraficaArticoli
                    .FirstOrDefaultAsync(a => a.Id == id);

                if (articolo == null)
                {
                    _logger.LogWarning("Articolo con ID {Id} non trovato", id);
                    TempData["ErrorMessage"] = "Articolo non trovato.";
                    return RedirectToAction(nameof(Index));
                }

                _logger.LogInformation("Visualizzazione dettagli articolo: {CodiceArticolo}", articolo.CodiceArticolo);
                return View(articolo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante il caricamento dei dettagli dell'articolo con ID {Id}", id);
                TempData["ErrorMessage"] = "Si è verificato un errore durante il caricamento dei dettagli dell'articolo.";
                return RedirectToAction(nameof(Index));
            }
        }

        /// <summary>
        /// API per ottenere i dati degli articoli in formato JSON (per DataTables o altri componenti)
        /// GET: AnagraficaArticoli/GetArticoliJson
        /// </summary>
        /// <returns>Dati degli articoli in formato JSON</returns>
        [HttpGet]
        public async Task<IActionResult> GetArticoliJson()
        {
            try
            {
                var articoli = await _context.AnagraficaArticoli
                    .OrderBy(a => a.CodiceArticolo)
                    .Select(a => new
                    {
                        a.Id,
                        a.CodiceArticolo,
                        a.CodiceAlternativo,
                        a.Descrizione,
                        a.DescrizioneUlteriore,
                        a.TipoArticolo,
                        a.UnitaMisura,
                        a.SecondaUnitaMisura,
                        Conversione = a.Conversione.ToString("F6"),
                        a.UnitaMisuraConfezione,
                        ConversioneConfezione = a.ConversioneConfezione.ToString("F6")
                    })
                    .ToListAsync();

                return Json(new { data = articoli });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante il caricamento dei dati JSON degli articoli");
                return Json(new { error = "Errore durante il caricamento dei dati" });
            }
        }
    }
}
