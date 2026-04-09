using AiDbMaster.Models;

namespace AiDbMaster.ViewModels
{
    public class DbTestateIndexViewModel
    {
        public List<DbTestata> Testate { get; set; } = new();

        // Filtri
        public string? FiltroCodiceDistinta { get; set; }
        public string? FiltroFantasma { get; set; }
        public string? FiltroGruppo { get; set; }
        public string? FiltroVersione { get; set; }

        // Paginazione
        public int PaginaCorrente { get; set; } = 1;
        public int RighePerPagina { get; set; } = 20;
        public int TotaleRecord { get; set; }
        public int TotalePagine => (int)Math.Ceiling((double)TotaleRecord / RighePerPagina);
        public bool HaPaginaPrecedente => PaginaCorrente > 1;
        public bool HaPaginaSuccessiva => PaginaCorrente < TotalePagine;
    }

    public class DbTestataEditViewModel
    {
        public DbTestata Testata { get; set; } = new();
    }
}
