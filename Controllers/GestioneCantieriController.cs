using AiDbMaster.Attributes;
using AiDbMaster.Data;
using AiDbMaster.Models;
using AiDbMaster.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace AiDbMaster.Controllers
{
    [Authorize]
    [RegisterResource("GestioneCantieri", "Gestione Cantieri",
        Description = "Gestione cantieri, fornitura e posa pavimentazioni",
        MenuIcon = "bi-building", MenuOrder = 10)]
    [RequirePermission("GestioneCantieri", "View")]
    public class GestioneCantieriController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<GestioneCantieriController> _logger;

        public GestioneCantieriController(
            ApplicationDbContext context,
            ILogger<GestioneCantieriController> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IActionResult> Index(string? q, CantiereStato? stato)
        {
            ViewData["Title"] = "Elenco Cantieri";
            ViewBag.FiltroTesto = q;
            ViewBag.FiltroStato = stato;
            ViewBag.Stati = GetStatiSelectList(stato);

            var query = _context.Cantieri.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(q))
            {
                var term = q.Trim();
                query = query.Where(c =>
                    c.Nome.Contains(term) ||
                    c.Codice.Contains(term) ||
                    (c.Localita != null && c.Localita.Contains(term)) ||
                    (c.ImpresaPosa != null && c.ImpresaPosa.Nome.Contains(term)) ||
                    _context.AnagraficaClienti.Any(cl =>
                        cl.CodiceCliente == c.CodiceCliente && cl.RagioneSociale.Contains(term)));
            }

            if (stato.HasValue)
                query = query.Where(c => c.Stato == stato.Value);

            var elenco = await query
                .OrderByDescending(c => c.DataCreazione)
                .Select(c => new CantiereElencoItemViewModel
                {
                    Id = c.Id,
                    Codice = c.Codice,
                    Nome = c.Nome,
                    CodiceCliente = c.CodiceCliente,
                    ClienteNome = _context.AnagraficaClienti
                        .Where(cl => cl.CodiceCliente == c.CodiceCliente)
                        .Select(cl => cl.RagioneSociale)
                        .FirstOrDefault() ?? string.Empty,
                    Localita = c.Localita,
                    Provincia = c.Provincia,
                    ImpresaPosa = c.ImpresaPosa != null ? c.ImpresaPosa.Nome : null,
                    DataInizioPrevista = c.DataInizioPrevista,
                    DataFinePrevista = c.DataFinePrevista,
                    Stato = c.Stato,
                    NumeroOrdini = c.Ordini.Count
                })
                .ToListAsync();

            return View(elenco);
        }

        [RequirePermission("GestioneCantieri", "Create")]
        public async Task<IActionResult> Create()
        {
            ViewData["Title"] = "Nuovo cantiere";
            await CaricaListeSchedaAsync(CantiereStato.Preventivo);
            return View(new CantiereSchedaViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission("GestioneCantieri", "Create")]
        public async Task<IActionResult> Create(CantiereSchedaViewModel model)
        {
            ValidaScheda(model);
            if (!ModelState.IsValid)
            {
                if (model.CodiceCliente.HasValue)
                {
                    model.ClienteNome = await GetNomeClienteAsync(model.CodiceCliente.Value);
                    model.NomeAgente = await GetNomeAgenteAsync(model.CodiceCliente.Value);
                }
                ViewData["Title"] = "Nuovo cantiere";
                await CaricaListeSchedaAsync(model.Stato, model.ImpresaPosaId);
                return View(model);
            }

            var utente = GetUtenteCorrente();
            var cantiere = new Cantiere
            {
                Codice = $"TMP-{Guid.NewGuid():N}"[..20],
                Nome = model.Nome.Trim(),
                CodiceCliente = model.CodiceCliente!.Value,
                CodiceDestinazione = model.CodiceDestinazione,
                Indirizzo = NullIfEmpty(model.Indirizzo),
                Cap = NullIfEmpty(model.Cap),
                Localita = NullIfEmpty(model.Localita),
                Provincia = NormalizzaProvincia(model.Provincia),
                Telefono = NullIfEmpty(model.Telefono),
                Referente = null,
                ImpresaPosaId = model.ImpresaPosaId,
                DataInizioPrevista = model.DataInizioPrevista,
                DataFinePrevista = model.DataFinePrevista,
                Stato = model.Stato,
                Note = NullIfEmpty(model.Note),
                PercorsoDocumenti = NullIfEmpty(model.PercorsoDocumenti),
                DataCreazione = DateTime.Now,
                UtenteCreazione = utente
            };
            ApplicaChecklist(cantiere, model);

            _context.Cantieri.Add(cantiere);
            await _context.SaveChangesAsync();

            cantiere.Codice = $"CAN-{cantiere.Id:D5}";
            await _context.SaveChangesAsync();
            _logger.LogInformation("Creato cantiere {Codice} per cliente {Cliente}", cantiere.Codice, cantiere.CodiceCliente);

            TempData["SuccessMessage"] = "Cantiere creato. Ora puoi collegare gli ordini del cliente.";
            return RedirectToAction(nameof(Edit), new { id = cantiere.Id });
        }

        public async Task<IActionResult> Edit(int id)
        {
            var cantiere = await CaricaCantiereConOrdiniAsync(id);

            if (cantiere == null)
            {
                TempData["ErrorMessage"] = "Cantiere non trovato.";
                return RedirectToAction(nameof(Index));
            }

            var scheda = await MapToSchedaAsync(cantiere);
            ViewData["Title"] = TitoloScheda(scheda);
            await CaricaListeSchedaAsync(cantiere.Stato, cantiere.ImpresaPosaId);
            return View(scheda);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission("GestioneCantieri", "Edit")]
        public async Task<IActionResult> Edit(int id, CantiereSchedaViewModel model)
        {
            if (id != model.Id)
            {
                TempData["ErrorMessage"] = "Richiesta non valida.";
                return RedirectToAction(nameof(Index));
            }

            ValidaScheda(model);
            if (!ModelState.IsValid)
            {
                var cantiereError = await CaricaCantiereConOrdiniAsync(id);
                if (cantiereError == null)
                {
                    TempData["ErrorMessage"] = "Cantiere non trovato.";
                    return RedirectToAction(nameof(Index));
                }

                model.Codice = cantiereError.Codice;
                model.ClienteNome = await GetNomeClienteAsync(cantiereError.CodiceCliente);
                model.NomeAgente = await GetNomeAgenteAsync(cantiereError.CodiceCliente);
                model.Ordini = MapOrdini(cantiereError);
                model.Referenti = MapReferenti(cantiereError);
                ViewData["Title"] = TitoloScheda(model);
                await CaricaListeSchedaAsync(model.Stato, model.ImpresaPosaId);
                return View(model);
            }

            var existing = await _context.Cantieri.FirstOrDefaultAsync(c => c.Id == id);
            if (existing == null)
            {
                TempData["ErrorMessage"] = "Cantiere non trovato.";
                return RedirectToAction(nameof(Index));
            }

            existing.Nome = model.Nome.Trim();
            existing.CodiceCliente = model.CodiceCliente!.Value;
            existing.CodiceDestinazione = model.CodiceDestinazione;
            existing.Indirizzo = NullIfEmpty(model.Indirizzo);
            existing.Cap = NullIfEmpty(model.Cap);
            existing.Localita = NullIfEmpty(model.Localita);
            existing.Provincia = NormalizzaProvincia(model.Provincia);
            existing.Telefono = NullIfEmpty(model.Telefono);
            // Il referente testuale è sostituito dall'anagrafica collegata.
            existing.ImpresaPosaId = model.ImpresaPosaId;
            existing.DataInizioPrevista = model.DataInizioPrevista;
            existing.DataFinePrevista = model.DataFinePrevista;
            existing.Stato = model.Stato;
            existing.Note = NullIfEmpty(model.Note);
            existing.PercorsoDocumenti = NullIfEmpty(model.PercorsoDocumenti);
            existing.DataUltimaModifica = DateTime.Now;
            existing.UtenteModifica = GetUtenteCorrente();
            ApplicaChecklist(existing, model);

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Scheda cantiere aggiornata.";
            return RedirectToAction(nameof(Edit), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission("GestioneCantieri", "Delete")]
        public async Task<IActionResult> Delete(int id)
        {
            var cantiere = await _context.Cantieri.FirstOrDefaultAsync(c => c.Id == id);
            if (cantiere == null)
            {
                TempData["ErrorMessage"] = "Cantiere non trovato.";
                return RedirectToAction(nameof(Index));
            }

            _context.Cantieri.Remove(cantiere);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Eliminato cantiere {Codice}", cantiere.Codice);
            TempData["SuccessMessage"] = $"Cantiere {cantiere.Codice} eliminato. Gli ordini non sono stati cancellati.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> CercaClienti(string term)
        {
            if (string.IsNullOrWhiteSpace(term) || term.Trim().Length < 2)
                return Json(Array.Empty<object>());

            var testo = term.Trim();
            var clienti = await _context.AnagraficaClienti
                .AsNoTracking()
                .Where(c =>
                    c.RagioneSociale.Contains(testo) ||
                    c.CodiceCliente.ToString().Contains(testo) ||
                    (c.DescrizioneUlteriore != null && c.DescrizioneUlteriore.Contains(testo)))
                .OrderBy(c => c.RagioneSociale)
                .Take(20)
                .Select(c => new
                {
                    id = c.CodiceCliente,
                    text = c.CodiceCliente + " - " + c.RagioneSociale
                })
                .ToListAsync();

            return Json(clienti);
        }

        [HttpGet]
        public async Task<IActionResult> DatiCliente(int codiceCliente)
        {
            var cliente = await _context.AnagraficaClienti
                .AsNoTracking()
                .Include(c => c.Agente)
                .FirstOrDefaultAsync(c => c.CodiceCliente == codiceCliente);

            if (cliente == null)
                return NotFound();

            return Json(new
            {
                codiceCliente = cliente.CodiceCliente,
                ragioneSociale = cliente.RagioneSociale,
                indirizzo = cliente.Indirizzo,
                cap = cliente.Cap,
                localita = cliente.Citta,
                provincia = cliente.Provincia,
                telefono = cliente.Telefono,
                agente = cliente.Agente != null
                    ? cliente.Agente.DescrizioneAgente
                    : null
            });
        }

        [HttpGet]
        public async Task<IActionResult> DestinazioniCliente(int codiceCliente)
        {
            var destinazioni = await _context.DestinazioniDiverse
                .AsNoTracking()
                .Where(d => d.CodiceConto == codiceCliente)
                .OrderBy(d => d.CodiceDestinazione)
                .Select(d => new
                {
                    codice = d.CodiceDestinazione,
                    text = (d.DescrizioneDestinazione ?? ("Destinazione " + d.CodiceDestinazione))
                           + " - " + (d.Indirizzo ?? "") + " " + (d.Localita ?? ""),
                    indirizzo = d.Indirizzo,
                    cap = d.Cap,
                    localita = d.Localita,
                    provincia = d.Provincia,
                    telefono = d.Telefono
                })
                .ToListAsync();

            return Json(destinazioni);
        }

        [HttpGet]
        public async Task<IActionResult> CercaOrdini(int codiceCliente, int? cantiereId)
        {
            var idsCollegati = _context.CantiereOrdini
                .AsNoTracking()
                .Select(o => o.OrdineTestataId);

            var ordini = await _context.OrdiniTestate
                .AsNoTracking()
                .Where(o => o.TipoOrdine == "R"
                            && o.CodiceCliente == codiceCliente
                            && !idsCollegati.Contains(o.Id))
                .OrderByDescending(o => o.DataOrdine)
                .ThenByDescending(o => o.NumeroOrdine)
                .Take(100)
                .Select(o => new
                {
                    id = o.Id,
                    text = o.TipoOrdine + o.AnnoOrdine + "/" + o.SerieOrdine + "/" + o.NumeroOrdine.ToString("000000")
                           + (o.RiferimentoOrdine != null && o.RiferimentoOrdine != ""
                               ? " - " + o.RiferimentoOrdine
                               : "")
                           + " del " + o.DataOrdine.ToString("dd/MM/yyyy")
                })
                .ToListAsync();

            return Json(ordini);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission("GestioneCantieri", "Edit")]
        public async Task<IActionResult> AggiungiOrdine(int cantiereId, int ordineId)
        {
            var cantiere = await _context.Cantieri.FirstOrDefaultAsync(c => c.Id == cantiereId);
            if (cantiere == null)
                return Json(new { success = false, message = "Cantiere non trovato." });

            var ordine = await _context.OrdiniTestate
                .AsNoTracking()
                .FirstOrDefaultAsync(o => o.Id == ordineId && o.TipoOrdine == "R");

            if (ordine == null)
                return Json(new { success = false, message = "Ordine cliente non trovato." });

            if (ordine.CodiceCliente != cantiere.CodiceCliente)
                return Json(new { success = false, message = "L'ordine appartiene a un altro cliente." });

            var giaCollegato = await _context.CantiereOrdini
                .AsNoTracking()
                .Include(o => o.Cantiere)
                .FirstOrDefaultAsync(o => o.OrdineTestataId == ordineId);

            if (giaCollegato != null)
            {
                var codice = giaCollegato.Cantiere?.Codice ?? giaCollegato.CantiereId.ToString();
                return Json(new { success = false, message = $"L'ordine è già collegato al cantiere {codice}." });
            }

            var link = new CantiereOrdine
            {
                CantiereId = cantiereId,
                OrdineTestataId = ordineId,
                DataCollegamento = DateTime.Now
            };
            _context.CantiereOrdini.Add(link);
            await _context.SaveChangesAsync();

            return Json(new
            {
                success = true,
                ordine = new
                {
                    id = link.Id,
                    ordineTestataId = ordine.Id,
                    numeroOrdine = ordine.NumeroOrdineCompleto,
                    dataOrdine = ordine.DataOrdine.ToString("dd/MM/yyyy"),
                    riferimento = ordine.RiferimentoOrdine,
                    statoEvasione = ordine.DescrizioneStatoEvasione,
                    statoEvasioneCss = ordine.StatoEvasioneCssClass,
                    trasportoPosa = ordine.TrasportoPosa == "S"
                }
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission("GestioneCantieri", "Edit")]
        public async Task<IActionResult> RimuoviOrdine(int id)
        {
            var link = await _context.CantiereOrdini.FirstOrDefaultAsync(o => o.Id == id);
            if (link == null)
                return Json(new { success = false, message = "Collegamento non trovato." });

            _context.CantiereOrdini.Remove(link);
            await _context.SaveChangesAsync();
            return Json(new { success = true });
        }

        [HttpGet]
        public async Task<IActionResult> CercaReferenti(int? cantiereId)
        {
            var idsCollegati = cantiereId.HasValue
                ? _context.CantiereReferenti.AsNoTracking()
                    .Where(r => r.CantiereId == cantiereId.Value)
                    .Select(r => r.ReferenteId)
                : _context.CantiereReferenti.AsNoTracking().Where(r => false).Select(r => r.ReferenteId);

            var referenti = await _context.ReferentiCantiere
                .AsNoTracking()
                .Where(r => r.Attivo && !idsCollegati.Contains(r.Id))
                .OrderBy(r => r.Nome)
                .Select(r => new
                {
                    id = r.Id,
                    text = r.Nome + (r.Telefono != null && r.Telefono != "" ? " - " + r.Telefono : "")
                })
                .ToListAsync();

            return Json(referenti);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission("GestioneCantieri", "Edit")]
        public async Task<IActionResult> AggiungiReferente(int cantiereId, int referenteId, string? ruolo)
        {
            var cantiere = await _context.Cantieri.AnyAsync(c => c.Id == cantiereId);
            if (!cantiere)
                return Json(new { success = false, message = "Cantiere non trovato." });

            var referente = await _context.ReferentiCantiere
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.Id == referenteId && r.Attivo);

            if (referente == null)
                return Json(new { success = false, message = "Referente non trovato o non attivo." });

            var giaCollegato = await _context.CantiereReferenti
                .AnyAsync(r => r.CantiereId == cantiereId && r.ReferenteId == referenteId);

            if (giaCollegato)
                return Json(new { success = false, message = "Questo referente è già collegato al cantiere." });

            var link = new CantiereReferente
            {
                CantiereId = cantiereId,
                ReferenteId = referenteId,
                Ruolo = string.IsNullOrWhiteSpace(ruolo) ? null : ruolo.Trim()
            };
            _context.CantiereReferenti.Add(link);
            await _context.SaveChangesAsync();

            return Json(new
            {
                success = true,
                referente = new
                {
                    id = link.Id,
                    referenteId = referente.Id,
                    nome = referente.Nome,
                    telefono = referente.Telefono,
                    email = referente.Email,
                    ruolo = link.Ruolo
                }
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission("GestioneCantieri", "Edit")]
        public async Task<IActionResult> RimuoviReferente(int id)
        {
            var link = await _context.CantiereReferenti.FirstOrDefaultAsync(r => r.Id == id);
            if (link == null)
                return Json(new { success = false, message = "Collegamento non trovato." });

            _context.CantiereReferenti.Remove(link);
            await _context.SaveChangesAsync();
            return Json(new { success = true });
        }

        [RequirePermission("GestioneCantieri", "View")]
        public async Task<IActionResult> Contabilita(int id, TipoContabilitaCantiere tipo = TipoContabilitaCantiere.Iniziale)
        {
            var model = await CaricaContabilitaViewModelAsync(id, tipo);
            if (model == null)
                return NotFound();

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission("GestioneCantieri", "Edit")]
        public async Task<IActionResult> SalvaContabilita(ContabilitaCantiereViewModel model)
        {
            var cantiere = await _context.Cantieri.FirstOrDefaultAsync(c => c.Id == model.CantiereId);
            if (cantiere == null)
                return NotFound();

            var testata = await GetOrCreateContabilitaAsync(cantiere.Id, model.Tipo);
            testata.AliquotaIva = model.AliquotaIva;
            testata.ImponibileAcconto = model.ImponibileAcconto;
            testata.RiferimentoOrdine = NullIfEmpty(model.RiferimentoOrdine);
            testata.Note = NullIfEmpty(model.Note);
            testata.DataUltimaModifica = DateTime.Now;

            await SostituisciRigheContabilitaAsync(testata.Id, model.Righe);

            if (!cantiere.ContabilitaCantiere)
                cantiere.ContabilitaCantiere = true;

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Contabilita), new { id = cantiere.Id, tipo = model.Tipo });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission("GestioneCantieri", "Edit")]
        public async Task<IActionResult> ImportaDaOrdini(int id, TipoContabilitaCantiere tipo)
        {
            var cantiere = await _context.Cantieri.FirstOrDefaultAsync(c => c.Id == id);
            if (cantiere == null)
                return NotFound();

            var testata = await GetOrCreateContabilitaAsync(id, tipo);
            var ordineCorrente = await _context.CantiereContabilitaRighe
                .Where(r => r.ContabilitaId == testata.Id)
                .Select(r => (int?)r.Ordine)
                .MaxAsync() ?? 0;

            var ordini = await _context.CantiereOrdini
                .AsNoTracking()
                .Where(o => o.CantiereId == id)
                .Include(o => o.Ordine)
                .Select(o => o.Ordine)
                .ToListAsync();

            if (ordini.Count == 0)
            {
                TempData["ErrorMessage"] = "Non ci sono ordini collegati a questo cantiere. Collegali dalla scheda e poi importa.";
                return RedirectToAction(nameof(Contabilita), new { id, tipo });
            }

            var importate = 0;
            foreach (var ordine in ordini)
            {
                var righeOrdine = await _context.OrdiniRighe
                    .AsNoTracking()
                    .Where(r =>
                        r.TipoOrdine == ordine.TipoOrdine &&
                        r.AnnoOrdine == ordine.AnnoOrdine &&
                        r.SerieOrdine == ordine.SerieOrdine &&
                        r.NumeroOrdine == ordine.NumeroOrdine)
                    .OrderBy(r => r.RigaOrdine)
                    .ToListAsync();

                foreach (var riga in righeOrdine)
                {
                    _context.CantiereContabilitaRighe.Add(new CantiereContabilitaRiga
                    {
                        ContabilitaId = testata.Id,
                        PadreId = null,
                        Ordine = ++ordineCorrente,
                        CodiceArticolo = NullIfEmpty(riga.CodiceArticolo),
                        Descrizione = string.IsNullOrWhiteSpace(riga.DescrizioneArticolo)
                            ? riga.CodiceArticolo
                            : riga.DescrizioneArticolo.Trim(),
                        UnitaMisura = NullIfEmpty(riga.UnitaMisura),
                        QuantitaPosata = riga.Quantita,
                        PrezzoVenditaCliente = riga.Prezzo
                    });
                    importate++;
                }
            }

            if (string.IsNullOrWhiteSpace(testata.RiferimentoOrdine))
            {
                testata.RiferimentoOrdine = string.Join("; ", ordini
                    .Select(o => $"{o.NumeroOrdineCompleto} del {o.DataOrdine:dd/MM/yyyy}"));
            }

            testata.DataUltimaModifica = DateTime.Now;
            if (!cantiere.ContabilitaCantiere)
                cantiere.ContabilitaCantiere = true;

            await _context.SaveChangesAsync();

            if (importate == 0)
                TempData["ErrorMessage"] = "Gli ordini collegati non hanno righe da importare.";

            return RedirectToAction(nameof(Contabilita), new { id, tipo });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission("GestioneCantieri", "Edit")]
        public async Task<IActionResult> CopiaInizialeSuFinale(int id)
        {
            var iniziale = await _context.CantiereContabilita
                .Include(c => c.Righe)
                .FirstOrDefaultAsync(c => c.CantiereId == id && c.Tipo == TipoContabilitaCantiere.Iniziale);

            if (iniziale == null)
            {
                TempData["ErrorMessage"] = "Non esiste ancora una contabilità iniziale da copiare.";
                return RedirectToAction(nameof(Contabilita), new { id, tipo = TipoContabilitaCantiere.Finale });
            }

            var finale = await GetOrCreateContabilitaAsync(id, TipoContabilitaCantiere.Finale);
            finale.AliquotaIva = iniziale.AliquotaIva;
            finale.ImponibileAcconto = iniziale.ImponibileAcconto;
            finale.RiferimentoOrdine = iniziale.RiferimentoOrdine;
            finale.Note = iniziale.Note;
            finale.DataUltimaModifica = DateTime.Now;

            await EliminaRigheContabilitaAsync(finale.Id);

            var padri = iniziale.Righe
                .Where(r => r.PadreId == null)
                .OrderBy(r => r.Ordine)
                .ToList();
            var mappaPadri = new Dictionary<int, int>();

            foreach (var padre in padri)
            {
                var nuovoPadre = CopiaRigaContabilita(padre, finale.Id, null);
                _context.CantiereContabilitaRighe.Add(nuovoPadre);
                await _context.SaveChangesAsync();
                mappaPadri[padre.Id] = nuovoPadre.Id;
            }

            foreach (var figlio in iniziale.Righe.Where(r => r.PadreId.HasValue).OrderBy(r => r.Ordine))
            {
                if (!mappaPadri.TryGetValue(figlio.PadreId!.Value, out var nuovoPadreId))
                    continue;

                _context.CantiereContabilitaRighe.Add(CopiaRigaContabilita(figlio, finale.Id, nuovoPadreId));
            }

            var cantiere = await _context.Cantieri.FirstOrDefaultAsync(c => c.Id == id);
            if (cantiere != null && !cantiere.ContabilitaCantiere)
                cantiere.ContabilitaCantiere = true;

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Contabilita), new { id, tipo = TipoContabilitaCantiere.Finale });
        }

        [HttpGet]
        public async Task<IActionResult> CercaArticoliContabilita(string term)
        {
            if (string.IsNullOrWhiteSpace(term) || term.Trim().Length < 2)
                return Json(Array.Empty<object>());

            var testo = term.Trim();
            var articoli = await _context.AnagraficaArticoli
                .AsNoTracking()
                .Where(a =>
                    a.CodiceArticolo.Contains(testo) ||
                    a.Descrizione.Contains(testo) ||
                    (a.CodiceAlternativo != null && a.CodiceAlternativo.Contains(testo)))
                .OrderBy(a => a.CodiceArticolo)
                .Take(20)
                .Select(a => new
                {
                    id = a.CodiceArticolo,
                    text = a.CodiceArticolo + " - " + a.Descrizione,
                    descrizione = a.Descrizione,
                    unitaMisura = a.UnitaMisura
                })
                .ToListAsync();

            return Json(articoli);
        }

        [HttpGet]
        public async Task<IActionResult> DatiArticoloContabilita(string codice)
        {
            if (string.IsNullOrWhiteSpace(codice))
                return Json(null);

            var testo = codice.Trim();
            var articolo = await _context.AnagraficaArticoli
                .AsNoTracking()
                .Where(a => a.CodiceArticolo == testo)
                .Select(a => new
                {
                    id = a.CodiceArticolo,
                    text = a.CodiceArticolo + " - " + a.Descrizione,
                    descrizione = a.Descrizione,
                    unitaMisura = a.UnitaMisura
                })
                .FirstOrDefaultAsync();

            return Json(articolo);
        }

        private void ValidaScheda(CantiereSchedaViewModel model)
        {
            if (!model.CodiceCliente.HasValue || model.CodiceCliente.Value <= 0)
                ModelState.AddModelError(nameof(model.CodiceCliente), "Il cliente è obbligatorio.");

            if (model.DataInizioPrevista.HasValue
                && model.DataFinePrevista.HasValue
                && model.DataFinePrevista.Value < model.DataInizioPrevista.Value)
            {
                ModelState.AddModelError(nameof(model.DataFinePrevista),
                    "La data di fine non può essere precedente all'inizio.");
            }
        }

        private async Task<Cantiere?> CaricaCantiereConOrdiniAsync(int id)
        {
            return await _context.Cantieri
                .AsNoTracking()
                .Include(c => c.Ordini)
                    .ThenInclude(o => o.Ordine)
                .Include(c => c.Referenti)
                    .ThenInclude(r => r.Referente)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        private async Task<CantiereSchedaViewModel> MapToSchedaAsync(Cantiere cantiere)
        {
            return new CantiereSchedaViewModel
            {
                Id = cantiere.Id,
                Codice = cantiere.Codice,
                Nome = cantiere.Nome,
                CodiceCliente = cantiere.CodiceCliente,
                ClienteNome = await GetNomeClienteAsync(cantiere.CodiceCliente),
                CodiceDestinazione = cantiere.CodiceDestinazione,
                Indirizzo = cantiere.Indirizzo,
                Cap = cantiere.Cap,
                Localita = cantiere.Localita,
                Provincia = cantiere.Provincia,
                Telefono = cantiere.Telefono,
                ImpresaPosaId = cantiere.ImpresaPosaId,
                Referenti = MapReferenti(cantiere),
                DataInizioPrevista = cantiere.DataInizioPrevista,
                DataFinePrevista = cantiere.DataFinePrevista,
                Stato = cantiere.Stato,
                Note = cantiere.Note,
                PercorsoDocumenti = cantiere.PercorsoDocumenti,
                NomeAgente = await GetNomeAgenteAsync(cantiere.CodiceCliente),
                TipoTrasporto = cantiere.TipoTrasporto,
                PuliziaCantiere = cantiere.PuliziaCantiere,
                CheckAffidabilitaCliente = cantiere.CheckAffidabilitaCliente,
                Pagamento = cantiere.Pagamento,
                ConfermaFirmata = cantiere.ConfermaFirmata,
                TrasformatoInFavaro1 = cantiere.TrasformatoInFavaro1,
                MagazzinoN = cantiere.MagazzinoN,
                AccreditoAcconto = cantiere.AccreditoAcconto,
                AnagraficaSdiPec = cantiere.AnagraficaSdiPec,
                Banca = cantiere.Banca,
                AliquotaIva = cantiere.AliquotaIva,
                DocAgevolazioneIva = cantiere.DocAgevolazioneIva,
                ContrattoPosatoreFirmato = cantiere.ContrattoPosatoreFirmato,
                Psc = cantiere.Psc,
                PosFavaro1 = cantiere.PosFavaro1,
                FinePosaFirmato = cantiere.FinePosaFirmato,
                FinePosaData = cantiere.FinePosaData,
                ContabilitaCantiere = cantiere.ContabilitaCantiere,
                FatturaSaldoAttivo = cantiere.FatturaSaldoAttivo,
                FatturaSaldoPassivo = cantiere.FatturaSaldoPassivo,
                Ordini = MapOrdini(cantiere)
            };
        }

        private static void ApplicaChecklist(Cantiere cantiere, CantiereSchedaViewModel model)
        {
            cantiere.TipoTrasporto = NullIfEmpty(model.TipoTrasporto);
            cantiere.PuliziaCantiere = model.PuliziaCantiere;
            cantiere.CheckAffidabilitaCliente = model.CheckAffidabilitaCliente;
            cantiere.Pagamento = NullIfEmpty(model.Pagamento);
            cantiere.ConfermaFirmata = model.ConfermaFirmata;
            cantiere.TrasformatoInFavaro1 = model.TrasformatoInFavaro1;
            cantiere.MagazzinoN = NullIfEmpty(model.MagazzinoN);
            cantiere.AccreditoAcconto = model.AccreditoAcconto;
            cantiere.AnagraficaSdiPec = model.AnagraficaSdiPec;
            cantiere.Banca = NullIfEmpty(model.Banca);
            cantiere.AliquotaIva = model.AliquotaIva;
            cantiere.DocAgevolazioneIva = model.DocAgevolazioneIva;
            cantiere.ContrattoPosatoreFirmato = model.ContrattoPosatoreFirmato;
            cantiere.Psc = model.Psc;
            cantiere.PosFavaro1 = model.PosFavaro1;
            cantiere.FinePosaFirmato = model.FinePosaFirmato;
            cantiere.FinePosaData = model.FinePosaData;
            cantiere.ContabilitaCantiere = model.ContabilitaCantiere;
            cantiere.FatturaSaldoAttivo = model.FatturaSaldoAttivo;
            cantiere.FatturaSaldoPassivo = model.FatturaSaldoPassivo;
        }

        private static List<CantiereOrdineItemViewModel> MapOrdini(Cantiere cantiere)
        {
            return cantiere.Ordini
                .OrderByDescending(o => o.Ordine?.DataOrdine)
                .Select(o => new CantiereOrdineItemViewModel
                {
                    Id = o.Id,
                    OrdineTestataId = o.OrdineTestataId,
                    NumeroOrdine = o.Ordine?.NumeroOrdineCompleto ?? o.OrdineTestataId.ToString(),
                    DataOrdine = o.Ordine?.DataOrdine ?? DateTime.MinValue,
                    Riferimento = o.Ordine?.RiferimentoOrdine,
                    StatoEvasione = o.Ordine?.DescrizioneStatoEvasione ?? "",
                    StatoEvasioneCss = o.Ordine?.StatoEvasioneCssClass ?? "badge bg-secondary",
                    TrasportoPosa = o.Ordine?.TrasportoPosa == "S"
                })
                .ToList();
        }

        private static string TitoloScheda(CantiereSchedaViewModel scheda)
        {
            if (string.IsNullOrWhiteSpace(scheda.ClienteNome))
                return scheda.Nome;
            return $"{scheda.Nome} - {scheda.ClienteNome}";
        }

        private static List<CantiereReferenteItemViewModel> MapReferenti(Cantiere cantiere)
        {
            return cantiere.Referenti
                .OrderBy(r => r.Referente?.Nome)
                .Select(r => new CantiereReferenteItemViewModel
                {
                    Id = r.Id,
                    ReferenteId = r.ReferenteId,
                    Nome = r.Referente?.Nome ?? "",
                    Telefono = r.Referente?.Telefono,
                    Email = r.Referente?.Email,
                    Ruolo = r.Ruolo
                })
                .ToList();
        }

        private async Task CaricaListeSchedaAsync(CantiereStato? stato, int? impresaPosaId = null)
        {
            ViewBag.Stati = GetStatiSelectList(stato);
            ViewBag.ImpresePosa = await GetImpresePosaSelectListAsync(impresaPosaId);
        }

        private async Task<List<SelectListItem>> GetImpresePosaSelectListAsync(int? selezionataId)
        {
            return await _context.ImpresePosa
                .AsNoTracking()
                .Where(i => i.Attivo || (selezionataId.HasValue && i.Id == selezionataId.Value))
                .OrderBy(i => i.Nome)
                .Select(i => new SelectListItem
                {
                    Value = i.Id.ToString(),
                    Text = i.Nome,
                    Selected = selezionataId.HasValue && i.Id == selezionataId.Value
                })
                .ToListAsync();
        }

        private static List<SelectListItem> GetStatiSelectList(CantiereStato? selezionato)
        {
            return Enum.GetValues<CantiereStato>()
                .Select(s => new SelectListItem
                {
                    Value = ((int)s).ToString(),
                    Text = s.GetDisplayName(),
                    Selected = selezionato.HasValue && s == selezionato.Value
                })
                .ToList();
        }

        private async Task<string?> GetNomeAgenteAsync(int codiceCliente)
        {
            return await _context.AnagraficaClienti
                .AsNoTracking()
                .Where(c => c.CodiceCliente == codiceCliente)
                .Join(
                    _context.TabellaAgenti.AsNoTracking(),
                    c => c.CodiceAgente,
                    a => a.CodiceAgente,
                    (c, a) => a.DescrizioneAgente)
                .FirstOrDefaultAsync();
        }

        private async Task<string?> GetNomeClienteAsync(int codiceCliente)
        {
            var nome = await _context.AnagraficaClienti
                .AsNoTracking()
                .Where(c => c.CodiceCliente == codiceCliente)
                .Select(c => c.RagioneSociale)
                .FirstOrDefaultAsync();

            return nome == null ? null : $"{codiceCliente} - {nome}";
        }

        private string GetUtenteCorrente()
        {
            return User.Identity?.Name ?? "sistema";
        }

        private static string? NullIfEmpty(string? value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }

        private static string? NormalizzaProvincia(string? provincia)
        {
            var value = NullIfEmpty(provincia);
            return value?.ToUpperInvariant();
        }

        private async Task<ContabilitaCantiereViewModel?> CaricaContabilitaViewModelAsync(int cantiereId, TipoContabilitaCantiere tipo)
        {
            var cantiere = await _context.Cantieri
                .AsNoTracking()
                .Include(c => c.ImpresaPosa)
                .FirstOrDefaultAsync(c => c.Id == cantiereId);

            if (cantiere == null)
                return null;

            var testate = await _context.CantiereContabilita
                .AsNoTracking()
                .Include(c => c.Righe)
                .Where(c => c.CantiereId == cantiereId)
                .ToListAsync();

            var corrente = testate.FirstOrDefault(c => c.Tipo == tipo);
            var clienteNome = await GetNomeClienteAsync(cantiere.CodiceCliente);
            var titolo = string.IsNullOrWhiteSpace(clienteNome)
                ? cantiere.Nome
                : $"{cantiere.Nome} - {clienteNome}";

            var model = new ContabilitaCantiereViewModel
            {
                CantiereId = cantiere.Id,
                Titolo = titolo,
                ClienteNome = clienteNome,
                ImpresaPosa = cantiere.ImpresaPosa?.Nome,
                NomeAgente = await GetNomeAgenteAsync(cantiere.CodiceCliente),
                MagazzinoN = cantiere.MagazzinoN,
                Tipo = tipo,
                HaContabilitaIniziale = testate.Any(c => c.Tipo == TipoContabilitaCantiere.Iniziale && c.Righe.Count > 0),
                HaContabilitaFinale = testate.Any(c => c.Tipo == TipoContabilitaCantiere.Finale && c.Righe.Count > 0),
                NumeroOrdiniCollegati = await _context.CantiereOrdini.CountAsync(o => o.CantiereId == cantiereId),
                AliquotaIva = corrente?.AliquotaIva ?? cantiere.AliquotaIva ?? 22,
                ImponibileAcconto = corrente?.ImponibileAcconto ?? 0,
                RiferimentoOrdine = corrente?.RiferimentoOrdine,
                Note = corrente?.Note,
                Righe = CostruisciAlberoRighe(corrente?.Righe)
            };

            if (string.IsNullOrWhiteSpace(model.RiferimentoOrdine))
            {
                var riferimenti = await _context.CantiereOrdini
                    .AsNoTracking()
                    .Where(o => o.CantiereId == cantiereId)
                    .Select(o => o.Ordine)
                    .ToListAsync();

                if (riferimenti.Count > 0)
                {
                    model.RiferimentoOrdine = string.Join("; ", riferimenti
                        .Select(o => $"{o.NumeroOrdineCompleto} del {o.DataOrdine:dd/MM/yyyy}"));
                }
            }

            if (model.Righe.Count == 0)
                model.Righe.Add(new ContabilitaCantiereRigaViewModel());

            return model;
        }

        private async Task<CantiereContabilita> GetOrCreateContabilitaAsync(int cantiereId, TipoContabilitaCantiere tipo)
        {
            var testata = await _context.CantiereContabilita
                .FirstOrDefaultAsync(c => c.CantiereId == cantiereId && c.Tipo == tipo);

            if (testata != null)
                return testata;

            testata = new CantiereContabilita
            {
                CantiereId = cantiereId,
                Tipo = tipo,
                AliquotaIva = 22,
                DataUltimaModifica = DateTime.Now
            };
            _context.CantiereContabilita.Add(testata);
            await _context.SaveChangesAsync();
            return testata;
        }

        private async Task SostituisciRigheContabilitaAsync(int contabilitaId, List<ContabilitaCantiereRigaViewModel>? righe)
        {
            await EliminaRigheContabilitaAsync(contabilitaId);

            var ordinePadre = 1;
            foreach (var padreVm in righe ?? new List<ContabilitaCantiereRigaViewModel>())
            {
                var figliValidi = (padreVm.Figli ?? new List<ContabilitaCantiereRigaViewModel>())
                    .Where(f => !RigaContabilitaVuota(f))
                    .ToList();

                if (RigaContabilitaVuota(padreVm) && figliValidi.Count == 0)
                    continue;

                var padre = CreaRigaDaViewModel(padreVm, contabilitaId, null, ordinePadre++);
                _context.CantiereContabilitaRighe.Add(padre);
                await _context.SaveChangesAsync();

                var ordineFiglio = 1;
                foreach (var figlioVm in figliValidi)
                {
                    _context.CantiereContabilitaRighe.Add(
                        CreaRigaDaViewModel(figlioVm, contabilitaId, padre.Id, ordineFiglio++));
                }
            }
        }

        private async Task EliminaRigheContabilitaAsync(int contabilitaId)
        {
            var esistenti = await _context.CantiereContabilitaRighe
                .Where(r => r.ContabilitaId == contabilitaId)
                .ToListAsync();

            if (esistenti.Count == 0)
                return;

            _context.CantiereContabilitaRighe.RemoveRange(esistenti.Where(r => r.PadreId != null));
            await _context.SaveChangesAsync();
            _context.CantiereContabilitaRighe.RemoveRange(esistenti.Where(r => r.PadreId == null));
            await _context.SaveChangesAsync();
        }

        private static List<ContabilitaCantiereRigaViewModel> CostruisciAlberoRighe(ICollection<CantiereContabilitaRiga>? righe)
        {
            if (righe == null || righe.Count == 0)
                return new List<ContabilitaCantiereRigaViewModel>();

            var tutte = righe.OrderBy(r => r.Ordine).Select(MapRigaViewModel).ToList();
            var perPadre = tutte
                .Where(r => r.PadreId.HasValue)
                .GroupBy(r => r.PadreId!.Value)
                .ToDictionary(g => g.Key, g => g.ToList());

            var padri = tutte
                .Where(r => !r.PadreId.HasValue)
                .ToList();

            foreach (var padre in padri)
            {
                if (perPadre.TryGetValue(padre.Id, out var figli))
                    padre.Figli = figli;
            }

            return padri;
        }

        private static ContabilitaCantiereRigaViewModel MapRigaViewModel(CantiereContabilitaRiga riga)
        {
            return new ContabilitaCantiereRigaViewModel
            {
                Id = riga.Id,
                PadreId = riga.PadreId,
                CodiceArticolo = riga.CodiceArticolo,
                Descrizione = riga.Descrizione,
                UnitaMisura = riga.UnitaMisura,
                QuantitaPosata = riga.QuantitaPosata,
                CostoMaterialeServizio = riga.CostoMaterialeServizio,
                PrezzoVenditaCliente = riga.PrezzoVenditaCliente,
                Note = riga.Note
            };
        }

        private static CantiereContabilitaRiga CreaRigaDaViewModel(
            ContabilitaCantiereRigaViewModel riga,
            int contabilitaId,
            int? padreId,
            int ordine)
        {
            return new CantiereContabilitaRiga
            {
                ContabilitaId = contabilitaId,
                PadreId = padreId,
                Ordine = ordine,
                CodiceArticolo = NullIfEmpty(riga.CodiceArticolo),
                Descrizione = string.IsNullOrWhiteSpace(riga.Descrizione)
                    ? (riga.CodiceArticolo ?? string.Empty)
                    : riga.Descrizione.Trim(),
                UnitaMisura = NullIfEmpty(riga.UnitaMisura),
                QuantitaPosata = riga.QuantitaPosata,
                CostoMaterialeServizio = riga.CostoMaterialeServizio,
                PrezzoVenditaCliente = riga.PrezzoVenditaCliente,
                Note = NullIfEmpty(riga.Note)
            };
        }

        private static CantiereContabilitaRiga CopiaRigaContabilita(
            CantiereContabilitaRiga origine,
            int contabilitaId,
            int? padreId)
        {
            return new CantiereContabilitaRiga
            {
                ContabilitaId = contabilitaId,
                PadreId = padreId,
                Ordine = origine.Ordine,
                CodiceArticolo = origine.CodiceArticolo,
                Descrizione = origine.Descrizione,
                UnitaMisura = origine.UnitaMisura,
                QuantitaPosata = origine.QuantitaPosata,
                CostoMaterialeServizio = origine.CostoMaterialeServizio,
                PrezzoVenditaCliente = origine.PrezzoVenditaCliente,
                Note = origine.Note
            };
        }

        private static bool RigaContabilitaVuota(ContabilitaCantiereRigaViewModel riga)
        {
            return string.IsNullOrWhiteSpace(riga.CodiceArticolo)
                && string.IsNullOrWhiteSpace(riga.Descrizione)
                && riga.QuantitaPosata is null or 0
                && riga.CostoMaterialeServizio is null or 0
                && riga.PrezzoVenditaCliente is null or 0
                && string.IsNullOrWhiteSpace(riga.Note);
        }
    }
}
