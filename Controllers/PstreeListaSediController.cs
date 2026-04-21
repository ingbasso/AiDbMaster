using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AiDbMaster.Data;
using AiDbMaster.Models;
using Microsoft.AspNetCore.Authorization;

namespace AiDbMaster.Controllers
{
    /// <summary>
    /// Controller per la gestione delle Sedi aziendali
    /// </summary>
    [Authorize]
    public class PstreeListaSediController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<PstreeListaSediController> _logger;

        public PstreeListaSediController(ApplicationDbContext context, ILogger<PstreeListaSediController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: ListaSedi
        public async Task<IActionResult> Index()
        {
            var sedi = await _context.PstreeListaSedi
                .OrderBy(s => s.Id)
                .ToListAsync();

            return View(sedi);
        }

        // GET: ListaSedi/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var sede = await _context.PstreeListaSedi
                .FirstOrDefaultAsync(s => s.Id == id);

            if (sede == null)
            {
                return NotFound();
            }

            // Conta quanti saldi usano questa sede
            var countSaldi = await _context.PstreeListaSaldi
                .CountAsync(s => s.IdSede == sede.Id);
            ViewBag.CountSaldi = countSaldi;

            // Conta quante rettifiche usano questa sede
            var countRettifiche = await _context.PstreeListaRettifiche
                .CountAsync(r => r.IdSede == sede.Id);
            ViewBag.CountRettifiche = countRettifiche;

            return View(sede);
        }

        // GET: ListaSedi/Create
        public async Task<IActionResult> Create()
        {
            // Suggerisci il prossimo ID disponibile
            var maxId = await _context.PstreeListaSedi
                .MaxAsync(s => (int?)s.Id) ?? 0;

            var model = new PstreeListaSedi
            {
                Id = maxId + 1
            };

            return View(model);
        }

        // POST: ListaSedi/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Sede,DescrizioneSede")] PstreeListaSedi sedeModel)
        {
            if (ModelState.IsValid)
            {
                // Verifica che l'ID non esista già
                if (await _context.PstreeListaSedi.AnyAsync(s => s.Id == sedeModel.Id))
                {
                    ModelState.AddModelError("Id", "Questo ID è già utilizzato.");
                    return View(sedeModel);
                }

                // Verifica che il nome non esista già
                if (await _context.PstreeListaSedi.AnyAsync(s => s.Sede == sedeModel.Sede))
                {
                    ModelState.AddModelError("Sede", "Questo nome sede è già utilizzato.");
                    return View(sedeModel);
                }

                _context.Add(sedeModel);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Sede '{sedeModel.Sede}' creata con successo!";
                _logger.LogInformation("Sede {Id} - {Sede} creata", sedeModel.Id, sedeModel.Sede);

                return RedirectToAction(nameof(Index));
            }

            return View(sedeModel);
        }

        // GET: ListaSedi/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var sede = await _context.PstreeListaSedi.FindAsync(id);
            if (sede == null)
            {
                return NotFound();
            }

            return View(sede);
        }

        // POST: ListaSedi/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Sede,DescrizioneSede")] PstreeListaSedi sedeModel)
        {
            if (id != sedeModel.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Verifica che il nome non esista già (escluso il record corrente)
                    if (await _context.PstreeListaSedi.AnyAsync(s => s.Sede == sedeModel.Sede && s.Id != sedeModel.Id))
                    {
                        ModelState.AddModelError("Sede", "Questo nome sede è già utilizzato.");
                        return View(sedeModel);
                    }

                    _context.Update(sedeModel);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = $"Sede '{sedeModel.Sede}' modificata con successo!";
                    _logger.LogInformation("Sede {Id} modificata", sedeModel.Id);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SedeExists(sedeModel.Id))
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

            return View(sedeModel);
        }

        // GET: ListaSedi/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var sede = await _context.PstreeListaSedi
                .FirstOrDefaultAsync(s => s.Id == id);

            if (sede == null)
            {
                return NotFound();
            }

            // Conta utilizzi
            var countSaldi = await _context.PstreeListaSaldi
                .CountAsync(s => s.IdSede == sede.Id);
            var countRettifiche = await _context.PstreeListaRettifiche
                .CountAsync(r => r.IdSede == sede.Id);

            ViewBag.CountSaldi = countSaldi;
            ViewBag.CountRettifiche = countRettifiche;
            ViewBag.IsUsed = countSaldi > 0 || countRettifiche > 0;

            return View(sede);
        }

        // POST: ListaSedi/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var sede = await _context.PstreeListaSedi.FindAsync(id);
            if (sede == null)
            {
                return NotFound();
            }

            // Verifica se è in uso
            var countSaldi = await _context.PstreeListaSaldi
                .CountAsync(s => s.IdSede == sede.Id);
            var countRettifiche = await _context.PstreeListaRettifiche
                .CountAsync(r => r.IdSede == sede.Id);

            if (countSaldi > 0 || countRettifiche > 0)
            {
                TempData["ErrorMessage"] = $"Impossibile eliminare '{sede.Sede}' perché è utilizzata in {countSaldi} saldi e {countRettifiche} rettifiche.";
                return RedirectToAction(nameof(Index));
            }

            var nome = sede.Sede;
            _context.PstreeListaSedi.Remove(sede);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Sede '{nome}' eliminata con successo!";
            _logger.LogInformation("Sede {Id} eliminata", id);

            return RedirectToAction(nameof(Index));
        }

        private bool SedeExists(int id)
        {
            return _context.PstreeListaSedi.Any(s => s.Id == id);
        }
    }
}
