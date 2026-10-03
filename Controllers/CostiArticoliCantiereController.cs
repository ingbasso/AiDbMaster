using System.Globalization;
using AiDbMaster.Attributes;
using AiDbMaster.Data;
using AiDbMaster.Models;
using AiDbMaster.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AiDbMaster.Controllers
{
    [Authorize]
    [RegisterResource("CostiArticoliCantiere", "Costi articoli",
        Description = "Costi unitari e prezzi medi usati nella contabilità cantieri",
        MenuIcon = "bi-cash-coin", MenuOrder = 13)]
    [RequirePermission("CostiArticoliCantiere", "View")]
    public class CostiArticoliCantiereController : Controller
    {
        private static readonly CultureInfo Italiano = CultureInfo.GetCultureInfo("it-IT");
        private readonly ApplicationDbContext _context;

        public CostiArticoliCantiereController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string? q)
        {
            ViewData["Title"] = "Costi articoli";
            var testo = q?.Trim();
            ViewBag.Ricerca = testo;

            var query = _context.CostiArticoliCantiere.AsNoTracking();
            if (!string.IsNullOrEmpty(testo))
            {
                query = query.Where(c =>
                    c.CodiceArticolo.Contains(testo) ||
                    c.Descrizione.Contains(testo));
            }

            var righe = await query
                .OrderBy(c => c.CodiceArticolo)
                .ToListAsync();

            return View(righe);
        }

        [RequirePermission("CostiArticoliCantiere", "Create")]
        public IActionResult Create()
        {
            ViewData["Title"] = "Nuovo costo articolo";
            return View(new CostoArticoloCantiereForm());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission("CostiArticoliCantiere", "Create")]
        public async Task<IActionResult> Create(CostoArticoloCantiereForm model)
        {
            var voce = await PreparaAsync(model, null);
            if (!ModelState.IsValid || voce == null)
            {
                ViewData["Title"] = "Nuovo costo articolo";
                return View(model);
            }

            _context.CostiArticoliCantiere.Add(voce);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Costo articolo creato.";
            return RedirectToAction(nameof(Index), new { q = voce.CodiceArticolo });
        }

        [RequirePermission("CostiArticoliCantiere", "Edit")]
        public async Task<IActionResult> Edit(int id)
        {
            var voce = await _context.CostiArticoliCantiere.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id);
            if (voce == null)
            {
                TempData["ErrorMessage"] = "Articolo non trovato.";
                return RedirectToAction(nameof(Index));
            }

            ViewData["Title"] = "Modifica costo articolo";
            return View(ToForm(voce));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission("CostiArticoliCantiere", "Edit")]
        public async Task<IActionResult> Edit(int id, CostoArticoloCantiereForm model)
        {
            var existing = await _context.CostiArticoliCantiere.FirstOrDefaultAsync(c => c.Id == id);
            if (existing == null)
            {
                TempData["ErrorMessage"] = "Articolo non trovato.";
                return RedirectToAction(nameof(Index));
            }

            var voce = await PreparaAsync(model, id);
            if (!ModelState.IsValid || voce == null)
            {
                model.Id = id;
                ViewData["Title"] = "Modifica costo articolo";
                return View(model);
            }

            existing.CodiceArticolo = voce.CodiceArticolo;
            existing.Descrizione = voce.Descrizione;
            existing.CostoUnitario = voce.CostoUnitario;
            existing.PrezzoMedioVendita = voce.PrezzoMedioVendita;
            existing.DataUltimaModifica = voce.DataUltimaModifica;
            existing.UtenteModifica = voce.UtenteModifica;
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Costo articolo aggiornato.";
            return RedirectToAction(nameof(Index), new { q = existing.CodiceArticolo });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission("CostiArticoliCantiere", "Delete")]
        public async Task<IActionResult> Delete(int id)
        {
            var voce = await _context.CostiArticoliCantiere.FirstOrDefaultAsync(c => c.Id == id);
            if (voce == null)
            {
                TempData["ErrorMessage"] = "Articolo non trovato.";
                return RedirectToAction(nameof(Index));
            }

            _context.CostiArticoliCantiere.Remove(voce);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Costo articolo eliminato. Le contabilità già salvate non cambiano.";
            return RedirectToAction(nameof(Index));
        }

        private static CostoArticoloCantiereForm ToForm(CostoArticoloCantiere voce)
        {
            return new CostoArticoloCantiereForm
            {
                Id = voce.Id,
                CodiceArticolo = voce.CodiceArticolo,
                Descrizione = voce.Descrizione,
                CostoUnitario = voce.CostoUnitario.ToString("0.00", Italiano),
                PrezzoMedioVendita = voce.PrezzoMedioVendita.ToString("0.00", Italiano)
            };
        }

        private async Task<CostoArticoloCantiere?> PreparaAsync(CostoArticoloCantiereForm model, int? escludiId)
        {
            model.CodiceArticolo = model.CodiceArticolo?.Trim() ?? string.Empty;
            model.Descrizione = model.Descrizione?.Trim() ?? string.Empty;

            if (!string.IsNullOrEmpty(model.CodiceArticolo))
            {
                var duplicato = _context.CostiArticoliCantiere.AsNoTracking()
                    .Where(c => c.CodiceArticolo == model.CodiceArticolo);
                if (escludiId.HasValue)
                    duplicato = duplicato.Where(c => c.Id != escludiId.Value);
                if (await duplicato.AnyAsync())
                    ModelState.AddModelError(nameof(model.CodiceArticolo), "Questo codice è già presente.");
            }

            var costo = LeggiImporto(model.CostoUnitario, nameof(model.CostoUnitario), "Il costo unitario");
            var prezzo = LeggiImporto(model.PrezzoMedioVendita, nameof(model.PrezzoMedioVendita), "Il prezzo medio");
            if (!ModelState.IsValid || !costo.HasValue || !prezzo.HasValue)
                return null;

            return new CostoArticoloCantiere
            {
                CodiceArticolo = model.CodiceArticolo,
                Descrizione = model.Descrizione,
                CostoUnitario = costo.Value,
                PrezzoMedioVendita = prezzo.Value,
                DataUltimaModifica = DateTime.Now,
                UtenteModifica = User.Identity?.Name
            };
        }

        private decimal? LeggiImporto(string? testo, string campo, string etichetta)
        {
            if (string.IsNullOrWhiteSpace(testo))
                return null;

            var normalizzato = testo.Trim().Replace(" ", "");
            if (normalizzato.Contains(',') && normalizzato.Contains('.'))
                normalizzato = normalizzato.Replace(".", "").Replace(',', '.');
            else
                normalizzato = normalizzato.Replace(',', '.');

            if (!decimal.TryParse(normalizzato, NumberStyles.Number, CultureInfo.InvariantCulture, out var valore))
            {
                ModelState.AddModelError(campo, etichetta + " non è un numero. Esempio: 13,15.");
                return null;
            }

            return Math.Round(valore, 2, MidpointRounding.AwayFromZero);
        }
    }
}
