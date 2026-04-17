using AiDbMaster.Data;
using AiDbMaster.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AiDbMaster.Controllers
{
    [Authorize]
    public class ComposizioneViaggiController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ComposizioneViaggiController> _logger;

        private static readonly Dictionary<string, string> TipiMezzo = new()
        {
            { "MotriceGru", "Motrice Gru" },
            { "AutotrenoGru", "Autotreno Gru" },
            { "AutotrenoAbbinato", "Autotreno Abbinato" },
            { "AutotrenoNoGru", "Autotreno No Gru" },
            { "Bilico", "Bilico" },
            { "BilicoInAbbinamento", "Bilico in Abbinamento" },
            { "MotriceInAbbinamento", "Motrice in Abbinamento" },
            { "Trasporto", "Trasporto" },
            { "TrasportoPosa", "Trasporto e Posa" },
            { "NessunMezzo", "Nessun Mezzo" }
        };

        public ComposizioneViaggiController(
            ApplicationDbContext context,
            ILogger<ComposizioneViaggiController> logger)
        {
            _context = context;
            _logger = logger;
        }

        public IActionResult Index()
        {
            ViewBag.Title = "Composizione Viaggi";
            return View();
        }

        /// <summary>
        /// Restituisce le province distinte degli ordini aperti (StatoEvasione != 'E')
        /// ricavate sia dal cliente sia dalla destinazione diversa
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetProvince()
        {
            try
            {
                // Province dai clienti degli ordini aperti
                var provinceClienti = await _context.OrdiniTestate
                    .AsNoTracking()
                    .Where(t => t.TipoOrdine == "R" && t.StatoEvasione != "E")
                    .Join(_context.AnagraficaClienti,
                        t => t.CodiceCliente,
                        c => c.CodiceCliente,
                        (t, c) => c.Provincia)
                    .Where(p => p != null && p != "")
                    .Distinct()
                    .ToListAsync();

                // Province dalle destinazioni diverse degli ordini aperti
                var provinceDestinazioni = await _context.OrdiniTestate
                    .AsNoTracking()
                    .Where(t => t.TipoOrdine == "R" && t.StatoEvasione != "E" && t.CodiceDestinazione != null)
                    .Join(_context.DestinazioniDiverse,
                        t => new { CodiceConto = t.CodiceCliente, CodiceDestinazione = t.CodiceDestinazione!.Value },
                        d => new { CodiceConto = d.CodiceConto, CodiceDestinazione = d.CodiceDestinazione },
                        (t, d) => d.Provincia)
                    .Where(p => p != null && p != "")
                    .Distinct()
                    .ToListAsync();

                var tutteProvince = provinceClienti
                    .Union(provinceDestinazioni)
                    .Where(p => !string.IsNullOrWhiteSpace(p))
                    .Select(p => p!.Trim().ToUpper())
                    .Distinct()
                    .OrderBy(p => p)
                    .Select(p => new { sigla = p })
                    .ToList();

                return Json(tutteProvince);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore nel recupero delle province");
                return StatusCode(500, new { error = true, message = ex.InnerException?.Message ?? ex.Message });
            }
        }

        /// <summary>
        /// Restituisce le righe ordine filtrate per tipo mezzo e province selezionate.
        /// La provincia si determina dalla destinazione diversa (se presente) o dal cliente.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetRighePerTipoMezzo(string tipoMezzo, [FromQuery] string[]? province, DateTime? dataConsegnaDa, DateTime? dataConsegnaA, bool portoFranco = false)
        {
            try
            {
                if (!TipiMezzo.ContainsKey(tipoMezzo))
                    return BadRequest(new { error = true, message = $"Tipo mezzo '{tipoMezzo}' non valido" });

                // Carica testate ordini clienti aperti con il tipo mezzo selezionato
                var queryTestate = _context.OrdiniTestate
                    .AsNoTracking()
                    .Where(t => t.TipoOrdine == "R" && t.StatoEvasione != "E");

                if (portoFranco)
                    queryTestate = queryTestate.Where(t => t.Porto == "1");

                // Filtra per il tipo mezzo specifico
                queryTestate = tipoMezzo switch
                {
                    "MotriceGru" => queryTestate.Where(t => t.MotriceGru == "S"),
                    "AutotrenoGru" => queryTestate.Where(t => t.AutotrenoGru == "S"),
                    "AutotrenoAbbinato" => queryTestate.Where(t => t.AutotrenoAbbinato == "S"),
                    "AutotrenoNoGru" => queryTestate.Where(t => t.AutotrenoNoGru == "S"),
                    "Bilico" => queryTestate.Where(t => t.Bilico == "S"),
                    "BilicoInAbbinamento" => queryTestate.Where(t => t.BilicoInAbbinamento == "S"),
                    "MotriceInAbbinamento" => queryTestate.Where(t => t.MotriceInAbbinamento == "S"),
                    "Trasporto" => queryTestate.Where(t => t.Trasporto == "S"),
                    "TrasportoPosa" => queryTestate.Where(t => t.TrasportoPosa == "S"),
                    "NessunMezzo" => queryTestate.Where(t =>
                        t.MotriceGru != "S" && t.AutotrenoGru != "S" && t.AutotrenoAbbinato != "S" &&
                        t.AutotrenoNoGru != "S" && t.Bilico != "S" && t.BilicoInAbbinamento != "S" &&
                        t.MotriceInAbbinamento != "S" && t.Trasporto != "S" && t.TrasportoPosa != "S"),
                    _ => queryTestate.Where(t => false)
                };

                var testate = await queryTestate
                    .Include(t => t.Cliente)
                    .Include(t => t.Agente)
                    .Select(t => new
                    {
                        t.Id,
                        t.TipoOrdine,
                        t.AnnoOrdine,
                        t.SerieOrdine,
                        t.NumeroOrdine,
                        t.DataConsegna,
                        t.CodiceCliente,
                        t.CodiceDestinazione,
                        t.CodiceAgente,
                        t.RiferimentoOrdine,
                        ClienteRagioneSociale = t.Cliente != null ? t.Cliente.RagioneSociale : "",
                        ClienteProvincia = t.Cliente != null ? t.Cliente.Provincia : "",
                        ClienteCitta = t.Cliente != null ? t.Cliente.Citta : "",
                        AgenteDescrizione = t.Agente != null ? t.Agente.DescrizioneAgente : "",
                        t.NoteTestata,
                        t.Porto,
                        t.PesoKg
                    })
                    .ToListAsync();

                if (!testate.Any())
                    return Json(new List<object>());

                // Carica destinazioni diverse per le testate che ne hanno una
                var testateConDest = testate.Where(t => t.CodiceDestinazione.HasValue).ToList();
                var destinazioni = new Dictionary<string, DestinazioniDiverse>();

                if (testateConDest.Any())
                {
                    var keys = testateConDest
                        .Select(t => new { t.CodiceCliente, CodDest = t.CodiceDestinazione!.Value })
                        .Distinct()
                        .ToList();

                    var destList = await _context.DestinazioniDiverse
                        .AsNoTracking()
                        .Where(d => keys.Select(k => k.CodiceCliente).Contains(d.CodiceConto)
                                 && keys.Select(k => k.CodDest).Contains(d.CodiceDestinazione))
                        .ToListAsync();

                    foreach (var d in destList)
                        destinazioni[$"{d.CodiceConto}|{d.CodiceDestinazione}"] = d;
                }

                // Determina la provincia effettiva per ogni testata
                var testateConProvincia = testate.Select(t =>
                {
                    string? provincia = null;
                    string? localita = null;
                    string? indirizzo = null;

                    if (t.CodiceDestinazione.HasValue)
                    {
                        var key = $"{t.CodiceCliente}|{t.CodiceDestinazione.Value}";
                        if (destinazioni.TryGetValue(key, out var dest))
                        {
                            provincia = dest.Provincia;
                            localita = dest.Localita;
                            indirizzo = dest.IndirizzoCompleto;
                        }
                    }

                    provincia ??= t.ClienteProvincia;
                    localita ??= t.ClienteCitta;

                    return new
                    {
                        t.Id,
                        t.TipoOrdine,
                        t.AnnoOrdine,
                        t.SerieOrdine,
                        t.NumeroOrdine,
                        t.DataConsegna,
                        t.CodiceCliente,
                        t.ClienteRagioneSociale,
                        t.RiferimentoOrdine,
                        t.AgenteDescrizione,
                        t.NoteTestata,
                        t.Porto,
                        t.PesoKg,
                        Provincia = (provincia ?? "").Trim().ToUpper(),
                        Localita = localita ?? "",
                        IndirizzoDestinazione = indirizzo ?? ""
                    };
                }).ToList();

                // Filtra per province se selezionate
                if (province != null && province.Length > 0)
                {
                    var provUpper = province.Select(p => p.Trim().ToUpper()).ToHashSet();
                    testateConProvincia = testateConProvincia
                        .Where(t => provUpper.Contains(t.Provincia))
                        .ToList();
                }

                if (!testateConProvincia.Any())
                    return Json(new List<object>());

                // Carica le righe ordine delle testate filtrate
                var testataIds = testateConProvincia.Select(t => t.Id).ToHashSet();
                var testateDict = testateConProvincia.ToDictionary(t => t.Id);

                var righe = await _context.OrdiniRighe
                    .AsNoTracking()
                    .Include(r => r.Testata)
                    .Where(r => r.Testata != null && testataIds.Contains(r.Testata.Id))
                    .Where(r => r.StatoEvasione != "E")
                    .ToListAsync();

                // Filtra per data consegna se specificata
                if (dataConsegnaDa.HasValue)
                    righe = righe.Where(r => r.DataConsegna >= dataConsegnaDa.Value).ToList();
                if (dataConsegnaA.HasValue)
                    righe = righe.Where(r => r.DataConsegna <= dataConsegnaA.Value).ToList();

                // Verifica righe già assegnate a viaggi
                var righeIds = righe.Select(r => r.Id).ToList();
                var righeGiaAssegnateList = await _context.ViaggioConsegnaRighe
                    .AsNoTracking()
                    .Where(vr => righeIds.Contains(vr.OrdineRigaId))
                    .Select(vr => vr.OrdineRigaId)
                    .Distinct()
                    .ToListAsync();
                var righeGiaAssegnate = righeGiaAssegnateList.ToHashSet();

                var risultato = righe
                    .Where(r => r.Testata != null && testateDict.ContainsKey(r.Testata.Id))
                    .Select(r =>
                    {
                        var t = testateDict[r.Testata!.Id];
                        var qtaRim = r.Quantita - r.QuantitaEvasa;
                        return new
                        {
                            rigaId = r.Id,
                            ordineId = t.Id,
                            numeroOrdine = $"{t.TipoOrdine}{t.AnnoOrdine}/{t.SerieOrdine}/{t.NumeroOrdine:D6}",
                            annoOrdine = t.AnnoOrdine,
                            numOrdine = t.NumeroOrdine,
                            cliente = t.ClienteRagioneSociale,
                            provincia = t.Provincia,
                            localita = t.Localita,
                            indirizzoDestinazione = t.IndirizzoDestinazione,
                            agente = t.AgenteDescrizione ?? "",
                            riferimentoOrdine = t.RiferimentoOrdine ?? "",
                            codiceArticolo = r.CodiceArticolo,
                            descrizioneArticolo = r.DescrizioneArticolo ?? "",
                            quantita = r.Quantita,
                            quantitaEvasa = r.QuantitaEvasa,
                            quantitaRimanente = qtaRim,
                            unitaMisura = r.UnitaMisura ?? "",
                            pesoKgUnitario = r.PesoKg,
                            pesoKgTotale = r.PesoKg * qtaRim,
                            dataConsegna = r.DataConsegna.ToString("dd/MM/yyyy"),
                            statoEvasione = r.StatoEvasione,
                            descrizioneStatoEvasione = r.DescrizioneStatoEvasione,
                            noteTestata = t.NoteTestata ?? "",
                            porto = t.Porto ?? "",
                            giaAssegnata = righeGiaAssegnate.Contains(r.Id)
                        };
                    })
                    .OrderBy(r => r.provincia)
                    .ThenBy(r => r.localita)
                    .ThenBy(r => r.cliente)
                    .ThenBy(r => r.numeroOrdine)
                    .ToList();

                return Json(risultato);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore nel recupero righe per tipo mezzo {TipoMezzo}", tipoMezzo);
                return StatusCode(500, new { error = true, message = ex.InnerException?.Message ?? ex.Message });
            }
        }

        /// <summary>
        /// Restituisce il conteggio righe per ciascun tipo mezzo (per i badge nelle tab)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetConteggiTipiMezzo([FromQuery] string[]? province)
        {
            try
            {
                var query = _context.OrdiniTestate
                    .AsNoTracking()
                    .Where(t => t.TipoOrdine == "R" && t.StatoEvasione != "E");

                var testate = await query.Select(t => new
                {
                    t.Id,
                    t.CodiceCliente,
                    t.CodiceDestinazione,
                    t.MotriceGru,
                    t.AutotrenoGru,
                    t.AutotrenoAbbinato,
                    t.AutotrenoNoGru,
                    t.Bilico,
                    t.BilicoInAbbinamento,
                    t.MotriceInAbbinamento,
                    t.Trasporto,
                    t.TrasportoPosa
                }).ToListAsync();

                // Se filtro province, dobbiamo determinare la provincia di ogni testata
                // Per semplicità, usiamo la provincia del cliente
                if (province != null && province.Length > 0)
                {
                    var provUpper = province.Select(p => p.Trim().ToUpper()).ToHashSet();
                    var clientiIds = testate.Select(t => t.CodiceCliente).Distinct().ToList();
                    var clientiProv = await _context.AnagraficaClienti
                        .AsNoTracking()
                        .Where(c => clientiIds.Contains(c.CodiceCliente) && c.Provincia != null)
                        .Select(c => new { c.CodiceCliente, c.Provincia })
                        .ToListAsync();

                    var clientiProvDict = clientiProv
                        .GroupBy(c => c.CodiceCliente)
                        .ToDictionary(g => g.Key, g => (g.First().Provincia ?? "").Trim().ToUpper());

                    testate = testate
                        .Where(t => clientiProvDict.ContainsKey(t.CodiceCliente)
                                 && provUpper.Contains(clientiProvDict[t.CodiceCliente]))
                        .ToList();
                }

                var conteggi = new Dictionary<string, int>
                {
                    ["MotriceGru"] = testate.Count(t => t.MotriceGru == "S"),
                    ["AutotrenoGru"] = testate.Count(t => t.AutotrenoGru == "S"),
                    ["AutotrenoAbbinato"] = testate.Count(t => t.AutotrenoAbbinato == "S"),
                    ["AutotrenoNoGru"] = testate.Count(t => t.AutotrenoNoGru == "S"),
                    ["Bilico"] = testate.Count(t => t.Bilico == "S"),
                    ["BilicoInAbbinamento"] = testate.Count(t => t.BilicoInAbbinamento == "S"),
                    ["MotriceInAbbinamento"] = testate.Count(t => t.MotriceInAbbinamento == "S"),
                    ["Trasporto"] = testate.Count(t => t.Trasporto == "S"),
                    ["TrasportoPosa"] = testate.Count(t => t.TrasportoPosa == "S"),
                    ["NessunMezzo"] = testate.Count(t =>
                        t.MotriceGru != "S" && t.AutotrenoGru != "S" && t.AutotrenoAbbinato != "S" &&
                        t.AutotrenoNoGru != "S" && t.Bilico != "S" && t.BilicoInAbbinamento != "S" &&
                        t.MotriceInAbbinamento != "S" && t.Trasporto != "S" && t.TrasportoPosa != "S")
                };

                return Json(conteggi);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore nel conteggio tipi mezzo");
                return StatusCode(500, new { error = true, message = ex.InnerException?.Message ?? ex.Message });
            }
        }

        /// <summary>
        /// Restituisce mezzi interni e autisti per il form di creazione viaggio
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetDatiViaggio()
        {
            try
            {
                var mezziInterni = await _context.MezziTrasporto
                    .AsNoTracking()
                    .Where(m => m.Attivo)
                    .OrderBy(m => m.Descrizione)
                    .Select(m => new { id = m.Id, descrizione = m.Descrizione, portataMaxKg = m.PortataMaxKg })
                    .ToListAsync();

                var mezziEsterni = await _context.MezziTrasportoEsterni
                    .AsNoTracking()
                    .OrderBy(m => m.NomeVettore)
                    .Select(m => new { id = m.Id, descrizione = $"{m.NomeVettore} - {m.Comune}" })
                    .ToListAsync();

                var autisti = await _context.Autisti
                    .AsNoTracking()
                    .Where(a => a.Attivo)
                    .OrderBy(a => a.Cognome).ThenBy(a => a.Nome)
                    .Select(a => new { id = a.Id, nome = $"{a.Cognome} {a.Nome}".Trim() })
                    .ToListAsync();

                var tipiTrasporto = await _context.TipiTrasporto
                    .AsNoTracking()
                    .Where(t => t.Attivo)
                    .OrderBy(t => t.Descrizione)
                    .Select(t => new { id = t.Id, codice = t.Codice, descrizione = t.Descrizione })
                    .ToListAsync();

                return Json(new { mezziInterni, mezziEsterni, autisti, tipiTrasporto });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore nel recupero dati viaggio");
                return StatusCode(500, new { error = true, message = ex.InnerException?.Message ?? ex.Message });
            }
        }

        /// <summary>
        /// Crea un nuovo viaggio con le righe selezionate
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreaViaggio([FromBody] CreaViaggioRequest request)
        {
            try
            {
                if (request.Righe == null || !request.Righe.Any())
                    return BadRequest(new { success = false, message = "Selezionare almeno una riga ordine" });

                var righeIds = request.Righe.Select(r => r.RigaId).ToList();
                var qtaPerRiga = request.Righe.ToDictionary(r => r.RigaId, r => r.Quantita);

                // Verifica che le righe esistano
                var righe = await _context.OrdiniRighe
                    .Where(r => righeIds.Contains(r.Id))
                    .ToListAsync();

                if (righe.Count != righeIds.Count)
                    return BadRequest(new { success = false, message = "Alcune righe selezionate non sono state trovate" });

                // Verifica che le righe non siano già assegnate
                var righeGiaAssegnate = await _context.ViaggioConsegnaRighe
                    .Where(vr => righeIds.Contains(vr.OrdineRigaId))
                    .Select(vr => vr.OrdineRigaId)
                    .ToListAsync();

                if (righeGiaAssegnate.Any())
                    return BadRequest(new { success = false, message = $"Le righe {string.Join(", ", righeGiaAssegnate)} sono già assegnate a un viaggio" });

                // Verifica disponibilità mezzo interno nella stessa data
                if (request.MezzoTrasportoId.HasValue)
                {
                    var mezzoOccupato = await _context.ViaggiConsegna
                        .AsNoTracking()
                        .AnyAsync(v => v.MezzoTrasportoId == request.MezzoTrasportoId.Value
                                    && v.DataConsegna == request.DataConsegna
                                    && v.Stato != "Annullato");
                    if (mezzoOccupato)
                    {
                        var mezzo = await _context.MezziTrasporto.AsNoTracking()
                            .Where(m => m.Id == request.MezzoTrasportoId.Value)
                            .Select(m => m.Descrizione).FirstOrDefaultAsync();
                        return BadRequest(new { success = false, message = $"Il mezzo '{mezzo}' è già impegnato per il {request.DataConsegna:dd/MM/yyyy}" });
                    }
                }

                // Calcola ora arrivo se non fornita: partenza + durata stimata
                var oraArrivo = request.OraArrivo ?? request.OraPartenza.Add(TimeSpan.FromMinutes(request.DurataStimataMinuti > 0 ? request.DurataStimataMinuti : 240));

                var viaggio = new ViaggioConsegna
                {
                    DataConsegna = request.DataConsegna,
                    TipoTrasportoId = request.TipoTrasportoId,
                    MezzoTrasportoId = request.MezzoTrasportoId,
                    MezzoTrasportoEsternoId = request.MezzoTrasportoEsternoId,
                    AutistaId = request.AutistaId,
                    OraPartenza = request.OraPartenza,
                    OraArrivo = oraArrivo,
                    DurataStimataMinuti = request.DurataStimataMinuti > 0 ? request.DurataStimataMinuti : 240,
                    Note = request.Note,
                    Stato = "Pianificato",
                    CreatoDa = User.Identity?.Name
                };

                _context.ViaggiConsegna.Add(viaggio);
                await _context.SaveChangesAsync();

                // Crea le righe del viaggio con la quantità personalizzata
                foreach (var riga in righe)
                {
                    var qtaAssegnata = qtaPerRiga.TryGetValue(riga.Id, out var q) ? q : riga.QuantitaRimanente;

                    var viaggioRiga = new ViaggioConsegnaRiga
                    {
                        ViaggioConsegnaId = viaggio.Id,
                        OrdineRigaId = riga.Id,
                        QuantitaAssegnata = qtaAssegnata,
                        PesoUnitarioKgSnapshot = riga.PesoKg,
                        PesoTotaleKgSnapshot = riga.PesoKg * qtaAssegnata
                    };
                    _context.ViaggioConsegnaRighe.Add(viaggioRiga);
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation("Viaggio {ViaggioId} creato con {NumRighe} righe", viaggio.Id, righe.Count);

                return Ok(new
                {
                    success = true,
                    message = $"Viaggio #{viaggio.Id} creato con {righe.Count} righe",
                    viaggioId = viaggio.Id
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore nella creazione del viaggio");
                return StatusCode(500, new { success = false, message = ex.InnerException?.Message ?? ex.Message });
            }
        }
    }

    public class CreaViaggioRequest
    {
        public List<RigaViaggioInput> Righe { get; set; } = new();
        public DateTime DataConsegna { get; set; }
        public int TipoTrasportoId { get; set; }
        public int? MezzoTrasportoId { get; set; }
        public int? MezzoTrasportoEsternoId { get; set; }
        public int? AutistaId { get; set; }
        public TimeSpan OraPartenza { get; set; }
        public TimeSpan? OraArrivo { get; set; }
        public int DurataStimataMinuti { get; set; } = 240;
        public string? Note { get; set; }
    }

    public class RigaViaggioInput
    {
        public int RigaId { get; set; }
        public decimal Quantita { get; set; }
    }
}
