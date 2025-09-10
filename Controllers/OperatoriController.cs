using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using AiDbMaster.Data;
using AiDbMaster.Models;
using AiDbMaster.Services;
using AiDbMaster.ViewModels;

namespace AiDbMaster.Controllers
{
    /// <summary>
    /// Controller per la gestione degli Operatori
    /// Fornisce funzionalità CRUD complete per la tabella Operatori
    /// </summary>
    [Authorize]
    public class OperatoriController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly OperatoriService _operatoriService;
        private readonly ILogger<OperatoriController> _logger;

        public OperatoriController(
            ApplicationDbContext context,
            OperatoriService operatoriService,
            ILogger<OperatoriController> logger)
        {
            _context = context;
            _operatoriService = operatoriService;
            _logger = logger;
        }

        /// <summary>
        /// Visualizza l'elenco di tutti gli operatori
        /// GET: Operatori
        /// </summary>
        /// <param name="search">Termine di ricerca per filtrare gli operatori</param>
        /// <param name="attivo">Filtro per stato attivo</param>
        /// <param name="livelloCompetenza">Filtro per livello di competenza</param>
        /// <param name="hasEmail">Filtro per operatori con email</param>
        /// <param name="hasTelefono">Filtro per operatori con telefono</param>
        /// <param name="dataAssunzioneDa">Filtro data assunzione da</param>
        /// <param name="dataAssunzioneA">Filtro data assunzione a</param>
        /// <param name="sortOrder">Ordinamento dei risultati</param>
        /// <param name="page">Numero di pagina per la paginazione</param>
        /// <param name="pageSize">Numero di elementi per pagina</param>
        /// <returns>Vista con l'elenco degli operatori</returns>
        public async Task<IActionResult> Index(
            string? search,
            bool? attivo,
            int? livelloCompetenza,
            bool? hasEmail,
            bool? hasTelefono,
            DateTime? dataAssunzioneDa,
            DateTime? dataAssunzioneA,
            string sortOrder = "cognome",
            int page = 1,
            int pageSize = 50)
        {
            try
            {
                _logger.LogInformation("Caricamento tabella operatori - Pagina: {Page}, Ricerca: {Search}", page, search);

                var viewModel = await _operatoriService.GetOperatoriAsync(
                    search, attivo, livelloCompetenza, hasEmail, hasTelefono, 
                    dataAssunzioneDa, dataAssunzioneA, sortOrder, page, pageSize);
                
                // Preparazione dati per la vista
                ViewBag.CurrentSearch = viewModel.Search;
                ViewBag.CurrentAttivo = viewModel.Attivo;
                ViewBag.CurrentLivelloCompetenza = viewModel.LivelloCompetenza;
                ViewBag.CurrentHasEmail = viewModel.HasEmail;
                ViewBag.CurrentHasTelefono = viewModel.HasTelefono;
                ViewBag.CurrentDataAssunzioneDa = viewModel.DataAssunzioneDa;
                ViewBag.CurrentDataAssunzioneA = viewModel.DataAssunzioneA;
                ViewBag.CurrentSort = viewModel.SortOrder;
                ViewBag.CurrentPage = viewModel.CurrentPage;
                ViewBag.PageSize = viewModel.PageSize;
                ViewBag.TotalCount = viewModel.TotalCount;
                ViewBag.TotalPages = viewModel.TotalPages;

                // Parametri per l'ordinamento
                ViewBag.CodiceSortParm = viewModel.CodiceSortParm;
                ViewBag.NomeSortParm = viewModel.NomeSortParm;
                ViewBag.CognomeSortParm = viewModel.CognomeSortParm;
                ViewBag.EmailSortParm = viewModel.EmailSortParm;
                ViewBag.AttivoSortParm = viewModel.AttivoSortParm;
                ViewBag.LivelloSortParm = viewModel.LivelloSortParm;
                ViewBag.DataAssunzioneSortParm = viewModel.DataAssunzioneSortParm;

                return View(viewModel.Operatori);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante il caricamento degli operatori");
                TempData["ErrorMessage"] = "Errore durante il caricamento degli operatori: " + ex.Message;
                return View(new List<Operatore>());
            }
        }

        /// <summary>
        /// Visualizza i dettagli di un operatore specifico
        /// GET: Operatori/Details/5
        /// </summary>
        /// <param name="id">ID dell'operatore</param>
        /// <returns>Vista con i dettagli dell'operatore</returns>
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var viewModel = await _operatoriService.GetOperatoreDetailsAsync(id.Value);
            if (viewModel == null)
            {
                return NotFound();
            }

            return View(viewModel.Operatore);
        }

        /// <summary>
        /// Visualizza il form per creare un nuovo operatore
        /// GET: Operatori/Create
        /// </summary>
        /// <returns>Vista con il form di creazione</returns>
        public IActionResult Create()
        {
            return View();
        }

        /// <summary>
        /// Crea un nuovo operatore
        /// POST: Operatori/Create
        /// </summary>
        /// <param name="model">Dati dell'operatore da creare</param>
        /// <returns>Redirect alla lista o vista con errori</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateOperatoreViewModel model)
        {
            if (ModelState.IsValid)
            {
                var result = await _operatoriService.CreateOperatoreAsync(model);
                
                if (result.Success)
                {
                    TempData["SuccessMessage"] = result.Message;
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    ModelState.AddModelError(string.Empty, result.Message);
                }
            }
            return View(model);
        }

        /// <summary>
        /// Visualizza il form per modificare un operatore
        /// GET: Operatori/Edit/5
        /// </summary>
        /// <param name="id">ID dell'operatore da modificare</param>
        /// <returns>Vista con il form di modifica</returns>
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var operatore = await _operatoriService.GetOperatoreByIdAsync(id.Value);
            if (operatore == null)
            {
                return NotFound();
            }

            var model = new EditOperatoreViewModel
            {
                IdOperatore = operatore.IdOperatore,
                CodiceOperatore = operatore.CodiceOperatore,
                Nome = operatore.Nome,
                Cognome = operatore.Cognome,
                Email = operatore.Email,
                Telefono = operatore.Telefono,
                DataAssunzione = operatore.DataAssunzione,
                LivelloCompetenza = operatore.LivelloCompetenza,
                Note = operatore.Note,
                Attivo = operatore.Attivo
            };

            return View(model);
        }

        /// <summary>
        /// Modifica un operatore esistente
        /// POST: Operatori/Edit/5
        /// </summary>
        /// <param name="id">ID dell'operatore</param>
        /// <param name="model">Dati modificati dell'operatore</param>
        /// <returns>Redirect alla lista o vista con errori</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, EditOperatoreViewModel model)
        {
            if (id != model.IdOperatore)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var result = await _operatoriService.UpdateOperatoreAsync(model);
                
                if (result.Success)
                {
                    TempData["SuccessMessage"] = result.Message;
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    ModelState.AddModelError(string.Empty, result.Message);
                }
            }
            return View(model);
        }

        /// <summary>
        /// Visualizza la conferma di eliminazione di un operatore
        /// GET: Operatori/Delete/5
        /// </summary>
        /// <param name="id">ID dell'operatore da eliminare</param>
        /// <returns>Vista con la conferma di eliminazione</returns>
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var operatore = await _context.Operatori
                .Include(o => o.OrdiniProduzione)
                .FirstOrDefaultAsync(o => o.IdOperatore == id);

            if (operatore == null)
            {
                return NotFound();
            }

            // Verifica se può essere eliminato
            var canDelete = await _operatoriService.CanDeleteOperatoreAsync(id.Value);
            
            var viewModel = new DeleteOperatoreViewModel
            {
                Operatore = operatore,
                HasOrdiniProduzione = !canDelete.CanDelete,
                OrdiniProduzioneCount = canDelete.RelatedCount,
                OrdiniProduzioneDescription = canDelete.Reason
            };

            return View(viewModel);
        }

        /// <summary>
        /// Elimina definitivamente un operatore
        /// POST: Operatori/Delete/5
        /// </summary>
        /// <param name="id">ID dell'operatore da eliminare</param>
        /// <returns>Redirect alla lista</returns>
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var result = await _operatoriService.DeleteOperatoreAsync(id);
            
            if (result.Success)
            {
                TempData["SuccessMessage"] = result.Message;
            }
            else
            {
                TempData["ErrorMessage"] = result.Message;
            }

            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// API per ottenere gli operatori attivi (per dropdown, autocomplete, etc.)
        /// GET: api/operatori/attivi
        /// </summary>
        /// <returns>Lista degli operatori attivi</returns>
        [HttpGet]
        [Route("api/operatori/attivi")]
        public async Task<IActionResult> GetOperatoriAttivi()
        {
            try
            {
                var operatori = await _operatoriService.GetOperatoriAttiviAsync();
                return Json(operatori);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante il recupero degli operatori attivi");
                return StatusCode(500, "Errore durante il recupero degli operatori");
            }
        }

        /// <summary>
        /// API per ottenere le statistiche degli operatori
        /// GET: api/operatori/stats
        /// </summary>
        /// <returns>Statistiche degli operatori</returns>
        [HttpGet]
        [Route("api/operatori/stats")]
        public async Task<IActionResult> GetStats()
        {
            try
            {
                var stats = await _operatoriService.GetStatsAsync();
                return Json(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante il recupero delle statistiche degli operatori");
                return StatusCode(500, "Errore durante il recupero delle statistiche");
            }
        }

        /// <summary>
        /// API per ottenere il riepilogo operatori per dashboard
        /// GET: api/operatori/summary
        /// </summary>
        /// <returns>Riepilogo operatori</returns>
        [HttpGet]
        [Route("api/operatori/summary")]
        public async Task<IActionResult> GetSummary()
        {
            try
            {
                var summary = await _operatoriService.GetSummaryAsync();
                return Json(summary);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante il recupero del riepilogo operatori");
                return StatusCode(500, "Errore durante il recupero del riepilogo");
            }
        }

        /// <summary>
        /// API per attivare/disattivare un operatore
        /// POST: api/operatori/toggle-attivo/5
        /// </summary>
        /// <param name="id">ID dell'operatore</param>
        /// <returns>Risultato dell'operazione</returns>
        [HttpPost]
        [Route("api/operatori/toggle-attivo/{id}")]
        public async Task<IActionResult> ToggleAttivo(int id)
        {
            try
            {
                var result = await _operatoriService.ToggleAttivoAsync(id);
                return Json(new { success = result.Success, message = result.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante il cambio stato dell'operatore - ID: {Id}", id);
                return Json(new { success = false, message = "Errore durante il cambio stato" });
            }
        }

        /// <summary>
        /// API per la ricerca avanzata operatori
        /// POST: api/operatori/search
        /// </summary>
        /// <param name="searchModel">Parametri di ricerca</param>
        /// <returns>Risultati della ricerca</returns>
        [HttpPost]
        [Route("api/operatori/search")]
        public async Task<IActionResult> SearchOperatori([FromBody] OperatoriSearchViewModel searchModel)
        {
            try
            {
                // Implementa la logica di ricerca avanzata
                var query = _context.Operatori.AsQueryable();

                if (!string.IsNullOrEmpty(searchModel.GeneralSearch))
                {
                    query = query.Where(o => 
                        o.CodiceOperatore.Contains(searchModel.GeneralSearch) ||
                        o.Nome.Contains(searchModel.GeneralSearch) ||
                        o.Cognome.Contains(searchModel.GeneralSearch) ||
                        (o.Email != null && o.Email.Contains(searchModel.GeneralSearch)));
                }

                if (!string.IsNullOrEmpty(searchModel.CodiceOperatore))
                {
                    query = query.Where(o => o.CodiceOperatore.Contains(searchModel.CodiceOperatore));
                }

                if (!string.IsNullOrEmpty(searchModel.Nome))
                {
                    query = query.Where(o => o.Nome.Contains(searchModel.Nome));
                }

                if (!string.IsNullOrEmpty(searchModel.Cognome))
                {
                    query = query.Where(o => o.Cognome.Contains(searchModel.Cognome));
                }

                if (searchModel.LivelloCompetenzaMin.HasValue)
                {
                    query = query.Where(o => o.LivelloCompetenza >= searchModel.LivelloCompetenzaMin);
                }

                if (searchModel.LivelloCompetenzaMax.HasValue)
                {
                    query = query.Where(o => o.LivelloCompetenza <= searchModel.LivelloCompetenzaMax);
                }

                if (searchModel.DataAssunzioneDa.HasValue)
                {
                    query = query.Where(o => o.DataAssunzione >= searchModel.DataAssunzioneDa);
                }

                if (searchModel.DataAssunzioneA.HasValue)
                {
                    query = query.Where(o => o.DataAssunzione <= searchModel.DataAssunzioneA);
                }

                if (searchModel.SoloAttivi == true)
                {
                    query = query.Where(o => o.Attivo);
                }

                if (searchModel.ConEmail == true)
                {
                    query = query.Where(o => !string.IsNullOrEmpty(o.Email));
                }

                if (searchModel.ConTelefono == true)
                {
                    query = query.Where(o => !string.IsNullOrEmpty(o.Telefono));
                }

                if (searchModel.ConOrdiniAssegnati == true)
                {
                    query = query.Where(o => o.OrdiniProduzione.Any());
                }

                var risultati = await query
                    .OrderBy(o => o.Cognome)
                    .ThenBy(o => o.Nome)
                    .Take(100) // Limita i risultati
                    .Select(o => new OperatoreApiViewModel
                    {
                        Id = o.IdOperatore,
                        Codice = o.CodiceOperatore,
                        Nome = o.Nome,
                        Cognome = o.Cognome,
                        Attivo = o.Attivo,
                        LivelloCompetenza = o.LivelloCompetenza,
                        Email = o.Email,
                        Telefono = o.Telefono
                    })
                    .ToListAsync();

                return Json(risultati);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante la ricerca avanzata operatori");
                return StatusCode(500, "Errore durante la ricerca");
            }
        }
    }
}
