using AiDbMaster.Attributes;
using AiDbMaster.Data;
using AiDbMaster.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace AiDbMaster.Services
{
    /// <summary>
    /// Servizio per la registrazione automatica delle risorse
    /// scansionando i controller con l'attribute [RegisterResource]
    /// </summary>
    public static class ResourceAutoRegistration
    {
        /// <summary>
        /// Scansiona tutti i controller e registra le risorse automaticamente
        /// </summary>
        public static async Task<ResourceAutoRegistrationResult> RegisterAllResourcesAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

            var result = new ResourceAutoRegistrationResult();

            try
            {
                logger.LogInformation("🔍 Avvio scansione controller per auto-registrazione risorse...");

                // Trova tutti i controller con [RegisterResource]
                var assembly = Assembly.GetExecutingAssembly();
                var controllerTypes = assembly.GetTypes()
                    .Where(t => t.IsClass && 
                               !t.IsAbstract && 
                               t.Name.EndsWith("Controller") &&
                               t.GetCustomAttribute<RegisterResourceAttribute>() != null)
                    .ToList();

                logger.LogInformation($"📋 Trovati {controllerTypes.Count} controller con [RegisterResource]");

                // Trova il ruolo Admin
                var adminRole = await roleManager.FindByNameAsync(UserRoles.Admin);
                if (adminRole == null)
                {
                    logger.LogWarning("⚠️ Ruolo Admin non trovato, skip auto-registrazione permessi");
                }

                foreach (var controllerType in controllerTypes)
                {
                    var attribute = controllerType.GetCustomAttribute<RegisterResourceAttribute>()!;

                    // Verifica se la risorsa esiste già
                    var existingResource = await context.Resources
                        .FirstOrDefaultAsync(r => r.Name == attribute.Name);

                    if (existingResource != null)
                    {
                        result.SkippedResources.Add($"{attribute.DisplayName} (già esistente)");
                        continue;
                    }

                    // Crea la nuova risorsa
                    var newResource = new Resource
                    {
                        Name = attribute.Name,
                        DisplayName = attribute.DisplayName,
                        Description = attribute.Description ?? $"Controller {attribute.DisplayName}",
                        MenuIcon = attribute.MenuIcon ?? "bi-file-earmark",
                        MenuOrder = attribute.MenuOrder,
                        ParentResourceId = attribute.ParentResourceId > 0 ? attribute.ParentResourceId : null, // 0 = nessun parent
                        IsMenuGroup = attribute.IsMenuGroup,
                        IsActive = true,
                        IsConfigured = false, // Safe by default
                        CreatedDate = DateTime.Now,
                        CreatedBy = "System"
                    };

                    context.Resources.Add(newResource);
                    await context.SaveChangesAsync(); // Salva per ottenere l'ID

                    logger.LogInformation($"✅ Risorsa '{attribute.DisplayName}' registrata con ID {newResource.Id}");
                    result.RegisteredResources.Add(attribute.DisplayName);

                    // Crea permesso completo per Admin
                    if (adminRole != null)
                    {
                        var adminPermission = new Permission
                        {
                            RoleId = adminRole.Id,
                            ResourceId = newResource.Id,
                            CanView = true,
                            CanCreate = !attribute.IsMenuGroup, // Menu group non hanno CRUD
                            CanEdit = !attribute.IsMenuGroup,
                            CanDelete = !attribute.IsMenuGroup,
                            CreatedDate = DateTime.Now,
                            ModifiedBy = "System"
                        };

                        context.Permissions.Add(adminPermission);
                        await context.SaveChangesAsync();

                        logger.LogInformation($"   → Permessi Admin creati per '{attribute.DisplayName}'");
                    }
                }

                result.Success = true;
                logger.LogInformation($"✅ Auto-registrazione completata: {result.RegisteredResources.Count} nuove, {result.SkippedResources.Count} saltate");
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.ErrorMessage = ex.Message;
                logger.LogError(ex, "❌ Errore durante auto-registrazione risorse");
            }

            return result;
        }
    }

    /// <summary>
    /// Risultato dell'operazione di auto-registrazione
    /// </summary>
    public class ResourceAutoRegistrationResult
    {
        public bool Success { get; set; }
        public List<string> RegisteredResources { get; set; } = new List<string>();
        public List<string> SkippedResources { get; set; } = new List<string>();
        public string? ErrorMessage { get; set; }
    }
}

