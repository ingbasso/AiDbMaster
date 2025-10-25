using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AiDbMaster.Data;
using AiDbMaster.ViewModels;

namespace AiDbMaster.Controllers
{
    [Authorize]
    public class InterrogazioniDBController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<InterrogazioniDBController> _logger;

        public InterrogazioniDBController(
            ApplicationDbContext context,
            ILogger<InterrogazioniDBController> logger)
        {
            _context = context;
            _logger = logger;
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
        /// GET: Mostra il form per la ricerca disponibilità
        /// </summary>
        [HttpGet]
        public IActionResult Disponibilita()
        {
            var model = new DisponibilitaViewModel
            {
                DataRiferimento = DateTime.Today // Imposta data odierna come default
            };
            return View(model);
        }

        /// <summary>
        /// POST: Esegue la ricerca disponibilità
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Disponibilita(DisponibilitaViewModel model)
        {
            if (!string.IsNullOrEmpty(model.CodiceArticolo))
            {
                try
                {
                    // Data di riferimento (se null, usa oggi)
                    var dataRiferimento = model.DataRiferimento ?? DateTime.Today;
                    var oggi = DateTime.Today;

                    // Calcola impegnato futuro (solo se data > oggi)
                    decimal impegnatoFuturo = 0;
                    if (dataRiferimento > oggi)
                    {
                        impegnatoFuturo = await _context.OrdiniRighe
                            .Where(r => r.TipoOrdine == "R")  // Solo ordini clienti
                            .Where(r => r.CodiceArticolo == model.CodiceArticolo)
                            .Where(r => r.DataConsegna > oggi)
                            .Where(r => r.DataConsegna <= dataRiferimento)
                            .Where(r => r.Quantita > r.QuantitaEvasa)  // Solo quantità da evadere
                            .SumAsync(r => (decimal?)(r.Quantita - r.QuantitaEvasa)) ?? 0;

                        _logger.LogInformation(
                            "Impegnato futuro calcolato per articolo {CodiceArticolo} " +
                            "tra {DataInizio} e {DataFine}: {ImpegnatoFuturo}",
                            model.CodiceArticolo, oggi, dataRiferimento, impegnatoFuturo);
                    }

                    // Query sulla tabella ProgressiviArticoli (solo magazzino 1)
                    var progressivi = await _context.ProgressiviArticoli
                        .Where(p => p.CodiceArticolo == model.CodiceArticolo)
                        .Where(p => p.CodiceMagazzino == 1)  // Solo magazzino 1
                        .OrderBy(p => p.CodiceMagazzino)
                        .ToListAsync();

                    // Mappa i risultati aggiungendo l'impegnato futuro
                    var risultati = progressivi.Select(p => new DisponibilitaRigaViewModel
                    {
                        CodiceArticolo = p.CodiceArticolo,
                        CodiceMagazzino = p.CodiceMagazzino,
                        Esistenza = p.Esistenza,
                        ImpegnatoDataOdierna = p.ImpegnatoDataOdierna + impegnatoFuturo, // Impegnato DB + futuro
                        OrdinatoFornitoriDataOdierna = p.OrdinatoFornitoriDataOdierna
                    }).ToList();

                    model.Risultati = risultati;

                    // Recupera la descrizione dell'articolo
                    var articolo = await _context.AnagraficaArticoli
                        .FirstOrDefaultAsync(a => a.CodiceArticolo == model.CodiceArticolo);
                    
                    if (articolo != null)
                    {
                        model.DescrizioneArticolo = articolo.Descrizione;
                    }

                    if (!risultati.Any())
                    {
                        ViewBag.Message = "Articolo non trovato nel magazzino 1.";
                        ViewBag.MessageType = "warning";
                    }
                    else
                    {
                        ViewBag.Message = $"Disponibilità articolo {model.CodiceArticolo} nel magazzino 1.";
                        ViewBag.MessageType = "success";
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Errore durante la ricerca disponibilità per articolo {CodiceArticolo}", model.CodiceArticolo);
                    ViewBag.Message = "Errore durante la ricerca. Riprova più tardi.";
                    ViewBag.MessageType = "danger";
                }
            }
            else
            {
                ViewBag.Message = "Seleziona un articolo per visualizzare la disponibilità.";
                ViewBag.MessageType = "info";
            }

            return View(model);
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

        // ========== CONSEGNE PROGRAMMATE ==========

        /// <summary>
        /// GET: Mostra il form per le consegne programmate (da implementare)
        /// </summary>
        [HttpGet]
        public IActionResult ConsegneProgrammate()
        {
            var model = new ConsegneProgrammateViewModel
            {
                DataInizio = DateTime.Today,
                DataFine = DateTime.Today.AddDays(30)
            };
            return View(model);
        }

        /// <summary>
        /// POST: Esegue la ricerca consegne programmate (da implementare)
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ConsegneProgrammate(ConsegneProgrammateViewModel model)
        {
            // TODO: Implementare la logica per le consegne programmate
            ViewBag.Message = "Funzionalità in fase di implementazione.";
            ViewBag.MessageType = "info";
            
            return View(model);
        }
    }
}

