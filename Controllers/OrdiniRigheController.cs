using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using AiDbMaster.Data;
using AiDbMaster.Models;

namespace AiDbMaster.Controllers
{
    /// <summary>
    /// Controller per la gestione delle Righe Ordini
    /// Fornisce funzionalità di visualizzazione delle righe ordini con dettagli articoli e evasioni
    /// </summary>
    [Authorize]
    public class OrdiniRigheController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<OrdiniRigheController> _logger;

        public OrdiniRigheController(
            ApplicationDbContext context,
            ILogger<OrdiniRigheController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Visualizza l'elenco di tutte le righe ordini
        /// GET: OrdiniRighe
        /// </summary>
        /// <param name="search">Termine di ricerca per filtrare le righe</param>
        /// <param name="tipoOrdine">Filtro per tipo ordine (O/R)</param>
        /// <param name="codiceArticolo">Filtro per codice articolo</param>
        /// <param name="codiceMagazzino">Filtro per codice magazzino</param>
        /// <param name="statoEvasione">Filtro per stato evasione</param>
        /// <param name="dataConsegnaInizio">Data inizio per filtro consegna</param>
        /// <param name="dataConsegnaFine">Data fine per filtro consegna</param>
        /// <param name="soloRigheDaEvadere">Mostra solo righe da evadere</param>
        /// <param name="sortOrder">Ordinamento dei risultati</param>
        /// <param name="page">Numero di pagina per la paginazione</param>
        /// <param name="pageSize">Numero di elementi per pagina</param>
        /// <returns>Vista con l'elenco delle righe ordini</returns>
        public async Task<IActionResult> Index(
            string? search,
            string? tipoOrdine,
            string? codiceArticolo,
            short? codiceMagazzino,
            string? statoEvasione,
            DateTime? dataConsegnaInizio,
            DateTime? dataConsegnaFine,
            bool soloRigheDaEvadere = false,
            string sortOrder = "consegna",
            int page = 1,
            int pageSize = 50)
        {
            try
            {
                _logger.LogInformation("Caricamento righe ordini - Pagina: {Page}, Ricerca: {Search}", page, search);

                // Query base con include delle relazioni
                var query = _context.OrdiniRighe
                    .Include(r => r.Testata)
                        .ThenInclude(t => t!.Cliente)
                    .Include(r => r.Testata)
                        .ThenInclude(t => t!.Agente)
                    .Include(r => r.Articolo)
                    .Include(r => r.Magazzino)
                    .AsQueryable();

                // Filtro per ricerca testuale
                if (!string.IsNullOrEmpty(search))
                {
                    query = query.Where(r => 
                        r.CodiceArticolo.Contains(search) ||
                        (r.DescrizioneArticolo != null && r.DescrizioneArticolo.Contains(search)) ||
                        r.NumeroOrdine.ToString().Contains(search) ||
                        r.SerieOrdine.Contains(search) ||
                        (r.Note != null && r.Note.Contains(search)) ||
                        (r.Testata != null && r.Testata.Cliente != null && r.Testata.Cliente.RagioneSociale != null && r.Testata.Cliente.RagioneSociale.Contains(search)));
                }

                // Filtro per tipo ordine
                if (!string.IsNullOrEmpty(tipoOrdine))
                {
                    query = query.Where(r => r.TipoOrdine == tipoOrdine);
                }

                // Filtro per codice articolo
                if (!string.IsNullOrEmpty(codiceArticolo))
                {
                    query = query.Where(r => r.CodiceArticolo == codiceArticolo);
                }

                // Filtro per codice magazzino
                if (codiceMagazzino.HasValue)
                {
                    query = query.Where(r => r.CodiceMagazzino == codiceMagazzino.Value);
                }

                // Filtro per stato evasione
                if (!string.IsNullOrEmpty(statoEvasione))
                {
                    query = statoEvasione switch
                    {
                        "Da Evadere" => query.Where(r => r.QuantitaEvasa == 0),
                        "Parzialmente Evasa" => query.Where(r => r.QuantitaEvasa > 0 && r.QuantitaEvasa < r.Quantita),
                        "Completamente Evasa" => query.Where(r => r.QuantitaEvasa >= r.Quantita),
                        _ => query
                    };
                }

                // Filtro per periodo consegna
                if (dataConsegnaInizio.HasValue)
                {
                    query = query.Where(r => r.DataConsegna >= dataConsegnaInizio.Value);
                }

                if (dataConsegnaFine.HasValue)
                {
                    query = query.Where(r => r.DataConsegna <= dataConsegnaFine.Value);
                }

                // Filtro solo righe da evadere
                if (soloRigheDaEvadere)
                {
                    query = query.Where(r => r.QuantitaEvasa < r.Quantita);
                }

                // Ordinamento
                query = sortOrder switch
                {
                    "ordine" => query.OrderBy(r => r.TipoOrdine).ThenBy(r => r.AnnoOrdine).ThenBy(r => r.SerieOrdine).ThenBy(r => r.NumeroOrdine).ThenBy(r => r.RigaOrdine),
                    "ordine_desc" => query.OrderByDescending(r => r.TipoOrdine).ThenByDescending(r => r.AnnoOrdine).ThenByDescending(r => r.SerieOrdine).ThenByDescending(r => r.NumeroOrdine).ThenByDescending(r => r.RigaOrdine),
                    "articolo" => query.OrderBy(r => r.CodiceArticolo),
                    "articolo_desc" => query.OrderByDescending(r => r.CodiceArticolo),
                    "consegna" => query.OrderBy(r => r.DataConsegna),
                    "consegna_desc" => query.OrderByDescending(r => r.DataConsegna),
                    "quantita" => query.OrderBy(r => r.Quantita),
                    "quantita_desc" => query.OrderByDescending(r => r.Quantita),
                    "evasione" => query.OrderBy(r => r.QuantitaEvasa / r.Quantita),
                    "evasione_desc" => query.OrderByDescending(r => r.QuantitaEvasa / r.Quantita),
                    "valore" => query.OrderBy(r => r.ValoreRiga),
                    "valore_desc" => query.OrderByDescending(r => r.ValoreRiga),
                    "prezzo" => query.OrderBy(r => r.Prezzo),
                    "prezzo_desc" => query.OrderByDescending(r => r.Prezzo),
                    _ => query.OrderBy(r => r.DataConsegna)
                };

                // Conteggio totale per la paginazione
                var totalCount = await query.CountAsync();

                // Paginazione
                var righe = await query
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                // Preparazione dei dati per la vista
                ViewBag.CurrentSort = sortOrder;
                ViewBag.CurrentSearch = search;
                ViewBag.CurrentTipoOrdine = tipoOrdine;
                ViewBag.CurrentCodiceArticolo = codiceArticolo;
                ViewBag.CurrentCodiceMagazzino = codiceMagazzino;
                ViewBag.CurrentStatoEvasione = statoEvasione;
                ViewBag.CurrentDataConsegnaInizio = dataConsegnaInizio;
                ViewBag.CurrentDataConsegnaFine = dataConsegnaFine;
                ViewBag.CurrentSoloRigheDaEvadere = soloRigheDaEvadere;
                ViewBag.CurrentPage = page;
                ViewBag.PageSize = pageSize;
                ViewBag.TotalCount = totalCount;
                ViewBag.TotalPages = (int)Math.Ceiling((double)totalCount / pageSize);

                // Dati per i filtri dropdown
                ViewBag.Articoli = await _context.AnagraficaArticoli
                    .Where(a => _context.OrdiniRighe.Any(r => r.CodiceArticolo == a.CodiceArticolo))
                    .OrderBy(a => a.CodiceArticolo)
                    .Select(a => new { a.CodiceArticolo, DescrizioneArticolo = a.Descrizione })
                    .Take(100)
                    .ToListAsync();

                ViewBag.Magazzini = await _context.TabellaMagazzini
                    .Where(m => _context.OrdiniRighe.Any(r => r.CodiceMagazzino == m.CodiceMagazzino))
                    .OrderBy(m => m.CodiceMagazzino)
                    .Select(m => new { m.CodiceMagazzino, m.DescrizioneMagazzino })
                    .ToListAsync();

                // Statistiche aggiuntive
                var statistiche = await query
                    .GroupBy(r => 1)
                    .Select(g => new
                    {
                        TotaleRighe = g.Count(),
                        RigheDaEvadere = g.Count(r => r.QuantitaEvasa == 0),
                        RigheParziali = g.Count(r => r.QuantitaEvasa > 0 && r.QuantitaEvasa < r.Quantita),
                        RigheEvase = g.Count(r => r.QuantitaEvasa >= r.Quantita),
                        QuantitaTotale = g.Sum(r => r.Quantita),
                        QuantitaEvasa = g.Sum(r => r.QuantitaEvasa),
                        ValoreTotale = g.Sum(r => r.ValoreRiga),
                        ValoreNetto = g.Sum(r => (r.Quantita * r.Prezzo * (1 - r.Sconto1 / 100) * (1 - r.Sconto2 / 100) * (1 - r.Sconto3 / 100))),
                        ArticoliDistinti = g.Select(r => r.CodiceArticolo).Distinct().Count(),
                        MagazziniDistinti = g.Select(r => r.CodiceMagazzino).Distinct().Count()
                    })
                    .FirstOrDefaultAsync();

                ViewBag.Statistiche = statistiche;

                _logger.LogInformation("Caricate {Count} righe su {Total} totali", righe.Count, totalCount);

                return View(righe);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante il caricamento delle righe ordini");
                TempData["ErrorMessage"] = "Si è verificato un errore durante il caricamento delle righe ordini.";
                return View(new List<OrdiniRighe>());
            }
        }

        /// <summary>
        /// Visualizza i dettagli di una riga ordine specifica
        /// GET: OrdiniRighe/Details/5
        /// </summary>
        /// <param name="id">ID della riga ordine da visualizzare</param>
        /// <returns>Vista con i dettagli della riga ordine</returns>
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                _logger.LogWarning("Tentativo di accesso ai dettagli riga ordine senza ID");
                return NotFound();
            }

            try
            {
                var riga = await _context.OrdiniRighe
                    .Include(r => r.Testata)
                        .ThenInclude(t => t!.Cliente)
                    .Include(r => r.Testata)
                        .ThenInclude(t => t!.Agente)
                    .Include(r => r.Testata)
                        .ThenInclude(t => t!.Magazzino)
                    .Include(r => r.Articolo)
                    .Include(r => r.Magazzino)
                    .FirstOrDefaultAsync(r => r.Id == id);

                if (riga == null)
                {
                    _logger.LogWarning("Riga ordine con ID {Id} non trovata", id);
                    return NotFound();
                }

                // Altre righe dello stesso ordine
                ViewBag.AltreRigheOrdine = await _context.OrdiniRighe
                    .Include(r => r.Articolo)
                    .Where(r => r.TipoOrdine == riga.TipoOrdine && 
                               r.AnnoOrdine == riga.AnnoOrdine && 
                               r.SerieOrdine == riga.SerieOrdine && 
                               r.NumeroOrdine == riga.NumeroOrdine && 
                               r.Id != riga.Id)
                    .OrderBy(r => r.RigaOrdine)
                    .ToListAsync();

                // Altre righe dello stesso articolo
                ViewBag.AltreRigheArticolo = await _context.OrdiniRighe
                    .Include(r => r.Testata)
                        .ThenInclude(t => t!.Cliente)
                    .Where(r => r.CodiceArticolo == riga.CodiceArticolo && r.Id != riga.Id)
                    .OrderByDescending(r => r.DataConsegna)
                    .Take(10)
                    .ToListAsync();

                // Progressivi articoli per il magazzino
                ViewBag.ProgressiviArticolo = await _context.ProgressiviArticoli
                    .Where(p => p.CodiceArticolo == riga.CodiceArticolo && p.CodiceMagazzino == riga.CodiceMagazzino)
                    .FirstOrDefaultAsync();

                // Statistiche articolo
                var statisticheArticolo = await _context.OrdiniRighe
                    .Where(r => r.CodiceArticolo == riga.CodiceArticolo)
                    .GroupBy(r => 1)
                    .Select(g => new
                    {
                        TotaleRighe = g.Count(),
                        QuantitaTotaleOrdinata = g.Sum(r => r.Quantita),
                        QuantitaTotaleEvasa = g.Sum(r => r.QuantitaEvasa),
                        ValoreTotale = g.Sum(r => r.ValoreRiga),
                        PrezzoMedio = g.Average(r => r.Prezzo),
                        ScontoMedio = g.Average(r => r.Sconto1 + r.Sconto2 + r.Sconto3),
                        OrdiniDistinti = g.Select(r => new { r.TipoOrdine, r.AnnoOrdine, r.SerieOrdine, r.NumeroOrdine }).Distinct().Count()
                    })
                    .FirstOrDefaultAsync();

                ViewBag.StatisticheArticolo = statisticheArticolo;

                _logger.LogInformation("Visualizzazione dettagli riga ordine: {IdentificativoRiga}", riga.IdentificativoRiga);

                return View(riga);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante il caricamento dei dettagli della riga ordine {Id}", id);
                TempData["ErrorMessage"] = "Si è verificato un errore durante il caricamento dei dettagli della riga ordine.";
                return RedirectToAction(nameof(Index));
            }
        }

        /// <summary>
        /// API per ottenere i dati delle righe ordini in formato JSON
        /// GET: OrdiniRighe/GetRigheJson
        /// </summary>
        /// <returns>Dati delle righe ordini in formato JSON</returns>
        [HttpGet]
        public async Task<IActionResult> GetRigheJson()
        {
            try
            {
                var righe = await _context.OrdiniRighe
                    .Include(r => r.Testata)
                        .ThenInclude(t => t!.Cliente)
                    .Include(r => r.Articolo)
                    .Include(r => r.Magazzino)
                    .OrderBy(r => r.DataConsegna)
                    .Select(r => new
                    {
                        r.Id,
                        r.TipoOrdine,
                        r.AnnoOrdine,
                        r.SerieOrdine,
                        r.NumeroOrdine,
                        r.RigaOrdine,
                        NumeroOrdineCompleto = r.NumeroOrdineCompleto,
                        IdentificativoRiga = r.IdentificativoRiga,
                        r.CodiceArticolo,
                        r.DescrizioneArticolo,
                        DescrizioneArticoloCompleta = r.DescrizioneArticoloCompleta,
                        r.CodiceMagazzino,
                        DescrizioneMagazzino = r.Magazzino != null ? r.Magazzino.DescrizioneMagazzino : null,
                        r.DataConsegna,
                        r.Quantita,
                        r.QuantitaEvasa,
                        QuantitaRimanente = r.QuantitaRimanente,
                        StatoEvasione = r.StatoEvasione,
                        r.UnitaMisura,
                        r.NumeroColli,
                        r.ColliEvasi,
                        ColliRimanenti = r.ColliRimanenti,
                        r.Prezzo,
                        PrezzoNetto = r.PrezzoNetto,
                        r.ValoreRiga,
                        ValoreNetto = r.ValoreNetto,

                        InfoQuantita = r.InfoQuantita,
                        InfoEvasione = r.InfoEvasione,
                        RagioneSocialeCliente = r.Testata != null && r.Testata.Cliente != null ? r.Testata.Cliente.RagioneSociale : null
                    })
                    .ToListAsync();

                return Json(new { data = righe });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante il caricamento dei dati JSON delle righe ordini");
                return Json(new { error = "Errore durante il caricamento dei dati" });
            }
        }

        /// <summary>
        /// API per ottenere statistiche aggregate delle righe ordini
        /// GET: OrdiniRighe/GetStatistiche
        /// </summary>
        /// <returns>Statistiche delle righe ordini in formato JSON</returns>
        [HttpGet]
        public async Task<IActionResult> GetStatistiche()
        {
            try
            {
                var statistiche = await _context.OrdiniRighe
                    .GroupBy(r => 1)
                    .Select(g => new
                    {
                        TotaleRighe = g.Count(),
                        RigheDaEvadere = g.Count(r => r.QuantitaEvasa == 0),
                        RigheParziali = g.Count(r => r.QuantitaEvasa > 0 && r.QuantitaEvasa < r.Quantita),
                        RigheEvase = g.Count(r => r.QuantitaEvasa >= r.Quantita),
                        QuantitaTotale = g.Sum(r => r.Quantita),
                        QuantitaEvasa = g.Sum(r => r.QuantitaEvasa),
                        QuantitaRimanente = g.Sum(r => r.Quantita - r.QuantitaEvasa),
                        ColliTotali = g.Sum(r => r.NumeroColli),
                        ColliEvasi = g.Sum(r => r.ColliEvasi),
                        ValoreTotale = g.Sum(r => r.ValoreRiga),
                        ArticoliDistinti = g.Select(r => r.CodiceArticolo).Distinct().Count(),
                        MagazziniDistinti = g.Select(r => r.CodiceMagazzino).Distinct().Count(),
                        OrdiniDistinti = g.Select(r => new { r.TipoOrdine, r.AnnoOrdine, r.SerieOrdine, r.NumeroOrdine }).Distinct().Count()
                    })
                    .FirstOrDefaultAsync();

                // Statistiche per tipo ordine
                var statisticheTipo = await _context.OrdiniRighe
                    .GroupBy(r => r.TipoOrdine)
                    .Select(g => new
                    {
                        TipoOrdine = g.Key,
                        Conteggio = g.Count(),
                        QuantitaTotale = g.Sum(r => r.Quantita),
                        QuantitaEvasa = g.Sum(r => r.QuantitaEvasa),
                        ValoreTotale = g.Sum(r => r.ValoreRiga)
                    })
                    .ToListAsync();

                // Statistiche per stato evasione
                var statisticheStato = await _context.OrdiniRighe
                    .GroupBy(r => r.QuantitaEvasa == 0 ? "Da Evadere" : 
                                 r.QuantitaEvasa < r.Quantita ? "Parzialmente Evasa" : "Completamente Evasa")
                    .Select(g => new
                    {
                        StatoEvasione = g.Key,
                        Conteggio = g.Count(),
                        QuantitaTotale = g.Sum(r => r.Quantita),
                        ValoreTotale = g.Sum(r => r.ValoreRiga)
                    })
                    .ToListAsync();

                var risultato = new
                {
                    Generali = statistiche,
                    PerTipo = statisticheTipo,
                    PerStato = statisticheStato
                };

                return Json(risultato);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante il caricamento delle statistiche delle righe ordini");
                return Json(new { error = "Errore durante il caricamento delle statistiche" });
            }
        }

        /// <summary>
        /// API per cercare righe ordini per autocompletamento
        /// GET: OrdiniRighe/SearchRighe
        /// </summary>
        /// <param name="term">Termine di ricerca</param>
        /// <returns>Lista di righe che corrispondono al termine di ricerca</returns>
        [HttpGet]
        public async Task<IActionResult> SearchRighe(string term)
        {
            try
            {
                if (string.IsNullOrEmpty(term) || term.Length < 2)
                {
                    return Json(new List<object>());
                }

                var righe = await _context.OrdiniRighe
                    .Include(r => r.Testata)
                        .ThenInclude(t => t!.Cliente)
                    .Where(r => r.CodiceArticolo.Contains(term) ||
                               (r.DescrizioneArticolo != null && r.DescrizioneArticolo.Contains(term)) ||
                               r.NumeroOrdine.ToString().Contains(term) ||
                               r.SerieOrdine.Contains(term))
                    .OrderBy(r => r.DataConsegna)
                    .Take(20)
                    .Select(r => new 
                    { 
                        value = r.Id, 
                        label = r.IdentificativoRiga,
                        articolo = r.DescrizioneArticoloCompleta,
                        quantita = r.InfoQuantita,
                        evasione = r.InfoEvasione,
                        statoEvasione = r.StatoEvasione,
                        dataConsegna = r.DataConsegna.ToString("dd/MM/yyyy"),
                        cliente = r.Testata != null && r.Testata.Cliente != null ? r.Testata.Cliente.RagioneSociale : null
                    })
                    .ToListAsync();

                return Json(righe);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante la ricerca righe ordini per termine: {Term}", term);
                return Json(new List<object>());
            }
        }
    }
}
