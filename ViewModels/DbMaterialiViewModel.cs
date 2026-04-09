using AiDbMaster.Models;

namespace AiDbMaster.ViewModels
{
    public class DbMaterialiIndexViewModel
    {
        public List<DbMateriale> Materiali { get; set; } = new();

        // Filtri
        public string? FiltroCodiceDistinta { get; set; }
        public string? FiltroCodiceFiglio { get; set; }
        public short? FiltroCodMagazzino { get; set; }

        // Dropdown per i filtri
        public List<short> MagazziniDisponibili { get; set; } = new();

        // Paginazione
        public int PaginaCorrente { get; set; } = 1;
        public int RighePerPagina { get; set; } = 20;
        public int TotaleRecord { get; set; }
        public int TotalePagine => (int)Math.Ceiling((double)TotaleRecord / RighePerPagina);
        public bool HaPaginaPrecedente => PaginaCorrente > 1;
        public bool HaPaginaSuccessiva => PaginaCorrente < TotalePagine;
    }

    public class DbMaterialeEditViewModel
    {
        public DbMateriale Materiale { get; set; } = new();
    }
}
