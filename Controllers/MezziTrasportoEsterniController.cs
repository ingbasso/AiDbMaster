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
    [RegisterResource("MezziTrasportoEsterni", "Mezzi Esterni", Description = "Anagrafica mezzi di trasporto esterni", MenuIcon = "bi-truck-flatbed", MenuOrder = 5)]
    [RequirePermission("MezziTrasportoEsterni", "View")]
    public class MezziTrasportoEsterniController : Controller
    {
        private readonly ApplicationDbContext _context;

        public MezziTrasportoEsterniController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string? comune, string? provincia, string? regione, string? tipoMezzo, bool? gru, int pagina = 1)
        {
            var query = _context.MezziTrasportoEsterni.AsNoTracking().AsQueryable();

            if (!string.IsNullOrEmpty(comune))
                query = query.Where(m => m.Comune == comune);
            if (!string.IsNullOrEmpty(provincia))
                query = query.Where(m => m.Provincia == provincia);
            if (!string.IsNullOrEmpty(regione))
                query = query.Where(m => m.Regione == regione);
            if (!string.IsNullOrEmpty(tipoMezzo))
                query = query.Where(m => m.TipoMezzo == tipoMezzo);
            if (gru.HasValue)
                query = query.Where(m => m.Gru == gru.Value);

            var totale = await query.CountAsync();

            var mezzi = await query
                .OrderBy(m => m.NomeVettore)
                .ThenBy(m => m.Comune)
                .Skip((pagina - 1) * 20)
                .Take(20)
                .ToListAsync();

            var allRecords = _context.MezziTrasportoEsterni.AsNoTracking();

            var model = new MezziTrasportoEsterniIndexViewModel
            {
                Mezzi = mezzi,
                FiltroComune = comune,
                FiltroProvincia = provincia,
                FiltroRegione = regione,
                FiltroTipoMezzo = tipoMezzo,
                FiltroGru = gru,
                PaginaCorrente = pagina,
                TotaleRecord = totale,
                ComuniDisponibili = await allRecords.Where(m => m.Comune != null).Select(m => m.Comune).Distinct().OrderBy(x => x).ToListAsync(),
                ProvinceDisponibili = await allRecords.Where(m => m.Provincia != null).Select(m => m.Provincia!).Distinct().OrderBy(x => x).ToListAsync(),
                RegioniDisponibili = await allRecords.Where(m => m.Regione != null).Select(m => m.Regione!).Distinct().OrderBy(x => x).ToListAsync(),
                TipiMezzoDisponibili = await allRecords.Where(m => m.TipoMezzo != null).Select(m => m.TipoMezzo!).Distinct().OrderBy(x => x).ToListAsync()
            };

            return View(model);
        }

        [RequirePermission("MezziTrasportoEsterni", "Create")]
        public IActionResult Create()
        {
            return View(new MezzoTrasportoEsterno());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission("MezziTrasportoEsterni", "Create")]
        public async Task<IActionResult> Create(MezzoTrasportoEsterno model)
        {
            if (!ModelState.IsValid)
                return View(model);

            _context.MezziTrasportoEsterni.Add(model);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Mezzo esterno creato con successo.";
            return RedirectToAction(nameof(Index));
        }

        [RequirePermission("MezziTrasportoEsterni", "Edit")]
        public async Task<IActionResult> Edit(int id)
        {
            var mezzo = await _context.MezziTrasportoEsterni.FindAsync(id);
            if (mezzo == null)
            {
                TempData["ErrorMessage"] = "Mezzo esterno non trovato.";
                return RedirectToAction(nameof(Index));
            }
            return View(mezzo);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission("MezziTrasportoEsterni", "Edit")]
        public async Task<IActionResult> Edit(int id, MezzoTrasportoEsterno model)
        {
            var existing = await _context.MezziTrasportoEsterni.FindAsync(id);
            if (existing == null)
            {
                TempData["ErrorMessage"] = "Mezzo esterno non trovato.";
                return RedirectToAction(nameof(Index));
            }

            existing.Comune = model.Comune;
            existing.Provincia = model.Provincia;
            existing.Regione = model.Regione;
            existing.NomeVettore = model.NomeVettore;
            existing.TipoMezzo = model.TipoMezzo;
            existing.Costo = model.Costo;
            existing.PortataMax = model.PortataMax;
            existing.Gru = model.Gru;
            existing.Trasbordo = model.Trasbordo;
            existing.Note = model.Note;

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Mezzo esterno aggiornato con successo.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission("MezziTrasportoEsterni", "Delete")]
        public async Task<IActionResult> Delete(int id)
        {
            var mezzo = await _context.MezziTrasportoEsterni.FindAsync(id);
            if (mezzo == null)
            {
                TempData["ErrorMessage"] = "Mezzo esterno non trovato.";
                return RedirectToAction(nameof(Index));
            }

            var usatoInViaggi = await _context.ViaggiConsegna
                .AnyAsync(v => v.MezzoTrasportoEsternoId == id);
            if (usatoInViaggi)
            {
                TempData["ErrorMessage"] = "Impossibile eliminare: il mezzo è utilizzato in uno o più viaggi.";
                return RedirectToAction(nameof(Index));
            }

            _context.MezziTrasportoEsterni.Remove(mezzo);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Mezzo esterno eliminato con successo.";
            return RedirectToAction(nameof(Index));
        }
    }
}
