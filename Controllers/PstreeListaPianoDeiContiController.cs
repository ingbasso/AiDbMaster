using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using AiDbMaster.Data;
using AiDbMaster.Models;
using Microsoft.AspNetCore.Authorization;

namespace AiDbMaster.Controllers
{
    /// <summary>
    /// Controller per la gestione del Piano dei Conti
    /// </summary>
    [Authorize]
    public class PstreeListaPianoDeiContiController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<PstreeListaPianoDeiContiController> _logger;

        public PstreeListaPianoDeiContiController(ApplicationDbContext context, ILogger<PstreeListaPianoDeiContiController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: ListaPianoDeiConti
        public async Task<IActionResult> Index(string? tipoPdC, string? searchTerm, bool? nonAssociare)
        {
            var query = _context.PstreeListaPianoDeiConti.AsQueryable();

            // Filtro per tipo
            if (!string.IsNullOrEmpty(tipoPdC))
            {
                query = query.Where(p => p.TipoPdC == tipoPdC);
            }

            // Filtro per NonAssociare
            if (nonAssociare.HasValue)
            {
                query = query.Where(p => p.NonAssociare == nonAssociare.Value);
            }

            // Ricerca per codice o descrizione
            if (!string.IsNullOrEmpty(searchTerm))
            {
                searchTerm = searchTerm.ToLower();
                query = query.Where(p => 
                    p.CodicePdC.ToLower().Contains(searchTerm) || 
                    p.DescrizionePdC.ToLower().Contains(searchTerm));
            }

            var conti = await query
                .OrderBy(p => p.CodicePdC)
                .ToListAsync();

            // Popola dropdown tipo
            PopulateTipoDropdown(tipoPdC);

            // Statistiche
            var totaleConti = await _context.PstreeListaPianoDeiConti.CountAsync();
            var contiPatrimoniali = await _context.PstreeListaPianoDeiConti.CountAsync(p => p.TipoPdC == "P");
            var contiEconomici = await _context.PstreeListaPianoDeiConti.CountAsync(p => p.TipoPdC == "E");
            var contiNonAssociare = await _context.PstreeListaPianoDeiConti.CountAsync(p => p.NonAssociare);

            ViewBag.TotaleConti = totaleConti;
            ViewBag.ContiPatrimoniali = contiPatrimoniali;
            ViewBag.ContiEconomici = contiEconomici;
            ViewBag.ContiNonAssociare = contiNonAssociare;
            ViewBag.CurrentTipo = tipoPdC;
            ViewBag.CurrentSearch = searchTerm;
            ViewBag.CurrentNonAssociare = nonAssociare;

            return View(conti);
        }

        // GET: ListaPianoDeiConti/Details/ABC123
        public async Task<IActionResult> Details(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var conto = await _context.PstreeListaPianoDeiConti
                .FirstOrDefaultAsync(p => p.CodicePdC == id);

            if (conto == null)
            {
                return NotFound();
            }

            return View(conto);
        }

        // GET: ListaPianoDeiConti/Create
        public IActionResult Create()
        {
            PopulateTipoDropdown();
            return View();
        }

        // POST: ListaPianoDeiConti/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("CodicePdC,DescrizionePdC,TipoPdC,NonAssociare")] PstreeListaPianoDeiConti conto)
        {
            if (ModelState.IsValid)
            {
                // Verifica che il codice non esista già
                if (await _context.PstreeListaPianoDeiConti.AnyAsync(p => p.CodicePdC == conto.CodicePdC))
                {
                    ModelState.AddModelError("CodicePdC", "Questo codice è già utilizzato.");
                    PopulateTipoDropdown(conto.TipoPdC);
                    return View(conto);
                }

                _context.Add(conto);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Conto '{conto.CodicePdC} - {conto.DescrizionePdC}' creato con successo!";
                _logger.LogInformation("Conto PdC {CodicePdC} creato", conto.CodicePdC);

                return RedirectToAction(nameof(Index));
            }

            PopulateTipoDropdown(conto.TipoPdC);
            return View(conto);
        }

        // GET: ListaPianoDeiConti/Edit/ABC123
        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var conto = await _context.PstreeListaPianoDeiConti.FindAsync(id);
            if (conto == null)
            {
                return NotFound();
            }

            PopulateTipoDropdown(conto.TipoPdC);
            return View(conto);
        }

        // POST: ListaPianoDeiConti/Edit/ABC123
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("CodicePdC,DescrizionePdC,TipoPdC,NonAssociare")] PstreeListaPianoDeiConti conto)
        {
            if (id != conto.CodicePdC)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(conto);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = $"Conto '{conto.CodicePdC}' modificato con successo!";
                    _logger.LogInformation("Conto PdC {CodicePdC} modificato", conto.CodicePdC);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ContoExists(conto.CodicePdC))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }

            PopulateTipoDropdown(conto.TipoPdC);
            return View(conto);
        }

        // GET: ListaPianoDeiConti/Delete/ABC123
        public async Task<IActionResult> Delete(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var conto = await _context.PstreeListaPianoDeiConti
                .FirstOrDefaultAsync(p => p.CodicePdC == id);

            if (conto == null)
            {
                return NotFound();
            }

            return View(conto);
        }

        // POST: ListaPianoDeiConti/Delete/ABC123
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var conto = await _context.PstreeListaPianoDeiConti.FindAsync(id);
            if (conto == null)
            {
                return NotFound();
            }

            var codice = conto.CodicePdC;
            _context.PstreeListaPianoDeiConti.Remove(conto);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Conto '{codice}' eliminato con successo!";
            _logger.LogInformation("Conto PdC {CodicePdC} eliminato", codice);

            return RedirectToAction(nameof(Index));
        }

        private bool ContoExists(string id)
        {
            return _context.PstreeListaPianoDeiConti.Any(p => p.CodicePdC == id);
        }

        private void PopulateTipoDropdown(string? selectedValue = null)
        {
            var tipi = new List<SelectListItem>
            {
                new SelectListItem { Value = "", Text = "-- Tutti --" },
                new SelectListItem { Value = "P", Text = "P - Patrimoniale", Selected = selectedValue == "P" },
                new SelectListItem { Value = "E", Text = "E - Economico", Selected = selectedValue == "E" }
            };

            ViewBag.TipiPdC = tipi;
        }
    }
}
