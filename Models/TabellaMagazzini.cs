using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AiDbMaster.Models
{
    /// <summary>
    /// Modello per la tabella TabellaMagazzini
    /// Rappresenta i magazzini configurati nel sistema
    /// </summary>
    [Table("TabellaMagazzini")]
    public class TabellaMagazzini
    {
        /// <summary>
        /// Identificativo univoco del record
        /// </summary>
        [Key]
        [Column("ID")]
        public int Id { get; set; }

        /// <summary>
        /// Codice identificativo del magazzino
        /// </summary>
        [Required(ErrorMessage = "Il codice magazzino è obbligatorio")]
        [Display(Name = "Codice Magazzino")]
        [Column("CodiceMagazzino")]
        public short CodiceMagazzino { get; set; }

        /// <summary>
        /// Descrizione del magazzino
        /// </summary>
        [StringLength(50, ErrorMessage = "La descrizione magazzino non può superare i 50 caratteri")]
        [Display(Name = "Descrizione Magazzino")]
        [Column("DescrizioneMagazzino")]
        public string? DescrizioneMagazzino { get; set; }

        /// <summary>
        /// Nome completo del magazzino per la visualizzazione
        /// </summary>
        [NotMapped]
        public string NomeCompleto
        {
            get
            {
                if (!string.IsNullOrEmpty(DescrizioneMagazzino))
                {
                    return $"{CodiceMagazzino} - {DescrizioneMagazzino}";
                }
                return $"Magazzino {CodiceMagazzino}";
            }
        }

        /// <summary>
        /// Descrizione completa per la ricerca e visualizzazione
        /// </summary>
        [NotMapped]
        public string DescrizioneCompleta
        {
            get
            {
                var descrizione = $"Magazzino {CodiceMagazzino}";
                
                if (!string.IsNullOrEmpty(DescrizioneMagazzino))
                {
                    descrizione += $" - {DescrizioneMagazzino}";
                }
                
                return descrizione;
            }
        }

        /// <summary>
        /// Indica se il magazzino ha una descrizione personalizzata
        /// </summary>
        [NotMapped]
        public bool HasDescrizione
        {
            get
            {
                return !string.IsNullOrEmpty(DescrizioneMagazzino);
            }
        }

        /// <summary>
        /// Testo per la ricerca (tutti i campi concatenati)
        /// </summary>
        [NotMapped]
        public string TestoRicerca
        {
            get
            {
                var testo = $"{CodiceMagazzino}";
                
                if (!string.IsNullOrEmpty(DescrizioneMagazzino))
                    testo += $" {DescrizioneMagazzino}";
                
                return testo.ToLower();
            }
        }

        /// <summary>
        /// Identificativo per visualizzazione in badge
        /// </summary>
        [NotMapped]
        public string BadgeText
        {
            get
            {
                return CodiceMagazzino.ToString();
            }
        }

        /// <summary>
        /// Classe CSS per il badge del magazzino
        /// </summary>
        [NotMapped]
        public string BadgeCssClass
        {
            get
            {
                return "badge bg-info text-white";
            }
        }

        /// <summary>
        /// Descrizione breve per tooltip
        /// </summary>
        [NotMapped]
        public string DescrizioneBreve
        {
            get
            {
                if (!string.IsNullOrEmpty(DescrizioneMagazzino))
                {
                    return DescrizioneMagazzino.Length > 30 
                        ? DescrizioneMagazzino.Substring(0, 27) + "..."
                        : DescrizioneMagazzino;
                }
                return $"Magazzino {CodiceMagazzino}";
            }
        }

        /// <summary>
        /// Informazioni per export e API
        /// </summary>
        [NotMapped]
        public string InfoExport
        {
            get
            {
                return $"Codice: {CodiceMagazzino}, Descrizione: {DescrizioneMagazzino ?? "Non specificata"}";
            }
        }
    }
}
