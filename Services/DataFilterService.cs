using AiDbMaster.Data;
using AiDbMaster.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using System.Reflection;
using System.Security.Claims;

namespace AiDbMaster.Services
{
    /// <summary>
    /// Servizio per applicare filtri automatici ai dati basandosi su ruolo e utente
    /// </summary>
    public class DataFilterService : IDataFilterService
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<DataFilterService> _logger;

        public DataFilterService(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            ILogger<DataFilterService> logger)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
        }

        /// <summary>
        /// Applica filtri automatici basati su ruolo utente
        /// </summary>
        public async Task<IQueryable<T>> ApplyUserFilterAsync<T>(
            IQueryable<T> query, 
            ClaimsPrincipal user, 
            string resourceName) where T : class
        {
            if (user?.Identity?.IsAuthenticated != true)
                return query.Take(0); // Nessun risultato per utenti non autenticati

            // Admin vede sempre tutto
            if (user.IsInRole(UserRoles.Admin))
                return query;

            // Ottieni l'utente corrente
            var currentUser = await _userManager.GetUserAsync(user);
            if (currentUser == null)
                return query.Take(0);

            // Ottieni i ruoli dell'utente
            var roles = await _userManager.GetRolesAsync(currentUser);

            // AGENTI: Filtra per CodiceAgente
            if (roles.Contains(UserRoles.Agenti) && currentUser.CodiceAgente.HasValue)
            {
                return ApplyAgenteFilter(query, currentUser.CodiceAgente.Value, resourceName);
            }

            // Altri ruoli: per ora nessun filtro specifico
            // Qui puoi aggiungere logiche per altri ruoli in futuro
            
            return query;
        }

        /// <summary>
        /// Applica il filtro specifico per gli Agenti
        /// </summary>
        private IQueryable<T> ApplyAgenteFilter<T>(IQueryable<T> query, short codiceAgente, string resourceName) where T : class
        {
            try
            {
                // Verifica se l'entità ha la proprietà CodiceAgente
                var entityType = typeof(T);
                var codiceAgenteProperty = entityType.GetProperty("CodiceAgente");

                if (codiceAgenteProperty != null)
                {
                    // Crea espressione lambda: entity => entity.CodiceAgente == codiceAgente
                    var parameter = Expression.Parameter(entityType, "entity");
                    var property = Expression.Property(parameter, codiceAgenteProperty);
                    var constant = Expression.Constant(codiceAgente);
                    var equals = Expression.Equal(property, constant);
                    var lambda = Expression.Lambda<Func<T, bool>>(equals, parameter);

                    // Applica il filtro
                    query = query.Where(lambda);

                    _logger.LogInformation(
                        "Filtro Agente applicato su {EntityType}.CodiceAgente = {CodiceAgente} per risorsa {ResourceName}",
                        entityType.Name,
                        codiceAgente,
                        resourceName
                    );
                }
                else
                {
                    _logger.LogWarning(
                        "Risorsa {ResourceName} (tipo {EntityType}) non ha proprietà CodiceAgente. Filtro Agente non applicato.",
                        resourceName,
                        entityType.Name
                    );
                }

                return query;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante applicazione filtro Agente su risorsa {ResourceName}", resourceName);
                return query;
            }
        }

        /// <summary>
        /// Verifica se l'utente ha filtri configurati
        /// </summary>
        public async Task<bool> HasFiltersAsync(ClaimsPrincipal user, string resourceName)
        {
            if (user?.Identity?.IsAuthenticated != true)
                return false;

            // Admin non ha filtri
            if (user.IsInRole(UserRoles.Admin))
                return false;

            var currentUser = await _userManager.GetUserAsync(user);
            if (currentUser == null)
                return false;

            var roles = await _userManager.GetRolesAsync(currentUser);

            // Agenti hanno sempre filtri se hanno un CodiceAgente
            if (roles.Contains(UserRoles.Agenti) && currentUser.CodiceAgente.HasValue)
                return true;

            // Controlla se ci sono filtri custom nel database
            var hasCustomFilters = await _context.UserDataFilters
                .AnyAsync(f => f.UserId == currentUser.Id && 
                              f.ResourceName == resourceName && 
                              f.IsActive);

            return hasCustomFilters;
        }
    }
}

