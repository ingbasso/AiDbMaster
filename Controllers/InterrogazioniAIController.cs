using Microsoft.AspNetCore.Mvc;
using AiDbMaster.Models;
using AiDbMaster.Services;
using System.Data;
using System.Text.Json;

namespace AiDbMaster.Controllers
{
    public class InterrogazioniAIController : Controller
    {
        private readonly DatabaseQuery _databaseQuery;
        private readonly IMistralAIService _mistralAIService;
        private readonly ILogger<InterrogazioniAIController> _logger;
        private readonly IWebHostEnvironment _env;

        public InterrogazioniAIController(
            DatabaseQuery databaseQuery, 
            IMistralAIService mistralAIService,
            ILogger<InterrogazioniAIController> logger,
            IWebHostEnvironment env)
        {
            _databaseQuery = databaseQuery;
            _mistralAIService = mistralAIService;
            _logger = logger;
            _env = env;
        }

        public IActionResult Index()
        {
            ViewBag.UseFluidContainer = true;
            return View();
        }

        /// <summary>
        /// Endpoint per interrogazioni AI: prende una domanda in linguaggio naturale e genera+esegue la query SQL
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> AskQuestion([FromBody] AskQuestionRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.Question))
                {
                    return Json(new { success = false, error = "La domanda non può essere vuota" });
                }

                _logger.LogInformation("Domanda ricevuta: {Question}", request.Question);

                // 1. Carica il context del database
                var contextPath = Path.Combine(_env.ContentRootPath, "OrdiniTR_AI_prompt_Context.json");
                var databaseContext = await System.IO.File.ReadAllTextAsync(contextPath);

                // 2. Genera la query SQL tramite Mistral AI
                var sqlQuery = await _mistralAIService.GenerateSQLQueryAsync(request.Question, databaseContext);

                // 3. Valida la query per sicurezza
                if (!MistralAIService.IsSafeQuery(sqlQuery))
                {
                    _logger.LogWarning("Query non sicura rilevata: {Query}", sqlQuery);
                    return Json(new { 
                        success = false, 
                        error = "La query generata contiene comandi non permessi per motivi di sicurezza." 
                    });
                }

                // 4. Esegui la query
                var result = await _databaseQuery.ExecuteQueryAsync(sqlQuery);

                // 5. Converti risultati
                var data = ConvertDataTableToObject(result);

                return Json(new
                {
                    success = true,
                    question = request.Question,
                    sqlQuery = sqlQuery,
                    data = data,
                    rowCount = result.Rows.Count
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante l'elaborazione della domanda");
                return Json(new { 
                    success = false, 
                    error = $"Errore: {ex.Message}" 
                });
            }
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

    public class AskQuestionRequest
    {
        public string Question { get; set; } = "";
    }
} 