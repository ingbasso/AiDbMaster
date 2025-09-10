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
    /// Controller per la gestione dei Centri di Lavoro
    /// Fornisce funzionalità CRUD complete per la tabella CentriLavoro
    /// </summary>
    [Authorize]
    public class CentriLavoroController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly CentriLavoroService _centriLavoroService;
        private readonly ILogger<CentriLavoroController> _logger;

        public CentriLavoroController(
            ApplicationDbContext context,
            CentriLavoroService centriLavoroService,
            ILogger<CentriLavoroController> logger)
        {
            _context = context;
            _centriLavoroService = centriLavoroService;
            _logger = logger;
        }

        /// <summary>
        /// Visualizza l'elenco di tutti i centri di lavoro
        /// GET: CentriLavoro
        /// </summary>
        /// <param name="search">Termine di ricerca per filtrare i centri</param>
        /// <param name="attivo">Filtro per stato attivo</param>
        /// <param name="hasCapacita">Filtro per centri con capacità definita</param>
        /// <param name="hasCosto">Filtro per centri con costo definito</param>
        /// <param name="sortOrder">Ordinamento dei risultati</param>
        /// <param name="page">Numero di pagina per la paginazione</param>
        /// <param name="pageSize">Numero di elementi per pagina</param>
        /// <returns>Vista con l'elenco dei centri di lavoro</returns>
        public async Task<IActionResult> Index(
            string? search,
            bool? attivo,
            bool? hasCapacita,
            bool? hasCosto,
            string sortOrder = "descrizione",
            int page = 1,
            int pageSize = 50)
        {
            try
            {
                _logger.LogInformation("Caricamento tabella centri di lavoro - Pagina: {Page}, Ricerca: {Search}", page, search);

                var viewModel = await _centriLavoroService.GetCentriLavoroAsync(search, attivo, hasCapacita, hasCosto, sortOrder, page, pageSize);
                
                // Preparazione dati per la vista (per compatibilità con la vista esistente)
                ViewBag.CurrentSearch = viewModel.Search;
                ViewBag.CurrentAttivo = viewModel.Attivo;
                ViewBag.CurrentHasCapacita = viewModel.HasCapacita;
                ViewBag.CurrentHasCosto = viewModel.HasCosto;
                ViewBag.CurrentSort = viewModel.SortOrder;
                ViewBag.CurrentPage = viewModel.CurrentPage;
                ViewBag.PageSize = viewModel.PageSize;
                ViewBag.TotalCount = viewModel.TotalCount;
                ViewBag.TotalPages = viewModel.TotalPages;

                // Parametri per l'ordinamento
                ViewBag.CodiceSortParm = viewModel.CodiceSortParm;
                ViewBag.DescrizioneSortParm = viewModel.DescrizioneSortParm;
                ViewBag.AttivoSortParm = viewModel.AttivoSortParm;
                ViewBag.CapacitaSortParm = viewModel.CapacitaSortParm;
                ViewBag.CostoSortParm = viewModel.CostoSortParm;
                ViewBag.DataSortParm = viewModel.DataSortParm;

                return View(viewModel.CentriLavoro);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante il caricamento dei centri di lavoro");
                TempData["ErrorMessage"] = "Errore durante il caricamento dei centri di lavoro: " + ex.Message;
                return View(new List<CentroLavoro>());
            }
        }

        /// <summary>
        /// Visualizza i dettagli di un centro di lavoro specifico
        /// GET: CentriLavoro/Details/5
        /// </summary>
        /// <param name="id">ID del centro di lavoro</param>
        /// <returns>Vista con i dettagli del centro di lavoro</returns>
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var viewModel = await _centriLavoroService.GetCentroLavoroDetailsAsync(id.Value);
            if (viewModel == null)
            {
                return NotFound();
            }

            return View(viewModel.CentroLavoro);
        }

        /// <summary>
        /// Visualizza il form per creare un nuovo centro di lavoro
        /// GET: CentriLavoro/Create
        /// </summary>
        /// <returns>Vista con il form di creazione</returns>
        public IActionResult Create()
        {
            return View();
        }

        /// <summary>
        /// Crea un nuovo centro di lavoro
        /// POST: CentriLavoro/Create
        /// </summary>
        /// <param name="model">Dati del centro di lavoro da creare</param>
        /// <returns>Redirect alla lista o vista con errori</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateCentroLavoroViewModel model)
        {
            if (ModelState.IsValid)
            {
                var result = await _centriLavoroService.CreateCentroLavoroAsync(model);
                
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
        /// Visualizza il form per modificare un centro di lavoro
        /// GET: CentriLavoro/Edit/5
        /// </summary>
        /// <param name="id">ID del centro di lavoro da modificare</param>
        /// <returns>Vista con il form di modifica</returns>
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var centroLavoro = await _centriLavoroService.GetCentroLavoroByIdAsync(id.Value);
            if (centroLavoro == null)
            {
                return NotFound();
            }

            var model = new EditCentroLavoroViewModel
            {
                IdCentroLavoro = centroLavoro.IdCentroLavoro,
                CodiceCentro = centroLavoro.CodiceCentro,
                DescrizioneCentro = centroLavoro.DescrizioneCentro,
                CapacitaOraria = centroLavoro.CapacitaOraria,
                CostoOrarioStandard = centroLavoro.CostoOrarioStandard,
                Note = centroLavoro.Note,
                Attivo = centroLavoro.Attivo,
                DataCreazione = centroLavoro.DataCreazione,
                DataUltimaModifica = centroLavoro.DataUltimaModifica
            };

            return View(model);
        }

        /// <summary>
        /// Modifica un centro di lavoro esistente
        /// POST: CentriLavoro/Edit/5
        /// </summary>
        /// <param name="id">ID del centro di lavoro</param>
        /// <param name="model">Dati modificati del centro di lavoro</param>
        /// <returns>Redirect alla lista o vista con errori</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, EditCentroLavoroViewModel model)
        {
            if (id != model.IdCentroLavoro)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var result = await _centriLavoroService.UpdateCentroLavoroAsync(model);
                
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
        /// Visualizza la conferma di eliminazione di un centro di lavoro
        /// GET: CentriLavoro/Delete/5
        /// </summary>
        /// <param name="id">ID del centro di lavoro da eliminare</param>
        /// <returns>Vista con la conferma di eliminazione</returns>
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var centroLavoro = await _context.CentriLavoro
                .Include(c => c.OrdiniProduzione)
                .FirstOrDefaultAsync(c => c.IdCentroLavoro == id);

            if (centroLavoro == null)
            {
                return NotFound();
            }

            // Verifica se può essere eliminato
            var canDelete = await _centriLavoroService.CanDeleteCentroLavoroAsync(id.Value);
            
            var viewModel = new DeleteCentroLavoroViewModel
            {
                CentroLavoro = centroLavoro,
                HasOrdiniProduzione = !canDelete.CanDelete,
                OrdiniProduzioneCount = canDelete.RelatedCount,
                OrdiniProduzioneDescription = canDelete.Reason
            };

            return View(viewModel);
        }

        /// <summary>
        /// Elimina definitivamente un centro di lavoro
        /// POST: CentriLavoro/Delete/5
        /// </summary>
        /// <param name="id">ID del centro di lavoro da eliminare</param>
        /// <returns>Redirect alla lista</returns>
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var result = await _centriLavoroService.DeleteCentroLavoroAsync(id);
            
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
        /// API per ottenere i centri di lavoro attivi (per dropdown, autocomplete, etc.)
        /// GET: api/centri-lavoro/attivi
        /// </summary>
        /// <returns>Lista dei centri di lavoro attivi</returns>
        [HttpGet]
        [Route("api/centri-lavoro/attivi")]
        public async Task<IActionResult> GetCentriLavoroAttivi()
        {
            try
            {
                var centriLavoro = await _centriLavoroService.GetCentriLavoroAttiviAsync();
                return Json(centriLavoro);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante il recupero dei centri di lavoro attivi");
                return StatusCode(500, "Errore durante il recupero dei centri di lavoro");
            }
        }

        /// <summary>
        /// API per ottenere le statistiche dei centri di lavoro
        /// GET: api/centri-lavoro/stats
        /// </summary>
        /// <returns>Statistiche dei centri di lavoro</returns>
        [HttpGet]
        [Route("api/centri-lavoro/stats")]
        public async Task<IActionResult> GetStats()
        {
            try
            {
                var stats = await _centriLavoroService.GetStatsAsync();
                return Json(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante il recupero delle statistiche dei centri di lavoro");
                return StatusCode(500, "Errore durante il recupero delle statistiche");
            }
        }

        /// <summary>
        /// API per attivare/disattivare un centro di lavoro
        /// POST: api/centri-lavoro/toggle-attivo/5
        /// </summary>
        /// <param name="id">ID del centro di lavoro</param>
        /// <returns>Risultato dell'operazione</returns>
        [HttpPost]
        [Route("api/centri-lavoro/toggle-attivo/{id}")]
        public async Task<IActionResult> ToggleAttivo(int id)
        {
            try
            {
                var result = await _centriLavoroService.ToggleAttivoAsync(id);
                return Json(new { success = result.Success, message = result.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante il cambio stato del centro di lavoro - ID: {Id}", id);
                return Json(new { success = false, message = "Errore durante il cambio stato" });
            }
        }
    }
}
