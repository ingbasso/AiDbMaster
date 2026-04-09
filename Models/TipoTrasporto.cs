using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AiDbMaster.Models
{
    [Table("TipiTrasporto")]
    public class TipoTrasporto
    {
        [Key]
        [Column("ID")]
        public int Id { get; set; }

        [Required]
        [StringLength(20)]
        [Display(Name = "Codice")]
        [Column("Codice")]
        public string Codice { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        [Display(Name = "Descrizione")]
        [Column("Descrizione")]
        public string Descrizione { get; set; } = string.Empty;

        [Display(Name = "Attivo")]
        [Column("Attivo")]
        public bool Attivo { get; set; } = true;
    }
}
