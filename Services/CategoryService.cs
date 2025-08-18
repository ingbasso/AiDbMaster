using AiDbMaster.Data;
using AiDbMaster.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AiDbMaster.Services
{
    public class CategoryService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<CategoryService>? _logger;

        // Costruttore originale che accetta solo il DbContext (per compatibilità con il codice esistente)
        public CategoryService(ApplicationDbContext context)
        {
            _context = context;
            _logger = null;
        }

        // Nuovo costruttore che accetta anche il logger
        public CategoryService(ApplicationDbContext context, ILogger<CategoryService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<List<DocumentCategory>> GetAllCategoriesAsync()
        {
            return await _context.DocumentCategories.ToListAsync();
        }

        public async Task<DocumentCategory?> GetCategoryByIdAsync(int id)
        {
            return await _context.DocumentCategories.FindAsync(id);
        }

        public async Task<DocumentCategory> CreateCategoryAsync(DocumentCategory category)
        {
            try
            {
                // Verifica se esiste già una categoria con lo stesso nome
                var existingCategory = await GetCategoryByNameAsync(category.Name);
                if (existingCategory != null)
                {
                    LogInformation($"La categoria '{category.Name}' esiste già, restituisco quella esistente");
                    return existingCategory;
                }

                LogInformation($"Creazione della categoria '{category.Name}'");
                _context.DocumentCategories.Add(category);
                await _context.SaveChangesAsync();
                LogInformation($"Categoria '{category.Name}' creata con successo, ID: {category.Id}");
                return category;
            }
            catch (Exception ex)
            {
                LogError(ex, $"Errore durante la creazione della categoria '{category.Name}'");
                throw; // Rilancia l'eccezione per gestirla a livello superiore
            }
        }

        public async Task<bool> UpdateCategoryAsync(DocumentCategory category)
        {
            try
            {
                _context.Update(category);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> DeleteCategoryAsync(int id)
        {
            var category = await _context.DocumentCategories.FindAsync(id);
            if (category == null)
            {
                return false;
            }

            try
            {
                _context.DocumentCategories.Remove(category);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Ottiene una categoria in base al nome
        /// </summary>
        public async Task<DocumentCategory?> GetCategoryByNameAsync(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                LogWarning("GetCategoryByNameAsync chiamato con nome nullo o vuoto");
                return null;
            }
            
            try
            {
                LogInformation($"Ricerca della categoria con nome '{name}'");
                var category = await _context.DocumentCategories
                    .FirstOrDefaultAsync(c => c.Name.ToLower() == name.ToLower());
                
                if (category != null)
                {
                    LogInformation($"Categoria '{name}' trovata, ID: {category.Id}");
                }
                else
                {
                    LogInformation($"Categoria '{name}' non trovata");
                }
                
                return category;
            }
            catch (Exception ex)
            {
                LogError(ex, $"Errore durante la ricerca della categoria '{name}'");
                return null; // In caso di errore, restituisci null invece di propagare l'eccezione
            }
        }
        
        // Metodi di logging per gestire il caso in cui il logger sia null
        private void LogInformation(string message)
        {
            _logger?.LogInformation(message);
            // Se il logger è null, possiamo anche scrivere su console per debug
            if (_logger == null)
            {
                Console.WriteLine($"INFO: {message}");
            }
        }
        
        private void LogWarning(string message)
        {
            _logger?.LogWarning(message);
            if (_logger == null)
            {
                Console.WriteLine($"WARNING: {message}");
            }
        }
        
        private void LogError(Exception ex, string message)
        {
            _logger?.LogError(ex, message);
            if (_logger == null)
            {
                Console.WriteLine($"ERROR: {message}. Exception: {ex.Message}");
            }
        }
    }
} 