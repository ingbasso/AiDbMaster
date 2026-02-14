using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using AiDbMaster.Data;
using AiDbMaster.Models;
using AiDbMaster.Attributes;

namespace AiDbMaster.Controllers
{
    /// <summary>
    /// Controller per la gestione dell'Anagrafica Articoli
    /// Fornisce funzionalità di visualizzazione degli articoli
    /// </summary>
    [Authorize]
    [RequirePermission("AnagraficaArticoli", "View")]
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
        /// <param name="codiceArticolo">Filtro per codice articolo (ricerca esatta)</param>
        /// <param name="sortOrder">Ordinamento dei risultati</param>
        /// <param name="page">Numero di pagina per la paginazione</param>
        /// <param name="pageSize">Numero di elementi per pagina</param>
        /// <returns>Vista con l'elenco degli articoli</returns>
        public async Task<IActionResult> Index(
            string? codiceArticolo,
            string sortOrder = "codice",
            int page = 1,
            int pageSize = 50)
        {
            try
            {
                _logger.LogInformation("Caricamento anagrafica articoli - Pagina: {Page}, Articolo: {CodiceArticolo}", page, codiceArticolo);

                // Query base con Include per le relazioni (Marca, Famiglia, ClasseProvvigione)
                var query = _context.AnagraficaArticoli
                    .Include(a => a.MarcaNavigation)
                    .Include(a => a.FamigliaNavigation)
                    .Include(a => a.ClasseProvvigioneNavigation)
                    .AsQueryable();

                // Filtro per codice articolo (ricerca esatta come Select2)
                if (!string.IsNullOrEmpty(codiceArticolo))
                {
                    query = query.Where(a => a.CodiceArticolo == codiceArticolo);
                }

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

                // Recupera la descrizione dell'articolo selezionato
                string? descrizioneArticolo = null;
                if (!string.IsNullOrEmpty(codiceArticolo))
                {
                    descrizioneArticolo = await _context.AnagraficaArticoli
                        .Where(a => a.CodiceArticolo == codiceArticolo)
                        .Select(a => a.Descrizione)
                        .FirstOrDefaultAsync();
                }

                // Preparazione dati per la vista
                ViewBag.CodiceArticolo = codiceArticolo;
                ViewBag.DescrizioneArticolo = descrizioneArticolo;
                ViewBag.SortOrder = sortOrder;
                ViewBag.CurrentPage = page;
                ViewBag.PageSize = pageSize;
                ViewBag.TotalItems = totalItems;
                ViewBag.TotalPages = (int)Math.Ceiling((double)totalItems / pageSize);

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
                    .Include(a => a.MarcaNavigation)
                    .Include(a => a.FamigliaNavigation)
                    .Include(a => a.ClasseProvvigioneNavigation)
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
                    .Include(a => a.MarcaNavigation)
                    .Include(a => a.FamigliaNavigation)
                    .Include(a => a.ClasseProvvigioneNavigation)
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
                        ConversioneConfezione = a.ConversioneConfezione.ToString("F6"),
                        a.Marca,
                        DescrizioneMarca = a.MarcaNavigation != null ? a.MarcaNavigation.DescrizioneMarca : null,
                        a.Famiglia,
                        DescrizioneFamiglia = a.FamigliaNavigation != null ? a.FamigliaNavigation.DescrizioneFamiglia : null,
                        a.ClasseProvvigione,
                        DescrizioneClasseProvvigione = a.ClasseProvvigioneNavigation != null ? a.ClasseProvvigioneNavigation.DescrizioneClasse : null,
                        a.Outlet,
                        a.FuoriProduzione
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
