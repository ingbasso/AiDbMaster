using Microsoft.AspNetCore.Mvc;
using AiDbMaster.Models;
using System.Data;

namespace AiDbMaster.Controllers
{
    public class GestioneOrdiniController : Controller
    {
        private readonly DatabaseQuery _databaseQuery;
        private readonly ILogger<GestioneOrdiniController> _logger;

        public GestioneOrdiniController(DatabaseQuery databaseQuery, ILogger<GestioneOrdiniController> logger)
        {
            _databaseQuery = databaseQuery;
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ExecuteQuery([FromBody] string query)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(query))
                {
                    return BadRequest("La query non può essere vuota");
                }

                var result = await _databaseQuery.ExecuteQueryAsync(query);
                return Json(new { success = true, data = ConvertDataTableToObject(result) });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore nell'esecuzione della query");
                return Json(new { success = false, error = ex.Message });
            }
        }

        private object ConvertDataTableToObject(DataTable dataTable)
        {
            var rows = new List<Dictionary<string, object>>();
            foreach (DataRow row in dataTable.Rows)
            {
                var dict = new Dictionary<string, object>();
                foreach (DataColumn col in dataTable.Columns)
                {
                    dict[col.ColumnName] = row[col] == DBNull.Value ? null! : row[col];
                }
                rows.Add(dict);
            }
            return rows;
        }
    }
} 