using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using AiDbMaster.Data;
using AiDbMaster.Models;
using AiDbMaster.ViewModels;
using Microsoft.AspNetCore.Authorization;

namespace AiDbMaster.Controllers
{
    /// <summary>
    /// Controller per la gestione della Lista Saldi
    /// Tabella alimentata da fonti esterne - Solo visualizzazione con filtri
    /// </summary>
    [Authorize]
    public class PstreeListaSaldiController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<PstreeListaSaldiController> _logger;

        private static readonly Dictionary<int, string> NomiMesi = new()
        {
            { 1, "Gennaio" }, { 2, "Febbraio" }, { 3, "Marzo" }, { 4, "Aprile" },
            { 5, "Maggio" }, { 6, "Giugno" }, { 7, "Luglio" }, { 8, "Agosto" },
            { 9, "Settembre" }, { 10, "Ottobre" }, { 11, "Novembre" }, { 12, "Dicembre" }
        };

        public PstreeListaSaldiController(ApplicationDbContext context, ILogger<PstreeListaSaldiController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: ListaSaldi
        public async Task<IActionResult> Index(
            string? codicePdC,
            int? mese,
            int? anno,
            int? idSede)
        {
            // Default sede con id più piccolo se non specificata
            int? sedeDefault = idSede;
            if (!idSede.HasValue)
            {
                var primaSede = await _context.PstreeListaSedi.OrderBy(s => s.Id).FirstOrDefaultAsync();
                sedeDefault = primaSede?.Id;
            }
            
            // Query base
            var query = _context.PstreeListaSaldi.AsQueryable();

            // Applica filtri
            if (!string.IsNullOrEmpty(codicePdC))
            {
                query = query.Where(s => s.CodicePdC == codicePdC);
            }

            if (mese.HasValue)
            {
                query = query.Where(s => s.Mese == mese.Value);
            }

            if (anno.HasValue)
            {
                query = query.Where(s => s.Anno == anno.Value);
            }

            // Applica sempre filtro sede (default = sede con id più piccolo)
            if (sedeDefault.HasValue)
            {
                query = query.Where(s => s.IdSede == sedeDefault.Value);
            }

            // Ordina per CodicePdC, poi Anno, poi Mese
            var saldi = await query
                .OrderBy(s => s.CodicePdC)
                .ThenByDescending(s => s.Anno)
                .ThenBy(s => s.Mese)
                .ToListAsync();

            // Carica dizionario Piano dei Conti per mostrare descrizioni
            var pdcDict = await _context.PstreeListaPianoDeiConti
                .ToDictionaryAsync(p => p.CodicePdC, p => p);
            ViewBag.PianoDeiContiDict = pdcDict;

            // Carica dizionari per mostrare descrizioni famiglie e sedi
            var famiglieDict = await _context.PstreeListaFamiglie
                .ToDictionaryAsync(f => f.Id, f => f.NomeFamiglia);
            ViewBag.FamiglieDescrizioni = famiglieDict;

            var sediDict = await _context.PstreeListaSedi
                .ToDictionaryAsync(s => s.Id, s => s.Sede);
            ViewBag.SediDescrizioni = sediDict;

            // Popola i dropdown per i filtri (senza famiglia)
            await PopulateFilterDropdowns(codicePdC, mese, anno, null, sedeDefault);

            // Passa i valori dei filtri alla view per mantenerli
            ViewBag.CurrentCodicePdC = codicePdC;
            ViewBag.CurrentMese = mese;
            ViewBag.CurrentAnno = anno;
            ViewBag.CurrentIdSede = sedeDefault;

            // Calcola totali per la selezione corrente
            ViewBag.TotaleDare = saldi.Sum(s => s.Dare ?? 0);
            ViewBag.TotaleAvere = saldi.Sum(s => s.Avere ?? 0);
            ViewBag.TotaleSaldo = saldi.Sum(s => s.Saldo);

            return View(saldi);
        }

        // GET: ListaSaldi/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var saldo = await _context.PstreeListaSaldi
                .FirstOrDefaultAsync(s => s.Id == id);

            if (saldo == null)
            {
                return NotFound();
            }

            // Carica il conto del Piano dei Conti
            ViewBag.PianoDeiConti = await _context.PstreeListaPianoDeiConti
                .FirstOrDefaultAsync(p => p.CodicePdC == saldo.CodicePdC);

            // Carica famiglia e sede
            if (saldo.IdFamiglia.HasValue)
            {
                ViewBag.Famiglia = await _context.PstreeListaFamiglie
                    .FirstOrDefaultAsync(f => f.Id == saldo.IdFamiglia.Value);
            }
            if (saldo.IdSede.HasValue)
            {
                ViewBag.Sede = await _context.PstreeListaSedi
                    .FirstOrDefaultAsync(s => s.Id == saldo.IdSede.Value);
            }

            return View(saldo);
        }

        // ========================================
        // ENDPOINT AJAX PER DROPDOWN AUTO-AGGIORNANTI
        // ========================================

        /// <summary>
        /// Restituisce la lista dei valori per i dropdown in base ai filtri attuali
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetFilterOptions(
            string? codicePdC,
            int? mese,
            int? anno,
            int? idFamiglia,
            int? idSede)
        {
            // Query base
            var query = _context.PstreeListaSaldi.AsQueryable();

            // Applica filtri per ottenere solo le opzioni coerenti
            if (!string.IsNullOrEmpty(codicePdC))
            {
                query = query.Where(s => s.CodicePdC == codicePdC);
            }

            if (mese.HasValue)
            {
                query = query.Where(s => s.Mese == mese.Value);
            }

            if (anno.HasValue)
            {
                query = query.Where(s => s.Anno == anno.Value);
            }

            if (idFamiglia.HasValue)
            {
                query = query.Where(s => s.IdFamiglia == idFamiglia.Value);
            }

            if (idSede.HasValue)
            {
                query = query.Where(s => s.IdSede == idSede.Value);
            }

            // Ottieni i codici PdC distinti presenti nei saldi filtrati
            var codiciPdCInSaldi = await query
                .Where(s => !string.IsNullOrEmpty(s.CodicePdC))
                .Select(s => s.CodicePdC)
                .Distinct()
                .ToListAsync();

            // Carica le descrizioni dal Piano dei Conti
            var codiciPdC = await _context.PstreeListaPianoDeiConti
                .Where(p => codiciPdCInSaldi.Contains(p.CodicePdC))
                .OrderBy(p => p.CodicePdC)
                .Select(p => new { p.CodicePdC, p.DescrizionePdC })
                .ToListAsync();

            // Mesi presenti nei saldi filtrati
            var mesiInSaldi = await query
                .Where(s => s.Mese.HasValue)
                .Select(s => s.Mese!.Value)
                .Distinct()
                .OrderBy(s => s)
                .ToListAsync();

            var mesi = mesiInSaldi.Select(m => new { 
                Id = m, 
                Nome = NomiMesi.GetValueOrDefault(m, m.ToString()) 
            }).ToList();

            var anni = await query
                .Where(s => s.Anno.HasValue)
                .Select(s => s.Anno!.Value)
                .Distinct()
                .OrderByDescending(s => s)
                .ToListAsync();

            // Per famiglie e sedi, restituiamo gli ID presenti nei saldi
            var idFamiglieInSaldi = await query
                .Where(s => s.IdFamiglia.HasValue)
                .Select(s => s.IdFamiglia!.Value)
                .Distinct()
                .ToListAsync();

            var idSediInSaldi = await query
                .Where(s => s.IdSede.HasValue)
                .Select(s => s.IdSede!.Value)
                .Distinct()
                .ToListAsync();

            // Carica le descrizioni
            var famiglie = await _context.PstreeListaFamiglie
                .Where(f => idFamiglieInSaldi.Contains(f.Id))
                .OrderBy(f => f.Id)
                .Select(f => new { f.Id, Nome = f.NomeFamiglia })
                .ToListAsync();

            var sedi = await _context.PstreeListaSedi
                .Where(s => idSediInSaldi.Contains(s.Id))
                .OrderBy(s => s.Id)
                .Select(s => new { s.Id, Nome = s.Sede })
                .ToListAsync();

            return Json(new
            {
                codiciPdC,
                mesi,
                anni,
                famiglie,
                sedi
            });
        }

        // ========================================
        // METODI HELPER
        // ========================================

        private async Task PopulateFilterDropdowns(
            string? selectedCodicePdC = null,
            int? selectedMese = null,
            int? selectedAnno = null,
            int? selectedIdFamiglia = null,
            int? selectedIdSede = null)
        {
            // Codice PdC - dropdown con tutti i conti del Piano dei Conti presenti nei saldi
            var codiciPdCInSaldi = await _context.PstreeListaSaldi
                .Where(s => !string.IsNullOrEmpty(s.CodicePdC))
                .Select(s => s.CodicePdC)
                .Distinct()
                .ToListAsync();

            var codiciPdC = await _context.PstreeListaPianoDeiConti
                .Where(p => codiciPdCInSaldi.Contains(p.CodicePdC))
                .OrderBy(p => p.CodicePdC)
                .Select(p => new SelectListItem
                {
                    Value = p.CodicePdC,
                    Text = $"{p.CodicePdC} - {p.DescrizionePdC}",
                    Selected = p.CodicePdC == selectedCodicePdC
                })
                .ToListAsync();

            ViewBag.CodiciPdC = codiciPdC;

            // Mese - dropdown con mesi presenti nei saldi
            var mesiInSaldi = await _context.PstreeListaSaldi
                .Where(s => s.Mese.HasValue)
                .Select(s => s.Mese!.Value)
                .Distinct()
                .OrderBy(s => s)
                .ToListAsync();

            var mesi = mesiInSaldi.Select(m => new SelectListItem
            {
                Value = m.ToString(),
                Text = $"{m} - {NomiMesi.GetValueOrDefault(m, m.ToString())}",
                Selected = selectedMese.HasValue && m == selectedMese.Value
            }).ToList();

            ViewBag.Mesi = mesi;

            // Anno
            var anni = await _context.PstreeListaSaldi
                .Where(s => s.Anno.HasValue)
                .Select(s => s.Anno!.Value)
                .Distinct()
                .OrderByDescending(s => s)
                .ToListAsync();

            ViewBag.Anni = new SelectList(anni, selectedAnno);

            // Famiglia - dropdown con tutte le famiglie disponibili
            var famiglie = await _context.PstreeListaFamiglie
                .OrderBy(f => f.Id)
                .Select(f => new SelectListItem
                {
                    Value = f.Id.ToString(),
                    Text = $"{f.Id} - {f.NomeFamiglia}",
                    Selected = selectedIdFamiglia.HasValue && f.Id == selectedIdFamiglia.Value
                })
                .ToListAsync();

            ViewBag.Famiglie = famiglie;

            // Sede - dropdown con tutte le sedi disponibili
            var sedi = await _context.PstreeListaSedi
                .OrderBy(s => s.Id)
                .Select(s => new SelectListItem
                {
                    Value = s.Id.ToString(),
                    Text = $"{s.Id} - {s.Sede}",
                    Selected = selectedIdSede.HasValue && s.Id == selectedIdSede.Value
                })
                .ToListAsync();

            ViewBag.Sedi = sedi;
        }
    }
}
