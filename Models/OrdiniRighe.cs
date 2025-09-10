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
        [Column("mo_magaz")]
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
        [Column("mo_coleva")]
        public decimal ColliEvasi { get; set; }

        /// <summary>
        /// Quantità evasa
        /// </summary>
        [Required(ErrorMessage = "La quantità evasa è obbligatoria")]
        [Display(Name = "Quantità Evasa")]
        [DisplayFormat(DataFormatString = "{0:N2}", ApplyFormatInEditMode = true)]
        [Column("mo_quaeva")]
        public decimal QuantitaEvasa { get; set; }

        /// <summary>
        /// Colli prenotati
        /// </summary>
        [Required(ErrorMessage = "I colli prenotati sono obbligatori")]
        [Display(Name = "Colli Prenotati")]
        [DisplayFormat(DataFormatString = "{0:N2}", ApplyFormatInEditMode = true)]
        [Column("mo_colpre")]
        public decimal ColliPrenotati { get; set; }

        /// <summary>
        /// Quantità prenotata
        /// </summary>
        [Required(ErrorMessage = "La quantità prenotata è obbligatoria")]
        [Display(Name = "Quantità Prenotata")]
        [DisplayFormat(DataFormatString = "{0:N2}", ApplyFormatInEditMode = true)]
        [Column("mo_quapre")]
        public decimal QuantitaPrenotata { get; set; }

        /// <summary>
        /// Flag evasione prenotazione
        /// </summary>
        [Required(ErrorMessage = "Il flag evasione prenotazione è obbligatorio")]
        [StringLength(1, ErrorMessage = "Il flag deve essere di 1 carattere")]
        [Display(Name = "Flag Evasione")]
        [Column("mo_flevapre")]
        public string FlagEvasionePrenotazione { get; set; } = string.Empty;

        /// <summary>
        /// Prezzo unitario
        /// </summary>
        [Required(ErrorMessage = "Il prezzo è obbligatorio")]
        [Display(Name = "Prezzo Unitario")]
        [DisplayFormat(DataFormatString = "{0:C}", ApplyFormatInEditMode = true)]
        [Column("Prezzo")]
        public decimal Prezzo { get; set; }

        /// <summary>
        /// Sconto 1 (percentuale)
        /// </summary>
        [Required(ErrorMessage = "Lo sconto 1 è obbligatorio")]
        [Display(Name = "Sconto 1 %")]
        [DisplayFormat(DataFormatString = "{0:N2}%", ApplyFormatInEditMode = true)]
        [Column("mo_scont1")]
        public decimal Sconto1 { get; set; }

        /// <summary>
        /// Sconto 2 (percentuale)
        /// </summary>
        [Required(ErrorMessage = "Lo sconto 2 è obbligatorio")]
        [Display(Name = "Sconto 2 %")]
        [DisplayFormat(DataFormatString = "{0:N2}%", ApplyFormatInEditMode = true)]
        [Column("mo_scont2")]
        public decimal Sconto2 { get; set; }

        /// <summary>
        /// Sconto 3 (percentuale)
        /// </summary>
        [Required(ErrorMessage = "Lo sconto 3 è obbligatorio")]
        [Display(Name = "Sconto 3 %")]
        [DisplayFormat(DataFormatString = "{0:N2}%", ApplyFormatInEditMode = true)]
        [Column("mo_scont3")]
        public decimal Sconto3 { get; set; }

        /// <summary>
        /// Provvigione (percentuale)
        /// </summary>
        [Required(ErrorMessage = "La provvigione è obbligatoria")]
        [Display(Name = "Provvigione %")]
        [DisplayFormat(DataFormatString = "{0:N2}%", ApplyFormatInEditMode = true)]
        [Column("mo_provv")]
        public decimal Provvigione { get; set; }

        /// <summary>
        /// Codice IVA
        /// </summary>
        [Required(ErrorMessage = "Il codice IVA è obbligatorio")]
        [Display(Name = "Codice IVA")]
        [Column("mo_codiva")]
        public short CodiceIva { get; set; }

        /// <summary>
        /// Prezzo con IVA
        /// </summary>
        [Required(ErrorMessage = "Il prezzo IVA è obbligatorio")]
        [Display(Name = "Prezzo + IVA")]
        [DisplayFormat(DataFormatString = "{0:C}", ApplyFormatInEditMode = true)]
        [Column("mo_preziva")]
        public decimal PrezzoConIva { get; set; }

        /// <summary>
        /// Prezzo in valuta
        /// </summary>
        [Required(ErrorMessage = "Il prezzo valuta è obbligatorio")]
        [Display(Name = "Prezzo Valuta")]
        [DisplayFormat(DataFormatString = "{0:C}", ApplyFormatInEditMode = true)]
        [Column("mo_prezvalc")]
        public decimal PrezzoValuta { get; set; }

        /// <summary>
        /// Note della riga
        /// </summary>
        [Display(Name = "Note")]
        [Column("mo_note")]
        public string? Note { get; set; }

        /// <summary>
        /// Prezzo di listino
        /// </summary>
        [Required(ErrorMessage = "Il prezzo listino è obbligatorio")]
        [Display(Name = "Prezzo Listino")]
        [DisplayFormat(DataFormatString = "{0:C}", ApplyFormatInEditMode = true)]
        [Column("mo_prelist")]
        public decimal PrezzoListino { get; set; }

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
        /// Prezzo netto dopo gli sconti
        /// </summary>
        [NotMapped]
        public decimal PrezzoNetto
        {
            get
            {
                var prezzoScontato = Prezzo;
                
                if (Sconto1 > 0)
                    prezzoScontato = prezzoScontato * (1 - Sconto1 / 100);
                
                if (Sconto2 > 0)
                    prezzoScontato = prezzoScontato * (1 - Sconto2 / 100);
                
                if (Sconto3 > 0)
                    prezzoScontato = prezzoScontato * (1 - Sconto3 / 100);
                
                return Math.Round(prezzoScontato, 4);
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
                
                if (!string.IsNullOrEmpty(Note))
                    testo += $" {Note}";
                
                testo += $" {StatoEvasione}";
                
                return testo.ToLower();
            }
        }
    }
}
