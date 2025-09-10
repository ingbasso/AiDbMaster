using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using AiDbMaster.Data;
using AiDbMaster.Models;

namespace AiDbMaster.Controllers
{
    /// <summary>
    /// Controller per la gestione dell'Anagrafica Fornitori
    /// Fornisce funzionalità di visualizzazione dei fornitori
    /// </summary>
    [Authorize]
    public class AnagraficaFornitoriController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AnagraficaFornitoriController> _logger;

        public AnagraficaFornitoriController(
            ApplicationDbContext context,
            ILogger<AnagraficaFornitoriController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Visualizza l'elenco di tutti i fornitori
        /// GET: AnagraficaFornitori
        /// </summary>
        /// <param name="search">Termine di ricerca per filtrare i fornitori</param>
        /// <param name="tipoAnagrafica">Filtro per tipo anagrafica</param>
        /// <param name="provincia">Filtro per provincia</param>
        /// <param name="sortOrder">Ordinamento dei risultati</param>
        /// <param name="page">Numero di pagina per la paginazione</param>
        /// <param name="pageSize">Numero di elementi per pagina</param>
        /// <returns>Vista con l'elenco dei fornitori</returns>
        public async Task<IActionResult> Index(
            string? search,
            string? tipoAnagrafica,
            string? provincia,
            string sortOrder = "codice",
            int page = 1,
            int pageSize = 50)
        {
            try
            {
                _logger.LogInformation("Caricamento anagrafica fornitori - Pagina: {Page}, Ricerca: {Search}", page, search);

                // Query base
                var query = _context.AnagraficaFornitori.AsQueryable();

                // Filtro per ricerca testuale
                if (!string.IsNullOrEmpty(search))
                {
                    query = query.Where(f => 
                        f.RagioneSociale.Contains(search) ||
                        f.CodiceFornitore.ToString().Contains(search) ||
                        (f.DescrizioneAggiuntiva != null && f.DescrizioneAggiuntiva.Contains(search)) ||
                        (f.Citta != null && f.Citta.Contains(search)) ||
                        (f.CodiceFiscale != null && f.CodiceFiscale.Contains(search)) ||
                        (f.PartitaIva != null && f.PartitaIva.Contains(search)));
                }

                // Filtro per tipo anagrafica
                if (!string.IsNullOrEmpty(tipoAnagrafica))
                {
                    query = query.Where(f => f.TipoAnagrafica == tipoAnagrafica);
                }

                // Filtro per provincia
                if (!string.IsNullOrEmpty(provincia))
                {
                    query = query.Where(f => f.Provincia == provincia);
                }

                // Ordinamento
                query = sortOrder switch
                {
                    "codice_desc" => query.OrderByDescending(f => f.CodiceFornitore),
                    "ragione" => query.OrderBy(f => f.RagioneSociale),
                    "ragione_desc" => query.OrderByDescending(f => f.RagioneSociale),
                    "citta" => query.OrderBy(f => f.Citta),
                    "citta_desc" => query.OrderByDescending(f => f.Citta),
                    "tipo" => query.OrderBy(f => f.TipoAnagrafica),
                    "tipo_desc" => query.OrderByDescending(f => f.TipoAnagrafica),
                    _ => query.OrderBy(f => f.CodiceFornitore)
                };

                // Conteggio totale per la paginazione
                var totalCount = await query.CountAsync();

                // Paginazione
                var fornitori = await query
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                // Preparazione dei dati per la vista
                ViewBag.CurrentSort = sortOrder;
                ViewBag.CurrentSearch = search;
                ViewBag.CurrentTipoAnagrafica = tipoAnagrafica;
                ViewBag.CurrentProvincia = provincia;
                ViewBag.CurrentPage = page;
                ViewBag.PageSize = pageSize;
                ViewBag.TotalCount = totalCount;
                ViewBag.TotalPages = (int)Math.Ceiling((double)totalCount / pageSize);

                // Dati per i filtri dropdown
                ViewBag.TipiAnagrafica = await _context.AnagraficaFornitori
                    .Where(f => !string.IsNullOrEmpty(f.TipoAnagrafica))
                    .Select(f => f.TipoAnagrafica)
                    .Distinct()
                    .OrderBy(t => t)
                    .ToListAsync();

                ViewBag.Province = await _context.AnagraficaFornitori
                    .Where(f => !string.IsNullOrEmpty(f.Provincia))
                    .Select(f => f.Provincia)
                    .Distinct()
                    .OrderBy(p => p)
                    .ToListAsync();

                _logger.LogInformation("Caricati {Count} fornitori su {Total} totali", fornitori.Count, totalCount);

                return View(fornitori);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante il caricamento dell'anagrafica fornitori");
                TempData["ErrorMessage"] = "Si è verificato un errore durante il caricamento dei fornitori.";
                return View(new List<AnagraficaFornitori>());
            }
        }

        /// <summary>
        /// Visualizza i dettagli di un fornitore specifico
        /// GET: AnagraficaFornitori/Details/5
        /// </summary>
        /// <param name="id">ID del fornitore da visualizzare</param>
        /// <returns>Vista con i dettagli del fornitore</returns>
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                _logger.LogWarning("Tentativo di accesso ai dettagli fornitore senza ID");
                return NotFound();
            }

            try
            {
                var fornitore = await _context.AnagraficaFornitori
                    .FirstOrDefaultAsync(f => f.Id == id);

                if (fornitore == null)
                {
                    _logger.LogWarning("Fornitore con ID {Id} non trovato", id);
                    return NotFound();
                }

                _logger.LogInformation("Visualizzazione dettagli fornitore: {CodiceFornitore} - {RagioneSociale}", 
                    fornitore.CodiceFornitore, fornitore.RagioneSociale);

                return View(fornitore);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante il caricamento dei dettagli del fornitore {Id}", id);
                TempData["ErrorMessage"] = "Si è verificato un errore durante il caricamento dei dettagli del fornitore.";
                return RedirectToAction(nameof(Index));
            }
        }

        /// <summary>
        /// API per ottenere i dati dei fornitori in formato JSON (per DataTables o altri componenti)
        /// GET: AnagraficaFornitori/GetFornitoriJson
        /// </summary>
        /// <returns>Dati dei fornitori in formato JSON</returns>
        [HttpGet]
        public async Task<IActionResult> GetFornitoriJson()
        {
            try
            {
                var fornitori = await _context.AnagraficaFornitori
                    .OrderBy(f => f.CodiceFornitore)
                    .Select(f => new
                    {
                        f.Id,
                        f.CodiceFornitore,
                        f.TipoAnagrafica,
                        f.RagioneSociale,
                        f.DescrizioneAggiuntiva,
                        f.Indirizzo,
                        f.Cap,
                        f.Citta,
                        f.Provincia,
                        f.CodiceFiscale,
                        f.PartitaIva,
                        f.Telefono,
                        f.FaxTelex,
                        IndirizzoCompleto = f.IndirizzoCompleto,
                        NomeCompleto = f.NomeCompleto,
                        ContattiCompleti = f.ContattiCompleti
                    })
                    .ToListAsync();

                return Json(new { data = fornitori });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante il caricamento dei dati JSON dei fornitori");
                return Json(new { error = "Errore durante il caricamento dei dati" });
            }
        }
    }
}
