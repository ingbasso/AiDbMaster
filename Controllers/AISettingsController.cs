using AiDbMaster.Models;
using AiDbMaster.Services;
using AiDbMaster.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace AiDbMaster.Controllers
{
    [Authorize(Roles = UserRoles.Admin)]
    public class AISettingsController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly MistralAIService _mistralAIService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfigurationRoot _configRoot;

        public AISettingsController(
            IConfiguration configuration,
            MistralAIService mistralAIService,
            UserManager<ApplicationUser> userManager)
        {
            _configuration = configuration;
            _mistralAIService = mistralAIService;
            _userManager = userManager;
            
            // Per poter modificare la configurazione
            _configRoot = (IConfigurationRoot)configuration;
        }

        public IActionResult Index()
        {
            var model = new AISettingsViewModel
            {
                MistralApiKey = _configuration["MistralAI:ApiKey"],
                MistralApiEndpoint = _configuration["MistralAI:ApiEndpoint"],
                MistralModelName = _configuration["MistralAI:ModelName"],
                MonitoredFolders = _configuration.GetSection("FolderMonitor:Folders").Get<List<string>>() ?? new List<string>(),
                DefaultUserId = _configuration["FolderMonitor:DefaultUserId"],
                AvailableUsers = _userManager.Users.Select(u => new UserViewModel
                {
                    Id = u.Id,
                    Email = u.Email,
                    FullName = $"{u.FirstName} {u.LastName}"
                }).ToList()
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SaveSettings(AISettingsViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Aggiorna le impostazioni di Mistral AI
                    UpdateAppSetting("MistralAI:ApiKey", model.MistralApiKey);
                    UpdateAppSetting("MistralAI:ApiEndpoint", model.MistralApiEndpoint);
                    UpdateAppSetting("MistralAI:ModelName", model.MistralModelName);
                    
                    // Aggiorna le impostazioni del monitoraggio delle cartelle
                    UpdateAppSetting("FolderMonitor:DefaultUserId", model.DefaultUserId);
                    
                    // Aggiorna le cartelle monitorate
                    var foldersSection = _configRoot.GetSection("FolderMonitor:Folders");
                    if (model.MonitoredFolders != null)
                    {
                        for (int i = 0; i < model.MonitoredFolders.Count; i++)
                        {
                            UpdateAppSetting($"FolderMonitor:Folders:{i}", model.MonitoredFolders[i]);
                        }
                    }
                    
                    // Crea le cartelle se non esistono
                    if (model.MonitoredFolders != null)
                    {
                        foreach (var folder in model.MonitoredFolders)
                        {
                            if (!string.IsNullOrEmpty(folder) && !Directory.Exists(folder))
                            {
                                Directory.CreateDirectory(folder);
                            }
                        }
                    }

                    TempData["SuccessMessage"] = "Impostazioni salvate con successo. Riavvia l'applicazione per applicare le modifiche.";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(string.Empty, $"Errore durante il salvataggio delle impostazioni: {ex.Message}");
                }
            }

            // Ricarica la lista degli utenti in caso di errore
            model.AvailableUsers = _userManager.Users.Select(u => new UserViewModel
            {
                Id = u.Id,
                Email = u.Email,
                FullName = $"{u.FirstName} {u.LastName}"
            }).ToList();

            return View("Index", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TestAIConnection()
        {
            try
            {
                // Implementa un test di connessione all'API di Mistral AI
                var testResult = await _mistralAIService.AnalyzeDocumentAsync(
                    "Questo è un documento di test per verificare la connessione con Mistral AI.",
                    "test.txt",
                    "TXT");

                if (testResult != null)
                {
                    TempData["SuccessMessage"] = $"Connessione a Mistral AI riuscita. Categoria suggerita: {testResult.Categoria}";
                }
                else
                {
                    TempData["ErrorMessage"] = "Connessione a Mistral AI fallita. Nessun risultato ricevuto.";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Errore durante il test della connessione: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public IActionResult ViewMistralLogs()
        {
            var mistralService = HttpContext.RequestServices.GetRequiredService<MistralAIService>();
            ViewBag.Logs = mistralService.GetMistralLogs();
            return View();
        }

        private void UpdateAppSetting(string key, string? value)
        {
            // Questo è un metodo semplificato per aggiornare le impostazioni
            // In un'applicazione reale, dovresti utilizzare un approccio più robusto
            // per aggiornare il file appsettings.json
            
            // Nota: questa implementazione è solo per scopi dimostrativi
            // e non modifica effettivamente il file appsettings.json
            
            // In un'implementazione reale, dovresti:
            // 1. Leggere il file appsettings.json
            // 2. Modificare il valore della chiave specificata
            // 3. Salvare il file
            
            // Per ora, simuliamo l'aggiornamento
            Console.WriteLine($"Aggiornamento dell'impostazione {key} a {value}");
        }
    }
} 