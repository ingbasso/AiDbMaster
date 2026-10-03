using AiDbMaster.Attributes;
using AiDbMaster.Data;
using AiDbMaster.Models;
using AiDbMaster.Services;
using AiDbMaster.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.StaticFiles;
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
        private readonly CantieriDocumentiService _documenti;
        private readonly IWebHostEnvironment _env;

        public GestioneCantieriController(
            ApplicationDbContext context,
            ILogger<GestioneCantieriController> logger,
            CantieriDocumentiService documenti,
            IWebHostEnvironment env)
        {
            _context = context;
            _logger = logger;
            _documenti = documenti;
            _env = env;
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
            await CompletaDocumentiAsync(scheda, cantiere.Codice);
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
                await CompletaDocumentiAsync(model, cantiereError.Codice);
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

        /// <summary>
        /// PDF della proforma. Legge solo la contabilità già salvata: le modifiche ancora
        /// aperte in pagina, e non confermate con Salva, non entrano nel documento.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Proforma(int id, TipoContabilitaCantiere tipo = TipoContabilitaCantiere.Iniziale)
        {
            var model = await CaricaContabilitaViewModelAsync(id, tipo);
            if (model == null)
                return NotFound();

            var cantiere = await _context.Cantieri
                .AsNoTracking()
                .FirstAsync(c => c.Id == id);

            var cliente = await _context.AnagraficaClienti
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.CodiceCliente == cantiere.CodiceCliente);

            model.RiferimentoOrdine = await _context.CantiereContabilita
                .AsNoTracking()
                .Where(c => c.CantiereId == id && c.Tipo == tipo)
                .Select(c => c.RiferimentoOrdine)
                .FirstOrDefaultAsync();

            var immagine = Path.Combine(_env.WebRootPath, "images", "logo-favaro1-wordmark.png");
            var pdf = ProformaContabilitaPdf.Crea(model, cantiere, cliente, immagine, DateTime.Today);
            var nomeFile = NomeFileProforma(cantiere.Codice, tipo);
            Response.Headers["Content-Disposition"] = $"inline; filename=\"{nomeFile}\"";
            return File(pdf, "application/pdf");
        }

        /// <summary>
        /// Excel della griglia. Come la proforma, usa solo la contabilità già salvata.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> EsportaExcel(int id, TipoContabilitaCantiere tipo = TipoContabilitaCantiere.Iniziale)
        {
            var model = await CaricaContabilitaViewModelAsync(id, tipo);
            if (model == null)
                return NotFound();

            var cantiere = await _context.Cantieri
                .AsNoTracking()
                .FirstAsync(c => c.Id == id);

            model.RiferimentoOrdine = await _context.CantiereContabilita
                .AsNoTracking()
                .Where(c => c.CantiereId == id && c.Tipo == tipo)
                .Select(c => c.RiferimentoOrdine)
                .FirstOrDefaultAsync();

            var excel = ContabilitaExcel.Crea(model, cantiere.Nome);
            var nomeFile = NomeFileExcel(cantiere.Codice, tipo);
            return File(excel, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", nomeFile);
        }

        private static string NomeFileExcel(string codice, TipoContabilitaCantiere tipo)
        {
            var caratteriVietati = Path.GetInvalidFileNameChars();
            var codicePulito = new string(codice.Select(c => caratteriVietati.Contains(c) ? '_' : c).ToArray());
            if (string.IsNullOrWhiteSpace(codicePulito))
                codicePulito = "cantiere";
            return $"Contabilita_{codicePulito}_{tipo}_{DateTime.Today:yyyyMMdd}.xlsx";
        }

        private static string NomeFileProforma(string codice, TipoContabilitaCantiere tipo)
        {
            var caratteriVietati = Path.GetInvalidFileNameChars();
            var codicePulito = new string(codice.Select(c => caratteriVietati.Contains(c) ? '_' : c).ToArray());
            if (string.IsNullOrWhiteSpace(codicePulito))
                codicePulito = "cantiere";
            return $"Proforma_{codicePulito}_{tipo}_{DateTime.Today:yyyyMMdd}.pdf";
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
            testata.RiferimentoOrdine = NullIfEmpty(model.RiferimentoOrdine);
            testata.Note = NullIfEmpty(model.Note);
            testata.DataUltimaModifica = DateTime.Now;

            await SostituisciAccontiAsync(testata.Id, model.Acconti);
            testata.ImponibileAcconto = model.ImponibileAcconto;
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
                .Include(c => c.Acconti)
                .FirstOrDefaultAsync(c => c.CantiereId == id && c.Tipo == TipoContabilitaCantiere.Iniziale);

            if (iniziale == null)
            {
                TempData["ErrorMessage"] = "Non esiste ancora una contabilità iniziale da copiare.";
                return RedirectToAction(nameof(Contabilita), new { id, tipo = TipoContabilitaCantiere.Finale });
            }

            var finale = await GetOrCreateContabilitaAsync(id, TipoContabilitaCantiere.Finale);
            finale.AliquotaIva = iniziale.AliquotaIva;
            finale.RiferimentoOrdine = iniziale.RiferimentoOrdine;
            finale.Note = iniziale.Note;
            finale.DataUltimaModifica = DateTime.Now;

            await EliminaRigheContabilitaAsync(finale.Id);
            await SostituisciAccontiAsync(finale.Id, iniziale.Acconti
                .OrderBy(a => a.Ordine)
                .Select(a => new ContabilitaAccontoViewModel
                {
                    Descrizione = a.Descrizione,
                    Imponibile = a.Imponibile
                })
                .ToList());
            finale.ImponibileAcconto = iniziale.Acconti.Sum(a => a.Imponibile);

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

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission("GestioneCantieri", "Edit")]
        [RequestSizeLimit(CantieriDocumentiService.DimensioneMassimaFile * 4)]
        [RequestFormLimits(MultipartBodyLengthLimit = CantieriDocumentiService.DimensioneMassimaFile * 4)]
        public async Task<IActionResult> CaricaDocumenti(int id, List<IFormFile>? files)
        {
            var cantiere = await _context.Cantieri.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id);
            if (cantiere == null)
                return Json(new { success = false, message = "Cantiere non trovato." });

            if (files == null || files.Count == 0)
                return Json(new { success = false, message = "Seleziona almeno un file." });

            var caricati = new List<string>();
            var errori = new List<string>();
            foreach (var file in files)
            {
                var risultato = await _documenti.SalvaFileAsync(cantiere.Codice, file);
                if (risultato.Ok && risultato.NomeSalvato != null)
                    caricati.Add(risultato.NomeSalvato);
                else
                    errori.Add($"{file.FileName}: {risultato.Messaggio}");
            }

            var cartella = await _documenti.CaricaCartellaAsync(cantiere.Codice);
            return Json(new
            {
                success = errori.Count == 0,
                message = errori.Count == 0
                    ? $"Caricati {caricati.Count} file."
                    : string.Join(" ", errori),
                files = cartella.File.Select(MapDocumentoJson)
            });
        }

        [HttpGet]
        public async Task<IActionResult> ScaricaDocumento(int id, string nome)
        {
            var cantiere = await _context.Cantieri.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id);
            if (cantiere == null)
                return NotFound();

            var risultato = await _documenti.PercorsoDownloadAsync(cantiere.Codice, nome);
            if (!risultato.Ok || string.IsNullOrWhiteSpace(risultato.Percorso))
                return NotFound();

            var provider = new FileExtensionContentTypeProvider();
            if (!provider.TryGetContentType(risultato.Percorso, out var contentType))
                contentType = "application/octet-stream";

            return PhysicalFile(risultato.Percorso, contentType, Path.GetFileName(risultato.Percorso));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission("GestioneCantieri", "Edit")]
        public async Task<IActionResult> EliminaDocumento(int id, string nome)
        {
            var cantiere = await _context.Cantieri.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id);
            if (cantiere == null)
                return Json(new { success = false, message = "Cantiere non trovato." });

            var risultato = await _documenti.EliminaFileAsync(cantiere.Codice, nome);
            var cartella = await _documenti.CaricaCartellaAsync(cantiere.Codice);
            return Json(new
            {
                success = risultato.Ok,
                message = risultato.Messaggio,
                files = cartella.File.Select(MapDocumentoJson)
            });
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

        private async Task CompletaDocumentiAsync(CantiereSchedaViewModel scheda, string codice)
        {
            var cartella = await _documenti.CaricaCartellaAsync(codice);
            scheda.CartellaDocumenti = cartella.Percorso;
            scheda.ErroreDocumenti = cartella.Errore;
            scheda.Documenti = cartella.File;
        }

        private static object MapDocumentoJson(CantiereDocumentoItemViewModel file)
        {
            return new
            {
                nome = file.Nome,
                dimensione = FormattaDimensione(file.Dimensione),
                data = file.DataModifica.ToString("dd/MM/yyyy HH:mm")
            };
        }

        private static string FormattaDimensione(long bytes)
        {
            if (bytes < 1024)
                return $"{bytes} B";
            if (bytes < 1024 * 1024)
                return $"{bytes / 1024.0:0.#} KB";
            return $"{bytes / (1024.0 * 1024.0):0.#} MB";
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
                .Include(c => c.Acconti)
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
                Acconti = MappaAcconti(corrente),
                RiferimentoOrdine = corrente?.RiferimentoOrdine,
                Note = corrente?.Note,
                Righe = CostruisciAlberoRighe(corrente?.Righe),
                CostiArticoli = await _context.CostiArticoliCantiere
                    .AsNoTracking()
                    .OrderBy(c => c.CodiceArticolo)
                    .Select(c => new CostoArticoloCantiereVoce
                    {
                        Codice = c.CodiceArticolo,
                        Costo = c.CostoUnitario,
                        PrezzoMedio = c.PrezzoMedioVendita
                    })
                    .ToListAsync()
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

            if (tipo == TipoContabilitaCantiere.Finale)
            {
                var iniziale = testate.FirstOrDefault(c => c.Tipo == TipoContabilitaCantiere.Iniziale);
                var alberoIniziale = CostruisciAlberoRighe(iniziale?.Righe);
                if (alberoIniziale.Count > 0)
                {
                    model.ConfrontaIniziale = true;
                    model.TotaleCostiIniziale = SommaCostiAlbero(alberoIniziale);
                    model.TotaleVenditaIniziale = alberoIniziale.Sum(r => r.TotaleVendita);
                    model.RigheSoloIniziale = CollegaConfrontoIniziale(model.Righe, alberoIniziale);
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

        private static List<ContabilitaAccontoViewModel> MappaAcconti(CantiereContabilita? testata)
        {
            var lista = testata?.Acconti
                .OrderBy(a => a.Ordine)
                .Select(a => new ContabilitaAccontoViewModel
                {
                    Id = a.Id,
                    Descrizione = a.Descrizione,
                    Imponibile = a.Imponibile
                })
                .ToList() ?? new List<ContabilitaAccontoViewModel>();

            if (lista.Count == 0 && testata?.ImponibileAcconto is > 0)
            {
                lista.Add(new ContabilitaAccontoViewModel
                {
                    Descrizione = "Acconto",
                    Imponibile = testata.ImponibileAcconto
                });
            }

            return lista;
        }

        private async Task SostituisciAccontiAsync(int contabilitaId, List<ContabilitaAccontoViewModel>? acconti)
        {
            var esistenti = await _context.CantiereContabilitaAcconti
                .Where(a => a.ContabilitaId == contabilitaId)
                .ToListAsync();
            if (esistenti.Count > 0)
                _context.CantiereContabilitaAcconti.RemoveRange(esistenti);

            var ordine = 1;
            foreach (var acconto in acconti ?? new List<ContabilitaAccontoViewModel>())
            {
                if (string.IsNullOrWhiteSpace(acconto.Descrizione) && acconto.Imponibile == 0)
                    continue;

                _context.CantiereContabilitaAcconti.Add(new CantiereContabilitaAcconto
                {
                    ContabilitaId = contabilitaId,
                    Ordine = ordine++,
                    Descrizione = NullIfEmpty(acconto.Descrizione),
                    Imponibile = acconto.Imponibile
                });
            }
        }

        private async Task SostituisciRigheContabilitaAsync(int contabilitaId, List<ContabilitaCantiereRigaViewModel>? righe)
        {
            await EliminaRigheContabilitaAsync(contabilitaId);

            var ordinePadre = 1;
            foreach (var padreVm in righe ?? new List<ContabilitaCantiereRigaViewModel>())
            {
                if (padreVm.IsSconto)
                {
                    NormalizzaSconto(padreVm);
                    var sconto = CreaRigaDaViewModel(padreVm, contabilitaId, null, ordinePadre++);
                    _context.CantiereContabilitaRighe.Add(sconto);
                    await _context.SaveChangesAsync();
                    continue;
                }

                if (padreVm.IsArticoloSingolo)
                {
                    if (RigaContabilitaVuota(padreVm))
                        continue;

                    padreVm.IsPosa = false;
                    padreVm.IsSconto = false;
                    padreVm.Figli = new List<ContabilitaCantiereRigaViewModel>();
                    var articolo = CreaRigaDaViewModel(padreVm, contabilitaId, null, ordinePadre++);
                    _context.CantiereContabilitaRighe.Add(articolo);
                    await _context.SaveChangesAsync();
                    continue;
                }

                var figliValidi = AllineaRigaPosa(padreVm, padreVm.Figli);
                figliValidi = figliValidi
                    .Where(f => f.IsPosa || !RigaContabilitaVuota(f))
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

        private static decimal SommaCostiAlbero(List<ContabilitaCantiereRigaViewModel> padri)
        {
            return padri.Sum(p => p.TotaleCosto + p.Figli.Sum(f => f.TotaleCosto));
        }

        /// <summary>
        /// Accoppia le righe finali a quelle iniziali: padre con lo stesso codice,
        /// figlio con lo stesso codice sotto quel padre, posa con la posa del padre.
        /// </summary>
        private static List<ContabilitaCantiereRigaViewModel> CollegaConfrontoIniziale(
            List<ContabilitaCantiereRigaViewModel> finali,
            List<ContabilitaCantiereRigaViewModel> iniziali)
        {
            var usatiPadri = new HashSet<ContabilitaCantiereRigaViewModel>();
            foreach (var padre in finali)
            {
                if (RigaContabilitaVuota(padre))
                    continue;

                var origine = TrovaCorrispondenza(iniziali, usatiPadri, padre);
                if (origine == null)
                {
                    padre.SoloFinale = true;
                    foreach (var figlio in padre.Figli.Where(f => !RigaContabilitaVuota(f)))
                        figlio.SoloFinale = true;
                    continue;
                }

                usatiPadri.Add(origine);
                CopiaNumeriConfronto(padre, origine);
                CollegaFigliConfronto(padre, origine);
            }

            return iniziali
                .Where(p => !usatiPadri.Contains(p) && !RigaContabilitaVuota(p))
                .Select(PreparaPadreMancante)
                .ToList();
        }

        private static void CollegaFigliConfronto(
            ContabilitaCantiereRigaViewModel padreFinale,
            ContabilitaCantiereRigaViewModel padreIniziale)
        {
            var usati = new HashSet<ContabilitaCantiereRigaViewModel>();
            var posaFinale = padreFinale.Figli.FirstOrDefault(f => f.IsPosa);
            var posaIniziale = padreIniziale.Figli.FirstOrDefault(f => f.IsPosa);

            if (posaFinale != null && posaIniziale != null)
            {
                CopiaNumeriConfronto(posaFinale, posaIniziale);
                usati.Add(posaIniziale);
            }
            else if (posaFinale != null)
            {
                posaFinale.SoloFinale = true;
            }
            else if (posaIniziale != null)
            {
                padreFinale.HaConfrontoPosa = true;
                padreFinale.InizialePosaQuantita = posaIniziale.QuantitaPosata;
                padreFinale.InizialePosaCostoUnitario = posaIniziale.CostoMaterialeServizio;
                padreFinale.InizialePosaPrezzoUnitario = posaIniziale.PrezzoVenditaCliente;
                usati.Add(posaIniziale);
            }

            foreach (var figlio in padreFinale.Figli.Where(f => !f.IsPosa))
            {
                if (RigaContabilitaVuota(figlio))
                    continue;

                var origine = TrovaCorrispondenza(padreIniziale.Figli, usati, figlio);
                if (origine == null)
                {
                    figlio.SoloFinale = true;
                    continue;
                }

                usati.Add(origine);
                CopiaNumeriConfronto(figlio, origine);
            }

            foreach (var origine in padreIniziale.Figli.Where(f => !usati.Contains(f) && !RigaContabilitaVuota(f)))
            {
                var copia = new ContabilitaCantiereRigaViewModel
                {
                    CodiceArticolo = origine.CodiceArticolo,
                    Descrizione = origine.Descrizione,
                    UnitaMisura = origine.UnitaMisura,
                    IsPosa = origine.IsPosa
                };
                CopiaNumeriConfronto(copia, origine);
                padreFinale.Mancanti.Add(copia);
            }
        }

        private static ContabilitaCantiereRigaViewModel PreparaPadreMancante(ContabilitaCantiereRigaViewModel origine)
        {
            CopiaNumeriConfronto(origine, origine);
            foreach (var figlio in origine.Figli.Where(f => !RigaContabilitaVuota(f)))
                CopiaNumeriConfronto(figlio, figlio);

            origine.Figli = origine.Figli.Where(f => f.HaConfronto).ToList();
            return origine;
        }

        private static ContabilitaCantiereRigaViewModel? TrovaCorrispondenza(
            IEnumerable<ContabilitaCantiereRigaViewModel> candidati,
            HashSet<ContabilitaCantiereRigaViewModel> usati,
            ContabilitaCantiereRigaViewModel cercata)
        {
            if (cercata.IsSconto)
                return candidati.FirstOrDefault(c => !usati.Contains(c) && c.IsSconto);

            if (cercata.IsArticoloSingolo)
            {
                var chiaveArticolo = ChiaveRiga(cercata);
                if (chiaveArticolo.Length == 0)
                    return null;
                return candidati.FirstOrDefault(c =>
                    !usati.Contains(c) && c.IsArticoloSingolo && ChiaveRiga(c) == chiaveArticolo);
            }

            if (cercata.IsPosa)
                return candidati.FirstOrDefault(c => !usati.Contains(c) && c.IsPosa);

            var chiave = ChiaveRiga(cercata);
            if (chiave.Length == 0)
                return null;

            return candidati.FirstOrDefault(c =>
                !usati.Contains(c) && !c.IsPosa && !c.IsSconto && !c.IsArticoloSingolo && ChiaveRiga(c) == chiave);
        }

        private static string ChiaveRiga(ContabilitaCantiereRigaViewModel riga)
        {
            var codice = (riga.CodiceArticolo ?? string.Empty).Trim();
            if (codice.Length > 0)
                return "C:" + codice.ToUpperInvariant();

            var descrizione = (riga.Descrizione ?? string.Empty).Trim();
            if (descrizione.Length > 0)
                return "D:" + descrizione.ToUpperInvariant();

            return string.Empty;
        }

        private static void CopiaNumeriConfronto(
            ContabilitaCantiereRigaViewModel destinazione,
            ContabilitaCantiereRigaViewModel origine)
        {
            destinazione.HaConfronto = true;
            destinazione.InizialeQuantita = origine.QuantitaPosata;
            destinazione.InizialeCostoUnitario = origine.CostoMaterialeServizio;
            destinazione.InizialePrezzoUnitario = origine.PrezzoVenditaCliente;
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
                {
                    var posa = figli.FirstOrDefault(f => f.IsPosa)
                        ?? figli.FirstOrDefault(f => ÈDescrizionePosa(f.Descrizione));
                    if (posa != null)
                        posa.IsPosa = true;
                    padre.Figli = figli
                        .OrderBy(f => f.IsPosa)
                        .ToList();
                }
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
                RicaricoPercentuale = riga.RicaricoPercentuale,
                PrezzoVenditaCliente = riga.PrezzoVenditaCliente,
                Note = riga.Note,
                IsPosa = riga.IsPosa,
                IsSconto = riga.IsSconto,
                IsArticoloSingolo = riga.IsArticoloSingolo,
                MostraInProforma = riga.MostraInProforma
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
                RicaricoPercentuale = riga.RicaricoPercentuale,
                PrezzoVenditaCliente = riga.PrezzoVenditaCliente,
                Note = NullIfEmpty(riga.Note),
                IsPosa = riga.IsSconto ? false : riga.IsPosa,
                IsSconto = riga.IsSconto && padreId == null,
                IsArticoloSingolo = riga.IsArticoloSingolo && padreId == null && !riga.IsSconto,
                MostraInProforma = riga.MostraInProforma
            };
        }

        private static void NormalizzaSconto(ContabilitaCantiereRigaViewModel riga)
        {
            riga.IsSconto = true;
            riga.IsPosa = false;
            riga.Figli = new List<ContabilitaCantiereRigaViewModel>();
            riga.CodiceArticolo = null;
            riga.UnitaMisura = null;
            riga.QuantitaPosata = null;
            riga.CostoMaterialeServizio = null;
            riga.RicaricoPercentuale = null;
            if (string.IsNullOrWhiteSpace(riga.Descrizione))
                riga.Descrizione = "Sconto";
            if (riga.PrezzoVenditaCliente.HasValue)
                riga.PrezzoVenditaCliente = Math.Abs(riga.PrezzoVenditaCliente.Value);
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
                RicaricoPercentuale = origine.RicaricoPercentuale,
                PrezzoVenditaCliente = origine.PrezzoVenditaCliente,
                Note = origine.Note,
                IsPosa = origine.IsPosa,
                IsSconto = origine.IsSconto,
                IsArticoloSingolo = origine.IsArticoloSingolo,
                MostraInProforma = origine.MostraInProforma
            };
        }

        private static List<ContabilitaCantiereRigaViewModel> AllineaRigaPosa(
            ContabilitaCantiereRigaViewModel padre,
            List<ContabilitaCantiereRigaViewModel>? figli)
        {
            var lista = figli ?? new List<ContabilitaCantiereRigaViewModel>();
            var posa = lista.FirstOrDefault(f => f.IsPosa)
                ?? lista.FirstOrDefault(f => ÈDescrizionePosa(f.Descrizione));

            if (posa == null)
            {
                posa = new ContabilitaCantiereRigaViewModel();
                lista.Add(posa);
            }

            padre.CostoMaterialeServizio = null;

            posa.IsPosa = true;
            posa.IsArticoloMateriale = false;
            if (string.IsNullOrWhiteSpace(posa.Descrizione))
                posa.Descrizione = "Posa";
            posa.QuantitaPosata ??= padre.QuantitaPosata;
            posa.UnitaMisura = padre.UnitaMisura;

            var materiale = lista.FirstOrDefault(f => !f.IsPosa && f.IsArticoloMateriale)
                ?? lista.FirstOrDefault(f => !f.IsPosa && StessoCodice(f.CodiceArticolo, padre.CodiceArticolo));
            if (materiale == null)
            {
                materiale = new ContabilitaCantiereRigaViewModel();
                lista.Insert(0, materiale);
            }

            materiale.IsPosa = false;
            materiale.IsArticoloMateriale = true;
            materiale.CodiceArticolo = padre.CodiceArticolo;
            materiale.Descrizione = padre.Descrizione ?? string.Empty;
            materiale.UnitaMisura = padre.UnitaMisura;
            materiale.QuantitaPosata ??= padre.QuantitaPosata;

            foreach (var figlio in lista.Where(f => !f.IsPosa && !f.IsArticoloMateriale))
                AllineaPrezzoERicarico(padre, figlio);

            AllineaPrezzoERicarico(padre, materiale);
            AllineaPrezzoERicarico(padre, posa);

            return lista
                .OrderBy(f => f.IsPosa ? 2 : f.IsArticoloMateriale ? 0 : 1)
                .ToList();
        }

        private static bool StessoCodice(string? a, string? b)
        {
            if (string.IsNullOrWhiteSpace(a) || string.IsNullOrWhiteSpace(b))
                return false;
            return string.Equals(a.Trim(), b.Trim(), StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Se l'utente ha scritto il prezzo, quello resta e il ricarico si ricava.
        /// Altrimenti resta il ricarico (30 se la cella è vuota) e si ricava il prezzo.
        /// </summary>
        private static void AllineaPrezzoERicarico(
            ContabilitaCantiereRigaViewModel padre,
            ContabilitaCantiereRigaViewModel figlio)
        {
            if (figlio.PrezzoDaPrezzo
                && figlio.PrezzoVenditaCliente.HasValue
                && figlio.CostoMaterialeServizio is > 0
                && figlio.QuantitaPosata is > 0
                && padre.QuantitaPosata is > 0)
            {
                var prezzo = Math.Round(figlio.PrezzoVenditaCliente.Value, 2, MidpointRounding.AwayFromZero);
                var baseCosto = figlio.QuantitaPosata.Value * figlio.CostoMaterialeServizio.Value;
                figlio.PrezzoVenditaCliente = prezzo;
                figlio.RicaricoPercentuale = Math.Round(
                    (prezzo * padre.QuantitaPosata.Value / baseCosto - 1m) * 100m,
                    4,
                    MidpointRounding.AwayFromZero);
                return;
            }

            figlio.RicaricoPercentuale ??= 30m;
            figlio.PrezzoVenditaCliente = PrezzoVenditaRipartito(padre, figlio);
        }

        /// <summary>
        /// Il costo del figlio è per la sua unità (MC, sacchetti, rotoli).
        /// Il prezzo vendita è quel costo, con ricarico, diviso per la quantità del padre:
        /// così è confrontabile con il prezzo vendita del padre.
        /// </summary>
        private static decimal? PrezzoVenditaRipartito(
            ContabilitaCantiereRigaViewModel padre,
            ContabilitaCantiereRigaViewModel figlio)
        {
            if (figlio.CostoMaterialeServizio is null
                || figlio.RicaricoPercentuale is null
                || figlio.QuantitaPosata is null
                || padre.QuantitaPosata is not > 0)
                return null;

            var importoConRicarico = figlio.QuantitaPosata.Value
                * figlio.CostoMaterialeServizio.Value
                * (1 + figlio.RicaricoPercentuale.Value / 100m);
            return Math.Round(importoConRicarico / padre.QuantitaPosata.Value, 2, MidpointRounding.AwayFromZero);
        }

        private static bool ÈDescrizionePosa(string? descrizione)
        {
            var testo = (descrizione ?? string.Empty).Trim();
            return testo.Equals("Posa", StringComparison.OrdinalIgnoreCase)
                || testo.StartsWith("Posa ", StringComparison.OrdinalIgnoreCase);
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
