using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AiDbMaster.Models
{
    [Table("Pstree_ListaRimanenze")]
    public class PstreeListaRimanenze
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Display(Name = "Valore")]
        public double Valore { get; set; }

        [Display(Name = "Mese")]
        public int Mese { get; set; }

        [Display(Name = "Anno")]
        public int Anno { get; set; }

        [Display(Name = "ID Famiglia")]
        public int IdFamiglia { get; set; }

        [Display(Name = "ID Sede")]
        public int IdSede { get; set; }

        [Display(Name = "Rettifica Valore")]
        public double RettificaValore { get; set; }

        [StringLength(500)]
        [Display(Name = "Note Rettifica")]
        public string? NoteRettifica { get; set; }

        [ForeignKey("IdFamiglia")]
        public virtual PstreeListaFamiglie? Famiglia { get; set; }

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
        [Display(Name = "Valore")]
        public string ValoreFormattato => Valore.ToString("N2");

        [NotMapped]
        [Display(Name = "Valore Effettivo")]
        public double ValoreEffettivo => Valore + RettificaValore;

        [NotMapped]
        public bool HasRettifica => RettificaValore != 0;
    }
}
