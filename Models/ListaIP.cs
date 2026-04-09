using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AiDbMaster.Models
{
    /// <summary>
    /// Modello per la tabella ListaIP (Impegni di Produzione).
    /// </summary>
    [Table("ListaIP")]
    public class ListaIP
    {
        [Key]
        [Column("ID")]
        public int ID { get; set; }

        [Required]
        [StringLength(1)]
        [Column("TipoOrdine", TypeName = "varchar(1)")]
        public string TipoOrdine { get; set; } = string.Empty;

        [Required]
        [Column("AnnoOrdine")]
        public short AnnoOrdine { get; set; }

        [Required]
        [StringLength(3)]
        [Column("SerieOrdine", TypeName = "varchar(3)")]
        public string SerieOrdine { get; set; } = string.Empty;

        [Required]
        [Column("NumeroOrdine")]
        public int NumeroOrdine { get; set; }

        [Required]
        [Column("RigaOrdine")]
        public int RigaOrdine { get; set; }

        [Required]
        [Column("RigaImpegno")]
        public int RigaImpegno { get; set; }

        [Required]
        [Column("CodiceMagazzino")]
        public short CodiceMagazzino { get; set; }

        [Required]
        [StringLength(50)]
        [Column("CodiceArticolo", TypeName = "varchar(50)")]
        public string CodiceArticolo { get; set; } = string.Empty;

        [StringLength(255)]
        [Column("DescrizioneArticolo", TypeName = "nvarchar(255)")]
        public string? DescrizioneArticolo { get; set; }

        [Required]
        [Column("DataConsegna")]
        public DateTime DataConsegna { get; set; }

        [StringLength(3)]
        [Column("UnitaMisura", TypeName = "varchar(3)")]
        public string? UnitaMisura { get; set; }

        [Required]
        [Column("Quantita", TypeName = "decimal(27,9)")]
        public decimal Quantita { get; set; }

        [StringLength(3)]
        [Column("UnitaMisuraColli", TypeName = "varchar(3)")]
        public string? UnitaMisuraColli { get; set; }

        [Required]
        [Column("NumeroColli", TypeName = "decimal(27,9)")]
        public decimal NumeroColli { get; set; }

        [Required]
        [Column("ColliEvasi", TypeName = "decimal(27,9)")]
        public decimal ColliEvasi { get; set; }

        [Required]
        [Column("QuantitaEvasa", TypeName = "decimal(27,9)")]
        public decimal QuantitaEvasa { get; set; }

        [Required]
        [Column("Prezzo", TypeName = "decimal(24,6)")]
        public decimal Prezzo { get; set; }

        [Column("NoteRiga", TypeName = "varchar(max)")]
        public string? NoteRiga { get; set; }

        [Required]
        [Column("ValoreRiga", TypeName = "money")]
        public decimal ValoreRiga { get; set; }

        [Required]
        [Column("UltimoAggiornamento")]
        public DateTime UltimoAggiornamento { get; set; }
    }
}
