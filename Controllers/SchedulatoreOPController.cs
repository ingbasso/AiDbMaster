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
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetOrdiniProduzione(
            DateTime? dataInizio = null,
            DateTime? dataFine = null)
        {
            try
            {
                // Range default: 2 mesi (1 mese prima e 1 mese dopo oggi)
                var start = dataInizio ?? DateTime.Today.AddMonths(-1);
                var end = dataFine ?? DateTime.Today.AddMonths(1);

                _logger.LogInformation($"Caricamento ordini produzione: {start:yyyy-MM-dd} - {end:yyyy-MM-dd}");

                // Prima conta TUTTI gli ordini
                var totaleOrdini = await _context.ListaOP.CountAsync();
                _logger.LogInformation($"Totale ordini in ListaOP: {totaleOrdini}");

                // Poi filtra per date
                var ordini = await _context.ListaOP
                    .Include(o => o.Stato)
                    .Include(o => o.CentroLavoro)
                    .Where(o => o.DataInizioOP >= start && o.DataInizioOP <= end)
                    .ToListAsync();
                
                _logger.LogInformation($"Ordini nel range {start:yyyy-MM-dd} - {end:yyyy-MM-dd}: {ordini.Count}");
                
                if (ordini.Count == 0)
                {
                    // Log delle date effettive degli ordini per debug
                    var dateOrdini = await _context.ListaOP
                        .OrderBy(o => o.DataInizioOP)
                        .Select(o => o.DataInizioOP)
                        .Take(5)
                        .ToListAsync();
                    
                    _logger.LogWarning($"Nessun ordine nel range. Prime 5 date in DB: {string.Join(", ", dateOrdini.Select(d => d.ToString("yyyy-MM-dd")))}");
                }

                // PRIMA: Aggiorna DataFinePrevista nel database per tutti gli ordini
                bool hasChanges = false;
                foreach (var ordine in ordini)
                {
                    // Calcola EndTime
                    var durataSecondi = (double)(ordine.Quantita * (decimal)ordine.TempoCiclo);
                    var endTimeCalcolato = durataSecondi > 0 
                        ? ordine.DataInizioOP.AddSeconds(durataSecondi)
                        : ordine.DataInizioOP.AddHours(1);
                    
                    // Aggiorna SEMPRE DataFinePrevista con l'EndTime calcolato
                    if (ordine.DataFinePrevista != endTimeCalcolato)
                    {
                        ordine.DataFinePrevista = endTimeCalcolato;
                        hasChanges = true;
                    }
                }
                
                // Salva tutte le modifiche in un colpo solo
                if (hasChanges)
                {
                    await _context.SaveChangesAsync();
                    _logger.LogInformation($"✅ Aggiornati DataFinePrevista per {ordini.Count} ordini");
                }
                
                // POI: Mappa gli ordini in eventi per Syncfusion
                var eventi = ordini.Select(o =>
                {
                    // Usa DataFinePrevista (che ora contiene l'EndTime calcolato)
                    var endTime = o.DataFinePrevista ?? o.DataInizioOP.AddHours(1);
                    
                    return new
                    {
                        // Dati per Syncfusion Schedule
                        Id = o.IdListaOP,
                        Subject = $"Ord. {o.AnnoOrdine}-{o.NumeroOrdine} Qta: {Math.Floor(o.Quantita)}",
                        StartTime = o.DataInizioOP,
                        EndTime = endTime,
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
                    DataInizioOP = o.DataInizioOP,
                    TempoCiclo = o.TempoCiclo,
                    DataInizioSetup = o.DataInizioSetup,
                    TempoSetup = o.TempoSetup,
                    IdStato = o.IdStato,
                    StatoDescrizione = o.Stato?.DescrizioneStato ?? "",
                    CodiceCentro = o.CodiceCentro,
                    CentroLavoroDescrizione = o.CentroLavoro?.DescrizioneCentro ?? "",
                    CodiceLavorazione = o.CodiceLavorazione,
                    Note = o.Note,
                    DataFineOP = o.DataFineOP,
                    DataFinePrevista = o.DataFinePrevista,
                    Priorita = o.Priorita,
                    IdOperatore = o.IdOperatore,
                    CostoOrario = o.CostoOrario,
                    TempoEffettivo = o.TempoEffettivo,
                    Modificato = o.Modificato,
                    PercentualeCompletamento = o.Quantita > 0 ? Math.Round((o.QuantitaProdotta / o.Quantita) * 100, 2) : 0
                    };
                }).ToList();

                _logger.LogInformation($"✅ Restituiti {eventi.Count} ordini di produzione per il calendario");
                
                // Log dei primi 3 eventi per debug
                if (eventi.Count > 0)
                {
                    _logger.LogInformation($"Primi eventi: {string.Join(", ", eventi.Take(3).Select(e => $"{e.Subject} [{e.StartTime:yyyy-MM-dd HH:mm} - {e.EndTime:yyyy-MM-dd HH:mm}]"))}");
                }
                
                return Json(eventi);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore nel recupero degli ordini di produzione");
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
                _logger.LogInformation($"📥 Richiesta resize ricevuta - ID: {request.Id}, StartTime: {request.StartTime:yyyy-MM-dd HH:mm:ss}, EndTime: {request.EndTime:yyyy-MM-dd HH:mm:ss}");
                
                var ordine = await _context.ListaOP.FindAsync(request.Id);
                if (ordine == null)
                {
                    return NotFound(new { success = false, message = "Ordine non trovato" });
                }
                
                _logger.LogInformation($"📋 Ordine DB - ID: {ordine.IdListaOP}, IdStato: {ordine.IdStato}, DataInizioOP DB: {ordine.DataInizioOP:yyyy-MM-dd HH:mm:ss}");

                // ===== VALIDAZIONE 1: Campo Modificato =====
                if (ordine.Modificato == 7)
                {
                    _logger.LogWarning($"Tentativo di resize ordine {ordine.IdListaOP} con Modificato=7 (bloccato)");
                    return BadRequest(new { 
                        success = false, 
                        message = "❌ Impossibile modificare: l'ordine è BLOCCATO (Modificato=7)" 
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
                    
                    _logger.LogInformation($"🔍 CONTROLLO IdStato=2:");
                    _logger.LogInformation($"   DataInizioOP DB:  {ordine.DataInizioOP:yyyy-MM-dd HH:mm:ss} (Kind: {ordine.DataInizioOP.Kind}) → UTC: {dataInizioOPUtc:yyyy-MM-dd HH:mm:ss}");
                    _logger.LogInformation($"   StartTime Request: {request.StartTime:yyyy-MM-dd HH:mm:ss} (Kind: {request.StartTime.Kind}) → UTC: {startTimeUtc:yyyy-MM-dd HH:mm:ss}");
                    _logger.LogInformation($"   Differenza: {differenzaSecondi:F3} secondi");
                    
                    if (differenzaSecondi > 2)
                    {
                        _logger.LogWarning($"❌ BLOCCATO - Differenza {differenzaSecondi:F3}s > 2s");
                        return BadRequest(new { 
                            success = false, 
                            message = "❌ Ordine OP in Produzione, non posso alterare Data Inizio OP" 
                        });
                    }
                    else
                    {
                        _logger.LogInformation($"✅ OK - Differenza {differenzaSecondi:F3}s <= 2s");
                    }
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

                if (dataInizioModificata && ordine.IdStato == 1)
                {
                    // IdStato = 1: Modifica DataInizio
                    ordine.DataInizioOP = request.StartTime;
                    _logger.LogInformation($"Ordine {ordine.IdListaOP}: Modificata DataInizioOP a {request.StartTime:yyyy-MM-dd HH:mm}");
                }

                if (dataFineModificata && (ordine.IdStato == 1 || ordine.IdStato == 2))
                {
                    // IdStato = 1 o 2: Modifica DataFine
                    ordine.DataFinePrevista = request.EndTime;
                    _logger.LogInformation($"Ordine {ordine.IdListaOP}: Modificata DataFinePrevista a {request.EndTime:yyyy-MM-dd HH:mm}");
                }

                // Ricalcola Quantità
                if (ordine.TempoCiclo > 0 && ordine.DataFinePrevista.HasValue)
                {
                    var durataSecondi = (decimal)(ordine.DataFinePrevista.Value - ordine.DataInizioOP).TotalSeconds;
                    var vecchiaQuantita = ordine.Quantita;
                    ordine.Quantita = Math.Round(durataSecondi / (decimal)ordine.TempoCiclo, 3);
                    
                    _logger.LogInformation($"Ordine {ordine.IdListaOP}: Quantità aggiornata da {vecchiaQuantita} a {ordine.Quantita} " +
                                         $"(Durata: {durataSecondi}s, TempoCiclo: {ordine.TempoCiclo}s)");
                }
                else
                {
                    _logger.LogWarning($"Ordine {ordine.IdListaOP}: Impossibile ricalcolare Quantità (TempoCiclo={ordine.TempoCiclo}, DataFinePrevista={ordine.DataFinePrevista})");
                }

                // Imposta Modificato = 1
                ordine.Modificato = 1;

                await _context.SaveChangesAsync();

                _logger.LogInformation($"✅ Ordine {ordine.IdListaOP} aggiornato tramite resize");

                return Ok(new { 
                    success = true, 
                    message = "✅ Ordine aggiornato con successo",
                    nuovaQuantita = ordine.Quantita,
                    dataInizioOP = ordine.DataInizioOP,
                    dataFinePrevista = ordine.DataFinePrevista
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
                _logger.LogInformation($"📥 Richiesta drag&drop ricevuta - ID: {request.Id}, StartTime: {request.StartTime:yyyy-MM-dd HH:mm:ss}, EndTime: {request.EndTime:yyyy-MM-dd HH:mm:ss}, RoomId: {request.RoomId}");
                
                var ordine = await _context.ListaOP.FindAsync(request.Id);
                if (ordine == null)
                {
                    return NotFound(new { success = false, message = "Ordine non trovato" });
                }
                
                _logger.LogInformation($"📋 Ordine DB - ID: {ordine.IdListaOP}, IdStato: {ordine.IdStato}, CodiceCentro: {ordine.CodiceCentro}");

                // ===== VALIDAZIONE 1: Solo IdStato = 1 può fare drag&drop =====
                if (ordine.IdStato != 1)
                {
                    _logger.LogWarning($"Tentativo di drag&drop ordine {ordine.IdListaOP} con IdStato={ordine.IdStato} (solo 1 ammesso)");
                    return BadRequest(new { 
                        success = false, 
                        message = $"❌ Impossibile spostare: solo ordini con Stato 1 (Emesso) possono essere spostati. Stato corrente: {ordine.IdStato}" 
                    });
                }

                // ===== VALIDAZIONE 2: Non si può cambiare centro di lavoro =====
                if (request.RoomId != ordine.CodiceCentro)
                {
                    _logger.LogWarning($"Tentativo di cambiare centro di lavoro per ordine {ordine.IdListaOP}: {ordine.CodiceCentro} → {request.RoomId}");
                    return BadRequest(new { 
                        success = false, 
                        message = "❌ Impossibile cambiare Centro di Lavoro. Puoi solo spostare nel tempo." 
                    });
                }

                // ===== AGGIORNAMENTO =====
                
                var vecchiaDataInizio = ordine.DataInizioOP;
                var vecchiaDataFine = ordine.DataFinePrevista;
                
                // Aggiorna DataInizioOP
                ordine.DataInizioOP = request.StartTime;
                
                // Aggiorna DataFinePrevista
                ordine.DataFinePrevista = request.EndTime;
                
                // Imposta Modificato = 1
                ordine.Modificato = 1;

                await _context.SaveChangesAsync();

                _logger.LogInformation($"✅ Ordine {ordine.IdListaOP} spostato tramite drag&drop:");
                _logger.LogInformation($"   DataInizioOP: {vecchiaDataInizio:yyyy-MM-dd HH:mm} → {ordine.DataInizioOP:yyyy-MM-dd HH:mm}");
                _logger.LogInformation($"   DataFinePrevista: {vecchiaDataFine:yyyy-MM-dd HH:mm} → {ordine.DataFinePrevista:yyyy-MM-dd HH:mm}");

                return Ok(new { 
                    success = true, 
                    message = "✅ Ordine spostato con successo",
                    dataInizioOP = ordine.DataInizioOP,
                    dataFinePrevista = ordine.DataFinePrevista
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Errore nell'aggiornamento drag&drop ordine {request.Id}");
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
}

