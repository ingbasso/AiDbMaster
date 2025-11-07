using AiDbMaster.Data;
using AiDbMaster.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System.Security.Claims;

namespace AiDbMaster.Services
{
    /// <summary>
    /// Servizio per la gestione e verifica dei permessi utente sulle risorse
    /// </summary>
    public class ResourcePermissionService : IResourcePermissionService
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IMemoryCache _cache;
        private readonly ILogger<ResourcePermissionService> _logger;
        private const int CacheExpirationMinutes = 30;

        public ResourcePermissionService(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            IMemoryCache cache,
            ILogger<ResourcePermissionService> logger)
        {
            _context = context;
            _userManager = userManager;
            _cache = cache;
            _logger = logger;
        }

        public async Task<bool> CanViewAsync(ClaimsPrincipal user, string resourceName)
        {
            return await CheckPermissionAsync(user, resourceName, p => p.CanView);
        }

        public async Task<bool> CanCreateAsync(ClaimsPrincipal user, string resourceName)
        {
            return await CheckPermissionAsync(user, resourceName, p => p.CanCreate);
        }

        public async Task<bool> CanEditAsync(ClaimsPrincipal user, string resourceName)
        {
            return await CheckPermissionAsync(user, resourceName, p => p.CanEdit);
        }

        public async Task<bool> CanDeleteAsync(ClaimsPrincipal user, string resourceName)
        {
            return await CheckPermissionAsync(user, resourceName, p => p.CanDelete);
        }

        /// <summary>
        /// Verifica un permesso specifico con auto-registrazione risorse
        /// </summary>
        private async Task<bool> CheckPermissionAsync(
            ClaimsPrincipal user, 
            string resourceName, 
            Func<ResourcePermissions, bool> permissionCheck)
        {
            if (user?.Identity?.IsAuthenticated != true)
                return false;

            // Admin ha sempre accesso a tutto
            if (user.IsInRole(UserRoles.Admin))
                return true;

            // Ottieni i permessi dell'utente (cached)
            var permissions = await GetUserPermissionsAsync(user);

            // Verifica se la risorsa esiste nei permessi
            if (permissions.TryGetValue(resourceName, out var resourcePermission))
            {
                return permissionCheck(resourcePermission);
            }

            // Se la risorsa non esiste, prova ad auto-registrarla
            await EnsureResourceExistsAsync(resourceName, user);

            // Ricarica i permessi dopo auto-registrazione
            var userId = _userManager.GetUserId(user);
            if (userId != null)
            {
                InvalidateUserCache(userId);
                permissions = await GetUserPermissionsAsync(user);

                if (permissions.TryGetValue(resourceName, out resourcePermission))
                {
                    return permissionCheck(resourcePermission);
                }
            }

            return false;
        }

        /// <summary>
        /// Ottiene tutti i permessi dell'utente con cache
        /// </summary>
        public async Task<Dictionary<string, ResourcePermissions>> GetUserPermissionsAsync(ClaimsPrincipal user)
        {
            if (user?.Identity?.IsAuthenticated != true)
                return new Dictionary<string, ResourcePermissions>();

            var userId = _userManager.GetUserId(user);
            if (userId == null)
                return new Dictionary<string, ResourcePermissions>();

            var cacheKey = $"UserPermissions_{userId}";

            // Prova a ottenere dalla cache
            if (_cache.TryGetValue(cacheKey, out Dictionary<string, ResourcePermissions>? cachedPermissions))
            {
                return cachedPermissions ?? new Dictionary<string, ResourcePermissions>();
            }

            // Admin ha permessi completi su tutto
            if (user.IsInRole(UserRoles.Admin))
            {
                var allResources = await _context.Resources
                    .Where(r => !r.IsMenuGroup && r.IsActive)
                    .ToListAsync();

                var adminPermissions = allResources.ToDictionary(
                    r => r.Name,
                    r => new ResourcePermissions
                    {
                        CanView = true,
                        CanCreate = true,
                        CanEdit = true,
                        CanDelete = true
                    }
                );

                // Cache per Admin
                _cache.Set(cacheKey, adminPermissions, TimeSpan.FromMinutes(CacheExpirationMinutes));
                return adminPermissions;
            }

            // Per altri ruoli, carica da database
            var userRoles = await _userManager.GetRolesAsync(await _userManager.GetUserAsync(user) ?? new ApplicationUser());
            
            var permissions = await _context.Permissions
                .Include(p => p.Resource)
                .Where(p => userRoles.Contains(p.Role.Name ?? string.Empty))
                .Where(p => p.Resource.IsActive && !p.Resource.IsMenuGroup)
                .GroupBy(p => p.Resource.Name)
                .Select(g => new
                {
                    ResourceName = g.Key,
                    Permissions = new ResourcePermissions
                    {
                        CanView = g.Any(p => p.CanView),
                        CanCreate = g.Any(p => p.CanCreate),
                        CanEdit = g.Any(p => p.CanEdit),
                        CanDelete = g.Any(p => p.CanDelete)
                    }
                })
                .ToDictionaryAsync(x => x.ResourceName, x => x.Permissions);

            // Cache i permessi
            _cache.Set(cacheKey, permissions, TimeSpan.FromMinutes(CacheExpirationMinutes));

            return permissions;
        }

        /// <summary>
        /// Assicura che una risorsa esista nel database (auto-registrazione)
        /// </summary>
        private async Task EnsureResourceExistsAsync(string resourceName, ClaimsPrincipal user)
        {
            try
            {
                // Controlla se la risorsa esiste già
                var exists = await _context.Resources.AnyAsync(r => r.Name == resourceName);
                if (exists)
                    return;

                var userId = _userManager.GetUserId(user);

                // Crea la nuova risorsa
                var resource = new Resource
                {
                    Name = resourceName,
                    DisplayName = resourceName,
                    Description = $"Risorsa auto-registrata: {resourceName}",
                    IsActive = true,
                    IsConfigured = false, // Non configurata
                    IsMenuGroup = false,
                    MenuOrder = 999, // In fondo
                    CreatedBy = userId,
                    CreatedDate = DateTime.Now
                };

                _context.Resources.Add(resource);
                await _context.SaveChangesAsync();

                // Crea permesso per Admin
                var adminRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == UserRoles.Admin);
                if (adminRole != null)
                {
                    var adminPermission = new Permission
                    {
                        RoleId = adminRole.Id,
                        ResourceId = resource.Id,
                        CanView = true,
                        CanCreate = true,
                        CanEdit = true,
                        CanDelete = true
                    };

                    _context.Permissions.Add(adminPermission);
                    await _context.SaveChangesAsync();
                }

                _logger.LogWarning(
                    "Risorsa '{ResourceName}' auto-registrata. Creata da utente {UserName}. Richiede configurazione permessi.",
                    resourceName,
                    user.Identity?.Name ?? "Unknown"
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante auto-registrazione risorsa '{ResourceName}'", resourceName);
            }
        }

        /// <summary>
        /// Invalida la cache dei permessi per un utente specifico
        /// </summary>
        public void InvalidateUserCache(string userId)
        {
            var cacheKey = $"UserPermissions_{userId}";
            _cache.Remove(cacheKey);
            _logger.LogInformation("Cache permessi invalidata per utente {UserId}", userId);
        }
    }
}
