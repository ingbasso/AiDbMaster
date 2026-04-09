using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using AiDbMaster.Data;
using AiDbMaster.Models;

namespace AiDbMaster.Controllers
{
    [Authorize(Roles = UserRoles.Admin)]
    public class TabellaOpzioniController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<TabellaOpzioniController> _logger;

        public TabellaOpzioniController(ApplicationDbContext context, ILogger<TabellaOpzioniController> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IActionResult> Index(string? search)
        {
            var query = _context.Opzioni.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(o =>
                    o.NomeOpzione.Contains(search) ||
                    (o.ValoreOpzione != null && o.ValoreOpzione.Contains(search)));
            }

            var opzioni = await query.OrderBy(o => o.NomeOpzione).ToListAsync();

            ViewBag.CurrentSearch = search;
            return View(opzioni);
        }

        public IActionResult Create()
        {
            return View(new Opzione());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Opzione opzione)
        {
            if (!ModelState.IsValid)
                return View(opzione);

            var esiste = await _context.Opzioni.AnyAsync(o => o.NomeOpzione == opzione.NomeOpzione);
            if (esiste)
            {
                ModelState.AddModelError("NomeOpzione", "Esiste già un'opzione con questo nome.");
                return View(opzione);
            }

            _context.Opzioni.Add(opzione);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Opzione creata: {Nome} = {Valore}", opzione.NomeOpzione, opzione.ValoreOpzione);
            TempData["SuccessMessage"] = $"Opzione \"{opzione.NomeOpzione}\" creata con successo.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var opzione = await _context.Opzioni.FindAsync(id);
            if (opzione == null)
                return NotFound();

            return View(opzione);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Opzione opzione)
        {
            if (id != opzione.ID)
                return NotFound();

            if (!ModelState.IsValid)
                return View(opzione);

            var duplicato = await _context.Opzioni
                .AnyAsync(o => o.NomeOpzione == opzione.NomeOpzione && o.ID != id);
            if (duplicato)
            {
                ModelState.AddModelError("NomeOpzione", "Esiste già un'altra opzione con questo nome.");
                return View(opzione);
            }

            try
            {
                _context.Update(opzione);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Opzione modificata: {Nome} = {Valore}", opzione.NomeOpzione, opzione.ValoreOpzione);
                TempData["SuccessMessage"] = $"Opzione \"{opzione.NomeOpzione}\" modificata con successo.";
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.Opzioni.AnyAsync(o => o.ID == id))
                    return NotFound();
                throw;
            }

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var opzione = await _context.Opzioni.FindAsync(id);
            if (opzione == null)
                return NotFound();

            return View(opzione);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var opzione = await _context.Opzioni.FindAsync(id);
            if (opzione == null)
                return NotFound();

            _context.Opzioni.Remove(opzione);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Opzione eliminata: {Nome}", opzione.NomeOpzione);
            TempData["SuccessMessage"] = $"Opzione \"{opzione.NomeOpzione}\" eliminata con successo.";
            return RedirectToAction(nameof(Index));
        }
    }
}
