namespace AiDbMaster.ViewModels
{
    public class DashboardConsegneViewModel
    {
        public string PeriodoLabel { get; set; } = string.Empty;
        public DateTime DataDa { get; set; }
        public DateTime DataA { get; set; }

        // KPI
        public int TotaleViaggi { get; set; }
        public int TotaleRighe { get; set; }
        public decimal PesoTotaleKg { get; set; }
        public decimal CostoTotale { get; set; }
        public decimal RicavoTotale { get; set; }
        public decimal MargineTotale => RicavoTotale - CostoTotale;
        public decimal MarginePercentuale => RicavoTotale > 0 ? Math.Round((MargineTotale / RicavoTotale) * 100, 1) : 0;
        public decimal UtilizzoMedioPercentuale { get; set; }
        public decimal CostoMedioPerKg => PesoTotaleKg > 0 ? Math.Round(CostoTotale / (PesoTotaleKg / 1000), 2) : 0;
        public decimal PesoMedioPerViaggio => TotaleViaggi > 0 ? Math.Round(PesoTotaleKg / TotaleViaggi, 0) : 0;

        // Confronto periodo precedente
        public int ViaggiPeriodoPrecedente { get; set; }
        public decimal CostoPeriodoPrecedente { get; set; }
        public decimal MarginePeriodoPrecedente { get; set; }
        public decimal VariazioneViaggiPerc => ViaggiPeriodoPrecedente > 0 ? Math.Round(((decimal)(TotaleViaggi - ViaggiPeriodoPrecedente) / ViaggiPeriodoPrecedente) * 100, 1) : 0;
        public decimal VariazioneCostoPerc => CostoPeriodoPrecedente > 0 ? Math.Round(((CostoTotale - CostoPeriodoPrecedente) / CostoPeriodoPrecedente) * 100, 1) : 0;
        public decimal VariazioneMarginePrecedentePerc => MarginePeriodoPrecedente != 0 ? Math.Round(((MargineTotale - MarginePeriodoPrecedente) / Math.Abs(MarginePeriodoPrecedente)) * 100, 1) : 0;

        // Allerte
        public int RigheDaPianificare { get; set; }
        public int ViaggiSenzaAutista { get; set; }
        public int ViaggiSottoutilizzati { get; set; }
        public int ViaggiSenzaPrezzo { get; set; }

        // Grafici (dati serializzati per Chart.js)
        public List<string> GraficoGiorniLabels { get; set; } = new();
        public List<int> GraficoViaggiInterni { get; set; } = new();
        public List<int> GraficoViaggiEsterni { get; set; } = new();
        public List<decimal> GraficoPesoPerGiorno { get; set; } = new();
        public List<decimal> GraficoMarginePerGiorno { get; set; } = new();
        public List<decimal> GraficoMargineCumulato { get; set; } = new();

        // Torta mezzi
        public List<string> TortaMezziLabels { get; set; } = new();
        public List<int> TortaMezziValori { get; set; } = new();

        // Tabelle dettaglio
        public List<DashboardMezzoDto> TopMezzi { get; set; } = new();
        public List<DashboardAutistaDto> TopAutisti { get; set; } = new();
        public List<DashboardClienteDto> TopClienti { get; set; } = new();
    }

    public class DashboardMezzoDto
    {
        public string Mezzo { get; set; } = string.Empty;
        public string Tipo { get; set; } = string.Empty;
        public int Viaggi { get; set; }
        public decimal PesoTotaleKg { get; set; }
        public decimal CostoTotale { get; set; }
        public decimal RicavoTotale { get; set; }
        public decimal Margine => RicavoTotale - CostoTotale;
        public decimal UtilizzoMedioPerc { get; set; }
    }

    public class DashboardAutistaDto
    {
        public string Autista { get; set; } = string.Empty;
        public int Viaggi { get; set; }
        public decimal OreTotali { get; set; }
        public decimal PesoTotaleKg { get; set; }
        public int ViaggiConGru { get; set; }
        public int ViaggiConTrasbordo { get; set; }
    }

    public class DashboardClienteDto
    {
        public string Cliente { get; set; } = string.Empty;
        public int RigheConsegnate { get; set; }
        public decimal PesoTotaleKg { get; set; }
        public int Viaggi { get; set; }
        public decimal CostoTrasporto { get; set; }
        public decimal RicavoTrasporto { get; set; }
    }
}
