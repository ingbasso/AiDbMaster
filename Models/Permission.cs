using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace AiDbMaster.Models
{
    /// <summary>
    /// Rappresenta i permessi di un ruolo su una risorsa specifica
    /// </summary>
    [Table("Permissions")]
    public class Permission
    {
        /// <summary>
        /// ID univoco del permesso
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// ID del ruolo (FK a AspNetRoles)
        /// </summary>
        [Required]
        [StringLength(450)]
        public string RoleId { get; set; } = string.Empty;

        /// <summary>
        /// ID della risorsa
        /// </summary>
        [Required]
        public int ResourceId { get; set; }

        /// <summary>
        /// Permesso di visualizzazione (Read)
        /// </summary>
        public bool CanView { get; set; }

        /// <summary>
        /// Permesso di creazione (Create)
        /// </summary>
        public bool CanCreate { get; set; }

        /// <summary>
        /// Permesso di modifica (Update)
        /// </summary>
        public bool CanEdit { get; set; }

        /// <summary>
        /// Permesso di eliminazione (Delete)
        /// </summary>
        public bool CanDelete { get; set; }

        /// <summary>
        /// Data di creazione del permesso
        /// </summary>
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        /// <summary>
        /// Data ultima modifica
        /// </summary>
        public DateTime? ModifiedDate { get; set; }

        /// <summary>
        /// Utente che ha modificato per ultimo
        /// </summary>
        [StringLength(450)]
        public string? ModifiedBy { get; set; }

        // Navigation properties

        /// <summary>
        /// Ruolo associato
        /// </summary>
        [ForeignKey("RoleId")]
        public virtual IdentityRole Role { get; set; } = null!;

        /// <summary>
        /// Risorsa associata
        /// </summary>
        [ForeignKey("ResourceId")]
        public virtual Resource Resource { get; set; } = null!;
    }
}

