namespace AiDbMaster.Models
{
    /// <summary>
    /// Impostazioni di Gestione Cantieri lette da appsettings (sezione Cantieri).
    /// Il percorso documenti si può sovrascrivere da TabellaOpzioni (Cantieri.CartellaDocumenti).
    /// </summary>
    public class CantieriOptions
    {
        public const string SectionName = "Cantieri";
        public const string NomeOpzioneCartella = "Cantieri.CartellaDocumenti";

        /// <summary>
        /// Cartella radice dei documenti cantiere sul file server (percorso UNC).
        /// Esempio: \\Svrfav\aidbmaster
        /// </summary>
        public string CartellaDocumenti { get; set; } = string.Empty;
    }
}
