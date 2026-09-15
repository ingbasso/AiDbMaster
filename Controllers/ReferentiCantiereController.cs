using AiDbMaster.Attributes;
using AiDbMaster.Data;
using AiDbMaster.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AiDbMaster.Controllers
{
    [Authorize]
    [RegisterResource("ReferentiCantiere", "Referenti di cantiere",
        Description = "Anagrafica referenti di cantiere",
        MenuIcon = "bi-person-vcard", MenuOrder = 12)]
    [RequirePermission("ReferentiCantiere", "View")]
    public class ReferentiCantiereController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ReferentiCantiereController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            ViewData["Title"] = "Referenti di cantiere";
            var referenti = await _context.ReferentiCantiere
                .AsNoTracking()
                .OrderBy(r => r.Nome)
                .ToListAsync();
            return View(referenti);
        }

        [RequirePermission("ReferentiCantiere", "Create")]
        public IActionResult Create()
        {
            ViewData["Title"] = "Nuovo referente";
            return View(new ReferenteCantiere());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission("ReferentiCantiere", "Create")]
        public async Task<IActionResult> Create(ReferenteCantiere model)
        {
            Normalizza(model);
            if (!ModelState.IsValid)
            {
                ViewData["Title"] = "Nuovo referente";
                return View(model);
            }

            _context.ReferentiCantiere.Add(model);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Referente creato.";
            return RedirectToAction(nameof(Index));
        }

        [RequirePermission("ReferentiCantiere", "Edit")]
        public async Task<IActionResult> Edit(int id)
        {
            var referente = await _context.ReferentiCantiere.FindAsync(id);
            if (referente == null)
            {
                TempData["ErrorMessage"] = "Referente non trovato.";
                return RedirectToAction(nameof(Index));
            }

            ViewData["Title"] = "Modifica referente";
            return View(referente);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission("ReferentiCantiere", "Edit")]
        public async Task<IActionResult> Edit(int id, ReferenteCantiere model)
        {
            var existing = await _context.ReferentiCantiere.FindAsync(id);
            if (existing == null)
            {
                TempData["ErrorMessage"] = "Referente non trovato.";
                return RedirectToAction(nameof(Index));
            }

            Normalizza(model);
            if (!ModelState.IsValid)
            {
                model.Id = id;
                ViewData["Title"] = "Modifica referente";
                return View(model);
            }

            existing.Nome = model.Nome;
            existing.Telefono = model.Telefono;
            existing.Email = model.Email;
            existing.Note = model.Note;
            existing.Attivo = model.Attivo;
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Referente aggiornato.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission("ReferentiCantiere", "Delete")]
        public async Task<IActionResult> Delete(int id)
        {
            var referente = await _context.ReferentiCantiere.FindAsync(id);
            if (referente == null)
            {
                TempData["ErrorMessage"] = "Referente non trovato.";
                return RedirectToAction(nameof(Index));
            }

            var usato = await _context.CantiereReferenti.AnyAsync(r => r.ReferenteId == id);
            if (usato)
            {
                TempData["ErrorMessage"] = "Impossibile eliminare: il referente è collegato a uno o più cantieri. Puoi disattivarlo.";
                return RedirectToAction(nameof(Index));
            }

            _context.ReferentiCantiere.Remove(referente);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Referente eliminato.";
            return RedirectToAction(nameof(Index));
        }

        private static void Normalizza(ReferenteCantiere model)
        {
            model.Nome = model.Nome?.Trim() ?? string.Empty;
            model.Telefono = string.IsNullOrWhiteSpace(model.Telefono) ? null : model.Telefono.Trim();
            model.Email = string.IsNullOrWhiteSpace(model.Email) ? null : model.Email.Trim();
            model.Note = string.IsNullOrWhiteSpace(model.Note) ? null : model.Note.Trim();
        }
    }
}
