using AiDbMaster.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace AiDbMaster.Attributes
{
    /// <summary>
    /// Attribute per proteggere controller e action con permessi granulari
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    public class RequirePermissionAttribute : Attribute, IAsyncAuthorizationFilter
    {
        /// <summary>
        /// Nome della risorsa da verificare
        /// </summary>
        public string Resource { get; set; }

        /// <summary>
        /// Azione richiesta: "View", "Create", "Edit", "Delete"
        /// </summary>
        public string Action { get; set; }

        public RequirePermissionAttribute(string resource, string action = "View")
        {
            Resource = resource;
            Action = action;
        }

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            // Verifica che l'utente sia autenticato
            if (context.HttpContext.User?.Identity?.IsAuthenticated != true)
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            // Ottieni il ResourcePermissionService
            var permissionService = context.HttpContext.RequestServices
                .GetService<IResourcePermissionService>();

            if (permissionService == null)
            {
                context.Result = new StatusCodeResult(500);
                return;
            }

            // Verifica il permesso specifico
            bool hasPermission = Action.ToLower() switch
            {
                "view" => await permissionService.CanViewAsync(context.HttpContext.User, Resource),
                "create" => await permissionService.CanCreateAsync(context.HttpContext.User, Resource),
                "edit" => await permissionService.CanEditAsync(context.HttpContext.User, Resource),
                "delete" => await permissionService.CanDeleteAsync(context.HttpContext.User, Resource),
                _ => false
            };

            if (!hasPermission)
            {
                // Utente non ha permessi: restituisci 403 Forbidden
                context.Result = new ForbidResult();
            }
        }
    }
}

