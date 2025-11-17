using AiDbMaster.Data;
using AiDbMaster.Models;
using AiDbMaster.Services;
using AiDbMaster.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace AiDbMaster.Controllers
{
    /// <summary>
    /// Controller per la gestione dei permessi (Profilazione)
    /// </summary>
    [Authorize(Roles = "Admin")]
    public class PermissionManagementController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ILogger<PermissionManagementController> _logger;

        public PermissionManagementController(
            ApplicationDbContext context,
            RoleManager<IdentityRole> roleManager,
            ILogger<PermissionManagementController> logger)
        {
            _context = context;
            _roleManager = roleManager;
            _logger = logger;
        }

        /// <summary>
        /// Pagina principale di gestione permessi
        /// </summary>
        public async Task<IActionResult> Index()
        {
            try
            {
                // Ottieni tutti i ruoli
                var roles = await _roleManager.Roles.OrderBy(r => r.Name).ToListAsync();

                // Seleziona il primo ruolo (o Admin se esiste)
                var defaultRole = roles.FirstOrDefault(r => r.Name == "Admin") ?? roles.FirstOrDefault();

                if (defaultRole == null)
                {
                    _logger.LogWarning("Nessun ruolo trovato nel sistema");
                    return View(new PermissionManagementViewModel());
                }

                var viewModel = await BuildViewModelAsync(defaultRole.Id);
                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore nel caricamento della pagina di gestione permessi");
                TempData["ErrorMessage"] = "Errore nel caricamento della pagina di gestione permessi";
                return View(new PermissionManagementViewModel());
            }
        }

        /// <summary>
        /// Ottieni i permessi per un ruolo specifico (API JSON)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetPermissionsByRole(string roleId)
        {
            try
            {
                if (string.IsNullOrEmpty(roleId))
                {
                    return BadRequest(new { success = false, message = "ID ruolo mancante" });
                }

                var viewModel = await BuildViewModelAsync(roleId);
                
                // Costruisci la risposta JSON con i permessi
                var response = new
                {
                    success = true,
                    roleName = viewModel.CurrentRoleName,
                    resources = viewModel.ResourceTree.Select(node => new
                    {
                        resourceId = node.Resource.Id,
                        resourceName = node.Resource.DisplayName,
                        menuIcon = node.Resource.MenuIcon,
                        isMenuGroup = node.Resource.IsMenuGroup,
                        isConfigured = node.Resource.IsConfigured,
                        permission = viewModel.Permissions.TryGetValue(node.Resource.Id, out var perm) ? new
                        {
                            canView = perm.CanView,
                            canCreate = perm.CanCreate,
                            canEdit = perm.CanEdit,
                            canDelete = perm.CanDelete
                        } : new
                        {
                            canView = false,
                            canCreate = false,
                            canEdit = false,
                            canDelete = false
                        },
                        children = node.Children.Select(child => new
                        {
                            resourceId = child.Id,
                            resourceName = child.DisplayName,
                            menuIcon = child.MenuIcon,
                            isMenuGroup = child.IsMenuGroup,
                            isConfigured = child.IsConfigured,
                            permission = viewModel.Permissions.TryGetValue(child.Id, out var childPerm) ? new
                            {
                                canView = childPerm.CanView,
                                canCreate = childPerm.CanCreate,
                                canEdit = childPerm.CanEdit,
                                canDelete = childPerm.CanDelete
                            } : new
                            {
                                canView = false,
                                canCreate = false,
                                canEdit = false,
                                canDelete = false
                            }
                        }).ToList()
                    }).ToList(),
                    unconfiguredCount = viewModel.UnconfiguredResourcesCount
                };

                return Json(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore nel recupero dei permessi per il ruolo {RoleId}", roleId);
                return StatusCode(500, new { success = false, message = "Errore nel caricamento dei permessi" });
            }
        }

        /// <summary>
        /// Salva i permessi modificati
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> SavePermissions([FromBody] SavePermissionsRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.RoleId))
                {
                    return BadRequest(new { success = false, message = "ID ruolo mancante" });
                }

                var role = await _roleManager.FindByIdAsync(request.RoleId);
                if (role == null)
                {
                    return NotFound(new { success = false, message = "Ruolo non trovato" });
                }

                var userId = User.Identity?.Name ?? "System";
                var now = DateTime.Now;

                // Per ogni permesso nella richiesta
                foreach (var permDto in request.Permissions)
                {
                    // Verifica che la risorsa esista
                    var resource = await _context.Resources.FindAsync(permDto.ResourceId);
                    if (resource == null)
                    {
                        _logger.LogWarning("Risorsa {ResourceId} non trovata", permDto.ResourceId);
                        continue;
                    }

                    // Trova il permesso esistente o creane uno nuovo
                    var existingPerm = await _context.Permissions
                        .FirstOrDefaultAsync(p => p.RoleId == request.RoleId && p.ResourceId == permDto.ResourceId);

                    if (existingPerm != null)
                    {
                        // Aggiorna permesso esistente
                        existingPerm.CanView = permDto.CanView;
                        existingPerm.CanCreate = permDto.CanCreate;
                        existingPerm.CanEdit = permDto.CanEdit;
                        existingPerm.CanDelete = permDto.CanDelete;
                        existingPerm.ModifiedDate = now;
                        existingPerm.ModifiedBy = userId;
                        _context.Permissions.Update(existingPerm);
                    }
                    else
                    {
                        // Crea nuovo permesso
                        var newPerm = new Permission
                        {
                            RoleId = request.RoleId,
                            ResourceId = permDto.ResourceId,
                            CanView = permDto.CanView,
                            CanCreate = permDto.CanCreate,
                            CanEdit = permDto.CanEdit,
                            CanDelete = permDto.CanDelete,
                            CreatedDate = now,
                            ModifiedBy = userId
                        };
                        _context.Permissions.Add(newPerm);
                    }

                    // Marca la risorsa come configurata
                    if (!resource.IsConfigured)
                    {
                        resource.IsConfigured = true;
                        _context.Resources.Update(resource);
                    }
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation("Permessi salvati per ruolo {RoleName} da utente {User}", role.Name, userId);

                return Json(new
                {
                    success = true,
                    message = $"Permessi salvati con successo per il ruolo {role.Name}"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore nel salvataggio dei permessi");
                return StatusCode(500, new { success = false, message = "Errore nel salvataggio dei permessi" });
            }
        }

        /// <summary>
        /// Ottieni lista risorse non configurate
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetUnconfiguredResources()
        {
            try
            {
                var unconfigured = await _context.Resources
                    .Where(r => !r.IsConfigured && r.IsActive)
                    .OrderBy(r => r.MenuOrder)
                    .Select(r => new
                    {
                        r.Id,
                        r.Name,
                        r.DisplayName,
                        r.MenuIcon,
                        r.CreatedDate
                    })
                    .ToListAsync();

                return Json(new { success = true, resources = unconfigured });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore nel recupero delle risorse non configurate");
                return StatusCode(500, new { success = false, message = "Errore nel caricamento" });
            }
        }

        /// <summary>
        /// Aggiunge nuove risorse scansionando i controller con [RegisterResource] (solo quelle mancanti)
        /// Approccio INCREMENTALE - non elimina nulla, aggiunge solo le risorse nuove
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> AddNewResources()
        {
            try
            {
                _logger.LogInformation("Richiesta aggiunta nuove risorse da utente {User}", User.Identity?.Name);

                // Trova Admin role per creare permessi
                var adminRole = await _roleManager.FindByNameAsync(UserRoles.Admin);
                if (adminRole == null)
                {
                    return Json(new { success = false, message = "Ruolo Admin non trovato" });
                }

                // Scansiona i controller con [RegisterResource]
                var assembly = System.Reflection.Assembly.GetExecutingAssembly();
                var controllerTypes = assembly.GetTypes()
                    .Where(t => t.IsClass && 
                               !t.IsAbstract && 
                               t.Name.EndsWith("Controller") &&
                               t.GetCustomAttribute<Attributes.RegisterResourceAttribute>() != null)
                    .ToList();

                _logger.LogInformation("📋 Trovati {Count} controller con [RegisterResource]", controllerTypes.Count);

                int addedCount = 0;
                int skippedCount = 0;
                var addedResources = new List<string>();
                var skippedResources = new List<string>();

                foreach (var controllerType in controllerTypes)
                {
                    var attribute = controllerType.GetCustomAttribute<Attributes.RegisterResourceAttribute>()!;

                    // Verifica se la risorsa esiste già
                    var existingResource = await _context.Resources
                        .FirstOrDefaultAsync(r => r.Name == attribute.Name);

                    if (existingResource != null)
                    {
                        skippedResources.Add($"{attribute.DisplayName} (già esistente)");
                        skippedCount++;
                        continue;
                    }

                    // Determina il ParentResourceId reale dal database (se specificato)
                    int? parentResourceId = null;
                    if (attribute.ParentResourceId > 0)
                    {
                        var parentResource = await _context.Resources.FindAsync(attribute.ParentResourceId);
                        if (parentResource != null)
                        {
                            parentResourceId = parentResource.Id;
                        }
                        else
                        {
                            _logger.LogWarning("⚠️ Parent Resource ID {ParentId} non trovato per '{ResourceName}'", attribute.ParentResourceId, attribute.Name);
                        }
                    }

                    // Crea la nuova risorsa
                    var newResource = new Resource
                    {
                        Name = attribute.Name,
                        DisplayName = attribute.DisplayName,
                        Description = attribute.Description ?? $"Controller {attribute.DisplayName}",
                        MenuIcon = attribute.MenuIcon ?? "bi-file-earmark",
                        MenuOrder = attribute.MenuOrder,
                        ParentResourceId = parentResourceId,
                        IsMenuGroup = attribute.IsMenuGroup,
                        IsActive = true,
                        IsConfigured = true, // Nuove risorse sono subito configurabili
                        CreatedDate = DateTime.Now,
                        CreatedBy = User.Identity?.Name ?? "System"
                    };

                    _context.Resources.Add(newResource);
                    await _context.SaveChangesAsync(); // Salva per ottenere l'ID

                    _logger.LogInformation("✅ Risorsa '{ResourceName}' aggiunta con ID {ResourceId}", attribute.DisplayName, newResource.Id);
                    addedResources.Add(attribute.DisplayName);
                    addedCount++;

                    // Crea permesso completo per Admin
                    var adminPermission = new Permission
                    {
                        RoleId = adminRole.Id,
                        ResourceId = newResource.Id,
                        CanView = true,
                        CanCreate = !attribute.IsMenuGroup,
                        CanEdit = !attribute.IsMenuGroup,
                        CanDelete = !attribute.IsMenuGroup,
                        CreatedDate = DateTime.Now,
                        ModifiedBy = User.Identity?.Name ?? "System"
                    };

                    _context.Permissions.Add(adminPermission);
                    await _context.SaveChangesAsync();

                    _logger.LogInformation("   → Permessi Admin creati per '{ResourceName}'", attribute.DisplayName);
                }

                var message = addedCount > 0
                    ? $"✅ Aggiunte {addedCount} nuove risorse! {skippedCount} già esistenti."
                    : $"ℹ️ Nessuna nuova risorsa da aggiungere. Tutte le {skippedCount} risorse con [RegisterResource] esistono già.";

                return Json(new
                {
                    success = true,
                    message = message,
                    addedCount = addedCount,
                    skippedCount = skippedCount,
                    addedResources = addedResources,
                    skippedResources = skippedResources
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Errore durante aggiunta nuove risorse");
                return StatusCode(500, new { success = false, message = $"Errore: {ex.Message}" });
            }
        }

        /// <summary>
        /// Forza il reset completo e re-seed delle risorse dal PermissionSeeder
        /// ATTENZIONE: Questo cancellerà tutte le risorse e permessi esistenti!
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> ResetAndReseedResources()
        {
            try
            {
                _logger.LogWarning("⚠️ RESET RISORSE richiesto da utente {User}", User.Identity?.Name);

                // Trova Admin role per ricreare i permessi
                var adminRole = await _roleManager.FindByNameAsync(UserRoles.Admin);
                if (adminRole == null)
                {
                    return Json(new { success = false, message = "Ruolo Admin non trovato" });
                }

                // 1. Elimina tutti i permessi
                var allPermissions = await _context.Permissions.ToListAsync();
                _context.Permissions.RemoveRange(allPermissions);
                await _context.SaveChangesAsync();
                _logger.LogInformation("✅ Eliminati {Count} permessi", allPermissions.Count);

                // 2. Elimina tutte le risorse
                var allResources = await _context.Resources.ToListAsync();
                _context.Resources.RemoveRange(allResources);
                await _context.SaveChangesAsync();
                _logger.LogInformation("✅ Eliminate {Count} risorse", allResources.Count);

                // 3. Re-seed delle risorse
                await Data.PermissionSeeder.SeedPermissionsAsync(HttpContext.RequestServices);
                _logger.LogInformation("✅ Re-seed risorse completato");

                return Json(new
                {
                    success = true,
                    message = $"Reset completato! Eliminate {allResources.Count} risorse e {allPermissions.Count} permessi. Risorse ri-create dal seed."
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Errore durante reset risorse");
                return StatusCode(500, new { success = false, message = $"Errore durante il reset: {ex.Message}" });
            }
        }

        #region Helper Methods

        /// <summary>
        /// Costruisce il ViewModel completo
        /// </summary>
        private async Task<PermissionManagementViewModel> BuildViewModelAsync(string roleId)
        {
            var viewModel = new PermissionManagementViewModel
            {
                CurrentRoleId = roleId
            };

            // Carica tutti i ruoli
            viewModel.Roles = await _roleManager.Roles.OrderBy(r => r.Name).ToListAsync();

            // Trova il ruolo corrente
            var currentRole = viewModel.Roles.FirstOrDefault(r => r.Id == roleId);
            viewModel.CurrentRoleName = currentRole?.Name ?? "Unknown";

            // Carica tutte le risorse attive
            var allResources = await _context.Resources
                .Where(r => r.IsActive)
                .OrderBy(r => r.MenuOrder)
                .ToListAsync();

            // Costruisci l'albero gerarchico (solo risorse di primo livello)
            var rootResources = allResources.Where(r => r.ParentResourceId == null).ToList();
            
            foreach (var root in rootResources)
            {
                var node = new ResourceTreeNode
                {
                    Resource = root,
                    Children = allResources
                        .Where(r => r.ParentResourceId == root.Id)
                        .OrderBy(r => r.MenuOrder)
                        .ToList()
                };
                viewModel.ResourceTree.Add(node);
            }

            // Carica i permessi per il ruolo corrente
            var permissions = await _context.Permissions
                .Where(p => p.RoleId == roleId)
                .ToListAsync();

            viewModel.Permissions = permissions.ToDictionary(p => p.ResourceId);

            // Conta le risorse non configurate
            viewModel.UnconfiguredResourcesCount = await _context.Resources
                .CountAsync(r => !r.IsConfigured && r.IsActive);

            return viewModel;
        }

        #endregion
    }
}

