using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AiDbMaster.Models
{
    [Table("Pstree_SottoGruppi")]
    public class PstreeSottoGruppi
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [StringLength(10)]
        [Display(Name = "Codice Famiglia")]
        public string CodiceFamiglia { get; set; } = string.Empty;

        [Required]
        [StringLength(10)]
        [Display(Name = "Codice Gruppo")]
        public string CodiceGruppo { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        [Display(Name = "Nome Sotto Gruppo")]
        public string NomeSottoGruppo { get; set; } = string.Empty;

        [StringLength(255)]
        [Display(Name = "Descrizione Sotto Gruppo")]
        public string? DescrizioneSottoGruppo { get; set; }

        [ForeignKey("CodiceFamiglia")]
        public virtual PstreeListaFamiglie? Famiglia { get; set; }

        [NotMapped]
        [Display(Name = "Sotto-Gruppo")]
        public string DescrizioneCompleta => $"{CodiceGruppo} - {NomeSottoGruppo}";

        [NotMapped]
        public string NomeFamiglia => Famiglia?.NomeFamiglia ?? $"Famiglia {CodiceFamiglia}";
    }
}
