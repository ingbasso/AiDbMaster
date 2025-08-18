using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AiDbMaster.Models
{
    public class DocumentPermission
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Display(Name = "Documento")]
        public int DocumentId { get; set; }

        [ForeignKey("DocumentId")]
        public Document? Document { get; set; }

        [Required]
        [Display(Name = "Utente")]
        public string? UserId { get; set; }

        [ForeignKey("UserId")]
        public ApplicationUser? User { get; set; }

        [Required]
        [Display(Name = "Tipo Permesso")]
        public PermissionType PermissionType { get; set; }

        [Display(Name = "Data Concessione")]
        public DateTime GrantedDate { get; set; } = DateTime.Now;

        [Display(Name = "Concesso Da")]
        public string? GrantedById { get; set; }

        [ForeignKey("GrantedById")]
        public ApplicationUser? GrantedBy { get; set; }
    }

    public enum PermissionType
    {
        [Display(Name = "Lettura")]
        Read,
        [Display(Name = "Modifica")]
        Edit,
        [Display(Name = "Eliminazione")]
        Delete,
        [Display(Name = "Gestione Completa")]
        FullControl
    }
} 