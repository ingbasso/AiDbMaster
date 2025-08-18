using AiDbMaster.Models;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AiDbMaster.ViewModels
{
    public class DocumentCreateViewModel
    {
        [Required(ErrorMessage = "Il nome del documento è obbligatorio")]
        [StringLength(255, ErrorMessage = "Il nome non può superare i 255 caratteri")]
        [Display(Name = "Nome Documento")]
        public string? Name { get; set; }

        [StringLength(500, ErrorMessage = "La descrizione non può superare i 500 caratteri")]
        [Display(Name = "Descrizione")]
        public string? Description { get; set; }

        [Display(Name = "Categoria")]
        public int? CategoryId { get; set; }

        [Required(ErrorMessage = "È necessario caricare un file")]
        [Display(Name = "File")]
        public IFormFile? File { get; set; }

        [Display(Name = "Tags (separati da virgola)")]
        public string? Tags { get; set; }

        [Display(Name = "È Confidenziale")]
        public bool IsConfidential { get; set; } = false;
    }

    public class DocumentEditViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Il nome del documento è obbligatorio")]
        [StringLength(255, ErrorMessage = "Il nome non può superare i 255 caratteri")]
        [Display(Name = "Nome Documento")]
        public string? Name { get; set; }

        [StringLength(500, ErrorMessage = "La descrizione non può superare i 500 caratteri")]
        [Display(Name = "Descrizione")]
        public string? Description { get; set; }

        [Display(Name = "Categoria")]
        public int? CategoryId { get; set; }

        [Display(Name = "Tags (separati da virgola)")]
        public string? Tags { get; set; }

        [Display(Name = "È Confidenziale")]
        public bool IsConfidential { get; set; } = false;
    }

    public class DocumentPermissionsViewModel
    {
        public int DocumentId { get; set; }
        public string? DocumentName { get; set; } = string.Empty;
        public List<DocumentPermission>? Permissions { get; set; } = new List<DocumentPermission>();
        public List<ApplicationUser>? AvailableUsers { get; set; } = new List<ApplicationUser>();
    }

    public class GrantPermissionViewModel
    {
        [Required]
        public int DocumentId { get; set; }

        [Required(ErrorMessage = "Seleziona un utente")]
        [Display(Name = "Utente")]
        public string? UserId { get; set; }

        [Required(ErrorMessage = "Seleziona un tipo di permesso")]
        [Display(Name = "Tipo Permesso")]
        public PermissionType PermissionType { get; set; }
    }
} 