using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AiDbMaster.Models
{
    /// <summary>
    /// Modello per la tabella degli operatori
    /// </summary>
    [Table("Operatori")]
    public class Operatore
    {
        /// <summary>
        /// Identificativo univoco dell'operatore
        /// </summary>
        [Key]
        public int IdOperatore { get; set; }

        /// <summary>
        /// Codice identificativo dell'operatore
        /// </summary>
        [Required]
        [StringLength(10)]
        public string CodiceOperatore { get; set; } = string.Empty;

        /// <summary>
        /// Nome dell'operatore
        /// </summary>
        [Required]
        [StringLength(50)]
        public string Nome { get; set; } = string.Empty;

        /// <summary>
        /// Cognome dell'operatore
        /// </summary>
        [Required]
        [StringLength(50)]
        public string Cognome { get; set; } = string.Empty;

        /// <summary>
        /// Email dell'operatore
        /// </summary>
        [StringLength(100)]
        public string? Email { get; set; }

        /// <summary>
        /// Telefono dell'operatore
        /// </summary>
        [StringLength(20)]
        public string? Telefono { get; set; }

        /// <summary>
        /// Indica se l'operatore è attivo
        /// </summary>
        public bool Attivo { get; set; } = true;

        /// <summary>
        /// Data di assunzione
        /// </summary>
        public DateTime? DataAssunzione { get; set; }

        /// <summary>
        /// Livello di competenza (1-5)
        /// </summary>
        public int? LivelloCompetenza { get; set; }

        /// <summary>
        /// Note sull'operatore
        /// </summary>
        [StringLength(500)]
        public string? Note { get; set; }

        // Proprietà calcolata per nome completo
        /// <summary>
        /// Nome completo dell'operatore
        /// </summary>
        [NotMapped]
        public string NomeCompleto => $"{Nome} {Cognome}";

        // Navigazione
        /// <summary>
        /// Lista degli ordini di produzione assegnati a questo operatore
        /// </summary>
        public virtual ICollection<ListaOP> OrdiniProduzione { get; set; } = new List<ListaOP>();

    }
}
