using AiDbMaster.Attributes;
using AiDbMaster.Data;
using AiDbMaster.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AiDbMaster.Controllers
{
    [Authorize]
    [RegisterResource("Autisti", "Autisti", Description = "Anagrafica autisti mezzi interni", MenuIcon = "bi-person-badge", MenuOrder = 4)]
    [RequirePermission("Autisti", "View")]
    public class AutistiController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AutistiController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var autisti = await _context.Autisti
                .AsNoTracking()
                .OrderBy(a => a.Cognome)
                .ThenBy(a => a.Nome)
                .ToListAsync();
            return View(autisti);
        }

        [RequirePermission("Autisti", "Create")]
        public IActionResult Create()
        {
            return View(new Autista());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission("Autisti", "Create")]
        public async Task<IActionResult> Create(Autista model)
        {
            if (!ModelState.IsValid)
                return View(model);

            _context.Autisti.Add(model);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Autista creato con successo.";
            return RedirectToAction(nameof(Index));
        }

        [RequirePermission("Autisti", "Edit")]
        public async Task<IActionResult> Edit(int id)
        {
            var autista = await _context.Autisti.FindAsync(id);
            if (autista == null)
            {
                TempData["ErrorMessage"] = "Autista non trovato.";
                return RedirectToAction(nameof(Index));
            }
            return View(autista);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission("Autisti", "Edit")]
        public async Task<IActionResult> Edit(int id, Autista model)
        {
            var existing = await _context.Autisti.FindAsync(id);
            if (existing == null)
            {
                TempData["ErrorMessage"] = "Autista non trovato.";
                return RedirectToAction(nameof(Index));
            }

            existing.Nome = model.Nome;
            existing.Cognome = model.Cognome;
            existing.Telefono = model.Telefono;
            existing.Attivo = model.Attivo;

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Autista aggiornato con successo.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission("Autisti", "Delete")]
        public async Task<IActionResult> Delete(int id)
        {
            var autista = await _context.Autisti.FindAsync(id);
            if (autista == null)
            {
                TempData["ErrorMessage"] = "Autista non trovato.";
                return RedirectToAction(nameof(Index));
            }

            var usatoMezzo = await _context.MezziTrasporto.AnyAsync(m => m.AutistaDefaultId == id);
            var usatoViaggio = await _context.ViaggiConsegna.AnyAsync(v => v.AutistaId == id);
            if (usatoMezzo || usatoViaggio)
            {
                TempData["ErrorMessage"] = "Impossibile eliminare: l'autista è associato a mezzi o viaggi.";
                return RedirectToAction(nameof(Index));
            }

            _context.Autisti.Remove(autista);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Autista eliminato con successo.";
            return RedirectToAction(nameof(Index));
        }
    }
}
