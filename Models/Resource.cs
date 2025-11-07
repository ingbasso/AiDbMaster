using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AiDbMaster.Models
{
    /// <summary>
    /// Rappresenta una risorsa (pagina/funzionalità) del sistema
    /// </summary>
    [Table("Resources")]
    public class Resource
    {
        /// <summary>
        /// ID univoco della risorsa
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Nome tecnico della risorsa (es. "AnagraficaArticoli")
        /// Usato nel codice per verificare permessi
        /// </summary>
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Nome visualizzato all'utente (es. "Anagrafica Articoli")
        /// </summary>
        [Required]
        [StringLength(100)]
        public string DisplayName { get; set; } = string.Empty;

        /// <summary>
        /// Descrizione della risorsa
        /// </summary>
        [StringLength(500)]
        public string? Description { get; set; }

        /// <summary>
        /// Icona Bootstrap Icons (es. "bi-table")
        /// </summary>
        [StringLength(50)]
        public string? MenuIcon { get; set; }

        /// <summary>
        /// Ordine di visualizzazione nel menu
        /// </summary>
        public int MenuOrder { get; set; }

        /// <summary>
        /// ID della risorsa parent (per sottomenu)
        /// </summary>
        public int? ParentResourceId { get; set; }

        /// <summary>
        /// Indica se è un gruppo/categoria nel menu (non ha permessi propri)
        /// </summary>
        public bool IsMenuGroup { get; set; }

        /// <summary>
        /// Indica se la risorsa è attiva
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Indica se i permessi sono stati configurati dall'admin
        /// </summary>
        public bool IsConfigured { get; set; } = false;

        /// <summary>
        /// Data di creazione della risorsa
        /// </summary>
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        /// <summary>
        /// ID dell'utente che ha creato la risorsa (auto-registrazione)
        /// </summary>
        [StringLength(450)]
        public string? CreatedBy { get; set; }

        // Navigation properties

        /// <summary>
        /// Risorsa parent (per navigazione gerarchia)
        /// </summary>
        [ForeignKey("ParentResourceId")]
        public virtual Resource? Parent { get; set; }

        /// <summary>
        /// Risorse figlie (sottomenu)
        /// </summary>
        public virtual ICollection<Resource> Children { get; set; } = new List<Resource>();

        /// <summary>
        /// Permessi associati a questa risorsa
        /// </summary>
        public virtual ICollection<Permission> Permissions { get; set; } = new List<Permission>();
    }
}

