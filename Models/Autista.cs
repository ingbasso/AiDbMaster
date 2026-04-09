using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AiDbMaster.Models
{
    [Table("Autisti")]
    public class Autista
    {
        [Key]
        [Column("ID")]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Nome")]
        [Column("Nome")]
        public string Nome { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        [Display(Name = "Cognome")]
        [Column("Cognome")]
        public string Cognome { get; set; } = string.Empty;

        [StringLength(20)]
        [Display(Name = "Telefono")]
        [Column("Telefono")]
        public string? Telefono { get; set; }

        [Display(Name = "Attivo")]
        [Column("Attivo")]
        public bool Attivo { get; set; } = true;

        [NotMapped]
        public string NomeCompleto => $"{Cognome} {Nome}";
    }
}
