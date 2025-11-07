using AiDbMaster.Data;
using AiDbMaster.Models;
using AiDbMaster.Services;
using AiDbMaster.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
        /// Sincronizza le risorse scansionando i controller con [RegisterResource]
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> SyncResources()
        {
            try
            {
                _logger.LogInformation("Richiesta sincronizzazione risorse da utente {User}", User.Identity?.Name);

                var result = await ResourceAutoRegistration.RegisterAllResourcesAsync(HttpContext.RequestServices);

                if (!result.Success)
                {
                    return Json(new
                    {
                        success = false,
                        message = $"Errore durante la sincronizzazione: {result.ErrorMessage}"
                    });
                }

                var message = result.RegisteredResources.Count > 0
                    ? $"Sincronizzazione completata! {result.RegisteredResources.Count} nuove risorse registrate, {result.SkippedResources.Count} già esistenti."
                    : $"Tutte le risorse sono già sincronizzate ({result.SkippedResources.Count} risorse trovate).";

                return Json(new
                {
                    success = true,
                    message = message,
                    registeredCount = result.RegisteredResources.Count,
                    skippedCount = result.SkippedResources.Count,
                    registered = result.RegisteredResources,
                    skipped = result.SkippedResources
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante sincronizzazione risorse");
                return StatusCode(500, new { success = false, message = "Errore durante la sincronizzazione" });
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

