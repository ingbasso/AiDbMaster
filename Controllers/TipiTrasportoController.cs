using AiDbMaster.Attributes;
using AiDbMaster.Data;
using AiDbMaster.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AiDbMaster.Controllers
{
    [Authorize]
    [RegisterResource("TipiTrasporto", "Tipi Trasporto", Description = "Anagrafica tipologie di trasporto", MenuIcon = "bi-signpost-split", MenuOrder = 4)]
    [RequirePermission("TipiTrasporto", "View")]
    public class TipiTrasportoController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TipiTrasportoController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var tipi = await _context.TipiTrasporto
                .AsNoTracking()
                .OrderBy(t => t.Descrizione)
                .ToListAsync();
            return View(tipi);
        }

        [RequirePermission("TipiTrasporto", "Create")]
        public IActionResult Create()
        {
            return View(new TipoTrasporto());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission("TipiTrasporto", "Create")]
        public async Task<IActionResult> Create(TipoTrasporto model)
        {
            if (!ModelState.IsValid)
                return View(model);

            _context.TipiTrasporto.Add(model);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Tipo trasporto creato con successo.";
            return RedirectToAction(nameof(Index));
        }

        [RequirePermission("TipiTrasporto", "Edit")]
        public async Task<IActionResult> Edit(int id)
        {
            var tipo = await _context.TipiTrasporto.FindAsync(id);
            if (tipo == null)
            {
                TempData["ErrorMessage"] = "Tipo trasporto non trovato.";
                return RedirectToAction(nameof(Index));
            }
            return View(tipo);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission("TipiTrasporto", "Edit")]
        public async Task<IActionResult> Edit(int id, TipoTrasporto model)
        {
            var existing = await _context.TipiTrasporto.FindAsync(id);
            if (existing == null)
            {
                TempData["ErrorMessage"] = "Tipo trasporto non trovato.";
                return RedirectToAction(nameof(Index));
            }

            existing.Codice = model.Codice;
            existing.Descrizione = model.Descrizione;
            existing.Attivo = model.Attivo;

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Tipo trasporto aggiornato con successo.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission("TipiTrasporto", "Delete")]
        public async Task<IActionResult> Delete(int id)
        {
            var tipo = await _context.TipiTrasporto.FindAsync(id);
            if (tipo == null)
            {
                TempData["ErrorMessage"] = "Tipo trasporto non trovato.";
                return RedirectToAction(nameof(Index));
            }

            var usato = await _context.ViaggiConsegna.AnyAsync(v => v.TipoTrasportoId == id);
            if (usato)
            {
                TempData["ErrorMessage"] = "Impossibile eliminare: il tipo è utilizzato in uno o più viaggi.";
                return RedirectToAction(nameof(Index));
            }

            _context.TipiTrasporto.Remove(tipo);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Tipo trasporto eliminato con successo.";
            return RedirectToAction(nameof(Index));
        }
    }
}
