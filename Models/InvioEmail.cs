using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AiDbMaster.Models
{
    /// <summary>
    /// Modello per la tabella InvioEmail.
    /// Traccia le notifiche email inviate per le righe ordine (es. "Avviso Merce Pronta").
    /// Ogni record rappresenta un invio email per una specifica riga ordine,
    /// evitando l'invio duplicato della stessa notifica.
    /// </summary>
    [Table("InvioEmail")]
    public class InvioEmail
    {
        /// <summary>
        /// ID auto-incrementale - Chiave primaria
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("ID")]
        public int ID { get; set; }

        /// <summary>
        /// Tipo ordine: "R" = Ordine Cliente, "O" = Ordine Fornitore.
        /// Corrisponde a OrdiniTestate.TipoOrdine / OrdiniRighe.TipoOrdine.
        /// </summary>
        [Required]
        [StringLength(1)]
        [Display(Name = "Tipo Ordine")]
        [Column("TipoOrdine", TypeName = "varchar(1)")]
        public string TipoOrdine { get; set; } = string.Empty;

        /// <summary>
        /// Anno dell'ordine.
        /// Corrisponde a OrdiniTestate.AnnoOrdine / OrdiniRighe.AnnoOrdine.
        /// </summary>
        [Required]
        [Display(Name = "Anno Ordine")]
        [Column("AnnoOrdine")]
        public short AnnoOrdine { get; set; }

        /// <summary>
        /// Serie dell'ordine.
        /// Corrisponde a OrdiniTestate.SerieOrdine / OrdiniRighe.SerieOrdine.
        /// </summary>
        [Required]
        [StringLength(3)]
        [Display(Name = "Serie Ordine")]
        [Column("SerieOrdine", TypeName = "varchar(3)")]
        public string SerieOrdine { get; set; } = string.Empty;

        /// <summary>
        /// Numero dell'ordine.
        /// Corrisponde a OrdiniTestate.NumeroOrdine / OrdiniRighe.NumeroOrdine.
        /// </summary>
        [Required]
        [Display(Name = "Numero Ordine")]
        [Column("NumeroOrdine")]
        public int NumeroOrdine { get; set; }

        /// <summary>
        /// Numero della riga ordine.
        /// Corrisponde a OrdiniRighe.RigaOrdine.
        /// </summary>
        [Required]
        [Display(Name = "Riga Ordine")]
        [Column("RigaOrdine")]
        public int RigaOrdine { get; set; }

        /// <summary>
        /// Data e ora in cui la notifica email è stata inviata.
        /// </summary>
        [Required]
        [Display(Name = "Data Invio")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy HH:mm}", ApplyFormatInEditMode = false)]
        [Column("DataInvio")]
        public DateTime DataInvio { get; set; }

        /// <summary>
        /// Indica se il record è stato contabilizzato/processato.
        /// Valori: 'S' = Sì, 'N' = No. Default: 'N'.
        /// </summary>
        [Required]
        [StringLength(1)]
        [Display(Name = "Contabilizzato")]
        [Column("Contabilizzato", TypeName = "varchar(1)")]
        public string Contabilizzato { get; set; } = "N";

        /// <summary>
        /// Origine dell'invio: "Manuale" o "Automatico".
        /// </summary>
        [StringLength(20)]
        [Display(Name = "Origine")]
        [Column("Origine", TypeName = "varchar(20)")]
        public string Origine { get; set; } = "Manuale";
    }
}
