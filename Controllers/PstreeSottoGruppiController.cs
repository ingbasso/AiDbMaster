using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using AiDbMaster.Data;
using AiDbMaster.Models;
using Microsoft.AspNetCore.Authorization;

namespace AiDbMaster.Controllers
{
    [Authorize]
    public class PstreeSottoGruppiController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PstreeSottoGruppiController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: SottoGruppi
        public async Task<IActionResult> Index(string? codiceFamiglia, string? searchTerm)
        {
            var query = _context.PstreeSottoGruppi
                .Include(s => s.Famiglia)
                .AsQueryable();

            // Filtro per famiglia
            if (!string.IsNullOrEmpty(codiceFamiglia))
            {
                query = query.Where(s => s.CodiceFamiglia == codiceFamiglia);
            }

            // Ricerca testuale
            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(s => 
                    s.NomeSottoGruppo.Contains(searchTerm) || 
                    s.CodiceGruppo.Contains(searchTerm) ||
                    (s.DescrizioneSottoGruppo != null && s.DescrizioneSottoGruppo.Contains(searchTerm)));
            }

            var sottoGruppi = await query
                .OrderBy(s => s.CodiceFamiglia)
                .ThenBy(s => s.CodiceGruppo)
                .ToListAsync();

            // Carica liste per filtri
            ViewBag.Famiglie = await _context.PstreeListaFamiglie
                .OrderBy(f => f.CodiceFamiglia)
                .Select(f => new SelectListItem
                {
                    Value = f.CodiceFamiglia,
                    Text = $"{f.CodiceFamiglia} - {f.NomeFamiglia}",
                    Selected = f.CodiceFamiglia == codiceFamiglia
                })
                .ToListAsync();

            ViewBag.CodiceFamiglia = codiceFamiglia;
            ViewBag.SearchTerm = searchTerm;

            return View(sottoGruppi);
        }

        // GET: SottoGruppi/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var sottoGruppo = await _context.PstreeSottoGruppi
                .Include(s => s.Famiglia)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (sottoGruppo == null)
            {
                return NotFound();
            }

            return View(sottoGruppo);
        }

        // GET: SottoGruppi/Create
        public async Task<IActionResult> Create(string? codiceFamiglia)
        {
            await LoadFamiglieDropdownAsync(codiceFamiglia);
            
            var model = new PstreeSottoGruppi();
            if (!string.IsNullOrEmpty(codiceFamiglia))
            {
                model.CodiceFamiglia = codiceFamiglia;
            }
            
            return View(model);
        }

        // POST: SottoGruppi/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("CodiceFamiglia,CodiceGruppo,NomeSottoGruppo,DescrizioneSottoGruppo")] PstreeSottoGruppi sottoGruppo)
        {
            if (ModelState.IsValid)
            {
                // Verifica duplicato (CodiceFamiglia + CodiceGruppo)
                var exists = await _context.PstreeSottoGruppi
                    .AnyAsync(s => s.CodiceFamiglia == sottoGruppo.CodiceFamiglia && 
                                   s.CodiceGruppo == sottoGruppo.CodiceGruppo);
                
                if (exists)
                {
                    ModelState.AddModelError("CodiceGruppo", "Esiste già un sotto-gruppo con questo codice per la famiglia selezionata.");
                    await LoadFamiglieDropdownAsync(sottoGruppo.CodiceFamiglia);
                    return View(sottoGruppo);
                }

                _context.Add(sottoGruppo);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Sotto-gruppo creato con successo!";
                return RedirectToAction(nameof(Index));
            }

            await LoadFamiglieDropdownAsync(sottoGruppo.CodiceFamiglia);
            return View(sottoGruppo);
        }

        // GET: SottoGruppi/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var sottoGruppo = await _context.PstreeSottoGruppi.FindAsync(id);
            if (sottoGruppo == null)
            {
                return NotFound();
            }

            await LoadFamiglieDropdownAsync(sottoGruppo.CodiceFamiglia);
            return View(sottoGruppo);
        }

        // POST: SottoGruppi/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,CodiceFamiglia,CodiceGruppo,NomeSottoGruppo,DescrizioneSottoGruppo")] PstreeSottoGruppi sottoGruppo)
        {
            if (id != sottoGruppo.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                // Verifica duplicato (escludendo se stesso)
                var exists = await _context.PstreeSottoGruppi
                    .AnyAsync(s => s.CodiceFamiglia == sottoGruppo.CodiceFamiglia && 
                                   s.CodiceGruppo == sottoGruppo.CodiceGruppo &&
                                   s.Id != id);
                
                if (exists)
                {
                    ModelState.AddModelError("CodiceGruppo", "Esiste già un sotto-gruppo con questo codice per la famiglia selezionata.");
                    await LoadFamiglieDropdownAsync(sottoGruppo.CodiceFamiglia);
                    return View(sottoGruppo);
                }

                try
                {
                    _context.Update(sottoGruppo);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Sotto-gruppo modificato con successo!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SottoGruppoExists(sottoGruppo.Id))
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

            await LoadFamiglieDropdownAsync(sottoGruppo.CodiceFamiglia);
            return View(sottoGruppo);
        }

        // GET: SottoGruppi/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var sottoGruppo = await _context.PstreeSottoGruppi
                .Include(s => s.Famiglia)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (sottoGruppo == null)
            {
                return NotFound();
            }

            return View(sottoGruppo);
        }

        // POST: SottoGruppi/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var sottoGruppo = await _context.PstreeSottoGruppi.FindAsync(id);
            if (sottoGruppo != null)
            {
                _context.PstreeSottoGruppi.Remove(sottoGruppo);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Sotto-gruppo eliminato con successo!";
            }

            return RedirectToAction(nameof(Index));
        }

        // API: Ottiene sotto-gruppi per famiglia (per dropdown dinamici)
        [HttpGet]
        public async Task<IActionResult> GetByFamiglia(string codiceFamiglia)
        {
            var sottoGruppi = await _context.PstreeSottoGruppi
                .Where(s => s.CodiceFamiglia == codiceFamiglia)
                .OrderBy(s => s.CodiceGruppo)
                .Select(s => new { s.Id, s.CodiceGruppo, s.NomeSottoGruppo })
                .ToListAsync();

            return Json(sottoGruppi);
        }

        private bool SottoGruppoExists(int id)
        {
            return _context.PstreeSottoGruppi.Any(e => e.Id == id);
        }

        private async Task LoadFamiglieDropdownAsync(string? selectedCodice = null)
        {
            ViewBag.Famiglie = await _context.PstreeListaFamiglie
                .OrderBy(f => f.CodiceFamiglia)
                .Select(f => new SelectListItem
                {
                    Value = f.CodiceFamiglia,
                    Text = $"{f.CodiceFamiglia} - {f.NomeFamiglia}",
                    Selected = f.CodiceFamiglia == selectedCodice
                })
                .ToListAsync();
        }
    }
}
