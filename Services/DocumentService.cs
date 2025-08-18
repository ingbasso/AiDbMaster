using AiDbMaster.Data;
using AiDbMaster.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace AiDbMaster.Services
{
    public class DocumentService
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public DocumentService(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        public async Task<List<Document>> GetAllDocumentsAsync()
        {
            return await _context.Documents
                .Include(d => d.Category)
                .Include(d => d.UploadedBy)
                .ToListAsync();
        }

        public async Task<List<Document>> GetDocumentsByCategoryAsync(int categoryId)
        {
            return await _context.Documents
                .Where(d => d.CategoryId == categoryId)
                .Include(d => d.Category)
                .Include(d => d.UploadedBy)
                .ToListAsync();
        }

        public async Task<Document?> GetDocumentByIdAsync(int id)
        {
            return await _context.Documents
                .Include(d => d.Category)
                .Include(d => d.UploadedBy)
                .FirstOrDefaultAsync(d => d.Id == id);
        }

        public async Task<List<Document>> GetDocumentsByUserIdAsync(string userId)
        {
            return await _context.Documents
                .Where(d => d.UploadedById == userId)
                .Include(d => d.Category)
                .Include(d => d.UploadedBy)
                .ToListAsync();
        }

        public async Task<Document> UploadDocumentAsync(IFormFile file, Document document, string userId)
        {
            try
            {
                // Crea la directory dei documenti se non esiste
                string documentsPath = Path.Combine(_webHostEnvironment.ContentRootPath, "DocumentsStorage");
                if (!Directory.Exists(documentsPath))
                {
                    Directory.CreateDirectory(documentsPath);
                }

                // Crea una sottodirectory per l'utente
                string userPath = Path.Combine(documentsPath, userId);
                if (!Directory.Exists(userPath))
                {
                    Directory.CreateDirectory(userPath);
                }

                // Genera un nome file univoco
                string fileName = $"{Guid.NewGuid()}_{Path.GetFileName(file.FileName)}";
                string filePath = Path.Combine(userPath, fileName);

                // Salva il file
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                // Imposta le proprietà del documento
                document.FilePath = filePath;
                document.FileSize = file.Length / 1024; // Converti in KB
                document.UploadedById = userId;
                document.UploadDate = DateTime.Now;

                // Determina il tipo di documento in base all'estensione
                string extension = Path.GetExtension(file.FileName).ToLower();
                document.FileType = GetDocumentType(extension);

                // Salva il documento nel database
                _context.Documents.Add(document);
                await _context.SaveChangesAsync();

                return document;
            }
            catch (DbUpdateException dbEx)
            {
                // Gestione specifica degli errori del database
                var innerException = dbEx.InnerException?.Message ?? "Errore sconosciuto del database";
                throw new Exception($"Errore durante il salvataggio del documento nel database: {innerException}", dbEx);
            }
            catch (IOException ioEx)
            {
                // Gestione degli errori di I/O
                throw new Exception($"Errore durante la gestione del file: {ioEx.Message}", ioEx);
            }
            catch (Exception ex)
            {
                // Gestione generica degli errori
                throw new Exception($"Errore durante il caricamento del documento: {ex.Message}", ex);
            }
        }

        public async Task<bool> UpdateDocumentAsync(Document document)
        {
            try
            {
                document.LastModifiedDate = DateTime.Now;
                _context.Update(document);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> DeleteDocumentAsync(int id)
        {
            var document = await _context.Documents.FindAsync(id);
            if (document == null)
            {
                return false;
            }

            try
            {
                // Elimina il file fisico
                if (File.Exists(document.FilePath))
                {
                    File.Delete(document.FilePath);
                }

                // Elimina il record dal database
                _context.Documents.Remove(document);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> HasPermissionAsync(int documentId, string userId, PermissionType permissionType)
        {
            // Gli amministratori hanno sempre accesso completo
            if (await IsUserInRoleAsync(userId, UserRoles.Admin))
            {
                return true;
            }

            // I manager hanno accesso completo a tutti i documenti
            if (await IsUserInRoleAsync(userId, UserRoles.Manager) && 
                (permissionType == PermissionType.Read || permissionType == PermissionType.Edit))
            {
                return true;
            }

            // Controlla se l'utente è il proprietario del documento
            var document = await _context.Documents.FindAsync(documentId);
            if (document != null && document.UploadedById == userId)
            {
                return true;
            }

            // Controlla i permessi specifici
            var permission = await _context.DocumentPermissions
                .FirstOrDefaultAsync(p => p.DocumentId == documentId && p.UserId == userId);

            if (permission == null)
            {
                return false;
            }

            // Verifica il tipo di permesso
            switch (permissionType)
            {
                case PermissionType.Read:
                    return true; // Tutti i permessi includono la lettura
                case PermissionType.Edit:
                    return permission.PermissionType == PermissionType.Edit || 
                           permission.PermissionType == PermissionType.Delete || 
                           permission.PermissionType == PermissionType.FullControl;
                case PermissionType.Delete:
                    return permission.PermissionType == PermissionType.Delete || 
                           permission.PermissionType == PermissionType.FullControl;
                case PermissionType.FullControl:
                    return permission.PermissionType == PermissionType.FullControl;
                default:
                    return false;
            }
        }

        private async Task<bool> IsUserInRoleAsync(string userId, string role)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return false;
            }

            var userRoles = await _context.UserRoles
                .Where(ur => ur.UserId == userId)
                .Join(_context.Roles,
                    ur => ur.RoleId,
                    r => r.Id,
                    (ur, r) => r.Name)
                .ToListAsync();

            return userRoles.Contains(role);
        }

        private DocumentType GetDocumentType(string extension)
        {
            switch (extension)
            {
                case ".pdf":
                    return DocumentType.PDF;
                case ".docx":
                case ".doc":
                    return DocumentType.DOCX;
                case ".txt":
                    return DocumentType.TXT;
                case ".eml":
                case ".msg":
                    return DocumentType.EMAIL;
                case ".xls":
                case ".xlsx":
                case ".csv":
                    return DocumentType.EXCEL;
                default:
                    return DocumentType.OTHER;
            }
        }

        /// <summary>
        /// Verifica se un documento con lo stesso percorso file è già stato elaborato
        /// </summary>
        public async Task<bool> IsDocumentProcessedAsync(string filePath)
        {
            return await _context.Documents.AnyAsync(d => d.FilePath == filePath);
        }

        /// <summary>
        /// Crea un nuovo documento senza richiedere un file caricato tramite form
        /// </summary>
        public async Task<Document> CreateDocumentAsync(Document document)
        {
            _context.Documents.Add(document);
            await _context.SaveChangesAsync();
            return document;
        }
    }
} 