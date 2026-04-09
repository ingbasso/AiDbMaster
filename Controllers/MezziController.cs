using AiDbMaster.Attributes;
using AiDbMaster.Data;
using AiDbMaster.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AiDbMaster.Controllers
{
    [Authorize]
    [RegisterResource("Mezzi", "Mezzi Interni", Description = "Anagrafica mezzi di trasporto interni", MenuIcon = "bi-truck-front", MenuOrder = 3)]
    [RequirePermission("Mezzi", "View")]
    public class MezziController : Controller
    {
        private readonly ApplicationDbContext _context;

        public MezziController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var mezzi = await _context.MezziTrasporto
                .AsNoTracking()
                .Include(m => m.AutistaDefault)
                .OrderBy(m => m.Descrizione)
                .ToListAsync();
            return View(mezzi);
        }

        [RequirePermission("Mezzi", "Create")]
        public async Task<IActionResult> Create()
        {
            ViewBag.Autisti = await GetAutistiListAsync();
            return View(new MezzoTrasporto());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission("Mezzi", "Create")]
        public async Task<IActionResult> Create(MezzoTrasporto model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Autisti = await GetAutistiListAsync();
                return View(model);
            }

            _context.MezziTrasporto.Add(model);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Mezzo interno creato con successo.";
            return RedirectToAction(nameof(Index));
        }

        [RequirePermission("Mezzi", "Edit")]
        public async Task<IActionResult> Edit(int id)
        {
            var mezzo = await _context.MezziTrasporto.FindAsync(id);
            if (mezzo == null)
            {
                TempData["ErrorMessage"] = "Mezzo non trovato.";
                return RedirectToAction(nameof(Index));
            }
            ViewBag.Autisti = await GetAutistiListAsync();
            return View(mezzo);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission("Mezzi", "Edit")]
        public async Task<IActionResult> Edit(int id, MezzoTrasporto model)
        {
            var existing = await _context.MezziTrasporto.FindAsync(id);
            if (existing == null)
            {
                TempData["ErrorMessage"] = "Mezzo non trovato.";
                return RedirectToAction(nameof(Index));
            }

            existing.CodiceMezzo = model.CodiceMezzo;
            existing.Targa = model.Targa;
            existing.Descrizione = model.Descrizione;
            existing.PortataMaxKg = model.PortataMaxKg;
            existing.Attivo = model.Attivo;
            existing.Gru = model.Gru;
            existing.Trasbordo = model.Trasbordo;
            existing.AutistaDefaultId = model.AutistaDefaultId;

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Mezzo interno aggiornato con successo.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission("Mezzi", "Delete")]
        public async Task<IActionResult> Delete(int id)
        {
            var mezzo = await _context.MezziTrasporto.FindAsync(id);
            if (mezzo == null)
            {
                TempData["ErrorMessage"] = "Mezzo non trovato.";
                return RedirectToAction(nameof(Index));
            }

            var usato = await _context.ViaggiConsegna.AnyAsync(v => v.MezzoTrasportoId == id);
            if (usato)
            {
                TempData["ErrorMessage"] = "Impossibile eliminare: il mezzo è utilizzato in uno o più viaggi.";
                return RedirectToAction(nameof(Index));
            }

            _context.MezziTrasporto.Remove(mezzo);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Mezzo interno eliminato con successo.";
            return RedirectToAction(nameof(Index));
        }

        private async Task<List<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem>> GetAutistiListAsync()
        {
            return await _context.Autisti
                .AsNoTracking()
                .Where(a => a.Attivo)
                .OrderBy(a => a.Cognome)
                .ThenBy(a => a.Nome)
                .Select(a => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
                {
                    Value = a.Id.ToString(),
                    Text = a.Cognome + " " + a.Nome
                })
                .ToListAsync();
        }
    }
}
