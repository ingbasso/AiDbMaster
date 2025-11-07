using AiDbMaster.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System.Security.Claims;

namespace AiDbMaster.TagHelpers
{
    /// <summary>
    /// Tag Helper per nascondere elementi HTML in base ai permessi
    /// Uso: <div permission-required="AnagraficaClienti:Create">Contenuto visibile solo con permesso Create</div>
    /// </summary>
    [HtmlTargetElement(Attributes = "permission-required")]
    public class PermissionTagHelper : TagHelper
    {
        private readonly IResourcePermissionService _permissionService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        /// <summary>
        /// Permesso richiesto nel formato "NomeRisorsa:Azione" (es: "AnagraficaClienti:Create")
        /// </summary>
        [HtmlAttributeName("permission-required")]
        public string PermissionRequired { get; set; } = string.Empty;

        public PermissionTagHelper(
            IResourcePermissionService permissionService,
            IHttpContextAccessor httpContextAccessor)
        {
            _permissionService = permissionService;
            _httpContextAccessor = httpContextAccessor;
        }

        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            var user = _httpContextAccessor.HttpContext?.User;

            if (user == null || !user.Identity?.IsAuthenticated == true)
            {
                // Utente non autenticato: nascondi elemento
                output.SuppressOutput();
                return;
            }

            // Parse "NomeRisorsa:Azione"
            var parts = PermissionRequired.Split(':');
            if (parts.Length != 2)
            {
                // Formato non valido: nascondi elemento
                output.SuppressOutput();
                return;
            }

            var resource = parts[0];
            var action = parts[1].ToLower();

            // Verifica il permesso
            bool hasPermission = action switch
            {
                "view" => await _permissionService.CanViewAsync(user, resource),
                "create" => await _permissionService.CanCreateAsync(user, resource),
                "edit" => await _permissionService.CanEditAsync(user, resource),
                "delete" => await _permissionService.CanDeleteAsync(user, resource),
                _ => false
            };

            if (!hasPermission)
            {
                // Utente non ha il permesso: nascondi elemento
                output.SuppressOutput();
            }
            else
            {
                // Utente ha il permesso: rimuovi l'attributo permission-required dall'output HTML
                output.Attributes.RemoveAll("permission-required");
            }
        }
    }
}
