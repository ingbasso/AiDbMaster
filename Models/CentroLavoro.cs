using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AiDbMaster.Models
{
    /// <summary>
    /// Modello per la tabella dei centri di lavoro
    /// </summary>
    [Table("CentriLavoro")]
    public class CentroLavoro
    {
        /// <summary>
        /// Identificativo univoco del centro di lavoro
        /// </summary>
        [Key]
        public int IdCentroLavoro { get; set; }

        /// <summary>
        /// Descrizione del centro di lavoro
        /// </summary>
        [Required]
        [StringLength(100)]
        public string DescrizioneCentro { get; set; } = string.Empty;

        /// <summary>
        /// Indica se il centro di lavoro è attivo
        /// </summary>
        public bool Attivo { get; set; } = true;

        /// <summary>
        /// Codice identificativo del centro (opzionale per future espansioni)
        /// </summary>
        [StringLength(10)]
        public string? CodiceCentro { get; set; }

        /// <summary>
        /// Capacità produttiva oraria (opzionale)
        /// </summary>
        public int? CapacitaOraria { get; set; }

        /// <summary>
        /// Costo orario standard del centro (opzionale)
        /// </summary>
        [Column(TypeName = "decimal(10,2)")]
        public decimal? CostoOrarioStandard { get; set; }

        /// <summary>
        /// Note sul centro di lavoro
        /// </summary>
        [StringLength(500)]
        public string? Note { get; set; }

        /// <summary>
        /// Data di creazione del record
        /// </summary>
        public DateTime DataCreazione { get; set; } = DateTime.Now;

        /// <summary>
        /// Data ultima modifica
        /// </summary>
        public DateTime? DataUltimaModifica { get; set; }

        // Navigazione
        /// <summary>
        /// Lista degli ordini di produzione assegnati a questo centro di lavoro
        /// </summary>
        public virtual ICollection<ListaOP> OrdiniProduzione { get; set; } = new List<ListaOP>();
    }
}
