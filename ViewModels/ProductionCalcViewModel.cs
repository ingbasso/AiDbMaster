namespace AiDbMaster.ViewModels
{
    public class ProductionCalcViewModel
    {
        // Filtro
        public string? CodiceArticolo { get; set; }

        // Dati Articolo (da AnagraficaArticoli)
        public string? Descrizione { get; set; }
        public string? DescrizioneUlteriore { get; set; }
        public string? UnitaMisura { get; set; }
        public decimal QtaUMPPerTavola { get; set; }
        public decimal QtaUMPPerPallet { get; set; }
        public decimal TavolePerPallet { get; set; }

        // Politica Riordino (da PoliticheRiordinoMagazzino)
        public string? PoliticaDiRiordino { get; set; }
        public decimal LottoStandardProduzione { get; set; }
        public decimal Sottolotto { get; set; }
        public decimal ScortaMinima { get; set; }
        public decimal ScortaMassima { get; set; }

        // Dati Produzione (da DB_Lavorazioni)
        public decimal TavoleOraTeoriche { get; set; }
        public decimal Efficienza { get; set; }
        public decimal TavoleOraReali { get; set; }

        // Flag per sapere se è stata fatta una ricerca
        public bool ArticoloTrovato { get; set; }
    }

}
