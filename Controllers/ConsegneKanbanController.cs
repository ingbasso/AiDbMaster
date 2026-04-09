using AiDbMaster.Attributes;
using AiDbMaster.Data;
using AiDbMaster.Models;
using AiDbMaster.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.Drawing;

namespace AiDbMaster.Controllers
{
    [Authorize]
    [RegisterResource("ConsegneKanban", "Kanban Consegne", Description = "Kanban pianificazione consegne", MenuIcon = "bi-kanban", MenuOrder = 1)]
    [RequirePermission("ConsegneKanban", "View")]
    public class ConsegneKanbanController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ConsegneKanbanController> _logger;

        public ConsegneKanbanController(ApplicationDbContext context, ILogger<ConsegneKanbanController> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IActionResult> Index(bool nascondiWeekend = true)
        {
            var today = DateTime.Today;
            var startDate = today.AddDays(-7).Date;
            var endDate = today.AddDays(15).Date;

            var durataDefault = await GetDurataDefaultMinutiAsync();

            var viaggi = await _context.ViaggiConsegna
                .AsNoTracking()
                .Include(v => v.TipoTrasporto)
                .Include(v => v.MezzoTrasporto)
                .Include(v => v.MezzoTrasportoEsterno)
                .Include(v => v.Autista)
                .Include(v => v.Righe)
                    .ThenInclude(r => r.OrdineRiga)
                .Where(v => v.DataConsegna >= startDate && v.DataConsegna <= endDate && v.Stato != "Annullato")
                .OrderBy(v => v.DataConsegna)
                .ThenBy(v => v.OraPartenza)
                .ToListAsync();

            var assegnazioniPerRiga = await _context.ViaggioConsegnaRighe
                .AsNoTracking()
                .Include(x => x.ViaggioConsegna)
                .Where(x => x.ViaggioConsegna != null && x.ViaggioConsegna.Stato != "Annullato")
                .GroupBy(x => x.OrdineRigaId)
                .Select(g => new { OrdineRigaId = g.Key, QuantitaAssegnata = g.Sum(x => x.QuantitaAssegnata) })
                .ToDictionaryAsync(x => x.OrdineRigaId, x => x.QuantitaAssegnata);

            var righeOrdine = await (from r in _context.OrdiniRighe.AsNoTracking()
                                     join t in _context.OrdiniTestate.AsNoTracking()
                                         on new { r.TipoOrdine, r.AnnoOrdine, r.SerieOrdine, r.NumeroOrdine }
                                         equals new { t.TipoOrdine, t.AnnoOrdine, t.SerieOrdine, t.NumeroOrdine }
                                     join c in _context.AnagraficaClienti.AsNoTracking()
                                         on t.CodiceCliente equals c.CodiceCliente
                                     join a in _context.AnagraficaArticoli.AsNoTracking()
                                         on r.CodiceArticolo equals a.CodiceArticolo into articoliGroup
                                     from articolo in articoliGroup.DefaultIfEmpty()
                                     join d in _context.DestinazioniDiverse.AsNoTracking()
                                         on new { CodiceConto = t.CodiceCliente, CodiceDestinazione = t.CodiceDestinazione ?? 0 }
                                         equals new { d.CodiceConto, d.CodiceDestinazione } into destGroup
                                     from dest in destGroup.DefaultIfEmpty()
                                     where r.TipoOrdine == "R"
                                     select new
                                     {
                                         Riga = r,
                                         Testata = t,
                                         Cliente = c,
                                         PesoUnitarioKg = articolo != null ? articolo.PesoUnitarioKg : null,
                                         Destinazione = dest
                                     }).ToListAsync();

            var righeDaPianificare = righeOrdine
                .Select(x =>
                {
                    var assegnata = assegnazioniPerRiga.TryGetValue(x.Riga.Id, out var qtaAssegnata) ? qtaAssegnata : 0m;
                    var residua = x.Riga.Quantita - x.Riga.QuantitaEvasa - assegnata;
                    var usaDestDiversa = x.Testata.CodiceDestinazione.HasValue
                        && x.Testata.CodiceDestinazione.Value != 0
                        && x.Destinazione != null;

                    string destinazione;
                    if (usaDestDiversa)
                    {
                        var d = x.Destinazione!;
                        var parti = new List<string>();
                        if (!string.IsNullOrEmpty(d.DescrizioneDestinazione)) parti.Add(d.DescrizioneDestinazione);
                        if (!string.IsNullOrEmpty(d.Indirizzo)) parti.Add(d.Indirizzo);
                        if (!string.IsNullOrEmpty(d.Localita)) parti.Add(d.Localita);
                        if (!string.IsNullOrEmpty(d.Provincia)) parti.Add($"({d.Provincia})");
                        destinazione = parti.Count > 0 ? string.Join(", ", parti) : "Dest. diversa N/D";
                    }
                    else
                    {
                        var parti = new List<string>();
                        if (!string.IsNullOrEmpty(x.Cliente.Indirizzo)) parti.Add(x.Cliente.Indirizzo);
                        if (!string.IsNullOrEmpty(x.Cliente.Citta)) parti.Add(x.Cliente.Citta);
                        if (!string.IsNullOrEmpty(x.Cliente.Provincia)) parti.Add($"({x.Cliente.Provincia})");
                        destinazione = parti.Count > 0 ? string.Join(", ", parti) : "";
                    }

                    return new RigaOrdineDaPianificareDto
                    {
                        OrdineRigaId = x.Riga.Id,
                        OrdineCompleto = $"{x.Riga.TipoOrdine}{x.Riga.AnnoOrdine}/{x.Riga.SerieOrdine}/{x.Riga.NumeroOrdine:D6}",
                        RigaOrdine = x.Riga.RigaOrdine,
                        DataConsegna = x.Riga.DataConsegna,
                        CodiceCliente = x.Testata.CodiceCliente,
                        Cliente = x.Cliente.RagioneSociale ?? $"Cliente {x.Testata.CodiceCliente}",
                        Destinazione = destinazione,
                        ProvinciaDest = usaDestDiversa ? x.Destinazione?.Provincia : x.Cliente.Provincia,
                        ComuneDest = usaDestDiversa ? x.Destinazione?.Localita : x.Cliente.Citta,
                        CodiceArticolo = x.Riga.CodiceArticolo,
                        DescrizioneArticolo = x.Riga.DescrizioneArticolo,
                        QuantitaOriginale = x.Riga.Quantita - x.Riga.QuantitaEvasa,
                        QuantitaGiaAssegnata = assegnata,
                        QuantitaResidua = residua,
                        UnitaMisura = x.Riga.UnitaMisura,
                        PesoUnitarioKg = x.PesoUnitarioKg
                    };
                })
                .Where(x => x.QuantitaResidua > 0
                    && x.DataConsegna.Date >= startDate
                    && x.DataConsegna.Date <= endDate)
                .OrderBy(x => x.DataConsegna)
                .ThenBy(x => x.Cliente)
                .Take(500)
                .ToList();

            var destinazionePerRigaOrdine = righeOrdine.ToDictionary(
                x => x.Riga.Id,
                x =>
                {
                    var usaDest = x.Testata.CodiceDestinazione.HasValue
                        && x.Testata.CodiceDestinazione.Value != 0
                        && x.Destinazione != null;
                    if (usaDest)
                    {
                        var d = x.Destinazione!;
                        return !string.IsNullOrEmpty(d.Localita) ? d.Localita
                            : !string.IsNullOrEmpty(d.DescrizioneDestinazione) ? d.DescrizioneDestinazione
                            : null;
                    }
                    return !string.IsNullOrEmpty(x.Cliente.Citta) ? x.Cliente.Citta
                        : !string.IsNullOrEmpty(x.Cliente.Indirizzo) ? x.Cliente.Indirizzo
                        : null;
                });

            var totalDays = (endDate - startDate).Days + 1;
            var giorniRange = Enumerable.Range(0, totalDays)
                .Select(offset => startDate.AddDays(offset))
                .ToList();

            if (nascondiWeekend)
            {
                giorniRange = giorniRange
                    .Where(d => d.DayOfWeek != DayOfWeek.Saturday && d.DayOfWeek != DayOfWeek.Sunday)
                    .ToList();
            }

            var giorni = giorniRange
                .Select(day =>
                {
                    var viaggiGiorno = viaggi.Where(v => v.DataConsegna.Date == day).ToList();
                    return new GiornoKanbanDto
                    {
                        Data = day,
                        Viaggi = viaggiGiorno.Select(v =>
                        {
                            var pesoTotale = v.Righe.Sum(r => r.PesoTotaleKgSnapshot);
                            var mezzoDescrizione = v.MezzoTrasporto?.Descrizione
                                ?? (v.MezzoTrasportoEsterno != null
                                    ? $"{v.MezzoTrasportoEsterno.NomeVettore} - {v.MezzoTrasportoEsterno.TipoMezzo}"
                                    : "N/D");
                            var portataMaxKg = v.MezzoTrasporto?.PortataMaxKg
                                ?? (v.MezzoTrasportoEsterno != null ? (decimal)v.MezzoTrasportoEsterno.PortataMax : 0);
                            return new ViaggioKanbanDto
                            {
                                Id = v.Id,
                                DataConsegna = v.DataConsegna,
                                TipoTrasportoId = v.TipoTrasportoId,
                                TipoTrasporto = v.TipoTrasporto?.Descrizione ?? "N/D",
                                MezzoTrasportoId = v.MezzoTrasportoId,
                                MezzoTrasportoEsternoId = v.MezzoTrasportoEsternoId,
                                Mezzo = mezzoDescrizione,
                                PortataMaxKg = portataMaxKg,
                                PesoTotaleKg = pesoTotale,
                                OraPartenza = v.OraPartenza,
                                OraArrivoEffettiva = v.OraArrivo ?? v.OraPartenza.Add(TimeSpan.FromMinutes(v.DurataStimataMinuti)),
                                Stato = v.Stato,
                                Note = v.Note,
                                AutistaId = v.AutistaId,
                                Autista = v.Autista != null ? $"{v.Autista.Cognome} {v.Autista.Nome}" : null,
                                CostoTrasporto = v.CostoTrasporto,
                                PrezzoVendita = v.PrezzoVendita,
                                TempoPausa = v.TempoPausa,
                                TempoScarico = v.TempoScarico,
                                Gru = v.Gru,
                                Trasbordo = v.Trasbordo,
                                Destinazioni = v.Righe
                                    .Select(r => destinazionePerRigaOrdine.TryGetValue(r.OrdineRigaId, out var loc) ? loc : null)
                                    .Where(loc => !string.IsNullOrEmpty(loc))
                                    .Distinct()
                                    .ToList()!,
                                Righe = v.Righe.Select(r => new RigaAssegnataDto
                                {
                                    ViaggioRigaId = r.Id,
                                    OrdineRigaId = r.OrdineRigaId,
                                    OrdineCompleto = r.OrdineRiga != null ? $"{r.OrdineRiga.TipoOrdine}{r.OrdineRiga.AnnoOrdine}/{r.OrdineRiga.SerieOrdine}/{r.OrdineRiga.NumeroOrdine:D6}" : "N/D",
                                    RigaOrdine = r.OrdineRiga?.RigaOrdine ?? 0,
                                    DataConsegna = r.OrdineRiga?.DataConsegna ?? DateTime.MinValue,
                                    CodiceArticolo = r.OrdineRiga?.CodiceArticolo ?? "N/D",
                                    DescrizioneArticolo = r.OrdineRiga?.DescrizioneArticolo,
                                    QuantitaAssegnata = r.QuantitaAssegnata,
                                    PesoTotaleKg = r.PesoTotaleKgSnapshot,
                                    NoteRiga = r.NoteRiga
                                }).ToList()
                            };
                        }).ToList()
                    };
                })
                .ToList();

            var model = new ConsegneKanbanViewModel
            {
                DataInizio = startDate,
                DataFine = endDate,
                DurataDefaultMinuti = durataDefault,
                NascondiWeekend = nascondiWeekend,
                Giorni = giorni,
                RigheDaPianificare = righeDaPianificare,
                TipiTrasporto = await _context.TipiTrasporto.AsNoTracking()
                    .Where(t => t.Attivo)
                    .OrderBy(t => t.Descrizione)
                    .Select(t => new LookupItemDto { Id = t.Id, Text = t.Descrizione })
                    .ToListAsync(),
                Mezzi = await _context.MezziTrasporto.AsNoTracking()
                    .Where(m => m.Attivo)
                    .OrderBy(m => m.Descrizione)
                    .Select(m => new LookupItemDto { Id = m.Id, Text = $"{m.Descrizione} ({m.PortataMaxKg:N0} Kg)" })
                    .ToListAsync(),
                MezziEsterni = await _context.MezziTrasportoEsterni.AsNoTracking()
                    .OrderBy(m => m.NomeVettore)
                    .Select(m => new LookupItemDto { Id = m.Id, Text = $"{m.NomeVettore} - {m.TipoMezzo} ({m.Comune})" })
                    .ToListAsync(),
                Autisti = await _context.Autisti.AsNoTracking()
                    .Where(a => a.Attivo)
                    .OrderBy(a => a.Cognome).ThenBy(a => a.Nome)
                    .Select(a => new LookupItemDto { Id = a.Id, Text = $"{a.Cognome} {a.Nome}" })
                    .ToListAsync(),
                MezzoAutistaDefaultMap = await _context.MezziTrasporto.AsNoTracking()
                    .Where(m => m.Attivo && m.AutistaDefaultId.HasValue)
                    .ToDictionaryAsync(m => m.Id, m => m.AutistaDefaultId),
                MezzoEsternoInfoMap = await _context.MezziTrasportoEsterni.AsNoTracking()
                    .ToDictionaryAsync(m => m.Id, m => new MezzoEsternoInfoDto
                    {
                        Costo = (decimal)m.Costo,
                        Gru = m.Gru,
                        Trasbordo = m.Trasbordo,
                        Regione = m.Regione,
                        Provincia = m.Provincia,
                        Comune = m.Comune,
                        NomeVettore = m.NomeVettore
                    })
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission("ConsegneKanban", "Create")]
        public async Task<IActionResult> CreaViaggio(DateTime dataConsegna, int tipoTrasportoId, int? mezzoTrasportoId, int? mezzoTrasportoEsternoId, TimeSpan oraPartenza, TimeSpan? oraArrivo, string? note, int? autistaId, decimal? costoTrasporto, decimal? prezzoVendita, int? tempoPausa, int? tempoScarico, bool? gru, bool? trasbordo, bool nascondiWeekend = false)
        {
            if (dataConsegna.Date < DateTime.Today)
            {
                TempData["ErrorMessage"] = "Non è possibile creare viaggi in date passate.";
                return RedirectToAction(nameof(Index), new { nascondiWeekend });
            }

            if (!mezzoTrasportoId.HasValue && !mezzoTrasportoEsternoId.HasValue)
            {
                TempData["ErrorMessage"] = "Selezionare un mezzo interno o esterno.";
                return RedirectToAction(nameof(Index), new { nascondiWeekend });
            }

            var durataDefault = await GetDurataDefaultMinutiAsync();
            var oraArrivoEffettiva = oraArrivo ?? oraPartenza.Add(TimeSpan.FromMinutes(durataDefault));

            var erroreValidazione = await ValidaViaggioAsync(
                dataConsegna.Date, mezzoTrasportoId, mezzoTrasportoEsternoId,
                oraPartenza, oraArrivoEffettiva, autistaId,
                gru ?? false, trasbordo ?? false, null);
            if (erroreValidazione != null)
            {
                TempData["ErrorMessage"] = erroreValidazione;
                return RedirectToAction(nameof(Index), new { nascondiWeekend });
            }

            var userName = User.Identity?.Name;
            var viaggio = new ViaggioConsegna
            {
                DataConsegna = dataConsegna.Date,
                TipoTrasportoId = tipoTrasportoId,
                MezzoTrasportoId = mezzoTrasportoId,
                MezzoTrasportoEsternoId = mezzoTrasportoEsternoId,
                OraPartenza = oraPartenza,
                OraArrivo = oraArrivo,
                DurataStimataMinuti = durataDefault,
                Stato = "Pianificato",
                Note = note,
                AutistaId = autistaId,
                CostoTrasporto = costoTrasporto ?? 0,
                PrezzoVendita = prezzoVendita ?? 0,
                TempoPausa = tempoPausa ?? 0,
                TempoScarico = tempoScarico ?? 0,
                Gru = gru ?? false,
                Trasbordo = trasbordo ?? false,
                CreatoDa = userName
            };

            _context.ViaggiConsegna.Add(viaggio);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Viaggio creato con successo.";
            return RedirectToAction(nameof(Index), new { nascondiWeekend });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission("ConsegneKanban", "Create")]
        public async Task<IActionResult> AssegnaRiga(int viaggioId, int ordineRigaId, decimal quantitaAssegnata, string? noteRiga, bool nascondiWeekend = false)
        {
            var viaggio = await _context.ViaggiConsegna
                .Include(v => v.MezzoTrasporto)
                .Include(v => v.MezzoTrasportoEsterno)
                .Include(v => v.Righe)
                .FirstOrDefaultAsync(v => v.Id == viaggioId);

            if (viaggio == null)
            {
                TempData["ErrorMessage"] = "Viaggio non trovato.";
                return RedirectToAction(nameof(Index), new { nascondiWeekend });
            }

            if (viaggio.DataConsegna.Date < DateTime.Today)
            {
                TempData["ErrorMessage"] = "Non è possibile assegnare righe a viaggi in date passate.";
                return RedirectToAction(nameof(Index), new { nascondiWeekend });
            }

            var rigaOrdine = await _context.OrdiniRighe.FirstOrDefaultAsync(r => r.Id == ordineRigaId);
            if (rigaOrdine == null)
            {
                TempData["ErrorMessage"] = "Riga ordine non trovata.";
                return RedirectToAction(nameof(Index), new { nascondiWeekend });
            }

            if (quantitaAssegnata <= 0)
            {
                TempData["ErrorMessage"] = "La quantità assegnata deve essere maggiore di zero.";
                return RedirectToAction(nameof(Index), new { nascondiWeekend });
            }

            var qtaResidua = await CalcolaQuantitaResiduaAsync(ordineRigaId, null);
            if (quantitaAssegnata > qtaResidua)
            {
                TempData["ErrorMessage"] = $"Quantità richiesta ({quantitaAssegnata:N2}) superiore alla quantità residua disponibile ({qtaResidua:N2}).";
                return RedirectToAction(nameof(Index), new { nascondiWeekend });
            }

            var pesoUnitario = await _context.AnagraficaArticoli
                .Where(a => a.CodiceArticolo == rigaOrdine.CodiceArticolo)
                .Select(a => a.PesoUnitarioKg)
                .FirstOrDefaultAsync();

            var pesoReale = (pesoUnitario.HasValue && pesoUnitario.Value > 0) ? pesoUnitario.Value : 0m;
            bool pesoMancante = pesoReale == 0m;

            var pesoTotaleNuovaAssegnazione = Math.Round(quantitaAssegnata * pesoReale, 3);
            var pesoGiaCaricato = viaggio.Righe.Sum(r => r.PesoTotaleKgSnapshot);
            var nuovoTotale = pesoGiaCaricato + pesoTotaleNuovaAssegnazione;
            var portataMax = viaggio.MezzoTrasporto?.PortataMaxKg
                ?? (viaggio.MezzoTrasportoEsterno != null ? (decimal)viaggio.MezzoTrasportoEsterno.PortataMax : 0);

            if (!pesoMancante && portataMax > 0 && nuovoTotale > portataMax)
            {
                TempData["ErrorMessage"] = $"Carico eccedente: peso totale {nuovoTotale:N3} Kg oltre la portata {portataMax:N3} Kg.";
                return RedirectToAction(nameof(Index), new { nascondiWeekend });
            }

            var esisteGiaSuViaggio = await _context.ViaggioConsegnaRighe
                .AnyAsync(r => r.ViaggioConsegnaId == viaggioId && r.OrdineRigaId == ordineRigaId);
            if (esisteGiaSuViaggio)
            {
                TempData["ErrorMessage"] = "La riga è già assegnata a questo viaggio.";
                return RedirectToAction(nameof(Index), new { nascondiWeekend });
            }

            _context.ViaggioConsegnaRighe.Add(new ViaggioConsegnaRiga
            {
                ViaggioConsegnaId = viaggioId,
                OrdineRigaId = ordineRigaId,
                QuantitaAssegnata = quantitaAssegnata,
                PesoUnitarioKgSnapshot = pesoReale,
                PesoTotaleKgSnapshot = pesoTotaleNuovaAssegnazione,
                NoteRiga = noteRiga
            });

            await _context.SaveChangesAsync();

            if (pesoMancante)
                TempData["ErrorMessage"] = $"⚠️ Riga assegnata, ma l'articolo {rigaOrdine.CodiceArticolo} non ha peso unitario. Il controllo capacità è stato saltato.";
            else
                TempData["SuccessMessage"] = "Riga assegnata correttamente al viaggio.";

            return RedirectToAction(nameof(Index), new { nascondiWeekend });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission("ConsegneKanban", "Edit")]
        public async Task<IActionResult> GestisciDropRiga(int viaggioDestId, int ordineRigaId, int? viaggioRigaId, decimal quantitaAssegnata, string? noteRiga, bool nascondiWeekend = false)
        {
            var viaggioDest = await _context.ViaggiConsegna
                .Include(v => v.MezzoTrasporto)
                .Include(v => v.MezzoTrasportoEsterno)
                .Include(v => v.Righe)
                .FirstOrDefaultAsync(v => v.Id == viaggioDestId);

            if (viaggioDest == null)
            {
                TempData["ErrorMessage"] = "Viaggio di destinazione non trovato.";
                return RedirectToAction(nameof(Index), new { nascondiWeekend });
            }

            if (viaggioDest.DataConsegna.Date < DateTime.Today)
            {
                TempData["ErrorMessage"] = "Non è possibile assegnare righe a viaggi in date passate.";
                return RedirectToAction(nameof(Index), new { nascondiWeekend });
            }

            if (quantitaAssegnata <= 0)
            {
                TempData["ErrorMessage"] = "La quantità deve essere maggiore di zero.";
                return RedirectToAction(nameof(Index), new { nascondiWeekend });
            }

            var rigaOrdine = await _context.OrdiniRighe.FirstOrDefaultAsync(r => r.Id == ordineRigaId);
            if (rigaOrdine == null)
            {
                TempData["ErrorMessage"] = "Riga ordine non trovata.";
                return RedirectToAction(nameof(Index), new { nascondiWeekend });
            }

            var pesoUnitario = await _context.AnagraficaArticoli
                .Where(a => a.CodiceArticolo == rigaOrdine.CodiceArticolo)
                .Select(a => a.PesoUnitarioKg)
                .FirstOrDefaultAsync();

            var pesoReale = (pesoUnitario.HasValue && pesoUnitario.Value > 0) ? pesoUnitario.Value : 0m;
            bool pesoMancante = pesoReale == 0m;

            var pesoTotaleNuovo = Math.Round(quantitaAssegnata * pesoReale, 3);
            string warningPeso = pesoMancante
                ? $"⚠️ Articolo {rigaOrdine.CodiceArticolo} senza peso unitario: il controllo capacità è stato saltato."
                : "";

            if (viaggioRigaId.HasValue)
            {
                var rigaViaggio = await _context.ViaggioConsegnaRighe
                    .Include(x => x.ViaggioConsegna)
                    .FirstOrDefaultAsync(x => x.Id == viaggioRigaId.Value);

                if (rigaViaggio == null)
                {
                    TempData["ErrorMessage"] = "Riga viaggio non trovata.";
                    return RedirectToAction(nameof(Index), new { nascondiWeekend });
                }

                var qtaResiduaMove = await CalcolaQuantitaResiduaAsync(ordineRigaId, viaggioRigaId.Value);
                if (quantitaAssegnata > qtaResiduaMove)
                {
                    TempData["ErrorMessage"] = $"Quantità richiesta ({quantitaAssegnata:N2}) superiore alla quantità residua disponibile ({qtaResiduaMove:N2}).";
                    return RedirectToAction(nameof(Index), new { nascondiWeekend });
                }

                var origineId = rigaViaggio.ViaggioConsegnaId;
                var destinazioneId = viaggioDestId;

                var duplicato = await _context.ViaggioConsegnaRighe
                    .AnyAsync(x => x.ViaggioConsegnaId == destinazioneId
                        && x.OrdineRigaId == ordineRigaId
                        && x.Id != rigaViaggio.Id);
                if (duplicato)
                {
                    TempData["ErrorMessage"] = "Nel viaggio di destinazione esiste già questa riga ordine.";
                    return RedirectToAction(nameof(Index), new { nascondiWeekend });
                }

                var pesoDestCorrente = await _context.ViaggioConsegnaRighe
                    .Where(x => x.ViaggioConsegnaId == destinazioneId && x.Id != rigaViaggio.Id)
                    .SumAsync(x => x.PesoTotaleKgSnapshot);
                var nuovoPesoDest = pesoDestCorrente + pesoTotaleNuovo;
                var portataDest = viaggioDest.MezzoTrasporto?.PortataMaxKg
                    ?? (viaggioDest.MezzoTrasportoEsterno != null ? (decimal)viaggioDest.MezzoTrasportoEsterno.PortataMax : 0);

                if (!pesoMancante && portataDest > 0 && nuovoPesoDest > portataDest)
                {
                    TempData["ErrorMessage"] = $"Carico eccedente sul viaggio destinazione: {nuovoPesoDest:N3} / {portataDest:N3} Kg.";
                    return RedirectToAction(nameof(Index), new { nascondiWeekend });
                }

                rigaViaggio.ViaggioConsegnaId = destinazioneId;
                rigaViaggio.QuantitaAssegnata = quantitaAssegnata;
                rigaViaggio.PesoUnitarioKgSnapshot = pesoReale;
                rigaViaggio.PesoTotaleKgSnapshot = pesoTotaleNuovo;
                rigaViaggio.NoteRiga = noteRiga;

                await _context.SaveChangesAsync();

                var msgOk = origineId == destinazioneId
                    ? "Riga viaggio aggiornata con successo."
                    : "Riga viaggio spostata con successo.";

                if (pesoMancante)
                    TempData["ErrorMessage"] = warningPeso;
                else
                    TempData["SuccessMessage"] = msgOk;

                return RedirectToAction(nameof(Index), new { nascondiWeekend });
            }
            else
            {
                var qtaResiduaNew = await CalcolaQuantitaResiduaAsync(ordineRigaId, null);
                if (quantitaAssegnata > qtaResiduaNew)
                {
                    TempData["ErrorMessage"] = $"Quantità richiesta ({quantitaAssegnata:N2}) superiore alla quantità residua disponibile ({qtaResiduaNew:N2}).";
                    return RedirectToAction(nameof(Index), new { nascondiWeekend });
                }

                var esisteGiaSuViaggio = await _context.ViaggioConsegnaRighe
                    .AnyAsync(r => r.ViaggioConsegnaId == viaggioDestId && r.OrdineRigaId == ordineRigaId);
                if (esisteGiaSuViaggio)
                {
                    TempData["ErrorMessage"] = "La riga è già assegnata al viaggio di destinazione.";
                    return RedirectToAction(nameof(Index), new { nascondiWeekend });
                }

                var pesoGiaCaricato = viaggioDest.Righe.Sum(r => r.PesoTotaleKgSnapshot);
                var nuovoTotale = pesoGiaCaricato + pesoTotaleNuovo;
                var portataMax = viaggioDest.MezzoTrasporto?.PortataMaxKg
                    ?? (viaggioDest.MezzoTrasportoEsterno != null ? (decimal)viaggioDest.MezzoTrasportoEsterno.PortataMax : 0);

                if (!pesoMancante && portataMax > 0 && nuovoTotale > portataMax)
                {
                    TempData["ErrorMessage"] = $"Carico eccedente: peso totale {nuovoTotale:N3} Kg oltre la portata {portataMax:N3} Kg.";
                    return RedirectToAction(nameof(Index), new { nascondiWeekend });
                }

                _context.ViaggioConsegnaRighe.Add(new ViaggioConsegnaRiga
                {
                    ViaggioConsegnaId = viaggioDestId,
                    OrdineRigaId = ordineRigaId,
                    QuantitaAssegnata = quantitaAssegnata,
                    PesoUnitarioKgSnapshot = pesoReale,
                    PesoTotaleKgSnapshot = pesoTotaleNuovo,
                    NoteRiga = noteRiga
                });

                await _context.SaveChangesAsync();

                if (pesoMancante)
                    TempData["ErrorMessage"] = warningPeso;
                else
                    TempData["SuccessMessage"] = "Riga assegnata correttamente al viaggio.";

                return RedirectToAction(nameof(Index), new { nascondiWeekend });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission("ConsegneKanban", "Create")]
        public async Task<IActionResult> AssegnaRigaAGiorno(
            DateTime dataConsegna,
            int? tipoTrasportoId,
            int? mezzoTrasportoId,
            int? mezzoTrasportoEsternoId,
            TimeSpan? oraPartenza,
            TimeSpan? oraArrivo,
            string? noteViaggio,
            int? autistaId,
            decimal? costoTrasporto,
            decimal? prezzoVendita,
            int? tempoPausa,
            int? tempoScarico,
            bool? gru,
            bool? trasbordo,
            int ordineRigaId,
            int? viaggioRigaId,
            decimal quantitaAssegnata,
            string? noteRiga,
            bool nascondiWeekend = false)
        {
            if (dataConsegna.Date < DateTime.Today)
            {
                TempData["ErrorMessage"] = "Non è possibile assegnare righe a date passate.";
                return RedirectToAction(nameof(Index), new { nascondiWeekend });
            }

            if (!tipoTrasportoId.HasValue || !oraPartenza.HasValue)
            {
                TempData["ErrorMessage"] = "Tipo trasporto e ora partenza sono obbligatori per creare un nuovo viaggio.";
                return RedirectToAction(nameof(Index), new { nascondiWeekend });
            }

            if (!mezzoTrasportoId.HasValue && !mezzoTrasportoEsternoId.HasValue)
            {
                TempData["ErrorMessage"] = "Selezionare un mezzo interno o esterno.";
                return RedirectToAction(nameof(Index), new { nascondiWeekend });
            }

            if (quantitaAssegnata <= 0)
            {
                TempData["ErrorMessage"] = "La quantità deve essere maggiore di zero.";
                return RedirectToAction(nameof(Index), new { nascondiWeekend });
            }

            var rigaOrdine = await _context.OrdiniRighe.FirstOrDefaultAsync(r => r.Id == ordineRigaId);
            if (rigaOrdine == null)
            {
                TempData["ErrorMessage"] = "Riga ordine non trovata.";
                return RedirectToAction(nameof(Index), new { nascondiWeekend });
            }

            var durataDefault = await GetDurataDefaultMinutiAsync();
            var oraArrivoEffettiva = oraArrivo ?? oraPartenza.Value.Add(TimeSpan.FromMinutes(durataDefault));

            var erroreValidazione = await ValidaViaggioAsync(
                dataConsegna.Date, mezzoTrasportoId, mezzoTrasportoEsternoId,
                oraPartenza.Value, oraArrivoEffettiva, autistaId,
                gru ?? false, trasbordo ?? false, null);
            if (erroreValidazione != null)
            {
                TempData["ErrorMessage"] = erroreValidazione;
                return RedirectToAction(nameof(Index), new { nascondiWeekend });
            }

            var qtaResidua = await CalcolaQuantitaResiduaAsync(ordineRigaId, viaggioRigaId);
            if (quantitaAssegnata > qtaResidua)
            {
                TempData["ErrorMessage"] = $"Quantità richiesta ({quantitaAssegnata:N2}) superiore alla quantità residua disponibile ({qtaResidua:N2}).";
                return RedirectToAction(nameof(Index), new { nascondiWeekend });
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var userName = User.Identity?.Name;
                var viaggio = new ViaggioConsegna
                {
                    DataConsegna = dataConsegna.Date,
                    TipoTrasportoId = tipoTrasportoId.Value,
                    MezzoTrasportoId = mezzoTrasportoId,
                    MezzoTrasportoEsternoId = mezzoTrasportoEsternoId,
                    OraPartenza = oraPartenza.Value,
                    OraArrivo = oraArrivo,
                    DurataStimataMinuti = durataDefault,
                    Stato = "Pianificato",
                    Note = noteViaggio,
                    AutistaId = autistaId,
                    CostoTrasporto = costoTrasporto ?? 0,
                    PrezzoVendita = prezzoVendita ?? 0,
                    TempoPausa = tempoPausa ?? 0,
                    TempoScarico = tempoScarico ?? 0,
                    Gru = gru ?? false,
                    Trasbordo = trasbordo ?? false,
                    CreatoDa = userName
                };

                _context.ViaggiConsegna.Add(viaggio);
                await _context.SaveChangesAsync();

                var pesoUnitario = await _context.AnagraficaArticoli
                    .Where(a => a.CodiceArticolo == rigaOrdine.CodiceArticolo)
                    .Select(a => a.PesoUnitarioKg)
                    .FirstOrDefaultAsync();

                var pesoReale = (pesoUnitario.HasValue && pesoUnitario.Value > 0) ? pesoUnitario.Value : 0m;
                bool pesoMancante = pesoReale == 0m;

                decimal qtaDaAssegnare = quantitaAssegnata;

                if (viaggioRigaId.HasValue)
                {
                    var rigaViaggio = await _context.ViaggioConsegnaRighe.FirstOrDefaultAsync(x => x.Id == viaggioRigaId.Value);
                    if (rigaViaggio != null)
                    {
                        rigaViaggio.ViaggioConsegnaId = viaggio.Id;
                        rigaViaggio.QuantitaAssegnata = qtaDaAssegnare;
                        rigaViaggio.PesoUnitarioKgSnapshot = pesoReale;
                        rigaViaggio.PesoTotaleKgSnapshot = Math.Round(qtaDaAssegnare * pesoReale, 3);
                        rigaViaggio.NoteRiga = noteRiga;
                    }
                }
                else
                {
                    _context.ViaggioConsegnaRighe.Add(new ViaggioConsegnaRiga
                    {
                        ViaggioConsegnaId = viaggio.Id,
                        OrdineRigaId = ordineRigaId,
                        QuantitaAssegnata = qtaDaAssegnare,
                        PesoUnitarioKgSnapshot = pesoReale,
                        PesoTotaleKgSnapshot = Math.Round(qtaDaAssegnare * pesoReale, 3),
                        NoteRiga = noteRiga
                    });
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                if (pesoMancante)
                    TempData["ErrorMessage"] = $"⚠️ Viaggio creato e riga assegnata, ma l'articolo {rigaOrdine.CodiceArticolo} non ha peso unitario.";
                else
                    TempData["SuccessMessage"] = "Viaggio creato e riga assegnata con successo.";
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Errore in AssegnaRigaAGiorno");
                TempData["ErrorMessage"] = "Errore durante la creazione del viaggio e assegnazione della riga.";
            }

            return RedirectToAction(nameof(Index), new { nascondiWeekend });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission("ConsegneKanban", "Edit")]
        public async Task<IActionResult> LiberaRiga(int viaggioRigaId, bool nascondiWeekend = false)
        {
            var rigaViaggio = await _context.ViaggioConsegnaRighe
                .Include(x => x.ViaggioConsegna)
                    .ThenInclude(v => v!.Righe)
                .FirstOrDefaultAsync(x => x.Id == viaggioRigaId);

            if (rigaViaggio == null)
            {
                TempData["ErrorMessage"] = "Riga viaggio non trovata.";
                return RedirectToAction(nameof(Index), new { nascondiWeekend });
            }

            var viaggio = rigaViaggio.ViaggioConsegna!;
            var righeRimaste = viaggio.Righe.Count;

            _context.ViaggioConsegnaRighe.Remove(rigaViaggio);

            if (righeRimaste <= 1)
            {
                _context.ViaggiConsegna.Remove(viaggio);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Riga liberata e viaggio eliminato (era l'unica riga).";
            }
            else
            {
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Riga liberata dal viaggio.";
            }

            return RedirectToAction(nameof(Index), new { nascondiWeekend });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission("ConsegneKanban", "Edit")]
        public async Task<IActionResult> ModificaViaggio(
            int viaggioId,
            DateTime dataConsegna,
            int tipoTrasportoId,
            int? mezzoTrasportoId,
            int? mezzoTrasportoEsternoId,
            TimeSpan oraPartenza,
            TimeSpan? oraArrivo,
            string? note,
            int? autistaId,
            decimal? costoTrasporto,
            decimal? prezzoVendita,
            int? tempoPausa,
            int? tempoScarico,
            bool? gru,
            bool? trasbordo,
            bool nascondiWeekend = false)
        {
            var viaggio = await _context.ViaggiConsegna.FirstOrDefaultAsync(v => v.Id == viaggioId);
            if (viaggio == null)
            {
                TempData["ErrorMessage"] = "Viaggio non trovato.";
                return RedirectToAction(nameof(Index), new { nascondiWeekend });
            }

            if (dataConsegna.Date < DateTime.Today)
            {
                TempData["ErrorMessage"] = "Non è possibile spostare un viaggio in una data passata.";
                return RedirectToAction(nameof(Index), new { nascondiWeekend });
            }

            if (!mezzoTrasportoId.HasValue && !mezzoTrasportoEsternoId.HasValue)
            {
                TempData["ErrorMessage"] = "Selezionare un mezzo interno o esterno.";
                return RedirectToAction(nameof(Index), new { nascondiWeekend });
            }

            var durataDefault = await GetDurataDefaultMinutiAsync();
            var oraArrivoEffettiva = oraArrivo ?? oraPartenza.Add(TimeSpan.FromMinutes(durataDefault));

            var erroreValidazione = await ValidaViaggioAsync(
                dataConsegna.Date, mezzoTrasportoId, mezzoTrasportoEsternoId,
                oraPartenza, oraArrivoEffettiva, autistaId,
                gru ?? false, trasbordo ?? false, viaggioId);
            if (erroreValidazione != null)
            {
                TempData["ErrorMessage"] = erroreValidazione;
                return RedirectToAction(nameof(Index), new { nascondiWeekend });
            }

            viaggio.DataConsegna = dataConsegna.Date;
            viaggio.TipoTrasportoId = tipoTrasportoId;
            viaggio.MezzoTrasportoId = mezzoTrasportoId;
            viaggio.MezzoTrasportoEsternoId = mezzoTrasportoEsternoId;
            viaggio.OraPartenza = oraPartenza;
            viaggio.OraArrivo = oraArrivo;
            viaggio.DurataStimataMinuti = durataDefault;
            viaggio.Note = note;
            viaggio.AutistaId = autistaId;
            viaggio.CostoTrasporto = costoTrasporto ?? 0;
            viaggio.PrezzoVendita = prezzoVendita ?? 0;
            viaggio.TempoPausa = tempoPausa ?? 0;
            viaggio.TempoScarico = tempoScarico ?? 0;
            viaggio.Gru = gru ?? false;
            viaggio.Trasbordo = trasbordo ?? false;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Viaggio modificato con successo.";
            return RedirectToAction(nameof(Index), new { nascondiWeekend });
        }

        private async Task<int> GetDurataDefaultMinutiAsync()
        {
            var value = await _context.Opzioni
                .Where(o => o.NomeOpzione == "Consegne.DurataDefaultMinuti")
                .Select(o => o.ValoreOpzione)
                .FirstOrDefaultAsync();

            if (int.TryParse(value, out var durata) && durata > 0)
            {
                return durata;
            }

            return 240;
        }

        [HttpGet]
        public async Task<IActionResult> GetDisponibilitaArticolo(string codiceArticolo)
        {
            if (string.IsNullOrEmpty(codiceArticolo))
                return Json(new { success = false, message = "Codice articolo mancante." });

            var oggi = DateTime.Today;

            var progressivo = await _context.ProgressiviArticoli
                .AsNoTracking()
                .Where(p => p.CodiceArticolo == codiceArticolo && p.CodiceMagazzino == 1)
                .FirstOrDefaultAsync();

            var impegnato = await _context.OrdiniRighe
                .AsNoTracking()
                .Where(r => r.TipoOrdine == "R"
                    && r.CodiceArticolo == codiceArticolo
                    && r.DataConsegna <= oggi)
                .SumAsync(r => (decimal?)(r.Quantita - r.QuantitaEvasa)) ?? 0;

            var articolo = await _context.AnagraficaArticoli
                .AsNoTracking()
                .Where(a => a.CodiceArticolo == codiceArticolo)
                .Select(a => new { a.Descrizione, a.UnitaMisura })
                .FirstOrDefaultAsync();

            var esistenza = progressivo?.Esistenza ?? 0;
            var disponibile = esistenza - impegnato;

            return Json(new
            {
                success = true,
                codiceArticolo,
                descrizione = articolo?.Descrizione ?? "N/D",
                unitaMisura = articolo?.UnitaMisura ?? "",
                esistenza,
                impegnato,
                disponibile,
                pronto = progressivo?.Pronto ?? 0
            });
        }

        // ==================== REPORT ====================

        [HttpGet]
        [RequirePermission("ConsegneKanban", "View")]
        public async Task<IActionResult> StampaFoglioViaggio(int id)
        {
            var viaggio = await _context.ViaggiConsegna
                .AsNoTracking()
                .Include(v => v.TipoTrasporto)
                .Include(v => v.MezzoTrasporto)
                .Include(v => v.MezzoTrasportoEsterno)
                .Include(v => v.Autista)
                .Include(v => v.Righe).ThenInclude(r => r.OrdineRiga)
                .FirstOrDefaultAsync(v => v.Id == id);

            if (viaggio == null) return NotFound();

            var righeInfo = await GetRigheConClienteDestinazioneAsync(viaggio.Righe);

            var model = new FoglioViaggioViewModel
            {
                ViaggioId = viaggio.Id,
                DataConsegna = viaggio.DataConsegna,
                TipoTrasporto = viaggio.TipoTrasporto?.Descrizione ?? "N/D",
                Mezzo = viaggio.MezzoTrasporto?.Descrizione
                    ?? (viaggio.MezzoTrasportoEsterno != null ? $"{viaggio.MezzoTrasportoEsterno.NomeVettore} - {viaggio.MezzoTrasportoEsterno.TipoMezzo}" : "N/D"),
                Targa = viaggio.MezzoTrasporto?.Targa,
                PortataMaxKg = viaggio.MezzoTrasporto?.PortataMaxKg
                    ?? (viaggio.MezzoTrasportoEsterno != null ? (decimal)viaggio.MezzoTrasportoEsterno.PortataMax : 0),
                Autista = viaggio.Autista != null ? $"{viaggio.Autista.Cognome} {viaggio.Autista.Nome}" : null,
                TelefonoAutista = viaggio.Autista?.Telefono,
                OraPartenza = viaggio.OraPartenza,
                OraArrivoStimata = viaggio.OraArrivoEffettiva,
                Gru = viaggio.Gru ?? false,
                Trasbordo = viaggio.Trasbordo ?? false,
                TempoPausa = viaggio.TempoPausa ?? 0,
                TempoScarico = viaggio.TempoScarico ?? 0,
                Note = viaggio.Note,
                CostoTrasporto = viaggio.CostoTrasporto ?? 0,
                PrezzoVendita = viaggio.PrezzoVendita ?? 0,
                PesoTotaleKg = viaggio.Righe.Sum(r => r.PesoTotaleKgSnapshot),
                Righe = righeInfo.Select((r, idx) => new FoglioViaggioRigaViewModel
                {
                    Progressivo = idx + 1,
                    OrdineCompleto = r.OrdineCompleto,
                    RigaOrdine = r.RigaOrdine,
                    Cliente = r.Cliente,
                    Destinazione = r.Destinazione,
                    CodiceArticolo = r.CodiceArticolo,
                    DescrizioneArticolo = r.DescrizioneArticolo,
                    QuantitaAssegnata = r.QuantitaAssegnata,
                    UnitaMisura = r.UnitaMisura,
                    PesoTotaleKg = r.PesoTotaleKg,
                    NoteRiga = r.NoteRiga
                }).ToList()
            };

            return View("StampaFoglioViaggio", model);
        }

        [HttpGet]
        [RequirePermission("ConsegneKanban", "View")]
        public async Task<IActionResult> StampaDistintaCarico(DateTime data)
        {
            var viaggi = await _context.ViaggiConsegna
                .AsNoTracking()
                .Include(v => v.MezzoTrasporto)
                .Include(v => v.MezzoTrasportoEsterno)
                .Include(v => v.Autista)
                .Include(v => v.Righe).ThenInclude(r => r.OrdineRiga)
                .Where(v => v.DataConsegna == data.Date && v.Stato != "Annullato")
                .OrderBy(v => v.OraPartenza)
                .ToListAsync();

            if (!viaggi.Any()) return NotFound();

            var viaggiVm = new List<DistintaCaricoViaggioViewModel>();
            int progGlobale = 0;

            foreach (var v in viaggi)
            {
                var righeInfo = await GetRigheConClienteDestinazioneAsync(v.Righe);
                viaggiVm.Add(new DistintaCaricoViaggioViewModel
                {
                    ViaggioId = v.Id,
                    Mezzo = v.MezzoTrasporto?.Descrizione
                        ?? (v.MezzoTrasportoEsterno != null ? $"{v.MezzoTrasportoEsterno.NomeVettore} - {v.MezzoTrasportoEsterno.TipoMezzo}" : "N/D"),
                    Targa = v.MezzoTrasporto?.Targa,
                    Autista = v.Autista != null ? $"{v.Autista.Cognome} {v.Autista.Nome}" : null,
                    OraPartenza = v.OraPartenza,
                    PesoTotaleKg = v.Righe.Sum(r => r.PesoTotaleKgSnapshot),
                    PortataMaxKg = v.MezzoTrasporto?.PortataMaxKg
                        ?? (v.MezzoTrasportoEsterno != null ? (decimal)v.MezzoTrasportoEsterno.PortataMax : 0),
                    Righe = righeInfo.Select(r => new DistintaCaricoRigaViewModel
                    {
                        Progressivo = ++progGlobale,
                        CodiceArticolo = r.CodiceArticolo,
                        DescrizioneArticolo = r.DescrizioneArticolo,
                        QuantitaAssegnata = r.QuantitaAssegnata,
                        UnitaMisura = r.UnitaMisura,
                        PesoUnitarioKg = r.PesoUnitarioKg,
                        PesoTotaleKg = r.PesoTotaleKg,
                        Cliente = r.Cliente,
                        OrdineCompleto = r.OrdineCompleto,
                        RigaOrdine = r.RigaOrdine
                    }).ToList()
                });
            }

            var model = new DistintaCaricoGiornalieraViewModel
            {
                DataConsegna = data.Date,
                PesoTotaleGiornata = viaggiVm.Sum(v => v.PesoTotaleKg),
                TotaleArticoli = progGlobale,
                Viaggi = viaggiVm
            };

            return View("StampaDistintaCarico", model);
        }

        [HttpGet]
        [RequirePermission("ConsegneKanban", "View")]
        public async Task<IActionResult> ExportPlanningGiornaliero(DateTime data)
        {
            var viaggi = await _context.ViaggiConsegna
                .AsNoTracking()
                .Include(v => v.TipoTrasporto)
                .Include(v => v.MezzoTrasporto)
                .Include(v => v.MezzoTrasportoEsterno)
                .Include(v => v.Autista)
                .Include(v => v.Righe).ThenInclude(r => r.OrdineRiga)
                .Where(v => v.DataConsegna == data.Date && v.Stato != "Annullato")
                .OrderBy(v => v.OraPartenza)
                .ToListAsync();

            var destinazioniMap = new Dictionary<int, string>();
            foreach (var v in viaggi)
            {
                var righeInfo = await GetRigheConClienteDestinazioneAsync(v.Righe);
                var destDistinte = righeInfo
                    .Select(r => r.Destinazione)
                    .Where(d => !string.IsNullOrEmpty(d))
                    .Distinct();
                destinazioniMap[v.Id] = string.Join(", ", destDistinte);
            }

            using var package = new ExcelPackage();
            var ws = package.Workbook.Worksheets.Add($"Planning {data:dd-MM-yyyy}");

            var headers = new[] { "#", "Ora Part.", "Ora Arr.", "Tipo", "Mezzo", "Targa", "Autista", "Righe", "Peso Kg", "Portata Kg", "%", "Destinazioni", "Gru", "Trasb.", "Costo", "Prezzo", "Margine", "Stato" };
            for (int i = 0; i < headers.Length; i++)
                ws.Cells[1, i + 1].Value = headers[i];

            using (var range = ws.Cells[1, 1, 1, headers.Length])
            {
                range.Style.Font.Bold = true;
                range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                range.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(0, 112, 192));
                range.Style.Font.Color.SetColor(Color.White);
                range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            }

            int row = 2;
            foreach (var v in viaggi)
            {
                var pesoTot = v.Righe.Sum(r => r.PesoTotaleKgSnapshot);
                var portata = v.MezzoTrasporto?.PortataMaxKg ?? (v.MezzoTrasportoEsterno != null ? (decimal)v.MezzoTrasportoEsterno.PortataMax : 0);
                var percCarico = portata > 0 ? Math.Round((pesoTot / portata) * 100, 1) : 0;
                var oraArr = v.OraArrivo ?? v.OraPartenza.Add(TimeSpan.FromMinutes(v.DurataStimataMinuti));

                ws.Cells[row, 1].Value = row - 1;
                ws.Cells[row, 2].Value = v.OraPartenza.ToString(@"hh\:mm");
                ws.Cells[row, 3].Value = oraArr.ToString(@"hh\:mm");
                ws.Cells[row, 4].Value = v.TipoTrasporto?.Descrizione ?? "N/D";
                ws.Cells[row, 5].Value = v.MezzoTrasporto?.Descrizione ?? (v.MezzoTrasportoEsterno != null ? $"{v.MezzoTrasportoEsterno.NomeVettore} - {v.MezzoTrasportoEsterno.TipoMezzo}" : "N/D");
                ws.Cells[row, 6].Value = v.MezzoTrasporto?.Targa ?? "";
                ws.Cells[row, 7].Value = v.Autista != null ? $"{v.Autista.Cognome} {v.Autista.Nome}" : "";
                ws.Cells[row, 8].Value = v.Righe.Count;
                ws.Cells[row, 9].Value = pesoTot;
                ws.Cells[row, 10].Value = portata;
                ws.Cells[row, 11].Value = percCarico;
                ws.Cells[row, 12].Value = destinazioniMap.TryGetValue(v.Id, out var dest) ? dest : "";
                ws.Cells[row, 13].Value = (v.Gru ?? false) ? "Sì" : "No";
                ws.Cells[row, 14].Value = (v.Trasbordo ?? false) ? "Sì" : "No";
                ws.Cells[row, 15].Value = v.CostoTrasporto ?? 0;
                ws.Cells[row, 16].Value = v.PrezzoVendita ?? 0;
                ws.Cells[row, 17].Value = (v.PrezzoVendita ?? 0) - (v.CostoTrasporto ?? 0);
                ws.Cells[row, 18].Value = v.Stato;

                ws.Cells[row, 9].Style.Numberformat.Format = "#,##0.000";
                ws.Cells[row, 10].Style.Numberformat.Format = "#,##0.000";
                ws.Cells[row, 11].Style.Numberformat.Format = "0.0\"%\"";
                ws.Cells[row, 15].Style.Numberformat.Format = "€ #,##0.00";
                ws.Cells[row, 16].Style.Numberformat.Format = "€ #,##0.00";
                ws.Cells[row, 17].Style.Numberformat.Format = "€ #,##0.00";

                if (row % 2 == 0)
                {
                    using var rowRange = ws.Cells[row, 1, row, headers.Length];
                    rowRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    rowRange.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(242, 242, 242));
                }

                row++;
            }

            var totRow = row;
            ws.Cells[totRow, 7].Value = "TOTALI";
            ws.Cells[totRow, 7].Style.Font.Bold = true;
            ws.Cells[totRow, 8].Value = viaggi.Sum(v => v.Righe.Count);
            ws.Cells[totRow, 9].Value = viaggi.Sum(v => v.Righe.Sum(r => r.PesoTotaleKgSnapshot));
            ws.Cells[totRow, 15].Value = viaggi.Sum(v => v.CostoTrasporto ?? 0);
            ws.Cells[totRow, 16].Value = viaggi.Sum(v => v.PrezzoVendita ?? 0);
            ws.Cells[totRow, 17].Value = viaggi.Sum(v => (v.PrezzoVendita ?? 0) - (v.CostoTrasporto ?? 0));
            ws.Cells[totRow, 9].Style.Numberformat.Format = "#,##0.000";
            ws.Cells[totRow, 15].Style.Numberformat.Format = "€ #,##0.00";
            ws.Cells[totRow, 16].Style.Numberformat.Format = "€ #,##0.00";
            ws.Cells[totRow, 17].Style.Numberformat.Format = "€ #,##0.00";
            using (var totRange = ws.Cells[totRow, 1, totRow, headers.Length])
            {
                totRange.Style.Font.Bold = true;
                totRange.Style.Border.Top.Style = ExcelBorderStyle.Medium;
            }

            ws.Cells[ws.Dimension.Address].AutoFitColumns();
            ws.View.FreezePanes(2, 1);

            var bytes = package.GetAsByteArray();
            return File(bytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"Planning_{data:yyyy-MM-dd}.xlsx");
        }

        [HttpGet]
        [RequirePermission("ConsegneKanban", "View")]
        public async Task<IActionResult> ExportGanttConsegne()
        {
            var itCulture = new System.Globalization.CultureInfo("it-IT");
            var oggi = DateTime.Today;
            var oraInizio = new TimeSpan(7, 0, 0);
            var oraFine = new TimeSpan(18, 0, 0);
            int orePerGiorno = (int)(oraFine - oraInizio).TotalHours; // 11

            var tuttiGiorni = Enumerable.Range(0, 21).Select(i => oggi.AddDays(i)).ToList();

            var viaggi = await _context.ViaggiConsegna
                .AsNoTracking()
                .Include(v => v.TipoTrasporto)
                .Include(v => v.MezzoTrasporto)
                .Include(v => v.MezzoTrasportoEsterno)
                .Include(v => v.Autista)
                .Include(v => v.Righe).ThenInclude(r => r.OrdineRiga)
                .Where(v => v.DataConsegna >= oggi && v.DataConsegna <= oggi.AddDays(20) && v.Stato != "Annullato")
                .OrderBy(v => v.DataConsegna).ThenBy(v => v.OraPartenza)
                .ToListAsync();

            var giorni = tuttiGiorni
                .Where(d => d.DayOfWeek != DayOfWeek.Saturday && d.DayOfWeek != DayOfWeek.Sunday
                    || viaggi.Any(v => v.DataConsegna.Date == d))
                .Take(7)
                .ToList();

            var destinazioniMap = new Dictionary<int, string>();
            foreach (var v in viaggi)
            {
                var righeInfo = await GetRigheConClienteDestinazioneAsync(v.Righe);
                var destDistinte = righeInfo
                    .Select(r => r.Destinazione?.Split(',').FirstOrDefault()?.Trim())
                    .Where(d => !string.IsNullOrEmpty(d))
                    .Distinct();
                destinazioniMap[v.Id] = string.Join(", ", destDistinte);
            }

            using var package = new ExcelPackage();
            var ws = package.Workbook.Worksheets.Add("Gantt Consegne");

            int colMezzo = 1;
            var colorInterno = Color.FromArgb(210, 245, 220);
            var colorEsterno = Color.FromArgb(255, 248, 210);
            var colorBarInt = Color.FromArgb(60, 170, 100);
            var colorBarEst = Color.FromArgb(230, 180, 50);
            var colorBarOverload = Color.FromArgb(220, 60, 60);
            var colorBarWarn = Color.FromArgb(240, 170, 50);

            ws.Cells[1, 1].Value = "Gantt Consegne";
            ws.Cells[1, 1].Style.Font.Bold = true;
            ws.Cells[1, 1].Style.Font.Size = 14;

            // Row 2: day headers, Row 3: hour labels
            int dayHeaderRow = 2;
            int hourRow = 3;
            int dataStartRow = 4;

            ws.Cells[dayHeaderRow, colMezzo].Value = "Mezzo / Viaggio";
            ws.Cells[dayHeaderRow, colMezzo].Style.Font.Bold = true;
            ws.Cells[dayHeaderRow, colMezzo].Style.Font.Size = 9;
            ws.Cells[hourRow, colMezzo].Value = "";

            ws.Column(colMezzo).Width = 42;

            for (int g = 0; g < giorni.Count; g++)
            {
                var giorno = giorni[g];
                int colStart = 2 + g * orePerGiorno;
                int colEnd = colStart + orePerGiorno - 1;
                var viaggiGiorno = viaggi.Where(v => v.DataConsegna.Date == giorno).ToList();
                var nMezzi = viaggiGiorno.Select(v => v.MezzoTrasportoId ?? v.MezzoTrasportoEsternoId ?? 0).Distinct().Count();

                using (var dayRange = ws.Cells[dayHeaderRow, colStart, dayHeaderRow, colEnd])
                {
                    dayRange.Merge = true;
                    dayRange.Value = $"{giorno.ToString("ddd dd/MM", itCulture).ToUpper()} — {viaggiGiorno.Count} viaggi, {nMezzi} mezzi";
                    dayRange.Style.Font.Bold = true;
                    dayRange.Style.Font.Size = 9;
                    dayRange.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    dayRange.Style.Fill.PatternType = ExcelFillStyle.Solid;

                    var isToday = giorno == oggi;
                    dayRange.Style.Fill.BackgroundColor.SetColor(isToday ? Color.FromArgb(0, 112, 192) : Color.FromArgb(60, 60, 60));
                    dayRange.Style.Font.Color.SetColor(Color.White);
                    dayRange.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                }

                for (int h = 0; h < orePerGiorno; h++)
                {
                    int col = colStart + h;
                    int ora = 7 + h;
                    ws.Cells[hourRow, col].Value = ora.ToString("D2");
                    ws.Cells[hourRow, col].Style.Font.Bold = true;
                    ws.Cells[hourRow, col].Style.Font.Size = 8;
                    ws.Cells[hourRow, col].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    ws.Cells[hourRow, col].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    ws.Cells[hourRow, col].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(220, 225, 230));
                    ws.Column(col).Width = 3.5;
                }

                // Day separator
                for (int r = dayHeaderRow; r < dataStartRow + 100; r++)
                {
                    ws.Cells[r, colStart].Style.Border.Left.Style = ExcelBorderStyle.Thin;
                    ws.Cells[r, colStart].Style.Border.Left.Color.SetColor(Color.FromArgb(150, 150, 150));
                }
            }

            int totalCols = 1 + giorni.Count * orePerGiorno;

            int row = dataStartRow;
            foreach (var giorno in giorni)
            {
                var viaggiGiorno = viaggi.Where(v => v.DataConsegna.Date == giorno).OrderBy(v => v.OraPartenza).ToList();
                if (!viaggiGiorno.Any()) continue;

                foreach (var v in viaggiGiorno)
                {
                    bool isInterno = v.MezzoTrasportoId.HasValue;
                    var mezzo = v.MezzoTrasporto?.Descrizione
                        ?? (v.MezzoTrasportoEsterno != null ? $"{v.MezzoTrasportoEsterno.NomeVettore} - {v.MezzoTrasportoEsterno.TipoMezzo}" : "N/D");
                    var autista = v.Autista != null ? v.Autista.Cognome : "";
                    var dest = destinazioniMap.TryGetValue(v.Id, out var d) ? d : "";
                    var flags = new List<string>();
                    if (v.Gru == true) flags.Add("Gru");
                    if (v.Trasbordo == true) flags.Add("Trasb");
                    var flagStr = flags.Any() ? $" [{string.Join(",", flags)}]" : "";

                    var parts = new List<string> { mezzo };
                    if (!string.IsNullOrEmpty(autista)) parts.Add(autista);
                    if (!string.IsNullOrEmpty(dest)) parts.Add(dest);
                    var cellText = string.Join(" | ", parts) + flagStr;

                    ws.Cells[row, colMezzo].Value = cellText;
                    ws.Cells[row, colMezzo].Style.Font.Size = 8;
                    ws.Cells[row, colMezzo].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    ws.Cells[row, colMezzo].Style.Fill.BackgroundColor.SetColor(isInterno ? colorInterno : colorEsterno);

                    int gIdx = giorni.IndexOf(giorno);
                    int colStart = 2 + gIdx * orePerGiorno;
                    var partenza = v.OraPartenza;
                    var arrivo = v.OraArrivo ?? v.OraPartenza.Add(TimeSpan.FromMinutes(v.DurataStimataMinuti));
                    var pesoTot = v.Righe.Sum(r => r.PesoTotaleKgSnapshot);
                    var portata = v.MezzoTrasporto?.PortataMaxKg ?? (v.MezzoTrasportoEsterno != null ? (decimal)v.MezzoTrasportoEsterno.PortataMax : 0);

                    var barColor = isInterno ? colorBarInt : colorBarEst;
                    if (portata > 0 && pesoTot > portata)
                        barColor = colorBarOverload;
                    else if (portata > 0 && pesoTot > portata * 0.9m)
                        barColor = colorBarWarn;

                    for (int h = 0; h < orePerGiorno; h++)
                    {
                        var slotStart = oraInizio.Add(TimeSpan.FromHours(h));
                        var slotEnd = slotStart.Add(TimeSpan.FromHours(1));
                        if (partenza < slotEnd && slotStart < arrivo)
                        {
                            var cell = ws.Cells[row, colStart + h];
                            cell.Style.Fill.PatternType = ExcelFillStyle.Solid;
                            cell.Style.Fill.BackgroundColor.SetColor(barColor);
                            cell.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                            cell.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                            cell.Style.Border.Top.Color.SetColor(Color.FromArgb(80, 80, 80));
                            cell.Style.Border.Bottom.Color.SetColor(Color.FromArgb(80, 80, 80));

                            if (slotStart <= partenza)
                            {
                                cell.Style.Border.Left.Style = ExcelBorderStyle.Medium;
                                cell.Style.Border.Left.Color.SetColor(Color.FromArgb(30, 30, 30));
                            }
                            if (slotEnd >= arrivo)
                            {
                                cell.Style.Border.Right.Style = ExcelBorderStyle.Medium;
                                cell.Style.Border.Right.Color.SetColor(Color.FromArgb(30, 30, 30));
                            }
                        }
                    }

                    row++;
                }
            }

            ws.View.FreezePanes(dataStartRow, 2);

            var bytes = package.GetAsByteArray();
            return File(bytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"Gantt_Consegne_{oggi:yyyy-MM-dd}.xlsx");
        }

        private record RigaConClienteDestinazione(
            string OrdineCompleto, int RigaOrdine, string Cliente, string? Destinazione,
            string CodiceArticolo, string? DescrizioneArticolo, string? UnitaMisura,
            decimal QuantitaAssegnata, decimal PesoUnitarioKg, decimal PesoTotaleKg, string? NoteRiga);

        private async Task<List<RigaConClienteDestinazione>> GetRigheConClienteDestinazioneAsync(ICollection<ViaggioConsegnaRiga> righe)
        {
            var result = new List<RigaConClienteDestinazione>();

            foreach (var riga in righe)
            {
                var or = riga.OrdineRiga;
                if (or == null) continue;

                var testata = await _context.OrdiniTestate.AsNoTracking()
                    .FirstOrDefaultAsync(t => t.TipoOrdine == or.TipoOrdine && t.AnnoOrdine == or.AnnoOrdine
                        && t.SerieOrdine == or.SerieOrdine && t.NumeroOrdine == or.NumeroOrdine);

                string cliente = "N/D";
                string? destinazione = null;

                if (testata != null)
                {
                    var cli = await _context.AnagraficaClienti.AsNoTracking()
                        .FirstOrDefaultAsync(c => c.CodiceCliente == testata.CodiceCliente);
                    cliente = cli?.RagioneSociale ?? $"Cliente {testata.CodiceCliente}";

                    if (testata.CodiceDestinazione.HasValue && testata.CodiceDestinazione.Value != 0)
                    {
                        var dest = await _context.DestinazioniDiverse.AsNoTracking()
                            .FirstOrDefaultAsync(d => d.CodiceConto == testata.CodiceCliente && d.CodiceDestinazione == testata.CodiceDestinazione.Value);
                        if (dest != null)
                        {
                            var parti = new List<string>();
                            if (!string.IsNullOrEmpty(dest.DescrizioneDestinazione)) parti.Add(dest.DescrizioneDestinazione);
                            if (!string.IsNullOrEmpty(dest.Indirizzo)) parti.Add(dest.Indirizzo);
                            if (!string.IsNullOrEmpty(dest.Localita)) parti.Add(dest.Localita);
                            if (!string.IsNullOrEmpty(dest.Provincia)) parti.Add($"({dest.Provincia})");
                            destinazione = string.Join(", ", parti);
                        }
                    }
                    else if (cli != null)
                    {
                        var parti = new List<string>();
                        if (!string.IsNullOrEmpty(cli.Indirizzo)) parti.Add(cli.Indirizzo);
                        if (!string.IsNullOrEmpty(cli.Citta)) parti.Add(cli.Citta);
                        if (!string.IsNullOrEmpty(cli.Provincia)) parti.Add($"({cli.Provincia})");
                        destinazione = string.Join(", ", parti);
                    }
                }

                result.Add(new RigaConClienteDestinazione(
                    $"{or.TipoOrdine}{or.AnnoOrdine}/{or.SerieOrdine}/{or.NumeroOrdine:D6}",
                    or.RigaOrdine, cliente, destinazione,
                    or.CodiceArticolo, or.DescrizioneArticolo, or.UnitaMisura,
                    riga.QuantitaAssegnata, riga.PesoUnitarioKgSnapshot, riga.PesoTotaleKgSnapshot, riga.NoteRiga));
            }

            return result.OrderBy(r => r.Cliente).ThenBy(r => r.OrdineCompleto).ThenBy(r => r.RigaOrdine).ToList();
        }

        private async Task<bool> EsisteConflittoMezzoAsync(DateTime dataConsegna, int mezzoId, TimeSpan nuovaPartenza, TimeSpan nuovoArrivo, int? viaggioDaEscludere)
        {
            var viaggiStessoMezzo = await _context.ViaggiConsegna
                .Where(v => v.DataConsegna == dataConsegna
                    && v.MezzoTrasportoId == mezzoId
                    && v.Stato != "Annullato"
                    && (!viaggioDaEscludere.HasValue || v.Id != viaggioDaEscludere.Value))
                .ToListAsync();

            foreach (var v in viaggiStessoMezzo)
            {
                var arrivoV = v.OraArrivo ?? v.OraPartenza.Add(TimeSpan.FromMinutes(v.DurataStimataMinuti));
                var overlap = nuovaPartenza < arrivoV && v.OraPartenza < nuovoArrivo;
                if (overlap)
                {
                    _logger.LogWarning("Conflitto mezzo interno rilevato su Data {Data} Mezzo {MezzoId}", dataConsegna, mezzoId);
                    return true;
                }
            }

            return false;
        }

        private async Task<bool> EsisteConflittoMezzoEsternoAsync(DateTime dataConsegna, int mezzoEsternoId, TimeSpan nuovaPartenza, TimeSpan nuovoArrivo, int? viaggioDaEscludere)
        {
            var viaggiStessoMezzo = await _context.ViaggiConsegna
                .Where(v => v.DataConsegna == dataConsegna
                    && v.MezzoTrasportoEsternoId == mezzoEsternoId
                    && v.Stato != "Annullato"
                    && (!viaggioDaEscludere.HasValue || v.Id != viaggioDaEscludere.Value))
                .ToListAsync();

            foreach (var v in viaggiStessoMezzo)
            {
                var arrivoV = v.OraArrivo ?? v.OraPartenza.Add(TimeSpan.FromMinutes(v.DurataStimataMinuti));
                var overlap = nuovaPartenza < arrivoV && v.OraPartenza < nuovoArrivo;
                if (overlap)
                {
                    _logger.LogWarning("Conflitto mezzo esterno rilevato su Data {Data} MezzoEsterno {MezzoEsternoId}", dataConsegna, mezzoEsternoId);
                    return true;
                }
            }

            return false;
        }

        private async Task<bool> EsisteConflittoAutistaAsync(DateTime dataConsegna, int autistaId, TimeSpan nuovaPartenza, TimeSpan nuovoArrivo, int? viaggioDaEscludere)
        {
            var viaggiStessoAutista = await _context.ViaggiConsegna
                .Where(v => v.DataConsegna == dataConsegna
                    && v.AutistaId == autistaId
                    && v.Stato != "Annullato"
                    && (!viaggioDaEscludere.HasValue || v.Id != viaggioDaEscludere.Value))
                .ToListAsync();

            foreach (var v in viaggiStessoAutista)
            {
                var arrivoV = v.OraArrivo ?? v.OraPartenza.Add(TimeSpan.FromMinutes(v.DurataStimataMinuti));
                var overlap = nuovaPartenza < arrivoV && v.OraPartenza < nuovoArrivo;
                if (overlap)
                {
                    _logger.LogWarning("Conflitto autista rilevato su Data {Data} Autista {AutistaId}", dataConsegna, autistaId);
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Validazione centralizzata per creazione/modifica viaggio.
        /// Restituisce il messaggio di errore oppure null se tutto ok.
        /// </summary>
        private async Task<string?> ValidaViaggioAsync(
            DateTime dataConsegna,
            int? mezzoTrasportoId,
            int? mezzoTrasportoEsternoId,
            TimeSpan oraPartenza,
            TimeSpan oraArrivoEffettiva,
            int? autistaId,
            bool gru,
            bool trasbordo,
            int? viaggioDaEscludere)
        {
            if (oraArrivoEffettiva <= oraPartenza)
                return "L'ora di arrivo deve essere successiva all'ora di partenza.";

            if (mezzoTrasportoId.HasValue)
            {
                var mezzo = await _context.MezziTrasporto.AsNoTracking().FirstOrDefaultAsync(m => m.Id == mezzoTrasportoId.Value);
                if (mezzo != null && !mezzo.Attivo)
                    return "Il mezzo interno selezionato non è attivo.";
                if (gru && mezzo != null && !mezzo.Gru)
                    return "Il mezzo interno selezionato non dispone di gru.";
                if (trasbordo && mezzo != null && !mezzo.Trasbordo)
                    return "Il mezzo interno selezionato non supporta il trasbordo.";

                if (await EsisteConflittoMezzoAsync(dataConsegna, mezzoTrasportoId.Value, oraPartenza, oraArrivoEffettiva, viaggioDaEscludere))
                    return "Il mezzo interno selezionato è già impegnato nella stessa fascia oraria.";
            }

            if (mezzoTrasportoEsternoId.HasValue)
            {
                var mezzoEst = await _context.MezziTrasportoEsterni.AsNoTracking().FirstOrDefaultAsync(m => m.Id == mezzoTrasportoEsternoId.Value);
                if (mezzoEst != null)
                {
                    if (gru && !mezzoEst.Gru)
                        return "Il mezzo esterno selezionato non dispone di gru.";
                    if (trasbordo && !mezzoEst.Trasbordo)
                        return "Il mezzo esterno selezionato non supporta il trasbordo.";
                }

                if (await EsisteConflittoMezzoEsternoAsync(dataConsegna, mezzoTrasportoEsternoId.Value, oraPartenza, oraArrivoEffettiva, viaggioDaEscludere))
                    return "Il mezzo esterno selezionato è già impegnato nella stessa fascia oraria.";
            }

            if (autistaId.HasValue)
            {
                var autista = await _context.Autisti.AsNoTracking().FirstOrDefaultAsync(a => a.Id == autistaId.Value);
                if (autista != null && !autista.Attivo)
                    return "L'autista selezionato non è attivo.";

                if (await EsisteConflittoAutistaAsync(dataConsegna, autistaId.Value, oraPartenza, oraArrivoEffettiva, viaggioDaEscludere))
                    return "L'autista selezionato è già impegnato in un altro viaggio nella stessa fascia oraria.";
            }

            return null;
        }

        /// <summary>
        /// Calcola la quantità residua disponibile per una riga ordine,
        /// escludendo eventualmente una specifica riga viaggio (utile in caso di spostamento).
        /// </summary>
        private async Task<decimal> CalcolaQuantitaResiduaAsync(int ordineRigaId, int? viaggioRigaIdDaEscludere)
        {
            var rigaOrdine = await _context.OrdiniRighe.AsNoTracking().FirstOrDefaultAsync(r => r.Id == ordineRigaId);
            if (rigaOrdine == null) return 0;

            var query = _context.ViaggioConsegnaRighe
                .AsNoTracking()
                .Include(x => x.ViaggioConsegna)
                .Where(x => x.OrdineRigaId == ordineRigaId
                    && x.ViaggioConsegna != null
                    && x.ViaggioConsegna.Stato != "Annullato");

            if (viaggioRigaIdDaEscludere.HasValue)
                query = query.Where(x => x.Id != viaggioRigaIdDaEscludere.Value);

            var qtaGiaAssegnata = await query.SumAsync(x => (decimal?)x.QuantitaAssegnata) ?? 0;

            return rigaOrdine.Quantita - rigaOrdine.QuantitaEvasa - qtaGiaAssegnata;
        }
    }
}
