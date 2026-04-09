using AiDbMaster.Models;

namespace AiDbMaster.ViewModels
{
    public class PoliticheRiordinoIndexViewModel
    {
        public List<PoliticaRiordinoMagazzino> Politiche { get; set; } = new();

        // Filtri
        public string? FiltroCodiceArticolo { get; set; }
        public short? FiltroCodiceMagazzino { get; set; }
        public string? FiltroPoliticaDiRiordino { get; set; }

        // Dropdown per i filtri
        public List<short> MagazziniDisponibili { get; set; } = new();
        public List<string> PoliticheDisponibili { get; set; } = new();

        // Paginazione
        public int PaginaCorrente { get; set; } = 1;
        public int RighePerPagina { get; set; } = 20;
        public int TotaleRecord { get; set; }
        public int TotalePagine => (int)Math.Ceiling((double)TotaleRecord / RighePerPagina);
        public bool HaPaginaPrecedente => PaginaCorrente > 1;
        public bool HaPaginaSuccessiva => PaginaCorrente < TotalePagine;
    }

    public class PoliticaRiordinoEditViewModel
    {
        public PoliticaRiordinoMagazzino Politica { get; set; } = new();

        // Dropdown
        public List<short> MagazziniDisponibili { get; set; } = new();
    }
}
