using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AiDbMaster.Models
{
    [Table("LogEmailAutomatico")]
    public class LogEmailAutomatico
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("ID")]
        public int ID { get; set; }

        [Required]
        [Column("DataOra")]
        public DateTime DataOra { get; set; } = DateTime.Now;

        /// <summary>
        /// Invio, Saltato, Errore, Info
        /// </summary>
        [Required]
        [StringLength(20)]
        [Column("Tipo", TypeName = "varchar(20)")]
        public string Tipo { get; set; } = "Info";

        [Column("AnnoOrdine")]
        public short? AnnoOrdine { get; set; }

        [StringLength(3)]
        [Column("SerieOrdine", TypeName = "varchar(3)")]
        public string? SerieOrdine { get; set; }

        [Column("NumeroOrdine")]
        public int? NumeroOrdine { get; set; }

        [Column("RigaOrdine")]
        public int? RigaOrdine { get; set; }

        [Column("CodiceCliente")]
        public int? CodiceCliente { get; set; }

        [StringLength(200)]
        [Column("RagioneSociale", TypeName = "varchar(200)")]
        public string? RagioneSociale { get; set; }

        [StringLength(200)]
        [Column("EmailDestinatario", TypeName = "varchar(200)")]
        public string? EmailDestinatario { get; set; }

        /// <summary>
        /// OK, Fallito, Saltato, -
        /// </summary>
        [Required]
        [StringLength(20)]
        [Column("Esito", TypeName = "varchar(20)")]
        public string Esito { get; set; } = "-";

        [StringLength(500)]
        [Column("Motivo", TypeName = "varchar(500)")]
        public string? Motivo { get; set; }

        [Column("Dettagli", TypeName = "varchar(max)")]
        public string? Dettagli { get; set; }
    }
}
