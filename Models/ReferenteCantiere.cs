using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AiDbMaster.Models
{
    [Table("ReferentiCantiere")]
    public class ReferenteCantiere
    {
        [Key]
        [Column("ID")]
        public int Id { get; set; }

        [Required(ErrorMessage = "Il nome del referente è obbligatorio")]
        [StringLength(100)]
        [Display(Name = "Nome")]
        [Column("Nome")]
        public string Nome { get; set; } = string.Empty;

        [StringLength(18)]
        [Display(Name = "Telefono")]
        [Column("Telefono")]
        public string? Telefono { get; set; }

        [StringLength(100)]
        [EmailAddress(ErrorMessage = "Email non valida")]
        [Display(Name = "Email")]
        [Column("Email")]
        public string? Email { get; set; }

        [Display(Name = "Note")]
        [Column("Note")]
        public string? Note { get; set; }

        [Display(Name = "Attivo")]
        [Column("Attivo")]
        public bool Attivo { get; set; } = true;

        public virtual ICollection<CantiereReferente> Cantieri { get; set; } = new List<CantiereReferente>();
    }
}
