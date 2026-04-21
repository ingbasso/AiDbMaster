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
        public async Task<IActionResult> GetRighePerTipoMezzo(string tipoMezzo, [FromQuery] string[]? province, DateTime? dataConsegnaDa, DateTime? dataConsegnaA, bool portoFranco = false, bool escludiEvasi = false, bool escludiSpediti = false)
        {
            try
            {
                if (!TipiMezzo.ContainsKey(tipoMezzo))
                    return BadRequest(new { error = true, message = $"Tipo mezzo '{tipoMezzo}' non valido" });

                // Carica testate ordini clienti aperti con il tipo mezzo selezionato
                var queryTestate = _context.OrdiniTestate
                    .AsNoTracking()
                    .Where(t => t.TipoOrdine == "R");

                if (escludiEvasi)
                    queryTestate = queryTestate.Where(t => t.StatoEvasione != "E");

                if (portoFranco)
                    queryTestate = queryTestate.Where(t => t.Porto == "1");

                // Il filtro data consegna si applica SOLO agli ordini evasi (StatoEvasione = 'E'),
                // gli ordini non evasi vengono sempre mostrati.
                // Se nessuna data è fornita, usa oggi -7 giorni come default per gli evasi.
                var dataLimiteEvasi = dataConsegnaDa ?? DateTime.Today.AddDays(-7);
                queryTestate = queryTestate.Where(t =>
                    t.StatoEvasione != "E" ||
                    (t.DataConsegna >= dataLimiteEvasi &&
                     (!dataConsegnaA.HasValue || t.DataConsegna <= dataConsegnaA.Value)));

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
                    .Include(r => r.Articolo)
                    .Where(r => r.Testata != null && testataIds.Contains(r.Testata.Id))
                    .Where(r => r.StatoEvasione != "E")
                    .ToListAsync();

                // Calcola quantità già assegnata ai viaggi (non annullati) per ogni riga
                // distinguendo tra viaggi normali e manuali
                var righeIds = righe.Select(r => r.Id).ToList();
                var assegnazioniPerRiga = await _context.ViaggioConsegnaRighe
                    .AsNoTracking()
                    .Where(vr => righeIds.Contains(vr.OrdineRigaId))
                    .Join(_context.ViaggiConsegna.Where(v => v.Stato != "Annullato"),
                          vr => vr.ViaggioConsegnaId, v => v.Id, (vr, v) => new { vr.OrdineRigaId, vr.QuantitaAssegnata, v.IsManuale })
                    .GroupBy(x => x.OrdineRigaId)
                    .Select(g => new
                    {
                        RigaId = g.Key,
                        TotaleAssegnato = g.Sum(x => x.QuantitaAssegnata),
                        TotaleManuale = g.Where(x => x.IsManuale).Sum(x => x.QuantitaAssegnata),
                        TotaleViaggi = g.Where(x => !x.IsManuale).Sum(x => x.QuantitaAssegnata)
                    })
                    .ToDictionaryAsync(x => x.RigaId);

                // Wrapper per accesso rapido
                decimal QtaTotAssegnata(int rigaId) => assegnazioniPerRiga.TryGetValue(rigaId, out var a) ? a.TotaleAssegnato : 0;
                decimal QtaManuale(int rigaId) => assegnazioniPerRiga.TryGetValue(rigaId, out var a) ? a.TotaleManuale : 0;
                decimal QtaViaggi(int rigaId) => assegnazioniPerRiga.TryGetValue(rigaId, out var a) ? a.TotaleViaggi : 0;

                var risultato = righe
                    .Where(r => r.Testata != null && testateDict.ContainsKey(r.Testata.Id))
                    .Select(r =>
                    {
                        var t = testateDict[r.Testata!.Id];
                        var qtaRim = r.Quantita - r.QuantitaEvasa;
                        var qtaGiaAssegnata = QtaTotAssegnata(r.Id);
                        var qtaSoloManuali = QtaManuale(r.Id);
                        var qtaSoloViaggi = QtaViaggi(r.Id);
                        var qtaDaSpedire = r.Quantita - qtaGiaAssegnata;
                        if (qtaDaSpedire < 0) qtaDaSpedire = 0;

                        var statoSped = qtaGiaAssegnata <= 0 ? "NS"
                            : qtaGiaAssegnata >= r.Quantita ? "SP" : "PS";
                        var descrStatoSped = statoSped switch
                        {
                            "NS" => "Non Spedita",
                            "PS" => "Parz. Spedita",
                            "SP" => "Spedita",
                            _ => "Sconosciuto"
                        };

                        // Se completamente spedita solo tramite manuali (nessun viaggio reale)
                        var soloManuale = qtaSoloManuali > 0 && qtaSoloViaggi <= 0;
                        var hasManuali = qtaSoloManuali > 0;

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
                            qtaGiaAssegnataViaggi = qtaGiaAssegnata,
                            qtaDaSpedire = qtaDaSpedire,
                            statoSpedizione = statoSped,
                            descrizioneStatoSpedizione = descrStatoSped,
                            completamenteSpedita = statoSped == "SP",
                            soloManuale = soloManuale,
                            hasManuali = hasManuali,
                            qtaPerPallet = r.Articolo?.QtaUMPPerPallet ?? 0m,
                            tavolePerPallet = r.Articolo?.TavolePerPallet ?? 0m,
                            qtaPerTavola = r.Articolo?.QtaUMPPerTavola ?? 0m
                        };
                    })
                    .OrderBy(r => r.provincia)
                    .ThenBy(r => r.localita)
                    .ThenBy(r => r.cliente)
                    .ThenBy(r => r.numeroOrdine)
                    .ToList();

                if (escludiSpediti)
                    risultato = risultato.Where(r => !r.completamenteSpedita).ToList();

                return Json(risultato);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore nel recupero righe per tipo mezzo {TipoMezzo}", tipoMezzo);
                return StatusCode(500, new { error = true, message = ex.InnerException?.Message ?? ex.Message });
            }
        }

        /// <summary>
        /// Restituisce il conteggio RIGHE per ciascun tipo mezzo (per i badge nelle tab).
        /// Usa la stessa logica di filtro di GetRighePerTipoMezzo per essere allineato.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetConteggiTipiMezzo([FromQuery] string[]? province, DateTime? dataConsegnaDa, DateTime? dataConsegnaA, bool portoFranco = false, bool escludiEvasi = false, bool escludiSpediti = false)
        {
            try
            {
                var queryTestate = _context.OrdiniTestate
                    .AsNoTracking()
                    .Include(t => t.Cliente)
                    .Where(t => t.TipoOrdine == "R");

                if (escludiEvasi)
                    queryTestate = queryTestate.Where(t => t.StatoEvasione != "E");

                if (portoFranco)
                    queryTestate = queryTestate.Where(t => t.Porto == "1");

                var dataLimiteEvasi = dataConsegnaDa ?? DateTime.Today.AddDays(-7);
                queryTestate = queryTestate.Where(t =>
                    t.StatoEvasione != "E" ||
                    (t.DataConsegna >= dataLimiteEvasi &&
                     (!dataConsegnaA.HasValue || t.DataConsegna <= dataConsegnaA.Value)));

                var testate = await queryTestate.Select(t => new
                {
                    t.Id,
                    t.CodiceCliente,
                    t.CodiceDestinazione,
                    ClienteProvincia = t.Cliente != null ? t.Cliente.Provincia : "",
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

                if (!testate.Any())
                    return Json(new Dictionary<string, int>());

                // Risolvi provincia da destinazione diversa (stessa logica della griglia)
                var testateConDest = testate.Where(t => t.CodiceDestinazione.HasValue).ToList();
                var destProvDict = new Dictionary<int, string>();

                if (testateConDest.Any())
                {
                    var destList = await _context.DestinazioniDiverse
                        .AsNoTracking()
                        .Where(d => testateConDest.Select(t => t.CodiceCliente).Contains(d.CodiceConto)
                                 && testateConDest.Select(t => t.CodiceDestinazione!.Value).Contains(d.CodiceDestinazione))
                        .Select(d => new { d.CodiceConto, d.CodiceDestinazione, d.Provincia })
                        .ToListAsync();

                    var destLookup = destList.ToDictionary(d => $"{d.CodiceConto}|{d.CodiceDestinazione}", d => (d.Provincia ?? "").Trim().ToUpper());

                    foreach (var t in testateConDest)
                    {
                        var key = $"{t.CodiceCliente}|{t.CodiceDestinazione!.Value}";
                        if (destLookup.TryGetValue(key, out var prov) && !string.IsNullOrEmpty(prov))
                            destProvDict[t.Id] = prov;
                    }
                }

                string GetProvincia(int testataId, int codiceCliente, string? clienteProv)
                {
                    if (destProvDict.TryGetValue(testataId, out var destProv))
                        return destProv;
                    return (clienteProv ?? "").Trim().ToUpper();
                }

                // Filtra per province selezionate (stessa logica della griglia: provincia da destinazione o cliente)
                if (province != null && province.Length > 0)
                {
                    var provUpper = province.Select(p => p.Trim().ToUpper()).ToHashSet();
                    testate = testate
                        .Where(t => provUpper.Contains(GetProvincia(t.Id, t.CodiceCliente, t.ClienteProvincia)))
                        .ToList();
                }

                if (!testate.Any())
                    return Json(new Dictionary<string, int>());

                // Carica righe non evase (stessa logica della griglia)
                var testataIds = testate.Select(t => t.Id).ToHashSet();

                var righePerTestata = await _context.OrdiniRighe
                    .AsNoTracking()
                    .Where(r => r.Testata != null && testataIds.Contains(r.Testata.Id))
                    .Where(r => r.StatoEvasione != "E")
                    .Select(r => new { TestataId = r.Testata!.Id, r.Id, r.Quantita })
                    .ToListAsync();

                // Se escludiSpediti, escludi righe completamente assegnate ai viaggi
                if (escludiSpediti)
                {
                    var righeIds = righePerTestata.Select(r => r.Id).ToList();
                    var assegnazioni = await _context.ViaggioConsegnaRighe
                        .AsNoTracking()
                        .Where(vr => righeIds.Contains(vr.OrdineRigaId))
                        .Join(_context.ViaggiConsegna.Where(v => v.Stato != "Annullato"),
                              vr => vr.ViaggioConsegnaId, v => v.Id, (vr, v) => new { vr.OrdineRigaId, vr.QuantitaAssegnata })
                        .GroupBy(x => x.OrdineRigaId)
                        .Select(g => new { RigaId = g.Key, Totale = g.Sum(x => x.QuantitaAssegnata) })
                        .ToDictionaryAsync(x => x.RigaId, x => x.Totale);

                    righePerTestata = righePerTestata
                        .Where(r => assegnazioni.GetValueOrDefault(r.Id, 0) < r.Quantita)
                        .ToList();
                }

                // Mappa testataId → tipo mezzo flags
                var testateDict = testate.ToDictionary(t => t.Id);

                // Conta righe per tipo mezzo
                var conteggi = new Dictionary<string, int>
                {
                    ["MotriceGru"] = 0, ["AutotrenoGru"] = 0, ["AutotrenoAbbinato"] = 0,
                    ["AutotrenoNoGru"] = 0, ["Bilico"] = 0, ["BilicoInAbbinamento"] = 0,
                    ["MotriceInAbbinamento"] = 0, ["Trasporto"] = 0, ["TrasportoPosa"] = 0,
                    ["NessunMezzo"] = 0
                };

                foreach (var riga in righePerTestata)
                {
                    if (!testateDict.TryGetValue(riga.TestataId, out var t)) continue;

                    if (t.MotriceGru == "S") conteggi["MotriceGru"]++;
                    if (t.AutotrenoGru == "S") conteggi["AutotrenoGru"]++;
                    if (t.AutotrenoAbbinato == "S") conteggi["AutotrenoAbbinato"]++;
                    if (t.AutotrenoNoGru == "S") conteggi["AutotrenoNoGru"]++;
                    if (t.Bilico == "S") conteggi["Bilico"]++;
                    if (t.BilicoInAbbinamento == "S") conteggi["BilicoInAbbinamento"]++;
                    if (t.MotriceInAbbinamento == "S") conteggi["MotriceInAbbinamento"]++;
                    if (t.Trasporto == "S") conteggi["Trasporto"]++;
                    if (t.TrasportoPosa == "S") conteggi["TrasportoPosa"]++;

                    if (t.MotriceGru != "S" && t.AutotrenoGru != "S" && t.AutotrenoAbbinato != "S" &&
                        t.AutotrenoNoGru != "S" && t.Bilico != "S" && t.BilicoInAbbinamento != "S" &&
                        t.MotriceInAbbinamento != "S" && t.Trasporto != "S" && t.TrasportoPosa != "S")
                        conteggi["NessunMezzo"]++;
                }

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
                    .Select(m => new {
                        id = m.Id,
                        descrizione = m.Descrizione,
                        portataMaxKg = m.PortataMaxKg,
                        rimorchioDisponibile = m.RimorchioDisponibile,
                        portataMaxConRimorchioKg = m.PortataMaxConRimorchioKg
                    })
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
        /// Restituisce tutti i mezzi esterni con dettagli completi per il popup di selezione
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetMezziEsterni()
        {
            try
            {
                var mezzi = await _context.MezziTrasportoEsterni
                    .AsNoTracking()
                    .OrderBy(m => m.Provincia).ThenBy(m => m.Comune).ThenBy(m => m.NomeVettore)
                    .Select(m => new {
                        id = m.Id,
                        comune = m.Comune,
                        provincia = m.Provincia ?? "",
                        regione = m.Regione ?? "",
                        nomeVettore = m.NomeVettore ?? "",
                        tipoMezzo = m.TipoMezzo ?? "",
                        costo = m.Costo,
                        portataMax = m.PortataMax,
                        gru = m.Gru,
                        trasbordo = m.Trasbordo,
                        note = m.Note ?? ""
                    })
                    .ToListAsync();

                return Json(mezzi);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore nel recupero mezzi esterni");
                return StatusCode(500, new { error = true, message = ex.InnerException?.Message ?? ex.Message });
            }
        }

        /// <summary>
        /// Aggiorna il costo di un mezzo esterno direttamente dalla composizione viaggi
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> UpdateCostoMezzoEsterno([FromBody] UpdateCostoMezzoEsternoRequest request)
        {
            try
            {
                var mezzo = await _context.MezziTrasportoEsterni.FindAsync(request.Id);
                if (mezzo == null)
                    return Json(new { success = false, message = "Mezzo esterno non trovato." });

                mezzo.Costo = request.Costo;
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = $"Costo aggiornato a € {request.Costo:F2} per {mezzo.NomeVettore} - {mezzo.Comune}" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore nell'aggiornamento costo mezzo esterno {Id}", request.Id);
                return StatusCode(500, new { success = false, message = ex.InnerException?.Message ?? ex.Message });
            }
        }

        /// <summary>
        /// Restituisce i viaggi collegati a un ordine (testata), con le righe assegnate
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetViaggiPerOrdine(int ordineTestataId)
        {
            try
            {
                var testata = await _context.OrdiniTestate
                    .AsNoTracking()
                    .Where(t => t.Id == ordineTestataId)
                    .Select(t => new
                    {
                        t.Id,
                        NumeroOrdine = $"{t.TipoOrdine}{t.AnnoOrdine}/{t.SerieOrdine}/{t.NumeroOrdine:D6}",
                        ClienteRagioneSociale = t.Cliente != null ? t.Cliente.RagioneSociale : ""
                    })
                    .FirstOrDefaultAsync();

                if (testata == null)
                    return NotFound(new { success = false, message = "Ordine non trovato" });

                // Carica righe ordine complete
                var righeOrdineEntita = await _context.OrdiniRighe
                    .AsNoTracking()
                    .Where(r => r.Testata != null && r.Testata.Id == ordineTestataId)
                    .ToListAsync();

                var righeOrdineIds = righeOrdineEntita.Select(r => r.Id).ToList();

                // Quantità assegnate ai viaggi non annullati per ogni riga
                var qtaAssegnateDict = await _context.ViaggioConsegnaRighe
                    .AsNoTracking()
                    .Where(vr => righeOrdineIds.Contains(vr.OrdineRigaId))
                    .Join(_context.ViaggiConsegna.Where(v => v.Stato != "Annullato"),
                          vr => vr.ViaggioConsegnaId, v => v.Id, (vr, v) => vr)
                    .GroupBy(vr => vr.OrdineRigaId)
                    .Select(g => new { RigaId = g.Key, Totale = g.Sum(x => x.QuantitaAssegnata) })
                    .ToDictionaryAsync(x => x.RigaId, x => x.Totale);

                // Riepilogo righe con 4 quantità
                var righeRiepilogo = righeOrdineEntita.Select(r =>
                {
                    var qtaSped = qtaAssegnateDict.GetValueOrDefault(r.Id, 0);
                    return new
                    {
                        rigaId = r.Id,
                        rigaOrdine = r.RigaOrdine,
                        codiceArticolo = r.CodiceArticolo,
                        descrizioneArticolo = r.DescrizioneArticolo ?? "",
                        unitaMisura = r.UnitaMisura ?? "",
                        quantitaOrdine = r.Quantita,
                        quantitaResidua = r.Quantita - r.QuantitaEvasa,
                        quantitaEvasa = r.QuantitaEvasa,
                        quantitaSpedita = qtaSped,
                        quantitaDaSpedire = Math.Max(0, r.Quantita - qtaSped),
                        desync = Math.Abs(r.QuantitaEvasa - qtaSped) > 0.001m
                    };
                }).OrderBy(r => r.rigaOrdine).ToList();

                // Dettaglio viaggi
                var viaggiRighe = await _context.ViaggioConsegnaRighe
                    .AsNoTracking()
                    .Where(vr => righeOrdineIds.Contains(vr.OrdineRigaId))
                    .Include(vr => vr.ViaggioConsegna)
                        .ThenInclude(v => v!.TipoTrasporto)
                    .Include(vr => vr.ViaggioConsegna)
                        .ThenInclude(v => v!.MezzoTrasporto)
                    .Include(vr => vr.ViaggioConsegna)
                        .ThenInclude(v => v!.Autista)
                    .Include(vr => vr.OrdineRiga)
                    .ToListAsync();

                var viaggiGrouped = viaggiRighe
                    .Where(vr => vr.ViaggioConsegna != null)
                    .GroupBy(vr => vr.ViaggioConsegnaId)
                    .Select(g =>
                    {
                        var v = g.First().ViaggioConsegna!;
                        return new
                        {
                            viaggioId = v.Id,
                            dataConsegna = v.DataConsegna.ToString("dd/MM/yyyy"),
                            stato = v.Stato,
                            isManuale = v.IsManuale,
                            oraPartenza = v.OraPartenza.ToString(@"hh\:mm"),
                            oraArrivo = v.OraArrivoEffettiva.ToString(@"hh\:mm"),
                            tipoTrasporto = v.TipoTrasporto?.Descrizione ?? "",
                            mezzo = v.MezzoTrasporto?.Descrizione ?? "",
                            autista = v.Autista != null ? $"{v.Autista.Cognome} {v.Autista.Nome}".Trim() : "",
                            note = v.Note ?? "",
                            righe = g.Select(vr => new
                            {
                                codiceArticolo = vr.OrdineRiga?.CodiceArticolo ?? "",
                                descrizioneArticolo = vr.OrdineRiga?.DescrizioneArticolo ?? "",
                                quantitaAssegnata = vr.QuantitaAssegnata,
                                pesoTotaleKg = vr.PesoTotaleKgSnapshot,
                                unitaMisura = vr.OrdineRiga?.UnitaMisura ?? ""
                            }).ToList()
                        };
                    })
                    .OrderByDescending(v => v.viaggioId)
                    .ToList();

                return Json(new
                {
                    success = true,
                    ordine = testata,
                    righeRiepilogo = righeRiepilogo,
                    viaggi = viaggiGrouped
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore nel recupero viaggi per ordine {OrdineId}", ordineTestataId);
                return StatusCode(500, new { success = false, message = ex.InnerException?.Message ?? ex.Message });
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

                // Calcola quantità già assegnata ai viaggi non annullati per ogni riga
                var qtaGiaAssegnate = await _context.ViaggioConsegnaRighe
                    .Where(vr => righeIds.Contains(vr.OrdineRigaId))
                    .Join(_context.ViaggiConsegna.Where(v => v.Stato != "Annullato"),
                          vr => vr.ViaggioConsegnaId, v => v.Id, (vr, v) => vr)
                    .GroupBy(vr => vr.OrdineRigaId)
                    .Select(g => new { RigaId = g.Key, TotaleAssegnato = g.Sum(x => x.QuantitaAssegnata) })
                    .ToDictionaryAsync(x => x.RigaId, x => x.TotaleAssegnato);

                // Verifica che la quantità richiesta non superi quella ancora da spedire
                var righeEccedenti = new List<string>();
                foreach (var riga in righe)
                {
                    var qtaRichiesta = qtaPerRiga.TryGetValue(riga.Id, out var q) ? q : riga.Quantita;
                    var qtaGia = qtaGiaAssegnate.GetValueOrDefault(riga.Id, 0);
                    var qtaDisponibile = riga.Quantita - qtaGia;
                    if (qtaDisponibile <= 0)
                        righeEccedenti.Add($"Riga {riga.Id} ({riga.CodiceArticolo}): completamente spedita");
                    else if (qtaRichiesta > qtaDisponibile)
                        righeEccedenti.Add($"Riga {riga.Id} ({riga.CodiceArticolo}): richieste {qtaRichiesta:N2}, disponibili {qtaDisponibile:N2}");
                }

                if (righeEccedenti.Any() && !request.ForzaQuantita)
                    return BadRequest(new { success = false, quantitaEccedente = true, message = "Quantità eccedenti:\n" + string.Join("\n", righeEccedenti) });

                if (righeEccedenti.Any())
                    _logger.LogWarning("Viaggio creato con quantità forzate: {Dettagli}", string.Join("; ", righeEccedenti));

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
                    ConRimorchio = request.ConRimorchio,
                    CostoTrasporto = request.CostoTrasporto,
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
        /// <summary>
        /// Crea una spedizione manuale (viaggio virtuale con IsManuale = true)
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreaSpedizioneManuale([FromBody] SpedizioneManualeRequest request)
        {
            try
            {
                if (request.Righe == null || !request.Righe.Any())
                    return BadRequest(new { success = false, message = "Selezionare almeno una riga ordine" });

                var righeIds = request.Righe.Select(r => r.RigaId).ToList();
                var qtaPerRiga = request.Righe.ToDictionary(r => r.RigaId, r => r.Quantita);

                var righe = await _context.OrdiniRighe
                    .Where(r => righeIds.Contains(r.Id))
                    .ToListAsync();

                if (righe.Count != righeIds.Count)
                    return BadRequest(new { success = false, message = "Alcune righe selezionate non sono state trovate" });

                // Verifica quantità disponibili
                var qtaGiaAssegnate = await _context.ViaggioConsegnaRighe
                    .Where(vr => righeIds.Contains(vr.OrdineRigaId))
                    .Join(_context.ViaggiConsegna.Where(v => v.Stato != "Annullato"),
                          vr => vr.ViaggioConsegnaId, v => v.Id, (vr, v) => vr)
                    .GroupBy(vr => vr.OrdineRigaId)
                    .Select(g => new { RigaId = g.Key, TotaleAssegnato = g.Sum(x => x.QuantitaAssegnata) })
                    .ToDictionaryAsync(x => x.RigaId, x => x.TotaleAssegnato);

                var righeEccedenti = new List<string>();
                foreach (var riga in righe)
                {
                    var qtaRichiesta = qtaPerRiga.TryGetValue(riga.Id, out var q) ? q : riga.Quantita;
                    var qtaGia = qtaGiaAssegnate.GetValueOrDefault(riga.Id, 0);
                    var qtaDisponibile = riga.Quantita - qtaGia;
                    if (qtaDisponibile <= 0)
                        righeEccedenti.Add($"Riga {riga.Id} ({riga.CodiceArticolo}): completamente spedita");
                    else if (qtaRichiesta > qtaDisponibile)
                        righeEccedenti.Add($"Riga {riga.Id} ({riga.CodiceArticolo}): richieste {qtaRichiesta:N2}, disponibili {qtaDisponibile:N2}");
                }

                if (righeEccedenti.Any() && !request.ForzaQuantita)
                    return BadRequest(new { success = false, quantitaEccedente = true, message = "Quantità eccedenti:\n" + string.Join("\n", righeEccedenti) });

                if (righeEccedenti.Any())
                    _logger.LogWarning("Spedizione manuale creata con quantità forzate: {Dettagli}", string.Join("; ", righeEccedenti));

                // Usa il primo tipo trasporto disponibile come default
                var tipoTrasportoDefault = await _context.TipiTrasporto
                    .AsNoTracking()
                    .Where(t => t.Attivo)
                    .Select(t => t.Id)
                    .FirstOrDefaultAsync();

                if (tipoTrasportoDefault == 0)
                    return BadRequest(new { success = false, message = "Nessun tipo trasporto configurato nel sistema" });

                var viaggio = new ViaggioConsegna
                {
                    DataConsegna = request.DataSpedizione ?? DateTime.Today,
                    TipoTrasportoId = tipoTrasportoDefault,
                    OraPartenza = TimeSpan.Zero,
                    DurataStimataMinuti = 0,
                    Note = request.Note,
                    Stato = "Completato",
                    IsManuale = true,
                    CreatoDa = User.Identity?.Name
                };

                _context.ViaggiConsegna.Add(viaggio);
                await _context.SaveChangesAsync();

                foreach (var riga in righe)
                {
                    var qtaAssegnata = qtaPerRiga.TryGetValue(riga.Id, out var q) ? q : riga.Quantita;

                    _context.ViaggioConsegnaRighe.Add(new ViaggioConsegnaRiga
                    {
                        ViaggioConsegnaId = viaggio.Id,
                        OrdineRigaId = riga.Id,
                        QuantitaAssegnata = qtaAssegnata,
                        PesoUnitarioKgSnapshot = riga.PesoKg,
                        PesoTotaleKgSnapshot = riga.PesoKg * qtaAssegnata
                    });
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation("Spedizione manuale #{ViaggioId} creata con {NumRighe} righe", viaggio.Id, righe.Count);

                return Ok(new
                {
                    success = true,
                    message = $"Spedizione manuale registrata ({righe.Count} righe)",
                    viaggioId = viaggio.Id
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore nella creazione della spedizione manuale");
                return StatusCode(500, new { success = false, message = ex.InnerException?.Message ?? ex.Message });
            }
        }
    }

    public class SpedizioneManualeRequest
    {
        public List<RigaViaggioInput> Righe { get; set; } = new();
        public DateTime? DataSpedizione { get; set; }
        public string? Note { get; set; }
        public bool ForzaQuantita { get; set; } = false;
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
        public bool ConRimorchio { get; set; } = false;
        public decimal? CostoTrasporto { get; set; }
        public bool ForzaQuantita { get; set; } = false;
    }

    public class RigaViaggioInput
    {
        public int RigaId { get; set; }
        public decimal Quantita { get; set; }
    }

    public class UpdateCostoMezzoEsternoRequest
    {
        public int Id { get; set; }
        public double Costo { get; set; }
    }
}
