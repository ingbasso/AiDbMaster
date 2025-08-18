using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using AiDbMaster.Data;
using AiDbMaster.Models;
using AiDbMaster.Services;
using AiDbMaster.ViewModels;

namespace AiDbMaster.Controllers
{
    [Authorize]
    public class CategoryController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly CategoryService _categoryService;
        private readonly ILogger<CategoryController> _logger;

        public CategoryController(
            ApplicationDbContext context,
            CategoryService categoryService,
            ILogger<CategoryController> logger)
        {
            _context = context;
            _categoryService = categoryService;
            _logger = logger;
        }

        // GET: Category
        public async Task<IActionResult> Index()
        {
            var categories = await _categoryService.GetAllCategoriesAsync();
            return View(categories);
        }

        // GET: Category/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var category = await _categoryService.GetCategoryByIdAsync(id);
            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }

        // GET: Category/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Category/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,Description")] DocumentCategory category)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await _categoryService.CreateCategoryAsync(category);
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Errore durante la creazione della categoria '{category.Name}'");
                    ModelState.AddModelError(string.Empty, $"Errore durante la creazione della categoria: {ex.Message}");
                }
            }
            return View(category);
        }

        // GET: Category/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var category = await _categoryService.GetCategoryByIdAsync(id);
            if (category == null)
            {
                return NotFound();
            }
            return View(category);
        }

        // POST: Category/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description")] DocumentCategory category)
        {
            if (id != category.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    await _categoryService.UpdateCategoryAsync(category);
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Errore durante l'aggiornamento della categoria '{category.Name}'");
                    ModelState.AddModelError(string.Empty, $"Errore durante l'aggiornamento della categoria: {ex.Message}");
                }
            }
            return View(category);
        }

        // GET: Category/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var category = await _categoryService.GetCategoryByIdAsync(id);
            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }

        // POST: Category/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _categoryService.DeleteCategoryAsync(id);
            return RedirectToAction(nameof(Index));
        }

        // GET: Category/Documents/5
        public async Task<IActionResult> Documents(int id)
        {
            var category = await _categoryService.GetCategoryByIdAsync(id);
            if (category == null)
            {
                return NotFound();
            }

            var documents = await _context.Documents
                .Where(d => d.CategoryId == id)
                .ToListAsync();

            ViewBag.Category = category;
            return View(documents);
        }

        // Metodo per creare una categoria direttamente
        [HttpPost]
        [Route("api/categories/create")]
        public async Task<IActionResult> CreateCategoryApi([FromBody] DocumentCategory category)
        {
            if (category == null || string.IsNullOrEmpty(category.Name))
            {
                return BadRequest("Nome categoria non valido");
            }

            try
            {
                // Verifica se la categoria esiste già
                var existingCategory = await _context.DocumentCategories
                    .FirstOrDefaultAsync(c => c.Name.ToLower() == category.Name.ToLower());

                if (existingCategory != null)
                {
                    return Ok(existingCategory);
                }

                // Crea la nuova categoria
                _context.DocumentCategories.Add(category);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Categoria '{category.Name}' creata con successo tramite API, ID: {category.Id}");
                return Ok(category);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Errore durante la creazione della categoria '{category.Name}' tramite API");
                return StatusCode(500, $"Errore durante la creazione della categoria: {ex.Message}");
            }
        }
    }
} 