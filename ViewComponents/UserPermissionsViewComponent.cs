using AiDbMaster.Services;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AiDbMaster.ViewComponents
{
    /// <summary>
    /// View Component per ottenere i permessi dell'utente corrente.
    /// Usato principalmente per il menu laterale per nascondere voci senza permesso.
    /// 
    /// Uso in Razor: @await Component.InvokeAsync("UserPermissions")
    /// </summary>
    public class UserPermissionsViewComponent : ViewComponent
    {
        private readonly IResourcePermissionService _permissionService;

        public UserPermissionsViewComponent(IResourcePermissionService permissionService)
        {
            _permissionService = permissionService;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var user = (ClaimsPrincipal)User;

            if (!user.Identity?.IsAuthenticated == true)
            {
                return Content(string.Empty);
            }

            var permissions = await _permissionService.GetUserPermissionsAsync(user);
            return View(permissions);
        }
    }
}

