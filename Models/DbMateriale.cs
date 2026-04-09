using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AiDbMaster.Models
{
    [Table("DB_Materiali")]
    public class DbMateriale
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }

        [Required]
        [StringLength(50)]
        public string CodiceDistinta { get; set; } = " ";

        [Required]
        [Column(TypeName = "decimal(27,9)")]
        public decimal RigaDistinta { get; set; }

        [Required]
        [StringLength(50)]
        public string CodiceFiglio { get; set; } = " ";

        [Required]
        [StringLength(3)]
        [Column("Unitàmisura")]
        public string UnitaMisura { get; set; } = " ";

        [Required]
        [Column("Quantità", TypeName = "decimal(27,9)")]
        public decimal Quantita { get; set; }

        [Required]
        [StringLength(3)]
        [Column("UnitàMisuraPrincipale")]
        public string UnitaMisuraPrincipale { get; set; } = " ";

        [Required]
        [Column("QuantitàUMP", TypeName = "decimal(27,9)")]
        public decimal QuantitaUMP { get; set; }

        [Required]
        [Column(TypeName = "decimal(27,9)")]
        public decimal PerPezzi { get; set; } = 1;

        [Required]
        [Column(TypeName = "decimal(27,9)")]
        public decimal Sfrido { get; set; }

        public string? Note { get; set; }

        [Required]
        [Column("DataInizioValidità")]
        public DateTime DataInizioValidita { get; set; }

        [Required]
        [Column("DataFineValidità")]
        public DateTime DataFineValidita { get; set; }

        [Required]
        [Column("Codmagazzino", TypeName = "smallint")]
        public short CodMagazzino { get; set; }

        [Required]
        [Column(TypeName = "smallint")]
        public short CodUlterioreMagazzino { get; set; }

        [Required]
        public DateTime UltimoAggiornamento { get; set; } = DateTime.Now;
    }
}
