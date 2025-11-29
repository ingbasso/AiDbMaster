using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AiDbMaster.Data;
using AiDbMaster.Models;

namespace AiDbMaster.Controllers
{
    [Authorize]
    public class SchedulatoreOPController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<SchedulatoreOPController> _logger;

        public SchedulatoreOPController(
            ApplicationDbContext context,
            ILogger<SchedulatoreOPController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Pagina principale Schedulatore Ordini di Produzione
        /// </summary>
        public IActionResult Index()
        {
            ViewBag.Title = "Schedulatore Ordini di Produzione";
            ViewBag.UseFluidContainer = true;
            return View();
        }

        /// <summary>
        /// API: Ottiene i centri di lavoro per le risorse del timeline
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetCentriLavoro()
        {
            try
            {
                _logger.LogInformation("Caricamento centri di lavoro per SchedulatoreOP");

                var centri = await _context.CentriLavoro
                    .Where(c => c.Attivo)
                    .OrderBy(c => c.DescrizioneCentro)
                    .Select(c => new
                    {
                        Id = c.CodiceCentro,
                        Name = c.DescrizioneCentro,
                        Capacity = c.CapacitaOraria ?? 1
                    })
                    .ToListAsync();

                _logger.LogInformation($"Caricati {centri.Count} centri di lavoro");
                return Json(centri);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore nel recupero dei centri di lavoro");
                return StatusCode(500, new { error = true, message = ex.Message });
            }
        }

        /// <summary>
        /// API: Ottiene gli ordini di produzione per il calendario
        /// CARICA TUTTI GLI ORDINI CON STATO != 4 (esclusi i Chiusi)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetOrdiniProduzione()
        {
            try
            {
                _logger.LogInformation("Caricamento TUTTI gli ordini con IdStato != 4 (esclusi Chiusi)");

                // Prima conta TUTTI gli ordini
                var totaleOrdini = await _context.ListaOP.CountAsync();
                _logger.LogInformation($"Totale ordini in ListaOP: {totaleOrdini}");

                // Carica TUTTI gli ordini con IdStato != 4 (escludi solo i Chiusi)
                var ordini = await _context.ListaOP
                    .Include(o => o.Stato)
                    .Include(o => o.CentroLavoro)
                    .Include(o => o.Lavorazione)
                    .Include(o => o.Operatore)
                    .Where(o => o.IdStato != 4)  // Escludi solo ordini Chiusi
                    .OrderBy(o => o.DataInizioOP) // Ordina per data
                    .ToListAsync();
                
                _logger.LogInformation($"Ordini caricati (IdStato != 4): {ordini.Count}");
                
                if (ordini.Count == 0)
                {
                    _logger.LogWarning("⚠️ Nessun ordine trovato con IdStato != 4");
                }
                else
                {
                    // Log statistiche
                    var dataMin = ordini.Min(o => o.DataInizioOP);
                    var dataMax = ordini.Max(o => o.DataInizioOP);
                    var ordiniPerStato = ordini.GroupBy(o => o.IdStato)
                        .Select(g => new { IdStato = g.Key, Count = g.Count() })
                        .ToList();
                    
                    _logger.LogInformation($"Range date ordini: {dataMin:yyyy-MM-dd} → {dataMax:yyyy-MM-dd}");
                    _logger.LogInformation($"Distribuzione stati: {string.Join(", ", ordiniPerStato.Select(s => $"Stato {s.IdStato}: {s.Count}"))}");
                }

                // Carica TUTTI i fermi per calcolare gli EndTime estesi
                var tuttiFermi = await _context.CalendarioFermiCentriLavoro.ToListAsync();
                
                // ===== RICALCOLO DataFinePrevista per ordini non modificati =====
                _logger.LogInformation("🔄 Ricalcolo DataFinePrevista per ordini con Modificato = 0...");
                
                // Conta ordini da ricalcolare
                var ordiniDaRicalcolare = ordini.Where(o => o.Modificato == 0).ToList();
                _logger.LogInformation($"Ordini con Modificato = 0: {ordiniDaRicalcolare.Count}");
                
                bool hasChanges = false;
                int contatoreAggiornati = 0;
                
                foreach (var ordine in ordiniDaRicalcolare)
                {
                    // Calcola durata TOTALE (lavoro + setup) in secondi
                    var durataLavoroSecondi = (decimal)(ordine.Quantita * (decimal)ordine.TempoCiclo);
                    var durataSetupSecondi = (decimal)((ordine.TempoSetup ?? 0) * 60); // Converti minuti in secondi
                    var durataTotaleSecondi = durataLavoroSecondi + durataSetupSecondi;
                    
                    // Filtra fermi per questo centro lavoro
                    var fermiCentro = tuttiFermi
                        .Where(f => f.CodiceCentro == ordine.CodiceCentro)
                        .OrderBy(f => f.DataInizioFermo)
                        .ToList();
                    
                    // Calcola DataFinePrevista SALTANDO i fermi
                    var nuovaDataFinePrevista = durataTotaleSecondi > 0
                        ? CalcolaEndTimeConFermi(ordine.DataInizioOP, durataTotaleSecondi, fermiCentro)
                        : ordine.DataInizioOP.AddHours(1); // Fallback se durata = 0
                    
                    // Aggiorna solo se diversa (per evitare UPDATE inutili)
                    if (!ordine.DataFinePrevista.HasValue || 
                        Math.Abs((ordine.DataFinePrevista.Value - nuovaDataFinePrevista).TotalSeconds) > 1)
                    {
                        ordine.DataFinePrevista = nuovaDataFinePrevista;
                        hasChanges = true;
                        contatoreAggiornati++;
                    }
                }
                
                // Salva tutte le modifiche nel database
                if (hasChanges)
                {
                    await _context.SaveChangesAsync();
                    _logger.LogInformation($"✅ DataFinePrevista aggiornata per {contatoreAggiornati} ordini");
                }
                else
                {
                    _logger.LogInformation("ℹ️ Nessun ordine da aggiornare (DataFinePrevista già corretta)");
                }

                // Mappa gli ordini in eventi per Syncfusion
                var eventi = ordini.Select(o =>
                {
                    // Calcola EndTime SEMPRE dalla Quantità, poi applica la logica sequenziale con fermi
                    
                    // 1. Calcola durata lavoro da Quantità × TempoCiclo + TempoSetup
                    var durataLavoroSecondi = (decimal)(o.Quantita * (decimal)o.TempoCiclo);
                    var tempoSetupSecondi = (decimal)((o.TempoSetup ?? 0) * 60); // Converti minuti in secondi
                    var durataTotaleSecondi = durataLavoroSecondi + tempoSetupSecondi;
                    
                    // 2. Applica la logica sequenziale che SALTA i fermi
                    var fermiCentro = tuttiFermi.Where(f => f.CodiceCentro == o.CodiceCentro).OrderBy(f => f.DataInizioFermo).ToList();
                    var endTime = durataTotaleSecondi > 0
                        ? CalcolaEndTimeConFermi(o.DataInizioOP, durataTotaleSecondi, fermiCentro)
                        : o.DataInizioOP.AddHours(1);
                    
                    // Calcola percentuale completamento
                    var percentuale = o.Quantita > 0 ? Math.Round((o.QuantitaProdotta / o.Quantita) * 100, 1) : 0;
                    
                    return new
                    {
                        // Dati per Syncfusion Schedule
                        Id = o.IdListaOP,
                        Subject = $"Ord. {o.AnnoOrdine}-{o.NumeroOrdine} Qta: {Math.Floor(o.Quantita)} ({percentuale}%)",
                        // Specifica Local per evitare conversioni timezone
                        StartTime = DateTime.SpecifyKind(o.DataInizioOP, DateTimeKind.Local),
                        EndTime = DateTime.SpecifyKind(endTime, DateTimeKind.Local),
                        RoomId = o.CodiceCentro, // Per associare alla risorsa (centro di lavoro)
                        // Colore basato su IdStato
                        CategoryColor = GetColorByStato(o.IdStato),
                        IsAllDay = false,
                    
                    // TUTTI i campi di ListaOP per il popup
                    TipoOrdine = o.TipoOrdine,
                    AnnoOrdine = o.AnnoOrdine,
                    SerieOrdine = o.SerieOrdine,
                    NumeroOrdine = o.NumeroOrdine,
                    RigaOrdine = o.RigaOrdine,
                    DescrOrdine = o.DescrOrdine,
                    CodiceArticolo = o.CodiceArticolo,
                    DescrizioneArticolo = o.DescrizioneArticolo,
                    UnitaMisura = o.UnitaMisura,
                    Quantita = o.Quantita,
                    QuantitaProdotta = o.QuantitaProdotta,
                    DataInizioOP = DateTime.SpecifyKind(o.DataInizioOP, DateTimeKind.Local),
                    TempoCiclo = o.TempoCiclo,
                    TempoCicloTavola = o.TempoCicloTavola,
                    DataInizioSetup = o.DataInizioSetup.HasValue 
                        ? DateTime.SpecifyKind(o.DataInizioSetup.Value, DateTimeKind.Local)
                        : (DateTime?)null,
                    TempoSetup = o.TempoSetup,
                    IdStato = o.IdStato,
                    StatoDescrizione = o.Stato?.DescrizioneStato ?? "",
                    CodiceCentro = o.CodiceCentro,
                    CentroLavoroDescrizione = o.CentroLavoro?.DescrizioneCentro ?? "",
                    CodiceLavorazione = o.CodiceLavorazione,
                    DescrizioneLavorazione = o.Lavorazione?.DescrizioneLavorazione ?? "",
                    Note = o.Note,
                    DataFineOP = o.DataFineOP.HasValue 
                        ? DateTime.SpecifyKind(o.DataFineOP.Value, DateTimeKind.Local)
                        : (DateTime?)null,
                    DataFinePrevista = o.DataFinePrevista.HasValue 
                        ? DateTime.SpecifyKind(o.DataFinePrevista.Value, DateTimeKind.Local)
                        : (DateTime?)null,
                    Priorita = o.Priorita,
                    IdOperatore = o.IdOperatore,
                    CodiceOperatore = o.Operatore?.CodiceOperatore ?? "",
                    NomeOperatore = o.Operatore != null ? $"{o.Operatore.Cognome} {o.Operatore.Nome}".Trim() : "",
                    CostoOrario = o.CostoOrario,
                    TempoEffettivo = o.TempoEffettivo,
                    Modificato = o.Modificato,
                    PercentualeCompletamento = o.Quantita > 0 ? Math.Round((o.QuantitaProdotta / o.Quantita) * 100, 2) : 0
                    };
                }).ToList();

                return Json(eventi);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore nel recupero degli ordini di produzione");
                return StatusCode(500, new { error = true, message = ex.Message });
            }
        }

        /// <summary>
        /// API: Ottiene gli stati OP per la legenda
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetStatiOP()
        {
            try
            {
                var stati = await _context.StatiOP
                    .OrderBy(s => s.IdStato)
                    .Select(s => new
                    {
                        Id = s.IdStato,
                        Descrizione = s.DescrizioneStato
                    })
                    .ToListAsync();


                return Ok(stati);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore nel caricamento stati OP");
                return StatusCode(500, new { error = true, message = ex.Message });
            }
        }

        /// <summary>
        /// API: Ottiene TUTTI i fermi dei centri di lavoro per colorare il calendario
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetFermiCentriLavoro()
        {
            try
            {
                _logger.LogInformation("Caricamento TUTTI i fermi centri di lavoro");

                // Carica TUTTI i fermi, senza filtro date
                var fermiDb = await _context.CalendarioFermiCentriLavoro
                    .OrderBy(f => f.DataInizioFermo)
                    .ToListAsync();
                
                _logger.LogInformation($"Fermi caricati: {fermiDb.Count}");

                // Mappa i fermi specificando il Kind delle date per evitare problemi di timezone
                var fermi = fermiDb.Select(f => new
                {
                    Id = f.Id,
                    CodiceCentro = f.CodiceCentro,
                    // Specifica Local per evitare conversioni timezone indesiderate
                    DataInizio = DateTime.SpecifyKind(f.DataInizioFermo, DateTimeKind.Local),
                    DataFine = f.DataFineFermo.HasValue 
                        ? DateTime.SpecifyKind(f.DataFineFermo.Value, DateTimeKind.Local)
                        : (DateTime?)null,
                    Descrizione = f.Motivo
                }).ToList();


                return Ok(fermi);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore nel caricamento fermi centri di lavoro");
                return StatusCode(500, new { error = true, message = ex.Message });
            }
        }

        /// <summary>
        /// API: Aggiorna ordine dopo resize nel calendario
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> UpdateOrdineResize([FromBody] ResizeOrdineRequest request)
        {
            try
            {
                var ordine = await _context.ListaOP.FindAsync(request.Id);
                if (ordine == null)
                {
                    return NotFound(new { success = false, message = "Ordine non trovato" });
                }

                // ===== VALIDAZIONE 1: Campo Modificato (solo 0 o 1 consentiti) =====
                if (ordine.Modificato != 0 && ordine.Modificato != 1)
                {
                    _logger.LogWarning($"Tentativo di resize ordine {ordine.IdListaOP} con Modificato={ordine.Modificato} (non consentito)");
                    return BadRequest(new { 
                        success = false, 
                        message = "❌ Modifica non permessa per concorrenza con Business Cube. Riprova." 
                    });
                }

                // ===== VALIDAZIONE 2: Stati non modificabili =====
                if (ordine.IdStato == 3)
                {
                    return BadRequest(new { success = false, message = "❌ Impossibile modificare: ordine in Stato 3" });
                }
                
                if (ordine.IdStato == 4)
                {
                    return BadRequest(new { success = false, message = "❌ Impossibile modificare: ordine in Stato 4" });
                }

                // ===== VALIDAZIONE 3: IdStato=2 non può modificare DataInizio =====
                if (ordine.IdStato == 2)
                {
                    // Normalizza entrambe le date a UTC per confronto corretto
                    var dataInizioOPUtc = ordine.DataInizioOP.Kind == DateTimeKind.Unspecified 
                        ? DateTime.SpecifyKind(ordine.DataInizioOP, DateTimeKind.Local).ToUniversalTime()
                        : ordine.DataInizioOP.ToUniversalTime();
                    
                    var startTimeUtc = request.StartTime.ToUniversalTime();
                    
                    var differenzaSecondi = Math.Abs((startTimeUtc - dataInizioOPUtc).TotalSeconds);
                    
                    if (differenzaSecondi > 2)
                    {
                        _logger.LogWarning($"Tentativo modifica DataInizioOP con IdStato=2 bloccato (differenza: {differenzaSecondi:F1}s)");
                        return BadRequest(new { 
                            success = false, 
                            message = "❌ Ordine OP in Produzione, non posso alterare Data Inizio OP" 
                        });
                    }
                }

                // ===== VALIDAZIONE 4: DataInizioOP non può cadere in un fermo =====
                var fermiCentro = await _context.CalendarioFermiCentriLavoro
                    .Where(f => f.CodiceCentro == ordine.CodiceCentro)
                    .ToListAsync();

                var fermoInConflitto = fermiCentro.FirstOrDefault(fermo =>
                {
                    var dataInizio = fermo.DataInizioFermo;
                    var dataFine = fermo.DataFineFermo ?? DateTime.MaxValue;
                    return request.StartTime >= dataInizio && request.StartTime < dataFine;
                });

                if (fermoInConflitto != null)
                {
                    _logger.LogWarning($"Tentativo di impostare DataInizioOP durante fermo (IdFermo={fermoInConflitto.Id})");
                    return BadRequest(new
                    {
                        success = false,
                        message = $"❌ Impossibile iniziare ordine durante un fermo del centro lavoro (Fermo: {fermoInConflitto.Motivo})"
                    });
                }

                // ===== CALCOLO E AGGIORNAMENTO =====
                
                // Normalizza date a UTC per confronto corretto (con tolleranza di 2 secondi)
                var requestStartTimeUtc = request.StartTime.ToUniversalTime();
                var requestEndTimeUtc = request.EndTime.ToUniversalTime();
                
                var ordineDataInizioOPUtc = ordine.DataInizioOP.Kind == DateTimeKind.Unspecified 
                    ? DateTime.SpecifyKind(ordine.DataInizioOP, DateTimeKind.Local).ToUniversalTime()
                    : ordine.DataInizioOP.ToUniversalTime();
                
                bool dataInizioModificata = Math.Abs((requestStartTimeUtc - ordineDataInizioOPUtc).TotalSeconds) > 2;
                
                bool dataFineModificata;
                if (ordine.DataFinePrevista.HasValue)
                {
                    var ordineDataFinePrevistaUtc = ordine.DataFinePrevista.Value.Kind == DateTimeKind.Unspecified 
                        ? DateTime.SpecifyKind(ordine.DataFinePrevista.Value, DateTimeKind.Local).ToUniversalTime()
                        : ordine.DataFinePrevista.Value.ToUniversalTime();
                    dataFineModificata = Math.Abs((requestEndTimeUtc - ordineDataFinePrevistaUtc).TotalSeconds) > 2;
                }
                else
                {
                    dataFineModificata = true; // Se DataFinePrevista è null, consideriamo sempre modificata
                }

                // Salva valori originali per confronto
                var dataInizioOriginale = ordine.DataInizioOP;
                var dataFineOriginale = ordine.DataFinePrevista;
                var quantitaOriginale = ordine.Quantita;

                // ===== CALCOLO QUANTITÀ E DATA FINE CON FERMI =====
                
                // 1. Calcola la durata VISIVA ATTUALE (con fermi estesi)
                var durataLavoroAttualeSecondi = (decimal)(ordine.Quantita * (decimal)ordine.TempoCiclo);
                var tempoSetupSecondi = (decimal)((ordine.TempoSetup ?? 0) * 60);
                var durataTotaleAttualeSecondi = durataLavoroAttualeSecondi + tempoSetupSecondi;
                var endTimeVisualeAttuale = durataTotaleAttualeSecondi > 0
                    ? CalcolaEndTimeConFermi(ordine.DataInizioOP, durataTotaleAttualeSecondi, fermiCentro)
                    : ordine.DataInizioOP.AddHours(1);
                
                var durataVisualeAttualeSecondi = (decimal)(endTimeVisualeAttuale - ordine.DataInizioOP).TotalSeconds;
                
                // 2. Calcola la durata VISIVA RICHIESTA dal resize
                var durataVisualeRichiestaSecondi = (decimal)(request.EndTime - request.StartTime).TotalSeconds;
                
                // 3. Calcola il DELTA (aumento o diminuzione)
                var deltaDurataSecondi = durataVisualeRichiestaSecondi - durataVisualeAttualeSecondi;
                
                // 4. Applica il delta alla durata di LAVORO (non visiva)
                var durataLavoroRichiestaSecondi = durataLavoroAttualeSecondi + deltaDurataSecondi;
                
                // 5. Calcola DataFinePrevista usando la logica sequenziale che SALTA i fermi
                var endTimeCalcolato = CalcolaEndTimeConFermi(request.StartTime, durataLavoroRichiestaSecondi, fermiCentro);
                
                // ===== AGGIORNA DATE ORDINE =====
                
                if (dataInizioModificata && ordine.IdStato == 1)
                {
                    // IdStato = 1: Modifica DataInizio
                    ordine.DataInizioOP = request.StartTime;
                }

                if (dataFineModificata && (ordine.IdStato == 1 || ordine.IdStato == 2))
                {
                    // IdStato = 1 o 2: Modifica DataFine
                    // USA la data finale calcolata iterativamente (che ha già i fermi estesi)
                    ordine.DataFinePrevista = endTimeCalcolato;
                }

                // ===== RICALCOLA QUANTITÀ =====
                // La quantità si basa sulla durata di LAVORO richiesta (già calcolata con il delta)
                
                if (ordine.TempoCiclo > 0 && durataLavoroRichiestaSecondi > 0)
                {
                    ordine.Quantita = Math.Round(durataLavoroRichiestaSecondi / (decimal)ordine.TempoCiclo, 3);
                }
                else
                {
                    _logger.LogWarning($"Ordine {ordine.IdListaOP}: Impossibile ricalcolare Quantità (TempoCiclo={ordine.TempoCiclo}, durataLavoroRichiesta={durataLavoroRichiestaSecondi})");
                }

                // Imposta Modificato = 1
                ordine.Modificato = 1;

                await _context.SaveChangesAsync();

                // Calcola EndTime visivo con fermi per Syncfusion
                var durataLavoroSecondiVisivo = (decimal)(ordine.Quantita * (decimal)ordine.TempoCiclo);
                var tempoSetupSecondiVisivo = (decimal)((ordine.TempoSetup ?? 0) * 60);
                var durataTotaleSecondiVisivo = durataLavoroSecondiVisivo + tempoSetupSecondiVisivo;
                var endTimeVisivo = durataTotaleSecondiVisivo > 0
                    ? CalcolaEndTimeConFermi(ordine.DataInizioOP, durataTotaleSecondiVisivo, fermiCentro)
                    : ordine.DataInizioOP.AddHours(1);

                return Ok(new { 
                    success = true, 
                    message = "✅ Ordine aggiornato con successo",
                    nuovaQuantita = ordine.Quantita,
                    dataInizioOP = DateTime.SpecifyKind(ordine.DataInizioOP, DateTimeKind.Local),
                    dataFinePrevista = ordine.DataFinePrevista.HasValue 
                        ? DateTime.SpecifyKind(ordine.DataFinePrevista.Value, DateTimeKind.Local)
                        : (DateTime?)null,
                    endTimeVisivo = DateTime.SpecifyKind(endTimeVisivo, DateTimeKind.Local)
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Errore nell'aggiornamento resize ordine {request.Id}");
                return StatusCode(500, new { success = false, message = "❌ Errore server: " + ex.Message });
            }
        }

        /// <summary>
        /// API: Aggiorna ordine dopo drag&drop nel calendario
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> UpdateOrdineDragDrop([FromBody] DragDropOrdineRequest request)
        {
            try
            {
                _logger.LogInformation($"📥 Richiesta drag&drop ricevuta - ID: {request.Id}, StartTime: {request.StartTime:yyyy-MM-dd HH:mm:ss}");
                
                var ordine = await _context.ListaOP.FindAsync(request.Id);
                if (ordine == null)
                {
                    return NotFound(new { success = false, message = "Ordine non trovato" });
                }
                
                // ===== VALIDAZIONE 1: Solo IdStato = 1 può fare drag&drop =====
                if (ordine.IdStato != 1)
                {
                    _logger.LogWarning($"Tentativo di drag&drop ordine {ordine.IdListaOP} con IdStato={ordine.IdStato} (solo 1 ammesso)");
                    return BadRequest(new { 
                        success = false, 
                        message = $"❌ Impossibile spostare: solo ordini con Stato 1 (Emesso) possono essere spostati. Stato corrente: {ordine.IdStato}" 
                    });
                }

                // ===== VALIDAZIONE 2: Campo Modificato (solo 0 o 1 consentiti) =====
                if (ordine.Modificato != 0 && ordine.Modificato != 1)
                {
                    _logger.LogWarning($"Tentativo di drag&drop ordine {ordine.IdListaOP} con Modificato={ordine.Modificato} (non consentito)");
                    return BadRequest(new { 
                        success = false, 
                        message = "❌ Modifica non permessa per concorrenza con Business Cube. Riprova." 
                    });
                }

                // ===== CARICA FERMI DEL CENTRO LAVORO =====
                var fermiCentro = await _context.CalendarioFermiCentriLavoro
                    .Where(f => f.CodiceCentro == ordine.CodiceCentro)
                    .ToListAsync();

                // ===== VALIDAZIONE 3: La nuova data di inizio NON deve cadere dentro un fermo =====
                foreach (var fermo in fermiCentro)
                {
                    if (request.StartTime >= fermo.DataInizioFermo && request.StartTime < (fermo.DataFineFermo ?? DateTime.MaxValue))
                    {
                        _logger.LogWarning($"Tentativo di spostare ordine {ordine.IdListaOP} con data inizio dentro fermo: {fermo.DataInizioFermo:yyyy-MM-dd HH:mm} - {fermo.DataFineFermo:yyyy-MM-dd HH:mm}");
                        return BadRequest(new { 
                            success = false, 
                            message = "❌ Impossibile spostare: la data di inizio cade in un periodo di fermo del centro lavoro." 
                        });
                    }
                }

                // ===== CALCOLO NUOVA DATA FINE CON FERMI =====
                
                // 1. Mantieni la quantità INVARIATA (è solo uno spostamento)
                var quantitaInvariata = ordine.Quantita;
                
                // 2. Calcola durata lavoro: Quantità × TempoCiclo + TempoSetup
                var durataLavorazioneSecondi = (decimal)(quantitaInvariata * (decimal)ordine.TempoCiclo);
                var tempoSetupSecondi = (decimal)((ordine.TempoSetup ?? 0) * 60);
                var durataTotaleSecondi = durataLavorazioneSecondi + tempoSetupSecondi;
                
                // 3. Calcola nuova DataFinePrevista usando CalcolaEndTimeConFermi (che salta i fermi)
                var nuovaDataFinePrevista = CalcolaEndTimeConFermi(request.StartTime, durataTotaleSecondi, fermiCentro);
                
                _logger.LogInformation($"Drag&Drop Ordine {ordine.IdListaOP}: " +
                    $"DataInizio {ordine.DataInizioOP:yyyy-MM-dd HH:mm} → {request.StartTime:yyyy-MM-dd HH:mm}, " +
                    $"DataFine {ordine.DataFinePrevista:yyyy-MM-dd HH:mm} → {nuovaDataFinePrevista:yyyy-MM-dd HH:mm}, " +
                    $"Quantità invariata: {quantitaInvariata}");

                // ===== AGGIORNAMENTO =====
                
                ordine.DataInizioOP = request.StartTime;
                ordine.DataFinePrevista = nuovaDataFinePrevista;
                ordine.Modificato = 1;

                await _context.SaveChangesAsync();

                // Calcola EndTime visivo con fermi per Syncfusion (stesso valore di DataFinePrevista)
                var endTimeVisivo = nuovaDataFinePrevista;

                return Ok(new { 
                    success = true, 
                    message = "✅ Ordine spostato con successo",
                    dataInizioOP = DateTime.SpecifyKind(ordine.DataInizioOP, DateTimeKind.Local),
                    dataFinePrevista = ordine.DataFinePrevista.HasValue 
                        ? DateTime.SpecifyKind(ordine.DataFinePrevista.Value, DateTimeKind.Local)
                        : (DateTime?)null,
                    endTimeVisivo = DateTime.SpecifyKind(endTimeVisivo, DateTimeKind.Local)
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Errore nell'aggiornamento drag&drop ordine {request.Id}");
                return StatusCode(500, new { success = false, message = "❌ Errore server: " + ex.Message });
            }
        }

        /// <summary>
        /// API: Aggiorna ordine manualmente dal popup (Data Inizio OP e Quantità)
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> UpdateOrdineManuale([FromBody] UpdateOrdineManualRequest request)
        {
            try
            {
                _logger.LogInformation($"📥 Richiesta modifica manuale - ID: {request.Id}, DataInizioOP: {request.DataInizioOP:yyyy-MM-dd HH:mm:ss}, Quantità: {request.Quantita}");
                
                var ordine = await _context.ListaOP.FindAsync(request.Id);
                if (ordine == null)
                {
                    return NotFound(new { success = false, message = "Ordine non trovato" });
                }
                

                // ===== VALIDAZIONE 1: Solo IdStato = 1 o 2 possono essere modificati manualmente =====
                if (ordine.IdStato != 1 && ordine.IdStato != 2)
                {
                    _logger.LogWarning($"Tentativo di modifica manuale ordine {ordine.IdListaOP} con IdStato={ordine.IdStato}");
                    return BadRequest(new { 
                        success = false, 
                        message = $"❌ Impossibile modificare: solo ordini con Stato 1 o 2 possono essere modificati. Stato corrente: {ordine.IdStato}" 
                    });
                }

                // ===== VALIDAZIONE 2: Campo Modificato (solo 0 o 1 consentiti) =====
                if (ordine.Modificato != 0 && ordine.Modificato != 1)
                {
                    _logger.LogWarning($"Tentativo di modifica manuale ordine {ordine.IdListaOP} con Modificato={ordine.Modificato} (non consentito)");
                    return BadRequest(new { 
                        success = false, 
                        message = "❌ Modifica non permessa per concorrenza con Business Cube. Riprova." 
                    });
                }

                // ===== VALIDAZIONE 3: Se IdStato=2, DataInizioOP non può essere modificata =====
                if (ordine.IdStato == 2)
                {
                    var dataInizioOPUtc = ordine.DataInizioOP.Kind == DateTimeKind.Unspecified 
                        ? DateTime.SpecifyKind(ordine.DataInizioOP, DateTimeKind.Local).ToUniversalTime()
                        : ordine.DataInizioOP.ToUniversalTime();
                    
                    var requestDataInizioUtc = request.DataInizioOP.ToUniversalTime();
                    var differenzaSecondi = Math.Abs((requestDataInizioUtc - dataInizioOPUtc).TotalSeconds);
                    
                    if (differenzaSecondi > 2)
                    {
                        _logger.LogWarning($"❌ Tentativo di modificare DataInizioOP con IdStato=2");
                        return BadRequest(new { 
                            success = false, 
                            message = "❌ Ordine in Produzione: impossibile modificare Data Inizio OP" 
                        });
                    }
                }

                // ===== VALIDAZIONE 4: Quantità deve essere > 0 =====
                if (request.Quantita <= 0)
                {
                    return BadRequest(new { 
                        success = false, 
                        message = "❌ La quantità deve essere maggiore di zero" 
                    });
                }

                // ===== AGGIORNAMENTO =====
                
                var vecchiaDataInizio = ordine.DataInizioOP;
                var vecchiaQuantita = ordine.Quantita;
                var vecchiaDataFinePrevista = ordine.DataFinePrevista;
                
                // Aggiorna DataInizioOP (solo se IdStato = 1)
                if (ordine.IdStato == 1)
                {
                    ordine.DataInizioOP = request.DataInizioOP;
                    _logger.LogInformation($"Ordine {ordine.IdListaOP}: DataInizioOP modificata da {vecchiaDataInizio:yyyy-MM-dd HH:mm} a {ordine.DataInizioOP:yyyy-MM-dd HH:mm}");
                }
                
                // Aggiorna Quantità
                ordine.Quantita = request.Quantita;
                _logger.LogInformation($"Ordine {ordine.IdListaOP}: Quantità modificata da {vecchiaQuantita} a {ordine.Quantita}");
                
                // ===== CALCOLO DATA FINE CON FERMI =====
                
                // 1. Carica fermi del centro lavoro
                var fermiCentro = await _context.CalendarioFermiCentriLavoro
                    .Where(f => f.CodiceCentro == ordine.CodiceCentro)
                    .ToListAsync();
                
                // 2. Calcola durata lavoro: Quantità × TempoCiclo + TempoSetup
                var durataLavorazioneSecondi = (decimal)(ordine.Quantita * (decimal)ordine.TempoCiclo);
                var tempoSetupSecondi = (decimal)((ordine.TempoSetup ?? 0) * 60); // Converti minuti in secondi
                var durataTotaleSecondi = durataLavorazioneSecondi + tempoSetupSecondi;
                
                // 3. Usa CalcolaEndTimeConFermi per saltare i fermi
                ordine.DataFinePrevista = CalcolaEndTimeConFermi(ordine.DataInizioOP, durataTotaleSecondi, fermiCentro);
                
                _logger.LogInformation($"Ordine {ordine.IdListaOP}: DataFinePrevista calcolata = {ordine.DataFinePrevista:yyyy-MM-dd HH:mm}");
                _logger.LogInformation($"  - Durata lavorazione: {durataLavorazioneSecondi}s (Quantità {ordine.Quantita} × TempoCiclo {ordine.TempoCiclo}s)");
                _logger.LogInformation($"  - Tempo setup: {tempoSetupSecondi}s ({ordine.TempoSetup ?? 0} minuti)");
                _logger.LogInformation($"  - Durata totale: {durataTotaleSecondi}s");
                
                // Imposta Modificato = 1
                ordine.Modificato = 1;

                await _context.SaveChangesAsync();


                return Ok(new { 
                    success = true, 
                    message = "✅ Ordine aggiornato con successo",
                    dataInizioOP = DateTime.SpecifyKind(ordine.DataInizioOP, DateTimeKind.Local),
                    quantita = ordine.Quantita,
                    dataFinePrevista = ordine.DataFinePrevista.HasValue 
                        ? DateTime.SpecifyKind(ordine.DataFinePrevista.Value, DateTimeKind.Local)
                        : (DateTime?)null
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Errore nell'aggiornamento manuale ordine {request.Id}");
                return StatusCode(500, new { success = false, message = "❌ Errore server: " + ex.Message });
            }
        }

        /// <summary>
        /// Ottiene il colore in base allo stato dell'ordine
        /// </summary>
        private static string GetColorByStato(int idStato)
        {
            return idStato switch
            {
                1 => "#FFA500", // Arancione
                2 => "#1E90FF", // Blu
                3 => "#9370DB", // Viola
                4 => "#32CD32", // Verde
                _ => "#808080"  // Grigio (default)
            };
        }

        /// <summary>
        /// Calcola l'EndTime di un ordine SALTANDO i fermi
        /// LOGICA SEQUENZIALE: Parto da startTime, aggiungo durataLavoroSecondi SALTANDO tutti i fermi
        /// </summary>
        private static DateTime CalcolaEndTimeConFermi(DateTime startTime, decimal durataLavoroSecondi, List<CalendarioFermiCentriLavoro> fermiCentro)
        {
            var currentTime = startTime;
            var secondiRimasti = durataLavoroSecondi;
            
            // Ordina i fermi per data inizio
            var fermiOrdinati = fermiCentro.OrderBy(f => f.DataInizioFermo).ToList();
            
            int iterazioni = 0;
            const int maxIterazioni = 100;
            
            foreach (var fermo in fermiOrdinati)
            {
                if (iterazioni++ > maxIterazioni || secondiRimasti <= 0)
                {
                    break;
                }
                
                var fermoInizio = fermo.DataInizioFermo;
                var fermoFine = fermo.DataFineFermo ?? currentTime.AddYears(1);
                
                // Calcola dove arriverebbe senza questo fermo
                var endTimeSenzaFermo = currentTime.AddSeconds((double)secondiRimasti);
                
                // Verifica se questo fermo interseca il percorso [currentTime, endTimeSenzaFermo]
                if (fermoInizio < endTimeSenzaFermo && fermoFine > currentTime)
                {
                    // C'è intersezione
                    
                    // Calcola tempo di lavoro PRIMA del fermo
                    if (fermoInizio > currentTime)
                    {
                        var tempoPreFermo = (decimal)(fermoInizio - currentTime).TotalSeconds;
                        if (tempoPreFermo <= secondiRimasti)
                        {
                            // Lavoro fino al fermo
                            currentTime = fermoInizio;
                            secondiRimasti -= tempoPreFermo;
                        }
                        else
                        {
                            // Il lavoro finisce prima del fermo
                            return currentTime.AddSeconds((double)secondiRimasti);
                        }
                    }
                    
                    // Se siamo nel fermo e abbiamo ancora secondi da lavorare, SALTO il fermo
                    if (secondiRimasti > 0 && currentTime >= fermoInizio && currentTime < fermoFine)
                    {
                        currentTime = fermoFine; // Salto alla fine del fermo
                    }
                }
            }
            
            // Aggiungi i secondi rimanenti
            return currentTime.AddSeconds((double)secondiRimasti);
        }
    }

    /// <summary>
    /// Modello per la richiesta di resize ordine
    /// </summary>
    public class ResizeOrdineRequest
    {
        public int Id { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
    }

    /// <summary>
    /// Modello per la richiesta di drag&drop ordine
    /// </summary>
    public class DragDropOrdineRequest
    {
        public int Id { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string RoomId { get; set; } = string.Empty;
    }

    /// <summary>
    /// Modello per la richiesta di modifica manuale ordine
    /// </summary>
    public class UpdateOrdineManualRequest
    {
        public int Id { get; set; }
        public DateTime DataInizioOP { get; set; }
        public decimal Quantita { get; set; }
    }
}

