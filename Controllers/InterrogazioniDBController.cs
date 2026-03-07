using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AiDbMaster.Data;
using AiDbMaster.ViewModels;
using AiDbMaster.Helpers;
using AiDbMaster.Models;
using AiDbMaster.Services;
using Microsoft.AspNetCore.Identity;

namespace AiDbMaster.Controllers
{
    [Authorize]
    public class InterrogazioniDBController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<InterrogazioniDBController> _logger;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly EmailService _emailService;

        public InterrogazioniDBController(
            ApplicationDbContext context,
            ILogger<InterrogazioniDBController> logger,
            UserManager<ApplicationUser> userManager,
            EmailService emailService)
        {
            _context = context;
            _logger = logger;
            _userManager = userManager;
            _emailService = emailService;
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
        /// Helper: Ottiene i giorni impegno dalla tabella ParametriChiave
        /// </summary>
        private async Task<int> GetGiorniImpegno()
        {
            var parametri = await _context.ParametriChiave.FirstOrDefaultAsync();
            return parametri?.GiorniImpegno ?? 0; // Default 0 se non configurato
        }

        /// <summary>
        /// GET: Mostra il form per la ricerca disponibilità
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Disponibilita()
        {
            ViewBag.UseFluidContainer = true; // Usa larghezza completa
            
            // Legge i giorni impegno dalla tabella ParametriChiave
            var giorniImpegno = await GetGiorniImpegno();
            var oggi = DateTime.Today;
            
            var model = new DisponibilitaViewModel
            {
                DataInizio = oggi,
                DataFine = oggi.AddDays(giorniImpegno),
                GiorniImpegno = giorniImpegno
            };
            return View(model);
        }

        /// <summary>
        /// POST: Esegue la ricerca disponibilità
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Disponibilita(DisponibilitaViewModel model, string? submitAction)
        {
            ViewBag.UseFluidContainer = true; // Usa larghezza completa
            
            // Legge i giorni impegno dalla tabella ParametriChiave
            var giorniImpegno = await GetGiorniImpegno();
            var oggi = DateTime.Today;
            
            // Imposta le date nel model
            model.DataInizio = oggi;
            model.DataFine = oggi.AddDays(giorniImpegno);
            model.GiorniImpegno = giorniImpegno;
            
            // Data di riferimento per i calcoli
            var dataRiferimento = model.DataFine;

            // ===== RICERCA OUTLET =====
            if (submitAction == "outlet")
            {
                model.RicercaOutlet = true;
                try
                {
                    List<string> codiciOutletTutti;

                    if (model.OutletSoloSelezionato && !string.IsNullOrEmpty(model.CodiceArticolo))
                    {
                        // Modalità "Solo Selezionato": usa solo l'articolo scelto nel filtro
                        codiciOutletTutti = new List<string> { model.CodiceArticolo };
                        _logger.LogInformation("Ricerca Outlet solo per articolo selezionato: {CodiceArticolo}", model.CodiceArticolo);
                    }
                    else
                    {
                        // Modalità standard: tutti gli articoli Outlet
                        var codiciOutletAnagrafica = await _context.AnagraficaArticoli
                            .Where(a => a.Outlet == "S")
                            .Select(a => a.CodiceArticolo)
                            .ToListAsync();

                        var codiciOutletMag20 = await _context.ProgressiviArticoli
                            .Where(p => p.CodiceMagazzino == 20 && p.Esistenza > 0)
                            .Select(p => p.CodiceArticolo)
                            .ToListAsync();

                        codiciOutletTutti = codiciOutletAnagrafica
                            .Union(codiciOutletMag20)
                            .Distinct()
                            .ToList();

                        _logger.LogInformation("Trovati {NumOutlet} articoli Outlet (anagrafica: {NumAnagrafica}, magazzino 20: {NumMag20})", 
                            codiciOutletTutti.Count, codiciOutletAnagrafica.Count, codiciOutletMag20.Count);
                    }

                    // Recupera i dati anagrafici con lo sconto Outlet diretto dall'anagrafica
                    var articoliOutlet = await _context.AnagraficaArticoli
                        .Where(a => codiciOutletTutti.Contains(a.CodiceArticolo))
                        .OrderBy(a => a.CodiceArticolo)
                        .Select(a => new { 
                            a.CodiceArticolo, 
                            PercSconto = a.PercScontoOutlet
                        })
                        .ToListAsync();

                    // Calcola disponibilità per ogni articolo outlet
                    model.ArticoliOutlet = new List<DisponibilitaArticoloGruppoViewModel>();
                    foreach (var artOutlet in articoliOutlet)
                    {
                        var risultatiOutlet = await CalcolaDisponibilitaArticolo(artOutlet.CodiceArticolo, dataRiferimento, includiMagazzino20: true);
                        
                        model.ArticoliOutlet.Add(new DisponibilitaArticoloGruppoViewModel
                        {
                            CodiceArticolo = artOutlet.CodiceArticolo,
                            DescrizioneArticolo = risultatiOutlet.DescrizioneArticolo,
                            IsArticoloPrincipale = false,
                            Risultati = risultatiOutlet.Risultati,
                            PercScontoExtra = artOutlet.PercSconto
                        });
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Errore durante la ricerca disponibilità articoli Outlet");
                }

                return View(model);
            }

            // ===== RICERCA ARTICOLO SINGOLO =====
            if (!string.IsNullOrEmpty(model.CodiceArticolo))
            {
                try
                {
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
        /// Calcola la disponibilità per un singolo articolo (metodo helper).
        /// Se includiMagazzino20 = true, mostra anche il magazzino 20 e calcola l'impegnato per magazzino.
        /// </summary>
        private async Task<(List<DisponibilitaRigaViewModel> Risultati, string DescrizioneArticolo)> 
            CalcolaDisponibilitaArticolo(string codiceArticolo, DateTime dataRiferimento, bool includiMagazzino20 = false)
        {
            var oggi = DateTime.Today;
            
            // === IMPEGNATO ===
            decimal impegnatoGlobale = 0;
            var impegnatoPerMagazzino = new Dictionary<short, decimal>();

            if (includiMagazzino20)
            {
                // Calcola impegnato raggruppato per CodiceMagazzino delle righe ordine
                var impegnati = await _context.OrdiniRighe
                    .Where(r => r.TipoOrdine == "R")
                    .Where(r => r.CodiceArticolo == codiceArticolo)
                    .Where(r => r.DataConsegna <= dataRiferimento)
                    .Where(r => r.Quantita > r.QuantitaEvasa)
                    .GroupBy(r => r.CodiceMagazzino)
                    .Select(g => new
                    {
                        CodiceMagazzino = g.Key,
                        Impegnato = g.Sum(r => (decimal?)((r.Quantita - r.QuantitaEvasa) * r.PercentualeInclusione / 100m)) ?? 0
                    })
                    .ToListAsync();

                foreach (var imp in impegnati)
                {
                    impegnatoPerMagazzino[imp.CodiceMagazzino] = imp.Impegnato;
                }
            }
            else
            {
                // Comportamento standard: impegnato globale (senza filtro magazzino)
                impegnatoGlobale = await _context.OrdiniRighe
                    .Where(r => r.TipoOrdine == "R")
                    .Where(r => r.CodiceArticolo == codiceArticolo)
                    .Where(r => r.DataConsegna <= dataRiferimento)
                    .Where(r => r.Quantita > r.QuantitaEvasa)
                    .SumAsync(r => (decimal?)((r.Quantita - r.QuantitaEvasa) * r.PercentualeInclusione / 100m)) ?? 0;
            }
            
            decimal impegnatoFuturo = 0;

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

            // === PROGRESSIVI ARTICOLI ===
            var progressivi = await _context.ProgressiviArticoli
                .Where(p => p.CodiceArticolo == codiceArticolo)
                .Where(p => includiMagazzino20 
                    ? (p.CodiceMagazzino == 1 || p.CodiceMagazzino == 20) 
                    : p.CodiceMagazzino == 1)
                .OrderBy(p => p.CodiceMagazzino)
                .ToListAsync();

            // Recupera descrizione e unità di misura
            var articolo = await _context.AnagraficaArticoli
                .FirstOrDefaultAsync(a => a.CodiceArticolo == codiceArticolo);
            
            var descrizioneArticolo = articolo?.Descrizione ?? "";
            var unitaMisura = articolo?.UnitaMisura ?? "";

            // Mappa i risultati
            var isFuoriProduzione = articolo?.FuoriProduzione == "S";
            var isSupermarket = articolo?.Supermarket == "S";

            var risultati = progressivi.Select(p => new DisponibilitaRigaViewModel
            {
                CodiceArticolo = p.CodiceArticolo,
                DescrizioneArticolo = descrizioneArticolo,
                UnitaMisura = unitaMisura,
                CodiceMagazzino = p.CodiceMagazzino,
                Esistenza = p.Esistenza,
                Pronto = p.Pronto,
                ImpegnatoAttuale = includiMagazzino20
                    ? impegnatoPerMagazzino.GetValueOrDefault(p.CodiceMagazzino, 0)
                    : impegnatoGlobale,
                ImpegnatoFuturo = impegnatoFuturo,
                OrdinatoFornitoriDataOdierna = p.OrdinatoFornitoriDataOdierna,
                ProduzioneDisponibile = (!includiMagazzino20 || p.CodiceMagazzino == 1) ? produzioneDisponibile : 0,
                IsFuoriProduzione = isFuoriProduzione,
                IsSupermarket = isSupermarket
            }).ToList();

            // Calcola le note dinamiche (solo per magazzino 1; per il 20 la nota non è significativa)
            foreach (var riga in risultati)
            {
                if (!includiMagazzino20 || riga.CodiceMagazzino == 1)
                {
                    var nota = await CalcolaNotaPrevisione(codiceArticolo, riga.Esistenza, riga.UnitaMisura, dataRiferimento);
                    riga.NotaPrevisione = nota.Nota;
                    riga.DisponibilitaDaVerificare = nota.DaVerificare;
                }
            }

            return (risultati, descrizioneArticolo);
        }

        /// <summary>
        /// Classe helper per rappresentare un movimento nella timeline
        /// </summary>
        private class MovimentoTimeline
        {
            public DateTime Data { get; set; }
            public string Tipo { get; set; } = string.Empty; // "Esistenza", "OrdineCliente", "OrdineProduzione"
            public decimal Quantita { get; set; } // positivo = entrata, negativo = uscita
            public int? GiorniAsciugatura { get; set; } // solo per produzioni
            public string Descrizione { get; set; } = string.Empty;
        }

        /// <summary>
        /// Calcola la nota di previsione dinamica per un articolo
        /// Analizza la timeline di movimenti e genera una nota descrittiva
        /// </summary>
        private async Task<(string Nota, bool DaVerificare)> CalcolaNotaPrevisione(
            string codiceArticolo, decimal esistenza, string unitaMisura, DateTime dataRiferimento)
        {
            var oggi = DateTime.Today;
            var movimenti = new List<MovimentoTimeline>();

            // 1. Aggiungi esistenza come primo movimento
            movimenti.Add(new MovimentoTimeline
            {
                Data = oggi,
                Tipo = "Esistenza",
                Quantita = esistenza,
                Descrizione = "Esistenza iniziale"
            });

            // 2. Carica ordini cliente (TipoOrdine = 'R') aperti fino alla data di riferimento
            // Include anche ordini arretrati (DataConsegna < oggi) per mostrare la storia completa
            var ordiniCliente = await _context.OrdiniRighe
                .Where(r => r.TipoOrdine == "R")
                .Where(r => r.CodiceArticolo == codiceArticolo)
                .Where(r => r.DataConsegna <= dataRiferimento)
                .Where(r => r.Quantita > r.QuantitaEvasa)
                .Select(r => new
                {
                    r.DataConsegna,
                    Residuo = (r.Quantita - r.QuantitaEvasa) * r.PercentualeInclusione / 100m
                })
                .ToListAsync();

            foreach (var ordine in ordiniCliente)
            {
                movimenti.Add(new MovimentoTimeline
                {
                    Data = ordine.DataConsegna,
                    Tipo = "OrdineCliente",
                    Quantita = -ordine.Residuo, // negativo perché sottrae
                    Descrizione = "Ordine Cliente"
                });
            }

            // 3. Carica tempi asciugatura
            var tempiAsciugatura = await _context.TempiAsciugatura
                .ToDictionaryAsync(t => t.IdMese, t => t.GiorniAsciugatura);

            // 4. Carica ordini produzione (stati: Emesso=1, In Produzione=2)
            var ordiniProduzione = await _context.ListaOP
                .Where(op => op.CodiceArticolo == codiceArticolo)
                .Where(op => op.IdStato == 1 || op.IdStato == 2) // Emesso o In Produzione
                .Where(op => op.DataFinePrevista != null)
                .Select(op => new
                {
                    op.Quantita,
                    op.QuantitaProdotta,
                    op.DataFinePrevista,
                    Mese = op.DataFinePrevista!.Value.Month
                })
                .ToListAsync();

            foreach (var prod in ordiniProduzione)
            {
                if (prod.DataFinePrevista.HasValue)
                {
                    var giorniAsciugatura = tempiAsciugatura.GetValueOrDefault(prod.Mese, 0);
                    var dataDisponibilita = prod.DataFinePrevista.Value.AddDays(giorniAsciugatura);

                    // Solo se la data disponibilità è nel periodo di analisi
                    if (dataDisponibilita >= oggi && dataDisponibilita <= dataRiferimento)
                    {
                        // Quantità rimanente da produrre
                        var quantitaRimanente = prod.Quantita - prod.QuantitaProdotta;
                        if (quantitaRimanente > 0)
                        {
                            movimenti.Add(new MovimentoTimeline
                            {
                                Data = dataDisponibilita,
                                Tipo = "OrdineProduzione",
                                Quantita = quantitaRimanente, // positivo perché aggiunge
                                GiorniAsciugatura = giorniAsciugatura,
                                Descrizione = $"Produzione (+{giorniAsciugatura}gg asc.)"
                            });
                        }
                    }
                }
            }

            // 5. Ordina movimenti: esistenza sempre PRIMA come punto di partenza,
            // poi tutti gli altri per data (produzioni prima degli ordini cliente a parità di data)
            movimenti = movimenti
                .OrderBy(m => m.Tipo == "Esistenza" ? 0 : 1) // Esistenza sempre prima
                .ThenBy(m => m.Data)
                .ThenBy(m => m.Tipo == "OrdineProduzione" ? 0 : 1) // Produzioni prima a parità di data
                .ToList();

            // 6. Calcola progressivo
            decimal progressivo = 0;
            var progressiviConData = new List<(DateTime Data, decimal Progressivo, string Tipo, int? GiorniAsc)>();
            
            foreach (var mov in movimenti)
            {
                if (mov.Tipo == "Esistenza")
                {
                    progressivo = mov.Quantita; // L'esistenza imposta il valore iniziale
                }
                else
                {
                    progressivo += mov.Quantita;
                }
                
                progressiviConData.Add((mov.Data, progressivo, mov.Tipo, mov.GiorniAsciugatura));
            }

            // 7. Analizza e genera la nota
            return GeneraNotaDaProgressivi(progressiviConData, unitaMisura, oggi);
        }

        /// <summary>
        /// Genera la nota testuale analizzando i progressivi calcolati
        /// </summary>
        private (string Nota, bool DaVerificare) GeneraNotaDaProgressivi(
            List<(DateTime Data, decimal Progressivo, string Tipo, int? GiorniAsc)> progressivi,
            string unitaMisura,
            DateTime oggi)
        {
            if (!progressivi.Any())
            {
                return ("Nessun dato disponibile", true);
            }

            // Trova le produzioni (punti di svolta)
            var produzioni = progressivi
                .Where(p => p.Tipo == "OrdineProduzione")
                .Select(p => p.Data)
                .Distinct()
                .OrderBy(d => d)
                .ToList();

            // Verifica se il progressivo va mai in negativo
            bool vaInNegativo = progressivi.Any(p => p.Progressivo < 0);
            
            // Trova l'ultimo progressivo
            var ultimoProgressivo = progressivi.Last().Progressivo;

            // CASO 1: Termina in negativo senza recupero
            if (ultimoProgressivo < 0)
            {
                return ("Disponibilità da verificare", true);
            }

            // CASO 2: Va in negativo ma poi recupera
            if (vaInNegativo)
            {
                // Trova l'ultima volta che torna positivo (grazie a una produzione)
                DateTime? dataUltimaRipresa = null;
                for (int i = progressivi.Count - 1; i >= 0; i--)
                {
                    if (progressivi[i].Progressivo < 0)
                    {
                        // Il punto successivo è dove torna positivo
                        if (i + 1 < progressivi.Count)
                        {
                            dataUltimaRipresa = progressivi[i + 1].Data;
                        }
                        break;
                    }
                }

                if (dataUltimaRipresa.HasValue)
                {
                    // Calcola il minimo progressivo dalla ripresa in poi
                    var minimoDopoRipresa = progressivi
                        .Where(p => p.Data >= dataUltimaRipresa.Value)
                        .Min(p => p.Progressivo);

                    // Verifica se ci sono altre produzioni dopo
                    var produzioniSuccessive = produzioni.Where(d => d > dataUltimaRipresa.Value).ToList();
                    
                    if (produzioniSuccessive.Any())
                    {
                        // Genera nota con più livelli
                        var notaParts = new List<string>();
                        notaParts.Add($"Disponibili {minimoDopoRipresa:N0} {unitaMisura} dal {dataUltimaRipresa.Value:dd/MM/yyyy}");
                        
                        foreach (var dataProd in produzioniSuccessive)
                        {
                            var minimoDopoQuestaProd = progressivi
                                .Where(p => p.Data >= dataProd)
                                .Min(p => p.Progressivo);
                            notaParts.Add($"Disponibili {minimoDopoQuestaProd:N0} {unitaMisura} dal {dataProd:dd/MM/yyyy}");
                        }
                        
                        return (string.Join(" - ", notaParts), false);
                    }
                    else
                    {
                        return ($"Disponibili {minimoDopoRipresa:N0} {unitaMisura} dal {dataUltimaRipresa.Value:dd/MM/yyyy}", false);
                    }
                }
                else
                {
                    return ("Disponibilità da verificare", true);
                }
            }

            // CASO 3: Mai negativo
            if (!vaInNegativo)
            {
                // Calcola la somma totale degli ordini clienti (movimenti negativi)
                // calcolando i delta tra progressivi consecutivi
                decimal sommaOrdiniClienti = 0;
                for (int i = 1; i < progressivi.Count; i++)
                {
                    var delta = progressivi[i].Progressivo - progressivi[i - 1].Progressivo;
                    if (delta < 0) // È un ordine cliente (sottrae)
                    {
                        sommaOrdiniClienti += Math.Abs(delta);
                    }
                }

                if (!produzioni.Any())
                {
                    // Nessuna produzione: mostra il minimo nel periodo
                    var minimo = progressivi.Min(p => p.Progressivo);
                    return ($"Disponibili {minimo:N0} {unitaMisura} dal {oggi:dd/MM/yyyy}", false);
                }
                else
                {
                    // Ci sono produzioni: mostra i livelli
                    var notaParts = new List<string>();
                    
                    // Prendi l'esistenza iniziale (primo progressivo, sempre l'Esistenza)
                    var esistenzaIniziale = progressivi.First().Progressivo;
                    
                    // Calcola il disponibile SENZA considerare le produzioni
                    // = esistenza - somma di TUTTI gli ordini clienti
                    var disponibileSenzaProduzioni = esistenzaIniziale - sommaOrdiniClienti;
                    
                    // Mostra la prima nota SOLO se l'esistenza copre TUTTI gli ordini clienti
                    // (anche quelli dopo le produzioni). Il principio è: non sono sicuro che
                    // le produzioni andranno in porto, quindi verifico prima se con l'esistente
                    // posso comunque soddisfare tutti gli ordini cliente.
                    if (disponibileSenzaProduzioni > 0)
                    {
                        notaParts.Add($"Disponibili {disponibileSenzaProduzioni:N0} {unitaMisura} dal {oggi:dd/MM/yyyy}");
                    }
                    
                    // Per ogni produzione, calcola il minimo da quella data in poi
                    // Queste note mostrano cosa succede SE le produzioni vengono completate
                    foreach (var dataProd in produzioni)
                    {
                        var minimoDopoQuestaProd = progressivi
                            .Where(p => p.Data >= dataProd)
                            .Min(p => p.Progressivo);
                        notaParts.Add($"Disponibili {minimoDopoQuestaProd:N0} {unitaMisura} dal {dataProd:dd/MM/yyyy}");
                    }
                    
                    return (string.Join(" - ", notaParts), false);
                }
            }

            return ("", false);
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
                                       on new { riga.AnnoOrdine, riga.SerieOrdine, riga.NumeroOrdine, riga.TipoOrdine } 
                                       equals new { testata.AnnoOrdine, testata.SerieOrdine, testata.NumeroOrdine, testata.TipoOrdine }
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
        /// Recupera il dettaglio degli ordini clienti che compongono l'impegnato (fino alla data di riferimento)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetDettaglioImpegnatoAttuale(string codiceArticolo, DateTime? dataRiferimento)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(codiceArticolo))
                {
                    return BadRequest(new { error = "Codice articolo obbligatorio" });
                }

                // Se non fornita, calcola la data di riferimento come oggi + GiorniImpegno
                var dataRif = dataRiferimento ?? DateTime.Today.AddDays(await GetGiorniImpegno());

                _logger.LogInformation("Recupero dettaglio impegnato per articolo {CodiceArticolo} fino alla data {DataRiferimento}", 
                    codiceArticolo, dataRif);

                // Query ordini clienti impegnati fino alla data di riferimento (TipoOrdine = 'R')
                // Filtra per ordini con DataConsegna <= dataRiferimento e quantità da evadere > 0
                // Usa Distinct per evitare duplicati da OrdiniTestate
                var ordini = await (from riga in _context.OrdiniRighe
                                   join testata in _context.OrdiniTestate
                                       on new { riga.AnnoOrdine, riga.SerieOrdine, riga.NumeroOrdine, riga.TipoOrdine } 
                                       equals new { testata.AnnoOrdine, testata.SerieOrdine, testata.NumeroOrdine, testata.TipoOrdine }
                                   join cliente in _context.AnagraficaClienti
                                       on testata.CodiceCliente equals cliente.CodiceCliente into clienteGroup
                                   from cliente in clienteGroup.DefaultIfEmpty()  // LEFT JOIN per non perdere ordini senza cliente
                                   where riga.CodiceArticolo == codiceArticolo
                                       && riga.TipoOrdine == "R"
                                       && riga.DataConsegna <= dataRif  // Impegnato: fino alla data di riferimento
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

                // Calcola totale impegnato usando la PercentualeInclusione
                var totaleImpegnatoAttuale = ordini.Sum(o => o.ContributoImpegnato);

                _logger.LogInformation("Trovati {NumeroOrdini} ordini clienti per un totale impegnato di {Totale}", 
                    ordini.Count, totaleImpegnatoAttuale);

                // Recupera descrizione e unità di misura articolo
                var articolo = await _context.AnagraficaArticoli
                    .FirstOrDefaultAsync(a => a.CodiceArticolo == codiceArticolo);

                var response = new DettaglioImpegnatoAttualeResponse
                {
                    CodiceArticolo = codiceArticolo,
                    DescrizioneArticolo = articolo?.Descrizione ?? "",
                    UnitaMisura = articolo?.UnitaMisura ?? "pz",
                    DataRiferimento = dataRif,
                    Ordini = ordini,
                    TotaleImpegnatoAttuale = totaleImpegnatoAttuale
                };

                return Json(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante il recupero del dettaglio impegnato per {CodiceArticolo}", codiceArticolo);
                return StatusCode(500, new { error = "Errore durante il recupero dei dati" });
            }
        }

        /// <summary>
        /// Recupera il dettaglio dei movimenti che compongono la nota previsione
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetDettaglioMovimentiNota(string codiceArticolo, decimal esistenza, DateTime dataRiferimento)
        {
            try
            {
                var oggi = DateTime.Today;
                var movimentiResult = new List<MovimentoNotaViewModel>();

                // 1. Aggiungi esistenza come primo movimento
                decimal progressivo = esistenza;
                movimentiResult.Add(new MovimentoNotaViewModel
                {
                    Data = oggi,
                    Tipo = "Esistenza",
                    Descrizione = "Giacenza iniziale a magazzino",
                    Quantita = esistenza,
                    Progressivo = progressivo
                });

                // 2. Carica ordini cliente (TipoOrdine = 'R') aperti fino alla data di riferimento
                // Include anche ordini arretrati (DataConsegna < oggi) per mostrare la storia completa
                var ordiniCliente = await _context.OrdiniRighe
                    .Where(r => r.TipoOrdine == "R")
                    .Where(r => r.CodiceArticolo == codiceArticolo)
                    .Where(r => r.DataConsegna <= dataRiferimento)
                    .Where(r => r.Quantita > r.QuantitaEvasa)
                    .Join(_context.OrdiniTestate,
                        r => new { r.TipoOrdine, r.AnnoOrdine, r.SerieOrdine, r.NumeroOrdine },
                        t => new { t.TipoOrdine, t.AnnoOrdine, t.SerieOrdine, t.NumeroOrdine },
                        (r, t) => new
                        {
                            r.DataConsegna,
                            Residuo = (r.Quantita - r.QuantitaEvasa) * r.PercentualeInclusione / 100m,
                            r.AnnoOrdine,
                            r.NumeroOrdine,
                            t.CodiceCliente,
                            Cliente = _context.AnagraficaClienti
                                .Where(c => c.CodiceCliente == t.CodiceCliente)
                                .Select(c => c.RagioneSociale)
                                .FirstOrDefault()
                        })
                    .OrderBy(o => o.DataConsegna)
                    .ToListAsync();

                foreach (var ordine in ordiniCliente)
                {
                    var quantita = -ordine.Residuo; // negativo
                    progressivo += quantita;
                    movimentiResult.Add(new MovimentoNotaViewModel
                    {
                        Data = ordine.DataConsegna,
                        Tipo = "Ordine Cliente",
                        Descrizione = $"Ord. {ordine.AnnoOrdine}/{ordine.NumeroOrdine} - {ordine.Cliente ?? ordine.CodiceCliente.ToString()}",
                        Quantita = quantita,
                        Progressivo = progressivo
                    });
                }

                // 3. Carica tempi asciugatura
                var tempiAsciugatura = await _context.TempiAsciugatura
                    .ToDictionaryAsync(t => t.IdMese, t => t.GiorniAsciugatura);

                // 4. Carica ordini produzione (stati: Emesso=1, In Produzione=2)
                var ordiniProduzione = await _context.ListaOP
                    .Where(op => op.CodiceArticolo == codiceArticolo)
                    .Where(op => op.IdStato == 1 || op.IdStato == 2)
                    .Where(op => op.DataFinePrevista != null)
                    .Select(op => new
                    {
                        op.Quantita,
                        op.QuantitaProdotta,
                        op.DataFinePrevista,
                        op.AnnoOrdine,
                        op.NumeroOrdine,
                        Mese = op.DataFinePrevista!.Value.Month
                    })
                    .ToListAsync();

                var produzioniConData = new List<(DateTime DataDisponibilita, decimal Quantita, string Descrizione, int GiorniAsc)>();
                foreach (var prod in ordiniProduzione)
                {
                    if (prod.DataFinePrevista.HasValue)
                    {
                        var giorniAsciugatura = tempiAsciugatura.GetValueOrDefault(prod.Mese, 0);
                        var dataDisponibilita = prod.DataFinePrevista.Value.AddDays(giorniAsciugatura);

                        if (dataDisponibilita >= oggi && dataDisponibilita <= dataRiferimento)
                        {
                            var quantitaRimanente = prod.Quantita - prod.QuantitaProdotta;
                            if (quantitaRimanente > 0)
                            {
                                produzioniConData.Add((
                                    dataDisponibilita,
                                    quantitaRimanente,
                                    $"OP {prod.AnnoOrdine}/{prod.NumeroOrdine} (Fine: {prod.DataFinePrevista.Value:dd/MM} + {giorniAsciugatura}gg asc.)",
                                    giorniAsciugatura
                                ));
                            }
                        }
                    }
                }

                // 5. Ora riordino tutti i movimenti per data e ricalcolo il progressivo
                var tuttiMovimenti = new List<(DateTime Data, string Tipo, string Descrizione, decimal Quantita, int? GiorniAsc, int Ordine)>();
                
                // Esistenza (ordine 0)
                tuttiMovimenti.Add((oggi, "Esistenza", "Giacenza iniziale a magazzino", esistenza, null, 0));
                
                // Ordini cliente (ordine 2)
                foreach (var ordine in ordiniCliente)
                {
                    tuttiMovimenti.Add((
                        ordine.DataConsegna,
                        "Ordine Cliente",
                        $"Ord. {ordine.AnnoOrdine}/{ordine.NumeroOrdine} - {ordine.Cliente ?? ordine.CodiceCliente.ToString()}",
                        -ordine.Residuo,
                        null,
                        2
                    ));
                }
                
                // Ordini produzione (ordine 1 - prima degli ordini cliente a parità di data)
                foreach (var prod in produzioniConData)
                {
                    tuttiMovimenti.Add((
                        prod.DataDisponibilita,
                        "Ordine Produzione",
                        prod.Descrizione,
                        prod.Quantita,
                        prod.GiorniAsc,
                        1
                    ));
                }

                // Ordina: esistenza sempre PRIMA come punto di partenza,
                // poi tutti gli altri per data (produzioni prima degli ordini cliente a parità di data)
                tuttiMovimenti = tuttiMovimenti
                    .OrderBy(m => m.Tipo == "Esistenza" ? 0 : 1) // Esistenza sempre prima
                    .ThenBy(m => m.Data)
                    .ThenBy(m => m.Ordine) // Produzioni (1) prima di Ordini Cliente (2)
                    .ToList();

                // Ricalcola progressivo
                movimentiResult.Clear();
                progressivo = 0;
                foreach (var mov in tuttiMovimenti)
                {
                    if (mov.Tipo == "Esistenza")
                        progressivo = mov.Quantita;
                    else
                        progressivo += mov.Quantita;

                    movimentiResult.Add(new MovimentoNotaViewModel
                    {
                        Data = mov.Data,
                        Tipo = mov.Tipo,
                        Descrizione = mov.Descrizione,
                        Quantita = mov.Quantita,
                        Progressivo = progressivo,
                        GiorniAsciugatura = mov.GiorniAsc
                    });
                }

                // Recupera descrizione e unità di misura articolo
                var articolo = await _context.AnagraficaArticoli
                    .FirstOrDefaultAsync(a => a.CodiceArticolo == codiceArticolo);

                // Genera la nota (riutilizzo la logica esistente)
                var notaResult = await CalcolaNotaPrevisione(codiceArticolo, esistenza, articolo?.UnitaMisura ?? "pz", dataRiferimento);

                var response = new DettaglioMovimentiNotaResponse
                {
                    CodiceArticolo = codiceArticolo,
                    DescrizioneArticolo = articolo?.Descrizione ?? "",
                    UnitaMisura = articolo?.UnitaMisura ?? "pz",
                    DataInizio = oggi,
                    DataFine = dataRiferimento,
                    NotaGenerata = notaResult.Nota,
                    Movimenti = movimentiResult
                };

                return Json(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante il recupero del dettaglio movimenti nota per {CodiceArticolo}", codiceArticolo);
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
                // Query base: Ordini clienti (TipoOrdine = 'R') con LEFT JOIN a DestinazioniDiverse
                var query = from testata in _context.OrdiniTestate
                            join cliente in _context.AnagraficaClienti
                                on testata.CodiceCliente equals cliente.CodiceCliente
                            join agente in _context.TabellaAgenti
                                on cliente.CodiceAgente equals agente.CodiceAgente into agenteGroup
                            from agente in agenteGroup.DefaultIfEmpty()
                            join destinazione in _context.DestinazioniDiverse
                                on new { CodiceConto = testata.CodiceCliente, CodiceDestinazione = (int?)testata.CodiceDestinazione }
                                equals new { destinazione.CodiceConto, CodiceDestinazione = (int?)destinazione.CodiceDestinazione } into destinazioneGroup
                            from destinazione in destinazioneGroup.DefaultIfEmpty()
                            where testata.TipoOrdine == "R"
                            select new
                            {
                                Testata = testata,
                                Cliente = cliente,
                                Agente = agente,
                                Destinazione = destinazione
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

                // NON applicare i filtri Provincia e Comune qui - verranno applicati dopo
                // perché devono considerare la destinazione diversa
                
                var testate = await query.ToListAsync();
                
                _logger.LogInformation("Caricati {Count} ordini prima del filtro geografico", testate.Count);
                
                // Applica filtri geografici DOPO aver caricato i dati, considerando destinazione diversa
                if (!string.IsNullOrWhiteSpace(model.Provincia) || !string.IsNullOrWhiteSpace(model.Comune))
                {
                    // Normalizza i filtri: Trim + case-insensitive
                    var filtroComune = model.Comune?.Trim() ?? "";
                    var filtroProvincia = model.Provincia?.Trim() ?? "";
                    
                    testate = testate.Where(item =>
                    {
                        var hasDestDiversa = item.Testata.CodiceDestinazione.HasValue && item.Destinazione != null;
                        var provinciaEffettiva = (hasDestDiversa && item.Destinazione != null ? item.Destinazione.Provincia : item.Cliente.Provincia)?.Trim();
                        var comuneEffettivo = (hasDestDiversa && item.Destinazione != null ? item.Destinazione.Localita : item.Cliente.Citta)?.Trim();
                        
                        // Confronto case-insensitive con Trim
                        bool matchProvincia = string.IsNullOrWhiteSpace(filtroProvincia) || 
                            (provinciaEffettiva != null && provinciaEffettiva.Equals(filtroProvincia, StringComparison.OrdinalIgnoreCase));
                        
                        bool matchComune = string.IsNullOrWhiteSpace(filtroComune) || 
                            (comuneEffettivo != null && comuneEffettivo.IndexOf(filtroComune, StringComparison.OrdinalIgnoreCase) >= 0);
                        
                        return matchProvincia && matchComune;
                    }).ToList();
                    
                    _logger.LogInformation("Dopo filtro geografico: {Count} ordini", testate.Count);
                }

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

                    // Verifica se c'è una destinazione diversa
                    var hasDestinazioneDiversa = item.Testata.CodiceDestinazione.HasValue && item.Destinazione != null;

                    // Ricava regione dalla provincia corretta (cliente o destinazione)
                    var provinciaEffettiva = hasDestinazioneDiversa && item.Destinazione != null ? item.Destinazione.Provincia : item.Cliente.Provincia;
                    var regione = RegioniHelper.GetRegione(provinciaEffettiva);

                    // Filtro per regione (se specificato) - considera destinazione diversa
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

                        // Dati cliente (SOVRASCRITTI se c'è destinazione diversa)
                        RagioneSociale = item.Cliente.RagioneSociale,
                        DescrizioneUlteriore = item.Cliente.DescrizioneUlteriore,
                        Indirizzo = hasDestinazioneDiversa && item.Destinazione != null ? item.Destinazione.Indirizzo : item.Cliente.Indirizzo,
                        Cap = hasDestinazioneDiversa && item.Destinazione != null ? item.Destinazione.Cap : item.Cliente.Cap,
                        Citta = hasDestinazioneDiversa && item.Destinazione != null ? item.Destinazione.Localita : item.Cliente.Citta,
                        Provincia = hasDestinazioneDiversa && item.Destinazione != null ? item.Destinazione.Provincia : item.Cliente.Provincia,
                        Regione = regione,
                        CodiceFiscale = item.Cliente.CodiceFiscale,
                        PartitaIva = item.Cliente.PartitaIva,
                        Telefono = item.Cliente.Telefono,

                        // Flag e dati destinazione diversa
                        HasDestinazioneDiversa = hasDestinazioneDiversa,
                        DescrizioneDestinazione = item.Destinazione?.DescrizioneDestinazione,
                        IndirizzoDestinazione = item.Destinazione?.Indirizzo,
                        CapDestinazione = item.Destinazione?.Cap,
                        LocalitaDestinazione = item.Destinazione?.Localita,
                        ProvinciaDestinazione = item.Destinazione?.Provincia,

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

                // Pre-carica le email inviate per gli ordini trovati
                var emailInviateConsegne = await _context.InvioEmail
                    .Where(e => e.TipoOrdine == "R")
                    .ToListAsync();

                var emailPerOrdineConsegne = emailInviateConsegne
                    .GroupBy(e => new { e.AnnoOrdine, e.SerieOrdine, e.NumeroOrdine })
                    .ToDictionary(
                        g => g.Key,
                        g => g.Max(e => e.DataInvio)
                    );

                // Imposta il flag email su ogni ordine
                foreach (var ordine in ordiniViewModel)
                {
                    var chiaveEmail = new { AnnoOrdine = ordine.AnnoOrdine, SerieOrdine = ordine.SerieOrdine, NumeroOrdine = ordine.NumeroOrdine };
                    if (emailPerOrdineConsegne.ContainsKey(chiaveEmail))
                    {
                        ordine.HasEmailInviata = true;
                        ordine.DataEmailInviata = emailPerOrdineConsegne[chiaveEmail];
                    }
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

        // ===== AVVISO MERCE PRONTA =====

        /// <summary>
        /// GET: Mostra la lista ordini con righe disponibili per invio email avviso merce pronta.
        /// Range: da oggi a oggi + GiorniEmail (da TabellaOpzioni).
        /// Mostra solo righe dove Esistenza >= QuantitaRimanente.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> AvvisoMercePronta()
        {
            ViewBag.UseFluidContainer = true;

            var model = new AvvisoMerceProntaViewModel();

            try
            {
                // 1. Leggi GiorniEmail da TabellaOpzioni
                var giorniEmail = await _emailService.GetOpzioneIntAsync("GiorniEmail", 7);
                var clienteEsclusoStr = await _emailService.GetOpzioneAsync("ClienteEscluso");
                var clienteEscluso = int.TryParse(clienteEsclusoStr, out var ce) ? ce : 9060650;

                var oggi = DateTime.Today;
                var dataInizio = oggi.AddDays(-1); // Parte da ieri
                var dataFine = oggi.AddDays(giorniEmail);

                model.DataDa = dataInizio;
                model.DataA = dataFine;
                model.GiorniEmail = giorniEmail;

                _logger.LogInformation("AvvisoMercePronta: range {Da} - {A}, GiorniEmail={Giorni}", 
                    dataInizio.ToString("dd/MM/yyyy"), dataFine.ToString("dd/MM/yyyy"), giorniEmail);

                // 2. Query: ordini clienti (TipoOrdine='R'), escluso cliente FAVARO1, esclusi ordini prenotati
                var ordiniConRighe = await (
                    from testata in _context.OrdiniTestate
                    join cliente in _context.AnagraficaClienti
                        on testata.CodiceCliente equals cliente.CodiceCliente
                    join agente in _context.TabellaAgenti
                        on cliente.CodiceAgente equals agente.CodiceAgente into agenteGroup
                    from agente in agenteGroup.DefaultIfEmpty()
                    where testata.TipoOrdine == "R"
                       && testata.CodiceCliente != clienteEscluso
                       && testata.Prenotato != "S"
                    select new
                    {
                        Testata = testata,
                        Cliente = cliente,
                        Agente = agente
                    }
                ).ToListAsync();

                // 3. Per ogni ordine, recupera le righe con DataConsegna nel range e quantità da evadere > 0
                var tuttiOrdini = new List<OrdineEmailViewModel>();
                
                // Pre-carica tutte le righe ordine nel range date (da ieri a oggi + GiorniEmail)
                var tutteLeRighe = await _context.OrdiniRighe
                    .Where(r => r.TipoOrdine == "R" 
                             && r.DataConsegna >= dataInizio 
                             && r.DataConsegna <= dataFine
                             && (r.Quantita - r.QuantitaEvasa) > 0)
                    .ToListAsync();

                // Raggruppa le righe per chiave ordine
                var righePerOrdine = tutteLeRighe.GroupBy(r => new { r.TipoOrdine, r.AnnoOrdine, r.SerieOrdine, r.NumeroOrdine })
                    .ToDictionary(g => g.Key, g => g.ToList());

                // Pre-carica Esistenza dal Magazzino 1 per tutti gli articoli coinvolti
                var codiciArticoli = tutteLeRighe.Select(r => r.CodiceArticolo).Distinct().ToList();
                var esistenzaDict = await _context.ProgressiviArticoli
                    .Where(p => p.CodiceMagazzino == 1 && codiciArticoli.Contains(p.CodiceArticolo))
                    .ToDictionaryAsync(p => p.CodiceArticolo, p => p.Esistenza);

                // Pre-carica gli invii email già effettuati
                var inviiEsistenti = await _context.InvioEmail
                    .Where(e => e.TipoOrdine == "R")
                    .ToListAsync();
                var inviiDict = inviiEsistenti
                    .GroupBy(e => new { e.TipoOrdine, e.AnnoOrdine, e.SerieOrdine, e.NumeroOrdine, e.RigaOrdine })
                    .ToDictionary(g => g.Key, g => g.First());

                // 4. Calcola totale ordinato per ogni articolo (per il controllo conflitto)
                var totaleOrdinatoPerArticolo = tutteLeRighe
                    .GroupBy(r => r.CodiceArticolo)
                    .ToDictionary(
                        g => g.Key,
                        g => g.Sum(r => r.Quantita - r.QuantitaEvasa)
                    );

                // 5. Costruisci i ViewModel per ogni ordine
                foreach (var item in ordiniConRighe)
                {
                    var chiaveOrdine = new { 
                        TipoOrdine = item.Testata.TipoOrdine, 
                        AnnoOrdine = item.Testata.AnnoOrdine, 
                        SerieOrdine = item.Testata.SerieOrdine, 
                        NumeroOrdine = item.Testata.NumeroOrdine 
                    };

                    if (!righePerOrdine.ContainsKey(chiaveOrdine))
                        continue;

                    var righeOrdine = righePerOrdine[chiaveOrdine];
                    var righeViewModel = new List<RigaEmailViewModel>();

                    foreach (var riga in righeOrdine)
                    {
                        var quantitaRimanente = riga.Quantita - riga.QuantitaEvasa;
                        var esistenza = esistenzaDict.ContainsKey(riga.CodiceArticolo) 
                            ? esistenzaDict[riga.CodiceArticolo] : 0m;

                        // Solo righe dove Esistenza >= QuantitaRimanente
                        if (esistenza < quantitaRimanente)
                            continue;

                        // Verifica email già inviata
                        var chiaveInvio = new { 
                            TipoOrdine = riga.TipoOrdine, 
                            AnnoOrdine = riga.AnnoOrdine, 
                            SerieOrdine = riga.SerieOrdine, 
                            NumeroOrdine = riga.NumeroOrdine, 
                            RigaOrdine = riga.RigaOrdine 
                        };
                        var invioEsistente = inviiDict.ContainsKey(chiaveInvio) ? inviiDict[chiaveInvio] : null;

                        // Verifica conflitto: disponibilità insufficiente per tutti gli ordini
                        var totaleOrdinato = totaleOrdinatoPerArticolo.ContainsKey(riga.CodiceArticolo) 
                            ? totaleOrdinatoPerArticolo[riga.CodiceArticolo] : 0m;
                        var isConflitto = totaleOrdinato > esistenza;

                        var rigaVm = new RigaEmailViewModel
                        {
                            TipoOrdine = riga.TipoOrdine,
                            AnnoOrdine = riga.AnnoOrdine,
                            SerieOrdine = riga.SerieOrdine,
                            NumeroOrdine = riga.NumeroOrdine,
                            RigaOrdine = riga.RigaOrdine,
                            CodiceArticolo = riga.CodiceArticolo,
                            DescrizioneArticolo = riga.DescrizioneArticolo,
                            UnitaMisura = riga.UnitaMisura,
                            Quantita = riga.Quantita,
                            QuantitaEvasa = riga.QuantitaEvasa,
                            DataConsegna = riga.DataConsegna,
                            Esistenza = esistenza,
                            IsConflitto = isConflitto,
                            MessaggioConflitto = isConflitto 
                                ? $"Attenzione: l'articolo {riga.CodiceArticolo} è richiesto per {totaleOrdinato:N2} {riga.UnitaMisura} ma la disponibilità è solo {esistenza:N2} {riga.UnitaMisura}" 
                                : null,
                            EmailGiaInviata = invioEsistente != null,
                            DataUltimoInvio = invioEsistente?.DataInvio
                        };

                        righeViewModel.Add(rigaVm);
                    }

                    // Se non ci sono righe disponibili per questo ordine, salta
                    if (!righeViewModel.Any())
                        continue;

                    var ordineVm = new OrdineEmailViewModel
                    {
                        TipoOrdine = item.Testata.TipoOrdine,
                        AnnoOrdine = item.Testata.AnnoOrdine,
                        SerieOrdine = item.Testata.SerieOrdine,
                        NumeroOrdine = item.Testata.NumeroOrdine,
                        CodiceCliente = item.Testata.CodiceCliente,
                        RagioneSociale = item.Cliente.RagioneSociale,
                        EmailCliente = item.Cliente.Email,
                        CodiceAgente = item.Cliente.CodiceAgente,
                        NomeAgente = item.Agente?.DescrizioneAgente,
                        EmailAgente = item.Agente?.Email,
                        DataOrdine = item.Testata.DataOrdine,
                        RiferimentoOrdine = item.Testata.RiferimentoOrdine,
                        Porto = item.Testata.Porto,
                        DescrizionePorto = item.Testata.DescrizionePorto,
                        PortoCssClass = item.Testata.PortoCssClass,
                        Righe = righeViewModel
                    };

                    // Verifica se almeno una riga dell'ordine ha già ricevuto un'email
                    var righeConEmail = righeViewModel.Where(r => r.EmailGiaInviata).ToList();
                    if (righeConEmail.Any())
                    {
                        ordineVm.HasEmailInviata = true;
                        ordineVm.DataEmailInviata = righeConEmail.Max(r => r.DataUltimoInvio);
                    }

                    tuttiOrdini.Add(ordineVm);
                }

                // Ordina per data consegna minima delle righe
                model.Ordini = tuttiOrdini
                    .OrderBy(o => o.Righe.Min(r => r.DataConsegna))
                    .ThenBy(o => o.RagioneSociale)
                    .ToList();

                _logger.LogInformation("AvvisoMercePronta: trovati {Ordini} ordini con {Righe} righe disponibili",
                    model.Ordini.Count, model.TotaleRigheDisponibili);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante il caricamento AvvisoMercePronta");
                model.Messaggio = "Si è verificato un errore durante il caricamento dei dati. Riprova più tardi.";
                model.TipoMessaggio = "danger";
            }

            return View(model);
        }

        /// <summary>
        /// POST: Invia le email per le righe selezionate dall'operatore.
        /// Raggruppa le righe per ordine e invia un'email per ogni ordine.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> InviaEmailMercePronta(List<string> righeSelezionate)
        {
            ViewBag.UseFluidContainer = true;

            if (righeSelezionate == null || !righeSelezionate.Any())
            {
                TempData["Messaggio"] = "Nessuna riga selezionata per l'invio email.";
                TempData["TipoMessaggio"] = "warning";
                return RedirectToAction("AvvisoMercePronta");
            }

            try
            {
                var giorniScadenza = await _emailService.GetOpzioneIntAsync("GiorniScadenzaMerce", 21);

                // Parse delle chiavi selezionate (formato: TipoOrdine|Anno|Serie|Numero|Riga)
                var righeParsate = new List<(string TipoOrdine, short AnnoOrdine, string SerieOrdine, int NumeroOrdine, int RigaOrdine)>();
                foreach (var chiave in righeSelezionate)
                {
                    var parti = chiave.Split('|');
                    if (parti.Length == 5 &&
                        short.TryParse(parti[1], out var anno) &&
                        int.TryParse(parti[3], out var numero) &&
                        int.TryParse(parti[4], out var rigaNum))
                    {
                        righeParsate.Add((parti[0], anno, parti[2], numero, rigaNum));
                    }
                }

                if (!righeParsate.Any())
                {
                    TempData["Messaggio"] = "Formato righe non valido.";
                    TempData["TipoMessaggio"] = "danger";
                    return RedirectToAction("AvvisoMercePronta");
                }

                // Raggruppa per ordine
                var righePerOrdine = righeParsate.GroupBy(r => new { r.TipoOrdine, r.AnnoOrdine, r.SerieOrdine, r.NumeroOrdine });

                int emailInviate = 0;
                int emailFallite = 0;
                int righeGiàInviate = 0;

                foreach (var gruppoOrdine in righePerOrdine)
                {
                    // Recupera dati testata e cliente
                    var testata = await _context.OrdiniTestate
                        .FirstOrDefaultAsync(t => t.TipoOrdine == gruppoOrdine.Key.TipoOrdine &&
                                                  t.AnnoOrdine == gruppoOrdine.Key.AnnoOrdine &&
                                                  t.SerieOrdine == gruppoOrdine.Key.SerieOrdine &&
                                                  t.NumeroOrdine == gruppoOrdine.Key.NumeroOrdine);

                    if (testata == null) continue;

                    var cliente = await _context.AnagraficaClienti
                        .FirstOrDefaultAsync(c => c.CodiceCliente == testata.CodiceCliente);
                    var agente = cliente != null 
                        ? await _context.TabellaAgenti.FirstOrDefaultAsync(a => a.CodiceAgente == cliente.CodiceAgente) 
                        : null;

                    // Costruisci il ViewModel dell'ordine per il template email
                    var ordineVm = new OrdineEmailViewModel
                    {
                        TipoOrdine = testata.TipoOrdine,
                        AnnoOrdine = testata.AnnoOrdine,
                        SerieOrdine = testata.SerieOrdine,
                        NumeroOrdine = testata.NumeroOrdine,
                        CodiceCliente = testata.CodiceCliente,
                        RagioneSociale = cliente?.RagioneSociale ?? "N/D",
                        EmailCliente = cliente?.Email,
                        CodiceAgente = cliente?.CodiceAgente ?? 0,
                        NomeAgente = agente?.DescrizioneAgente,
                        EmailAgente = agente?.Email,
                        DataOrdine = testata.DataOrdine,
                        RiferimentoOrdine = testata.RiferimentoOrdine,
                    };

                    // Recupera le righe ordine dal DB
                    var righeEmail = new List<RigaEmailViewModel>();
                    foreach (var rigaParsata in gruppoOrdine)
                    {
                        // Verifica se email già inviata
                        var giaInviata = await _emailService.EmailGiaInviataAsync(
                            rigaParsata.TipoOrdine, rigaParsata.AnnoOrdine, rigaParsata.SerieOrdine,
                            rigaParsata.NumeroOrdine, rigaParsata.RigaOrdine);

                        if (giaInviata)
                        {
                            righeGiàInviate++;
                            continue;
                        }

                        var rigaDb = await _context.OrdiniRighe
                            .FirstOrDefaultAsync(r => r.TipoOrdine == rigaParsata.TipoOrdine &&
                                                       r.AnnoOrdine == rigaParsata.AnnoOrdine &&
                                                       r.SerieOrdine == rigaParsata.SerieOrdine &&
                                                       r.NumeroOrdine == rigaParsata.NumeroOrdine &&
                                                       r.RigaOrdine == rigaParsata.RigaOrdine);

                        if (rigaDb != null)
                        {
                            righeEmail.Add(new RigaEmailViewModel
                            {
                                TipoOrdine = rigaDb.TipoOrdine,
                                AnnoOrdine = rigaDb.AnnoOrdine,
                                SerieOrdine = rigaDb.SerieOrdine,
                                NumeroOrdine = rigaDb.NumeroOrdine,
                                RigaOrdine = rigaDb.RigaOrdine,
                                CodiceArticolo = rigaDb.CodiceArticolo,
                                DescrizioneArticolo = rigaDb.DescrizioneArticolo,
                                UnitaMisura = rigaDb.UnitaMisura,
                                Quantita = rigaDb.Quantita,
                                QuantitaEvasa = rigaDb.QuantitaEvasa,
                                DataConsegna = rigaDb.DataConsegna
                            });
                        }
                    }

                    if (!righeEmail.Any())
                        continue;

                    // Genera e invia email
                    var oggetto = "Avviso disponibilità merce pronta";
                    var corpo = _emailService.GeneraCorpoEmail(ordineVm, righeEmail, giorniScadenza);
                    var esito = await _emailService.InviaEmailAsync(oggetto, corpo);

                    if (esito)
                    {
                        emailInviate++;
                        // Registra l'invio per ogni riga
                        foreach (var riga in righeEmail)
                        {
                            await _emailService.RegistraInvioAsync(riga);
                        }
                    }
                    else
                    {
                        emailFallite++;
                    }
                }

                // Messaggio di riepilogo
                var messaggi = new List<string>();
                if (emailInviate > 0)
                    messaggi.Add($"{emailInviate} email inviate con successo");
                if (emailFallite > 0)
                    messaggi.Add($"{emailFallite} email fallite");
                if (righeGiàInviate > 0)
                    messaggi.Add($"{righeGiàInviate} righe già inviate in precedenza (saltate)");

                TempData["Messaggio"] = string.Join(". ", messaggi) + ".";
                TempData["TipoMessaggio"] = emailFallite > 0 ? "warning" : "success";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante l'invio email merce pronta");
                TempData["Messaggio"] = "Si è verificato un errore durante l'invio delle email.";
                TempData["TipoMessaggio"] = "danger";
            }

            return RedirectToAction("AvvisoMercePronta");
        }

        // ===== LISTA EMAIL INVIATE =====

        /// <summary>
        /// GET: Mostra la lista di tutte le email inviate con i dettagli ordine, cliente e agente.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> ListaEmailInviate()
        {
            ViewBag.UseFluidContainer = true;

            var model = new ListaEmailInviateViewModel();

            try
            {
                // Recupera tutti i record di invio email, ordinati per data invio decrescente
                var invii = await _context.InvioEmail
                    .OrderByDescending(e => e.DataInvio)
                    .ToListAsync();

                if (!invii.Any())
                {
                    return View(model);
                }

                // Pre-carica le righe ordine corrispondenti
                var chiavi = invii.Select(e => new { e.TipoOrdine, e.AnnoOrdine, e.SerieOrdine, e.NumeroOrdine, e.RigaOrdine }).ToList();
                
                // Recupera tutte le righe ordine coinvolte
                var righeOrdine = await _context.OrdiniRighe
                    .Where(r => r.TipoOrdine == "R")
                    .ToListAsync();

                var righeDict = righeOrdine
                    .GroupBy(r => new { r.TipoOrdine, r.AnnoOrdine, r.SerieOrdine, r.NumeroOrdine, r.RigaOrdine })
                    .ToDictionary(g => g.Key, g => g.First());

                // Pre-carica le testate ordine coinvolte
                var testate = await _context.OrdiniTestate
                    .Where(t => t.TipoOrdine == "R")
                    .ToListAsync();

                var testateDict = testate
                    .GroupBy(t => new { t.TipoOrdine, t.AnnoOrdine, t.SerieOrdine, t.NumeroOrdine })
                    .ToDictionary(g => g.Key, g => g.First());

                // Pre-carica clienti e agenti
                var clienti = await _context.AnagraficaClienti.ToListAsync();
                var clientiDict = clienti.ToDictionary(c => c.CodiceCliente, c => c);

                var agenti = await _context.TabellaAgenti.ToListAsync();
                var agentiDict = agenti.ToDictionary(a => a.CodiceAgente, a => a);

                // Costruisci i ViewModel
                foreach (var invio in invii)
                {
                    var dettaglio = new EmailInviataDettaglioViewModel
                    {
                        ID = invio.ID,
                        TipoOrdine = invio.TipoOrdine,
                        AnnoOrdine = invio.AnnoOrdine,
                        SerieOrdine = invio.SerieOrdine,
                        NumeroOrdine = invio.NumeroOrdine,
                        RigaOrdine = invio.RigaOrdine,
                        DataInvio = invio.DataInvio,
                        Contabilizzato = invio.Contabilizzato
                    };

                    // Cerca la riga ordine
                    var chiaveRiga = new { invio.TipoOrdine, invio.AnnoOrdine, invio.SerieOrdine, invio.NumeroOrdine, invio.RigaOrdine };
                    if (righeDict.ContainsKey(chiaveRiga))
                    {
                        var riga = righeDict[chiaveRiga];
                        dettaglio.CodiceArticolo = riga.CodiceArticolo;
                        dettaglio.DescrizioneArticolo = riga.DescrizioneArticolo;
                        dettaglio.UnitaMisura = riga.UnitaMisura;
                        dettaglio.Quantita = riga.Quantita;
                        dettaglio.QuantitaEvasa = riga.QuantitaEvasa;
                        dettaglio.DataConsegna = riga.DataConsegna;
                    }

                    // Cerca la testata ordine per avere il codice cliente
                    var chiaveTestata = new { invio.TipoOrdine, invio.AnnoOrdine, invio.SerieOrdine, invio.NumeroOrdine };
                    if (testateDict.ContainsKey(chiaveTestata))
                    {
                        var testata = testateDict[chiaveTestata];
                        dettaglio.CodiceCliente = testata.CodiceCliente;

                        // Cerca il cliente
                        if (clientiDict.ContainsKey(testata.CodiceCliente))
                        {
                            var cliente = clientiDict[testata.CodiceCliente];
                            dettaglio.RagioneSociale = cliente.RagioneSociale;

                            // Cerca l'agente
                            if (agentiDict.ContainsKey(cliente.CodiceAgente))
                            {
                                dettaglio.NomeAgente = agentiDict[cliente.CodiceAgente].DescrizioneAgente;
                            }
                        }
                    }

                    model.EmailInviate.Add(dettaglio);
                }

                _logger.LogInformation("ListaEmailInviate: trovati {Totale} record di email inviate", model.TotaleEmail);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante il caricamento della Lista Email Inviate");
            }

            return View(model);
        }
    }
}

