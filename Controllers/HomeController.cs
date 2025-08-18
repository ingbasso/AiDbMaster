using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using AiDbMaster.Models;
using AiDbMaster.Services;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;

namespace AiDbMaster.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly DocumentService _documentService;
    private readonly CategoryService _categoryService;

    public HomeController(
        ILogger<HomeController> logger,
        DocumentService documentService,
        CategoryService categoryService)
    {
        _logger = logger;
        _documentService = documentService;
        _categoryService = categoryService;
    }

    public async Task<IActionResult> Index()
    {
        // Se l'utente è autenticato, mostra la dashboard
        if (User.Identity?.IsAuthenticated == true)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isAdmin = User.IsInRole(UserRoles.Admin);
            var isManager = User.IsInRole(UserRoles.Manager);

            // Ottieni i documenti in base al ruolo
            var documents = isAdmin || isManager
                ? await _documentService.GetAllDocumentsAsync()
                : userId != null 
                    ? await _documentService.GetDocumentsByUserIdAsync(userId)
                    : new List<Document>();

            // Prendi solo i 10 documenti più recenti
            var recentDocuments = documents
                .OrderByDescending(d => d.UploadDate)
                .Take(10)
                .ToList();

            // Ottieni tutte le categorie
            var categories = await _categoryService.GetAllCategoriesAsync();

            // Passa i dati alla vista
            ViewBag.RecentDocuments = recentDocuments;
            ViewBag.Categories = categories;
            ViewBag.TotalDocuments = documents.Count;
            ViewBag.TotalCategories = categories.Count;
        }

        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
