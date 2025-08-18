using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AiDbMaster.Models
{
    public class DocumentCategory
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        [Display(Name = "Nome Categoria")]
        public string Name { get; set; } = string.Empty;

        [StringLength(255)]
        [Display(Name = "Descrizione")]
        public string? Description { get; set; }

        // Relazione con i documenti
        public ICollection<Document>? Documents { get; set; }
    }
} 