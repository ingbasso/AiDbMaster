using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AiDbMaster.Models
{
    /// <summary>
    /// Modello per la tabella degli ordini di produzione
    /// </summary>
    [Table("ListaOP")]
    public class ListaOP
    {
        /// <summary>
        /// Identificativo univoco dell'ordine di produzione
        /// </summary>
        [Key]
        public int IdListaOP { get; set; }

        /// <summary>
        /// Tipo ordine (1 carattere)
        /// </summary>
        [Required]
        [StringLength(1)]
        public string TipoOrdine { get; set; } = string.Empty;

        /// <summary>
        /// Anno dell'ordine
        /// </summary>
        [Required]
        public short AnnoOrdine { get; set; }

        /// <summary>
        /// Serie dell'ordine (3 caratteri)
        /// </summary>
        [Required]
        [StringLength(3)]
        public string SerieOrdine { get; set; } = string.Empty;

        /// <summary>
        /// Numero dell'ordine
        /// </summary>
        [Required]
        public int NumeroOrdine { get; set; }

        /// <summary>
        /// Riga dell'ordine
        /// </summary>
        [Required]
        public int RigaOrdine { get; set; }

        /// <summary>
        /// Descrizione dell'ordine
        /// </summary>
        [StringLength(100)]
        public string? DescrOrdine { get; set; }

        /// <summary>
        /// Codice dell'articolo da produrre
        /// </summary>
        [Required]
        [StringLength(50)]
        public string CodiceArticolo { get; set; } = string.Empty;

        /// <summary>
        /// Descrizione dell'articolo da produrre
        /// </summary>
        [Required]
        [StringLength(50)]
        public string DescrizioneArticolo { get; set; } = string.Empty;

        /// <summary>
        /// Unità di misura
        /// </summary>
        [Required]
        [StringLength(3)]
        public string UnitaMisura { get; set; } = string.Empty;

        /// <summary>
        /// Quantità da produrre
        /// </summary>
        [Required]
        [Column(TypeName = "decimal(10,3)")]
        public decimal Quantita { get; set; }

        /// <summary>
        /// Quantità effettivamente prodotta
        /// </summary>
        [Required]
        [Column(TypeName = "decimal(10,3)")]
        public decimal QuantitaProdotta { get; set; }

        /// <summary>
        /// Data inizio ordine di produzione
        /// </summary>
        [Required]
        public DateTime DataInizioOP { get; set; }

        /// <summary>
        /// Tempo ciclo in secondi
        /// </summary>
        [Required]
        public float TempoCiclo { get; set; }

        /// <summary>
        /// Data inizio setup/attrezzaggio
        /// </summary>
        public DateTime? DataInizioSetup { get; set; }

        /// <summary>
        /// Tempo setup/attrezzaggio in minuti
        /// </summary>
        public float? TempoSetup { get; set; }

        /// <summary>
        /// FK verso la tabella StatiOP
        /// </summary>
        [Required]
        public int IdStato { get; set; }

        /// <summary>
        /// FK verso la tabella CentriLavoro
        /// </summary>
        [Required]
        public int IdCentroLavoro { get; set; }

        /// <summary>
        /// FK verso la tabella Lavorazioni
        /// </summary>
        [Required]
        public int IdLavorazione { get; set; }

        /// <summary>
        /// Note sull'ordine di produzione
        /// </summary>
        [StringLength(400)]
        public string? Note { get; set; }

        // CAMPI AGGIUNTIVI RICHIESTI

        /// <summary>
        /// Data fine ordine di produzione (pianificata/effettiva)
        /// </summary>
        public DateTime? DataFineOP { get; set; }

        /// <summary>
        /// Data fine prevista
        /// </summary>
        public DateTime? DataFinePrevista { get; set; }

        /// <summary>
        /// Priorità dell'ordine (1=Bassa, 2=Normale, 3=Media, 4=Alta, 5=Urgente)
        /// </summary>
        public int? Priorita { get; set; }

        /// <summary>
        /// FK verso la tabella Operatori
        /// </summary>
        public int? IdOperatore { get; set; }

        /// <summary>
        /// Costo orario del centro di lavoro in euro
        /// </summary>
        [Column(TypeName = "decimal(10,2)")]
        public decimal? CostoOrario { get; set; }

        /// <summary>
        /// Tempo effettivamente impiegato in secondi
        /// </summary>
        public float? TempoEffettivo { get; set; }

        // PROPRIETÀ CALCOLATE

        /// <summary>
        /// Percentuale di completamento
        /// </summary>
        [NotMapped]
        public decimal PercentualeCompletamento => 
            Quantita > 0 ? Math.Round((QuantitaProdotta / Quantita) * 100, 2) : 0;

        /// <summary>
        /// Tempo ciclo in formato leggibile (HH:mm:ss)
        /// </summary>
        [NotMapped]
        public string TempoCicloFormattato => TimeSpan.FromSeconds(TempoCiclo).ToString(@"hh\:mm\:ss");

        /// <summary>
        /// Tempo setup in formato leggibile (HH:mm)
        /// </summary>
        [NotMapped]
        public string TempoSetupFormattato => TempoSetup.HasValue ? 
            TimeSpan.FromMinutes(TempoSetup.Value).ToString(@"hh\:mm") : "Non specificato";

        /// <summary>
        /// Tempo effettivo in formato leggibile (HH:mm:ss)
        /// </summary>
        [NotMapped]
        public string TempoEffettivoFormattato => TempoEffettivo.HasValue ? 
            TimeSpan.FromSeconds(TempoEffettivo.Value).ToString(@"hh\:mm\:ss") : "Non specificato";

        /// <summary>
        /// Descrizione della priorità
        /// </summary>
        [NotMapped]
        public string PrioritaDescrizione => Priorita switch
        {
            1 => "Bassa",
            2 => "Normale", 
            3 => "Media",
            4 => "Alta",
            5 => "Urgente",
            _ => "Non definita"
        };

        /// <summary>
        /// Identificativo completo dell'ordine
        /// </summary>
        [NotMapped]
        public string IdentificativoCompleto => $"{TipoOrdine}{AnnoOrdine}/{SerieOrdine}/{NumeroOrdine:D6}-{RigaOrdine}";


        // NAVIGAZIONE

        /// <summary>
        /// Stato dell'ordine di produzione
        /// </summary>
        [ForeignKey("IdStato")]
        public virtual StatoOP? Stato { get; set; }

        /// <summary>
        /// Operatore assegnato all'ordine
        /// </summary>
        [ForeignKey("IdOperatore")]
        public virtual Operatore? Operatore { get; set; }

        /// <summary>
        /// Centro di lavoro assegnato all'ordine
        /// </summary>
        [ForeignKey("IdCentroLavoro")]
        public virtual CentroLavoro? CentroLavoro { get; set; }

        /// <summary>
        /// Lavorazione assegnata all'ordine
        /// </summary>
        [ForeignKey("IdLavorazione")]
        public virtual Lavorazioni? Lavorazione { get; set; }
    }
}
