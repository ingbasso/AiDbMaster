using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using AiDbMaster.Data;
using AiDbMaster.Models;
using Microsoft.AspNetCore.Authorization;

namespace AiDbMaster.Controllers
{
    /// <summary>
    /// Controller per la gestione degli ordini di produzione
    /// </summary>
    [Authorize]
    public class ListaOPController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ListaOPController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Visualizza la lista degli ordini di produzione con filtri
        /// </summary>
        public async Task<IActionResult> Index(string? filtroStato, string? filtroOperatore, string? filtroCentro, string? filtroLavorazione, string? filtroArticolo, int? filtroPriorita)
        {
            ViewBag.Title = "Gestione Ordini di Produzione";
            
            // Prepara i dati per i filtri
            await PrepareFilterData();
            
            // Query base con tutte le relazioni
            var query = _context.ListaOP
                .Include(l => l.Stato)
                .Include(l => l.Operatore)
                .Include(l => l.CentroLavoro)
                .Include(l => l.Lavorazione)
                .AsQueryable();

            // Applica filtri
            if (!string.IsNullOrEmpty(filtroStato))
            {
                query = query.Where(l => l.Stato!.CodiceStato == filtroStato);
                ViewBag.FiltroStato = filtroStato;
            }

            if (!string.IsNullOrEmpty(filtroOperatore))
            {
                if (int.TryParse(filtroOperatore, out int operatoreId))
                {
                    query = query.Where(l => l.IdOperatore == operatoreId);
                    ViewBag.FiltroOperatore = filtroOperatore;
                }
            }

            if (!string.IsNullOrEmpty(filtroCentro))
            {
                if (int.TryParse(filtroCentro, out int centroId))
                {
                    query = query.Where(l => l.IdCentroLavoro == centroId);
                    ViewBag.FiltroCentro = filtroCentro;
                }
            }

            if (!string.IsNullOrEmpty(filtroLavorazione))
            {
                if (int.TryParse(filtroLavorazione, out int lavorazioneId))
                {
                    query = query.Where(l => l.IdLavorazione == lavorazioneId);
                    ViewBag.FiltroLavorazione = filtroLavorazione;
                }
            }

            if (!string.IsNullOrEmpty(filtroArticolo))
            {
                query = query.Where(l => l.CodiceArticolo.Contains(filtroArticolo) || 
                                       l.DescrizioneArticolo.Contains(filtroArticolo));
                ViewBag.FiltroArticolo = filtroArticolo;
            }

            if (filtroPriorita.HasValue)
            {
                query = query.Where(l => l.Priorita == filtroPriorita);
                ViewBag.FiltroPriorita = filtroPriorita;
            }

            // Ordina per data inizio (più recenti prima)
            var ordini = await query
                .OrderByDescending(l => l.DataInizioOP)
                .ToListAsync();

            return View(ordini);
        }

        /// <summary>
        /// Dashboard con statistiche degli ordini
        /// </summary>
        public async Task<IActionResult> Dashboard()
        {
            ViewBag.Title = "Dashboard Ordini di Produzione";

            var statistiche = new
            {
                TotaleOrdini = await _context.ListaOP.CountAsync(),
                OrdiniEmessi = await _context.ListaOP.CountAsync(l => l.Stato!.CodiceStato == "EM"),
                OrdiniInProduzione = await _context.ListaOP.CountAsync(l => l.Stato!.CodiceStato == "PR"),
                OrdiniSospesi = await _context.ListaOP.CountAsync(l => l.Stato!.CodiceStato == "SO"),
                OrdiniChiusi = await _context.ListaOP.CountAsync(l => l.Stato!.CodiceStato == "CH"),
                
                // Statistiche per centro di lavoro
                OrdiniPerCentro = await _context.ListaOP
                    .Include(l => l.CentroLavoro)
                    .GroupBy(l => l.CentroLavoro!.DescrizioneCentro)
                    .Select(g => new { Centro = g.Key, Conteggio = g.Count() })
                    .ToListAsync(),
                
                // Ordini urgenti
                OrdiniUrgenti = await _context.ListaOP
                    .Include(l => l.Stato)
                    .Include(l => l.CentroLavoro)
                    .Where(l => l.Priorita >= 4 && l.Stato!.CodiceStato != "CH")
                    .OrderByDescending(l => l.Priorita)
                    .Take(10)
                    .ToListAsync(),
                
                // Ordini in ritardo (data fine prevista passata)
                OrdiniInRitardo = await _context.ListaOP
                    .Include(l => l.Stato)
                    .Include(l => l.CentroLavoro)
                    .Where(l => l.DataFinePrevista < DateTime.Now && l.Stato!.CodiceStato != "CH")
                    .OrderBy(l => l.DataFinePrevista)
                    .Take(10)
                    .ToListAsync()
            };

            return View(statistiche);
        }

        /// <summary>
        /// Visualizza i dettagli di un ordine
        /// </summary>
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ordine = await _context.ListaOP
                .Include(l => l.Stato)
                .Include(l => l.Operatore)
                .Include(l => l.CentroLavoro)
                .FirstOrDefaultAsync(m => m.IdListaOP == id);

            if (ordine == null)
            {
                return NotFound();
            }

            ViewBag.Title = $"Dettagli Ordine {ordine.IdentificativoCompleto}";
            return View(ordine);
        }

        /// <summary>
        /// Form per creare un nuovo ordine
        /// </summary>
        public async Task<IActionResult> Create()
        {
            ViewBag.Title = "Nuovo Ordine di Produzione";
            await PrepareSelectLists();
            
            // Imposta valori predefiniti
            var nuovoOrdine = new ListaOP
            {
                DataInizioOP = DateTime.Now,
                AnnoOrdine = (short)DateTime.Now.Year,
                IdStato = 1, // Emesso
                Priorita = 2, // Normale
                QuantitaProdotta = 0
            };
            
            return View(nuovoOrdine);
        }

        /// <summary>
        /// Salva un nuovo ordine
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("TipoOrdine,AnnoOrdine,SerieOrdine,NumeroOrdine,RigaOrdine,DescrOrdine,CodiceArticolo,DescrizioneArticolo,UnitaMisura,Quantita,QuantitaProdotta,DataInizioOP,TempoCiclo,DataInizioSetup,TempoSetup,IdStato,IdCentroLavoro,IdLavorazione,Note,DataFineOP,DataFinePrevista,Priorita,IdOperatore,CostoOrario,TempoEffettivo")] ListaOP listaOP)
        {
            if (ModelState.IsValid)
            {
                _context.Add(listaOP);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Ordine di produzione creato con successo!";
                return RedirectToAction(nameof(Index));
            }
            
            await PrepareSelectLists(listaOP);
            ViewBag.Title = "Nuovo Ordine di Produzione";
            return View(listaOP);
        }

        /// <summary>
        /// Form per modificare un ordine
        /// </summary>
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ordine = await _context.ListaOP.FindAsync(id);
            if (ordine == null)
            {
                return NotFound();
            }

            await PrepareSelectLists(ordine);
            ViewBag.Title = $"Modifica Ordine {ordine.IdentificativoCompleto}";
            return View(ordine);
        }

        /// <summary>
        /// Salva le modifiche di un ordine
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdListaOP,TipoOrdine,AnnoOrdine,SerieOrdine,NumeroOrdine,RigaOrdine,DescrOrdine,CodiceArticolo,DescrizioneArticolo,UnitaMisura,Quantita,QuantitaProdotta,DataInizioOP,TempoCiclo,DataInizioSetup,TempoSetup,IdStato,IdCentroLavoro,IdLavorazione,Note,DataFineOP,DataFinePrevista,Priorita,IdOperatore,CostoOrario,TempoEffettivo")] ListaOP listaOP)
        {
            if (id != listaOP.IdListaOP)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(listaOP);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Ordine di produzione aggiornato con successo!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ListaOPExists(listaOP.IdListaOP))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            
            await PrepareSelectLists(listaOP);
            ViewBag.Title = $"Modifica Ordine {listaOP.IdentificativoCompleto}";
            return View(listaOP);
        }

        /// <summary>
        /// Conferma eliminazione ordine
        /// </summary>
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ordine = await _context.ListaOP
                .Include(l => l.Stato)
                .Include(l => l.Operatore)
                .Include(l => l.CentroLavoro)
                .Include(l => l.Lavorazione)
                .FirstOrDefaultAsync(m => m.IdListaOP == id);

            if (ordine == null)
            {
                return NotFound();
            }

            ViewBag.Title = $"Elimina Ordine {ordine.IdentificativoCompleto}";
            return View(ordine);
        }

        /// <summary>
        /// Elimina un ordine
        /// </summary>
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var ordine = await _context.ListaOP.FindAsync(id);
            if (ordine != null)
            {
                _context.ListaOP.Remove(ordine);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Ordine di produzione eliminato con successo!";
            }

            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// Aggiorna rapidamente lo stato di un ordine
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> UpdateStato(int id, int nuovoStato)
        {
            var ordine = await _context.ListaOP.FindAsync(id);
            if (ordine == null)
            {
                return Json(new { success = false, message = "Ordine non trovato" });
            }

            ordine.IdStato = nuovoStato;
            
            // Se viene chiuso, imposta la data fine
            if (nuovoStato == 3) // Chiuso
            {
                ordine.DataFineOP = DateTime.Now;
            }

            await _context.SaveChangesAsync();
            
            var stato = await _context.StatiOP.FindAsync(nuovoStato);
            return Json(new { success = true, message = $"Stato aggiornato a: {stato?.DescrizioneStato}" });
        }

        #region Metodi Helper

        /// <summary>
        /// Prepara le SelectList per i dropdown
        /// </summary>
        private async Task PrepareSelectLists(ListaOP? ordine = null)
        {
            ViewData["IdStato"] = new SelectList(
                await _context.StatiOP.Where(s => s.Attivo).OrderBy(s => s.Ordine).ToListAsync(),
                "IdStato", "DescrizioneStato", ordine?.IdStato);

            ViewData["IdOperatore"] = new SelectList(
                await _context.Operatori.Where(o => o.Attivo).OrderBy(o => o.Nome).ThenBy(o => o.Cognome).ToListAsync(),
                "IdOperatore", "NomeCompleto", ordine?.IdOperatore);

            ViewData["IdCentroLavoro"] = new SelectList(
                await _context.CentriLavoro.Where(c => c.Attivo).OrderBy(c => c.DescrizioneCentro).ToListAsync(),
                "IdCentroLavoro", "DescrizioneCentro", ordine?.IdCentroLavoro);

            ViewData["IdLavorazione"] = new SelectList(
                await _context.Lavorazioni.Where(l => l.Attivo).OrderBy(l => l.DescrizioneLavorazione).ToListAsync(),
                "IdLavorazione", "DescrizioneLavorazione", ordine?.IdLavorazione);

            ViewData["Priorita"] = new SelectList(new[]
            {
                new { Value = 1, Text = "1 - Bassa" },
                new { Value = 2, Text = "2 - Normale" },
                new { Value = 3, Text = "3 - Media" },
                new { Value = 4, Text = "4 - Alta" },
                new { Value = 5, Text = "5 - Urgente" }
            }, "Value", "Text", ordine?.Priorita);
        }

        /// <summary>
        /// Prepara i dati per i filtri
        /// </summary>
        private async Task PrepareFilterData()
        {
            ViewBag.Stati = await _context.StatiOP
                .Where(s => s.Attivo)
                .OrderBy(s => s.Ordine)
                .Select(s => new { s.CodiceStato, s.DescrizioneStato })
                .ToListAsync();

            ViewBag.Operatori = await _context.Operatori
                .Where(o => o.Attivo)
                .OrderBy(o => o.Nome)
                .ThenBy(o => o.Cognome)
                .Select(o => new { o.IdOperatore, NomeCompleto = o.Nome + " " + o.Cognome })
                .ToListAsync();

            ViewBag.CentriLavoro = await _context.CentriLavoro
                .Where(c => c.Attivo)
                .OrderBy(c => c.DescrizioneCentro)
                .Select(c => new { c.IdCentroLavoro, c.DescrizioneCentro })
                .ToListAsync();

            ViewBag.Lavorazioni = await _context.Lavorazioni
                .Where(l => l.Attivo)
                .OrderBy(l => l.DescrizioneLavorazione)
                .Select(l => new { l.IdLavorazione, l.DescrizioneLavorazione, l.CodiceLavorazione })
                .ToListAsync();

            ViewBag.Priorita = new[]
            {
                new { Value = 1, Text = "Bassa" },
                new { Value = 2, Text = "Normale" },
                new { Value = 3, Text = "Media" },
                new { Value = 4, Text = "Alta" },
                new { Value = 5, Text = "Urgente" }
            };
        }

        /// <summary>
        /// Verifica se un ordine esiste
        /// </summary>
        private bool ListaOPExists(int id)
        {
            return _context.ListaOP.Any(e => e.IdListaOP == id);
        }

        #endregion
    }
}
