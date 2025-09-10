using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AiDbMaster.Models
{
    /// <summary>
    /// Modello per la tabella ProgressiviArticoli
    /// Rappresenta le giacenze e i movimenti degli articoli per magazzino nel sistema
    /// </summary>
    [Table("ProgressiviArticoli")]
    public class ProgressiviArticoli
    {
        /// <summary>
        /// Identificativo univoco del record
        /// </summary>
        [Key]
        [Column("ID")]
        public int Id { get; set; }

        /// <summary>
        /// Codice dell'articolo
        /// </summary>
        [Required(ErrorMessage = "Il codice articolo è obbligatorio")]
        [StringLength(50, ErrorMessage = "Il codice articolo non può superare i 50 caratteri")]
        [Display(Name = "Codice Articolo")]
        [Column("CodiceArticolo")]
        public string CodiceArticolo { get; set; } = string.Empty;

        /// <summary>
        /// Codice del magazzino
        /// </summary>
        [Required(ErrorMessage = "Il codice magazzino è obbligatorio")]
        [Display(Name = "Codice Magazzino")]
        [Column("CodiceMagazzino")]
        public short CodiceMagazzino { get; set; }

        /// <summary>
        /// Quantità esistente in magazzino
        /// </summary>
        [Required(ErrorMessage = "L'esistenza è obbligatoria")]
        [Display(Name = "Esistenza")]
        [Column("Esistenza")]
        [DisplayFormat(DataFormatString = "{0:N2}", ApplyFormatInEditMode = true)]
        public decimal Esistenza { get; set; }

        /// <summary>
        /// Quantità ordinata (in arrivo)
        /// </summary>
        [Required(ErrorMessage = "La quantità ordinata è obbligatoria")]
        [Display(Name = "Ordinato")]
        [Column("Ordinato")]
        [DisplayFormat(DataFormatString = "{0:N2}", ApplyFormatInEditMode = true)]
        public decimal Ordinato { get; set; }

        /// <summary>
        /// Quantità impegnata (riservata per ordini)
        /// </summary>
        [Required(ErrorMessage = "La quantità impegnata è obbligatoria")]
        [Display(Name = "Impegnato")]
        [Column("Impegnato")]
        [DisplayFormat(DataFormatString = "{0:N2}", ApplyFormatInEditMode = true)]
        public decimal Impegnato { get; set; }

        /// <summary>
        /// Quantità prenotata
        /// </summary>
        [Required(ErrorMessage = "La quantità prenotata è obbligatoria")]
        [Display(Name = "Prenotato")]
        [Column("Prenotato")]
        [DisplayFormat(DataFormatString = "{0:N2}", ApplyFormatInEditMode = true)]
        public decimal Prenotato { get; set; }

        /// <summary>
        /// Quantità disponibile calcolata (Esistenza - Impegnato - Prenotato)
        /// </summary>
        [NotMapped]
        public decimal Disponibile
        {
            get
            {
                return Esistenza - Impegnato - Prenotato;
            }
        }

        /// <summary>
        /// Quantità totale prevista (Esistenza + Ordinato)
        /// </summary>
        [NotMapped]
        public decimal TotalePrevisto
        {
            get
            {
                return Esistenza + Ordinato;
            }
        }

        /// <summary>
        /// Percentuale di impegno sul totale esistente
        /// </summary>
        [NotMapped]
        public decimal PercentualeImpegno
        {
            get
            {
                if (Esistenza == 0) return 0;
                return Math.Round((Impegnato / Esistenza) * 100, 2);
            }
        }

        /// <summary>
        /// Stato della giacenza per la visualizzazione
        /// </summary>
        [NotMapped]
        public string StatoGiacenza
        {
            get
            {
                if (Esistenza <= 0) return "Esaurito";
                if (Disponibile <= 0) return "Non Disponibile";
                if (Disponibile < (Esistenza * 0.2m)) return "Scorta Bassa";
                return "Disponibile";
            }
        }

        /// <summary>
        /// Classe CSS per lo stato della giacenza
        /// </summary>
        [NotMapped]
        public string StatoGiacenzaCssClass
        {
            get
            {
                return StatoGiacenza switch
                {
                    "Esaurito" => "badge bg-danger",
                    "Non Disponibile" => "badge bg-warning text-dark",
                    "Scorta Bassa" => "badge bg-warning",
                    "Disponibile" => "badge bg-success",
                    _ => "badge bg-secondary"
                };
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
                return $"Art: {CodiceArticolo} - Mag: {CodiceMagazzino} - Disp: {Disponibile:N2}";
            }
        }

        /// <summary>
        /// Riepilogo delle quantità
        /// </summary>
        [NotMapped]
        public string RiepilogoQuantita
        {
            get
            {
                return $"E:{Esistenza:N0} O:{Ordinato:N0} I:{Impegnato:N0} P:{Prenotato:N0} D:{Disponibile:N0}";
            }
        }

        /// <summary>
        /// Indica se l'articolo ha movimenti (ordinato, impegnato o prenotato > 0)
        /// </summary>
        [NotMapped]
        public bool HasMovimenti
        {
            get
            {
                return Ordinato > 0 || Impegnato > 0 || Prenotato > 0;
            }
        }

        /// <summary>
        /// Chiave composta articolo-magazzino per identificazione univoca
        /// </summary>
        [NotMapped]
        public string ChiaveArticoloMagazzino
        {
            get
            {
                return $"{CodiceArticolo}|{CodiceMagazzino}";
            }
        }
    }
}
