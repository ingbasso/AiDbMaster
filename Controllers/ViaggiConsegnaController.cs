using AiDbMaster.Attributes;
using AiDbMaster.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
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

        public async Task<IActionResult> Index(
            DateTime? dataDa, DateTime? dataA,
            string? cliente, string? numeroOrdine, string? codiceArticolo,
            string? stato, int? autistaId, string? tipoMezzo)
        {
            var from = (dataDa ?? DateTime.Today.AddDays(-30)).Date;
            var to = (dataA ?? DateTime.Today.AddDays(7)).Date;

            var query = _context.ViaggiConsegna
                .AsNoTracking()
                .Include(v => v.TipoTrasporto)
                .Include(v => v.MezzoTrasporto)
                .Include(v => v.MezzoTrasportoEsterno)
                .Include(v => v.Autista)
                .Include(v => v.Righe).ThenInclude(r => r.OrdineRiga).ThenInclude(o => o!.Testata).ThenInclude(t => t!.Cliente)
                .Include(v => v.Destinazioni)
                .Where(v => v.DataConsegna >= from && v.DataConsegna <= to);

            if (!string.IsNullOrWhiteSpace(stato))
                query = query.Where(v => v.Stato == stato);

            if (autistaId.HasValue)
                query = query.Where(v => v.AutistaId == autistaId.Value);

            if (!string.IsNullOrWhiteSpace(tipoMezzo))
            {
                if (tipoMezzo == "Interno")
                    query = query.Where(v => v.MezzoTrasportoId != null);
                else if (tipoMezzo == "Esterno")
                    query = query.Where(v => v.MezzoTrasportoEsternoId != null);
            }

            if (!string.IsNullOrWhiteSpace(cliente))
            {
                var clienteLower = cliente.ToLower();
                query = query.Where(v => v.Righe.Any(r =>
                    r.OrdineRiga != null && r.OrdineRiga.Testata != null
                    && r.OrdineRiga.Testata.Cliente != null
                    && r.OrdineRiga.Testata.Cliente.RagioneSociale != null
                    && r.OrdineRiga.Testata.Cliente.RagioneSociale.ToLower().Contains(clienteLower)));
            }

            if (!string.IsNullOrWhiteSpace(numeroOrdine))
            {
                var ordTrim = numeroOrdine.Trim();
                if (int.TryParse(ordTrim, out var ordNum))
                {
                    query = query.Where(v => v.Righe.Any(r =>
                        r.OrdineRiga != null && r.OrdineRiga.NumeroOrdine == ordNum));
                }
                else
                {
                    var ordLower = ordTrim.ToLower();
                    query = query.Where(v => v.Righe.Any(r =>
                        r.OrdineRiga != null
                        && (r.OrdineRiga.TipoOrdine + r.OrdineRiga.AnnoOrdine.ToString() + "/" + r.OrdineRiga.SerieOrdine + "/" + r.OrdineRiga.NumeroOrdine.ToString()).ToLower().Contains(ordLower)));
                }
            }

            if (!string.IsNullOrWhiteSpace(codiceArticolo))
            {
                var artLower = codiceArticolo.Trim().ToLower();
                query = query.Where(v => v.Righe.Any(r =>
                    r.OrdineRiga != null
                    && r.OrdineRiga.CodiceArticolo.ToLower().Contains(artLower)));
            }

            var viaggi = await query
                .OrderByDescending(v => v.DataConsegna)
                .ThenBy(v => v.OraPartenza)
                .ToListAsync();

            ViewBag.DataDa = from;
            ViewBag.DataA = to;
            ViewBag.Cliente = cliente;
            ViewBag.NumeroOrdine = numeroOrdine;
            ViewBag.CodiceArticolo = codiceArticolo;
            ViewBag.Stato = stato;
            ViewBag.AutistaId = autistaId;
            ViewBag.TipoMezzo = tipoMezzo;

            ViewBag.Autisti = await _context.Autisti
                .AsNoTracking()
                .Where(a => a.Attivo)
                .OrderBy(a => a.Cognome).ThenBy(a => a.Nome)
                .Select(a => new SelectListItem { Value = a.Id.ToString(), Text = $"{a.Cognome} {a.Nome}" })
                .ToListAsync();

            ViewBag.StatiDisponibili = new List<SelectListItem>
            {
                new("Pianificato", "Pianificato"),
                new("Confermato", "Confermato"),
                new("In Corso", "In Corso"),
                new("Completato", "Completato"),
                new("Annullato", "Annullato")
            };

            return View(viaggi);
        }
    }
}
