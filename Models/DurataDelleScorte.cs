using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AiDbMaster.Models
{
    [Table("DurataDelleScorte")]
    public class DurataDelleScorte
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("ID")]
        public int ID { get; set; }

        [Required]
        [Display(Name = "Cod. Marca")]
        [Column("CodMarca")]
        public short CodMarca { get; set; }

        [StringLength(50)]
        [Display(Name = "Descrizione Marca")]
        [Column("DescrizioneMarca", TypeName = "varchar(50)")]
        public string? DescrizioneMarca { get; set; }

        [Required]
        [StringLength(4)]
        [Display(Name = "Cod. Famiglia")]
        [Column("CodFamiglia", TypeName = "varchar(4)")]
        public string CodFamiglia { get; set; } = string.Empty;

        [StringLength(50)]
        [Display(Name = "Descr. Famiglia")]
        [Column("DescrFamiglia", TypeName = "varchar(50)")]
        public string? DescrFamiglia { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Codice Articolo")]
        [Column("CodiceArticolo", TypeName = "varchar(50)")]
        public string CodiceArticolo { get; set; } = string.Empty;

        [Required]
        [StringLength(255)]
        [Display(Name = "Descrizione")]
        [Column("Descrizione", TypeName = "varchar(255)")]
        public string Descrizione { get; set; } = string.Empty;

        [Required]
        [StringLength(3)]
        [Display(Name = "U.M.")]
        [Column("UnitàMisura", TypeName = "varchar(3)")]
        public string UnitaMisura { get; set; } = string.Empty;

        [Display(Name = "Magazzino")]
        [Column("Magazzino")]
        public short? Magazzino { get; set; }

        [Display(Name = "Data Ultimo Scarico")]
        [Column("DataUltimoScarico")]
        public DateTime? DataUltimoScarico { get; set; }

        [Display(Name = "Esistenza")]
        [Column("Esistenza")]
        public decimal? Esistenza { get; set; }

        [Display(Name = "Disponibilità")]
        [Column("Disponibilità")]
        public decimal? Disponibilita { get; set; }

        [Display(Name = "Consumo Ultimo Mese")]
        [Column("ConsumoUltimomese")]
        public decimal? ConsumoUltimoMese { get; set; }

        [Display(Name = "Consumo Due Mesi Fa")]
        [Column("ConsumoDueMesifa")]
        public decimal? ConsumoDueMesiFa { get; set; }

        [Display(Name = "Consumo Tre Mesi Fa")]
        [Column("ConsumoTreMesifa")]
        public decimal? ConsumoTreMesiFa { get; set; }

        [Display(Name = "Consumo Medio Ponderato")]
        [Column("ConsumoMedioPonderato")]
        public decimal? ConsumoMedioPonderato { get; set; }

        [Display(Name = "Durata delle Scorte")]
        [Column("DurataDelleScorte")]
        public decimal? DurataScorte { get; set; }
    }
}
