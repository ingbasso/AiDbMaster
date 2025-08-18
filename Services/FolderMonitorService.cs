using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AiDbMaster.Models;
using AiDbMaster.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf.Canvas.Parser.Listener;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using OfficeOpenXml;
using MimeKit;
using System.Net.Http;
using System.Text.Json;
using System.Net.Http.Json;

namespace AiDbMaster.Services
{
    public class FolderMonitorService : IHostedService, IDisposable
    {
        private readonly ILogger<FolderMonitorService> _logger;
        private readonly IConfiguration _configuration;
        private readonly IServiceProvider _serviceProvider;
        private readonly List<FileSystemWatcher> _watchers = new();
        private readonly string[] _supportedExtensions = { ".pdf", ".docx", ".txt", ".xlsx", ".csv", ".eml" };
        private readonly Dictionary<string, DocumentType> _extensionToTypeMap;
        private readonly Timer? _timer;
        private readonly HttpClient _httpClient;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public FolderMonitorService(
            ILogger<FolderMonitorService> logger,
            IConfiguration configuration,
            IServiceProvider serviceProvider,
            IWebHostEnvironment webHostEnvironment)
        {
            _logger = logger;
            _configuration = configuration;
            _serviceProvider = serviceProvider;
            _webHostEnvironment = webHostEnvironment;
            
            // Inizializza il dizionario delle estensioni
            _extensionToTypeMap = new Dictionary<string, DocumentType>
            {
                { ".pdf", DocumentType.PDF },
                { ".docx", DocumentType.DOCX },
                { ".txt", DocumentType.TXT },
                { ".xlsx", DocumentType.EXCEL },
                { ".csv", DocumentType.EXCEL },
                { ".eml", DocumentType.EMAIL }
            };
            
            // Crea un timer per il monitoraggio periodico
            _timer = new Timer(DoWork, null, Timeout.Infinite, Timeout.Infinite);
            
            // Inizializza l'HttpClient
            _httpClient = new HttpClient();
            var serverUrl = _configuration.GetSection("Server:Url").Value;
            if (string.IsNullOrEmpty(serverUrl))
            {
                throw new InvalidOperationException("Server:Url non configurato in appsettings.json");
            }
            _httpClient.BaseAddress = new Uri(serverUrl);
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Servizio di monitoraggio cartelle avviato, ma in modalità manuale.");
            
            // Non avviamo più il timer per il monitoraggio automatico
            // _timer = new Timer(DoWork, null, TimeSpan.Zero, TimeSpan.FromMinutes(5));
            
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Servizio di monitoraggio cartelle fermato");
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Change(Timeout.Infinite, 0);
        }

        // Metodi helper per gestire il caso in cui notificationService è null
        private async Task SendUpdateAsync(CatalogNotificationService? notificationService, string message)
        {
            if (notificationService != null)
            {
                await notificationService.SendUpdateAsync(message);
            }
        }

        private async Task SendProgressAsync(CatalogNotificationService? notificationService, int processed, int total)
        {
            if (notificationService != null)
            {
                await notificationService.SendProgressAsync(processed, total);
            }
        }

        private async Task SendCompletedAsync(CatalogNotificationService? notificationService, int processed, int errors)
        {
            if (notificationService != null)
            {
                await notificationService.SendCompletedAsync(processed, errors);
            }
        }

        // Metodo pubblico che può essere chiamato dal controller
        public async Task ScanFoldersAsync()
        {
            try
            {
                _logger.LogInformation("Avvio scansione manuale delle cartelle monitorate");
                
                using (var scope = _serviceProvider.CreateScope())
                {
                    var notificationService = scope.ServiceProvider.GetRequiredService<CatalogNotificationService>();
                    await notificationService.SendUpdateAsync("Avvio della catalogazione dei documenti...");
                    
                    await DoWorkAsync(null, notificationService);
                    
                    await notificationService.SendUpdateAsync("Catalogazione completata");
                }
                
                _logger.LogInformation("Scansione manuale completata");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante la scansione manuale delle cartelle");
                
                using (var scope = _serviceProvider.CreateScope())
                {
                    var notificationService = scope.ServiceProvider.GetRequiredService<CatalogNotificationService>();
                    await notificationService.SendUpdateAsync($"Errore durante la catalogazione: {ex.Message}");
                }
                
                throw;
            }
        }

        // Modifica il metodo DoWork per supportare la chiamata asincrona
        private void DoWork(object? state)
        {
            DoWorkAsync(state).Wait();
        }

        private async Task DoWorkAsync(object? state, CatalogNotificationService? notificationService = null)
        {
            int totalFiles = 0;
            int processedFiles = 0;
            int errorFiles = 0;
            Dictionary<string, string> errorDetails = new Dictionary<string, string>();
            
            try
            {
                _logger.LogInformation("Servizio di monitoraggio cartelle avviato");
                await SendUpdateAsync(notificationService, "Inizializzazione del servizio di monitoraggio...");

                // Ottieni le cartelle da monitorare dalla configurazione
                var foldersToMonitor = _configuration.GetSection("FolderMonitor:Folders").Get<string[]>();
                
                if (foldersToMonitor == null || foldersToMonitor.Length == 0)
                {
                    string errorMsg = "Nessuna cartella configurata per il monitoraggio";
                    _logger.LogError(errorMsg);
                    await SendUpdateAsync(notificationService, $"⚠️ ERRORE: {errorMsg}");
                    await SendCompletedAsync(notificationService, 0, 1);
                    return;
                }

                await SendUpdateAsync(notificationService, $"📂 Cartelle da monitorare: {string.Join(", ", foldersToMonitor)}");

                // Verifica che almeno una cartella esista
                bool atLeastOneFolderExists = false;
                foreach (var folder in foldersToMonitor)
                {
                    if (Directory.Exists(folder))
                    {
                        atLeastOneFolderExists = true;
                        await SendUpdateAsync(notificationService, $"✅ Cartella trovata: {folder}");
                        _logger.LogInformation($"Cartella trovata: {folder}");
                    }
                    else
                    {
                        _logger.LogWarning($"La cartella {folder} non esiste");
                        await SendUpdateAsync(notificationService, $"⚠️ Attenzione: La cartella {folder} non esiste");
                        
                        // Prova a creare la cartella
                        try
                        {
                            Directory.CreateDirectory(folder);
                            await SendUpdateAsync(notificationService, $"✅ Cartella creata: {folder}");
                            _logger.LogInformation($"Cartella creata: {folder}");
                            atLeastOneFolderExists = true;
                        }
                        catch (Exception ex)
                        {
                            string errorMsg = $"Impossibile creare la cartella {folder}";
                            _logger.LogError(ex, errorMsg);
                            await SendUpdateAsync(notificationService, $"❌ ERRORE: {errorMsg}. Dettagli: {ex.Message}");
                            errorDetails[folder] = ex.Message;
                            errorFiles++;
                        }
                    }
                }

                if (!atLeastOneFolderExists)
                {
                    string errorMsg = "Nessuna delle cartelle configurate esiste";
                    _logger.LogError(errorMsg);
                    await SendUpdateAsync(notificationService, $"❌ ERRORE: {errorMsg}");
                    await SendCompletedAsync(notificationService, 0, errorFiles);
                    return;
                }

                // Ottieni l'ID dell'utente amministratore predefinito
                var defaultUserId = _configuration["FolderMonitor:DefaultUserId"];
                if (string.IsNullOrEmpty(defaultUserId) || defaultUserId == "ADMIN_USER_ID_HERE")
                {
                    _logger.LogWarning("ID utente predefinito non configurato correttamente");
                    await SendUpdateAsync(notificationService, "⚠️ Attenzione: ID utente predefinito non configurato correttamente. Verrà utilizzato l'ID del primo amministratore trovato.");
                    
                    // Prova a ottenere l'ID del primo amministratore
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var userManager = scope.ServiceProvider.GetRequiredService<Microsoft.AspNetCore.Identity.UserManager<AiDbMaster.Models.ApplicationUser>>();
                        var roleManager = scope.ServiceProvider.GetRequiredService<Microsoft.AspNetCore.Identity.RoleManager<Microsoft.AspNetCore.Identity.IdentityRole>>();
                        
                        var adminRole = await roleManager.FindByNameAsync("Admin");
                        if (adminRole != null && !string.IsNullOrEmpty(adminRole.Name))
                        {
                            var adminUsers = await userManager.GetUsersInRoleAsync(adminRole.Name);
                            if (adminUsers.Any())
                            {
                                defaultUserId = adminUsers.First().Id;
                                await SendUpdateAsync(notificationService, $"✅ Trovato ID amministratore: {defaultUserId}");
                                _logger.LogInformation($"Trovato ID amministratore: {defaultUserId}");
                            }
                        }
                    }
                    
                    if (string.IsNullOrEmpty(defaultUserId) || defaultUserId == "ADMIN_USER_ID_HERE")
                    {
                        string errorMsg = "Impossibile trovare un ID utente amministratore valido";
                        _logger.LogError(errorMsg);
                        await SendUpdateAsync(notificationService, $"❌ ERRORE: {errorMsg}");
                        await SendCompletedAsync(notificationService, 0, errorFiles + 1);
                        return;
                    }
                }

                using (var scope = _serviceProvider.CreateScope())
                {
                    var documentService = scope.ServiceProvider.GetRequiredService<DocumentService>();
                    var categoryService = scope.ServiceProvider.GetRequiredService<CategoryService>();
                    
                    // Crea o ottieni la categoria "Da Catalogare"
                    await SendUpdateAsync(notificationService, "🔍 Verifica della categoria 'Da Catalogare'...");
                    var uncategorizedCategory = await categoryService.GetCategoryByNameAsync("Da Catalogare");
                    if (uncategorizedCategory == null)
                    {
                        await SendUpdateAsync(notificationService, "➕ Creazione della categoria 'Da Catalogare'...");
                        try
                        {
                            // Crea direttamente la categoria usando il servizio
                            uncategorizedCategory = new DocumentCategory
                            {
                                Name = "Da Catalogare",
                                Description = "Documenti in attesa di catalogazione"
                            };
                            
                            await categoryService.CreateCategoryAsync(uncategorizedCategory);
                            _logger.LogInformation("Creata categoria 'Da Catalogare'");
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Errore durante la creazione della categoria 'Da Catalogare'");
                            await SendUpdateAsync(notificationService, $"❌ Errore durante la creazione della categoria 'Da Catalogare': {ex.Message}");
                            throw; // Rilancia l'eccezione per interrompere il processo
                        }
                    }

                    // Conta il numero totale di file da elaborare
                    await SendUpdateAsync(notificationService, "🔢 Conteggio dei file da elaborare...");
                    foreach (var folder in foldersToMonitor)
                    {
                        if (!Directory.Exists(folder))
                            continue;

                        try
                        {
                            var files = Directory.GetFiles(folder, "*.*", SearchOption.AllDirectories)
                                .Where(f => _supportedExtensions.Contains(Path.GetExtension(f).ToLowerInvariant()))
                                .ToList();
                            
                            totalFiles += files.Count;
                            _logger.LogInformation($"Trovati {files.Count} file nella cartella {folder}");
                        }
                        catch (Exception ex)
                        {
                            string errorMsg = $"Errore durante la scansione della cartella {folder}";
                            _logger.LogError(ex, errorMsg);
                            await SendUpdateAsync(notificationService, $"❌ ERRORE: {errorMsg}. Dettagli: {ex.Message}");
                            errorDetails[folder] = ex.Message;
                            errorFiles++;
                        }
                    }

                    if (totalFiles == 0)
                    {
                        await SendUpdateAsync(notificationService, "ℹ️ Nessun file da elaborare nelle cartelle monitorate");
                        await SendCompletedAsync(notificationService, 0, errorFiles);
                        return;
                    }

                    await SendUpdateAsync(notificationService, $"📊 Trovati {totalFiles} file da elaborare");
                    
                    // Processa i file in ogni cartella
                    foreach (var folder in foldersToMonitor)
                    {
                        if (!Directory.Exists(folder))
                            continue;

                        await SendUpdateAsync(notificationService, $"🔍 Scansione della cartella {folder}...");
                        
                        try
                        {
                            // Ottieni tutti i file nelle cartelle monitorate
                            var files = Directory.GetFiles(folder, "*.*", SearchOption.AllDirectories)
                                .Where(f => _supportedExtensions.Contains(Path.GetExtension(f).ToLowerInvariant()))
                                .ToList();
                            
                            for (int i = 0; i < files.Count; i++)
                            {
                                var file = files[i];
                                
                                // Aggiorna il progresso
                                await SendProgressAsync(notificationService, processedFiles + errorFiles, totalFiles);
                                
                                try
                                {
                                    // Verifica se il file è già stato elaborato
                                    var isProcessed = await documentService.IsDocumentProcessedAsync(file);
                                    if (isProcessed)
                                    {
                                        await SendUpdateAsync(notificationService, $"⏩ Il file {Path.GetFileName(file)} è già stato elaborato");
                                        processedFiles++;
                                        continue;
                                    }
                                    
                                    // Ottieni il tipo di documento in base all'estensione
                                    string extension = Path.GetExtension(file).ToLowerInvariant();
                                    var documentType = _extensionToTypeMap.TryGetValue(extension, out var type) 
                                        ? type 
                                        : DocumentType.OTHER;
                                    
                                    string fileTypeMessage = extension switch
                                    {
                                        ".pdf" => "PDF",
                                        ".docx" or ".doc" => "Word",
                                        ".xlsx" or ".xls" => "Excel",
                                        ".eml" or ".msg" => "Email",
                                        _ => extension.TrimStart('.')
                                    };
                                    
                                    await SendUpdateAsync(notificationService, $"📄 Elaborazione del file {fileTypeMessage} {Path.GetFileName(file)}...");
                                    
                                    // Estrai il contenuto del file
                                    await SendUpdateAsync(notificationService, $"📝 Estrazione del testo da {Path.GetFileName(file)}...");
                                    string fileContent = await ExtractFileContentAsync(file);
                                    
                                    if (string.IsNullOrEmpty(fileContent))
                                    {
                                        await SendUpdateAsync(notificationService, $"⚠️ Nessun contenuto estratto da {Path.GetFileName(file)}");
                                        fileContent = $"[Nessun contenuto estratto dal file {extension}]";
                                    }
                                    
                                    // Prova ad analizzare il documento con Mistral AI
                                    DocumentCategory category = uncategorizedCategory;
                                    string tags = "da_catalogare";
                                    bool isConfidential = false;
                                    
                                    try
                                    {
                                        await SendUpdateAsync(notificationService, $"🧠 Analisi AI del contenuto di {Path.GetFileName(file)}...");
                                        
                                        // Ottieni il servizio MistralAI
                                        var mistralService = scope.ServiceProvider.GetService<MistralAIService>();
                                        if (mistralService != null)
                                        {
                                            try
                                            {
                                                var analysisResult = await mistralService.AnalyzeDocumentAsync(
                                                    fileContent,
                                                    Path.GetFileName(file),
                                                    documentType.ToString());
                                                    
                                                if (analysisResult != null && !string.IsNullOrEmpty(analysisResult.Categoria))
                                                {
                                                    // Pulisci il nome della categoria
                                                    string cleanCategoryName = analysisResult.Categoria.Trim();
                                                    if (string.IsNullOrEmpty(cleanCategoryName))
                                                    {
                                                        cleanCategoryName = "Categoria Generica";
                                                    }

                                                    // Prepara la descrizione della categoria
                                                    string description = !string.IsNullOrEmpty(analysisResult.DescrizioneCategoria) 
                                                        ? analysisResult.DescrizioneCategoria 
                                                        : $"Categoria creata automaticamente per {Path.GetFileName(file)}";

                                                    Console.WriteLine($"DEBUG: Tentativo di creazione/ricerca categoria '{cleanCategoryName}'");
                                                    _logger.LogInformation($"Tentativo di creazione/ricerca categoria '{cleanCategoryName}'");
                                                    
                                                    // Prima verifica se la categoria esiste già usando il categoryService
                                                    var existingCategory = await categoryService.GetCategoryByNameAsync(cleanCategoryName);
                                                    if (existingCategory != null)
                                                    {
                                                        category = existingCategory;
                                                        Console.WriteLine($"DEBUG: Categoria esistente trovata tramite service: '{category.Name}' (ID: {category.Id})");
                                                        _logger.LogInformation($"Categoria esistente trovata tramite service: '{category.Name}' (ID: {category.Id})");
                                                        await SendUpdateAsync(notificationService, $"✅ Categoria trovata per {Path.GetFileName(file)}: {category.Name}");
                                                        
                                                        // Aggiorna i tag e la confidenzialità
                                                        tags = string.Join(",", analysisResult.Tags ?? new List<string>());
                                                        isConfidential = analysisResult.Confidenziale;
                                                    }
                                                    else
                                                    {
                                                        Console.WriteLine($"DEBUG: Categoria '{cleanCategoryName}' non trovata, tentativo di creazione");
                                                        
                                                        // Prova a creare la categoria con diversi metodi, in ordine di preferenza
                                                        try
                                                        {
                                                            // 1. Prova a creare la categoria usando il categoryService
                                                            Console.WriteLine($"DEBUG: Tentativo 1 - Creazione categoria tramite CategoryService");
                                                            try
                                                            {
                                                                category = await categoryService.CreateCategoryAsync(new DocumentCategory 
                                                                { 
                                                                    Name = cleanCategoryName, 
                                                                    Description = description 
                                                                });
                                                                
                                                                if (category != null && category.Id > 0)
                                                                {
                                                                    Console.WriteLine($"DEBUG: Categoria creata con successo tramite service: '{category.Name}' (ID: {category.Id})");
                                                                    _logger.LogInformation($"Categoria creata con successo tramite service: '{category.Name}' (ID: {category.Id})");
                                                                    await SendUpdateAsync(notificationService, $"🆕 Creata nuova categoria '{category.Name}' per {Path.GetFileName(file)}");
                                                                    
                                                                    // Aggiorna i tag e la confidenzialità
                                                                    tags = string.Join(",", analysisResult.Tags ?? new List<string>());
                                                                    isConfidential = analysisResult.Confidenziale;
                                                                    
                                                                    // Categoria creata con successo, possiamo uscire
                                                                    goto CategoryCreated;
                                                                }
                                                                else
                                                                {
                                                                    Console.WriteLine($"DEBUG: Categoria non creata correttamente tramite service (ID non valido o null)");
                                                                }
                                                            }
                                                            catch (Exception serviceEx)
                                                            {
                                                                Console.WriteLine($"DEBUG: Errore durante la creazione tramite service: {serviceEx.Message}");
                                                                if (serviceEx.InnerException != null)
                                                                {
                                                                    Console.WriteLine($"DEBUG: Inner exception: {serviceEx.InnerException.Message}");
                                                                }
                                                            }
                                                            
                                                            // 2. Prova a creare la categoria direttamente con DbContext
                                                            Console.WriteLine($"DEBUG: Tentativo 2 - Creazione categoria tramite DbContext diretto");
                                                            try
                                                            {
                                                                // Ottieni un nuovo DbContext
                                                                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                                                                
                                                                // Verifica ancora una volta se la categoria esiste
                                                                var dbCategory = await dbContext.DocumentCategories
                                                                    .FirstOrDefaultAsync(c => c.Name.ToLower() == cleanCategoryName.ToLower());
                                                                    
                                                                if (dbCategory != null)
                                                                {
                                                                    category = dbCategory;
                                                                    Console.WriteLine($"DEBUG: Categoria esistente trovata tramite DbContext: '{category.Name}' (ID: {category.Id})");
                                                                    _logger.LogInformation($"Categoria esistente trovata tramite DbContext: '{category.Name}' (ID: {category.Id})");
                                                                }
                                                                else
                                                                {
                                                                    // Crea una nuova categoria
                                                                    var newCategory = new DocumentCategory
                                                                    {
                                                                        Name = cleanCategoryName,
                                                                        Description = description
                                                                    };
                                                                    
                                                                    Console.WriteLine($"DEBUG: Aggiunta nuova categoria al DbContext: '{newCategory.Name}'");
                                                                    
                                                                    // Aggiungi la categoria al database
                                                                    dbContext.DocumentCategories.Add(newCategory);
                                                                    
                                                                    Console.WriteLine($"DEBUG: Salvataggio modifiche al database...");
                                                                    await dbContext.SaveChangesAsync();
                                                                    
                                                                    category = newCategory;
                                                                    Console.WriteLine($"DEBUG: Categoria creata con successo tramite DbContext: '{category.Name}' (ID: {category.Id})");
                                                                    _logger.LogInformation($"Categoria creata con successo tramite DbContext: '{category.Name}' (ID: {category.Id})");
                                                                }
                                                                
                                                                await SendUpdateAsync(notificationService, $"🆕 Creata nuova categoria '{category.Name}' per {Path.GetFileName(file)}");
                                                                
                                                                // Aggiorna i tag e la confidenzialità
                                                                tags = string.Join(",", analysisResult.Tags ?? new List<string>());
                                                                isConfidential = analysisResult.Confidenziale;
                                                                
                                                                // Categoria creata con successo, possiamo uscire
                                                                goto CategoryCreated;
                                                            }
                                                            catch (Exception dbEx)
                                                            {
                                                                Console.WriteLine($"DEBUG: Errore durante la creazione tramite DbContext: {dbEx.Message}");
                                                                if (dbEx.InnerException != null)
                                                                {
                                                                    Console.WriteLine($"DEBUG: Inner exception: {dbEx.InnerException.Message}");
                                                                }
                                                            }
                                                            
                                                            // 3. Prova a creare la categoria con SQL diretto
                                                            Console.WriteLine($"DEBUG: Tentativo 3 - Creazione categoria tramite SQL diretto");
                                                            try
                                                            {
                                                                // Ottieni un nuovo DbContext
                                                                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                                                                
                                                                // Crea un ID univoco per la categoria
                                                                var categoryId = Guid.NewGuid().ToString();
                                                                
                                                                // Crea la query SQL
                                                                var sql = $@"
                                                                    IF NOT EXISTS (SELECT 1 FROM DocumentCategories WHERE Name = '{cleanCategoryName}')
                                                                    BEGIN
                                                                        INSERT INTO DocumentCategories (Id, Name, Description)
                                                                        VALUES ({dbContext.DocumentCategories.Count() + 1}, '{cleanCategoryName}', '{description}')
                                                                    END
                                                                    
                                                                    SELECT * FROM DocumentCategories WHERE Name = '{cleanCategoryName}'
                                                                ";
                                                                
                                                                Console.WriteLine($"DEBUG: Esecuzione query SQL: {sql}");
                                                                
                                                                // Esegui la query
                                                                var result = await dbContext.DocumentCategories
                                                                    .FromSqlRaw(sql)
                                                                    .FirstOrDefaultAsync();
                                                                
                                                                if (result != null)
                                                                {
                                                                    category = result;
                                                                    Console.WriteLine($"DEBUG: Categoria creata/trovata con successo tramite SQL: '{category.Name}' (ID: {category.Id})");
                                                                    _logger.LogInformation($"Categoria creata/trovata con successo tramite SQL: '{category.Name}' (ID: {category.Id})");
                                                                    
                                                                    await SendUpdateAsync(notificationService, $"🆕 Creata nuova categoria '{category.Name}' per {Path.GetFileName(file)}");
                                                                    
                                                                    // Aggiorna i tag e la confidenzialità
                                                                    tags = string.Join(",", analysisResult.Tags ?? new List<string>());
                                                                    isConfidential = analysisResult.Confidenziale;
                                                                    
                                                                    // Categoria creata con successo, possiamo uscire
                                                                    goto CategoryCreated;
                                                                }
                                                                else
                                                                {
                                                                    Console.WriteLine($"DEBUG: Nessun risultato dalla query SQL");
                                                                }
                                                            }
                                                            catch (Exception sqlEx)
                                                            {
                                                                Console.WriteLine($"DEBUG: Errore durante la creazione tramite SQL: {sqlEx.Message}");
                                                                if (sqlEx.InnerException != null)
                                                                {
                                                                    Console.WriteLine($"DEBUG: Inner exception: {sqlEx.InnerException.Message}");
                                                                }
                                                            }
                                                            
                                                            // Se arriviamo qui, tutti i tentativi sono falliti
                                                            throw new Exception($"Impossibile creare la categoria '{cleanCategoryName}' dopo multipli tentativi");
                                                        }
                                                        catch (Exception ex)
                                                        {
                                                            _logger.LogError(ex, $"Errore durante la gestione della categoria '{cleanCategoryName}'");
                                                            Console.WriteLine($"ERRORE: Impossibile gestire la categoria '{cleanCategoryName}'. Dettaglio: {ex.Message}");
                                                            
                                                            if (ex.InnerException != null)
                                                            {
                                                                Console.WriteLine($"ERRORE INTERNO: {ex.InnerException.Message}");
                                                            }
                                                            
                                                            // Fallback alla categoria "Da Catalogare"
                                                            await SendUpdateAsync(notificationService, $"⚠️ Impossibile creare la categoria '{cleanCategoryName}', utilizzo 'Da Catalogare'");
                                                            
                                                            try
                                                            {
                                                                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                                                                
                                                                // Cerca la categoria "Da Catalogare"
                                                                var defaultCategory = await dbContext.DocumentCategories
                                                                    .FirstOrDefaultAsync(c => c.Name == "Da Catalogare");
                                                                    
                                                                if (defaultCategory != null)
                                                                {
                                                                    category = defaultCategory;
                                                                    _logger.LogInformation($"Utilizzo categoria 'Da Catalogare' per {file}");
                                                                    await SendUpdateAsync(notificationService, $"⚠️ Fallback alla categoria 'Da Catalogare' per {Path.GetFileName(file)}");
                                                                }
                                                                else
                                                                {
                                                                    // Crea la categoria "Da Catalogare"
                                                                    var newDefaultCategory = new DocumentCategory
                                                                    {
                                                                        Name = "Da Catalogare",
                                                                        Description = "Documenti in attesa di catalogazione"
                                                                    };
                                                                    
                                                                    dbContext.DocumentCategories.Add(newDefaultCategory);
                                                                    await dbContext.SaveChangesAsync();
                                                                    
                                                                    category = newDefaultCategory;
                                                                    _logger.LogInformation("Creata categoria predefinita 'Da Catalogare'");
                                                                    await SendUpdateAsync(notificationService, $"🆕 Creata categoria predefinita 'Da Catalogare' per {Path.GetFileName(file)}");
                                                                }
                                                            }
                                                            catch (Exception innerEx)
                                                            {
                                                                _logger.LogError(innerEx, "Errore critico durante la gestione della categoria predefinita");
                                                                await SendUpdateAsync(notificationService, $"❌ ERRORE CRITICO: Impossibile gestire la categoria predefinita. {innerEx.Message}");
                                                                throw;
                                                            }
                                                        }
                                                    }
                                                    
                                                    // Etichetta per il goto
                                                    CategoryCreated:
                                                    Console.WriteLine($"DEBUG: Categoria finale utilizzata: '{category?.Name}' (ID: {category?.Id})");
                                                }
                                                else
                                                {
                                                    await SendUpdateAsync(notificationService, $"⚠️ Nessuna categoria suggerita dall'AI per {Path.GetFileName(file)}, utilizzo 'Da Catalogare'");
                                                    _logger.LogWarning($"Nessuna categoria suggerita dall'AI per {file}");
                                                    
                                                    // Utilizza la categoria "Da Catalogare"
                                                    try
                                                    {
                                                        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                                                        
                                                        // Cerca la categoria "Da Catalogare"
                                                        var defaultCategory = await dbContext.DocumentCategories
                                                            .FirstOrDefaultAsync(c => c.Name == "Da Catalogare");
                                                            
                                                        if (defaultCategory != null)
                                                        {
                                                            category = defaultCategory;
                                                            _logger.LogInformation($"Utilizzo categoria 'Da Catalogare' per {file}");
                                                        }
                                                        else
                                                        {
                                                            // Crea la categoria "Da Catalogare"
                                                            var newDefaultCategory = new DocumentCategory
                                                            {
                                                                Name = "Da Catalogare",
                                                                Description = "Documenti in attesa di catalogazione"
                                                            };
                                                            
                                                            dbContext.DocumentCategories.Add(newDefaultCategory);
                                                            await dbContext.SaveChangesAsync();
                                                            
                                                            category = newDefaultCategory;
                                                            _logger.LogInformation("Creata categoria predefinita 'Da Catalogare'");
                                                            await SendUpdateAsync(notificationService, $"🆕 Creata categoria predefinita 'Da Catalogare' per {Path.GetFileName(file)}");
                                                        }
                                                    }
                                                    catch (Exception innerEx)
                                                    {
                                                        _logger.LogError(innerEx, "Errore critico durante la gestione della categoria predefinita");
                                                        await SendUpdateAsync(notificationService, $"❌ ERRORE CRITICO: Impossibile gestire la categoria predefinita. {innerEx.Message}");
                                                        throw;
                                                    }
                                                }
                                            }
                                            catch (Exception ex)
                                            {
                                                string errorMsg = $"Errore durante l'analisi del documento {Path.GetFileName(file)}";
                                                _logger.LogError(ex, errorMsg);
                                                await SendUpdateAsync(notificationService, $"⚠️ {errorMsg}: {ex.Message}. Utilizzo categoria 'Da Catalogare'");
                                                errorDetails[file] = ex.Message;
                                            }
                                        }
                                        else
                                        {
                                            await SendUpdateAsync(notificationService, $"⚠️ Servizio MistralAI non disponibile, utilizzo categoria 'Da Catalogare'");
                                            _logger.LogWarning("Servizio MistralAI non disponibile");
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        // In caso di errore nell'analisi, usa la categoria "Da Catalogare"
                                        string errorMsg = $"Errore durante l'analisi del documento {Path.GetFileName(file)}";
                                        _logger.LogError(ex, errorMsg);
                                        await SendUpdateAsync(notificationService, $"⚠️ {errorMsg}: {ex.Message}. Utilizzo categoria 'Da Catalogare'");
                                        errorDetails[file] = ex.Message;
                                    }
                                    
                                    // Copia il file nella cartella DocumentsStorage
                                    await SendUpdateAsync(notificationService, $"📋 Copia del file nella cartella di archiviazione...");
                                    string destinationPath = await CopyFileToDocumentStorageAsync(file, defaultUserId);
                                    
                                    // Verifica se il file è stato copiato in una nuova posizione
                                    bool fileCopied = destinationPath != file;
                                    if (fileCopied)
                                    {
                                        await SendUpdateAsync(notificationService, $"✅ File copiato nella cartella di archiviazione");
                                    }
                                    else
                                    {
                                        await SendUpdateAsync(notificationService, $"⚠️ Impossibile copiare il file, verrà utilizzato il percorso originale");
                                    }
                                    
                                    var document = new Document
                                    {
                                        Name = Path.GetFileName(file),
                                        Description = "Documento importato automaticamente",
                                        FileType = documentType,
                                        FilePath = destinationPath, // Usa il nuovo percorso
                                        FileSize = new FileInfo(file).Length / 1024, // Dimensione in KB
                                        UploadDate = DateTime.Now,
                                        CategoryId = category?.Id ?? 0, // Usa 0 o un altro valore predefinito appropriato
                                        Tags = tags,
                                        IsConfidential = isConfidential,
                                        UploadedById = defaultUserId
                                    };
                                    
                                    // Salva il documento nel database
                                    using (var documentScope = _serviceProvider.CreateScope())
                                    {
                                        var dbContext = documentScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                                        dbContext.Documents.Add(document);
                                        await dbContext.SaveChangesAsync();
                                        
                                        await SendUpdateAsync(notificationService, $"✅ Documento salvato nel database con ID: {document.Id}");
                                        _logger.LogInformation($"Documento creato con ID {document.Id} per il file {file}");
                                        
                                        // Ora che il documento è stato salvato nel database, possiamo cancellare il file originale
                                        if (fileCopied)
                                        {
                                            await DeleteOriginalFileAsync(file, notificationService);
                                        }
                                        
                                        // Incrementa il contatore dei file processati
                                        processedFiles++;
                                        
                                        // Messaggio di conferma finale
                                        await SendUpdateAsync(notificationService, $"✅ Documento {Path.GetFileName(file)} catalogato con successo nella categoria {category?.Name}");
                                    }
                                }
                                catch (Exception ex)
                                {
                                    string errorMsg = $"Errore durante l'elaborazione del file {Path.GetFileName(file)}";
                                    _logger.LogError(ex, errorMsg);
                                    await SendUpdateAsync(notificationService, $"❌ ERRORE: {errorMsg}. Dettagli: {ex.Message}");
                                    errorDetails[file] = ex.Message;
                                    errorFiles++;
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            string errorMsg = $"Errore durante la scansione della cartella {folder}";
                            _logger.LogError(ex, errorMsg);
                            await SendUpdateAsync(notificationService, $"❌ ERRORE: {errorMsg}. Dettagli: {ex.Message}");
                            errorDetails[folder] = ex.Message;
                            errorFiles++;
                        }
                    }
                    
                    // Invia il riepilogo finale
                    await SendCompletedAsync(notificationService, processedFiles, errorFiles);
                    
                    if (processedFiles > 0)
                    {
                        await SendUpdateAsync(notificationService, $"✅ Catalogazione completata: {processedFiles} documenti elaborati" + (errorFiles > 0 ? $", {errorFiles} errori" : ""));
                        
                        if (errorFiles > 0 && errorDetails.Count > 0)
                        {
                            await SendUpdateAsync(notificationService, "⚠️ Dettagli degli errori:");
                            foreach (var error in errorDetails.Take(5)) // Mostra solo i primi 5 errori per non sovraccaricare il log
                            {
                                await SendUpdateAsync(notificationService, $"  - {Path.GetFileName(error.Key)}: {error.Value}");
                            }
                            
                            if (errorDetails.Count > 5)
                            {
                                await SendUpdateAsync(notificationService, $"  ... e altri {errorDetails.Count - 5} errori. Controlla i log per maggiori dettagli.");
                            }
                        }
                    }
                    else if (errorFiles > 0)
                    {
                        await SendUpdateAsync(notificationService, $"❌ Catalogazione completata con errori: {errorFiles} errori");
                        
                        if (errorDetails.Count > 0)
                        {
                            await SendUpdateAsync(notificationService, "⚠️ Dettagli degli errori:");
                            foreach (var error in errorDetails.Take(5))
                            {
                                await SendUpdateAsync(notificationService, $"  - {Path.GetFileName(error.Key)}: {error.Value}");
                            }
                            
                            if (errorDetails.Count > 5)
                            {
                                await SendUpdateAsync(notificationService, $"  ... e altri {errorDetails.Count - 5} errori. Controlla i log per maggiori dettagli.");
                            }
                        }
                    }
                    else
                    {
                        await SendUpdateAsync(notificationService, "ℹ️ Nessun nuovo documento trovato nelle cartelle monitorate");
                    }
                }
            }
            catch (Exception ex)
            {
                string errorMsg = "Errore critico durante la catalogazione dei documenti";
                _logger.LogError(ex, errorMsg);
                await SendUpdateAsync(notificationService, $"❌ ERRORE CRITICO: {errorMsg}. Dettagli: {ex.Message}");
                await SendUpdateAsync(notificationService, $"Stack trace: {ex.StackTrace}");
                await SendCompletedAsync(notificationService, processedFiles, errorFiles + 1);
                throw;
            }
        }

        /// <summary>
        /// Estrae il contenuto testuale da un file
        /// </summary>
        private async Task<string> ExtractFileContentAsync(string filePath)
        {
            try
            {
                var extension = Path.GetExtension(filePath).ToLowerInvariant();
                
                // Per i file di testo
                if (extension == ".txt")
                {
                    return await File.ReadAllTextAsync(filePath);
                }
                // Per i file PDF
                else if (extension == ".pdf")
                {
                    try
                    {
                        StringBuilder text = new StringBuilder();
                        
                        using (PdfReader pdfReader = new PdfReader(filePath))
                        using (PdfDocument pdfDoc = new PdfDocument(pdfReader))
                        {
                            int numberOfPages = pdfDoc.GetNumberOfPages();
                            
                            // Estrai il testo da ogni pagina
                            for (int i = 1; i <= numberOfPages; i++)
                            {
                                var page = pdfDoc.GetPage(i);
                                var strategy = new SimpleTextExtractionStrategy();
                                string pageText = PdfTextExtractor.GetTextFromPage(page, strategy);
                                text.AppendLine(pageText);
                            }
                        }
                        
                        return LimitTextLength(text.ToString());
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Errore durante l'estrazione del testo dal PDF: {filePath}");
                        return $"[Errore durante l'estrazione del testo dal PDF: {ex.Message}]";
                    }
                }
                // Per i file DOCX
                else if (extension == ".docx" || extension == ".doc")
                {
                    try
                    {
                        StringBuilder text = new StringBuilder();
                        
                        using (WordprocessingDocument doc = WordprocessingDocument.Open(filePath, false))
                        {
                            var body = doc.MainDocumentPart?.Document.Body;
                            if (body != null)
                            {
                                text.Append(body.InnerText);
                            }
                        }
                        
                        return LimitTextLength(text.ToString());
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Errore durante l'estrazione del testo dal DOCX: {filePath}");
                        return $"[Errore durante l'estrazione del testo dal DOCX: {ex.Message}]";
                    }
                }
                // Per i file XLSX
                else if (extension == ".xlsx" || extension == ".xls")
                {
                    try
                    {
                        StringBuilder text = new StringBuilder();
                        
                        // Configura EPPlus per funzionare in modalità non commerciale
                        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                        
                        using (var package = new ExcelPackage(new FileInfo(filePath)))
                        {
                            foreach (var worksheet in package.Workbook.Worksheets)
                            {
                                text.AppendLine($"Foglio: {worksheet.Name}");
                                
                                // Determina l'intervallo di celle utilizzate
                                int rowCount = worksheet.Dimension?.Rows ?? 0;
                                int colCount = worksheet.Dimension?.Columns ?? 0;
                                
                                for (int row = 1; row <= Math.Min(rowCount, 100); row++) // Limita a 100 righe
                                {
                                    for (int col = 1; col <= colCount; col++)
                                    {
                                        var cellValue = worksheet.Cells[row, col].Value;
                                        if (cellValue != null)
                                        {
                                            text.Append(cellValue.ToString() + "\t");
                                        }
                                        else
                                        {
                                            text.Append("\t");
                                        }
                                    }
                                    text.AppendLine();
                                }
                                
                                if (rowCount > 100)
                                {
                                    text.AppendLine("... [altre righe omesse] ...");
                                }
                            }
                        }
                        
                        return LimitTextLength(text.ToString());
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Errore durante l'estrazione del testo dall'XLSX: {filePath}");
                        return $"[Errore durante l'estrazione del testo dall'XLSX: {ex.Message}]";
                    }
                }
                // Per i file CSV
                else if (extension == ".csv")
                {
                    try
                    {
                        return LimitTextLength(await File.ReadAllTextAsync(filePath));
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Errore durante l'estrazione del testo dal CSV: {filePath}");
                        return $"[Errore durante l'estrazione del testo dal CSV: {ex.Message}]";
                    }
                }
                // Per i file EML
                else if (extension == ".eml" || extension == ".msg")
                {
                    try
                    {
                        StringBuilder text = new StringBuilder();
                        
                        var message = MimeMessage.Load(filePath);
                        
                        text.AppendLine($"Da: {message.From}");
                        text.AppendLine($"A: {message.To}");
                        text.AppendLine($"Cc: {message.Cc}");
                        text.AppendLine($"Oggetto: {message.Subject}");
                        text.AppendLine($"Data: {message.Date}");
                        text.AppendLine();
                        
                        // Estrai il testo dal corpo dell'email
                        if (message.TextBody != null)
                        {
                            text.AppendLine(message.TextBody);
                        }
                        else if (message.HtmlBody != null)
                        {
                            // Semplice rimozione dei tag HTML
                            string htmlBody = message.HtmlBody;
                            string plainText = System.Text.RegularExpressions.Regex.Replace(htmlBody, "<[^>]*>", "");
                            text.AppendLine(plainText);
                        }
                        
                        // Aggiungi informazioni sugli allegati
                        if (message.Attachments.Any())
                        {
                            text.AppendLine("\nAllegati:");
                            foreach (var attachment in message.Attachments)
                            {
                                text.AppendLine($"- {attachment.ContentDisposition?.FileName ?? "Allegato senza nome"}");
                            }
                        }
                        
                        return LimitTextLength(text.ToString());
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Errore durante l'estrazione del testo dall'EML: {filePath}");
                        return $"[Errore durante l'estrazione del testo dall'EML: {ex.Message}]";
                    }
                }
                
                // Per gli altri tipi di file, restituisci un messaggio che indica che è necessario un parser specifico
                return $"[Questo è un file {extension}. È necessario un parser specifico per estrarre il contenuto.]";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Errore durante l'estrazione del contenuto dal file: {filePath}");
                return string.Empty;
            }
        }

        /// <summary>
        /// Limita la lunghezza del testo estratto per evitare problemi con l'API
        /// </summary>
        private string LimitTextLength(string text, int maxLength = 10000)
        {
            if (string.IsNullOrEmpty(text))
                return string.Empty;
                
            return text.Length <= maxLength ? text : text.Substring(0, maxLength) + "...";
        }
        
        /// <summary>
        /// Crea una nuova categoria direttamente nel database
        /// </summary>
        private async Task<DocumentCategory> CreateCategoryDirectlyAsync(string name, string description)
        {
            _logger.LogInformation($"Tentativo di creazione diretta della categoria '{name}'");
            Console.WriteLine($"DEBUG: Tentativo di creazione diretta della categoria '{name}'");
            
            // Crea un nuovo scope per ottenere un nuovo DbContext
            using (var scope = _serviceProvider.CreateScope())
            {
                try
                {
                    // Ottieni un nuovo DbContext
                    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                    
                    // Verifica se la categoria esiste già
                    var existingCategory = await dbContext.DocumentCategories
                        .FirstOrDefaultAsync(c => c.Name.ToLower() == name.ToLower());
                        
                    if (existingCategory != null)
                    {
                        _logger.LogInformation($"Categoria '{name}' già esistente, ID: {existingCategory.Id}");
                        Console.WriteLine($"DEBUG: Categoria '{name}' già esistente, ID: {existingCategory.Id}");
                        return existingCategory;
                    }
                    
                    // Crea la nuova categoria
                    var newCategory = new DocumentCategory
                    {
                        Name = name,
                        Description = description
                    };
                    
                    // Aggiungi la categoria al DbContext e salva
                    dbContext.DocumentCategories.Add(newCategory);
                    await dbContext.SaveChangesAsync();
                    
                    _logger.LogInformation($"Categoria '{name}' creata con successo, ID: {newCategory.Id}");
                    Console.WriteLine($"DEBUG: Categoria '{name}' creata con successo, ID: {newCategory.Id}");
                    
                    return newCategory;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Errore durante la creazione diretta della categoria '{name}'");
                    Console.WriteLine($"DEBUG ERROR: Errore durante la creazione diretta della categoria '{name}'. Errore: {ex.Message}");
                    Console.WriteLine($"DEBUG ERROR: Stack trace: {ex.StackTrace}");
                    
                    if (ex.InnerException != null)
                    {
                        Console.WriteLine($"DEBUG ERROR: Inner exception: {ex.InnerException.Message}");
                    }
                    
                    throw; // Rilancia l'eccezione
                }
            }
        }

        /// <summary>
        /// Crea una nuova categoria utilizzando l'API
        /// </summary>
        private async Task<DocumentCategory> CreateCategoryViaApiAsync(string name, string description)
        {
            _logger.LogInformation($"Tentativo di creazione della categoria '{name}' tramite API");
            Console.WriteLine($"DEBUG: Tentativo di creazione della categoria '{name}' tramite API");
            
            try
            {
                // Prepara i dati della categoria
                var categoryData = new DocumentCategory
                {
                    Name = name,
                    Description = description
                };
                
                // Invia la richiesta all'API
                var response = await _httpClient.PostAsJsonAsync("api/categories/create", categoryData);
                
                // Verifica se la richiesta è andata a buon fine
                if (response.IsSuccessStatusCode)
                {
                    // Leggi la risposta
                    var category = await response.Content.ReadFromJsonAsync<DocumentCategory>();
                    
                    _logger.LogInformation($"Categoria '{name}' creata con successo tramite API, ID: {category?.Id}");
                    Console.WriteLine($"DEBUG: Categoria '{name}' creata con successo tramite API, ID: {category?.Id}");
                    
                    return category ?? throw new InvalidOperationException($"Impossibile creare la categoria '{name}': risposta API valida ma oggetto category null");
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError($"Errore durante la creazione della categoria '{name}' tramite API. Codice: {response.StatusCode}, Errore: {errorContent}");
                    Console.WriteLine($"DEBUG ERROR: Errore durante la creazione della categoria '{name}' tramite API. Codice: {response.StatusCode}, Errore: {errorContent}");
                    
                    throw new Exception($"Errore durante la creazione della categoria '{name}' tramite API. Codice: {response.StatusCode}, Errore: {errorContent}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Errore durante la creazione della categoria '{name}' tramite API");
                Console.WriteLine($"DEBUG ERROR: Errore durante la creazione della categoria '{name}' tramite API. Errore: {ex.Message}");
                Console.WriteLine($"DEBUG ERROR: Stack trace: {ex.StackTrace}");
                
                throw; // Rilancia l'eccezione
            }
        }

        /// <summary>
        /// Copia un file nella cartella DocumentsStorage e restituisce il nuovo percorso
        /// </summary>
        private async Task<string> CopyFileToDocumentStorageAsync(string sourceFilePath, string userId)
        {
            try
            {
                // Crea la directory dei documenti se non esiste
                string documentsPath = Path.Combine(_webHostEnvironment.ContentRootPath, "DocumentsStorage");
                if (!Directory.Exists(documentsPath))
                {
                    Directory.CreateDirectory(documentsPath);
                }

                // Crea una sottodirectory per l'utente
                string userPath = Path.Combine(documentsPath, userId);
                if (!Directory.Exists(userPath))
                {
                    Directory.CreateDirectory(userPath);
                }

                // Genera un nome file univoco
                string fileName = $"{Guid.NewGuid()}_{Path.GetFileName(sourceFilePath)}";
                string destinationPath = Path.Combine(userPath, fileName);

                // Copia il file
                await Task.Run(() => File.Copy(sourceFilePath, destinationPath, true));
                
                _logger.LogInformation($"File copiato da {sourceFilePath} a {destinationPath}");
                
                return destinationPath;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Errore durante la copia del file {sourceFilePath}");
                // In caso di errore, restituisci il percorso originale
                return sourceFilePath;
            }
        }
        
        /// <summary>
        /// Cancella un file dalla cartella di origine dopo che è stato catalogato
        /// </summary>
        private async Task DeleteOriginalFileAsync(string sourceFilePath, CatalogNotificationService? notificationService)
        {
            try
            {
                if (File.Exists(sourceFilePath))
                {
                    await Task.Run(() => File.Delete(sourceFilePath));
                    _logger.LogInformation($"File originale eliminato: {sourceFilePath}");
                    
                    if (notificationService != null)
                    {
                        await SendUpdateAsync(notificationService, $"✅ File originale eliminato dalla cartella di origine");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, $"Impossibile eliminare il file originale: {sourceFilePath}");
                
                if (notificationService != null)
                {
                    await SendUpdateAsync(notificationService, $"⚠️ Impossibile eliminare il file originale dalla cartella di origine");
                }
            }
        }
    }
} 