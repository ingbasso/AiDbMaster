using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Text;
using System.Net.Http;
using System.Net.Http.Headers;
using AiDbMaster.Services;

namespace AiDbMaster.Controllers
{
    public class AIQueryController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<AIQueryController> _logger;
        private readonly MistralAIService _mistralService;

        public AIQueryController(
            IConfiguration configuration, 
            ILogger<AIQueryController> logger,
            MistralAIService mistralService)
        {
            _configuration = configuration;
            _logger = logger;
            _mistralService = mistralService;
        }

        [HttpPost]
        public async Task<IActionResult> ConvertToSQL([FromBody] ConvertToSQLRequest request)
        {
            try
            {
                // Determina quale schema usare in base alla pagina di provenienza
                string schemaFileName = DetermineSchemaFile(request.Source);
                
                // Leggi lo schema dal file JSON
                var schemaPath = Path.Combine(Directory.GetCurrentDirectory(), schemaFileName);
                var schemaJson = await System.IO.File.ReadAllTextAsync(schemaPath);
                var schema = JsonSerializer.Deserialize<DatabaseSchema>(schemaJson);

                // Crea il prompt per l'AI
                var prompt = CreatePrompt(schema, request.Query, request.Source);

                // Chiamata a Mistral AI per generare la query
                var sqlQuery = await GenerateSQLQuery(prompt);

                return Json(new { success = true, query = sqlQuery, prompt = prompt });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante la conversione della query");
                return Json(new { success = false, error = "Errore durante la conversione della query: " + ex.Message });
            }
        }

        private string DetermineSchemaFile(string source)
        {
            switch (source?.ToLower())
            {
                case "gestioneordini":
                case "ordini":
                    return "Ordini_AI_prompt_Context.json";
                case "grafici":
                default:
                    return "T01_schema_AI_prompt_context.json";
            }
        }

        private async Task<string> GenerateSQLQuery(string prompt)
        {
            try
            {
                var messages = new[]
                {
                    new { role = "system", content = @"Sei un esperto di SQL Server. Il tuo compito è convertire richieste in linguaggio naturale in query SQL.
Regole da seguire:
1. Genera SOLO la query SQL, senza spiegazioni o testo aggiuntivo
2. La query DEVE iniziare con SELECT o con un commento SQL (--)
3. Usa la sintassi di SQL Server
4. Includi commenti per spiegare la logica
5. Usa alias per tabelle e colonne
6. Includi ORDER BY quando necessario
7. Usa GROUP BY e funzioni di aggregazione quando appropriato
8. Ottimizza la query per le performance
9. NON aggiungere testo prima o dopo la query
10. IMPORTANTE: Per agenti, regioni, province, gruppi, sottogruppi e clienti, usa SEMPRE le colonne con suffisso _DES e MAI quelle con suffisso _COD o Cod
11. Usa INNER JOIN o LEFT JOIN appropriati quando necessario per collegare le tabelle
12. Assicurati che tutti i campi utilizzati esistano nelle tabelle specificate" },
                    new { role = "user", content = prompt }
                };

                _logger.LogInformation($"Invio richiesta a Mistral AI con prompt: {prompt}");
                var response = await _mistralService.GetCompletionAsync(messages);
                _logger.LogInformation($"Risposta ricevuta da Mistral AI: {response}");
                
                if (string.IsNullOrWhiteSpace(response))
                {
                    throw new Exception("La risposta dell'AI è vuota");
                }

                // Estrai la query SQL dalla risposta
                var sqlQuery = response.Trim();
                
                // Rimuovi eventuali backticks o markdown
                sqlQuery = sqlQuery.Replace("```sql", "").Replace("```", "").Trim();
                
                // Verifica che la risposta contenga una query SQL valida
                if (!sqlQuery.StartsWith("SELECT", StringComparison.OrdinalIgnoreCase) && 
                    !sqlQuery.StartsWith("--", StringComparison.OrdinalIgnoreCase))
                {
                    _logger.LogWarning($"La risposta non contiene una query SQL valida: {sqlQuery}");
                    throw new Exception($"La risposta non contiene una query SQL valida. Risposta ricevuta: {sqlQuery}");
                }

                return sqlQuery;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante la generazione della query SQL");
                throw new Exception("Impossibile generare la query SQL: " + ex.Message);
            }
        }

        private string CreatePrompt(DatabaseSchema schema, string naturalLanguageQuery, string source)
        {
            var prompt = new StringBuilder();
            
            if (schema.tables != null && schema.tables.Count > 0)
            {
                // Schema multi-tabella (come per gli ordini)
                prompt.AppendLine("Schema del database:");
                foreach (var table in schema.tables)
                {
                    prompt.AppendLine($"Tabella: {table.table_name}");
                    prompt.AppendLine($"Descrizione: {table.description}");
                    prompt.AppendLine("Colonne:");
                    foreach (var column in table.columns)
                    {
                        prompt.AppendLine($"  - {column.name} ({column.type}): {column.description}");
                    }
                    prompt.AppendLine($"Chiave primaria: {string.Join(", ", table.primary_key)}");
                    if (table.foreign_keys != null && table.foreign_keys.Count > 0)
                    {
                        prompt.AppendLine("Chiavi esterne:");
                        foreach (var fk in table.foreign_keys)
                        {
                            prompt.AppendLine($"  - {fk.column} → {fk.references.table}.{fk.references.column}");
                        }
                    }
                    prompt.AppendLine();
                }
            }
            else
            {
                // Schema singola tabella (come per i grafici)
                prompt.AppendLine("Schema del database:");
                prompt.AppendLine($"Tabella: {schema.table_name}");
                prompt.AppendLine("Colonne disponibili:");
                
                foreach (var column in schema.columns)
                {
                    prompt.AppendLine($"- {column.name} ({column.type}): {column.description}");
                }
            }

            prompt.AppendLine("\nRegole per la conversione:");
            prompt.AppendLine("1. Usa solo le colonne presenti nello schema");
            prompt.AppendLine("2. Genera una query SQL valida e ottimizzata");
            prompt.AppendLine("3. Includi solo le colonne necessarie per la risposta");
            prompt.AppendLine("4. Aggiungi commenti per spiegare la logica della query");
            prompt.AppendLine("5. La query deve essere compatibile con SQL Server");
            prompt.AppendLine("6. Usa alias per le tabelle e le colonne quando appropriato");
            prompt.AppendLine("7. Includi ORDER BY quando necessario");
            prompt.AppendLine("8. Usa GROUP BY e funzioni di aggregazione quando appropriato");
            prompt.AppendLine("9. Per i top N, usa TOP N o OFFSET-FETCH");
            prompt.AppendLine("10. Usa INNER JOIN o LEFT JOIN quando necessario per collegare le tabelle");
            
            if (source?.ToLower() == "grafici")
            {
                prompt.AppendLine("\nRegole IMPORTANTI per le colonne:");
                prompt.AppendLine("- Per gli AGENTI: usa sempre AGENTE_DES e MAI AGENTE_COD");
                prompt.AppendLine("- Per le REGIONI: usa sempre REGIONE_DES e MAI REGIONE_COD");
                prompt.AppendLine("- Per le PROVINCE: usa sempre PROVINCIA_DES e MAI PROVINCIA_COD");
                prompt.AppendLine("- Per i GRUPPI: usa sempre GRUPPO_DES e MAI GRUPPO_COD");
                prompt.AppendLine("- Per i SOTTOGRUPPI: usa sempre SOTTOGRUPPO_DES e MAI SOTTOGRUPPO_COD");
                prompt.AppendLine("- Per i CLIENTI: usa sempre CLIENTE_DES e MAI CodCLIENTE");
                prompt.AppendLine("\nNOTA: Quando si fa riferimento a qualsiasi entità (agenti, regioni, province, gruppi, sottogruppi, clienti), usa SEMPRE la colonna con suffisso _DES e MAI la colonna con suffisso _COD o Cod");
            }
            
            prompt.AppendLine("\nRichiesta in linguaggio naturale:");
            prompt.AppendLine(naturalLanguageQuery);

            prompt.AppendLine("\nGenera SOLO la query SQL, senza spiegazioni o testo aggiuntivo.");

            return prompt.ToString();
        }
    }

    public class ConvertToSQLRequest
    {
        public string Query { get; set; }
        public string Source { get; set; }
    }

    public class DatabaseSchema
    {
        public string table_name { get; set; }
        public List<Column> columns { get; set; }
        public List<Table> tables { get; set; }
    }

    public class Table
    {
        public string table_name { get; set; }
        public string description { get; set; }
        public List<Column> columns { get; set; }
        public List<string> primary_key { get; set; }
        public List<ForeignKey> foreign_keys { get; set; }
    }

    public class ForeignKey
    {
        public string column { get; set; }
        public Reference references { get; set; }
    }

    public class Reference
    {
        public string table { get; set; }
        public string column { get; set; }
    }

    public class Column
    {
        public string name { get; set; }
        public string type { get; set; }
        public string description { get; set; }
    }
} 