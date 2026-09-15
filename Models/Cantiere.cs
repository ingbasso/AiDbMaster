using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AiDbMaster.Models
{
    [Table("Cantieri")]
    public class Cantiere
    {
        [Key]
        [Column("ID")]
        public int Id { get; set; }

        [Required]
        [StringLength(20)]
        [Display(Name = "Codice")]
        [Column("Codice")]
        public string Codice { get; set; } = string.Empty;

        [Required(ErrorMessage = "Il nome del cantiere è obbligatorio")]
        [StringLength(150)]
        [Display(Name = "Nome cantiere")]
        [Column("Nome")]
        public string Nome { get; set; } = string.Empty;

        [Required(ErrorMessage = "Il cliente è obbligatorio")]
        [Display(Name = "Cliente")]
        [Column("CodiceCliente")]
        public int CodiceCliente { get; set; }

        [NotMapped]
        public AnagraficaClienti? Cliente { get; set; }

        [Display(Name = "Destinazione")]
        [Column("CodiceDestinazione")]
        public int? CodiceDestinazione { get; set; }

        [StringLength(70)]
        [Display(Name = "Indirizzo")]
        [Column("Indirizzo")]
        public string? Indirizzo { get; set; }

        [StringLength(10)]
        [Display(Name = "CAP")]
        [Column("Cap")]
        public string? Cap { get; set; }

        [StringLength(50)]
        [Display(Name = "Località")]
        [Column("Localita")]
        public string? Localita { get; set; }

        [StringLength(2)]
        [Display(Name = "Provincia")]
        [Column("Provincia")]
        public string? Provincia { get; set; }

        [StringLength(18)]
        [Display(Name = "Telefono")]
        [Column("Telefono")]
        public string? Telefono { get; set; }

        [StringLength(100)]
        [Display(Name = "Referente")]
        [Column("Referente")]
        public string? Referente { get; set; }

        [Display(Name = "Impresa di posa")]
        [Column("ImpresaPosaId")]
        public int? ImpresaPosaId { get; set; }

        public virtual ImpresaPosa? ImpresaPosa { get; set; }

        [Display(Name = "Inizio previsto")]
        [Column("DataInizioPrevista")]
        public DateTime? DataInizioPrevista { get; set; }

        [Display(Name = "Fine prevista")]
        [Column("DataFinePrevista")]
        public DateTime? DataFinePrevista { get; set; }

        [Display(Name = "Stato")]
        [Column("Stato")]
        public CantiereStato Stato { get; set; } = CantiereStato.Preventivo;

        [Display(Name = "Note")]
        [Column("Note")]
        public string? Note { get; set; }

        [StringLength(500)]
        [Display(Name = "Cartella documenti")]
        [Column("PercorsoDocumenti")]
        public string? PercorsoDocumenti { get; set; }

        [StringLength(50)]
        [Display(Name = "Trasporto")]
        [Column("TipoTrasporto")]
        public string? TipoTrasporto { get; set; }

        [Display(Name = "Pulizia cantiere")]
        [Column("PuliziaCantiere")]
        public bool PuliziaCantiere { get; set; }

        [Display(Name = "Check affidabilità cliente")]
        [Column("CheckAffidabilitaCliente")]
        public bool CheckAffidabilitaCliente { get; set; }

        [StringLength(100)]
        [Display(Name = "Pagamento")]
        [Column("Pagamento")]
        public string? Pagamento { get; set; }

        [Display(Name = "Conferma firmata")]
        [Column("ConfermaFirmata")]
        public bool ConfermaFirmata { get; set; }

        [Display(Name = "Trasformato in Favaro1")]
        [Column("TrasformatoInFavaro1")]
        public bool TrasformatoInFavaro1 { get; set; }

        [StringLength(20)]
        [Display(Name = "Magazzino N.")]
        [Column("MagazzinoN")]
        public string? MagazzinoN { get; set; }

        [Display(Name = "Accredito acconto")]
        [Column("AccreditoAcconto")]
        public bool AccreditoAcconto { get; set; }

        [Display(Name = "Anagrafica SDI/PEC")]
        [Column("AnagraficaSdiPec")]
        public bool AnagraficaSdiPec { get; set; }

        [StringLength(100)]
        [Display(Name = "Banca")]
        [Column("Banca")]
        public string? Banca { get; set; }

        [Display(Name = "Aliquota IVA %")]
        [Column("AliquotaIva", TypeName = "decimal(5,2)")]
        public decimal? AliquotaIva { get; set; }

        [Display(Name = "Doc. agevolazione IVA")]
        [Column("DocAgevolazioneIva")]
        public bool DocAgevolazioneIva { get; set; }

        [Display(Name = "Contratto posatore firmato")]
        [Column("ContrattoPosatoreFirmato")]
        public bool ContrattoPosatoreFirmato { get; set; }

        [Display(Name = "PSC")]
        [Column("Psc")]
        public bool Psc { get; set; }

        [Display(Name = "POS Favaro1")]
        [Column("PosFavaro1")]
        public bool PosFavaro1 { get; set; }

        [Display(Name = "Fine posa firmato")]
        [Column("FinePosaFirmato")]
        public bool FinePosaFirmato { get; set; }

        [Display(Name = "Fine posa il")]
        [Column("FinePosaData")]
        public DateTime? FinePosaData { get; set; }

        [Display(Name = "Contabilità di cantiere")]
        [Column("ContabilitaCantiere")]
        public bool ContabilitaCantiere { get; set; }

        [Display(Name = "Fattura saldo attivo")]
        [Column("FatturaSaldoAttivo")]
        public bool FatturaSaldoAttivo { get; set; }

        [Display(Name = "Fattura saldo passiva")]
        [Column("FatturaSaldoPassivo")]
        public bool FatturaSaldoPassivo { get; set; }

        [Display(Name = "Creato il")]
        [Column("DataCreazione")]
        public DateTime DataCreazione { get; set; } = DateTime.Now;

        [Display(Name = "Modificato il")]
        [Column("DataUltimaModifica")]
        public DateTime? DataUltimaModifica { get; set; }

        [StringLength(100)]
        [Column("UtenteCreazione")]
        public string? UtenteCreazione { get; set; }

        [StringLength(100)]
        [Column("UtenteModifica")]
        public string? UtenteModifica { get; set; }

        public virtual ICollection<CantiereOrdine> Ordini { get; set; } = new List<CantiereOrdine>();

        public virtual ICollection<CantiereReferente> Referenti { get; set; } = new List<CantiereReferente>();

        public virtual ICollection<CantiereContabilita> Contabilita { get; set; } = new List<CantiereContabilita>();

        [NotMapped]
        public string IndirizzoCompleto
        {
            get
            {
                var parti = new List<string>();
                if (!string.IsNullOrWhiteSpace(Indirizzo))
                    parti.Add(Indirizzo);

                var citta = new List<string>();
                if (!string.IsNullOrWhiteSpace(Cap))
                    citta.Add(Cap);
                if (!string.IsNullOrWhiteSpace(Localita))
                    citta.Add(Localita);
                if (!string.IsNullOrWhiteSpace(Provincia))
                    citta.Add($"({Provincia})");

                if (citta.Count > 0)
                    parti.Add(string.Join(" ", citta));

                return parti.Count > 0 ? string.Join(", ", parti) : string.Empty;
            }
        }
    }
}
