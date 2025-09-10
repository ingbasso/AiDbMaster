using Microsoft.AspNetCore.Mvc;
using Syncfusion.EJ2.Grids;

namespace AiDbMaster.Controllers
{
    public class SyncfusionTestController : Controller
    {
        public IActionResult Index()
        {
            // Dati di esempio per la griglia
            var sampleData = new List<object>
            {
                new { Id = 1, Nome = "Mario Rossi", Email = "mario.rossi@email.com", Città = "Roma", Età = 35, Stipendio = 2500 },
                new { Id = 2, Nome = "Giulia Bianchi", Email = "giulia.bianchi@email.com", Città = "Milano", Età = 28, Stipendio = 2200 },
                new { Id = 3, Nome = "Luca Verdi", Email = "luca.verdi@email.com", Città = "Napoli", Età = 42, Stipendio = 2800 },
                new { Id = 4, Nome = "Anna Neri", Email = "anna.neri@email.com", Città = "Torino", Età = 31, Stipendio = 2400 },
                new { Id = 5, Nome = "Paolo Blu", Email = "paolo.blu@email.com", Città = "Firenze", Età = 39, Stipendio = 2600 },
                new { Id = 6, Nome = "Sara Gialli", Email = "sara.gialli@email.com", Città = "Bologna", Età = 26, Stipendio = 2100 },
                new { Id = 7, Nome = "Marco Rossi", Email = "marco.rossi@email.com", Città = "Venezia", Età = 33, Stipendio = 2300 },
                new { Id = 8, Nome = "Elena Bianchi", Email = "elena.bianchi@email.com", Città = "Genova", Età = 29, Stipendio = 2250 },
                new { Id = 9, Nome = "Andrea Verdi", Email = "andrea.verdi@email.com", Città = "Palermo", Età = 37, Stipendio = 2450 },
                new { Id = 10, Nome = "Francesca Neri", Email = "francesca.neri@email.com", Città = "Catania", Età = 24, Stipendio = 2000 }
            };

            // Dati di esempio per il Scheduler
            var scheduleData = new List<object>
            {
                new { 
                    Id = 1, 
                    Subject = "Riunione Team", 
                    StartTime = new DateTime(2025, 1, 15, 9, 0, 0), 
                    EndTime = new DateTime(2025, 1, 15, 10, 30, 0),
                    Description = "Riunione settimanale del team di sviluppo",
                    Location = "Sala Conferenze A"
                },
                new { 
                    Id = 2, 
                    Subject = "Presentazione Progetto", 
                    StartTime = new DateTime(2025, 1, 15, 14, 0, 0), 
                    EndTime = new DateTime(2025, 1, 15, 15, 0, 0),
                    Description = "Presentazione del nuovo progetto ai clienti",
                    Location = "Sala Conferenze B"
                },
                new { 
                    Id = 3, 
                    Subject = "Formazione Syncfusion", 
                    StartTime = new DateTime(2025, 1, 16, 10, 0, 0), 
                    EndTime = new DateTime(2025, 1, 16, 12, 0, 0),
                    Description = "Sessione di formazione sui controlli Syncfusion",
                    Location = "Aula Formazione"
                },
                new { 
                    Id = 4, 
                    Subject = "Review Codice", 
                    StartTime = new DateTime(2025, 1, 16, 15, 0, 0), 
                    EndTime = new DateTime(2025, 1, 16, 16, 30, 0),
                    Description = "Review del codice del progetto AiDbMaster",
                    Location = "Ufficio Sviluppo"
                },
                new { 
                    Id = 5, 
                    Subject = "Demo Sistema", 
                    StartTime = new DateTime(2025, 1, 17, 9, 30, 0), 
                    EndTime = new DateTime(2025, 1, 17, 11, 0, 0),
                    Description = "Dimostrazione del sistema AiDbMaster",
                    Location = "Sala Demo"
                },
                new { 
                    Id = 6, 
                    Subject = "Meeting Clienti", 
                    StartTime = new DateTime(2025, 1, 17, 14, 30, 0), 
                    EndTime = new DateTime(2025, 1, 17, 16, 0, 0),
                    Description = "Incontro con i clienti per feedback",
                    Location = "Sala Conferenze A"
                },
                new { 
                    Id = 7, 
                    Subject = "Test Integrazione", 
                    StartTime = new DateTime(2025, 1, 18, 10, 0, 0), 
                    EndTime = new DateTime(2025, 1, 18, 12, 0, 0),
                    Description = "Test di integrazione dei controlli Syncfusion",
                    Location = "Laboratorio Test"
                }
            };

            ViewBag.ScheduleData = scheduleData;
            return View(sampleData);
        }
    }
}
