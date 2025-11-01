using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AiDbMaster.Data;
using AiDbMaster.Models;

namespace AiDbMaster.Controllers
{
    public class FCProvaController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<FCProvaController> _logger;

        public FCProvaController(ApplicationDbContext context, ILogger<FCProvaController> logger)
        {
            _context = context;
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        // API: Carica tutti i centri di lavoro
        [HttpGet]
        public async Task<IActionResult> GetCentriLavoro()
        {
            try
            {
                var centri = await _context.CentriLavoro
                    .Select(c => new
                    {
                        Id = c.CodiceCentro,
                        Name = c.DescrizioneCentro,
                        Color = "#3498db" // Blu di default
                    })
                    .ToListAsync();

                return Json(centri);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore nel caricamento dei centri di lavoro");
                return StatusCode(500, new { message = "Errore nel caricamento dei centri di lavoro" });
            }
        }

        // API: Carica tutti i fermi
        [HttpGet]
        public async Task<IActionResult> GetFermi()
        {
            try
            {
                var fermi = await (from f in _context.CalendarioFermiCentriLavoro
                                   join c in _context.CentriLavoro on f.CodiceCentro equals c.CodiceCentro
                                   select new
                                   {
                                       Id = f.Id,
                                       Subject = $"{c.DescrizioneCentro}: {f.Motivo ?? "Fermo"}",
                                       StartTime = f.DataInizioFermo,
                                       EndTime = f.DataFineFermo,
                                       CodiceCentro = f.CodiceCentro,
                                       Motivo = f.Motivo,
                                       TipoFermo = (int)f.TipoFermo,
                                       IsPianificato = f.IsPianificato
                                   }).ToListAsync();

                return Json(fermi);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore nel caricamento dei fermi");
                return StatusCode(500, new { message = "Errore nel caricamento dei fermi" });
            }
        }

        // API: Crea nuovo fermo
        [HttpPost]
        public async Task<IActionResult> CreateFermo([FromBody] CreateFermoRequest request)
        {
            try
            {
                var nuovoFermo = new CalendarioFermiCentriLavoro
                {
                    CodiceCentro = request.CodiceCentro,
                    DataInizioFermo = request.DataInizioFermo,
                    DataFineFermo = request.DataFineFermo,
                    TipoFermo = (TipoFermo)request.TipoFermo,
                    Motivo = request.Motivo,
                    IsPianificato = request.IsPianificato
                };

                _context.CalendarioFermiCentriLavoro.Add(nuovoFermo);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Fermo creato con successo" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore nella creazione del fermo");
                return StatusCode(500, new { message = "Errore nella creazione del fermo" });
            }
        }

        // API: Aggiorna fermo
        [HttpPost]
        public async Task<IActionResult> UpdateFermo([FromBody] UpdateFermoRequest request)
        {
            try
            {
                var fermo = await _context.CalendarioFermiCentriLavoro.FindAsync(request.Id);
                if (fermo == null)
                {
                    return NotFound(new { message = "Fermo non trovato" });
                }

                fermo.CodiceCentro = request.CodiceCentro;
                fermo.DataInizioFermo = request.DataInizioFermo;
                fermo.DataFineFermo = request.DataFineFermo;
                fermo.TipoFermo = (TipoFermo)request.TipoFermo;
                fermo.Motivo = request.Motivo;
                fermo.IsPianificato = request.IsPianificato;

                await _context.SaveChangesAsync();

                return Ok(new { message = "Fermo aggiornato con successo" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore nell'aggiornamento del fermo");
                return StatusCode(500, new { message = "Errore nell'aggiornamento del fermo" });
            }
        }

        // API: Elimina fermo
        [HttpPost]
        public async Task<IActionResult> DeleteFermo([FromBody] DeleteFermoRequest request)
        {
            try
            {
                var fermo = await _context.CalendarioFermiCentriLavoro.FindAsync(request.Id);
                if (fermo == null)
                {
                    return NotFound(new { message = "Fermo non trovato" });
                }

                _context.CalendarioFermiCentriLavoro.Remove(fermo);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Fermo eliminato con successo" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore nell'eliminazione del fermo");
                return StatusCode(500, new { message = "Errore nell'eliminazione del fermo" });
            }
        }

        // API: Genera fermi settimanali
        [HttpPost]
        public async Task<IActionResult> GeneraFermiSettimanali([FromBody] GeneraFermiSettimanaliRequest model)
        {
            try
            {
                // Carica centri
                List<string> centri;

                if (model.ApplicaATutti)
                {
                    // Applica a tutti i centri attivi
                    centri = await _context.CentriLavoro
                        .Where(c => c.Attivo == true)
                        .Select(c => c.CodiceCentro)
                        .ToListAsync();
                }
                else if (!string.IsNullOrEmpty(model.CodiceCentro))
                {
                    // Applica solo al centro selezionato
                    var centroEsiste = await _context.CentriLavoro
                        .AnyAsync(c => c.CodiceCentro == model.CodiceCentro && c.Attivo == true);

                    if (!centroEsiste)
                    {
                        return BadRequest(new { message = "Centro di lavoro non trovato o non attivo" });
                    }

                    centri = new List<string> { model.CodiceCentro };
                }
                else
                {
                    return BadRequest(new { message = "Seleziona un centro o applica a tutti" });
                }

                if (!centri.Any())
                {
                    return BadRequest(new { message = "Nessun centro attivo trovato" });
                }

                var fermiCreati = 0;

                // Per ogni settimana nel range
                for (int numSettimana = model.DaSettimana; numSettimana <= model.ASettimana; numSettimana++)
                {
                    // Calcola il lunedì della settimana
                    var lunedi = GetMondayOfWeek(model.Anno, numSettimana);

                    foreach (var centro in centri)
                    {
                        // 1. Lun 20:00 - Mar 06:00 (Turno Notturno)
                        CreaFermoNotturno(lunedi, DayOfWeek.Monday, centro, model.Motivo);
                        fermiCreati++;

                        // 2. Mar 20:00 - Mer 06:00 (Turno Notturno)
                        CreaFermoNotturno(lunedi, DayOfWeek.Tuesday, centro, model.Motivo);
                        fermiCreati++;

                        // 3. Mer 20:00 - Gio 06:00 (Turno Notturno)
                        CreaFermoNotturno(lunedi, DayOfWeek.Wednesday, centro, model.Motivo);
                        fermiCreati++;

                        // 4. Gio 20:00 - Ven 06:00 (Turno Notturno)
                        CreaFermoNotturno(lunedi, DayOfWeek.Thursday, centro, model.Motivo);
                        fermiCreati++;

                        // 5. Weekend: Ven 20:00 - Lun 06:00 (settimana successiva)
                        var venerdi = lunedi.AddDays(4); // Venerdì della settimana
                        var lunediSuccessivo = lunedi.AddDays(7); // Lunedì settimana successiva

                        var fermoWeekend = new CalendarioFermiCentriLavoro
                        {
                            CodiceCentro = centro,
                            DataInizioFermo = venerdi.Date.AddHours(20),
                            DataFineFermo = lunediSuccessivo.Date.AddHours(6),
                            TipoFermo = TipoFermo.WeekEnd, // = 2
                            Motivo = model.Motivo ?? "Fermo programmato",
                            IsPianificato = true
                        };
                        _context.CalendarioFermiCentriLavoro.Add(fermoWeekend);
                        fermiCreati++;
                    }
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation($"Generati {fermiCreati} fermi settimanali per {centri.Count} centri");

                return Ok(new
                {
                    message = $"Generati {fermiCreati} fermi per {centri.Count} centro/i",
                    totaleFermi = fermiCreati
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore nella generazione dei fermi settimanali");
                return StatusCode(500, new { message = "Errore nella generazione dei fermi settimanali" });
            }
        }

        // Helper: Crea un fermo notturno
        private void CreaFermoNotturno(DateTime lunediSettimana, DayOfWeek giorno, string centro, string? motivo)
        {
            var giornoInizio = lunediSettimana.AddDays((int)giorno - (int)DayOfWeek.Monday);
            var giornoFine = giornoInizio.AddDays(1);

            var fermo = new CalendarioFermiCentriLavoro
            {
                CodiceCentro = centro,
                DataInizioFermo = giornoInizio.Date.AddHours(20),
                DataFineFermo = giornoFine.Date.AddHours(6),
                TipoFermo = TipoFermo.TurnoNotturno, // = 1
                Motivo = motivo ?? "Fermo programmato",
                IsPianificato = true
            };
            _context.CalendarioFermiCentriLavoro.Add(fermo);
        }

        // Helper: Ottiene il lunedì di una settimana specifica dell'anno (ISO 8601)
        private DateTime GetMondayOfWeek(int anno, int numeroSettimana)
        {
            return System.Globalization.ISOWeek.ToDateTime(anno, numeroSettimana, DayOfWeek.Monday);
        }
    }

    // Request models
    public class CreateFermoRequest
    {
        public string CodiceCentro { get; set; } = string.Empty;
        public DateTime DataInizioFermo { get; set; }
        public DateTime DataFineFermo { get; set; }
        public int TipoFermo { get; set; }
        public string? Motivo { get; set; }
        public bool IsPianificato { get; set; }
    }

    public class UpdateFermoRequest
    {
        public int Id { get; set; }
        public string CodiceCentro { get; set; } = string.Empty;
        public DateTime DataInizioFermo { get; set; }
        public DateTime DataFineFermo { get; set; }
        public int TipoFermo { get; set; }
        public string? Motivo { get; set; }
        public bool IsPianificato { get; set; }
    }

    public class DeleteFermoRequest
    {
        public int Id { get; set; }
    }

    public class GeneraFermiSettimanaliRequest
    {
        public int Anno { get; set; }
        public int DaSettimana { get; set; }
        public int ASettimana { get; set; }
        public string? Motivo { get; set; }
        public bool ApplicaATutti { get; set; }
        public string? CodiceCentro { get; set; }
    }
}

