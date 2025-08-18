using AiDbMaster.Models;
using AiDbMaster.Services;
using AiDbMaster.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System.IO;

namespace AiDbMaster.Controllers
{
    [Authorize]
    public class DocumentController : Controller
    {
        private readonly DocumentService _documentService;
        private readonly CategoryService _categoryService;
        private readonly PermissionService _permissionService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<DocumentController> _logger;
        private readonly IConfiguration _configuration;
        private readonly FolderMonitorService _folderMonitorService;

        public DocumentController(
            DocumentService documentService,
            CategoryService categoryService,
            PermissionService permissionService,
            UserManager<ApplicationUser> userManager,
            ILogger<DocumentController> logger,
            IConfiguration configuration,
            FolderMonitorService folderMonitorService)
        {
            _documentService = documentService;
            _categoryService = categoryService;
            _permissionService = permissionService;
            _userManager = userManager;
            _logger = logger;
            _configuration = configuration;
            _folderMonitorService = folderMonitorService;
        }

        // GET: Document
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isAdmin = User.IsInRole(UserRoles.Admin);
            var isManager = User.IsInRole(UserRoles.Manager);

            // Gli amministratori e i manager vedono tutti i documenti
            if (isAdmin || isManager)
            {
                var allDocuments = await _documentService.GetAllDocumentsAsync();
                return View(allDocuments);
            }
            else
            {
                if (userId == null)
                {
                    return RedirectToAction("Login", "Account");
                }

                // Gli utenti normali vedono solo i propri documenti
                var userDocuments = await _documentService.GetDocumentsByUserIdAsync(userId);
                return View(userDocuments);
            }
        }

        // GET: Document/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return Forbid();
            }
            var document = await _documentService.GetDocumentByIdAsync(id);

            if (document == null)
            {
                return NotFound();
            }

            // Verifica i permessi
            if (!await _documentService.HasPermissionAsync(id, userId, PermissionType.Read))
            {
                return Forbid();
            }

            return View(document);
        }

        // GET: Document/Create
        public async Task<IActionResult> Create()
        {
            var categories = await _categoryService.GetAllCategoriesAsync();
            ViewBag.Categories = new SelectList(categories, "Id", "Name");
            return View();
        }

        // POST: Document/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(DocumentCreateViewModel model)
        {
            if (ModelState.IsValid)
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (userId == null)
                {
                    return RedirectToAction("Login", "Account");
                }
                
                try
                {
                    var document = new Document
                    {
                        Name = model.Name,
                        Description = model.Description,
                        CategoryId = model.CategoryId,
                        IsConfidential = model.IsConfidential,
                        Tags = model.Tags
                    };

                    if (model.File != null)
                    {
                        await _documentService.UploadDocumentAsync(model.File, document, userId);
                        TempData["SuccessMessage"] = "Documento caricato con successo!";
                        return RedirectToAction(nameof(Index));
                    }
                    else
                    {
                        ModelState.AddModelError("File", "È necessario selezionare un file da caricare.");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Errore durante il caricamento del documento");
                    ModelState.AddModelError(string.Empty, $"Errore durante il caricamento del documento: {ex.Message}");
                }
            }

            var categories = await _categoryService.GetAllCategoriesAsync();
            ViewBag.Categories = new SelectList(categories, "Id", "Name", model.CategoryId);
            return View(model);
        }

        // GET: Document/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return Forbid();
            }
            var document = await _documentService.GetDocumentByIdAsync(id);

            if (document == null)
            {
                return NotFound();
            }

            // Verifica i permessi
            if (!await _documentService.HasPermissionAsync(id, userId, PermissionType.Edit))
            {
                return Forbid();
            }

            var model = new DocumentEditViewModel
            {
                Id = document.Id,
                Name = document.Name,
                Description = document.Description,
                CategoryId = document.CategoryId,
                IsConfidential = document.IsConfidential,
                Tags = document.Tags
            };

            var categories = await _categoryService.GetAllCategoriesAsync();
            ViewBag.Categories = new SelectList(categories, "Id", "Name", document.CategoryId);
            return View(model);
        }

        // POST: Document/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, DocumentEditViewModel model)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return Forbid();
            }
            
            // Verifica i permessi
            if (!await _documentService.HasPermissionAsync(id, userId, PermissionType.Edit))
            {
                return Forbid();
            }

            if (ModelState.IsValid)
            {
                var document = await _documentService.GetDocumentByIdAsync(id);
                if (document == null)
                {
                    return NotFound();
                }

                document.Name = model.Name;
                document.Description = model.Description;
                document.CategoryId = model.CategoryId;
                document.IsConfidential = model.IsConfidential;
                document.Tags = model.Tags;

                await _documentService.UpdateDocumentAsync(document);
                return RedirectToAction(nameof(Index));
            }

            var categories = await _categoryService.GetAllCategoriesAsync();
            ViewBag.Categories = new SelectList(categories, "Id", "Name", model.CategoryId);
            return View(model);
        }

        // GET: Document/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return Forbid();
            }
            var document = await _documentService.GetDocumentByIdAsync(id);

            if (document == null)
            {
                return NotFound();
            }

            // Verifica i permessi
            if (!await _documentService.HasPermissionAsync(id, userId, PermissionType.Delete))
            {
                return Forbid();
            }

            return View(document);
        }

        // POST: Document/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return Forbid();
            }
            
            // Verifica i permessi
            if (!await _documentService.HasPermissionAsync(id, userId, PermissionType.Delete))
            {
                return Forbid();
            }

            await _documentService.DeleteDocumentAsync(id);
            return RedirectToAction(nameof(Index));
        }

        // GET: Document/Download/5
        public async Task<IActionResult> Download(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return Forbid();
            }
            var document = await _documentService.GetDocumentByIdAsync(id);

            if (document == null)
            {
                return NotFound();
            }

            // Verifica i permessi
            if (!await _documentService.HasPermissionAsync(id, userId, PermissionType.Read))
            {
                return Forbid();
            }

            // Verifica che il file esista
            if (!System.IO.File.Exists(document.FilePath))
            {
                return NotFound("Il file non è più disponibile.");
            }

            // Determina il tipo MIME in base al tipo di documento
            string contentType = "application/octet-stream";
            switch (document.FileType)
            {
                case DocumentType.PDF:
                    contentType = "application/pdf";
                    break;
                case DocumentType.DOCX:
                    contentType = "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
                    break;
                case DocumentType.TXT:
                    contentType = "text/plain";
                    break;
                case DocumentType.EMAIL:
                    contentType = "message/rfc822";
                    break;
                case DocumentType.EXCEL:
                    contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                    break;
            }

            // Leggi il file e restituiscilo come FileResult
            var fileBytes = System.IO.File.ReadAllBytes(document.FilePath);
            var fileName = System.IO.Path.GetFileName(document.FilePath);
            return File(fileBytes, contentType, fileName);
        }

        // GET: Document/Permissions/5
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> Permissions(int id)
        {
            var document = await _documentService.GetDocumentByIdAsync(id);
            if (document == null)
            {
                return NotFound();
            }

            var permissions = await _permissionService.GetPermissionsByDocumentIdAsync(id);
            var users = await _userManager.Users.ToListAsync();

            var model = new DocumentPermissionsViewModel
            {
                DocumentId = id,
                DocumentName = document.Name ?? string.Empty,
                Permissions = permissions,
                AvailableUsers = users.Where(u => !permissions.Any(p => p.UserId == u.Id)).ToList()
            };

            return View(model);
        }

        // POST: Document/GrantPermission
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> GrantPermission(GrantPermissionViewModel model)
        {
            if (ModelState.IsValid)
            {
                var grantedById = User.FindFirstValue(ClaimTypes.NameIdentifier);
                
                var permission = new DocumentPermission
                {
                    DocumentId = model.DocumentId,
                    UserId = model.UserId,
                    PermissionType = model.PermissionType,
                    GrantedById = grantedById,
                    GrantedDate = DateTime.Now,
                    Document = null
                };

                await _permissionService.GrantPermissionAsync(permission);
                return RedirectToAction(nameof(Permissions), new { id = model.DocumentId });
            }

            return RedirectToAction(nameof(Permissions), new { id = model.DocumentId });
        }

        // POST: Document/RevokePermission
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> RevokePermission(int documentId, string userId)
        {
            await _permissionService.RevokePermissionAsync(documentId, userId);
            return RedirectToAction(nameof(Permissions), new { id = documentId });
        }

        /// <summary>
        /// Avvia la catalogazione manuale dei documenti dalle cartelle monitorate
        /// </summary>
        [HttpPost]
        [Authorize(Roles = UserRoles.Admin + "," + UserRoles.Manager)]
        public async Task<IActionResult> CatalogDocuments()
        {
            try
            {
                // Ottieni il servizio di notifica
                var notificationService = HttpContext.RequestServices.GetRequiredService<CatalogNotificationService>();
                
                // Invia un messaggio di test
                await notificationService.SendUpdateAsync("Test di connessione SignalR");
                
                // Avvia la catalogazione in background
                _ = Task.Run(async () => 
                {
                    try 
                    {
                        using (var scope = HttpContext.RequestServices.CreateScope())
                        {
                            // Ottieni il servizio di monitoraggio
                            var folderMonitorService = scope.ServiceProvider.GetRequiredService<FolderMonitorService>();
                            await folderMonitorService.ScanFoldersAsync();
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Errore durante la scansione delle cartelle");
                        await notificationService.SendUpdateAsync($"❌ ERRORE durante la scansione: {ex.Message}");
                        await notificationService.SendCompletedAsync(0, 1);
                    }
                });
                
                // Restituisci immediatamente una risposta JSON
                return Json(new { success = true, message = "Catalogazione avviata con successo" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante l'avvio della catalogazione dei documenti");
                return Json(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Ottiene il tipo di documento in base all'estensione
        /// </summary>
        private DocumentType GetDocumentTypeFromExtension(string extension)
        {
            return extension.ToLowerInvariant() switch
            {
                ".pdf" => DocumentType.PDF,
                ".docx" or ".doc" => DocumentType.DOCX,
                ".txt" => DocumentType.TXT,
                ".eml" or ".msg" => DocumentType.EMAIL,
                ".xls" or ".xlsx" or ".csv" => DocumentType.EXCEL,
                _ => DocumentType.OTHER
            };
        }
    }
} 