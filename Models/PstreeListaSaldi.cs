using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AiDbMaster.Models
{
    [Table("Pstree_ListaSaldi")]
    public class PstreeListaSaldi
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [StringLength(20)]
        [Display(Name = "Codice PdC")]
        public string CodicePdC { get; set; } = string.Empty;

        [Display(Name = "Dare")]
        [Column(TypeName = "money")]
        public decimal? Dare { get; set; }

        [Display(Name = "Avere")]
        [Column(TypeName = "money")]
        public decimal? Avere { get; set; }

        [Display(Name = "Mese")]
        public int? Mese { get; set; }

        [Display(Name = "Anno")]
        public int? Anno { get; set; }

        [Display(Name = "ID Famiglia")]
        public int? IdFamiglia { get; set; }

        [Display(Name = "ID Sede")]
        public int? IdSede { get; set; }

        [ForeignKey("CodicePdC")]
        public virtual PstreeListaPianoDeiConti? PianoDeiConti { get; set; }

        [ForeignKey("IdFamiglia")]
        public virtual PstreeListaFamiglie? Famiglia { get; set; }

        [ForeignKey("IdSede")]
        public virtual PstreeListaSedi? Sede { get; set; }

        [NotMapped]
        [Display(Name = "Saldo")]
        public decimal Saldo => (Dare ?? 0) - (Avere ?? 0);

        [NotMapped]
        [Display(Name = "Nome Mese")]
        public string NomeMese => Mese switch
        {
            1 => "Gennaio", 2 => "Febbraio", 3 => "Marzo", 4 => "Aprile",
            5 => "Maggio", 6 => "Giugno", 7 => "Luglio", 8 => "Agosto",
            9 => "Settembre", 10 => "Ottobre", 11 => "Novembre", 12 => "Dicembre",
            _ => "-"
        };

        [NotMapped]
        [Display(Name = "Periodo")]
        public string Periodo => Mese.HasValue && Anno.HasValue ? $"{NomeMese} {Anno}" : "-";
    }
}
