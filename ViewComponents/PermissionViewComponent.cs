using AiDbMaster.Services;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AiDbMaster.ViewComponents
{
    /// <summary>
    /// ViewComponent per controllare i permessi nelle View
    /// Uso: @await Component.InvokeAsync("Permission", new { resource = "AnagraficaClienti", action = "View" })
    /// </summary>
    public class PermissionViewComponent : ViewComponent
    {
        private readonly IResourcePermissionService _permissionService;

        public PermissionViewComponent(IResourcePermissionService permissionService)
        {
            _permissionService = permissionService;
        }

        /// <summary>
        /// Verifica se l'utente ha un permesso specifico
        /// </summary>
        /// <param name="resource">Nome della risorsa</param>
        /// <param name="action">Azione: View, Create, Edit, Delete</param>
        /// <returns>True se l'utente ha il permesso</returns>
        public async Task<IViewComponentResult> InvokeAsync(string resource, string action)
        {
            var user = UserClaimsPrincipal;
            
            if (user == null || !user.Identity?.IsAuthenticated == true)
            {
                return Content("false");
            }

            bool hasPermission = action.ToLower() switch
            {
                "view" => await _permissionService.CanViewAsync(user, resource),
                "create" => await _permissionService.CanCreateAsync(user, resource),
                "edit" => await _permissionService.CanEditAsync(user, resource),
                "delete" => await _permissionService.CanDeleteAsync(user, resource),
                _ => false
            };

            return Content(hasPermission.ToString().ToLower());
        }
    }
}

