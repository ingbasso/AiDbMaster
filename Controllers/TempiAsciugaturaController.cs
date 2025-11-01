using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using AiDbMaster.Data;
using AiDbMaster.Models;

namespace AiDbMaster.Controllers
{
    /// <summary>
    /// Controller per la gestione dei tempi di asciugatura mensili
    /// Fornisce funzionalità per visualizzare e modificare i giorni di asciugatura per ogni mese
    /// </summary>
    [Authorize]
    public class TempiAsciugaturaController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<TempiAsciugaturaController> _logger;

        public TempiAsciugaturaController(
            ApplicationDbContext context,
            ILogger<TempiAsciugaturaController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Visualizza la pagina principale con il grid dei tempi di asciugatura
        /// GET: TempiAsciugatura
        /// </summary>
        /// <returns>Vista con il grid dei tempi di asciugatura</returns>
        public IActionResult Index()
        {
            _logger.LogInformation("Accesso alla pagina Tempi di Asciugatura");
            return View();
        }

        /// <summary>
        /// API per ottenere tutti i tempi di asciugatura
        /// GET: api/tempi-asciugatura
        /// </summary>
        /// <returns>Lista dei tempi di asciugatura in formato JSON</returns>
        [HttpGet]
        [Route("api/tempi-asciugatura")]
        public async Task<IActionResult> GetTempiAsciugatura()
        {
            try
            {
                var tempiAsciugatura = await _context.TempiAsciugatura
                    .OrderBy(t => t.IdMese)
                    .ToListAsync();

                _logger.LogInformation("Recuperati {Count} record di tempi asciugatura", tempiAsciugatura.Count);
                return Json(tempiAsciugatura);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante il recupero dei tempi di asciugatura");
                return StatusCode(500, new { error = "Errore durante il recupero dei dati" });
            }
        }

        /// <summary>
        /// API per aggiornare un tempo di asciugatura
        /// POST: api/tempi-asciugatura/update
        /// </summary>
        /// <param name="tempoAsciugatura">Dati del tempo di asciugatura da aggiornare</param>
        /// <returns>Risultato dell'operazione</returns>
        [HttpPost]
        [Route("api/tempi-asciugatura/update")]
        public async Task<IActionResult> UpdateTempoAsciugatura([FromBody] TempiAsciugatura tempoAsciugatura)
        {
            try
            {
                if (tempoAsciugatura == null)
                {
                    return BadRequest(new { error = "Dati non validi" });
                }

                // Verifica che l'IdMese sia valido (1-12)
                if (tempoAsciugatura.IdMese < 1 || tempoAsciugatura.IdMese > 12)
                {
                    return BadRequest(new { error = "IdMese deve essere compreso tra 1 e 12" });
                }

                // Verifica che GiorniAsciugatura sia >= 0
                if (tempoAsciugatura.GiorniAsciugatura < 0)
                {
                    return BadRequest(new { error = "I giorni di asciugatura non possono essere negativi" });
                }

                var esistente = await _context.TempiAsciugatura
                    .FirstOrDefaultAsync(t => t.IdMese == tempoAsciugatura.IdMese);

                if (esistente == null)
                {
                    return NotFound(new { error = "Mese non trovato" });
                }

                // Aggiorna solo il campo GiorniAsciugatura
                esistente.GiorniAsciugatura = tempoAsciugatura.GiorniAsciugatura;

                await _context.SaveChangesAsync();

                _logger.LogInformation(
                    "Aggiornato tempo asciugatura per mese {Mese}: {Giorni} giorni",
                    esistente.Mese,
                    esistente.GiorniAsciugatura);

                return Json(new { success = true, message = "Tempo di asciugatura aggiornato con successo" });
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex, "Errore di concorrenza durante l'aggiornamento del tempo di asciugatura");
                return StatusCode(409, new { error = "Il record è stato modificato da un altro utente" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante l'aggiornamento del tempo di asciugatura");
                return StatusCode(500, new { error = "Errore durante l'aggiornamento dei dati" });
            }
        }

        /// <summary>
        /// API per aggiornare in batch i tempi di asciugatura (per operazioni Syncfusion Grid)
        /// POST: api/tempi-asciugatura/batch
        /// </summary>
        /// <param name="changes">Lista delle modifiche da applicare</param>
        /// <returns>Risultato dell'operazione</returns>
        [HttpPost]
        [Route("api/tempi-asciugatura/batch")]
        public async Task<IActionResult> BatchUpdate([FromBody] List<TempiAsciugatura> changes)
        {
            try
            {
                if (changes == null || !changes.Any())
                {
                    return BadRequest(new { error = "Nessuna modifica da applicare" });
                }

                foreach (var change in changes)
                {
                    var esistente = await _context.TempiAsciugatura
                        .FirstOrDefaultAsync(t => t.IdMese == change.IdMese);

                    if (esistente != null)
                    {
                        esistente.GiorniAsciugatura = change.GiorniAsciugatura;
                    }
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation("Aggiornati {Count} record di tempi asciugatura in batch", changes.Count);

                return Json(new { success = true, message = $"Aggiornati {changes.Count} record con successo" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante l'aggiornamento batch dei tempi di asciugatura");
                return StatusCode(500, new { error = "Errore durante l'aggiornamento dei dati" });
            }
        }

        /// <summary>
        /// API per ottenere i giorni di asciugatura per un mese specifico
        /// GET: api/tempi-asciugatura/{idMese}
        /// </summary>
        /// <param name="idMese">ID del mese (1-12)</param>
        /// <returns>Giorni di asciugatura per il mese specificato</returns>
        [HttpGet]
        [Route("api/tempi-asciugatura/{idMese}")]
        public async Task<IActionResult> GetGiorniAsciugaturaByMese(int idMese)
        {
            try
            {
                if (idMese < 1 || idMese > 12)
                {
                    return BadRequest(new { error = "IdMese deve essere compreso tra 1 e 12" });
                }

                var tempo = await _context.TempiAsciugatura
                    .FirstOrDefaultAsync(t => t.IdMese == idMese);

                if (tempo == null)
                {
                    return NotFound(new { error = "Mese non trovato" });
                }

                return Json(tempo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante il recupero del tempo di asciugatura per mese {IdMese}", idMese);
                return StatusCode(500, new { error = "Errore durante il recupero dei dati" });
            }
        }
    }
}

