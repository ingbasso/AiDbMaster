using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AiDbMaster.Data;
using AiDbMaster.ViewModels;
using AiDbMaster.Helpers;
using AiDbMaster.Models;
using Microsoft.AspNetCore.Identity;

namespace AiDbMaster.Controllers
{
    [Authorize]
    public class InterrogazioniDBController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<InterrogazioniDBController> _logger;
        private readonly UserManager<ApplicationUser> _userManager;

        public InterrogazioniDBController(
            ApplicationDbContext context,
            ILogger<InterrogazioniDBController> logger,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _logger = logger;
            _userManager = userManager;
        }

        /// <summary>
        /// Helper: Ottiene il CodiceAgente dell'utente corrente (se è un agente)
        /// </summary>
        private async Task<short?> GetCodiceAgenteUtenteCorrente()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            return currentUser?.CodiceAgente;
        }

        /// <summary>
        /// Pagina principale del menu Interrogazioni DB
        /// </summary>
        public IActionResult Index()
        {
            return View();
        }

        // ========== DISPONIBILITÀ ==========

        /// <summary>
        /// GET: Mostra il form per la ricerca disponibilità
        /// </summary>
        [HttpGet]
        public IActionResult Disponibilita()
        {
            ViewBag.UseFluidContainer = true; // Usa larghezza completa
            var model = new DisponibilitaViewModel
            {
                DataRiferimento = DateTime.Today // Imposta data odierna come default
            };
            return View(model);
        }

        /// <summary>
        /// POST: Esegue la ricerca disponibilità
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Disponibilita(DisponibilitaViewModel model)
        {
            ViewBag.UseFluidContainer = true; // Usa larghezza completa
            if (!string.IsNullOrEmpty(model.CodiceArticolo))
            {
                try
                {
                    // Data di riferimento (se null, usa oggi)
                    var dataRiferimento = model.DataRiferimento ?? DateTime.Today;

                    // Calcola disponibilità articolo principale
                    var risultatiPrincipale = await CalcolaDisponibilitaArticolo(model.CodiceArticolo, dataRiferimento);
                    model.Risultati = risultatiPrincipale.Risultati;
                    model.DescrizioneArticolo = risultatiPrincipale.DescrizioneArticolo;

                    // Cerca articoli sostitutivi solo se richiesto
                    if (model.VisualizzaSostitutivi)
                    {
                        var codiciSostitutivi = await _context.ArticoliSostitutivi
                            .Where(a => a.CodiceArticolo == model.CodiceArticolo || 
                                       a.CodiceArticoloSostitutivo == model.CodiceArticolo)
                            .Select(a => a.CodiceArticolo == model.CodiceArticolo 
                                ? a.CodiceArticoloSostitutivo 
                                : a.CodiceArticolo)
                            .Distinct()
                            .ToListAsync();

                        _logger.LogInformation("Trovati {NumSostitutivi} articoli sostitutivi per {CodiceArticolo}", 
                            codiciSostitutivi.Count, model.CodiceArticolo);

                        // Calcola disponibilità per ogni articolo sostitutivo
                        model.ArticoliSostitutivi = new List<DisponibilitaArticoloGruppoViewModel>();
                        foreach (var codiceSostitutivo in codiciSostitutivi)
                        {
                            var risultatiSostitutivo = await CalcolaDisponibilitaArticolo(codiceSostitutivo, dataRiferimento);
                            
                            if (risultatiSostitutivo.Risultati.Any())
                            {
                                model.ArticoliSostitutivi.Add(new DisponibilitaArticoloGruppoViewModel
                                {
                                    CodiceArticolo = codiceSostitutivo,
                                    DescrizioneArticolo = risultatiSostitutivo.DescrizioneArticolo,
                                    IsArticoloPrincipale = false,
                                    Risultati = risultatiSostitutivo.Risultati
                                });
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Errore durante la ricerca disponibilità per articolo {CodiceArticolo}", model.CodiceArticolo);
                }
            }

            return View(model);
        }

        /// <summary>
        /// Calcola la disponibilità per un singolo articolo (metodo helper)
        /// </summary>
        private async Task<(List<DisponibilitaRigaViewModel> Risultati, string DescrizioneArticolo)> 
            CalcolaDisponibilitaArticolo(string codiceArticolo, DateTime dataRiferimento)
        {
            var oggi = DateTime.Today;
            
            // Calcola impegnato attuale in tempo reale
            // Formula: Σ ((Quantita - QuantitaEvasa) × PercentualeInclusione / 100)
            // per ordini clienti con DataConsegna <= oggi e residuo > 0
            decimal impegnatoAttuale = await _context.OrdiniRighe
                .Where(r => r.TipoOrdine == "R")
                .Where(r => r.CodiceArticolo == codiceArticolo)
                .Where(r => r.DataConsegna <= oggi)
                .Where(r => r.Quantita > r.QuantitaEvasa)
                .SumAsync(r => (decimal?)((r.Quantita - r.QuantitaEvasa) * r.PercentualeInclusione / 100m)) ?? 0;
            
            // Calcola impegnato futuro (solo se data > oggi)
            // Formula: Σ ((Quantita - QuantitaEvasa) × PercentualeInclusione / 100)
            // per ordini clienti con DataConsegna > oggi e DataConsegna <= dataRiferimento
            decimal impegnatoFuturo = 0;
            if (dataRiferimento > oggi)
            {
                impegnatoFuturo = await _context.OrdiniRighe
                    .Where(r => r.TipoOrdine == "R")
                    .Where(r => r.CodiceArticolo == codiceArticolo)
                    .Where(r => r.DataConsegna > oggi)
                    .Where(r => r.DataConsegna <= dataRiferimento)
                    .Where(r => r.Quantita > r.QuantitaEvasa)
                    .SumAsync(r => (decimal?)((r.Quantita - r.QuantitaEvasa) * r.PercentualeInclusione / 100m)) ?? 0;
            }

            // Carica tempi asciugatura in memoria
            var tempiAsciugatura = await _context.TempiAsciugatura
                .ToDictionaryAsync(t => t.IdMese, t => t.GiorniAsciugatura);

            // Calcola produzione disponibile (solo se data > oggi)
            decimal produzioneDisponibile = 0;
            if (dataRiferimento > oggi)
            {
                var produzioni = await _context.ListaOP
                    .Where(op => op.CodiceArticolo == codiceArticolo)
                    .Where(op => op.DataFinePrevista != null)
                    .Where(op => op.DataFinePrevista > oggi)
                    .Select(op => new
                    {
                        op.Quantita,
                        op.DataFinePrevista,
                        Mese = op.DataFinePrevista!.Value.Month
                    })
                    .ToListAsync();

                foreach (var prod in produzioni)
                {
                    if (prod.DataFinePrevista.HasValue)
                    {
                        var giorniAsciugatura = tempiAsciugatura.GetValueOrDefault(prod.Mese, 0);
                        var dataDisponibilita = prod.DataFinePrevista.Value.AddDays(giorniAsciugatura);

                        if (dataDisponibilita <= dataRiferimento)
                        {
                            produzioneDisponibile += prod.Quantita;
                        }
                    }
                }
            }

            // Query sulla tabella ProgressiviArticoli (solo magazzino 1)
            var progressivi = await _context.ProgressiviArticoli
                .Where(p => p.CodiceArticolo == codiceArticolo)
                .Where(p => p.CodiceMagazzino == 1)
                .OrderBy(p => p.CodiceMagazzino)
                .ToListAsync();

            // Recupera descrizione e unità di misura
            var articolo = await _context.AnagraficaArticoli
                .FirstOrDefaultAsync(a => a.CodiceArticolo == codiceArticolo);
            
            var descrizioneArticolo = articolo?.Descrizione ?? "";
            var unitaMisura = articolo?.UnitaMisura ?? "";

            // Mappa i risultati
            // Nota: ImpegnatoAttuale viene calcolato in tempo reale (non da ProgressiviArticoli)
            var risultati = progressivi.Select(p => new DisponibilitaRigaViewModel
            {
                CodiceArticolo = p.CodiceArticolo,
                DescrizioneArticolo = descrizioneArticolo,
                UnitaMisura = unitaMisura,
                CodiceMagazzino = p.CodiceMagazzino,
                Esistenza = p.Esistenza,
                ImpegnatoAttuale = impegnatoAttuale,  // Calcolato in tempo reale con PercentualeInclusione
                ImpegnatoFuturo = impegnatoFuturo,
                OrdinatoFornitoriDataOdierna = p.OrdinatoFornitoriDataOdierna,
                ProduzioneDisponibile = produzioneDisponibile
            }).ToList();

            return (risultati, descrizioneArticolo);
        }

        /// <summary>
        /// API per il dropdown autocompletante degli articoli
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> SearchArticoli(string term)
        {
            if (string.IsNullOrEmpty(term) || term.Length < 2)
                return Json(new List<object>());

            try
            {
                var articoli = await _context.AnagraficaArticoli
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
                _logger.LogError(ex, "Errore durante la ricerca articoli con term: {Term}", term);
                return Json(new List<object>());
            }
        }

        /// <summary>
        /// API per ottenere il dettaglio delle produzioni programmate
        /// Mostra quali ordini di produzione saranno disponibili entro la data di riferimento
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetDettaglioProduzioni(string codiceArticolo, DateTime? dataRiferimento)
        {
            if (string.IsNullOrEmpty(codiceArticolo))
            {
                return BadRequest(new { error = "Codice articolo richiesto" });
            }

            try
            {
                var dataRif = dataRiferimento ?? DateTime.Today;
                var oggi = DateTime.Today;

                // Carica tempi asciugatura in memoria
                var tempiAsciugatura = await _context.TempiAsciugatura
                    .ToDictionaryAsync(t => t.IdMese, t => t.GiorniAsciugatura);

                // Query produzioni programmate
                var produzioni = await _context.ListaOP
                    .Where(op => op.CodiceArticolo == codiceArticolo)
                    .Where(op => op.DataFinePrevista != null)
                    .Where(op => op.DataFinePrevista > oggi)
                    .Select(op => new
                    {
                        op.AnnoOrdine,
                        op.NumeroOrdine,
                        op.Quantita,
                        op.DataFinePrevista,
                        Mese = op.DataFinePrevista!.Value.Month
                    })
                    .OrderBy(op => op.DataFinePrevista)
                    .ToListAsync();

                // Calcola dettaglio con tempi asciugatura
                var produzioniDettaglio = new List<ProduzioneDettaglioViewModel>();
                decimal totaleDisponibile = 0;
                decimal totaleNonDisponibile = 0;
                int numOrdiniDisponibili = 0;
                int numOrdiniNonDisponibili = 0;

                foreach (var prod in produzioni)
                {
                    if (prod.DataFinePrevista.HasValue)
                    {
                        var giorniAsciugatura = tempiAsciugatura.GetValueOrDefault(prod.Mese, 0);
                        var dataDisponibilita = prod.DataFinePrevista.Value.AddDays(giorniAsciugatura);
                        var disponibile = dataDisponibilita <= dataRif;

                        if (disponibile)
                        {
                            totaleDisponibile += prod.Quantita;
                            numOrdiniDisponibili++;
                        }
                        else
                        {
                            totaleNonDisponibile += prod.Quantita;
                            numOrdiniNonDisponibili++;
                        }

                        produzioniDettaglio.Add(new ProduzioneDettaglioViewModel
                        {
                            NumeroOrdine = $"{prod.AnnoOrdine}/{prod.NumeroOrdine:D4}",
                            Quantita = prod.Quantita,
                            DataFinePrevista = prod.DataFinePrevista.Value,
                            GiorniAsciugatura = giorniAsciugatura,
                            DataDisponibilita = dataDisponibilita,
                            DisponibileEntroData = disponibile
                        });
                    }
                }

                // Recupera descrizione e unità di misura articolo
                var articolo = await _context.AnagraficaArticoli
                    .FirstOrDefaultAsync(a => a.CodiceArticolo == codiceArticolo);

                var response = new DettaglioProduzioniResponse
                {
                    CodiceArticolo = codiceArticolo,
                    DescrizioneArticolo = articolo?.Descrizione ?? "",
                    UnitaMisura = articolo?.UnitaMisura ?? "pz",
                    DataRiferimento = dataRif,
                    Produzioni = produzioniDettaglio,
                    TotaleDisponibile = totaleDisponibile,
                    TotaleNonDisponibile = totaleNonDisponibile,
                    NumeroOrdiniDisponibili = numOrdiniDisponibili,
                    NumeroOrdiniNonDisponibili = numOrdiniNonDisponibili
                };

                _logger.LogInformation(
                    "Dettaglio produzioni per {CodiceArticolo}: {Disponibili} disponibili, {NonDisponibili} non disponibili",
                    codiceArticolo, numOrdiniDisponibili, numOrdiniNonDisponibili);

                return Json(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante il recupero del dettaglio produzioni per {CodiceArticolo}", codiceArticolo);
                return StatusCode(500, new { error = "Errore durante il recupero dei dati" });
            }
        }

        /// <summary>
        /// Recupera il dettaglio degli ordini clienti che compongono l'impegnato futuro
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetDettaglioImpegnatoFuturo(string codiceArticolo, DateTime? dataRiferimento)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(codiceArticolo))
                {
                    return BadRequest(new { error = "Codice articolo obbligatorio" });
                }

                var oggi = DateTime.Today;
                var dataRif = dataRiferimento ?? oggi;

                _logger.LogInformation("Recupero dettaglio impegnato futuro per articolo {CodiceArticolo} da {DataOggi} a {DataRiferimento}", 
                    codiceArticolo, oggi, dataRif);

                // Query ordini clienti impegnati (TipoOrdine = 'R')
                // IMPORTANTE: Usa riga.DataConsegna (non testata.DataConsegna) per essere coerente con il calcolo principale
                // Usa Distinct per evitare duplicati da OrdiniTestate
                var ordini = await (from riga in _context.OrdiniRighe
                                   join testata in _context.OrdiniTestate
                                       on new { riga.AnnoOrdine, riga.NumeroOrdine, riga.TipoOrdine } 
                                       equals new { testata.AnnoOrdine, testata.NumeroOrdine, testata.TipoOrdine }
                                   join cliente in _context.AnagraficaClienti
                                       on testata.CodiceCliente equals cliente.CodiceCliente into clienteGroup
                                   from cliente in clienteGroup.DefaultIfEmpty()  // LEFT JOIN per non perdere ordini senza cliente
                                   where riga.CodiceArticolo == codiceArticolo
                                       && riga.TipoOrdine == "R"
                                       && riga.DataConsegna > oggi  // Usa riga.DataConsegna (come nella query principale)
                                       && riga.DataConsegna <= dataRif
                                       && (riga.Quantita - riga.QuantitaEvasa) > 0  // Solo quantità da evadere
                                   select new OrdineClienteDettaglioViewModel
                                   {
                                       AnnoOrdine = riga.AnnoOrdine,
                                       NumeroOrdine = riga.NumeroOrdine,
                                       DataConsegna = riga.DataConsegna,  // Usa riga.DataConsegna
                                       CodiceCliente = testata.CodiceCliente,
                                       RagioneSociale = cliente != null ? cliente.RagioneSociale ?? "" : "N/D",
                                       Quantita = riga.Quantita,
                                       QuantitaEvasa = riga.QuantitaEvasa,
                                       PercentualeInclusione = riga.PercentualeInclusione
                                   })
                                   .Distinct()  // Evita duplicati da OrdiniTestate
                                   .OrderBy(o => o.DataConsegna)
                                   .ThenBy(o => o.AnnoOrdine)
                                   .ThenBy(o => o.NumeroOrdine)
                                   .ToListAsync();

                // Calcola totale impegnato futuro usando la PercentualeInclusione
                var totaleImpegnatoFuturo = ordini.Sum(o => o.ContributoImpegnato);

                _logger.LogInformation("Trovati {NumeroOrdini} ordini clienti per un totale impegnato futuro di {Totale}", 
                    ordini.Count, totaleImpegnatoFuturo);

                // Recupera descrizione e unità di misura articolo
                var articolo = await _context.AnagraficaArticoli
                    .FirstOrDefaultAsync(a => a.CodiceArticolo == codiceArticolo);

                var response = new DettaglioImpegnatoFuturoResponse
                {
                    CodiceArticolo = codiceArticolo,
                    DescrizioneArticolo = articolo?.Descrizione ?? "",
                    UnitaMisura = articolo?.UnitaMisura ?? "pz",
                    DataRiferimento = dataRif,
                    Ordini = ordini,
                    TotaleImpegnatoFuturo = totaleImpegnatoFuturo
                };

                return Json(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante il recupero del dettaglio impegnato futuro per {CodiceArticolo}", codiceArticolo);
                return StatusCode(500, new { error = "Errore durante il recupero dei dati" });
            }
        }

        /// <summary>
        /// Recupera il dettaglio degli ordini clienti che compongono l'impegnato attuale (alla data odierna)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetDettaglioImpegnatoAttuale(string codiceArticolo)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(codiceArticolo))
                {
                    return BadRequest(new { error = "Codice articolo obbligatorio" });
                }

                var oggi = DateTime.Today;

                _logger.LogInformation("Recupero dettaglio impegnato attuale per articolo {CodiceArticolo} alla data {DataOggi}", 
                    codiceArticolo, oggi);

                // Query ordini clienti impegnati alla data odierna (TipoOrdine = 'R')
                // Filtra per ordini con DataConsegna <= oggi e quantità da evadere > 0
                // Usa Distinct per evitare duplicati da OrdiniTestate
                var ordini = await (from riga in _context.OrdiniRighe
                                   join testata in _context.OrdiniTestate
                                       on new { riga.AnnoOrdine, riga.NumeroOrdine, riga.TipoOrdine } 
                                       equals new { testata.AnnoOrdine, testata.NumeroOrdine, testata.TipoOrdine }
                                   join cliente in _context.AnagraficaClienti
                                       on testata.CodiceCliente equals cliente.CodiceCliente into clienteGroup
                                   from cliente in clienteGroup.DefaultIfEmpty()  // LEFT JOIN per non perdere ordini senza cliente
                                   where riga.CodiceArticolo == codiceArticolo
                                       && riga.TipoOrdine == "R"
                                       && riga.DataConsegna <= oggi  // ✅ Impegnato attuale: <= oggi
                                       && (riga.Quantita - riga.QuantitaEvasa) > 0  // Solo quantità da evadere
                                   select new OrdineClienteDettaglioViewModel
                                   {
                                       AnnoOrdine = riga.AnnoOrdine,
                                       NumeroOrdine = riga.NumeroOrdine,
                                       DataConsegna = riga.DataConsegna,
                                       CodiceCliente = testata.CodiceCliente,
                                       RagioneSociale = cliente != null ? cliente.RagioneSociale ?? "" : "N/D",
                                       Quantita = riga.Quantita,
                                       QuantitaEvasa = riga.QuantitaEvasa,
                                       PercentualeInclusione = riga.PercentualeInclusione
                                   })
                                   .Distinct()  // Evita duplicati da OrdiniTestate
                                   .OrderBy(o => o.DataConsegna)
                                   .ThenBy(o => o.AnnoOrdine)
                                   .ThenBy(o => o.NumeroOrdine)
                                   .ToListAsync();

                // Calcola totale impegnato attuale usando la PercentualeInclusione
                var totaleImpegnatoAttuale = ordini.Sum(o => o.ContributoImpegnato);

                _logger.LogInformation("Trovati {NumeroOrdini} ordini clienti per un totale impegnato attuale di {Totale}", 
                    ordini.Count, totaleImpegnatoAttuale);

                // Recupera descrizione e unità di misura articolo
                var articolo = await _context.AnagraficaArticoli
                    .FirstOrDefaultAsync(a => a.CodiceArticolo == codiceArticolo);

                var response = new DettaglioImpegnatoAttualeResponse
                {
                    CodiceArticolo = codiceArticolo,
                    DescrizioneArticolo = articolo?.Descrizione ?? "",
                    UnitaMisura = articolo?.UnitaMisura ?? "pz",
                    DataRiferimento = oggi,
                    Ordini = ordini,
                    TotaleImpegnatoAttuale = totaleImpegnatoAttuale
                };

                return Json(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante il recupero del dettaglio impegnato attuale per {CodiceArticolo}", codiceArticolo);
                return StatusCode(500, new { error = "Errore durante il recupero dei dati" });
            }
        }

        // ========== CONSEGNE PROGRAMMATE ==========

        /// <summary>
        /// GET: Mostra il form per le consegne programmate
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> ConsegneProgrammate()
        {
            ViewBag.UseFluidContainer = true;
            
            var codiceAgente = await GetCodiceAgenteUtenteCorrente();
            ViewBag.IsAgente = codiceAgente.HasValue;
            if (codiceAgente.HasValue)
            {
                ViewBag.CodiceAgenteUtente = codiceAgente.Value;
                
                // Recupera il nome dell'agente
                var agente = await _context.TabellaAgenti
                    .FirstOrDefaultAsync(a => a.CodiceAgente == codiceAgente.Value);
                ViewBag.NomeAgenteUtente = agente?.DescrizioneAgente ?? $"Agente {codiceAgente.Value}";
                
                _logger.LogInformation("Utente agente (Codice: {CodiceAgente}). Filtro automatico applicato.", codiceAgente.Value);
            }
            
            var oggi = DateTime.Today;
            var model = new ConsegneProgrammateViewModel
            {
                DataConsegnaDa = new DateTime(oggi.Year, oggi.Month, 1),
                DataConsegnaA = new DateTime(oggi.Year, oggi.Month, DateTime.DaysInMonth(oggi.Year, oggi.Month)),
                OrdinamentoPer = "DataConsegna"
            };
            return View(model);
        }

        /// <summary>
        /// POST: Esegue la ricerca consegne programmate
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConsegneProgrammate(ConsegneProgrammateViewModel model)
        {
            ViewBag.UseFluidContainer = true;

            // Applica filtro agente se necessario
            var codiceAgente = await GetCodiceAgenteUtenteCorrente();
            ViewBag.IsAgente = codiceAgente.HasValue;
            if (codiceAgente.HasValue)
            {
                model.CodiceAgente = codiceAgente.Value; // Forza filtro
                ViewBag.CodiceAgenteUtente = codiceAgente.Value;
                
                // Recupera il nome dell'agente
                var agente = await _context.TabellaAgenti
                    .FirstOrDefaultAsync(a => a.CodiceAgente == codiceAgente.Value);
                ViewBag.NomeAgenteUtente = agente?.DescrizioneAgente ?? $"Agente {codiceAgente.Value}";
                
                _logger.LogInformation("Utente agente (Codice: {CodiceAgente}). Filtro applicato nella ricerca.", codiceAgente.Value);
            }

            try
            {
                // Query base: Ordini clienti (TipoOrdine = 'R')
                var query = from testata in _context.OrdiniTestate
                            join cliente in _context.AnagraficaClienti
                                on testata.CodiceCliente equals cliente.CodiceCliente
                            join agente in _context.TabellaAgenti
                                on cliente.CodiceAgente equals agente.CodiceAgente into agenteGroup
                            from agente in agenteGroup.DefaultIfEmpty()
                            where testata.TipoOrdine == "R"
                            select new
                            {
                                Testata = testata,
                                Cliente = cliente,
                                Agente = agente
                            };

                // Filtro per cliente
                if (model.CodiceCliente.HasValue)
                {
                    query = query.Where(x => x.Testata.CodiceCliente == model.CodiceCliente.Value);
                }

                // Filtro per agente (FORZATO se l'utente è un agente)
                if (model.CodiceAgente.HasValue)
                {
                    query = query.Where(x => x.Cliente.CodiceAgente == model.CodiceAgente.Value);
                }

                // Filtro per provincia
                if (!string.IsNullOrWhiteSpace(model.Provincia))
                {
                    query = query.Where(x => x.Cliente.Provincia == model.Provincia);
                }

                // Filtro per comune (città)
                if (!string.IsNullOrWhiteSpace(model.Comune))
                {
                    query = query.Where(x => x.Cliente.Citta != null && x.Cliente.Citta.Contains(model.Comune));
                }

                var testate = await query.ToListAsync();

                // Adesso recupero le righe per ogni testata con filtro data consegna
                var ordiniViewModel = new List<OrdineConsegnaViewModel>();

                // Pre-carica le giacenze (Esistenza) per tutti gli articoli dal Magazzino 1
                var tuttiCodiciArticoli = new HashSet<string>();
                foreach (var item in testate)
                {
                    var codici = await _context.OrdiniRighe
                        .Where(r => r.TipoOrdine == item.Testata.TipoOrdine &&
                                   r.AnnoOrdine == item.Testata.AnnoOrdine &&
                                   r.SerieOrdine == item.Testata.SerieOrdine &&
                                   r.NumeroOrdine == item.Testata.NumeroOrdine)
                        .Select(r => r.CodiceArticolo)
                        .ToListAsync();
                    foreach (var c in codici) tuttiCodiciArticoli.Add(c);
                }

                // Dizionario Esistenza (Giacenza Magazzino 1)
                var esistenzaDict = await _context.ProgressiviArticoli
                    .Where(p => p.CodiceMagazzino == 1 && tuttiCodiciArticoli.Contains(p.CodiceArticolo))
                    .ToDictionaryAsync(p => p.CodiceArticolo, p => p.Esistenza);

                // Dizionario Impegnato Attuale (calcolato come in Disponibilita)
                var oggi = DateTime.Today;
                var impegnatoDict = await _context.OrdiniRighe
                    .Where(r => r.TipoOrdine == "R" && 
                               r.DataConsegna <= oggi && 
                               tuttiCodiciArticoli.Contains(r.CodiceArticolo))
                    .GroupBy(r => r.CodiceArticolo)
                    .Select(g => new 
                    { 
                        CodiceArticolo = g.Key, 
                        ImpegnatoAttuale = g.Sum(r => (r.Quantita - r.QuantitaEvasa) * r.PercentualeInclusione / 100m)
                    })
                    .ToDictionaryAsync(x => x.CodiceArticolo, x => x.ImpegnatoAttuale);

                foreach (var item in testate)
                {
                    // Recupera le righe dell'ordine con filtro data consegna
                    var righeQuery = _context.OrdiniRighe
                        .Where(r => r.TipoOrdine == item.Testata.TipoOrdine)
                        .Where(r => r.AnnoOrdine == item.Testata.AnnoOrdine)
                        .Where(r => r.SerieOrdine == item.Testata.SerieOrdine)
                        .Where(r => r.NumeroOrdine == item.Testata.NumeroOrdine);

                    // Filtro data consegna DA
                    if (model.DataConsegnaDa.HasValue)
                    {
                        righeQuery = righeQuery.Where(r => r.DataConsegna >= model.DataConsegnaDa.Value);
                    }

                    // Filtro data consegna A
                    if (model.DataConsegnaA.HasValue)
                    {
                        righeQuery = righeQuery.Where(r => r.DataConsegna <= model.DataConsegnaA.Value);
                    }

                    var righe = await righeQuery.ToListAsync();

                    // Se l'ordine non ha righe che soddisfano i filtri data, salta
                    if (!righe.Any())
                        continue;

                    // Ricava regione dalla provincia
                    var regione = RegioniHelper.GetRegione(item.Cliente.Provincia);

                    // Filtro per regione (se specificato)
                    if (!string.IsNullOrWhiteSpace(model.Regione) && 
                        !string.Equals(regione, model.Regione, StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    // Crea il ViewModel per l'ordine
                    var ordineViewModel = new OrdineConsegnaViewModel
                    {
                        // Dati testata
                        Id = item.Testata.Id,
                        CodiceCliente = item.Testata.CodiceCliente,
                        TipoOrdine = item.Testata.TipoOrdine,
                        AnnoOrdine = item.Testata.AnnoOrdine,
                        SerieOrdine = item.Testata.SerieOrdine,
                        NumeroOrdine = item.Testata.NumeroOrdine,
                        DataOrdine = item.Testata.DataOrdine,
                        RiferimentoOrdine = item.Testata.RiferimentoOrdine,
                        DataConsegnaTestata = item.Testata.DataConsegna,
                        CodiceAgente = item.Testata.CodiceAgente,
                        NoteTestata = item.Testata.NoteTestata,

                        // Dati cliente
                        RagioneSociale = item.Cliente.RagioneSociale,
                        DescrizioneUlteriore = item.Cliente.DescrizioneUlteriore,
                        Indirizzo = item.Cliente.Indirizzo,
                        Cap = item.Cliente.Cap,
                        Citta = item.Cliente.Citta,
                        Provincia = item.Cliente.Provincia,
                        Regione = regione,
                        CodiceFiscale = item.Cliente.CodiceFiscale,
                        PartitaIva = item.Cliente.PartitaIva,
                        Telefono = item.Cliente.Telefono,

                        // Dati agente
                        NomeAgente = item.Agente?.DescrizioneAgente,

                        // Righe ordine
                        Righe = righe.Select(r => new RigaOrdineConsegnaViewModel
                        {
                            Id = r.Id,
                            TipoOrdine = r.TipoOrdine,
                            AnnoOrdine = r.AnnoOrdine,
                            SerieOrdine = r.SerieOrdine,
                            NumeroOrdine = r.NumeroOrdine,
                            RigaOrdine = r.RigaOrdine,
                            CodiceMagazzino = r.CodiceMagazzino,
                            CodiceArticolo = r.CodiceArticolo,
                            DescrizioneArticolo = r.DescrizioneArticolo,
                            DataConsegna = r.DataConsegna,
                            UnitaMisura = r.UnitaMisura,
                            Quantita = r.Quantita,
                            UnitaMisuraColli = r.UnitaMisuraColli,
                            NumeroColli = r.NumeroColli,
                            ColliEvasi = r.ColliEvasi,
                            QuantitaEvasa = r.QuantitaEvasa,
                            Prezzo = r.Prezzo,
                            PercentualeInclusione = r.PercentualeInclusione,
                            NoteRiga = r.NoteRiga,
                            ValoreRiga = r.ValoreRiga,
                            // Disponibilità Magazzino 1
                            Esistenza = esistenzaDict.ContainsKey(r.CodiceArticolo) ? esistenzaDict[r.CodiceArticolo] : 0,
                            ImpegnatoAttuale = impegnatoDict.ContainsKey(r.CodiceArticolo) ? impegnatoDict[r.CodiceArticolo] : 0
                        }).ToList()
                    };

                    ordiniViewModel.Add(ordineViewModel);
                }

                // Ordinamento
                ordiniViewModel = model.OrdinamentoPer switch
                {
                    "Cliente" => ordiniViewModel.OrderBy(o => o.RagioneSociale).ThenBy(o => o.Righe.Min(r => r.DataConsegna)).ToList(),
                    "Ordine" => ordiniViewModel.OrderBy(o => o.AnnoOrdine).ThenBy(o => o.NumeroOrdine).ToList(),
                    _ => ordiniViewModel.OrderBy(o => o.Righe.Min(r => r.DataConsegna)).ThenBy(o => o.RagioneSociale).ToList() // DataConsegna
                };

                model.Ordini = ordiniViewModel;

                // Informazioni aggiuntive per i filtri selezionati
                if (model.CodiceCliente.HasValue)
                {
                    var cliente = await _context.AnagraficaClienti
                        .FirstOrDefaultAsync(c => c.CodiceCliente == model.CodiceCliente.Value);
                    model.RagioneSocialeCliente = cliente?.RagioneSociale;
                }

                if (model.CodiceAgente.HasValue)
                {
                    var agente = await _context.TabellaAgenti
                        .FirstOrDefaultAsync(a => a.CodiceAgente == model.CodiceAgente.Value);
                    model.NomeAgente = agente?.DescrizioneAgente;
                }

                _logger.LogInformation("Ricerca consegne programmate completata. Trovati {Count} ordini con {RigheCount} righe totali.",
                    model.Ordini.Count, model.Ordini.Sum(o => o.NumeroRighe));

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante la ricerca delle consegne programmate");
                ModelState.AddModelError("", "Si è verificato un errore durante la ricerca. Riprova più tardi.");
                return View(model);
            }
        }

        /// <summary>
        /// API: Ricerca clienti per autocomplete (Select2)
        /// Se l'utente è un agente, restituisce solo i clienti del suo CodiceAgente
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> SearchClienti(string term)
        {
            if (string.IsNullOrWhiteSpace(term) || term.Length < 2)
                return Json(new { results = new List<object>() });

            var query = _context.AnagraficaClienti
                .Where(c => c.CodiceCliente.ToString().Contains(term) || 
                           (c.RagioneSociale != null && c.RagioneSociale.Contains(term)));

            // Applica filtro agente se necessario
            var codiceAgente = await GetCodiceAgenteUtenteCorrente();
            if (codiceAgente.HasValue)
            {
                query = query.Where(c => c.CodiceAgente == codiceAgente.Value);
                _logger.LogInformation("SearchClienti: Filtro agente {CodiceAgente} applicato", codiceAgente.Value);
            }

            var clienti = await query
                .OrderBy(c => c.RagioneSociale)
                .Take(20)
                .Select(c => new { id = c.CodiceCliente, text = $"{c.CodiceCliente} - {c.RagioneSociale}" })
                .ToListAsync();

            return Json(new { results = clienti });
        }

        /// <summary>
        /// API: Ottieni tutte le regioni italiane
        /// </summary>
        [HttpGet]
        public IActionResult GetRegioni()
        {
            var regioni = RegioniHelper.GetTutteLeRegioni();
            return Json(regioni);
        }

        /// <summary>
        /// API: Ottieni tutte le province distinte dai clienti
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetProvince(string? regione)
        {
            IQueryable<string> query = _context.AnagraficaClienti
                .Where(c => c.Provincia != null)
                .Select(c => c.Provincia!);

            // Se è specificata una regione, filtra solo le province di quella regione
            if (!string.IsNullOrWhiteSpace(regione))
            {
                var provinceRegione = RegioniHelper.GetProvincePerRegione(regione);
                query = query.Where(p => provinceRegione.Contains(p));
            }

            var province = await query
                .Distinct()
                .OrderBy(p => p)
                .ToListAsync();

            return Json(province);
        }

        /// <summary>
        /// API: Ottieni tutti i comuni (città) distinti dai clienti
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetComuni(string? provincia)
        {
            IQueryable<string> query = _context.AnagraficaClienti
                .Where(c => c.Citta != null)
                .Select(c => c.Citta!);

            if (!string.IsNullOrWhiteSpace(provincia))
            {
                query = _context.AnagraficaClienti
                    .Where(c => c.Provincia == provincia && c.Citta != null)
                    .Select(c => c.Citta!);
            }

            var comuni = await query
                .Distinct()
                .OrderBy(c => c)
                .ToListAsync();

            return Json(comuni);
        }

        /// <summary>
        /// API: Ottieni tutti gli agenti attivi
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAgenti()
        {
            var agenti = await _context.TabellaAgenti
                .Where(a => a.Attivo)
                .OrderBy(a => a.DescrizioneAgente)
                .Select(a => new
                {
                    codice = a.CodiceAgente,
                    descrizione = a.DescrizioneAgente ?? $"Agente {a.CodiceAgente}"
                })
                .ToListAsync();

            return Json(agenti);
        }
    }
}

