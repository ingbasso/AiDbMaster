using AiDbMaster.Attributes;
using AiDbMaster.Data;
using AiDbMaster.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AiDbMaster.Controllers
{
    [Authorize]
    [RegisterResource("ImpresePosa", "Imprese di posa",
        Description = "Anagrafica imprese di posa esterne",
        MenuIcon = "bi-person-workspace", MenuOrder = 11)]
    [RequirePermission("ImpresePosa", "View")]
    public class ImpresePosaController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ImpresePosaController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            ViewData["Title"] = "Imprese di posa";
            var imprese = await _context.ImpresePosa
                .AsNoTracking()
                .OrderBy(i => i.Nome)
                .ToListAsync();
            return View(imprese);
        }

        [RequirePermission("ImpresePosa", "Create")]
        public IActionResult Create()
        {
            ViewData["Title"] = "Nuova impresa di posa";
            return View(new ImpresaPosa());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission("ImpresePosa", "Create")]
        public async Task<IActionResult> Create(ImpresaPosa model)
        {
            Normalizza(model);
            if (await NomeDuplicatoAsync(model.Nome, null))
                ModelState.AddModelError(nameof(model.Nome), "Esiste già un'impresa con questo nome.");

            if (!ModelState.IsValid)
            {
                ViewData["Title"] = "Nuova impresa di posa";
                return View(model);
            }

            _context.ImpresePosa.Add(model);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Impresa di posa creata.";
            return RedirectToAction(nameof(Index));
        }

        [RequirePermission("ImpresePosa", "Edit")]
        public async Task<IActionResult> Edit(int id)
        {
            var impresa = await _context.ImpresePosa.FindAsync(id);
            if (impresa == null)
            {
                TempData["ErrorMessage"] = "Impresa non trovata.";
                return RedirectToAction(nameof(Index));
            }

            ViewData["Title"] = "Modifica impresa di posa";
            return View(impresa);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission("ImpresePosa", "Edit")]
        public async Task<IActionResult> Edit(int id, ImpresaPosa model)
        {
            var existing = await _context.ImpresePosa.FindAsync(id);
            if (existing == null)
            {
                TempData["ErrorMessage"] = "Impresa non trovata.";
                return RedirectToAction(nameof(Index));
            }

            Normalizza(model);
            if (await NomeDuplicatoAsync(model.Nome, id))
                ModelState.AddModelError(nameof(model.Nome), "Esiste già un'impresa con questo nome.");

            if (!ModelState.IsValid)
            {
                model.Id = id;
                ViewData["Title"] = "Modifica impresa di posa";
                return View(model);
            }

            existing.Nome = model.Nome;
            existing.Telefono = model.Telefono;
            existing.Note = model.Note;
            existing.Attivo = model.Attivo;
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Impresa di posa aggiornata.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission("ImpresePosa", "Delete")]
        public async Task<IActionResult> Delete(int id)
        {
            var impresa = await _context.ImpresePosa.FindAsync(id);
            if (impresa == null)
            {
                TempData["ErrorMessage"] = "Impresa non trovata.";
                return RedirectToAction(nameof(Index));
            }

            var usata = await _context.Cantieri.AnyAsync(c => c.ImpresaPosaId == id);
            if (usata)
            {
                TempData["ErrorMessage"] = "Impossibile eliminare: l'impresa è collegata a uno o più cantieri. Puoi disattivarla.";
                return RedirectToAction(nameof(Index));
            }

            _context.ImpresePosa.Remove(impresa);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Impresa di posa eliminata.";
            return RedirectToAction(nameof(Index));
        }

        private async Task<bool> NomeDuplicatoAsync(string nome, int? escludiId)
        {
            var query = _context.ImpresePosa.AsNoTracking().Where(i => i.Nome == nome);
            if (escludiId.HasValue)
                query = query.Where(i => i.Id != escludiId.Value);
            return await query.AnyAsync();
        }

        private static void Normalizza(ImpresaPosa model)
        {
            model.Nome = model.Nome?.Trim() ?? string.Empty;
            model.Telefono = string.IsNullOrWhiteSpace(model.Telefono) ? null : model.Telefono.Trim();
            model.Note = string.IsNullOrWhiteSpace(model.Note) ? null : model.Note.Trim();
        }
    }
}
