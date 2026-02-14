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
    /// Controller per la gestione delle Famiglie articoli.
    /// Fornisce funzionalità CRUD complete per la tabella TabellaFamiglie.
    /// </summary>
    [Authorize]
    public class FamiglieController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly FamiglieService _famiglieService;
        private readonly ILogger<FamiglieController> _logger;

        public FamiglieController(
            ApplicationDbContext context,
            FamiglieService famiglieService,
            ILogger<FamiglieController> logger)
        {
            _context = context;
            _famiglieService = famiglieService;
            _logger = logger;
        }

        /// <summary>
        /// Visualizza l'elenco di tutte le famiglie
        /// GET: Famiglie
        /// </summary>
        public async Task<IActionResult> Index(
            string? search,
            string sortOrder = "codice",
            int page = 1,
            int pageSize = 50)
        {
            try
            {
                _logger.LogInformation("Caricamento tabella famiglie - Pagina: {Page}, Ricerca: {Search}", page, search);

                var viewModel = await _famiglieService.GetFamiglieAsync(search, sortOrder, page, pageSize);

                // Preparazione dati per la vista
                ViewBag.CurrentSearch = viewModel.Search;
                ViewBag.CurrentSort = viewModel.SortOrder;
                ViewBag.CurrentPage = viewModel.CurrentPage;
                ViewBag.PageSize = viewModel.PageSize;
                ViewBag.TotalCount = viewModel.TotalCount;
                ViewBag.TotalPages = viewModel.TotalPages;

                // Parametri per l'ordinamento
                ViewBag.CodiceSortParm = viewModel.CodiceSortParm;
                ViewBag.DescrizioneSortParm = viewModel.DescrizioneSortParm;
                ViewBag.DataSortParm = viewModel.DataSortParm;

                // Statistiche
                ViewBag.TotaleFamiglie = viewModel.TotaleFamiglie;

                return View(viewModel.Famiglie);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante il caricamento delle famiglie");
                TempData["ErrorMessage"] = "Errore durante il caricamento delle famiglie: " + ex.Message;
                return View(new List<Famiglia>());
            }
        }

        /// <summary>
        /// Visualizza i dettagli di una famiglia specifica
        /// GET: Famiglie/Details/5
        /// </summary>
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var viewModel = await _famiglieService.GetFamigliaDetailsAsync(id.Value);
            if (viewModel == null)
            {
                return NotFound();
            }

            // Passa la navigazione alla vista
            ViewBag.PreviousId = viewModel.PreviousId;
            ViewBag.NextId = viewModel.NextId;

            return View(viewModel.Famiglia);
        }

        /// <summary>
        /// Visualizza il form per creare una nuova famiglia
        /// GET: Famiglie/Create
        /// </summary>
        public IActionResult Create()
        {
            return View();
        }

        /// <summary>
        /// Crea una nuova famiglia
        /// POST: Famiglie/Create
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateFamigliaViewModel model)
        {
            if (ModelState.IsValid)
            {
                var result = await _famiglieService.CreateFamigliaAsync(model);

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
        /// Visualizza il form per modificare una famiglia
        /// GET: Famiglie/Edit/5
        /// </summary>
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var famiglia = await _famiglieService.GetFamigliaByIdAsync(id.Value);
            if (famiglia == null)
            {
                return NotFound();
            }

            var model = new EditFamigliaViewModel
            {
                ID = famiglia.ID,
                CodiceFamiglia = famiglia.CodiceFamiglia,
                DescrizioneFamiglia = famiglia.DescrizioneFamiglia,
                UltimoAggiornamento = famiglia.UltimoAggiornamento
            };

            return View(model);
        }

        /// <summary>
        /// Modifica una famiglia esistente
        /// POST: Famiglie/Edit/5
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, EditFamigliaViewModel model)
        {
            if (id != model.ID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var result = await _famiglieService.UpdateFamigliaAsync(model);

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
        /// Visualizza la conferma di eliminazione di una famiglia
        /// GET: Famiglie/Delete/5
        /// </summary>
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var famiglia = await _context.Famiglie
                .FirstOrDefaultAsync(f => f.ID == id);

            if (famiglia == null)
            {
                return NotFound();
            }

            // Verifica se può essere eliminata
            var canDelete = await _famiglieService.CanDeleteFamigliaAsync(id.Value);

            var viewModel = new DeleteFamigliaViewModel
            {
                Famiglia = famiglia
            };

            if (!canDelete.CanDelete)
            {
                ViewBag.CannotDeleteReason = canDelete.Reason;
            }

            return View(viewModel);
        }

        /// <summary>
        /// Elimina definitivamente una famiglia
        /// POST: Famiglie/Delete/5
        /// </summary>
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var result = await _famiglieService.DeleteFamigliaAsync(id);

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
        /// API per ottenere tutte le famiglie (per dropdown, autocomplete, etc.)
        /// GET: api/famiglie/all
        /// </summary>
        [HttpGet]
        [Route("api/famiglie/all")]
        public async Task<IActionResult> GetFamiglie()
        {
            try
            {
                var famiglie = await _famiglieService.GetFamiglieApiAsync();
                return Json(famiglie);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante il recupero delle famiglie");
                return StatusCode(500, "Errore durante il recupero delle famiglie");
            }
        }

        /// <summary>
        /// API per ottenere le statistiche delle famiglie
        /// GET: api/famiglie/stats
        /// </summary>
        [HttpGet]
        [Route("api/famiglie/stats")]
        public async Task<IActionResult> GetStats()
        {
            try
            {
                var stats = await _famiglieService.GetStatsAsync();
                return Json(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante il recupero delle statistiche delle famiglie");
                return StatusCode(500, "Errore durante il recupero delle statistiche");
            }
        }
    }
}
