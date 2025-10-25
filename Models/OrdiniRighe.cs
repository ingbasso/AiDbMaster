using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AiDbMaster.Models
{
    /// <summary>
    /// Modello per la tabella OrdiniRighe
    /// Rappresenta le righe degli ordini con dettagli articoli, quantità e prezzi
    /// </summary>
    [Table("OrdiniRighe")]
    public class OrdiniRighe
    {
        /// <summary>
        /// Identificativo univoco del record
        /// </summary>
        [Key]
        [Column("ID")]
        public int Id { get; set; }

        /// <summary>
        /// Tipo ordine: "O" = Ordine Fornitore, "R" = Ordine Cliente
        /// </summary>
        [Required(ErrorMessage = "Il tipo ordine è obbligatorio")]
        [StringLength(1, ErrorMessage = "Il tipo ordine deve essere di 1 carattere")]
        [Display(Name = "Tipo Ordine")]
        [Column("TipoOrdine")]
        public string TipoOrdine { get; set; } = string.Empty;

        /// <summary>
        /// Anno dell'ordine
        /// </summary>
        [Required(ErrorMessage = "L'anno ordine è obbligatorio")]
        [Display(Name = "Anno Ordine")]
        [Column("AnnoOrdine")]
        public short AnnoOrdine { get; set; }

        /// <summary>
        /// Serie dell'ordine
        /// </summary>
        [Required(ErrorMessage = "La serie ordine è obbligatoria")]
        [StringLength(3, ErrorMessage = "La serie ordine non può superare i 3 caratteri")]
        [Display(Name = "Serie Ordine")]
        [Column("SerieOrdine")]
        public string SerieOrdine { get; set; } = string.Empty;

        /// <summary>
        /// Numero dell'ordine
        /// </summary>
        [Required(ErrorMessage = "Il numero ordine è obbligatorio")]
        [Display(Name = "Numero Ordine")]
        [Column("NumeroOrdine")]
        public int NumeroOrdine { get; set; }

        /// <summary>
        /// Numero della riga nell'ordine
        /// </summary>
        [Required(ErrorMessage = "Il numero riga è obbligatorio")]
        [Display(Name = "Riga Ordine")]
        [Column("RigaOrdine")]
        public int RigaOrdine { get; set; }

        /// <summary>
        /// Codice magazzino per la riga
        /// </summary>
        [Required(ErrorMessage = "Il codice magazzino è obbligatorio")]
        [Display(Name = "Magazzino")]
        [Column("CodiceMagazzino")]
        public short CodiceMagazzino { get; set; }

        /// <summary>
        /// Codice articolo (relazione con AnagraficaArticoli)
        /// </summary>
        [Required(ErrorMessage = "Il codice articolo è obbligatorio")]
        [StringLength(50, ErrorMessage = "Il codice articolo non può superare i 50 caratteri")]
        [Display(Name = "Codice Articolo")]
        [Column("CodiceArticolo")]
        public string CodiceArticolo { get; set; } = string.Empty;

        /// <summary>
        /// Descrizione dell'articolo
        /// </summary>
        [StringLength(50, ErrorMessage = "La descrizione articolo non può superare i 50 caratteri")]
        [Display(Name = "Descrizione Articolo")]
        [Column("DescrizioneArticolo")]
        public string? DescrizioneArticolo { get; set; }

        /// <summary>
        /// Data di consegna prevista per la riga
        /// </summary>
        [Required(ErrorMessage = "La data consegna è obbligatoria")]
        [Display(Name = "Data Consegna")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        [Column("DataConsegna")]
        public DateTime DataConsegna { get; set; }

        /// <summary>
        /// Unità di misura
        /// </summary>
        [StringLength(3, ErrorMessage = "L'unità di misura non può superare i 3 caratteri")]
        [Display(Name = "Unità di Misura")]
        [Column("UnitaMisura")]
        public string? UnitaMisura { get; set; }

        /// <summary>
        /// Quantità ordinata
        /// </summary>
        [Required(ErrorMessage = "La quantità è obbligatoria")]
        [Display(Name = "Quantità")]
        [DisplayFormat(DataFormatString = "{0:N2}", ApplyFormatInEditMode = true)]
        [Column("Quantita")]
        public decimal Quantita { get; set; }

        /// <summary>
        /// Unità di misura per i colli
        /// </summary>
        [StringLength(3, ErrorMessage = "L'unità di misura colli non può superare i 3 caratteri")]
        [Display(Name = "U.M. Colli")]
        [Column("UnitaMisuraColli")]
        public string? UnitaMisuraColli { get; set; }

        /// <summary>
        /// Numero di colli
        /// </summary>
        [Required(ErrorMessage = "Il numero colli è obbligatorio")]
        [Display(Name = "Numero Colli")]
        [DisplayFormat(DataFormatString = "{0:N2}", ApplyFormatInEditMode = true)]
        [Column("NumeroColli")]
        public decimal NumeroColli { get; set; }

        /// <summary>
        /// Colli evasi
        /// </summary>
        [Required(ErrorMessage = "I colli evasi sono obbligatori")]
        [Display(Name = "Colli Evasi")]
        [DisplayFormat(DataFormatString = "{0:N2}", ApplyFormatInEditMode = true)]
        [Column("ColliEvasi")]
        public decimal ColliEvasi { get; set; }

        /// <summary>
        /// Quantità evasa
        /// </summary>
        [Required(ErrorMessage = "La quantità evasa è obbligatoria")]
        [Display(Name = "Quantità Evasa")]
        [DisplayFormat(DataFormatString = "{0:N2}", ApplyFormatInEditMode = true)]
        [Column("QuantitaEvasa")]
        public decimal QuantitaEvasa { get; set; }

        /// <summary>
        /// Prezzo unitario
        /// </summary>
        [Required(ErrorMessage = "Il prezzo è obbligatorio")]
        [Display(Name = "Prezzo Unitario")]
        [DisplayFormat(DataFormatString = "{0:C}", ApplyFormatInEditMode = true)]
        [Column("Prezzo")]
        public decimal Prezzo { get; set; }

        /// <summary>
        /// Percentuale di inclusione dell'articolo nell'ordine
        /// </summary>
        [Display(Name = "Percentuale Inclusione %")]
        [Column("PercentualeInclusione")]
        public int PercentualeInclusione { get; set; }

        /// <summary>
        /// Note della riga
        /// </summary>
        [Display(Name = "Note")]
        [Column("NoteRiga")]
        public string? NoteRiga { get; set; }

        /// <summary>
        /// Valore totale della riga
        /// </summary>
        [Required(ErrorMessage = "Il valore riga è obbligatorio")]
        [Display(Name = "Valore Riga")]
        [DisplayFormat(DataFormatString = "{0:C}", ApplyFormatInEditMode = true)]
        [Column("ValoreRiga")]
        public decimal ValoreRiga { get; set; }

        // Proprietà di navigazione per le relazioni
        /// <summary>
        /// Testata dell'ordine associata
        /// </summary>
        public virtual OrdiniTestate? Testata { get; set; }

        /// <summary>
        /// Articolo associato alla riga
        /// </summary>
        [ForeignKey("CodiceArticolo")]
        public virtual AnagraficaArticoli? Articolo { get; set; }

        /// <summary>
        /// Magazzino associato alla riga
        /// </summary>
        [ForeignKey("CodiceMagazzino")]
        public virtual TabellaMagazzini? Magazzino { get; set; }

        // Proprietà calcolate per la visualizzazione
        /// <summary>
        /// Numero ordine completo per la visualizzazione
        /// </summary>
        [NotMapped]
        public string NumeroOrdineCompleto
        {
            get
            {
                return $"{TipoOrdine}{AnnoOrdine}/{SerieOrdine}/{NumeroOrdine:D6}";
            }
        }

        /// <summary>
        /// Identificativo completo della riga
        /// </summary>
        [NotMapped]
        public string IdentificativoRiga
        {
            get
            {
                return $"{NumeroOrdineCompleto} - Riga {RigaOrdine}";
            }
        }

        /// <summary>
        /// Quantità rimanente da evadere
        /// </summary>
        [NotMapped]
        public decimal QuantitaRimanente
        {
            get
            {
                return Quantita - QuantitaEvasa;
            }
        }

        /// <summary>
        /// Colli rimanenti da evadere
        /// </summary>
        [NotMapped]
        public decimal ColliRimanenti
        {
            get
            {
                return NumeroColli - ColliEvasi;
            }
        }



        /// <summary>
        /// Stato di evasione della riga
        /// </summary>
        [NotMapped]
        public string StatoEvasione
        {
            get
            {
                if (QuantitaEvasa <= 0)
                    return "Da Evadere";
                else if (QuantitaEvasa < Quantita)
                    return "Parzialmente Evasa";
                else
                    return "Completamente Evasa";
            }
        }

        /// <summary>
        /// Classe CSS per lo stato di evasione
        /// </summary>
        [NotMapped]
        public string StatoEvasioneCssClass
        {
            get
            {
                return StatoEvasione switch
                {
                    "Da Evadere" => "badge bg-danger",
                    "Parzialmente Evasa" => "badge bg-warning text-dark",
                    "Completamente Evasa" => "badge bg-success",
                    _ => "badge bg-secondary"
                };
            }
        }



        /// <summary>
        /// Prezzo netto (senza sconti applicati)
        /// </summary>
        [NotMapped]
        public decimal PrezzoNetto
        {
            get
            {
                return Math.Round(Prezzo, 4);
            }
        }

        /// <summary>
        /// Valore netto della riga (quantità x prezzo netto)
        /// </summary>
        [NotMapped]
        public decimal ValoreNetto
        {
            get
            {
                return Math.Round(Quantita * PrezzoNetto, 2);
            }
        }

        /// <summary>
        /// Descrizione completa dell'articolo per la visualizzazione
        /// </summary>
        [NotMapped]
        public string DescrizioneArticoloCompleta
        {
            get
            {
                if (!string.IsNullOrEmpty(DescrizioneArticolo))
                {
                    return $"{CodiceArticolo} - {DescrizioneArticolo}";
                }
                return CodiceArticolo;
            }
        }

        /// <summary>
        /// Informazioni quantità per la visualizzazione
        /// </summary>
        [NotMapped]
        public string InfoQuantita
        {
            get
            {
                var info = $"{Quantita:N2}";
                
                if (!string.IsNullOrEmpty(UnitaMisura))
                {
                    info += $" {UnitaMisura}";
                }
                
                if (NumeroColli > 0)
                {
                    info += $" ({NumeroColli:N2}";
                    if (!string.IsNullOrEmpty(UnitaMisuraColli))
                    {
                        info += $" {UnitaMisuraColli}";
                    }
                    info += ")";
                }
                
                return info;
            }
        }

        /// <summary>
        /// Informazioni evasione per la visualizzazione
        /// </summary>
        [NotMapped]
        public string InfoEvasione
        {
            get
            {
                return $"Evasa: {QuantitaEvasa:N2}/{Quantita:N2}";
            }
        }

        /// <summary>
        /// Chiave composita per la relazione con la testata
        /// </summary>
        [NotMapped]
        public string ChiaveComposita
        {
            get
            {
                return $"{TipoOrdine}|{AnnoOrdine}|{SerieOrdine}|{NumeroOrdine}";
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
                var testo = $"{NumeroOrdineCompleto} {CodiceArticolo}";
                
                if (!string.IsNullOrEmpty(DescrizioneArticolo))
                    testo += $" {DescrizioneArticolo}";
                
                if (!string.IsNullOrEmpty(NoteRiga))
                    testo += $" {NoteRiga}";
                
                testo += $" {StatoEvasione}";
                
                return testo.ToLower();
            }
        }
    }
}
