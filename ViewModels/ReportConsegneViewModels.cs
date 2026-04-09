namespace AiDbMaster.ViewModels
{
    public class FoglioViaggioViewModel
    {
        public int ViaggioId { get; set; }
        public DateTime DataConsegna { get; set; }
        public string TipoTrasporto { get; set; } = string.Empty;
        public string Mezzo { get; set; } = string.Empty;
        public string? Targa { get; set; }
        public decimal PortataMaxKg { get; set; }
        public string? Autista { get; set; }
        public string? TelefonoAutista { get; set; }
        public TimeSpan OraPartenza { get; set; }
        public TimeSpan OraArrivoStimata { get; set; }
        public bool Gru { get; set; }
        public bool Trasbordo { get; set; }
        public int TempoPausa { get; set; }
        public int TempoScarico { get; set; }
        public string? Note { get; set; }
        public decimal CostoTrasporto { get; set; }
        public decimal PrezzoVendita { get; set; }
        public decimal PesoTotaleKg { get; set; }
        public decimal PercentualeCarico => PortataMaxKg > 0 ? Math.Round((PesoTotaleKg / PortataMaxKg) * 100, 1) : 0;
        public List<FoglioViaggioRigaViewModel> Righe { get; set; } = new();
    }

    public class FoglioViaggioRigaViewModel
    {
        public int Progressivo { get; set; }
        public string OrdineCompleto { get; set; } = string.Empty;
        public int RigaOrdine { get; set; }
        public string Cliente { get; set; } = string.Empty;
        public string? Destinazione { get; set; }
        public string CodiceArticolo { get; set; } = string.Empty;
        public string? DescrizioneArticolo { get; set; }
        public decimal QuantitaAssegnata { get; set; }
        public string? UnitaMisura { get; set; }
        public decimal PesoTotaleKg { get; set; }
        public string? NoteRiga { get; set; }
    }

    public class PlanningGiornalieroRigaViewModel
    {
        public int ViaggioId { get; set; }
        public DateTime DataConsegna { get; set; }
        public TimeSpan OraPartenza { get; set; }
        public TimeSpan OraArrivo { get; set; }
        public string TipoTrasporto { get; set; } = string.Empty;
        public string Mezzo { get; set; } = string.Empty;
        public string? Targa { get; set; }
        public string? Autista { get; set; }
        public int NumeroRighe { get; set; }
        public decimal PesoTotaleKg { get; set; }
        public decimal PortataMaxKg { get; set; }
        public string Destinazioni { get; set; } = string.Empty;
        public bool Gru { get; set; }
        public bool Trasbordo { get; set; }
        public decimal CostoTrasporto { get; set; }
        public decimal PrezzoVendita { get; set; }
        public decimal Margine => PrezzoVendita - CostoTrasporto;
        public string Stato { get; set; } = string.Empty;
    }

    public class DistintaCaricoGiornalieraViewModel
    {
        public DateTime DataConsegna { get; set; }
        public decimal PesoTotaleGiornata { get; set; }
        public int TotaleArticoli { get; set; }
        public List<DistintaCaricoViaggioViewModel> Viaggi { get; set; } = new();
    }

    public class DistintaCaricoViaggioViewModel
    {
        public int ViaggioId { get; set; }
        public string Mezzo { get; set; } = string.Empty;
        public string? Targa { get; set; }
        public string? Autista { get; set; }
        public TimeSpan OraPartenza { get; set; }
        public decimal PesoTotaleKg { get; set; }
        public decimal PortataMaxKg { get; set; }
        public List<DistintaCaricoRigaViewModel> Righe { get; set; } = new();
    }

    public class DistintaCaricoRigaViewModel
    {
        public int Progressivo { get; set; }
        public string CodiceArticolo { get; set; } = string.Empty;
        public string? DescrizioneArticolo { get; set; }
        public decimal QuantitaAssegnata { get; set; }
        public string? UnitaMisura { get; set; }
        public decimal PesoUnitarioKg { get; set; }
        public decimal PesoTotaleKg { get; set; }
        public string Cliente { get; set; } = string.Empty;
        public string OrdineCompleto { get; set; } = string.Empty;
        public int RigaOrdine { get; set; }
    }
}
