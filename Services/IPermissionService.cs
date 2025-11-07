using System.Security.Claims;

namespace AiDbMaster.Services
{
    /// <summary>
    /// Interfaccia per il servizio di gestione permessi risorse
    /// </summary>
    public interface IResourcePermissionService
    {
        /// <summary>
        /// Verifica se l'utente può visualizzare una risorsa
        /// </summary>
        Task<bool> CanViewAsync(ClaimsPrincipal user, string resourceName);

        /// <summary>
        /// Verifica se l'utente può creare su una risorsa
        /// </summary>
        Task<bool> CanCreateAsync(ClaimsPrincipal user, string resourceName);

        /// <summary>
        /// Verifica se l'utente può modificare una risorsa
        /// </summary>
        Task<bool> CanEditAsync(ClaimsPrincipal user, string resourceName);

        /// <summary>
        /// Verifica se l'utente può eliminare da una risorsa
        /// </summary>
        Task<bool> CanDeleteAsync(ClaimsPrincipal user, string resourceName);

        /// <summary>
        /// Ottiene tutti i permessi dell'utente (cached)
        /// </summary>
        Task<Dictionary<string, ResourcePermissions>> GetUserPermissionsAsync(ClaimsPrincipal user);

        /// <summary>
        /// Invalida la cache dei permessi per un utente
        /// </summary>
        void InvalidateUserCache(string userId);
    }

    /// <summary>
    /// Rappresenta i permessi su una risorsa specifica
    /// </summary>
    public class ResourcePermissions
    {
        public bool CanView { get; set; }
        public bool CanCreate { get; set; }
        public bool CanEdit { get; set; }
        public bool CanDelete { get; set; }
    }
}

