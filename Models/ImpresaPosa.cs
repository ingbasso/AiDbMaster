using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AiDbMaster.Models
{
    [Table("ImpresePosa")]
    public class ImpresaPosa
    {
        [Key]
        [Column("ID")]
        public int Id { get; set; }

        [Required(ErrorMessage = "Il nome dell'impresa è obbligatorio")]
        [StringLength(150)]
        [Display(Name = "Nome")]
        [Column("Nome")]
        public string Nome { get; set; } = string.Empty;

        [StringLength(18)]
        [Display(Name = "Telefono")]
        [Column("Telefono")]
        public string? Telefono { get; set; }

        [Display(Name = "Note")]
        [Column("Note")]
        public string? Note { get; set; }

        [Display(Name = "Attiva")]
        [Column("Attivo")]
        public bool Attivo { get; set; } = true;
    }
}
