using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using AiDbMaster.Data;
using AiDbMaster.Models;
using AiDbMaster.Attributes;
using AiDbMaster.Services;

namespace AiDbMaster.Controllers
{
    /// <summary>
    /// Controller per la gestione dell'Anagrafica Clienti
    /// Fornisce funzionalità di visualizzazione dei clienti
    /// </summary>
    [Authorize]
    [RegisterResource("AnagraficaClienti", "Anagrafica Clienti", 
        Description = "Gestione clienti", 
        MenuIcon = "bi-people", 
        MenuOrder = 2, 
        ParentResourceId = 2)] // Parent: Tabelle
    [RequirePermission("AnagraficaClienti", "View")]
    public class AnagraficaClientiController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AnagraficaClientiController> _logger;
        private readonly IDataFilterService _dataFilterService;

        public AnagraficaClientiController(
            ApplicationDbContext context,
            ILogger<AnagraficaClientiController> logger,
            IDataFilterService dataFilterService)
        {
            _context = context;
            _logger = logger;
            _dataFilterService = dataFilterService;
        }

        /// <summary>
        /// Visualizza l'elenco di tutti i clienti
        /// GET: AnagraficaClienti
        /// </summary>
        /// <param name="search">Termine di ricerca per filtrare i clienti</param>
        /// <param name="tipoAnagrafica">Filtro per tipo anagrafica</param>
        /// <param name="provincia">Filtro per provincia</param>
        /// <param name="sortOrder">Ordinamento dei risultati</param>
        /// <param name="page">Numero di pagina per la paginazione</param>
        /// <param name="pageSize">Numero di elementi per pagina</param>
        /// <returns>Vista con l'elenco dei clienti</returns>
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
                _logger.LogInformation("Caricamento anagrafica clienti - Pagina: {Page}, Ricerca: {Search}", page, search);

                // Query base con inclusione dell'agente
                var query = _context.AnagraficaClienti.Include(c => c.Agente).AsQueryable();

                // Applica filtro dati basato su ruolo utente (Agenti vedono solo propri clienti)
                query = await _dataFilterService.ApplyUserFilterAsync(query, User, "AnagraficaClienti");

                // Filtro per ricerca testuale
                if (!string.IsNullOrEmpty(search))
                {
                    query = query.Where(c => 
                        c.CodiceCliente.ToString().Contains(search) ||
                        c.RagioneSociale.Contains(search) ||
                        (c.DescrizioneUlteriore != null && c.DescrizioneUlteriore.Contains(search)) ||
                        (c.CodiceFiscale != null && c.CodiceFiscale.Contains(search)) ||
                        (c.PartitaIva != null && c.PartitaIva.Contains(search)) ||
                        (c.Citta != null && c.Citta.Contains(search)));
                }

                // Filtro per tipo anagrafica
                if (!string.IsNullOrEmpty(tipoAnagrafica))
                {
                    query = query.Where(c => c.Tipo == tipoAnagrafica);
                }

                // Filtro per provincia
                if (!string.IsNullOrEmpty(provincia))
                {
                    query = query.Where(c => c.Provincia == provincia);
                }

                // Ordinamento
                query = sortOrder.ToLower() switch
                {
                    "codice" => query.OrderBy(c => c.CodiceCliente),
                    "codice_desc" => query.OrderByDescending(c => c.CodiceCliente),
                    "ragione" => query.OrderBy(c => c.RagioneSociale),
                    "ragione_desc" => query.OrderByDescending(c => c.RagioneSociale),
                    "tipo" => query.OrderBy(c => c.Tipo).ThenBy(c => c.CodiceCliente),
                    "tipo_desc" => query.OrderByDescending(c => c.Tipo).ThenBy(c => c.CodiceCliente),
                    "citta" => query.OrderBy(c => c.Citta).ThenBy(c => c.CodiceCliente),
                    "citta_desc" => query.OrderByDescending(c => c.Citta).ThenBy(c => c.CodiceCliente),
                    "provincia" => query.OrderBy(c => c.Provincia).ThenBy(c => c.CodiceCliente),
                    "provincia_desc" => query.OrderByDescending(c => c.Provincia).ThenBy(c => c.CodiceCliente),
                    "agente" => query.OrderBy(c => c.Agente!.DescrizioneAgente ?? c.CodiceAgente.ToString()).ThenBy(c => c.CodiceCliente),
                    "agente_desc" => query.OrderByDescending(c => c.Agente!.DescrizioneAgente ?? c.CodiceAgente.ToString()).ThenBy(c => c.CodiceCliente),
                    _ => query.OrderBy(c => c.CodiceCliente)
                };

                // Conteggio totale per la paginazione
                var totalItems = await query.CountAsync();

                // Paginazione
                var clienti = await query
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                // Preparazione dati per la vista
                ViewBag.Search = search;
                ViewBag.TipoAnagrafica = tipoAnagrafica;
                ViewBag.Provincia = provincia;
                ViewBag.SortOrder = sortOrder;
                ViewBag.CurrentPage = page;
                ViewBag.PageSize = pageSize;
                ViewBag.TotalItems = totalItems;
                ViewBag.TotalPages = (int)Math.Ceiling((double)totalItems / pageSize);

                // Lista dei tipi anagrafica per il filtro dropdown
                ViewBag.TipiAnagrafica = await _context.AnagraficaClienti
                    .Where(c => c.Tipo != null && c.Tipo != "")
                    .Select(c => c.Tipo)
                    .Distinct()
                    .OrderBy(t => t)
                    .ToListAsync();

                // Lista delle province per il filtro dropdown
                ViewBag.Province = await _context.AnagraficaClienti
                    .Where(c => c.Provincia != null && c.Provincia != "")
                    .Select(c => c.Provincia!)
                    .Distinct()
                    .OrderBy(p => p)
                    .ToListAsync();

                _logger.LogInformation("Caricati {Count} clienti su {Total} totali", clienti.Count, totalItems);

                return View(clienti);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante il caricamento dell'anagrafica clienti");
                TempData["ErrorMessage"] = "Si è verificato un errore durante il caricamento dei clienti.";
                return View(new List<AnagraficaClienti>());
            }
        }

        /// <summary>
        /// Visualizza i dettagli di un cliente specifico
        /// GET: AnagraficaClienti/Details/5
        /// </summary>
        /// <param name="id">ID del cliente da visualizzare</param>
        /// <returns>Vista con i dettagli del cliente</returns>
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var cliente = await _context.AnagraficaClienti
                    .Include(c => c.Agente)
                    .FirstOrDefaultAsync(c => c.Id == id);

                if (cliente == null)
                {
                    _logger.LogWarning("Cliente con ID {Id} non trovato", id);
                    TempData["ErrorMessage"] = "Cliente non trovato.";
                    return RedirectToAction(nameof(Index));
                }

                _logger.LogInformation("Visualizzazione dettagli cliente: {CodiceCliente} - {RagioneSociale}", 
                    cliente.CodiceCliente, cliente.RagioneSociale);
                return View(cliente);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante il caricamento dei dettagli del cliente con ID {Id}", id);
                TempData["ErrorMessage"] = "Si è verificato un errore durante il caricamento dei dettagli del cliente.";
                return RedirectToAction(nameof(Index));
            }
        }

        /// <summary>
        /// API per ottenere i dati dei clienti in formato JSON (per DataTables o altri componenti)
        /// GET: AnagraficaClienti/GetClientiJson
        /// </summary>
        /// <returns>Dati dei clienti in formato JSON</returns>
        [HttpGet]
        public async Task<IActionResult> GetClientiJson()
        {
            try
            {
                // Query base
                var query = _context.AnagraficaClienti.AsQueryable();

                // Applica filtro dati basato su ruolo utente
                query = await _dataFilterService.ApplyUserFilterAsync(query, User, "AnagraficaClienti");

                var clienti = await query
                    .OrderBy(c => c.CodiceCliente)
                    .Select(c => new
                    {
                        c.Id,
                        c.CodiceCliente,
                        c.Tipo,
                        c.RagioneSociale,
                        c.DescrizioneUlteriore,
                        c.Indirizzo,
                        c.Cap,
                        c.Citta,
                        c.Provincia,
                        c.CodiceFiscale,
                        c.PartitaIva,
                        c.Telefono,
                        c.CodiceAgente,
                        IndirizzoCompleto = c.IndirizzoCompleto,
                        NomeCompleto = c.NomeCompleto
                    })
                    .ToListAsync();

                return Json(new { data = clienti });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante il caricamento dei dati JSON dei clienti");
                return Json(new { error = "Errore durante il caricamento dei dati" });
            }
        }
    }
}
