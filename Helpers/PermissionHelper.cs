using AiDbMaster.Services;
using System.Security.Claims;

namespace AiDbMaster.Helpers
{
    /// <summary>
    /// Helper statico per controllare i permessi nelle view.
    /// Fornisce metodi sincroni e asincroni per verificare i permessi.
    /// </summary>
    public static class PermissionHelper
    {
        /// <summary>
        /// Verifica se l'utente può visualizzare una risorsa (sincrono, usa cache)
        /// </summary>
        public static bool CanView(ClaimsPrincipal user, string resourceName, IResourcePermissionService permissionService)
        {
            return permissionService.CanViewAsync(user, resourceName).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Verifica se l'utente può creare nella risorsa (sincrono, usa cache)
        /// </summary>
        public static bool CanCreate(ClaimsPrincipal user, string resourceName, IResourcePermissionService permissionService)
        {
            return permissionService.CanCreateAsync(user, resourceName).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Verifica se l'utente può modificare nella risorsa (sincrono, usa cache)
        /// </summary>
        public static bool CanEdit(ClaimsPrincipal user, string resourceName, IResourcePermissionService permissionService)
        {
            return permissionService.CanEditAsync(user, resourceName).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Verifica se l'utente può eliminare nella risorsa (sincrono, usa cache)
        /// </summary>
        public static bool CanDelete(ClaimsPrincipal user, string resourceName, IResourcePermissionService permissionService)
        {
            return permissionService.CanDeleteAsync(user, resourceName).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Ottiene tutti i permessi dell'utente (sincrono, usa cache)
        /// </summary>
        public static Dictionary<string, ResourcePermissions> GetUserPermissions(ClaimsPrincipal user, IResourcePermissionService permissionService)
        {
            return permissionService.GetUserPermissionsAsync(user).GetAwaiter().GetResult();
        }
    }
}

