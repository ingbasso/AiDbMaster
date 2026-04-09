using System.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using AiDbMaster.ViewModels;
using AiDbMaster.Attributes;

namespace AiDbMaster.Controllers
{
    [Authorize]
    [RegisterResource("ConsumiProduzione", "Consumi Produzione Previsti", Description = "Consumi produzione previsti da stored procedure", MenuIcon = "bi-clipboard-data", MenuOrder = 6)]
    [RequirePermission("ConsumiProduzione", "View")]
    public class ConsumiProduzioneController : Controller
    {
        private readonly string _connectionString;
        private readonly ILogger<ConsumiProduzioneController> _logger;

        public ConsumiProduzioneController(IConfiguration configuration, ILogger<ConsumiProduzioneController> logger)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Index()
        {
            var model = new ConsumiProduzionePrevViewModel
            {
                DataDa = DateTime.Today,
                DataA = DateTime.Today.AddDays(30)
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(ConsumiProduzionePrevViewModel model)
        {
            if (!model.DataDa.HasValue || !model.DataA.HasValue)
            {
                ModelState.AddModelError("", "Entrambe le date sono obbligatorie.");
                return View(model);
            }

            if (model.DataA < model.DataDa)
            {
                ModelState.AddModelError("", "La data 'A' deve essere uguale o successiva alla data 'Da'.");
                return View(model);
            }

            try
            {
                model.Risultati = await EseguiStoredProcedure(model.DataDa.Value, model.DataA.Value);
                model.RicercaEffettuata = true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante l'esecuzione della stored procedure ElencoImpegniProduzione");
                ModelState.AddModelError("", $"Errore durante l'esecuzione della query: {ex.Message}");
            }

            return View(model);
        }

        private async Task<DataTable> EseguiStoredProcedure(DateTime dataDa, DateTime dataA)
        {
            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand("ElencoImpegniProduzione", connection);
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.Add(new SqlParameter("@DaDataConsegna", SqlDbType.DateTime) { Value = dataDa });
            command.Parameters.Add(new SqlParameter("@ADataConsegna", SqlDbType.DateTime) { Value = dataA });

            using var adapter = new SqlDataAdapter(command);
            var dataTable = new DataTable();

            await connection.OpenAsync();
            await Task.Run(() => adapter.Fill(dataTable));

            return dataTable;
        }
    }
}
