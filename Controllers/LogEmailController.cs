using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using AiDbMaster.Data;

namespace AiDbMaster.Controllers
{
    [Authorize]
    public class LogEmailController : Controller
    {
        private readonly ApplicationDbContext _context;

        public LogEmailController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string? filtroTipo, string? filtroEsito, DateTime? dataDa, DateTime? dataA)
        {
            var query = _context.LogEmailAutomatico.AsQueryable();

            dataDa ??= DateTime.Today.AddDays(-7);
            dataA ??= DateTime.Today.AddDays(1);

            query = query.Where(l => l.DataOra >= dataDa.Value && l.DataOra < dataA.Value);

            if (!string.IsNullOrWhiteSpace(filtroTipo))
                query = query.Where(l => l.Tipo == filtroTipo);

            if (!string.IsNullOrWhiteSpace(filtroEsito))
                query = query.Where(l => l.Esito == filtroEsito);

            var logs = await query.OrderByDescending(l => l.DataOra).ToListAsync();

            ViewBag.FiltroTipo = filtroTipo;
            ViewBag.FiltroEsito = filtroEsito;
            ViewBag.DataDa = dataDa.Value.ToString("yyyy-MM-dd");
            ViewBag.DataA = dataA.Value.ToString("yyyy-MM-dd");
            ViewBag.TotaleRecord = logs.Count;
            ViewBag.TotaleInviate = logs.Count(l => l.Esito == "OK");
            ViewBag.TotaleFallite = logs.Count(l => l.Esito == "Fallito");
            ViewBag.TotaleSaltate = logs.Count(l => l.Esito == "Saltato");

            return View(logs);
        }

        [HttpPost]
        public async Task<IActionResult> SvuotaLog()
        {
            await _context.Database.ExecuteSqlRawAsync("DELETE FROM LogEmailAutomatico");
            TempData["Messaggio"] = "Log cancellati con successo.";
            return RedirectToAction(nameof(Index));
        }
    }
}
