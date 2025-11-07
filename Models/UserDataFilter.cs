using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AiDbMaster.Models
{
    /// <summary>
    /// Rappresenta i filtri sui dati per utenti specifici (es. Agenti vedono solo i propri clienti)
    /// </summary>
    [Table("UserDataFilters")]
    public class UserDataFilter
    {
        /// <summary>
        /// ID univoco del filtro
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// ID dell'utente (FK a AspNetUsers)
        /// </summary>
        [Required]
        [StringLength(450)]
        public string UserId { get; set; } = string.Empty;

        /// <summary>
        /// Nome della risorsa su cui applicare il filtro (es. "AnagraficaClienti")
        /// </summary>
        [Required]
        [StringLength(100)]
        public string ResourceName { get; set; } = string.Empty;

        /// <summary>
        /// Tipo di filtro: "OwnData", "AllData", "Custom"
        /// </summary>
        [Required]
        [StringLength(50)]
        public string FilterType { get; set; } = "OwnData";

        /// <summary>
        /// Valore del filtro in formato JSON (per filtri custom complessi)
        /// </summary>
        [Column(TypeName = "nvarchar(max)")]
        public string? FilterValue { get; set; }

        /// <summary>
        /// Indica se il filtro è attivo
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Data di creazione
        /// </summary>
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        // Navigation properties

        /// <summary>
        /// Utente associato
        /// </summary>
        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; } = null!;
    }
}

