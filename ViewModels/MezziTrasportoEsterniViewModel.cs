using AiDbMaster.Models;

namespace AiDbMaster.ViewModels
{
    public class MezziTrasportoEsterniIndexViewModel
    {
        public List<MezzoTrasportoEsterno> Mezzi { get; set; } = new();

        // Filtri
        public string? FiltroComune { get; set; }
        public string? FiltroProvincia { get; set; }
        public string? FiltroRegione { get; set; }
        public string? FiltroTipoMezzo { get; set; }
        public bool? FiltroGru { get; set; }

        // Valori distinti per i dropdown dei filtri
        public List<string> ComuniDisponibili { get; set; } = new();
        public List<string> ProvinceDisponibili { get; set; } = new();
        public List<string> RegioniDisponibili { get; set; } = new();
        public List<string> TipiMezzoDisponibili { get; set; } = new();

        // Paginazione
        public int PaginaCorrente { get; set; } = 1;
        public int RighePerPagina { get; set; } = 20;
        public int TotaleRecord { get; set; }
        public int TotalePagine => (int)Math.Ceiling((double)TotaleRecord / RighePerPagina);
        public bool HaPaginaPrecedente => PaginaCorrente > 1;
        public bool HaPaginaSuccessiva => PaginaCorrente < TotalePagine;
    }
}
