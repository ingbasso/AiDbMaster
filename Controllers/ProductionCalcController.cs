using AiDbMaster.Attributes;
using AiDbMaster.Data;
using AiDbMaster.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AiDbMaster.Controllers
{
    [Authorize]
    [RegisterResource("ProductionCalc", "Production Calc", Description = "Calcolo produzione articoli", MenuIcon = "bi-calculator", MenuOrder = 5)]
    [RequirePermission("ProductionCalc", "View")]
    public class ProductionCalcController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ProductionCalcController> _logger;

        public ProductionCalcController(ApplicationDbContext context, ILogger<ProductionCalcController> logger)
        {
            _context = context;
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View(new ProductionCalcViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(string codiceArticolo)
        {
            var model = new ProductionCalcViewModel { CodiceArticolo = codiceArticolo };

            if (string.IsNullOrWhiteSpace(codiceArticolo))
                return View(model);

            try
            {
                var articolo = await _context.AnagraficaArticoli
                    .AsNoTracking()
                    .FirstOrDefaultAsync(a => a.CodiceArticolo == codiceArticolo);

                if (articolo == null)
                {
                    ViewBag.Errore = "Articolo non trovato.";
                    return View(model);
                }

                model.Descrizione = articolo.Descrizione;
                model.DescrizioneUlteriore = articolo.DescrizioneUlteriore;
                model.UnitaMisura = articolo.UnitaMisura;
                model.QtaUMPPerTavola = articolo.QtaUMPPerTavola;
                model.QtaUMPPerPallet = articolo.QtaUMPPerPallet;
                model.TavolePerPallet = articolo.TavolePerPallet;

                var codiceArticoloM = codiceArticolo + "_M";

                var politica = await _context.PoliticheRiordinoMagazzino
                    .AsNoTracking()
                    .FirstOrDefaultAsync(p => p.CodiceArticolo == codiceArticoloM);

                if (politica != null)
                {
                    model.PoliticaDiRiordino = politica.PoliticaDiRiordino;
                    model.LottoStandardProduzione = politica.LottoStandardProduzione;
                    model.Sottolotto = politica.Sottolotto;
                    model.ScortaMinima = politica.ScortaMinima;
                    model.ScortaMassima = politica.ScortaMassima;
                }

                var lavorazione = await _context.DbLavorazioni
                    .AsNoTracking()
                    .Where(l => l.CodiceDistinta == codiceArticoloM)
                    .OrderByDescending(l => l.ID)
                    .FirstOrDefaultAsync();

                if (lavorazione != null)
                {
                    model.TavoleOraTeoriche = lavorazione.TavoleOraTeoriche;
                    model.Efficienza = lavorazione.Efficienza;
                    model.TavoleOraReali = lavorazione.TavoleOraReali;
                }

                model.ArticoloTrovato = true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante il calcolo produzione per articolo {CodiceArticolo}", codiceArticolo);
                ViewBag.Errore = "Errore durante il caricamento dei dati.";
            }

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> SearchArticoli(string term)
        {
            if (string.IsNullOrEmpty(term) || term.Length < 2)
                return Json(new List<object>());

            try
            {
                var articoli = await _context.AnagraficaArticoli
                    .AsNoTracking()
                    .Where(a => a.CodiceArticolo.Contains(term) ||
                               a.Descrizione.Contains(term))
                    .OrderBy(a => a.CodiceArticolo)
                    .Select(a => new
                    {
                        id = a.CodiceArticolo,
                        text = $"{a.CodiceArticolo} - {a.Descrizione}"
                    })
                    .Take(20)
                    .ToListAsync();

                return Json(articoli);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore ricerca articoli: {Term}", term);
                return Json(new List<object>());
            }
        }
    }
}
