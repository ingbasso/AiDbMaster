using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using AiDbMaster.Data;
using AiDbMaster.Models;
using AiDbMaster.ViewModels;
using Microsoft.AspNetCore.Authorization;

namespace AiDbMaster.Controllers
{
    [Authorize]
    public class PstreeStrutturaContoEconomicoController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<PstreeStrutturaContoEconomicoController> _logger;

        public PstreeStrutturaContoEconomicoController(ApplicationDbContext context, ILogger<PstreeStrutturaContoEconomicoController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: StrutturaContoEconomico
        public async Task<IActionResult> Index()
        {
            var struttura = await _context.PstreeStrutturaContoEconomico
                .OrderBy(s => s.IdCodiceConto)
                .ToListAsync();

            return View(struttura);
        }

        // GET: StrutturaContoEconomico/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var conto = await _context.PstreeStrutturaContoEconomico
                .FirstOrDefaultAsync(m => m.IdCodiceConto == id);

            if (conto == null)
            {
                return NotFound();
            }

            if (conto.ParentId > 0)
            {
                conto.Parent = await _context.PstreeStrutturaContoEconomico.FindAsync(conto.ParentId);
            }

            conto.Figli = await _context.PstreeStrutturaContoEconomico
                .Where(s => s.ParentId == conto.IdCodiceConto)
                .OrderBy(s => s.Ordine)
                .ToListAsync();

            return View(conto);
        }

        // GET: StrutturaContoEconomico/Create
        public async Task<IActionResult> Create()
        {
            await PopulateParentDropdownAsync();
            PopulateTipoContoDropdown();
            
            var model = new PstreeStrutturaContoEconomico
            {
                ParentId = 0,
                Ordine = await GetNextOrdineAsync(0),
                Livello = 1
            };
            
            return View(model);
        }

        // POST: StrutturaContoEconomico/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IdCodiceConto,DescrizioneConto,TipoConto,ParentId,Ordine,Livello,VoceRettifica,VoceRimanenza,GruppoPercentuale,CostiFD,CashFlowEconomico")] PstreeStrutturaContoEconomico conto)
        {
            if (ModelState.IsValid)
            {
                if (conto.ParentId > 0)
                {
                    var parent = await _context.PstreeStrutturaContoEconomico.FindAsync(conto.ParentId);
                    if (parent != null)
                    {
                        conto.Livello = parent.Livello + 1;
                    }
                }
                else
                {
                    conto.Livello = 1;
                }

                _context.Add(conto);
                await _context.SaveChangesAsync();
                
                TempData["SuccessMessage"] = $"Conto '{conto.DescrizioneConto}' creato con successo!";
                _logger.LogInformation("Conto {IdCodiceConto} - {DescrizioneConto} creato", conto.IdCodiceConto, conto.DescrizioneConto);
                
                return RedirectToAction(nameof(Index));
            }

            await PopulateParentDropdownAsync(conto.ParentId);
            PopulateTipoContoDropdown(conto.TipoConto);
            return View(conto);
        }

        // GET: StrutturaContoEconomico/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var conto = await _context.PstreeStrutturaContoEconomico.FindAsync(id);
            if (conto == null)
            {
                return NotFound();
            }

            await PopulateParentDropdownAsync(conto.ParentId, conto.IdCodiceConto);
            PopulateTipoContoDropdown(conto.TipoConto);
            return View(conto);
        }

        // POST: StrutturaContoEconomico/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdCodiceConto,DescrizioneConto,TipoConto,ParentId,Ordine,Livello,VoceRettifica,VoceRimanenza,GruppoPercentuale,CostiFD,CashFlowEconomico")] PstreeStrutturaContoEconomico conto)
        {
            if (id != conto.IdCodiceConto)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    if (conto.ParentId == conto.IdCodiceConto)
                    {
                        ModelState.AddModelError("ParentId", "Un conto non può essere padre di sé stesso.");
                        await PopulateParentDropdownAsync(conto.ParentId, conto.IdCodiceConto);
                        PopulateTipoContoDropdown(conto.TipoConto);
                        return View(conto);
                    }

                    if (conto.ParentId > 0)
                    {
                        var parent = await _context.PstreeStrutturaContoEconomico.FindAsync(conto.ParentId);
                        if (parent != null)
                        {
                            conto.Livello = parent.Livello + 1;
                        }
                    }
                    else
                    {
                        conto.Livello = 1;
                    }

                    _context.Update(conto);
                    await _context.SaveChangesAsync();
                    
                    TempData["SuccessMessage"] = $"Conto '{conto.DescrizioneConto}' modificato con successo!";
                    _logger.LogInformation("Conto {IdCodiceConto} modificato", conto.IdCodiceConto);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ContoExists(conto.IdCodiceConto))
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

            await PopulateParentDropdownAsync(conto.ParentId, conto.IdCodiceConto);
            PopulateTipoContoDropdown(conto.TipoConto);
            return View(conto);
        }

        // GET: StrutturaContoEconomico/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var conto = await _context.PstreeStrutturaContoEconomico
                .FirstOrDefaultAsync(m => m.IdCodiceConto == id);

            if (conto == null)
            {
                return NotFound();
            }

            if (conto.ParentId > 0)
            {
                conto.Parent = await _context.PstreeStrutturaContoEconomico.FindAsync(conto.ParentId);
            }

            conto.Figli = await _context.PstreeStrutturaContoEconomico
                .Where(s => s.ParentId == conto.IdCodiceConto)
                .OrderBy(s => s.Ordine)
                .ToListAsync();

            return View(conto);
        }

        // POST: StrutturaContoEconomico/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var conto = await _context.PstreeStrutturaContoEconomico
                .FirstOrDefaultAsync(s => s.IdCodiceConto == id);

            if (conto == null)
            {
                return NotFound();
            }

            var countFigli = await _context.PstreeStrutturaContoEconomico
                .CountAsync(s => s.ParentId == id);

            if (countFigli > 0)
            {
                TempData["ErrorMessage"] = $"Impossibile eliminare '{conto.DescrizioneConto}' perché ha {countFigli} sotto-conti collegati.";
                return RedirectToAction(nameof(Index));
            }

            _context.PstreeStrutturaContoEconomico.Remove(conto);
            await _context.SaveChangesAsync();
            
            TempData["SuccessMessage"] = $"Conto '{conto.DescrizioneConto}' eliminato con successo!";
            _logger.LogInformation("Conto {IdCodiceConto} - {DescrizioneConto} eliminato", conto.IdCodiceConto, conto.DescrizioneConto);
            
            return RedirectToAction(nameof(Index));
        }

        // GET: StrutturaContoEconomico/Tree - Vista ad albero
        public async Task<IActionResult> Tree()
        {
            var tuttiConti = await _context.PstreeStrutturaContoEconomico
                .OrderBy(s => s.IdCodiceConto)
                .ToListAsync();

            foreach (var conto in tuttiConti)
            {
                conto.Figli = tuttiConti
                    .Where(c => c.ParentId == conto.IdCodiceConto)
                    .OrderBy(c => c.IdCodiceConto)
                    .ToList();
            }

            var radici = tuttiConti
                .Where(c => c.ParentId == 0)
                .OrderBy(c => c.IdCodiceConto)
                .ToList();

            return View(radici);
        }

        // === METODI HELPER ===

        private bool ContoExists(int id)
        {
            return _context.PstreeStrutturaContoEconomico.Any(e => e.IdCodiceConto == id);
        }

        private async Task PopulateParentDropdownAsync(int? selectedId = null, int? excludeId = null)
        {
            var conti = await _context.PstreeStrutturaContoEconomico
                .OrderBy(s => s.Livello)
                .ThenBy(s => s.Ordine)
                .ToListAsync();

            if (excludeId.HasValue)
            {
                conti = conti.Where(c => c.IdCodiceConto != excludeId.Value).ToList();
            }

            var items = new List<SelectListItem>
            {
                new SelectListItem { Value = "0", Text = "-- Nessun padre (Radice) --" }
            };

            foreach (var conto in conti)
            {
                var indentCount = Math.Max(0, (conto.Livello - 1) * 2);
                var indent = indentCount > 0 ? new string('-', indentCount) : "";
                items.Add(new SelectListItem
                {
                    Value = conto.IdCodiceConto.ToString(),
                    Text = $"{indent} {conto.DescrizioneCompleta}",
                    Selected = selectedId.HasValue && conto.IdCodiceConto == selectedId.Value
                });
            }

            ViewBag.ParentId = items;
        }

        private void PopulateTipoContoDropdown(string? selectedValue = null)
        {
            var tipi = new List<SelectListItem>
            {
                new SelectListItem { Value = "F", Text = "Foglia (Dettaglio)", Selected = selectedValue == "F" },
                new SelectListItem { Value = "R", Text = "Ricavo", Selected = selectedValue == "R" },
                new SelectListItem { Value = "C", Text = "Costo", Selected = selectedValue == "C" },
                new SelectListItem { Value = "T", Text = "Totale", Selected = selectedValue == "T" },
                new SelectListItem { Value = "S", Text = "Sottototale", Selected = selectedValue == "S" }
            };

            ViewBag.TipoConto = tipi;
        }

        private async Task<int> GetNextOrdineAsync(int parentId)
        {
            var maxOrdine = await _context.PstreeStrutturaContoEconomico
                .Where(s => s.ParentId == parentId)
                .MaxAsync(s => (int?)s.Ordine) ?? 0;

            return maxOrdine + 10;
        }
    }
}
