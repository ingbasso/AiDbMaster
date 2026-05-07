using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AiDbMaster.Models
{
    [Table("StoricoMaterialeLiberato")]
    public class StoricoMaterialeLiberato
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("ID")]
        public int ID { get; set; }

        [Required]
        [Display(Name = "Data Liberazione")]
        [Column("DataLiberazione")]
        public DateTime DataLiberazione { get; set; }

        [Required]
        [StringLength(1)]
        [Display(Name = "Tipo Ordine")]
        [Column("TipoOrdine", TypeName = "varchar(1)")]
        public string TipoOrdine { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Anno Ordine")]
        [Column("AnnoOrdine")]
        public short AnnoOrdine { get; set; }

        [Required]
        [StringLength(3)]
        [Display(Name = "Serie Ordine")]
        [Column("SerieOrdine", TypeName = "varchar(3)")]
        public string SerieOrdine { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Numero Ordine")]
        [Column("NumeroOrdine")]
        public int NumeroOrdine { get; set; }

        [Required]
        [Display(Name = "Riga Ordine")]
        [Column("RigaOrdine")]
        public int RigaOrdine { get; set; }

        [Required]
        [Display(Name = "Codice Cliente")]
        [Column("CodiceCliente")]
        public int CodiceCliente { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Ragione Sociale")]
        [Column("RagioneSociale", TypeName = "varchar(50)")]
        public string RagioneSociale { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        [Display(Name = "Codice Articolo")]
        [Column("CodiceArticolo", TypeName = "varchar(50)")]
        public string CodiceArticolo { get; set; } = string.Empty;

        [StringLength(255)]
        [Display(Name = "Descrizione Articolo")]
        [Column("DescrizioneArticolo")]
        public string? DescrizioneArticolo { get; set; }

        [Required]
        [Display(Name = "Data Consegna")]
        [Column("DataConsegna")]
        public DateTime DataConsegna { get; set; }

        [StringLength(3)]
        [Display(Name = "Unità di Misura")]
        [Column("UnitaMisura", TypeName = "varchar(3)")]
        public string? UnitaMisura { get; set; }

        [Required]
        [Display(Name = "Quantità")]
        [Column("Quantita")]
        public decimal Quantita { get; set; }

        [StringLength(3)]
        [Display(Name = "U.M. Colli")]
        [Column("UnitaMisuraColli", TypeName = "varchar(3)")]
        public string? UnitaMisuraColli { get; set; }

        [Required]
        [Display(Name = "Numero Colli")]
        [Column("NumeroColli")]
        public decimal NumeroColli { get; set; }

        [Required]
        [Display(Name = "Ultimo Aggiornamento")]
        [Column("UltimoAggiornamento")]
        public DateTime UltimoAggiornamento { get; set; }

        [NotMapped]
        public string NumeroOrdineCompleto => $"{TipoOrdine}{AnnoOrdine}/{SerieOrdine.Trim()}/{NumeroOrdine:D6}";
    }
}
