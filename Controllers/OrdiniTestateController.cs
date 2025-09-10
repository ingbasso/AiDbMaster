using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using AiDbMaster.Data;
using AiDbMaster.Models;

namespace AiDbMaster.Controllers
{
    /// <summary>
    /// Controller per la gestione delle Testate Ordini
    /// Fornisce funzionalità di visualizzazione degli ordini (Fornitori "O" e Clienti "R")
    /// </summary>
    [Authorize]
    public class OrdiniTestateController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<OrdiniTestateController> _logger;

        public OrdiniTestateController(
            ApplicationDbContext context,
            ILogger<OrdiniTestateController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Visualizza l'elenco di tutte le testate ordini
        /// GET: OrdiniTestate
        /// </summary>
        /// <param name="search">Termine di ricerca per filtrare gli ordini</param>
        /// <param name="tipoOrdine">Filtro per tipo ordine (O/R)</param>
        /// <param name="annoOrdine">Filtro per anno ordine</param>
        /// <param name="codiceCliente">Filtro per codice cliente</param>
        /// <param name="codiceAgente">Filtro per codice agente</param>
        /// <param name="statoOrdine">Filtro per stato ordine</param>
        /// <param name="dataInizio">Data inizio per filtro periodo</param>
        /// <param name="dataFine">Data fine per filtro periodo</param>
        /// <param name="sortOrder">Ordinamento dei risultati</param>
        /// <param name="page">Numero di pagina per la paginazione</param>
        /// <param name="pageSize">Numero di elementi per pagina</param>
        /// <returns>Vista con l'elenco delle testate ordini</returns>
        public async Task<IActionResult> Index(
            string? search,
            string? tipoOrdine,
            short? annoOrdine,
            int? codiceCliente,
            short? codiceAgente,
            string? statoOrdine,
            DateTime? dataInizio,
            DateTime? dataFine,
            string sortOrder = "data_desc",
            int page = 1,
            int pageSize = 50)
        {
            // Usa container fluid per massimizzare lo spazio
            ViewBag.UseFluidContainer = true;
            
            try
            {
                _logger.LogInformation("Caricamento testate ordini - Pagina: {Page}, Ricerca: {Search}", page, search);

                // Query base con include delle relazioni
                var query = _context.OrdiniTestate
                    .Include(o => o.Cliente)
                    .Include(o => o.Agente)
                    .Include(o => o.Magazzino)
                    .Include(o => o.Righe)
                    .AsQueryable();

                // Filtro per ricerca testuale
                if (!string.IsNullOrEmpty(search))
                {
                    query = query.Where(o => 
                        o.NumeroOrdine.ToString().Contains(search) ||
                        o.SerieOrdine.Contains(search) ||
                        (o.Riferimento != null && o.Riferimento.Contains(search)) ||
                        (o.Cliente != null && o.Cliente.RagioneSociale != null && o.Cliente.RagioneSociale.Contains(search)) ||
                        (o.Note != null && o.Note.Contains(search)));
                }

                // Filtro per tipo ordine
                if (!string.IsNullOrEmpty(tipoOrdine))
                {
                    query = query.Where(o => o.TipoOrdine == tipoOrdine);
                }

                // Filtro per anno ordine
                if (annoOrdine.HasValue)
                {
                    query = query.Where(o => o.AnnoOrdine == annoOrdine.Value);
                }

                // Filtro per codice cliente
                if (codiceCliente.HasValue)
                {
                    query = query.Where(o => o.CodiceCliente == codiceCliente.Value);
                }

                // Filtro per codice agente
                if (codiceAgente.HasValue)
                {
                    query = query.Where(o => o.CodiceAgente == codiceAgente.Value);
                }

                // Filtro per periodo
                if (dataInizio.HasValue)
                {
                    query = query.Where(o => o.DataOrdine >= dataInizio.Value);
                }

                if (dataFine.HasValue)
                {
                    query = query.Where(o => o.DataOrdine <= dataFine.Value);
                }

                // Filtro per stato ordine (basato su data consegna)
                if (!string.IsNullOrEmpty(statoOrdine))
                {
                    var oggi = DateTime.Today;
                    query = statoOrdine switch
                    {
                        "Scaduto" => query.Where(o => o.DataConsegna.HasValue && o.DataConsegna.Value.Date < oggi),
                        "In Scadenza" => query.Where(o => o.DataConsegna.HasValue && o.DataConsegna.Value.Date == oggi),
                        "Prossima Consegna" => query.Where(o => o.DataConsegna.HasValue && o.DataConsegna.Value.Date > oggi && o.DataConsegna.Value.Date <= oggi.AddDays(7)),
                        "Programmato" => query.Where(o => o.DataConsegna.HasValue && o.DataConsegna.Value.Date > oggi.AddDays(7)),
                        "Senza Consegna" => query.Where(o => !o.DataConsegna.HasValue),
                        _ => query
                    };
                }

                // Ordinamento
                query = sortOrder switch
                {
                    "numero" => query.OrderBy(o => o.TipoOrdine).ThenBy(o => o.AnnoOrdine).ThenBy(o => o.SerieOrdine).ThenBy(o => o.NumeroOrdine),
                    "numero_desc" => query.OrderByDescending(o => o.TipoOrdine).ThenByDescending(o => o.AnnoOrdine).ThenByDescending(o => o.SerieOrdine).ThenByDescending(o => o.NumeroOrdine),
                    "data" => query.OrderBy(o => o.DataOrdine),
                    "data_desc" => query.OrderByDescending(o => o.DataOrdine),
                    "cliente" => query.OrderBy(o => o.Cliente!.RagioneSociale),
                    "cliente_desc" => query.OrderByDescending(o => o.Cliente!.RagioneSociale),
                    "tipo" => query.OrderBy(o => o.TipoOrdine),
                    "tipo_desc" => query.OrderByDescending(o => o.TipoOrdine),
                    "consegna" => query.OrderBy(o => o.DataConsegna),
                    "consegna_desc" => query.OrderByDescending(o => o.DataConsegna),
                    "valore" => query.OrderBy(o => o.Righe.Sum(r => r.ValoreRiga)),
                    "valore_desc" => query.OrderByDescending(o => o.Righe.Sum(r => r.ValoreRiga)),
                    _ => query.OrderByDescending(o => o.DataOrdine)
                };

                // Conteggio totale per la paginazione
                var totalCount = await query.CountAsync();

                // Paginazione
                var ordini = await query
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                // Preparazione dei dati per la vista
                ViewBag.CurrentSort = sortOrder;
                ViewBag.CurrentSearch = search;
                ViewBag.CurrentTipoOrdine = tipoOrdine;
                ViewBag.CurrentAnnoOrdine = annoOrdine;
                ViewBag.CurrentCodiceCliente = codiceCliente;
                ViewBag.CurrentCodiceAgente = codiceAgente;
                ViewBag.CurrentStatoOrdine = statoOrdine;
                ViewBag.CurrentDataInizio = dataInizio;
                ViewBag.CurrentDataFine = dataFine;
                ViewBag.CurrentPage = page;
                ViewBag.PageSize = pageSize;
                ViewBag.TotalCount = totalCount;
                ViewBag.TotalPages = (int)Math.Ceiling((double)totalCount / pageSize);

                // Dati per i filtri dropdown
                ViewBag.AnniOrdine = await _context.OrdiniTestate
                    .Select(o => o.AnnoOrdine)
                    .Distinct()
                    .OrderByDescending(a => a)
                    .ToListAsync();

                ViewBag.Clienti = await _context.AnagraficaClienti
                    .Where(c => _context.OrdiniTestate.Any(o => o.CodiceCliente == c.CodiceCliente))
                    .OrderBy(c => c.RagioneSociale)
                    .Select(c => new { c.CodiceCliente, c.RagioneSociale })
                    .ToListAsync();

                ViewBag.Agenti = await _context.TabellaAgenti
                    .Where(a => _context.OrdiniTestate.Any(o => o.CodiceAgente == a.CodiceAgente))
                    .OrderBy(a => a.DescrizioneAgente)
                    .Select(a => new { a.CodiceAgente, a.DescrizioneAgente })
                    .ToListAsync();

                // Statistiche aggiuntive
                var dataOggi = DateTime.Today;
                var statistiche = await _context.OrdiniTestate
                    .Where(o => o.TipoOrdine == tipoOrdine) // Filtra per tipo ordine corrente
                    .GroupBy(o => 1)
                    .Select(g => new
                    {
                        TotaleOrdini = g.Count(),
                        OrdiniFornitore = g.Count(o => o.TipoOrdine == "O"),
                        OrdiniCliente = g.Count(o => o.TipoOrdine == "R"),
                        OrdiniConConsegna = g.Count(o => o.DataConsegna.HasValue),
                        OrdiniScaduti = g.Count(o => o.DataConsegna.HasValue && o.DataConsegna.Value.Date < dataOggi),
                        OrdiniInScadenza = g.Count(o => o.DataConsegna.HasValue && o.DataConsegna.Value.Date == dataOggi),
                        ValoreTotale = g.SelectMany(o => o.Righe).Sum(r => r.ValoreRiga),
                        ColliTotali = g.Sum(o => o.TotaleColli)
                    })
                    .FirstOrDefaultAsync();

                ViewBag.Statistiche = statistiche;

                _logger.LogInformation("Caricati {Count} ordini su {Total} totali", ordini.Count, totalCount);

                return View(ordini);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante il caricamento delle testate ordini");
                TempData["ErrorMessage"] = "Si è verificato un errore durante il caricamento degli ordini.";
                return View(new List<OrdiniTestate>());
            }
        }

        /// <summary>
        /// Visualizza i dettagli di una testata ordine specifica
        /// GET: OrdiniTestate/Details/5
        /// </summary>
        /// <param name="id">ID della testata ordine da visualizzare</param>
        /// <returns>Vista con i dettagli della testata ordine</returns>
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                _logger.LogWarning("Tentativo di accesso ai dettagli testata ordine senza ID");
                return NotFound();
            }

            try
            {
                var ordine = await _context.OrdiniTestate
                    .Include(o => o.Cliente)
                    .Include(o => o.Agente)
                    .Include(o => o.Magazzino)
                    .Include(o => o.Righe)
                        .ThenInclude(r => r.Articolo)
                    .Include(o => o.Righe)
                        .ThenInclude(r => r.Magazzino)
                    .FirstOrDefaultAsync(o => o.Id == id);

                if (ordine == null)
                {
                    _logger.LogWarning("Testata ordine con ID {Id} non trovata", id);
                    return NotFound();
                }

                // Statistiche delle righe
                var statisticheRighe = ordine.Righe.GroupBy(r => 1).Select(g => new
                {
                    TotaleRighe = g.Count(),
                    RigheEvase = g.Count(r => r.QuantitaEvasa >= r.Quantita),
                    RigheParziali = g.Count(r => r.QuantitaEvasa > 0 && r.QuantitaEvasa < r.Quantita),
                    RigheDaEvadere = g.Count(r => r.QuantitaEvasa <= 0),
                    ValoreTotale = g.Sum(r => r.ValoreRiga),
                    ValoreNetto = g.Sum(r => r.ValoreNetto),
                    QuantitaTotale = g.Sum(r => r.Quantita),
                    QuantitaEvasa = g.Sum(r => r.QuantitaEvasa)
                }).FirstOrDefault();

                ViewBag.StatisticheRighe = statisticheRighe;

                // Altri ordini dello stesso cliente
                ViewBag.AltriOrdiniCliente = await _context.OrdiniTestate
                    .Where(o => o.CodiceCliente == ordine.CodiceCliente && o.Id != ordine.Id)
                    .OrderByDescending(o => o.DataOrdine)
                    .Take(10)
                    .ToListAsync();

                // Ordini dello stesso agente
                ViewBag.OrdiniAgente = await _context.OrdiniTestate
                    .Where(o => o.CodiceAgente == ordine.CodiceAgente && o.Id != ordine.Id)
                    .OrderByDescending(o => o.DataOrdine)
                    .Take(10)
                    .ToListAsync();

                _logger.LogInformation("Visualizzazione dettagli ordine: {NumeroOrdineCompleto}", ordine.NumeroOrdineCompleto);

                return View(ordine);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante il caricamento dei dettagli della testata ordine {Id}", id);
                TempData["ErrorMessage"] = "Si è verificato un errore durante il caricamento dei dettagli dell'ordine.";
                return RedirectToAction(nameof(Index));
            }
        }

        /// <summary>
        /// API per ottenere i dati delle testate ordini in formato JSON
        /// GET: OrdiniTestate/GetOrdiniJson
        /// </summary>
        /// <returns>Dati delle testate ordini in formato JSON</returns>
        [HttpGet]
        public async Task<IActionResult> GetOrdiniJson()
        {
            try
            {
                var ordini = await _context.OrdiniTestate
                    .Include(o => o.Cliente)
                    .Include(o => o.Agente)
                    .Include(o => o.Magazzino)
                    .OrderByDescending(o => o.DataOrdine)
                    .Select(o => new
                    {
                        o.Id,
                        o.TipoOrdine,
                        o.AnnoOrdine,
                        o.SerieOrdine,
                        o.NumeroOrdine,
                        NumeroOrdineCompleto = o.NumeroOrdineCompleto,
                        o.DataOrdine,
                        o.DataConsegna,
                        o.CodiceCliente,
                        RagioneSocialeCliente = o.Cliente != null ? o.Cliente.RagioneSociale : null,
                        o.CodiceAgente,
                        DescrizioneAgente = o.Agente != null ? o.Agente.DescrizioneAgente : null,
                        o.CodiceMagazzino,
                        DescrizioneMagazzino = o.Magazzino != null ? o.Magazzino.DescrizioneMagazzino : null,
                        o.TotaleColli,
                        o.Riferimento,
                        o.Note,
                        DescrizioneTipoOrdine = o.DescrizioneTipoOrdine,
                        StatoOrdine = o.StatoOrdine,
                        GiorniAllaConsegna = o.GiorniAllaConsegna,
                        DescrizioneCompleta = o.DescrizioneCompleta,
                        RiepilogoOrdine = o.RiepilogoOrdine
                    })
                    .ToListAsync();

                return Json(new { data = ordini });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante il caricamento dei dati JSON delle testate ordini");
                return Json(new { error = "Errore durante il caricamento dei dati" });
            }
        }

        /// <summary>
        /// API per ottenere statistiche aggregate delle testate ordini
        /// GET: OrdiniTestate/GetStatistiche
        /// </summary>
        /// <returns>Statistiche delle testate ordini in formato JSON</returns>
        [HttpGet]
        public async Task<IActionResult> GetStatistiche()
        {
            try
            {
                var statistiche = await _context.OrdiniTestate
                    .GroupBy(o => 1)
                    .Select(g => new
                    {
                        TotaleOrdini = g.Count(),
                        OrdiniFornitore = g.Count(o => o.TipoOrdine == "O"),
                        OrdiniCliente = g.Count(o => o.TipoOrdine == "R"),
                        OrdiniConConsegna = g.Count(o => o.DataConsegna.HasValue),
                        OrdiniScaduti = g.Count(o => o.DataConsegna.HasValue && o.DataConsegna.Value.Date < DateTime.Today),
                        OrdiniInScadenza = g.Count(o => o.DataConsegna.HasValue && o.DataConsegna.Value.Date == DateTime.Today),
                        ClientiDistinti = g.Select(o => o.CodiceCliente).Distinct().Count(),
                        AgentiDistinti = g.Select(o => o.CodiceAgente).Distinct().Count(),
                        MagazziniDistinti = g.Select(o => o.CodiceMagazzino).Distinct().Count(),
                        ColliTotali = g.Sum(o => o.TotaleColli),
                        AnnoMinimo = g.Min(o => o.AnnoOrdine),
                        AnnoMassimo = g.Max(o => o.AnnoOrdine)
                    })
                    .FirstOrDefaultAsync();

                // Statistiche per tipo ordine
                var statisticheTipo = await _context.OrdiniTestate
                    .GroupBy(o => o.TipoOrdine)
                    .Select(g => new
                    {
                        TipoOrdine = g.Key,
                        Conteggio = g.Count(),
                        ColliTotali = g.Sum(o => o.TotaleColli)
                    })
                    .ToListAsync();

                // Statistiche per anno
                var statisticheAnno = await _context.OrdiniTestate
                    .GroupBy(o => o.AnnoOrdine)
                    .Select(g => new
                    {
                        Anno = g.Key,
                        Conteggio = g.Count(),
                        ColliTotali = g.Sum(o => o.TotaleColli)
                    })
                    .OrderByDescending(s => s.Anno)
                    .ToListAsync();

                var risultato = new
                {
                    Generali = statistiche,
                    PerTipo = statisticheTipo,
                    PerAnno = statisticheAnno
                };

                return Json(risultato);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante il caricamento delle statistiche delle testate ordini");
                return Json(new { error = "Errore durante il caricamento delle statistiche" });
            }
        }



        /// <summary>
        /// Carica le righe di un ordine specifico via AJAX
        /// GET: OrdiniTestate/GetRigheOrdine
        /// </summary>
        /// <param name="ordineId">ID dell'ordine di cui caricare le righe</param>
        /// <returns>Vista parziale con le righe dell'ordine</returns>
        [HttpGet]
        public async Task<IActionResult> GetRigheOrdine(int ordineId)
        {
            try
            {
                _logger.LogInformation("Inizio caricamento righe per ordine ID: {OrdineId}", ordineId);
                
                // Prima recupero la testata per ottenere i campi di collegamento
                _logger.LogInformation("Recupero testata per ordine ID: {OrdineId}", ordineId);
                
                var testata = await _context.OrdiniTestate
                    .Where(t => t.Id == ordineId)
                    .Select(t => new { t.TipoOrdine, t.AnnoOrdine, t.SerieOrdine, t.NumeroOrdine })
                    .FirstOrDefaultAsync();

                if (testata == null)
                {
                    _logger.LogWarning("Testata ordine non trovata per ID: {OrdineId}", ordineId);
                    return Content($"<div class=\"alert alert-warning\">Testata ordine non trovata per ID: {ordineId}</div>");
                }
                
                _logger.LogInformation("Testata trovata: Tipo={TipoOrdine}, Anno={Anno}, Serie='{Serie}', Numero={Numero}", 
                    testata.TipoOrdine, testata.AnnoOrdine, testata.SerieOrdine ?? "VUOTO", testata.NumeroOrdine);

                // Ora recupero le righe usando i campi di collegamento
                // Gestisco correttamente i valori vuoti per SerieOrdine
                _logger.LogInformation("Cerco righe con: Tipo={Tipo}, Anno={Anno}, Serie='{Serie}', Numero={Numero}", 
                    testata.TipoOrdine, testata.AnnoOrdine, testata.SerieOrdine ?? "VUOTO", testata.NumeroOrdine);
                
                var righe = await _context.OrdiniRighe
                    .Include(r => r.Articolo)
                    .Include(r => r.Magazzino)
                    .Where(r => r.TipoOrdine == testata.TipoOrdine && 
                               r.AnnoOrdine == testata.AnnoOrdine && 
                               (r.SerieOrdine ?? "") == (testata.SerieOrdine ?? "") && 
                               r.NumeroOrdine == testata.NumeroOrdine)
                    .OrderBy(r => r.RigaOrdine)
                    .ToListAsync();

                _logger.LogInformation("Query righe completata. Trovate {Count} righe", righe.Count);

                // Log dei dati per debug (solo prime 3 righe per non intasare i log)
                for (int i = 0; i < Math.Min(3, righe.Count); i++)
                {
                    var riga = righe[i];
                    _logger.LogInformation("Riga {Numero}: Articolo={Articolo}, DataConsegna={Data}, Quantita={Qta}", 
                        riga.RigaOrdine, riga.CodiceArticolo, riga.DataConsegna, riga.Quantita);
                }

                _logger.LogInformation("Trovate {Count} righe per l'ordine {OrdineId} (Tipo: {TipoOrdine}, Anno: {Anno}, Serie: {Serie}, Numero: {Numero})", 
                    righe.Count, ordineId, testata.TipoOrdine, testata.AnnoOrdine, testata.SerieOrdine, testata.NumeroOrdine);

                if (!righe.Any())
                {
                    _logger.LogWarning("Nessuna riga trovata per l'ordine {OrdineId}", ordineId);
                    return PartialView("_RigheOrdineEmpty");
                }



                return PartialView("_RigheOrdinePartial", righe);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante il caricamento delle righe per l'ordine {OrdineId}. Dettagli: {Message}", ordineId, ex.Message);
                
                // Restituisco un messaggio di errore più dettagliato per il debug
                return Content($"<div class=\"alert alert-danger\"><h5>Errore nel caricamento righe ordine {ordineId}</h5><p><strong>Errore:</strong> {ex.Message}</p><p><strong>Tipo:</strong> {ex.GetType().Name}</p></div>");
            }
        }





        /// <summary>
        /// API per cercare testate ordini per autocompletamento
        /// GET: OrdiniTestate/SearchOrdini
        /// </summary>
        /// <param name="term">Termine di ricerca</param>
        /// <returns>Lista di ordini che corrispondono al termine di ricerca</returns>
        [HttpGet]
        public async Task<IActionResult> SearchOrdini(string term)
        {
            try
            {
                if (string.IsNullOrEmpty(term) || term.Length < 2)
                {
                    return Json(new List<object>());
                }

                var ordini = await _context.OrdiniTestate
                    .Include(o => o.Cliente)
                    .Where(o => o.NumeroOrdine.ToString().Contains(term) ||
                               o.SerieOrdine.Contains(term) ||
                               (o.Riferimento != null && o.Riferimento.Contains(term)) ||
                               (o.Cliente != null && o.Cliente.RagioneSociale != null && o.Cliente.RagioneSociale.Contains(term)))
                    .OrderByDescending(o => o.DataOrdine)
                    .Take(20)
                    .Select(o => new 
                    { 
                        value = o.Id, 
                        label = o.NumeroOrdineCompleto,
                        descrizione = o.DescrizioneCompleta,
                        tipoOrdine = o.TipoOrdine,
                        cliente = o.Cliente != null ? o.Cliente.RagioneSociale : null,
                        dataOrdine = o.DataOrdine.ToString("dd/MM/yyyy")
                    })
                    .ToListAsync();

                return Json(ordini);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante la ricerca testate ordini per termine: {Term}", term);
                return Json(new List<object>());
            }
        }
    }
}
