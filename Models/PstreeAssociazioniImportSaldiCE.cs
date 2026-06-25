using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AiDbMaster.Models
{
    [Table("Pstree_AssociazioniImportSaldiCE")]
    public class PstreeAssociazioniImportSaldiCE
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [StringLength(20)]
        [Display(Name = "Codice PdC")]
        public string CodicePdC { get; set; } = string.Empty;

        [Display(Name = "ID Codice Conto")]
        public int IdCodiceConto { get; set; }

        [Display(Name = "ID Sede")]
        public int IdSede { get; set; }

        [Display(Name = "Anno")]
        public int Anno { get; set; }

        [Display(Name = "Mese")]
        public int Mese { get; set; }

        [Display(Name = "Percentuale")]
        public double Percentuale { get; set; }

        [ForeignKey("CodicePdC")]
        public virtual PstreeListaPianoDeiConti? PianoDeiConti { get; set; }

        [ForeignKey("IdCodiceConto")]
        public virtual PstreeStrutturaContoEconomico? ContoEconomico { get; set; }

        [ForeignKey("IdSede")]
        public virtual PstreeListaSedi? Sede { get; set; }

        [NotMapped]
        [Display(Name = "Nome Mese")]
        public string NomeMese => Mese switch
        {
            1 => "Gennaio", 2 => "Febbraio", 3 => "Marzo", 4 => "Aprile",
            5 => "Maggio", 6 => "Giugno", 7 => "Luglio", 8 => "Agosto",
            9 => "Settembre", 10 => "Ottobre", 11 => "Novembre", 12 => "Dicembre",
            _ => "N/D"
        };

        [NotMapped]
        [Display(Name = "Periodo")]
        public string Periodo => $"{NomeMese} {Anno}";

        [NotMapped]
        [Display(Name = "Percentuale")]
        public string PercentualeFormattata => $"{Percentuale:N2}%";
    }
}
