using System.Security.Claims;

namespace AiDbMaster.Services
{
    /// <summary>
    /// Interfaccia per il servizio di filtraggio dati basato su utente/ruolo
    /// </summary>
    public interface IDataFilterService
    {
        /// <summary>
        /// Applica filtri automatici a una query basandosi sull'utente e la risorsa
        /// Per gli Agenti: filtra per CodiceAgente
        /// Per Admin: nessun filtro
        /// </summary>
        Task<IQueryable<T>> ApplyUserFilterAsync<T>(IQueryable<T> query, ClaimsPrincipal user, string resourceName) where T : class;

        /// <summary>
        /// Verifica se l'utente ha filtri configurati per una risorsa
        /// </summary>
        Task<bool> HasFiltersAsync(ClaimsPrincipal user, string resourceName);
    }
}

