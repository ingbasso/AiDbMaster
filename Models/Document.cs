using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AiDbMaster.Models
{
    public class Document
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(255)]
        [Display(Name = "Nome Documento")]
        public string? Name { get; set; }

        [StringLength(500)]
        [Display(Name = "Descrizione")]
        public string? Description { get; set; }

        [Required]
        [Display(Name = "Tipo File")]
        public DocumentType FileType { get; set; }

        [Required]
        [Display(Name = "Percorso File")]
        public string? FilePath { get; set; }

        [Display(Name = "Dimensione File (KB)")]
        public long FileSize { get; set; }

        [Required]
        [Display(Name = "Data Caricamento")]
        public DateTime UploadDate { get; set; } = DateTime.Now;

        [Display(Name = "Data Ultima Modifica")]
        public DateTime? LastModifiedDate { get; set; }

        [Required]
        [Display(Name = "Caricato Da")]
        public string? UploadedById { get; set; }

        [ForeignKey("UploadedById")]
        public ApplicationUser? UploadedBy { get; set; }

        [Display(Name = "Categoria")]
        public int? CategoryId { get; set; }

        [ForeignKey("CategoryId")]
        public DocumentCategory? Category { get; set; }

        [Display(Name = "Tags")]
        public string? Tags { get; set; }

        [Display(Name = "È Confidenziale")]
        public bool IsConfidential { get; set; } = false;
    }

    public enum DocumentType
    {
        [Display(Name = "PDF")]
        PDF,
        [Display(Name = "Word")]
        DOCX,
        [Display(Name = "Testo")]
        TXT,
        [Display(Name = "Email")]
        EMAIL,
        [Display(Name = "Excel")]
        EXCEL,
        [Display(Name = "Altro")]
        OTHER
    }
} 