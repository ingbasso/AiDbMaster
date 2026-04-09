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
                    // 1. Home / Dashboard
                    new Resource 
                    { 
                        Name = "Home", 
                        DisplayName = "Home", 
                        Description = "Pagina principale del sistema",
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

                    // 3. GRUPPO PRODUZIONE
                    new Resource 
                    { 
                        Name = "Produzione", 
                        DisplayName = "Produzione", 
                        Description = "Gruppo pianificazione produzione",
                        MenuIcon = "bi-gear-wide-connected", 
                        MenuOrder = 3, 
                        ParentResourceId = null, 
                        IsMenuGroup = true,
                        IsConfigured = true
                    },

                    // 4. GRUPPO CONSEGNE
                    new Resource
                    {
                        Name = "Consegne",
                        DisplayName = "Consegne",
                        Description = "Gruppo pianificazione consegne",
                        MenuIcon = "bi-truck",
                        MenuOrder = 4,
                        ParentResourceId = null,
                        IsMenuGroup = true,
                        IsConfigured = true
                    },

                    // 5. GRUPPO INTERROGAZIONI DB
                    new Resource 
                    { 
                        Name = "InterrogazioniDB", 
                        DisplayName = "Interrogazioni DB", 
                        Description = "Interrogazioni e analisi database",
                        MenuIcon = "bi-search", 
                        MenuOrder = 5, 
                        ParentResourceId = null, 
                        IsMenuGroup = true,
                        IsConfigured = true
                    },

                    // 6. GRUPPO AMMINISTRAZIONE
                    new Resource 
                    { 
                        Name = "Amministrazione", 
                        DisplayName = "Amministrazione", 
                        Description = "Gruppo amministrazione sistema",
                        MenuIcon = "bi-gear", 
                        MenuOrder = 6, 
                        ParentResourceId = null, 
                        IsMenuGroup = true,
                        IsConfigured = true
                    }
                };

                // ===== FASE 1: Salva prima i gruppi ROOT =====
                await context.Resources.AddRangeAsync(resources);
                await context.SaveChangesAsync();
                logger.LogInformation($"✅ Fase 1: {resources.Count} risorse root salvate");

                // Recupera gli ID reali generati dal database
                var homeRes = await context.Resources.FirstAsync(r => r.Name == "Home");
                var tabelleRes = await context.Resources.FirstAsync(r => r.Name == "Tabelle");
                var produzioneRes = await context.Resources.FirstAsync(r => r.Name == "Produzione");
                var consegneRes = await context.Resources.FirstAsync(r => r.Name == "Consegne");
                var interrogazioniDBRes = await context.Resources.FirstAsync(r => r.Name == "InterrogazioniDB");
                var amministrazioneRes = await context.Resources.FirstAsync(r => r.Name == "Amministrazione");

                logger.LogInformation($"   → Home ID: {homeRes.Id}");
                logger.LogInformation($"   → Tabelle ID: {tabelleRes.Id}");
                logger.LogInformation($"   → Produzione ID: {produzioneRes.Id}");
                logger.LogInformation($"   → Consegne ID: {consegneRes.Id}");
                logger.LogInformation($"   → InterrogazioniDB ID: {interrogazioniDBRes.Id}");
                logger.LogInformation($"   → Amministrazione ID: {amministrazioneRes.Id}");

                // ===== FASE 2: Crea risorse FIGLIE usando gli ID reali =====
                var childResources = new List<Resource>
                {
                    // ==== TABELLE (13 pagine) ====
                    new Resource { Name = "AnagraficaArticoli", DisplayName = "Anagrafica Articoli", Description = "Gestione articoli di magazzino", MenuIcon = "bi-table", MenuOrder = 1, ParentResourceId = tabelleRes.Id, IsMenuGroup = false, IsConfigured = true },
                    new Resource { Name = "AnagraficaClienti", DisplayName = "Anagrafica Clienti", Description = "Gestione clienti", MenuIcon = "bi-people", MenuOrder = 2, ParentResourceId = tabelleRes.Id, IsMenuGroup = false, IsConfigured = true },
                    new Resource { Name = "AnagraficaFornitori", DisplayName = "Anagrafica Fornitori", Description = "Gestione fornitori", MenuIcon = "bi-truck", MenuOrder = 3, ParentResourceId = tabelleRes.Id, IsMenuGroup = false, IsConfigured = true },
                    new Resource { Name = "ArticoliSostitutivi", DisplayName = "Articoli Sostitutivi", Description = "Gestione sostituzioni articoli", MenuIcon = "bi-arrow-left-right", MenuOrder = 4, ParentResourceId = tabelleRes.Id, IsMenuGroup = false, IsConfigured = true },
                    new Resource { Name = "ProgressiviArticoli", DisplayName = "Progressivi Articoli", Description = "Gestione giacenze e progressivi", MenuIcon = "bi-boxes", MenuOrder = 5, ParentResourceId = tabelleRes.Id, IsMenuGroup = false, IsConfigured = true },
                    new Resource { Name = "TabellaAgenti", DisplayName = "Agenti", Description = "Gestione agenti di vendita", MenuIcon = "bi-person-workspace", MenuOrder = 6, ParentResourceId = tabelleRes.Id, IsMenuGroup = false, IsConfigured = true },
                    new Resource { Name = "TabellaMagazzini", DisplayName = "Magazzini", Description = "Gestione magazzini", MenuIcon = "bi-building", MenuOrder = 7, ParentResourceId = tabelleRes.Id, IsMenuGroup = false, IsConfigured = true },
                    new Resource { Name = "Lavorazioni", DisplayName = "Lavorazioni", Description = "Gestione lavorazioni di produzione", MenuIcon = "bi-gear-wide", MenuOrder = 8, ParentResourceId = tabelleRes.Id, IsMenuGroup = false, IsConfigured = true },
                    new Resource { Name = "CentriLavoro", DisplayName = "Centri di Lavoro", Description = "Gestione centri di lavoro", MenuIcon = "bi-building-gear", MenuOrder = 9, ParentResourceId = tabelleRes.Id, IsMenuGroup = false, IsConfigured = true },
                    new Resource { Name = "Operatori", DisplayName = "Operatori", Description = "Gestione operatori produzione", MenuIcon = "bi-people", MenuOrder = 10, ParentResourceId = tabelleRes.Id, IsMenuGroup = false, IsConfigured = true },
                    new Resource { Name = "StatiOP", DisplayName = "Stati OP", Description = "Gestione stati ordini di produzione", MenuIcon = "bi-flag", MenuOrder = 11, ParentResourceId = tabelleRes.Id, IsMenuGroup = false, IsConfigured = true },
                    new Resource { Name = "OrdiniTestate", DisplayName = "Gestione Ordini CF", Description = "Gestione ordini clienti", MenuIcon = "bi-clipboard-check", MenuOrder = 12, ParentResourceId = tabelleRes.Id, IsMenuGroup = false, IsConfigured = true },
                    new Resource { Name = "TempiAsciugatura", DisplayName = "Tempi di Asciugatura", Description = "Gestione tempi di asciugatura", MenuIcon = "bi-calendar-day", MenuOrder = 13, ParentResourceId = tabelleRes.Id, IsMenuGroup = false, IsConfigured = true },
                    new Resource { Name = "DbTestate", DisplayName = "Testate Distinta Base", Description = "Gestione testate distinta base", MenuIcon = "bi-file-earmark-text", MenuOrder = 14, ParentResourceId = tabelleRes.Id, IsMenuGroup = false, IsConfigured = true },
                    new Resource { Name = "DbLavorazioni", DisplayName = "Cicli di Lavorazione", Description = "Gestione cicli/distinte di lavorazione", MenuIcon = "bi-diagram-3", MenuOrder = 15, ParentResourceId = tabelleRes.Id, IsMenuGroup = false, IsConfigured = true },
                    new Resource { Name = "DbMateriali", DisplayName = "Distinta Base Materiali", Description = "Gestione distinta base materiali", MenuIcon = "bi-box-seam", MenuOrder = 16, ParentResourceId = tabelleRes.Id, IsMenuGroup = false, IsConfigured = true },
                    new Resource { Name = "PoliticheRiordinoMagazzino", DisplayName = "Politiche Riordino", Description = "Gestione politiche riordino magazzino", MenuIcon = "bi-arrow-repeat", MenuOrder = 17, ParentResourceId = tabelleRes.Id, IsMenuGroup = false, IsConfigured = true },

                    // ==== PRODUZIONE (4 pagine) ====
                    new Resource { Name = "ListaOPDashboard", DisplayName = "Dashboard", Description = "Dashboard produzione", MenuIcon = "bi-graph-up", MenuOrder = 1, ParentResourceId = produzioneRes.Id, IsMenuGroup = false, IsConfigured = true },
                    new Resource { Name = "SchedulatoreOP", DisplayName = "Schedulatore OP", Description = "Schedulazione ordini di produzione", MenuIcon = "bi-calendar2-check", MenuOrder = 2, ParentResourceId = produzioneRes.Id, IsMenuGroup = false, IsConfigured = true },
                    new Resource { Name = "ListaOP", DisplayName = "Ordini di Produzione", Description = "Gestione ordini di produzione", MenuIcon = "bi-list-ul", MenuOrder = 3, ParentResourceId = produzioneRes.Id, IsMenuGroup = false, IsConfigured = true },
                    new Resource { Name = "FermiSchedulati", DisplayName = "Fermi Schedulati", Description = "Gestione fermi centri lavoro", MenuIcon = "bi-calendar-check", MenuOrder = 4, ParentResourceId = produzioneRes.Id, IsMenuGroup = false, IsConfigured = true },
                    new Resource { Name = "ProductionCalc", DisplayName = "Production Calc", Description = "Calcolo produzione articoli", MenuIcon = "bi-calculator", MenuOrder = 5, ParentResourceId = produzioneRes.Id, IsMenuGroup = false, IsConfigured = true },

                    // ==== CONSEGNE (5 pagine) ====
                    new Resource { Name = "ConsegneKanban", DisplayName = "Kanban Consegne", Description = "Kanban pianificazione consegne", MenuIcon = "bi-kanban", MenuOrder = 1, ParentResourceId = consegneRes.Id, IsMenuGroup = false, IsConfigured = true },
                    new Resource { Name = "ViaggiConsegna", DisplayName = "Viaggi Consegna", Description = "Elenco viaggi consegna", MenuIcon = "bi-calendar-event", MenuOrder = 2, ParentResourceId = consegneRes.Id, IsMenuGroup = false, IsConfigured = true },
                    new Resource { Name = "Mezzi", DisplayName = "Mezzi", Description = "Anagrafica mezzi di trasporto", MenuIcon = "bi-truck-front", MenuOrder = 3, ParentResourceId = consegneRes.Id, IsMenuGroup = false, IsConfigured = true },
                    new Resource { Name = "TipiTrasporto", DisplayName = "Tipi Trasporto", Description = "Anagrafica tipi trasporto", MenuIcon = "bi-signpost-split", MenuOrder = 4, ParentResourceId = consegneRes.Id, IsMenuGroup = false, IsConfigured = true },
                    new Resource { Name = "MezziTrasportoEsterni", DisplayName = "Mezzi Esterni", Description = "Anagrafica mezzi di trasporto esterni", MenuIcon = "bi-truck-flatbed", MenuOrder = 5, ParentResourceId = consegneRes.Id, IsMenuGroup = false, IsConfigured = true },

                    // ==== INTERROGAZIONI DB (5 pagine) ====
                    new Resource { Name = "Disponibilita", DisplayName = "Disponibilità", Description = "Verifica disponibilità articoli", MenuIcon = "bi-boxes", MenuOrder = 1, ParentResourceId = interrogazioniDBRes.Id, IsMenuGroup = false, IsConfigured = true },
                    new Resource { Name = "ConsegneProgrammate", DisplayName = "Consegne Programmate", Description = "Gestione consegne programmate", MenuIcon = "bi-calendar-event", MenuOrder = 2, ParentResourceId = interrogazioniDBRes.Id, IsMenuGroup = false, IsConfigured = true },
                    new Resource { Name = "DashboardConsegne", DisplayName = "Dashboard Consegne", Description = "Dashboard analisi consegne", MenuIcon = "bi-graph-up", MenuOrder = 3, ParentResourceId = interrogazioniDBRes.Id, IsMenuGroup = false, IsConfigured = true },
                    new Resource { Name = "Grafici", DisplayName = "Grafici", Description = "Grafici e statistiche avanzate", MenuIcon = "bi-graph-up", MenuOrder = 4, ParentResourceId = interrogazioniDBRes.Id, IsMenuGroup = false, IsConfigured = true },
                    new Resource { Name = "InterrogazioniAI", DisplayName = "Interrogazioni AI", Description = "Interrogazioni con intelligenza artificiale", MenuIcon = "bi-robot", MenuOrder = 5, ParentResourceId = interrogazioniDBRes.Id, IsMenuGroup = false, IsConfigured = true },

                    // ==== AMMINISTRAZIONE (6 pagine) ====
                    new Resource { Name = "UserManagement", DisplayName = "Gestione Utenti", Description = "Amministrazione utenti", MenuIcon = "bi-people", MenuOrder = 1, ParentResourceId = amministrazioneRes.Id, IsMenuGroup = false, IsConfigured = true },
                    new Resource { Name = "RoleManagement", DisplayName = "Gestione Ruoli", Description = "Amministrazione ruoli", MenuIcon = "bi-shield-lock", MenuOrder = 2, ParentResourceId = amministrazioneRes.Id, IsMenuGroup = false, IsConfigured = true },
                    new Resource { Name = "PermissionManagement", DisplayName = "Gestione Permessi", Description = "Configurazione permessi sistema", MenuIcon = "bi-shield-lock-fill", MenuOrder = 3, ParentResourceId = amministrazioneRes.Id, IsMenuGroup = false, IsConfigured = true },
                    new Resource { Name = "AgentiToUser", DisplayName = "Converti Agenti in Utenti", Description = "Conversione agenti in utenti sistema", MenuIcon = "bi-person-plus-fill", MenuOrder = 4, ParentResourceId = amministrazioneRes.Id, IsMenuGroup = false, IsConfigured = true },
                    new Resource { Name = "AISettings", DisplayName = "Impostazioni AI", Description = "Configurazione sistema AI", MenuIcon = "bi-robot", MenuOrder = 5, ParentResourceId = amministrazioneRes.Id, IsMenuGroup = false, IsConfigured = true },
                    new Resource { Name = "SyncfusionTest", DisplayName = "Test Syncfusion", Description = "Test componenti Syncfusion", MenuIcon = "bi-grid-3x3-gap", MenuOrder = 6, ParentResourceId = amministrazioneRes.Id, IsMenuGroup = false, IsConfigured = true }
                };

                await context.Resources.AddRangeAsync(childResources);
                await context.SaveChangesAsync();
                logger.LogInformation($"✅ Fase 2: {childResources.Count} risorse figlie salvate");

                var totalResources = resources.Count + childResources.Count;
                logger.LogInformation($"✅ Seed completato: {totalResources} risorse create in totale.");

                // ===== PERMESSI DEFAULT PER ADMIN (può tutto) =====
                
                var adminRole = await roleManager.FindByNameAsync(UserRoles.Admin);
                if (adminRole != null)
                {
                    var allResources = await context.Resources.Where(r => !r.IsMenuGroup).ToListAsync();
                    var adminPermissions = new List<Permission>();
                    
                    // Admin ha permessi completi su tutte le risorse NON gruppo
                    foreach (var resource in allResources)
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
                    
                    logger.LogInformation($"✅ Permessi Admin: {adminPermissions.Count} permessi creati.");
                }

                // ===== PERMESSI DEFAULT PER AGENTI (solo clienti, ordini, consegne - view+edit) =====
                
                var agentiRole = await roleManager.FindByNameAsync(UserRoles.Agenti);
                if (agentiRole != null)
                {
                    var agentiPermissions = new List<Permission>();
                    
                    // Agenti: Home (view)
                    var homeResource = await context.Resources.FirstOrDefaultAsync(r => r.Name == "Home");
                    if (homeResource != null)
                    {
                        agentiPermissions.Add(new Permission
                        {
                            RoleId = agentiRole.Id,
                            ResourceId = homeResource.Id,
                            CanView = true,
                            CanCreate = false,
                            CanEdit = false,
                            CanDelete = false
                        });
                    }

                    // Agenti: Clienti (view + edit)
                    var clientiResource = await context.Resources.FirstOrDefaultAsync(r => r.Name == "AnagraficaClienti");
                    if (clientiResource != null)
                    {
                        agentiPermissions.Add(new Permission
                        {
                            RoleId = agentiRole.Id,
                            ResourceId = clientiResource.Id,
                            CanView = true,
                            CanCreate = false,
                            CanEdit = true,
                            CanDelete = false
                        });
                    }

                    // Agenti: Ordini (view + edit)
                    var ordiniResource = await context.Resources.FirstOrDefaultAsync(r => r.Name == "OrdiniTestate");
                    if (ordiniResource != null)
                    {
                        agentiPermissions.Add(new Permission
                        {
                            RoleId = agentiRole.Id,
                            ResourceId = ordiniResource.Id,
                            CanView = true,
                            CanCreate = false,
                            CanEdit = true,
                            CanDelete = false
                        });
                    }

                    // Agenti: Consegne Programmate (view)
                    var consegneResource = await context.Resources.FirstOrDefaultAsync(r => r.Name == "ConsegneProgrammate");
                    if (consegneResource != null)
                    {
                        agentiPermissions.Add(new Permission
                        {
                            RoleId = agentiRole.Id,
                            ResourceId = consegneResource.Id,
                            CanView = true,
                            CanCreate = false,
                            CanEdit = false,
                            CanDelete = false
                        });
                    }

                    // Agenti: Dashboard Consegne (view)
                    var dashboardConsegneResource = await context.Resources.FirstOrDefaultAsync(r => r.Name == "DashboardConsegne");
                    if (dashboardConsegneResource != null)
                    {
                        agentiPermissions.Add(new Permission
                        {
                            RoleId = agentiRole.Id,
                            ResourceId = dashboardConsegneResource.Id,
                            CanView = true,
                            CanCreate = false,
                            CanEdit = false,
                            CanDelete = false
                        });
                    }

                    // Agenti: Kanban Consegne (view)
                    var kanbanConsegneResource = await context.Resources.FirstOrDefaultAsync(r => r.Name == "ConsegneKanban");
                    if (kanbanConsegneResource != null)
                    {
                        agentiPermissions.Add(new Permission
                        {
                            RoleId = agentiRole.Id,
                            ResourceId = kanbanConsegneResource.Id,
                            CanView = true,
                            CanCreate = false,
                            CanEdit = false,
                            CanDelete = false
                        });
                    }

                    await context.Permissions.AddRangeAsync(agentiPermissions);
                    await context.SaveChangesAsync();
                    
                    logger.LogInformation($"✅ Permessi Agenti: {agentiPermissions.Count} permessi creati.");
                }

                logger.LogInformation("✅ Seed permessi completato con successo!");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "❌ Errore durante il seed dei permessi");
                throw;
            }
        }
    }
}
