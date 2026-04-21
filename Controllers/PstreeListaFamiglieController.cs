using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using AiDbMaster.Data;
using AiDbMaster.Models;
using Microsoft.AspNetCore.Authorization;

namespace AiDbMaster.Controllers
{
    /// <summary>
    /// Controller per la gestione delle Famiglie di Prodotti/Attività
    /// </summary>
    [Authorize]
    public class PstreeListaFamiglieController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<PstreeListaFamiglieController> _logger;

        public PstreeListaFamiglieController(ApplicationDbContext context, ILogger<PstreeListaFamiglieController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: ListaFamiglie
        public async Task<IActionResult> Index()
        {
            var famiglie = await _context.PstreeListaFamiglie
                .OrderBy(f => f.Id)
                .ToListAsync();

            // Carica dizionario conti per mostrare descrizioni
            var contiIds = famiglie.Select(f => f.IdCodiceConto).Distinct().ToList();
            var conti = await _context.PstreeStrutturaContoEconomico
                .Where(c => contiIds.Contains(c.IdCodiceConto))
                .ToDictionaryAsync(c => c.IdCodiceConto, c => c.DescrizioneCompleta);
            ViewBag.ContiDescrizioni = conti;

            return View(famiglie);
        }

        // GET: ListaFamiglie/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var famiglia = await _context.PstreeListaFamiglie
                .FirstOrDefaultAsync(f => f.Id == id);

            if (famiglia == null)
            {
                return NotFound();
            }

            // Conta quanti saldi usano questa famiglia
            var countSaldi = await _context.PstreeListaSaldi
                .CountAsync(s => s.IdFamiglia == famiglia.Id);
            ViewBag.CountSaldi = countSaldi;

            // Conta quante rettifiche usano questa famiglia
            var countRettifiche = await _context.PstreeListaRettifiche
                .CountAsync(r => r.IdFamiglia == famiglia.Id);
            ViewBag.CountRettifiche = countRettifiche;

            // Carica il conto economico associato
            var contoAssociato = await _context.PstreeStrutturaContoEconomico
                .FirstOrDefaultAsync(c => c.IdCodiceConto == famiglia.IdCodiceConto);
            ViewBag.ContoAssociato = contoAssociato;

            return View(famiglia);
        }

        // GET: ListaFamiglie/Create
        public async Task<IActionResult> Create()
        {
            // Suggerisci il prossimo ID disponibile
            var maxId = await _context.PstreeListaFamiglie
                .MaxAsync(f => (int?)f.Id) ?? 0;

            var model = new PstreeListaFamiglie
            {
                Id = maxId + 1
            };

            await PopulateContiRimanenzaDropdownAsync();
            return View(model);
        }

        // POST: ListaFamiglie/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,CodiceFamiglia,NomeFamiglia,DescrizioneFamiglia,IdCodiceConto")] PstreeListaFamiglie famiglia)
        {
            if (ModelState.IsValid)
            {
                // Verifica che l'ID non esista già
                if (await _context.PstreeListaFamiglie.AnyAsync(f => f.Id == famiglia.Id))
                {
                    ModelState.AddModelError("Id", "Questo ID è già utilizzato.");
                    await PopulateContiRimanenzaDropdownAsync(famiglia.IdCodiceConto);
                    return View(famiglia);
                }

                // Verifica che il nome non esista già
                if (await _context.PstreeListaFamiglie.AnyAsync(f => f.NomeFamiglia == famiglia.NomeFamiglia))
                {
                    ModelState.AddModelError("NomeFamiglia", "Questo nome famiglia è già utilizzato.");
                    await PopulateContiRimanenzaDropdownAsync(famiglia.IdCodiceConto);
                    return View(famiglia);
                }

                _context.Add(famiglia);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Famiglia '{famiglia.NomeFamiglia}' creata con successo!";
                _logger.LogInformation("Famiglia {Id} - {NomeFamiglia} creata", famiglia.Id, famiglia.NomeFamiglia);

                return RedirectToAction(nameof(Index));
            }

            await PopulateContiRimanenzaDropdownAsync(famiglia.IdCodiceConto);
            return View(famiglia);
        }

        // GET: ListaFamiglie/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var famiglia = await _context.PstreeListaFamiglie.FindAsync(id);
            if (famiglia == null)
            {
                return NotFound();
            }

            await PopulateContiRimanenzaDropdownAsync(famiglia.IdCodiceConto);
            return View(famiglia);
        }

        // POST: ListaFamiglie/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,CodiceFamiglia,NomeFamiglia,DescrizioneFamiglia,IdCodiceConto")] PstreeListaFamiglie famiglia)
        {
            if (id != famiglia.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Verifica che il nome non esista già (escluso il record corrente)
                    if (await _context.PstreeListaFamiglie.AnyAsync(f => f.NomeFamiglia == famiglia.NomeFamiglia && f.Id != famiglia.Id))
                    {
                        ModelState.AddModelError("NomeFamiglia", "Questo nome famiglia è già utilizzato.");
                        await PopulateContiRimanenzaDropdownAsync(famiglia.IdCodiceConto);
                        return View(famiglia);
                    }

                    _context.Update(famiglia);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = $"Famiglia '{famiglia.NomeFamiglia}' modificata con successo!";
                    _logger.LogInformation("Famiglia {Id} modificata", famiglia.Id);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!FamigliaExists(famiglia.Id))
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

            await PopulateContiRimanenzaDropdownAsync(famiglia.IdCodiceConto);
            return View(famiglia);
        }

        // GET: ListaFamiglie/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var famiglia = await _context.PstreeListaFamiglie
                .FirstOrDefaultAsync(f => f.Id == id);

            if (famiglia == null)
            {
                return NotFound();
            }

            // Conta utilizzi
            var countSaldi = await _context.PstreeListaSaldi
                .CountAsync(s => s.IdFamiglia == famiglia.Id);
            var countRettifiche = await _context.PstreeListaRettifiche
                .CountAsync(r => r.IdFamiglia == famiglia.Id);

            ViewBag.CountSaldi = countSaldi;
            ViewBag.CountRettifiche = countRettifiche;
            ViewBag.IsUsed = countSaldi > 0 || countRettifiche > 0;

            return View(famiglia);
        }

        // POST: ListaFamiglie/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var famiglia = await _context.PstreeListaFamiglie.FindAsync(id);
            if (famiglia == null)
            {
                return NotFound();
            }

            // Verifica se è in uso
            var countSaldi = await _context.PstreeListaSaldi
                .CountAsync(s => s.IdFamiglia == famiglia.Id);
            var countRettifiche = await _context.PstreeListaRettifiche
                .CountAsync(r => r.IdFamiglia == famiglia.Id);

            if (countSaldi > 0 || countRettifiche > 0)
            {
                TempData["ErrorMessage"] = $"Impossibile eliminare '{famiglia.NomeFamiglia}' perché è utilizzata in {countSaldi} saldi e {countRettifiche} rettifiche.";
                return RedirectToAction(nameof(Index));
            }

            var nome = famiglia.NomeFamiglia;
            _context.PstreeListaFamiglie.Remove(famiglia);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Famiglia '{nome}' eliminata con successo!";
            _logger.LogInformation("Famiglia {Id} eliminata", id);

            return RedirectToAction(nameof(Index));
        }

        private bool FamigliaExists(int id)
        {
            return _context.PstreeListaFamiglie.Any(f => f.Id == id);
        }

        /// <summary>
        /// Popola il dropdown con i conti economici che hanno VoceRimanenza = true
        /// </summary>
        private async Task PopulateContiRimanenzaDropdownAsync(int? selectedId = null)
        {
            var conti = await _context.PstreeStrutturaContoEconomico
                .Where(c => c.VoceRimanenza)
                .OrderBy(c => c.IdCodiceConto)
                .Select(c => new SelectListItem
                {
                    Value = c.IdCodiceConto.ToString(),
                    Text = $"{c.IdCodiceConto} - {c.DescrizioneConto}",
                    Selected = selectedId.HasValue && c.IdCodiceConto == selectedId.Value
                })
                .ToListAsync();

            ViewBag.ContiRimanenza = conti;
        }
    }
}
