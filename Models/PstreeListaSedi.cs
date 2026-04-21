using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AiDbMaster.Models
{
    [Table("Pstree_ListaSedi")]
    public class PstreeListaSedi
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Sede")]
        public string Sede { get; set; } = string.Empty;

        [StringLength(255)]
        [Display(Name = "Descrizione Sede")]
        public string? DescrizioneSede { get; set; }

        [NotMapped]
        [Display(Name = "Sede")]
        public string DescrizioneCompleta => $"{Id} - {Sede}";
    }
}
