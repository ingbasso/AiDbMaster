using AiDbMaster.Models;
using Microsoft.AspNetCore.Identity;

namespace AiDbMaster.ViewModels
{
    /// <summary>
    /// ViewModel per la pagina di gestione permessi
    /// </summary>
    public class PermissionManagementViewModel
    {
        /// <summary>
        /// Albero gerarchico delle risorse
        /// </summary>
        public List<ResourceTreeNode> ResourceTree { get; set; } = new List<ResourceTreeNode>();

        /// <summary>
        /// ID del ruolo selezionato
        /// </summary>
        public string? CurrentRoleId { get; set; }

        /// <summary>
        /// Nome del ruolo selezionato
        /// </summary>
        public string? CurrentRoleName { get; set; }

        /// <summary>
        /// Lista di tutti i ruoli disponibili
        /// </summary>
        public List<IdentityRole> Roles { get; set; } = new List<IdentityRole>();

        /// <summary>
        /// Dizionario dei permessi: ResourceId -> Permission
        /// </summary>
        public Dictionary<int, Permission> Permissions { get; set; } = new Dictionary<int, Permission>();

        /// <summary>
        /// Numero di risorse non configurate
        /// </summary>
        public int UnconfiguredResourcesCount { get; set; }
    }

    /// <summary>
    /// Nodo dell'albero gerarchico delle risorse
    /// </summary>
    public class ResourceTreeNode
    {
        /// <summary>
        /// Risorsa
        /// </summary>
        public Resource Resource { get; set; } = null!;

        /// <summary>
        /// Risorse figlie (sottomenu)
        /// </summary>
        public List<Resource> Children { get; set; } = new List<Resource>();
    }

    /// <summary>
    /// DTO per salvare i permessi
    /// </summary>
    public class SavePermissionsRequest
    {
        public string RoleId { get; set; } = string.Empty;
        public List<PermissionDto> Permissions { get; set; } = new List<PermissionDto>();
    }

    public class PermissionDto
    {
        public int ResourceId { get; set; }
        public bool CanView { get; set; }
        public bool CanCreate { get; set; }
        public bool CanEdit { get; set; }
        public bool CanDelete { get; set; }
    }
}

