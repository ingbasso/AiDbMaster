using AiDbMaster.Attributes;
using AiDbMaster.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AiDbMaster.Controllers
{
    [Authorize]
    [RegisterResource("ViaggiConsegna", "Lista Viaggi Consegna", Description = "Elenco viaggi pianificati", MenuIcon = "bi-calendar-event", MenuOrder = 2)]
    [RequirePermission("ViaggiConsegna", "View")]
    public class ViaggiConsegnaController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ViaggiConsegnaController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(DateTime? dataDa, DateTime? dataA)
        {
            var from = (dataDa ?? DateTime.Today).Date;
            var to = (dataA ?? DateTime.Today.AddDays(7)).Date;

            var viaggi = await _context.ViaggiConsegna
                .AsNoTracking()
                .Include(v => v.TipoTrasporto)
                .Include(v => v.MezzoTrasporto)
                .Include(v => v.MezzoTrasportoEsterno)
                .Include(v => v.Righe)
                .Where(v => v.DataConsegna >= from && v.DataConsegna <= to)
                .OrderBy(v => v.DataConsegna)
                .ThenBy(v => v.OraPartenza)
                .ToListAsync();

            ViewBag.DataDa = from;
            ViewBag.DataA = to;
            return View(viaggi);
        }
    }
}
