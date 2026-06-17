using AiDbMaster.Attributes;
using AiDbMaster.Data;
using AiDbMaster.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace AiDbMaster.Controllers
{
    [Authorize]
    [RegisterResource("Indisponibilita", "Assenze e Fermi", Description = "Assenze autisti e indisponibilità mezzi", MenuIcon = "bi-calendar-x", MenuOrder = 6)]
    [RequirePermission("Indisponibilita", "View")]
    public class IndisponibilitaController : Controller
    {
        private readonly ApplicationDbContext _context;

        public IndisponibilitaController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string? tipo)
        {
            var query = _context.Indisponibilita
                .AsNoTracking()
                .Include(i => i.Autista)
                .Include(i => i.MezzoTrasporto)
                .AsQueryable();

            if (!string.IsNullOrEmpty(tipo))
                query = query.Where(i => i.Tipo == tipo);

            var lista = await query
                .OrderByDescending(i => i.DataInizio)
                .ThenBy(i => i.Tipo)
                .ToListAsync();

            ViewBag.FiltroTipo = tipo;
            return View(lista);
        }

        public IActionResult Calendario()
        {
            return View();
        }

        /// <summary>
        /// Restituisce le indisponibilità in formato evento per il calendario Syncfusion.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetEventi()
        {
            var lista = await _context.Indisponibilita
                .AsNoTracking()
                .Include(i => i.Autista)
                .Include(i => i.MezzoTrasporto)
                .ToListAsync();

            var eventi = lista.Select(i =>
            {
                var inizio = i.GiornoIntero
                    ? i.DataInizio.Date
                    : i.DataInizio.Date + (i.OraInizio ?? TimeSpan.Zero);
                var fine = i.GiornoIntero
                    ? i.DataFine.Date.AddDays(1).AddSeconds(-1)
                    : i.DataFine.Date + (i.OraFine ?? new TimeSpan(23, 59, 0));

                var soggetto = i.Tipo == TipoIndisponibilita.Mezzo
                    ? (i.MezzoTrasporto?.Descrizione ?? "Mezzo")
                    : (i.Autista != null ? $"{i.Autista.Cognome} {i.Autista.Nome}" : "Autista");

                return new
                {
                    id = i.Id,
                    subject = $"{soggetto} — {i.Causale}",
                    startTime = inizio,
                    endTime = fine,
                    isAllDay = i.GiornoIntero,
                    tipo = i.Tipo,
                    soggetto,
                    causale = i.Causale,
                    note = i.Note ?? "",
                    color = i.Tipo == TipoIndisponibilita.Mezzo ? "#c92a2a" : "#1971c2"
                };
            });

            return Json(eventi);
        }

        [RequirePermission("Indisponibilita", "Create")]
        public async Task<IActionResult> Create(DateTime? data)
        {
            await PopolaSelectAsync();
            var giorno = data?.Date ?? DateTime.Today;
            return View(new Indisponibilita
            {
                Tipo = TipoIndisponibilita.Mezzo,
                DataInizio = giorno,
                DataFine = giorno,
                GiornoIntero = true
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission("Indisponibilita", "Create")]
        public async Task<IActionResult> Create(Indisponibilita model)
        {
            Normalizza(model);
            ValidaModello(model);

            if (!ModelState.IsValid)
            {
                await PopolaSelectAsync();
                return View(model);
            }

            model.CreatoDa = User.Identity?.Name;
            model.DataCreazione = DateTime.Now;
            _context.Indisponibilita.Add(model);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Indisponibilità registrata con successo.";
            return RedirectToAction(nameof(Index));
        }

        [RequirePermission("Indisponibilita", "Edit")]
        public async Task<IActionResult> Edit(int id)
        {
            var item = await _context.Indisponibilita.FindAsync(id);
            if (item == null)
            {
                TempData["ErrorMessage"] = "Indisponibilità non trovata.";
                return RedirectToAction(nameof(Index));
            }
            await PopolaSelectAsync();
            return View(item);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission("Indisponibilita", "Edit")]
        public async Task<IActionResult> Edit(int id, Indisponibilita model)
        {
            var existing = await _context.Indisponibilita.FindAsync(id);
            if (existing == null)
            {
                TempData["ErrorMessage"] = "Indisponibilità non trovata.";
                return RedirectToAction(nameof(Index));
            }

            Normalizza(model);
            ValidaModello(model);

            if (!ModelState.IsValid)
            {
                await PopolaSelectAsync();
                model.Id = id;
                return View(model);
            }

            existing.Tipo = model.Tipo;
            existing.AutistaId = model.AutistaId;
            existing.MezzoTrasportoId = model.MezzoTrasportoId;
            existing.DataInizio = model.DataInizio;
            existing.DataFine = model.DataFine;
            existing.GiornoIntero = model.GiornoIntero;
            existing.OraInizio = model.OraInizio;
            existing.OraFine = model.OraFine;
            existing.Causale = model.Causale;
            existing.Note = model.Note;

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Indisponibilità aggiornata con successo.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission("Indisponibilita", "Delete")]
        public async Task<IActionResult> Delete(int id)
        {
            var item = await _context.Indisponibilita.FindAsync(id);
            if (item == null)
            {
                TempData["ErrorMessage"] = "Indisponibilità non trovata.";
                return RedirectToAction(nameof(Index));
            }

            _context.Indisponibilita.Remove(item);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Indisponibilità eliminata con successo.";
            return RedirectToAction(nameof(Index));
        }

        // ===== Helper =====

        private static void Normalizza(Indisponibilita model)
        {
            if (model.Tipo == TipoIndisponibilita.Mezzo)
            {
                model.AutistaId = null;
            }
            else
            {
                model.MezzoTrasportoId = null;
            }

            if (model.GiornoIntero)
            {
                model.OraInizio = null;
                model.OraFine = null;
            }
        }

        private void ValidaModello(Indisponibilita model)
        {
            if (model.Tipo == TipoIndisponibilita.Mezzo && !model.MezzoTrasportoId.HasValue)
                ModelState.AddModelError(nameof(model.MezzoTrasportoId), "Selezionare il mezzo.");

            if (model.Tipo == TipoIndisponibilita.Autista && !model.AutistaId.HasValue)
                ModelState.AddModelError(nameof(model.AutistaId), "Selezionare l'autista.");

            if (model.DataFine.Date < model.DataInizio.Date)
                ModelState.AddModelError(nameof(model.DataFine), "La data fine non può essere precedente alla data inizio.");

            if (string.IsNullOrWhiteSpace(model.Causale))
                ModelState.AddModelError(nameof(model.Causale), "Selezionare una causale.");

            if (!model.GiornoIntero)
            {
                if (!model.OraInizio.HasValue || !model.OraFine.HasValue)
                    ModelState.AddModelError(nameof(model.OraInizio), "Indicare ora inizio e ora fine, oppure selezionare 'Giornata intera'.");
                else if (model.OraFine.Value <= model.OraInizio.Value)
                    ModelState.AddModelError(nameof(model.OraFine), "L'ora fine deve essere successiva all'ora inizio.");
            }
        }

        private async Task PopolaSelectAsync()
        {
            ViewBag.Mezzi = await _context.MezziTrasporto.AsNoTracking()
                .Where(m => m.Attivo)
                .OrderBy(m => m.Descrizione)
                .Select(m => new SelectListItem { Value = m.Id.ToString(), Text = m.Descrizione })
                .ToListAsync();

            ViewBag.Autisti = await _context.Autisti.AsNoTracking()
                .Where(a => a.Attivo)
                .OrderBy(a => a.Cognome).ThenBy(a => a.Nome)
                .Select(a => new SelectListItem { Value = a.Id.ToString(), Text = a.Cognome + " " + a.Nome })
                .ToListAsync();

            ViewBag.CausaliMezzo = CausaliIndisponibilita.PerMezzo;
            ViewBag.CausaliAutista = CausaliIndisponibilita.PerAutista;
        }
    }
}
