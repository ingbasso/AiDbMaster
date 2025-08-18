using AiDbMaster.Models;
using Microsoft.AspNetCore.Identity;

namespace AiDbMaster.Data
{
    public static class DbSeeder
    {
        public static async Task SeedRolesAndAdminAsync(IServiceProvider serviceProvider)
        {
            // Ottieni i servizi necessari
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            // Crea i ruoli se non esistono
            string[] roleNames = { UserRoles.Admin, UserRoles.Manager, UserRoles.Employee, UserRoles.User };
            foreach (var roleName in roleNames)
            {
                var roleExists = await roleManager.RoleExistsAsync(roleName);
                if (!roleExists)
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }

            // Crea un utente amministratore se non esiste
            var adminUser = await userManager.FindByEmailAsync("admin@example.com");
            if (adminUser == null)
            {
                var admin = new ApplicationUser
                {
                    UserName = "admin@example.com",
                    Email = "admin@example.com",
                    FirstName = "Admin",
                    LastName = "User",
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(admin, "Admin123!");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(admin, UserRoles.Admin);
                }
            }

            // Crea un utente manager di esempio se non esiste
            var managerUser = await userManager.FindByEmailAsync("manager@example.com");
            if (managerUser == null)
            {
                var manager = new ApplicationUser
                {
                    UserName = "manager@example.com",
                    Email = "manager@example.com",
                    FirstName = "Manager",
                    LastName = "User",
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(manager, "Manager123!");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(manager, UserRoles.Manager);
                }
            }
        }
    }
} 