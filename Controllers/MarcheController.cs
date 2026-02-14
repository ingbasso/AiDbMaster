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
    /// Controller per la gestione delle Marche.
    /// Fornisce funzionalità CRUD complete per la tabella TabellaMarche.
    /// </summary>
    [Authorize]
    public class MarcheController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly MarcheService _marcheService;
        private readonly ILogger<MarcheController> _logger;

        public MarcheController(
            ApplicationDbContext context,
            MarcheService marcheService,
            ILogger<MarcheController> logger)
        {
            _context = context;
            _marcheService = marcheService;
            _logger = logger;
        }

        /// <summary>
        /// Visualizza l'elenco di tutte le marche
        /// GET: Marche
        /// </summary>
        public async Task<IActionResult> Index(
            string? search,
            string sortOrder = "codice",
            int page = 1,
            int pageSize = 50)
        {
            try
            {
                _logger.LogInformation("Caricamento tabella marche - Pagina: {Page}, Ricerca: {Search}", page, search);

                var viewModel = await _marcheService.GetMarcheAsync(search, sortOrder, page, pageSize);

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
                ViewBag.TotaleMarche = viewModel.TotaleMarche;

                return View(viewModel.Marche);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante il caricamento delle marche");
                TempData["ErrorMessage"] = "Errore durante il caricamento delle marche: " + ex.Message;
                return View(new List<Marca>());
            }
        }

        /// <summary>
        /// Visualizza i dettagli di una marca specifica
        /// GET: Marche/Details/5
        /// </summary>
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var viewModel = await _marcheService.GetMarcaDetailsAsync(id.Value);
            if (viewModel == null)
            {
                return NotFound();
            }

            ViewBag.PreviousId = viewModel.PreviousId;
            ViewBag.NextId = viewModel.NextId;

            return View(viewModel.Marca);
        }

        /// <summary>
        /// Visualizza il form per creare una nuova marca
        /// GET: Marche/Create
        /// </summary>
        public IActionResult Create()
        {
            return View();
        }

        /// <summary>
        /// Crea una nuova marca
        /// POST: Marche/Create
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateMarcaViewModel model)
        {
            if (ModelState.IsValid)
            {
                var result = await _marcheService.CreateMarcaAsync(model);

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
        /// Visualizza il form per modificare una marca
        /// GET: Marche/Edit/5
        /// </summary>
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var marca = await _marcheService.GetMarcaByIdAsync(id.Value);
            if (marca == null)
            {
                return NotFound();
            }

            var model = new EditMarcaViewModel
            {
                ID = marca.ID,
                CodiceMarca = marca.CodiceMarca,
                DescrizioneMarca = marca.DescrizioneMarca,
                UltimoAggiornamento = marca.UltimoAggiornamento
            };

            return View(model);
        }

        /// <summary>
        /// Modifica una marca esistente
        /// POST: Marche/Edit/5
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, EditMarcaViewModel model)
        {
            if (id != model.ID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var result = await _marcheService.UpdateMarcaAsync(model);

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
        /// Visualizza la conferma di eliminazione di una marca
        /// GET: Marche/Delete/5
        /// </summary>
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var marca = await _context.Marche
                .FirstOrDefaultAsync(m => m.ID == id);

            if (marca == null)
            {
                return NotFound();
            }

            var canDelete = await _marcheService.CanDeleteMarcaAsync(id.Value);

            var viewModel = new DeleteMarcaViewModel
            {
                Marca = marca
            };

            if (!canDelete.CanDelete)
            {
                ViewBag.CannotDeleteReason = canDelete.Reason;
            }

            return View(viewModel);
        }

        /// <summary>
        /// Elimina definitivamente una marca
        /// POST: Marche/Delete/5
        /// </summary>
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var result = await _marcheService.DeleteMarcaAsync(id);

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
        /// API per ottenere tutte le marche (per dropdown, autocomplete, etc.)
        /// GET: api/marche/all
        /// </summary>
        [HttpGet]
        [Route("api/marche/all")]
        public async Task<IActionResult> GetMarche()
        {
            try
            {
                var marche = await _marcheService.GetMarcheApiAsync();
                return Json(marche);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante il recupero delle marche");
                return StatusCode(500, "Errore durante il recupero delle marche");
            }
        }

        /// <summary>
        /// API per ottenere le statistiche delle marche
        /// GET: api/marche/stats
        /// </summary>
        [HttpGet]
        [Route("api/marche/stats")]
        public async Task<IActionResult> GetStats()
        {
            try
            {
                var stats = await _marcheService.GetStatsAsync();
                return Json(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante il recupero delle statistiche delle marche");
                return StatusCode(500, "Errore durante il recupero delle statistiche");
            }
        }
    }
}
