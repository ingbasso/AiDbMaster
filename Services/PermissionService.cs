using AiDbMaster.Data;
using AiDbMaster.Models;
using Microsoft.EntityFrameworkCore;

namespace AiDbMaster.Services
{
    /// <summary>
    /// Servizio per la gestione dei permessi sui documenti (legacy)
    /// </summary>
    public class PermissionService
    {
        private readonly ApplicationDbContext _context;

        public PermissionService(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Ottiene i permessi per un documento specifico
        /// </summary>
        public async Task<List<DocumentPermission>> GetPermissionsByDocumentIdAsync(int documentId)
        {
            return await _context.DocumentPermissions
                .Include(p => p.User)
                .Include(p => p.GrantedBy)
                .Where(p => p.DocumentId == documentId)
                .ToListAsync();
        }

        /// <summary>
        /// Concede un permesso su un documento
        /// </summary>
        public async Task<DocumentPermission> GrantPermissionAsync(DocumentPermission permission)
        {
            _context.DocumentPermissions.Add(permission);
            await _context.SaveChangesAsync();
            return permission;
        }

        /// <summary>
        /// Revoca un permesso su un documento
        /// </summary>
        public async Task RevokePermissionAsync(int documentId, string userId)
        {
            var permission = await _context.DocumentPermissions
                .FirstOrDefaultAsync(p => p.DocumentId == documentId && p.UserId == userId);
            
            if (permission != null)
            {
                _context.DocumentPermissions.Remove(permission);
                await _context.SaveChangesAsync();
            }
        }
    }
}
