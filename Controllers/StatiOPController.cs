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
    /// Controller per la gestione degli Stati degli Ordini di Produzione
    /// Fornisce funzionalità CRUD complete per la tabella StatiOP
    /// </summary>
    [Authorize]
    public class StatiOPController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly StatiOPService _statiOPService;
        private readonly ILogger<StatiOPController> _logger;

        public StatiOPController(
            ApplicationDbContext context,
            StatiOPService statiOPService,
            ILogger<StatiOPController> logger)
        {
            _context = context;
            _statiOPService = statiOPService;
            _logger = logger;
        }

        /// <summary>
        /// Visualizza l'elenco di tutti gli stati OP
        /// GET: StatiOP
        /// </summary>
        /// <param name="search">Termine di ricerca per filtrare gli stati</param>
        /// <param name="attivo">Filtro per stato attivo</param>
        /// <param name="codiceStato">Filtro per codice stato specifico</param>
        /// <param name="sortOrder">Ordinamento dei risultati</param>
        /// <param name="page">Numero di pagina per la paginazione</param>
        /// <param name="pageSize">Numero di elementi per pagina</param>
        /// <returns>Vista con l'elenco degli stati OP</returns>
        public async Task<IActionResult> Index(
            string? search,
            bool? attivo,
            string? codiceStato,
            string sortOrder = "ordine",
            int page = 1,
            int pageSize = 50)
        {
            try
            {
                _logger.LogInformation("Caricamento tabella stati OP - Pagina: {Page}, Ricerca: {Search}", page, search);

                var viewModel = await _statiOPService.GetStatiOPAsync(search, attivo, codiceStato, sortOrder, page, pageSize);
                
                // Preparazione dati per la vista
                ViewBag.CurrentSearch = viewModel.Search;
                ViewBag.CurrentAttivo = viewModel.Attivo;
                ViewBag.CurrentCodiceStato = viewModel.CodiceStato;
                ViewBag.CurrentSort = viewModel.SortOrder;
                ViewBag.CurrentPage = viewModel.CurrentPage;
                ViewBag.PageSize = viewModel.PageSize;
                ViewBag.TotalCount = viewModel.TotalCount;
                ViewBag.TotalPages = viewModel.TotalPages;

                // Parametri per l'ordinamento
                ViewBag.CodiceSortParm = viewModel.CodiceSortParm;
                ViewBag.DescrizioneSortParm = viewModel.DescrizioneSortParm;
                ViewBag.AttivoSortParm = viewModel.AttivoSortParm;
                ViewBag.OrdineSortParm = viewModel.OrdineSortParm;

                return View(viewModel.StatiOP);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante il caricamento degli stati OP");
                TempData["ErrorMessage"] = "Errore durante il caricamento degli stati OP: " + ex.Message;
                return View(new List<StatoOP>());
            }
        }

        /// <summary>
        /// Visualizza i dettagli di uno stato OP specifico
        /// GET: StatiOP/Details/5
        /// </summary>
        /// <param name="id">ID dello stato OP</param>
        /// <returns>Vista con i dettagli dello stato OP</returns>
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var viewModel = await _statiOPService.GetStatoOPDetailsAsync(id.Value);
            if (viewModel == null)
            {
                return NotFound();
            }

            return View(viewModel.StatoOP);
        }

        /// <summary>
        /// Visualizza il form per creare un nuovo stato OP
        /// GET: StatiOP/Create
        /// </summary>
        /// <returns>Vista con il form di creazione</returns>
        public async Task<IActionResult> Create()
        {
            var model = new CreateStatoOPViewModel
            {
                Ordine = await _statiOPService.GetNextOrderAsync()
            };
            return View(model);
        }

        /// <summary>
        /// Crea un nuovo stato OP
        /// POST: StatiOP/Create
        /// </summary>
        /// <param name="model">Dati dello stato OP da creare</param>
        /// <returns>Redirect alla lista o vista con errori</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateStatoOPViewModel model)
        {
            if (ModelState.IsValid)
            {
                var result = await _statiOPService.CreateStatoOPAsync(model);
                
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
        /// Visualizza il form per modificare uno stato OP
        /// GET: StatiOP/Edit/5
        /// </summary>
        /// <param name="id">ID dello stato OP da modificare</param>
        /// <returns>Vista con il form di modifica</returns>
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var statoOP = await _statiOPService.GetStatoOPByIdAsync(id.Value);
            if (statoOP == null)
            {
                return NotFound();
            }

            var model = new EditStatoOPViewModel
            {
                IdStato = statoOP.IdStato,
                CodiceStato = statoOP.CodiceStato,
                DescrizioneStato = statoOP.DescrizioneStato,
                Ordine = statoOP.Ordine,
                Attivo = statoOP.Attivo
            };

            // Indica se è uno stato di sistema
            ViewBag.IsSystemState = _statiOPService.IsSystemState(statoOP.CodiceStato);

            return View(model);
        }

        /// <summary>
        /// Modifica uno stato OP esistente
        /// POST: StatiOP/Edit/5
        /// </summary>
        /// <param name="id">ID dello stato OP</param>
        /// <param name="model">Dati modificati dello stato OP</param>
        /// <returns>Redirect alla lista o vista con errori</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, EditStatoOPViewModel model)
        {
            if (id != model.IdStato)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var result = await _statiOPService.UpdateStatoOPAsync(model);
                
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

            // Ricarica flag per stato di sistema
            var statoOP = await _statiOPService.GetStatoOPByIdAsync(id);
            ViewBag.IsSystemState = statoOP != null && _statiOPService.IsSystemState(statoOP.CodiceStato);

            return View(model);
        }

        /// <summary>
        /// Visualizza la conferma di eliminazione di uno stato OP
        /// GET: StatiOP/Delete/5
        /// </summary>
        /// <param name="id">ID dello stato OP da eliminare</param>
        /// <returns>Vista con la conferma di eliminazione</returns>
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var statoOP = await _context.StatiOP
                .Include(s => s.OrdiniProduzione)
                .FirstOrDefaultAsync(s => s.IdStato == id);

            if (statoOP == null)
            {
                return NotFound();
            }

            // Verifica se può essere eliminato
            var canDelete = await _statiOPService.CanDeleteStatoOPAsync(id.Value);
            
            var viewModel = new DeleteStatoOPViewModel
            {
                StatoOP = statoOP,
                HasOrdiniProduzione = !canDelete.CanDelete && canDelete.RelatedCount > 0,
                OrdiniProduzioneCount = canDelete.RelatedCount,
                OrdiniProduzioneDescription = canDelete.Reason,
                IsSystemState = _statiOPService.IsSystemState(statoOP.CodiceStato)
            };

            return View(viewModel);
        }

        /// <summary>
        /// Elimina definitivamente uno stato OP
        /// POST: StatiOP/Delete/5
        /// </summary>
        /// <param name="id">ID dello stato OP da eliminare</param>
        /// <returns>Redirect alla lista</returns>
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var result = await _statiOPService.DeleteStatoOPAsync(id);
            
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
        /// API per ottenere gli stati OP attivi (per dropdown, autocomplete, etc.)
        /// GET: api/stati-op/attivi
        /// </summary>
        /// <returns>Lista degli stati OP attivi</returns>
        [HttpGet]
        [Route("api/stati-op/attivi")]
        public async Task<IActionResult> GetStatiOPAttivi()
        {
            try
            {
                var statiOP = await _statiOPService.GetStatiOPAttiviAsync();
                return Json(statiOP);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante il recupero degli stati OP attivi");
                return StatusCode(500, "Errore durante il recupero degli stati OP");
            }
        }

        /// <summary>
        /// API per ottenere le statistiche degli stati OP
        /// GET: api/stati-op/stats
        /// </summary>
        /// <returns>Statistiche degli stati OP</returns>
        [HttpGet]
        [Route("api/stati-op/stats")]
        public async Task<IActionResult> GetStats()
        {
            try
            {
                var stats = await _statiOPService.GetStatsAsync();
                return Json(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante il recupero delle statistiche degli stati OP");
                return StatusCode(500, "Errore durante il recupero delle statistiche");
            }
        }

        /// <summary>
        /// API per ottenere il riepilogo stati OP per dashboard
        /// GET: api/stati-op/summary
        /// </summary>
        /// <returns>Riepilogo stati OP</returns>
        [HttpGet]
        [Route("api/stati-op/summary")]
        public async Task<IActionResult> GetSummary()
        {
            try
            {
                var summary = await _statiOPService.GetSummaryAsync();
                return Json(summary);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante il recupero del riepilogo stati OP");
                return StatusCode(500, "Errore durante il recupero del riepilogo");
            }
        }

        /// <summary>
        /// API per attivare/disattivare uno stato OP
        /// POST: api/stati-op/toggle-attivo/5
        /// </summary>
        /// <param name="id">ID dello stato OP</param>
        /// <returns>Risultato dell'operazione</returns>
        [HttpPost]
        [Route("api/stati-op/toggle-attivo/{id}")]
        public async Task<IActionResult> ToggleAttivo(int id)
        {
            try
            {
                var result = await _statiOPService.ToggleAttivoAsync(id);
                return Json(new { success = result.Success, message = result.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante il cambio stato dello stato OP - ID: {Id}", id);
                return Json(new { success = false, message = "Errore durante il cambio stato" });
            }
        }

        /// <summary>
        /// Visualizza la pagina per riordinare gli stati
        /// GET: StatiOP/Reorder
        /// </summary>
        /// <returns>Vista per il riordinamento</returns>
        public async Task<IActionResult> Reorder()
        {
            var stati = await _context.StatiOP
                .OrderBy(s => s.Ordine)
                .ThenBy(s => s.CodiceStato)
                .Select(s => new StatoOPOrderItem
                {
                    IdStato = s.IdStato,
                    CodiceStato = s.CodiceStato,
                    DescrizioneStato = s.DescrizioneStato,
                    Ordine = s.Ordine,
                    Attivo = s.Attivo
                })
                .ToListAsync();

            var viewModel = new ReorderStatiOPViewModel
            {
                Stati = stati
            };

            return View(viewModel);
        }

        /// <summary>
        /// Salva il nuovo ordine degli stati
        /// POST: StatiOP/Reorder
        /// </summary>
        /// <param name="model">Dati del riordinamento</param>
        /// <returns>Redirect alla lista</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reorder(ReorderStatiOPViewModel model)
        {
            if (ModelState.IsValid)
            {
                var result = await _statiOPService.ReorderStatiAsync(model.Stati);
                
                if (result.Success)
                {
                    TempData["SuccessMessage"] = result.Message;
                }
                else
                {
                    TempData["ErrorMessage"] = result.Message;
                }
            }

            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// API per il riordinamento drag & drop
        /// POST: api/stati-op/reorder
        /// </summary>
        /// <param name="stati">Lista degli stati con nuovo ordine</param>
        /// <returns>Risultato dell'operazione</returns>
        [HttpPost]
        [Route("api/stati-op/reorder")]
        public async Task<IActionResult> ApiReorder([FromBody] List<StatoOPOrderItem> stati)
        {
            try
            {
                var result = await _statiOPService.ReorderStatiAsync(stati);
                return Json(new { success = result.Success, message = result.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante il riordinamento degli stati OP");
                return Json(new { success = false, message = "Errore durante il riordinamento" });
            }
        }

        /// <summary>
        /// API per ottenere il prossimo ordine disponibile
        /// GET: api/stati-op/next-order
        /// </summary>
        /// <returns>Prossimo numero d'ordine</returns>
        [HttpGet]
        [Route("api/stati-op/next-order")]
        public async Task<IActionResult> GetNextOrder()
        {
            try
            {
                var nextOrder = await _statiOPService.GetNextOrderAsync();
                return Json(new { order = nextOrder });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante il recupero del prossimo ordine");
                return StatusCode(500, "Errore durante il recupero del prossimo ordine");
            }
        }
    }
}
