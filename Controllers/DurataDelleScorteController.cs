using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AiDbMaster.Data;

namespace AiDbMaster.Controllers
{
    [Authorize]
    public class DurataDelleScorteController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<DurataDelleScorteController> _logger;

        public DurataDelleScorteController(
            ApplicationDbContext context,
            ILogger<DurataDelleScorteController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Index()
        {
            ViewBag.UseFluidContainer = true;
            return View();
        }

        [HttpGet]
        [Route("api/durata-delle-scorte")]
        public async Task<IActionResult> GetDurataDelleScorte(
            short? codMarca, string? codFamiglia, string? codiceArticolo)
        {
            try
            {
                var query = _context.DurataDelleScorte.AsQueryable();

                if (codMarca.HasValue)
                    query = query.Where(d => d.CodMarca == codMarca.Value);

                if (!string.IsNullOrWhiteSpace(codFamiglia))
                    query = query.Where(d => d.CodFamiglia == codFamiglia);

                if (!string.IsNullOrWhiteSpace(codiceArticolo))
                    query = query.Where(d => d.CodiceArticolo == codiceArticolo);

                var risultati = await query
                    .OrderBy(d => d.CodiceArticolo)
                    .Select(d => new
                    {
                        d.ID,
                        d.CodMarca,
                        DescrizioneMarca = (d.DescrizioneMarca ?? "").Trim(),
                        CodFamiglia = d.CodFamiglia.Trim(),
                        DescrFamiglia = (d.DescrFamiglia ?? "").Trim(),
                        CodiceArticolo = d.CodiceArticolo.Trim(),
                        Descrizione = d.Descrizione.Trim(),
                        UnitaMisura = d.UnitaMisura.Trim(),
                        d.Magazzino,
                        DataUltimoScarico = d.DataUltimoScarico.HasValue
                            ? d.DataUltimoScarico.Value.ToString("dd/MM/yyyy")
                            : "",
                        Esistenza = d.Esistenza ?? 0,
                        Disponibilita = d.Disponibilita ?? 0,
                        ConsumoUltimoMese = d.ConsumoUltimoMese ?? 0,
                        ConsumoDueMesiFa = d.ConsumoDueMesiFa ?? 0,
                        ConsumoTreMesiFa = d.ConsumoTreMesiFa ?? 0,
                        ConsumoMedioPonderato = d.ConsumoMedioPonderato ?? 0,
                        DurataDelleScorte = d.DurataScorte ?? 0
                    })
                    .ToListAsync();

                return Json(new { success = true, data = risultati, totale = risultati.Count });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante il caricamento della Durata delle Scorte");
                return Json(new { success = false, errore = ex.Message });
            }
        }

        [HttpGet]
        [Route("api/durata-delle-scorte/marche")]
        public async Task<IActionResult> GetMarche()
        {
            try
            {
                var marche = await _context.DurataDelleScorte
                    .Select(d => new { d.CodMarca, d.DescrizioneMarca })
                    .Distinct()
                    .OrderBy(m => m.CodMarca)
                    .Select(m => new
                    {
                        codice = m.CodMarca,
                        descrizione = (m.DescrizioneMarca ?? "").Trim(),
                        testo = m.CodMarca + " - " + (m.DescrizioneMarca ?? "").Trim()
                    })
                    .ToListAsync();

                return Json(marche);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore caricamento marche per filtro DurataDelleScorte");
                return Json(new List<object>());
            }
        }

        [HttpGet]
        [Route("api/durata-delle-scorte/famiglie")]
        public async Task<IActionResult> GetFamiglie()
        {
            try
            {
                var famiglie = await _context.DurataDelleScorte
                    .Select(d => new { d.CodFamiglia, d.DescrFamiglia })
                    .Distinct()
                    .OrderBy(f => f.CodFamiglia)
                    .Select(f => new
                    {
                        codice = f.CodFamiglia.Trim(),
                        descrizione = (f.DescrFamiglia ?? "").Trim(),
                        testo = f.CodFamiglia.Trim() + " - " + (f.DescrFamiglia ?? "").Trim()
                    })
                    .ToListAsync();

                return Json(famiglie);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore caricamento famiglie per filtro DurataDelleScorte");
                return Json(new List<object>());
            }
        }

        [HttpGet]
        [Route("api/durata-delle-scorte/articoli")]
        public async Task<IActionResult> GetArticoli()
        {
            try
            {
                var articoli = await _context.DurataDelleScorte
                    .Select(d => new { d.CodiceArticolo, d.Descrizione })
                    .Distinct()
                    .OrderBy(a => a.CodiceArticolo)
                    .Select(a => new
                    {
                        codice = a.CodiceArticolo.Trim(),
                        descrizione = a.Descrizione.Trim(),
                        testo = a.CodiceArticolo.Trim() + " - " + a.Descrizione.Trim()
                    })
                    .ToListAsync();

                return Json(articoli);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore caricamento articoli per filtro DurataDelleScorte");
                return Json(new List<object>());
            }
        }
    }
}
