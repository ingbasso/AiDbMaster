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
    /// Controller per la gestione delle Lavorazioni
    /// Fornisce funzionalità CRUD complete per la tabella Lavorazioni
    /// </summary>
    [Authorize]
    public class LavorazioniController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly LavorazioniService _lavorazioniService;
        private readonly ILogger<LavorazioniController> _logger;

        public LavorazioniController(
            ApplicationDbContext context,
            LavorazioniService lavorazioniService,
            ILogger<LavorazioniController> logger)
        {
            _context = context;
            _lavorazioniService = lavorazioniService;
            _logger = logger;
        }

        /// <summary>
        /// Visualizza l'elenco di tutte le lavorazioni
        /// GET: Lavorazioni
        /// </summary>
        /// <param name="search">Termine di ricerca per filtrare le lavorazioni</param>
        /// <param name="attivo">Filtro per stato attivo</param>
        /// <param name="sortOrder">Ordinamento dei risultati</param>
        /// <param name="page">Numero di pagina per la paginazione</param>
        /// <param name="pageSize">Numero di elementi per pagina</param>
        /// <returns>Vista con l'elenco delle lavorazioni</returns>
        public async Task<IActionResult> Index(
            string? search,
            bool? attivo,
            string sortOrder = "descrizione",
            int page = 1,
            int pageSize = 50)
        {
            try
            {
                _logger.LogInformation("Caricamento tabella lavorazioni - Pagina: {Page}, Ricerca: {Search}", page, search);

                var viewModel = await _lavorazioniService.GetLavorazioniAsync(search, attivo, sortOrder, page, pageSize);
                
                // Preparazione dati per la vista (per compatibilità con la vista esistente)
                ViewBag.CurrentSearch = viewModel.Search;
                ViewBag.CurrentAttivo = viewModel.Attivo;
                ViewBag.CurrentSort = viewModel.SortOrder;
                ViewBag.CurrentPage = viewModel.CurrentPage;
                ViewBag.PageSize = viewModel.PageSize;
                ViewBag.TotalCount = viewModel.TotalCount;
                ViewBag.TotalPages = viewModel.TotalPages;

                // Parametri per l'ordinamento
                ViewBag.CodiceSortParm = viewModel.CodiceSortParm;
                ViewBag.DescrizioneSortParm = viewModel.DescrizioneSortParm;
                ViewBag.AttivoSortParm = viewModel.AttivoSortParm;
                ViewBag.DataSortParm = viewModel.DataSortParm;

                return View(viewModel.Lavorazioni);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante il caricamento delle lavorazioni");
                TempData["ErrorMessage"] = "Errore durante il caricamento delle lavorazioni: " + ex.Message;
                return View(new List<Lavorazioni>());
            }
        }

        /// <summary>
        /// Visualizza i dettagli di una lavorazione specifica
        /// GET: Lavorazioni/Details/5
        /// </summary>
        /// <param name="id">ID della lavorazione</param>
        /// <returns>Vista con i dettagli della lavorazione</returns>
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var viewModel = await _lavorazioniService.GetLavorazioneDetailsAsync(id.Value);
            if (viewModel == null)
            {
                return NotFound();
            }

            return View(viewModel.Lavorazione);
        }

        /// <summary>
        /// Visualizza il form per creare una nuova lavorazione
        /// GET: Lavorazioni/Create
        /// </summary>
        /// <returns>Vista con il form di creazione</returns>
        public IActionResult Create()
        {
            return View();
        }

        /// <summary>
        /// Crea una nuova lavorazione
        /// POST: Lavorazioni/Create
        /// </summary>
        /// <param name="lavorazione">Dati della lavorazione da creare</param>
        /// <returns>Redirect alla lista o vista con errori</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("CodiceLavorazione,DescrizioneLavorazione,Attivo")] Lavorazioni lavorazione)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Verifica unicità del codice se specificato
                    if (!string.IsNullOrEmpty(lavorazione.CodiceLavorazione))
                    {
                        var esistente = await _context.Lavorazioni
                            .FirstOrDefaultAsync(l => l.CodiceLavorazione == lavorazione.CodiceLavorazione);
                        
                        if (esistente != null)
                        {
                            ModelState.AddModelError("CodiceLavorazione", "Esiste già una lavorazione con questo codice.");
                            return View(lavorazione);
                        }
                    }

                    lavorazione.DataCreazione = DateTime.Now;
                    _context.Add(lavorazione);
                    await _context.SaveChangesAsync();

                    _logger.LogInformation("Lavorazione creata con successo - ID: {Id}, Descrizione: {Descrizione}", 
                        lavorazione.IdLavorazione, lavorazione.DescrizioneLavorazione);

                    TempData["SuccessMessage"] = "Lavorazione creata con successo!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Errore durante la creazione della lavorazione: {Descrizione}", 
                        lavorazione.DescrizioneLavorazione);
                    ModelState.AddModelError(string.Empty, "Errore durante la creazione della lavorazione: " + ex.Message);
                }
            }
            return View(lavorazione);
        }

        /// <summary>
        /// Visualizza il form per modificare una lavorazione
        /// GET: Lavorazioni/Edit/5
        /// </summary>
        /// <param name="id">ID della lavorazione da modificare</param>
        /// <returns>Vista con il form di modifica</returns>
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var lavorazione = await _context.Lavorazioni.FindAsync(id);
            if (lavorazione == null)
            {
                return NotFound();
            }
            return View(lavorazione);
        }

        /// <summary>
        /// Modifica una lavorazione esistente
        /// POST: Lavorazioni/Edit/5
        /// </summary>
        /// <param name="id">ID della lavorazione</param>
        /// <param name="lavorazione">Dati modificati della lavorazione</param>
        /// <returns>Redirect alla lista o vista con errori</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdLavorazione,CodiceLavorazione,DescrizioneLavorazione,Attivo,DataCreazione")] Lavorazioni lavorazione)
        {
            if (id != lavorazione.IdLavorazione)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Verifica unicità del codice se specificato
                    if (!string.IsNullOrEmpty(lavorazione.CodiceLavorazione))
                    {
                        var esistente = await _context.Lavorazioni
                            .FirstOrDefaultAsync(l => l.CodiceLavorazione == lavorazione.CodiceLavorazione && 
                                                     l.IdLavorazione != lavorazione.IdLavorazione);
                        
                        if (esistente != null)
                        {
                            ModelState.AddModelError("CodiceLavorazione", "Esiste già una lavorazione con questo codice.");
                            return View(lavorazione);
                        }
                    }

                    lavorazione.DataUltimaModifica = DateTime.Now;
                    _context.Update(lavorazione);
                    await _context.SaveChangesAsync();

                    _logger.LogInformation("Lavorazione modificata con successo - ID: {Id}, Descrizione: {Descrizione}", 
                        lavorazione.IdLavorazione, lavorazione.DescrizioneLavorazione);

                    TempData["SuccessMessage"] = "Lavorazione modificata con successo!";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!LavorazioneExists(lavorazione.IdLavorazione))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Errore durante la modifica della lavorazione - ID: {Id}", 
                        lavorazione.IdLavorazione);
                    ModelState.AddModelError(string.Empty, "Errore durante la modifica della lavorazione: " + ex.Message);
                }
            }
            return View(lavorazione);
        }

        /// <summary>
        /// Visualizza la conferma di eliminazione di una lavorazione
        /// GET: Lavorazioni/Delete/5
        /// </summary>
        /// <param name="id">ID della lavorazione da eliminare</param>
        /// <returns>Vista con la conferma di eliminazione</returns>
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var lavorazione = await _context.Lavorazioni
                .FirstOrDefaultAsync(l => l.IdLavorazione == id);
            if (lavorazione == null)
            {
                return NotFound();
            }

            return View(lavorazione);
        }

        /// <summary>
        /// Elimina definitivamente una lavorazione
        /// POST: Lavorazioni/Delete/5
        /// </summary>
        /// <param name="id">ID della lavorazione da eliminare</param>
        /// <returns>Redirect alla lista</returns>
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var lavorazione = await _context.Lavorazioni.FindAsync(id);
                if (lavorazione != null)
                {
                    _context.Lavorazioni.Remove(lavorazione);
                    await _context.SaveChangesAsync();

                    _logger.LogInformation("Lavorazione eliminata con successo - ID: {Id}, Descrizione: {Descrizione}", 
                        lavorazione.IdLavorazione, lavorazione.DescrizioneLavorazione);

                    TempData["SuccessMessage"] = "Lavorazione eliminata con successo!";
                }
                else
                {
                    TempData["ErrorMessage"] = "Lavorazione non trovata.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante l'eliminazione della lavorazione - ID: {Id}", id);
                TempData["ErrorMessage"] = "Errore durante l'eliminazione della lavorazione: " + ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// Verifica se una lavorazione esiste
        /// </summary>
        /// <param name="id">ID della lavorazione</param>
        /// <returns>True se esiste, False altrimenti</returns>
        private bool LavorazioneExists(int id)
        {
            return _context.Lavorazioni.Any(e => e.IdLavorazione == id);
        }

        /// <summary>
        /// API per ottenere le lavorazioni attive (per dropdown, autocomplete, etc.)
        /// GET: api/lavorazioni/attive
        /// </summary>
        /// <returns>Lista delle lavorazioni attive</returns>
        [HttpGet]
        [Route("api/lavorazioni/attive")]
        public async Task<IActionResult> GetLavorazioniAttive()
        {
            try
            {
                var lavorazioni = await _lavorazioniService.GetLavorazioniAttiveAsync();
                return Json(lavorazioni);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante il recupero delle lavorazioni attive");
                return StatusCode(500, "Errore durante il recupero delle lavorazioni");
            }
        }
    }
}
