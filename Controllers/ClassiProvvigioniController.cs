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
    /// Controller per la gestione delle Classi Provvigioni.
    /// Fornisce funzionalità CRUD complete per la tabella TabellaClassiProvvigioni.
    /// </summary>
    [Authorize]
    public class ClassiProvvigioniController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ClassiProvvigioniService _classiProvvigioniService;
        private readonly ILogger<ClassiProvvigioniController> _logger;

        public ClassiProvvigioniController(
            ApplicationDbContext context,
            ClassiProvvigioniService classiProvvigioniService,
            ILogger<ClassiProvvigioniController> logger)
        {
            _context = context;
            _classiProvvigioniService = classiProvvigioniService;
            _logger = logger;
        }

        /// <summary>
        /// Visualizza l'elenco di tutte le classi provvigioni
        /// GET: ClassiProvvigioni
        /// </summary>
        /// <param name="search">Termine di ricerca per filtrare le classi</param>
        /// <param name="sortOrder">Ordinamento dei risultati</param>
        /// <param name="page">Numero di pagina per la paginazione</param>
        /// <param name="pageSize">Numero di elementi per pagina</param>
        /// <returns>Vista con l'elenco delle classi provvigioni</returns>
        public async Task<IActionResult> Index(
            string? search,
            string sortOrder = "codice",
            int page = 1,
            int pageSize = 50)
        {
            try
            {
                _logger.LogInformation("Caricamento tabella classi provvigioni - Pagina: {Page}, Ricerca: {Search}", page, search);

                var viewModel = await _classiProvvigioniService.GetClassiProvvigioniAsync(search, sortOrder, page, pageSize);

                // Preparazione dati per la vista (per compatibilità con la vista esistente)
                ViewBag.CurrentSearch = viewModel.Search;
                ViewBag.CurrentSort = viewModel.SortOrder;
                ViewBag.CurrentPage = viewModel.CurrentPage;
                ViewBag.PageSize = viewModel.PageSize;
                ViewBag.TotalCount = viewModel.TotalCount;
                ViewBag.TotalPages = viewModel.TotalPages;

                // Parametri per l'ordinamento
                ViewBag.CodiceSortParm = viewModel.CodiceSortParm;
                ViewBag.DescrizioneSortParm = viewModel.DescrizioneSortParm;
                ViewBag.PercScontoSortParm = viewModel.PercScontoSortParm;
                ViewBag.DataSortParm = viewModel.DataSortParm;

                // Statistiche
                ViewBag.TotaleClassi = viewModel.TotaleClassi;
                ViewBag.PercScontoMedia = viewModel.PercScontoMedia;
                ViewBag.PercScontoMin = viewModel.PercScontoMin;
                ViewBag.PercScontoMax = viewModel.PercScontoMax;

                return View(viewModel.ClassiProvvigioni);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante il caricamento delle classi provvigioni");
                TempData["ErrorMessage"] = "Errore durante il caricamento delle classi provvigioni: " + ex.Message;
                return View(new List<ClasseProvvigione>());
            }
        }

        /// <summary>
        /// Visualizza i dettagli di una classe provvigione specifica
        /// GET: ClassiProvvigioni/Details/5
        /// </summary>
        /// <param name="id">ID della classe provvigione</param>
        /// <returns>Vista con i dettagli della classe provvigione</returns>
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var viewModel = await _classiProvvigioniService.GetClasseProvvigioneDetailsAsync(id.Value);
            if (viewModel == null)
            {
                return NotFound();
            }

            // Passa la navigazione alla vista
            ViewBag.PreviousId = viewModel.PreviousId;
            ViewBag.NextId = viewModel.NextId;

            return View(viewModel.ClasseProvvigione);
        }

        /// <summary>
        /// Visualizza il form per creare una nuova classe provvigione
        /// GET: ClassiProvvigioni/Create
        /// </summary>
        /// <returns>Vista con il form di creazione</returns>
        public IActionResult Create()
        {
            return View();
        }

        /// <summary>
        /// Crea una nuova classe provvigione
        /// POST: ClassiProvvigioni/Create
        /// </summary>
        /// <param name="model">Dati della classe provvigione da creare</param>
        /// <returns>Redirect alla lista o vista con errori</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateClasseProvvigioneViewModel model)
        {
            if (ModelState.IsValid)
            {
                var result = await _classiProvvigioniService.CreateClasseProvvigioneAsync(model);

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
        /// Visualizza il form per modificare una classe provvigione
        /// GET: ClassiProvvigioni/Edit/5
        /// </summary>
        /// <param name="id">ID della classe provvigione da modificare</param>
        /// <returns>Vista con il form di modifica</returns>
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var classeProvvigione = await _classiProvvigioniService.GetClasseProvvigioneByIdAsync(id.Value);
            if (classeProvvigione == null)
            {
                return NotFound();
            }

            var model = new EditClasseProvvigioneViewModel
            {
                ID = classeProvvigione.ID,
                CodiceClasse = classeProvvigione.CodiceClasse,
                DescrizioneClasse = classeProvvigione.DescrizioneClasse,
                Perc_Sconto = classeProvvigione.Perc_Sconto,
                UltimoAggiornamento = classeProvvigione.UltimoAggiornamento
            };

            return View(model);
        }

        /// <summary>
        /// Modifica una classe provvigione esistente
        /// POST: ClassiProvvigioni/Edit/5
        /// </summary>
        /// <param name="id">ID della classe provvigione</param>
        /// <param name="model">Dati modificati della classe provvigione</param>
        /// <returns>Redirect alla lista o vista con errori</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, EditClasseProvvigioneViewModel model)
        {
            if (id != model.ID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var result = await _classiProvvigioniService.UpdateClasseProvvigioneAsync(model);

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
        /// Visualizza la conferma di eliminazione di una classe provvigione
        /// GET: ClassiProvvigioni/Delete/5
        /// </summary>
        /// <param name="id">ID della classe provvigione da eliminare</param>
        /// <returns>Vista con la conferma di eliminazione</returns>
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var classeProvvigione = await _context.ClassiProvvigioni
                .FirstOrDefaultAsync(c => c.ID == id);

            if (classeProvvigione == null)
            {
                return NotFound();
            }

            // Verifica se può essere eliminata
            var canDelete = await _classiProvvigioniService.CanDeleteClasseProvvigioneAsync(id.Value);

            var viewModel = new DeleteClasseProvvigioneViewModel
            {
                ClasseProvvigione = classeProvvigione
            };

            if (!canDelete.CanDelete)
            {
                ViewBag.CannotDeleteReason = canDelete.Reason;
            }

            return View(viewModel);
        }

        /// <summary>
        /// Elimina definitivamente una classe provvigione
        /// POST: ClassiProvvigioni/Delete/5
        /// </summary>
        /// <param name="id">ID della classe provvigione da eliminare</param>
        /// <returns>Redirect alla lista</returns>
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var result = await _classiProvvigioniService.DeleteClasseProvvigioneAsync(id);

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
        /// API per ottenere tutte le classi provvigioni (per dropdown, autocomplete, etc.)
        /// GET: api/classi-provvigioni/all
        /// </summary>
        /// <returns>Lista di tutte le classi provvigioni</returns>
        [HttpGet]
        [Route("api/classi-provvigioni/all")]
        public async Task<IActionResult> GetClassiProvvigioni()
        {
            try
            {
                var classiProvvigioni = await _classiProvvigioniService.GetClassiProvvigioniApiAsync();
                return Json(classiProvvigioni);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante il recupero delle classi provvigioni");
                return StatusCode(500, "Errore durante il recupero delle classi provvigioni");
            }
        }

        /// <summary>
        /// API per ottenere le statistiche delle classi provvigioni
        /// GET: api/classi-provvigioni/stats
        /// </summary>
        /// <returns>Statistiche delle classi provvigioni</returns>
        [HttpGet]
        [Route("api/classi-provvigioni/stats")]
        public async Task<IActionResult> GetStats()
        {
            try
            {
                var stats = await _classiProvvigioniService.GetStatsAsync();
                return Json(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante il recupero delle statistiche delle classi provvigioni");
                return StatusCode(500, "Errore durante il recupero delle statistiche");
            }
        }
    }
}
