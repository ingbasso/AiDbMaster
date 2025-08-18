using AiDbMaster.Data;
using AiDbMaster.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AiDbMaster.Services
{
    public class PermissionService
    {
        private readonly ApplicationDbContext _context;

        public PermissionService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<DocumentPermission>> GetPermissionsByDocumentIdAsync(int documentId)
        {
            return await _context.DocumentPermissions
                .Where(p => p.DocumentId == documentId)
                .Include(p => p.User)
                .Include(p => p.GrantedBy)
                .ToListAsync();
        }

        public async Task<List<DocumentPermission>> GetPermissionsByUserIdAsync(string userId)
        {
            return await _context.DocumentPermissions
                .Where(p => p.UserId == userId)
                .Include(p => p.Document)
                .Include(p => p.GrantedBy)
                .ToListAsync();
        }

        public async Task<DocumentPermission?> GetPermissionAsync(int documentId, string userId)
        {
            return await _context.DocumentPermissions
                .FirstOrDefaultAsync(p => p.DocumentId == documentId && p.UserId == userId);
        }

        public async Task<DocumentPermission> GrantPermissionAsync(DocumentPermission permission)
        {
            // Verifica se esiste già un permesso per questo documento e utente
            var existingPermission = await _context.DocumentPermissions
                .FirstOrDefaultAsync(p => p.DocumentId == permission.DocumentId && p.UserId == permission.UserId);

            if (existingPermission != null)
            {
                // Aggiorna il permesso esistente
                existingPermission.PermissionType = permission.PermissionType;
                existingPermission.GrantedById = permission.GrantedById;
                existingPermission.GrantedDate = permission.GrantedDate;
                
                _context.Update(existingPermission);
                await _context.SaveChangesAsync();
                return existingPermission;
            }
            else
            {
                // Crea un nuovo permesso
                _context.DocumentPermissions.Add(permission);
                await _context.SaveChangesAsync();
                return permission;
            }
        }

        public async Task<bool> RevokePermissionAsync(int documentId, string userId)
        {
            var permission = await _context.DocumentPermissions
                .FirstOrDefaultAsync(p => p.DocumentId == documentId && p.UserId == userId);

            if (permission == null)
            {
                return false;
            }

            try
            {
                _context.DocumentPermissions.Remove(permission);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> RevokeAllPermissionsForDocumentAsync(int documentId)
        {
            var permissions = await _context.DocumentPermissions
                .Where(p => p.DocumentId == documentId)
                .ToListAsync();

            if (!permissions.Any())
            {
                return true;
            }

            try
            {
                _context.DocumentPermissions.RemoveRange(permissions);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
} 