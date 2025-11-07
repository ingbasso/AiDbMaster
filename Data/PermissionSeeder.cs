using AiDbMaster.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AiDbMaster.Data
{
    /// <summary>
    /// Classe per inizializzare risorse e permessi del sistema
    /// </summary>
    public static class PermissionSeeder
    {
        /// <summary>
        /// Seed delle risorse e permessi iniziali
        /// </summary>
        public static async Task SeedPermissionsAsync(IServiceProvider serviceProvider)
        {
            var context = serviceProvider.GetRequiredService<ApplicationDbContext>();
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var logger = serviceProvider.GetRequiredService<ILogger<Program>>();

            try
            {
                // Verifica se ci sono già risorse (evita duplicati)
                if (await context.Resources.AnyAsync())
                {
                    logger.LogInformation("Risorse già presenti nel database, skip seed.");
                    return;
                }

                logger.LogInformation("Inizio seed risorse e permessi...");

                // ===== DEFINIZIONE RISORSE SEGUENDO ORDINE MENU =====

                var resources = new List<Resource>
                {
                    // 1. Dashboard
                    new Resource 
                    { 
                        Name = "Dashboard", 
                        DisplayName = "Dashboard", 
                        Description = "Dashboard principale del sistema",
                        MenuIcon = "bi-house-door", 
                        MenuOrder = 1, 
                        ParentResourceId = null, 
                        IsMenuGroup = false,
                        IsConfigured = true
                    },

                    // 2. GRUPPO TABELLE
                    new Resource 
                    { 
                        Name = "Tabelle", 
                        DisplayName = "Tabelle", 
                        Description = "Gruppo tabelle anagrafiche",
                        MenuIcon = "bi-table", 
                        MenuOrder = 2, 
                        ParentResourceId = null, 
                        IsMenuGroup = true,
                        IsConfigured = true
                    },
                    new Resource 
                    { 
                        Name = "AnagraficaArticoli", 
                        DisplayName = "Anagrafica Articoli", 
                        Description = "Gestione articoli di magazzino",
                        MenuIcon = "bi-table", 
                        MenuOrder = 1, 
                        ParentResourceId = 2, // Parent: Tabelle
                        IsMenuGroup = false,
                        IsConfigured = true
                    },
                    new Resource 
                    { 
                        Name = "AnagraficaClienti", 
                        DisplayName = "Anagrafica Clienti", 
                        Description = "Gestione clienti",
                        MenuIcon = "bi-people", 
                        MenuOrder = 2, 
                        ParentResourceId = 2,
                        IsMenuGroup = false,
                        IsConfigured = true
                    },
                    new Resource 
                    { 
                        Name = "AnagraficaFornitori", 
                        DisplayName = "Anagrafica Fornitori", 
                        Description = "Gestione fornitori",
                        MenuIcon = "bi-truck", 
                        MenuOrder = 3, 
                        ParentResourceId = 2,
                        IsMenuGroup = false,
                        IsConfigured = true
                    },
                    new Resource 
                    { 
                        Name = "ArticoliSostitutivi", 
                        DisplayName = "Articoli Sostitutivi", 
                        Description = "Gestione sostituzioni articoli",
                        MenuIcon = "bi-arrow-left-right", 
                        MenuOrder = 4, 
                        ParentResourceId = 2,
                        IsMenuGroup = false,
                        IsConfigured = true
                    },
                    new Resource 
                    { 
                        Name = "ProgressiviArticoli", 
                        DisplayName = "Progressivi Articoli", 
                        Description = "Gestione giacenze e progressivi",
                        MenuIcon = "bi-boxes", 
                        MenuOrder = 5, 
                        ParentResourceId = 2,
                        IsMenuGroup = false,
                        IsConfigured = true
                    },
                    new Resource 
                    { 
                        Name = "TabellaAgenti", 
                        DisplayName = "Agenti", 
                        Description = "Gestione agenti di vendita",
                        MenuIcon = "bi-person-workspace", 
                        MenuOrder = 6, 
                        ParentResourceId = 2,
                        IsMenuGroup = false,
                        IsConfigured = true
                    },
                    new Resource 
                    { 
                        Name = "TabellaMagazzini", 
                        DisplayName = "Magazzini", 
                        Description = "Gestione magazzini",
                        MenuIcon = "bi-building", 
                        MenuOrder = 7, 
                        ParentResourceId = 2,
                        IsMenuGroup = false,
                        IsConfigured = true
                    },
                    new Resource 
                    { 
                        Name = "Lavorazioni", 
                        DisplayName = "Lavorazioni", 
                        Description = "Gestione lavorazioni di produzione",
                        MenuIcon = "bi-gear-wide", 
                        MenuOrder = 8, 
                        ParentResourceId = 2,
                        IsMenuGroup = false,
                        IsConfigured = true
                    },
                    new Resource 
                    { 
                        Name = "CentriLavoro", 
                        DisplayName = "Centri Lavoro", 
                        Description = "Gestione centri di lavoro",
                        MenuIcon = "bi-building-gear", 
                        MenuOrder = 9, 
                        ParentResourceId = 2,
                        IsMenuGroup = false,
                        IsConfigured = true
                    },

                    // 3. GRUPPO ORDINI
                    new Resource 
                    { 
                        Name = "Ordini", 
                        DisplayName = "Ordini", 
                        Description = "Gruppo gestione ordini",
                        MenuIcon = "bi-card-list", 
                        MenuOrder = 3, 
                        ParentResourceId = null, 
                        IsMenuGroup = true,
                        IsConfigured = true
                    },
                    new Resource 
                    { 
                        Name = "OrdiniTestate", 
                        DisplayName = "Ordini Clienti", 
                        Description = "Gestione ordini clienti",
                        MenuIcon = "bi-card-list", 
                        MenuOrder = 1, 
                        ParentResourceId = 12,
                        IsMenuGroup = false,
                        IsConfigured = true
                    },
                    new Resource 
                    { 
                        Name = "OrdiniRighe", 
                        DisplayName = "Righe Ordine", 
                        Description = "Dettaglio righe ordini",
                        MenuIcon = "bi-list-ul", 
                        MenuOrder = 2, 
                        ParentResourceId = 12,
                        IsMenuGroup = false,
                        IsConfigured = true
                    },

                    // 4. GRUPPO PRODUZIONE
                    new Resource 
                    { 
                        Name = "Produzione", 
                        DisplayName = "Produzione", 
                        Description = "Gruppo pianificazione produzione",
                        MenuIcon = "bi-gear-wide-connected", 
                        MenuOrder = 4, 
                        ParentResourceId = null, 
                        IsMenuGroup = true,
                        IsConfigured = true
                    },
                    new Resource 
                    { 
                        Name = "SchedulatoreOP", 
                        DisplayName = "Schedulatore", 
                        Description = "Schedulazione ordini di produzione",
                        MenuIcon = "bi-calendar3", 
                        MenuOrder = 1, 
                        ParentResourceId = 15,
                        IsMenuGroup = false,
                        IsConfigured = true
                    },
                    new Resource 
                    { 
                        Name = "ListaOP", 
                        DisplayName = "Lista Ordini Produzione", 
                        Description = "Gestione ordini di produzione",
                        MenuIcon = "bi-list-check", 
                        MenuOrder = 2, 
                        ParentResourceId = 15,
                        IsMenuGroup = false,
                        IsConfigured = true
                    },
                    new Resource 
                    { 
                        Name = "Operatori", 
                        DisplayName = "Operatori", 
                        Description = "Gestione operatori produzione",
                        MenuIcon = "bi-person-badge", 
                        MenuOrder = 3, 
                        ParentResourceId = 15,
                        IsMenuGroup = false,
                        IsConfigured = true
                    },
                    new Resource 
                    { 
                        Name = "CalendarioFermi", 
                        DisplayName = "Calendario Fermi", 
                        Description = "Gestione fermi centri lavoro",
                        MenuIcon = "bi-calendar-x", 
                        MenuOrder = 4, 
                        ParentResourceId = 15,
                        IsMenuGroup = false,
                        IsConfigured = true
                    },

                    // 5. GRUPPO AMMINISTRAZIONE
                    new Resource 
                    { 
                        Name = "Amministrazione", 
                        DisplayName = "Amministrazione", 
                        Description = "Gruppo amministrazione sistema",
                        MenuIcon = "bi-gear", 
                        MenuOrder = 5, 
                        ParentResourceId = null, 
                        IsMenuGroup = true,
                        IsConfigured = true
                    },
                    new Resource 
                    { 
                        Name = "UserManagement", 
                        DisplayName = "Gestione Utenti", 
                        Description = "Amministrazione utenti",
                        MenuIcon = "bi-people", 
                        MenuOrder = 1, 
                        ParentResourceId = 20,
                        IsMenuGroup = false,
                        IsConfigured = true
                    },
                    new Resource 
                    { 
                        Name = "RoleManagement", 
                        DisplayName = "Gestione Ruoli", 
                        Description = "Amministrazione ruoli",
                        MenuIcon = "bi-shield-lock", 
                        MenuOrder = 2, 
                        ParentResourceId = 20,
                        IsMenuGroup = false,
                        IsConfigured = true
                    },
                    new Resource 
                    { 
                        Name = "PermissionManagement", 
                        DisplayName = "Gestione Permessi", 
                        Description = "Configurazione permessi sistema",
                        MenuIcon = "bi-key", 
                        MenuOrder = 3, 
                        ParentResourceId = 20,
                        IsMenuGroup = false,
                        IsConfigured = true
                    },
                    new Resource 
                    { 
                        Name = "AgentiToUser", 
                        DisplayName = "Converti Agenti in Utenti", 
                        Description = "Conversione agenti in utenti sistema",
                        MenuIcon = "bi-person-plus-fill", 
                        MenuOrder = 4, 
                        ParentResourceId = 20,
                        IsMenuGroup = false,
                        IsConfigured = true
                    },
                    new Resource 
                    { 
                        Name = "AISettings", 
                        DisplayName = "Impostazioni AI", 
                        Description = "Configurazione sistema AI",
                        MenuIcon = "bi-robot", 
                        MenuOrder = 5, 
                        ParentResourceId = 20,
                        IsMenuGroup = false,
                        IsConfigured = true
                    }
                };

                // Salva risorse
                await context.Resources.AddRangeAsync(resources);
                await context.SaveChangesAsync();

                logger.LogInformation($"Seed completato: {resources.Count} risorse create.");

                // ===== PERMESSI DEFAULT PER ADMIN (può tutto) =====
                
                var adminRole = await roleManager.FindByNameAsync(UserRoles.Admin);
                if (adminRole != null)
                {
                    var adminPermissions = new List<Permission>();
                    
                    // Admin ha permessi completi su tutte le risorse NON gruppo
                    foreach (var resource in resources.Where(r => !r.IsMenuGroup))
                    {
                        adminPermissions.Add(new Permission
                        {
                            RoleId = adminRole.Id,
                            ResourceId = resource.Id,
                            CanView = true,
                            CanCreate = true,
                            CanEdit = true,
                            CanDelete = true
                        });
                    }

                    await context.Permissions.AddRangeAsync(adminPermissions);
                    await context.SaveChangesAsync();
                    
                    logger.LogInformation($"Permessi Admin: {adminPermissions.Count} permessi creati.");
                }

                // ===== PERMESSI DEFAULT PER AGENTI (solo clienti e ordini, view+edit) =====
                
                var agentiRole = await roleManager.FindByNameAsync(UserRoles.Agenti);
                if (agentiRole != null)
                {
                    var agentiPermissions = new List<Permission>();
                    
                    // Agenti: Dashboard (view)
                    var dashboardRes = resources.FirstOrDefault(r => r.Name == "Dashboard");
                    if (dashboardRes != null)
                    {
                        agentiPermissions.Add(new Permission
                        {
                            RoleId = agentiRole.Id,
                            ResourceId = dashboardRes.Id,
                            CanView = true,
                            CanCreate = false,
                            CanEdit = false,
                            CanDelete = false
                        });
                    }

                    // Agenti: Clienti (view + edit)
                    var clientiRes = resources.FirstOrDefault(r => r.Name == "AnagraficaClienti");
                    if (clientiRes != null)
                    {
                        agentiPermissions.Add(new Permission
                        {
                            RoleId = agentiRole.Id,
                            ResourceId = clientiRes.Id,
                            CanView = true,
                            CanCreate = false,
                            CanEdit = true,
                            CanDelete = false
                        });
                    }

                    // Agenti: Ordini (view + edit)
                    var ordiniRes = resources.FirstOrDefault(r => r.Name == "OrdiniTestate");
                    if (ordiniRes != null)
                    {
                        agentiPermissions.Add(new Permission
                        {
                            RoleId = agentiRole.Id,
                            ResourceId = ordiniRes.Id,
                            CanView = true,
                            CanCreate = false,
                            CanEdit = true,
                            CanDelete = false
                        });
                    }

                    await context.Permissions.AddRangeAsync(agentiPermissions);
                    await context.SaveChangesAsync();
                    
                    logger.LogInformation($"Permessi Agenti: {agentiPermissions.Count} permessi creati.");
                }

                logger.LogInformation("Seed permessi completato con successo!");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Errore durante il seed dei permessi");
                throw;
            }
        }
    }
}

