using AiDbMaster.Models;
using AiDbMaster.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AiDbMaster.Controllers
{
    [Authorize(Roles = UserRoles.Admin)]
    public class RoleManagementController : Controller
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<RoleManagementController> _logger;

        public RoleManagementController(
            RoleManager<IdentityRole> roleManager,
            UserManager<ApplicationUser> userManager,
            ILogger<RoleManagementController> logger)
        {
            _roleManager = roleManager;
            _userManager = userManager;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var roles = await _roleManager.Roles.ToListAsync();
            var roleViewModels = new List<RoleViewModel>();

            foreach (var role in roles)
            {
                var usersInRole = await _userManager.GetUsersInRoleAsync(role.Name);
                roleViewModels.Add(new RoleViewModel
                {
                    Id = role.Id,
                    Name = role.Name,
                    UserCount = usersInRole.Count
                });
            }

            return View(roleViewModels);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View(new CreateRoleViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateRoleViewModel model)
        {
            if (ModelState.IsValid)
            {
                var role = new IdentityRole(model.Name);
                var result = await _roleManager.CreateAsync(role);

                if (result.Succeeded)
                {
                    _logger.LogInformation($"Ruolo {model.Name} creato con successo");
                    return RedirectToAction(nameof(Index));
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            var role = await _roleManager.FindByIdAsync(id);
            if (role == null)
            {
                return NotFound();
            }

            var usersInRole = await _userManager.GetUsersInRoleAsync(role.Name);
            var allUsers = await _userManager.Users.ToListAsync();

            var model = new EditRoleViewModel
            {
                Id = role.Id,
                Name = role.Name,
                Users = allUsers.Select(u => new UserRoleViewModel
                {
                    UserId = u.Id,
                    UserName = $"{u.FirstName} {u.LastName}",
                    IsInRole = usersInRole.Any(ur => ur.Id == u.Id)
                }).ToList()
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditRoleViewModel model)
        {
            if (ModelState.IsValid)
            {
                var role = await _roleManager.FindByIdAsync(model.Id);
                if (role == null)
                {
                    return NotFound();
                }

                role.Name = model.Name;
                var result = await _roleManager.UpdateAsync(role);

                if (result.Succeeded)
                {
                    // Aggiorna i ruoli degli utenti
                    foreach (var user in model.Users)
                    {
                        var appUser = await _userManager.FindByIdAsync(user.UserId);
                        if (appUser != null)
                        {
                            if (user.IsInRole)
                            {
                                if (!await _userManager.IsInRoleAsync(appUser, role.Name))
                                {
                                    await _userManager.AddToRoleAsync(appUser, role.Name);
                                }
                            }
                            else
                            {
                                if (await _userManager.IsInRoleAsync(appUser, role.Name))
                                {
                                    await _userManager.RemoveFromRoleAsync(appUser, role.Name);
                                }
                            }
                        }
                    }

                    _logger.LogInformation($"Ruolo {model.Name} aggiornato con successo");
                    return RedirectToAction(nameof(Index));
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string id)
        {
            var role = await _roleManager.FindByIdAsync(id);
            if (role == null)
            {
                return NotFound();
            }

            // Verifica se ci sono utenti nel ruolo
            var usersInRole = await _userManager.GetUsersInRoleAsync(role.Name);
            if (usersInRole.Any())
            {
                TempData["ErrorMessage"] = "Impossibile eliminare il ruolo perché ci sono utenti assegnati.";
                return RedirectToAction(nameof(Index));
            }

            var result = await _roleManager.DeleteAsync(role);
            if (result.Succeeded)
            {
                _logger.LogInformation($"Ruolo {role.Name} eliminato con successo");
                return RedirectToAction(nameof(Index));
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return RedirectToAction(nameof(Index));
        }
    }
} 