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

                await EnsureContabilitaCantiereAsync(context, roleManager, logger);

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

        /// <summary>
        /// La contabilità non è la scheda cantiere: ha un permesso suo.
        /// Chi già apriva i cantieri lo riceve, tranne il ruolo Gestione Cantieri,
        /// che può vedere e modificare la scheda ma non creare cantieri e non entrare in contabilità.
        /// </summary>
        private static async Task EnsureContabilitaCantiereAsync(
            ApplicationDbContext context,
            RoleManager<IdentityRole> roleManager,
            ILogger logger)
        {
            const string nomeRisorsa = "ContabilitaCantieri";
            const string ruoloScheda = "Gestione Cantieri";

            var risorsa = await context.Resources.FirstOrDefaultAsync(r => r.Name == nomeRisorsa);
            if (risorsa == null)
            {
                risorsa = new Resource
                {
                    Name = nomeRisorsa,
                    DisplayName = "Contabilità cantiere",
                    Description = "Contabilità iniziale e finale del cantiere",
                    MenuIcon = "bi-calculator",
                    MenuOrder = 2,
                    IsMenuGroup = false,
                    IsActive = true,
                    IsConfigured = true,
                    CreatedDate = DateTime.Now,
                    CreatedBy = "System"
                };
                context.Resources.Add(risorsa);
                await context.SaveChangesAsync();
                logger.LogInformation("Risorsa Contabilità cantiere registrata.");
            }

            var giaConPermessi = await context.Permissions.AnyAsync(p => p.ResourceId == risorsa.Id);
            if (!giaConPermessi)
            {
                var elenco = await context.Resources.FirstOrDefaultAsync(r => r.Name == "GestioneCantieri");
                if (elenco != null)
                {
                    var permessiElenco = await context.Permissions
                        .Where(p => p.ResourceId == elenco.Id)
                        .Join(context.Roles, p => p.RoleId, r => r.Id, (p, r) => new { Permesso = p, r.Name })
                        .ToListAsync();

                    foreach (var voce in permessiElenco)
                    {
                        if (string.Equals(voce.Name, ruoloScheda, StringComparison.OrdinalIgnoreCase))
                            continue;

                        context.Permissions.Add(new Permission
                        {
                            RoleId = voce.Permesso.RoleId,
                            ResourceId = risorsa.Id,
                            CanView = voce.Permesso.CanView,
                            CanCreate = voce.Permesso.CanCreate,
                            CanEdit = voce.Permesso.CanEdit,
                            CanDelete = voce.Permesso.CanDelete,
                            CreatedDate = DateTime.Now,
                            ModifiedBy = "System"
                        });
                    }

                    await context.SaveChangesAsync();
                }
            }

            var ruolo = await roleManager.FindByNameAsync(ruoloScheda);
            if (ruolo == null)
                return;

            await ImpostaPermessoAsync(context, ruolo.Id, "GestioneCantieri",
                puoVedere: true, puoCreare: false, puoModificare: true, puoCancellare: false);
            await ImpostaPermessoAsync(context, ruolo.Id, nomeRisorsa,
                puoVedere: false, puoCreare: false, puoModificare: false, puoCancellare: false);
        }

        private static async Task ImpostaPermessoAsync(
            ApplicationDbContext context,
            string roleId,
            string nomeRisorsa,
            bool puoVedere,
            bool puoCreare,
            bool puoModificare,
            bool puoCancellare)
        {
            var risorsa = await context.Resources.FirstOrDefaultAsync(r => r.Name == nomeRisorsa);
            if (risorsa == null)
                return;

            var permesso = await context.Permissions
                .FirstOrDefaultAsync(p => p.RoleId == roleId && p.ResourceId == risorsa.Id);

            if (permesso == null)
            {
                context.Permissions.Add(new Permission
                {
                    RoleId = roleId,
                    ResourceId = risorsa.Id,
                    CanView = puoVedere,
                    CanCreate = puoCreare,
                    CanEdit = puoModificare,
                    CanDelete = puoCancellare,
                    CreatedDate = DateTime.Now,
                    ModifiedBy = "System"
                });
            }
            else
            {
                permesso.CanView = puoVedere;
                permesso.CanCreate = puoCreare;
                permesso.CanEdit = puoModificare;
                permesso.CanDelete = puoCancellare;
                permesso.ModifiedDate = DateTime.Now;
                permesso.ModifiedBy = "System";
            }

            await context.SaveChangesAsync();
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

