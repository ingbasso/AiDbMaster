namespace AiDbMaster.ViewModels
{
    public class ConsegneKanbanViewModel
    {
        public DateTime DataInizio { get; set; }
        public DateTime DataFine { get; set; }
        public int DurataDefaultMinuti { get; set; } = 240;
        public bool NascondiWeekend { get; set; }

        public List<GiornoKanbanDto> Giorni { get; set; } = new();
        public List<RigaOrdineDaPianificareDto> RigheDaPianificare { get; set; } = new();

        public List<LookupItemDto> TipiTrasporto { get; set; } = new();
        public List<LookupItemDto> Mezzi { get; set; } = new();
        public List<LookupItemDto> MezziInterni { get; set; } = new();
        public List<LookupItemDto> MezziEsterni { get; set; } = new();
        public List<LookupItemDto> Autisti { get; set; } = new();
        public Dictionary<int, int?> MezzoAutistaDefaultMap { get; set; } = new();
        public Dictionary<int, MezzoRimorchioInfoDto> MezzoRimorchioMap { get; set; } = new();
        public Dictionary<int, MezzoEsternoInfoDto> MezzoEsternoInfoMap { get; set; } = new();
    }

    public class GiornoKanbanDto
    {
        public DateTime Data { get; set; }
        public string Etichetta => $"{Data:ddd dd/MM}";
        public List<ViaggioKanbanDto> Viaggi { get; set; } = new();

        /// <summary>Fermi mezzi interni che coprono questa giornata (chiave = MezzoTrasportoId).</summary>
        public List<IndisponibilitaInfoDto> MezziNonDisponibili { get; set; } = new();

        /// <summary>Assenze autisti che coprono questa giornata.</summary>
        public List<IndisponibilitaInfoDto> AutistiAssenti { get; set; } = new();
    }

    public class IndisponibilitaInfoDto
    {
        public int Id { get; set; }
        public int? MezzoId { get; set; }
        public int? AutistaId { get; set; }
        public string Soggetto { get; set; } = string.Empty;
        public string Causale { get; set; } = string.Empty;
        public bool GiornoIntero { get; set; }
        public TimeSpan? OraInizio { get; set; }
        public TimeSpan? OraFine { get; set; }
        public string Fascia => GiornoIntero || !OraInizio.HasValue || !OraFine.HasValue
            ? "Tutto il giorno"
            : $"{OraInizio.Value:hh\\:mm}-{OraFine.Value:hh\\:mm}";
    }

    public class ViaggioKanbanDto
    {
        public int Id { get; set; }
        public DateTime DataConsegna { get; set; }
        public int TipoTrasportoId { get; set; }
        public string TipoTrasporto { get; set; } = string.Empty;
        public int? MezzoTrasportoId { get; set; }
        public int? MezzoTrasportoEsternoId { get; set; }
        public string Mezzo { get; set; } = string.Empty;
        public bool ConRimorchio { get; set; }
        public decimal PortataMaxKg { get; set; }
        public decimal PesoTotaleKg { get; set; }
        public decimal PercentualeCarico => PortataMaxKg <= 0 ? 0 : Math.Round((PesoTotaleKg / PortataMaxKg) * 100, 1);
        public TimeSpan OraPartenza { get; set; }
        public TimeSpan OraArrivoEffettiva { get; set; }
        public string Stato { get; set; } = string.Empty;
        public string? Note { get; set; }
        public int? AutistaId { get; set; }
        public string? Autista { get; set; }
        public decimal? CostoTrasporto { get; set; }
        public decimal? PrezzoVendita { get; set; }
        public int? TempoPausa { get; set; }
        public int? TempoScarico { get; set; }
        public bool? Gru { get; set; }
        public bool? Trasbordo { get; set; }
        public decimal Margine => (PrezzoVendita ?? 0) - (CostoTrasporto ?? 0);
        public List<string> Destinazioni { get; set; } = new();
        public List<string> Clienti { get; set; } = new();
        public List<RigaAssegnataDto> Righe { get; set; } = new();
    }

    public class RigaAssegnataDto
    {
        public int ViaggioRigaId { get; set; }
        public int OrdineRigaId { get; set; }
        public string OrdineCompleto { get; set; } = string.Empty;
        public int RigaOrdine { get; set; }
        public DateTime DataConsegna { get; set; }
        public string CodiceArticolo { get; set; } = string.Empty;
        public string? DescrizioneArticolo { get; set; }
        public decimal QuantitaAssegnata { get; set; }
        public decimal PesoTotaleKg { get; set; }
        public string? NoteRiga { get; set; }
        public string Cliente { get; set; } = string.Empty;
        public string? Localita { get; set; }
        public int CodiceCliente { get; set; }
        public int? CodiceDestinazione { get; set; }
        public bool GruDestinazione { get; set; }
        public bool TrasbordoDestinazione { get; set; }
        public decimal PrezzoVenditaDestinazione { get; set; }
    }

    public class RigaOrdineDaPianificareDto
    {
        public int OrdineRigaId { get; set; }
        public string OrdineCompleto { get; set; } = string.Empty;
        public int RigaOrdine { get; set; }
        public DateTime DataConsegna { get; set; }
        public int CodiceCliente { get; set; }
        public string Cliente { get; set; } = string.Empty;
        public string? Destinazione { get; set; }
        public string? ProvinciaDest { get; set; }
        public string? ComuneDest { get; set; }
        public string CodiceArticolo { get; set; } = string.Empty;
        public string? DescrizioneArticolo { get; set; }
        public decimal QuantitaOriginale { get; set; }
        public decimal QuantitaGiaAssegnata { get; set; }
        public decimal QuantitaResidua { get; set; }
        public string? UnitaMisura { get; set; }
        public decimal? PesoUnitarioKg { get; set; }
        public decimal? PesoResiduoKg => PesoUnitarioKg.HasValue ? Math.Round(PesoUnitarioKg.Value * QuantitaResidua, 3) : null;
        public bool IsParzialmenteAssegnata => QuantitaGiaAssegnata > 0;
    }

    public class LookupItemDto
    {
        public int Id { get; set; }
        public string Text { get; set; } = string.Empty;
    }

    public class MezzoRimorchioInfoDto
    {
        public bool RimorchioDisponibile { get; set; }
        public decimal? PortataMaxConRimorchioKg { get; set; }
    }

    public class MezzoEsternoInfoDto
    {
        public decimal Costo { get; set; }
        public bool Gru { get; set; }
        public bool Trasbordo { get; set; }
        public string? Regione { get; set; }
        public string? Provincia { get; set; }
        public string? Comune { get; set; }
        public string? NomeVettore { get; set; }
    }
}
