using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AiDbMaster.Models
{
    /// <summary>
    /// Modello per la tabella CalendarioFermiCentriLavoro
    /// Rappresenta il calendario dei fermi programmati dei centri di lavoro
    /// </summary>
    [Table("CalendarioFermiCentriLavoro")]
    public class CalendarioFermiCentriLavoro
    {
        /// <summary>
        /// Identificativo univoco del fermo
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Codice del centro di lavoro (FK verso CentriLavoro)
        /// </summary>
        [Required(ErrorMessage = "Il codice centro è obbligatorio")]
        [StringLength(10)]
        [Display(Name = "Codice Centro")]
        [Column("CodiceCentro")]
        public string CodiceCentro { get; set; } = string.Empty;

        /// <summary>
        /// Data e ora di inizio del fermo
        /// </summary>
        [Required(ErrorMessage = "La data inizio fermo è obbligatoria")]
        [Display(Name = "Data Inizio Fermo")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy HH:mm}", ApplyFormatInEditMode = true)]
        [Column("DataInizioFermo")]
        public DateTime DataInizioFermo { get; set; }

        /// <summary>
        /// Data e ora di fine del fermo (nullable se fermo ancora attivo)
        /// </summary>
        [Display(Name = "Data Fine Fermo")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy HH:mm}", ApplyFormatInEditMode = true)]
        [Column("DataFineFermo")]
        public DateTime? DataFineFermo { get; set; }

        /// <summary>
        /// Tipo di fermo (WeekEnd, Festivo)
        /// </summary>
        [Required(ErrorMessage = "Il tipo fermo è obbligatorio")]
        [Display(Name = "Tipo Fermo")]
        [Column("TipoFermo")]
        public TipoFermo TipoFermo { get; set; }

        /// <summary>
        /// Descrizione/motivo del fermo
        /// </summary>
        [StringLength(200, ErrorMessage = "Il motivo non può superare i 200 caratteri")]
        [Display(Name = "Motivo")]
        [Column("Motivo")]
        public string? Motivo { get; set; }

        /// <summary>
        /// Note aggiuntive sul fermo
        /// </summary>
        [Display(Name = "Note")]
        [Column("Note")]
        public string? Note { get; set; }

        /// <summary>
        /// Indica se il fermo è pianificato o imprevisto
        /// </summary>
        [Required]
        [Display(Name = "Pianificato")]
        [Column("IsPianificato")]
        public bool IsPianificato { get; set; } = true;

        /// <summary>
        /// Data di creazione del record
        /// </summary>
        [Required]
        [Display(Name = "Data Creazione")]
        [Column("DataCreazione")]
        public DateTime DataCreazione { get; set; } = DateTime.Now;

        /// <summary>
        /// Data di ultima modifica del record
        /// </summary>
        [Display(Name = "Data Ultima Modifica")]
        [Column("DataUltimaModifica")]
        public DateTime? DataUltimaModifica { get; set; }

        // Proprietà di navigazione

        /// <summary>
        /// Centro di lavoro associato al fermo
        /// </summary>
        [ForeignKey("CodiceCentro")]
        public virtual CentroLavoro? CentroLavoro { get; set; }

        // Proprietà calcolate

        /// <summary>
        /// Indica se il fermo è attualmente attivo
        /// </summary>
        [NotMapped]
        public bool IsFermoAttivo
        {
            get
            {
                var now = DateTime.Now;
                return DataInizioFermo <= now && (!DataFineFermo.HasValue || DataFineFermo.Value >= now);
            }
        }

        /// <summary>
        /// Indica se il fermo è futuro (non ancora iniziato)
        /// </summary>
        [NotMapped]
        public bool IsFermoFuturo
        {
            get
            {
                return DataInizioFermo > DateTime.Now;
            }
        }

        /// <summary>
        /// Indica se il fermo è terminato
        /// </summary>
        [NotMapped]
        public bool IsFermoTerminato
        {
            get
            {
                return DataFineFermo.HasValue && DataFineFermo.Value < DateTime.Now;
            }
        }

        /// <summary>
        /// Durata del fermo (effettiva se terminato, parziale se in corso, prevista se futuro)
        /// </summary>
        [NotMapped]
        public TimeSpan? DurataFermo
        {
            get
            {
                if (DataFineFermo.HasValue)
                {
                    return DataFineFermo.Value - DataInizioFermo;
                }
                else if (DataInizioFermo <= DateTime.Now)
                {
                    // Fermo in corso
                    return DateTime.Now - DataInizioFermo;
                }
                return null; // Fermo futuro senza data fine
            }
        }

        /// <summary>
        /// Durata formattata in formato leggibile
        /// </summary>
        [NotMapped]
        public string DurataFormattata
        {
            get
            {
                if (DurataFermo.HasValue)
                {
                    var durata = DurataFermo.Value;
                    if (durata.TotalDays >= 1)
                        return $"{Math.Floor(durata.TotalDays)} giorni, {durata.Hours} ore";
                    else if (durata.TotalHours >= 1)
                        return $"{Math.Floor(durata.TotalHours)} ore, {durata.Minutes} minuti";
                    else
                        return $"{durata.Minutes} minuti";
                }
                return "N/D";
            }
        }

        /// <summary>
        /// Stato del fermo per la visualizzazione
        /// </summary>
        [NotMapped]
        public string StatoFermo
        {
            get
            {
                if (IsFermoFuturo)
                    return "Programmato";
                else if (IsFermoAttivo)
                    return "In Corso";
                else if (IsFermoTerminato)
                    return "Terminato";
                return "Sconosciuto";
            }
        }

        /// <summary>
        /// CSS class per lo stato del fermo
        /// </summary>
        [NotMapped]
        public string StatoFermoCssClass
        {
            get
            {
                return StatoFermo switch
                {
                    "Programmato" => "badge bg-info",
                    "In Corso" => "badge bg-warning text-dark",
                    "Terminato" => "badge bg-secondary",
                    _ => "badge bg-light text-dark"
                };
            }
        }

        /// <summary>
        /// Descrizione completa del fermo
        /// </summary>
        [NotMapped]
        public string DescrizioneCompleta
        {
            get
            {
                var descrizione = $"{TipoFermo} - {StatoFermo}";
                
                if (!string.IsNullOrEmpty(Motivo))
                {
                    descrizione += $": {Motivo}";
                }
                
                return descrizione;
            }
        }

        /// <summary>
        /// Periodo del fermo formattato
        /// </summary>
        [NotMapped]
        public string PeriodoFormattato
        {
            get
            {
                var periodo = DataInizioFermo.ToString("dd/MM/yyyy HH:mm");
                
                if (DataFineFermo.HasValue)
                {
                    periodo += $" - {DataFineFermo.Value:dd/MM/yyyy HH:mm}";
                }
                else
                {
                    periodo += " - In corso";
                }
                
                return periodo;
            }
        }
    }
}


