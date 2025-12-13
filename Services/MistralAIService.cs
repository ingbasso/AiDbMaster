using System.Text;
using System.Text.Json;

namespace AiDbMaster.Services
{
    public interface IMistralAIService
    {
        Task<string> GenerateSQLQueryAsync(string userQuestion, string databaseContext);
        Task<string> GetCompletionAsync(object[] messages);
        Task<DocumentAnalysisResult?> AnalyzeDocumentAsync(string content, string filename, string fileType);
    }

    public class DocumentAnalysisResult
    {
        public string Categoria { get; set; } = "";
        public string DescrizioneCategoria { get; set; } = "";
        public string Descrizione { get; set; } = "";
        public List<string> Tags { get; set; } = new();
        public bool Confidenziale { get; set; } = false;
    }

    public class MistralAIService : IMistralAIService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<MistralAIService> _logger;
        private const string MISTRAL_API_URL = "https://api.mistral.ai/v1/chat/completions";

        public MistralAIService(
            HttpClient httpClient, 
            IConfiguration configuration,
            ILogger<MistralAIService> logger)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<string> GenerateSQLQueryAsync(string userQuestion, string databaseContext)
        {
            try
            {
                var apiKey = _configuration["MistralAI:ApiKey"];
                if (string.IsNullOrEmpty(apiKey))
                {
                    throw new InvalidOperationException("Mistral AI API Key non configurata in appsettings.json");
                }

                var model = _configuration["MistralAI:Model"] ?? "mistral-large-latest";

                // Costruisci il prompt per l'AI
                var systemPrompt = @"Sei un esperto SQL Server assistant. Il tuo compito è generare query SQL sicure e ottimizzate basandoti sul contesto del database fornito.

REGOLE IMPORTANTI:
1. Genera SOLO la query SQL, senza spiegazioni o commenti
2. Usa SEMPRE TipoOrdine = 'R' per filtrare gli ordini clienti
3. Per JOIN tra OrdiniTestate e OrdiniRighe usa la chiave composita: (TipoOrdine, AnnoOrdine, SerieOrdine, NumeroOrdine)
4. Usa GETDATE() per la data odierna
5. Usa TOP N invece di LIMIT
6. Formatta date come 'YYYY-MM-DD'
7. NON usare DROP, DELETE, UPDATE, INSERT, ALTER, TRUNCATE
8. Usa INNER JOIN o LEFT JOIN appropriati
9. Includi sempre nomi tabelle completi (es: OrdiniRighe.DataConsegna)
10. Per calcoli su fatturato usa ValoreRiga, per quantità usa (Quantita - QuantitaEvasa)
11. AGGIUNGI SEMPRE 'DISTINCT' nella SELECT finale (l'ultima SELECT se ci sono subquery)
12. FORMATTA SEMPRE gli importi con 2 decimali usando CAST o ROUND: ROUND(CAST(campo AS DECIMAL(18,2)), 2)

REGOLE PER CONSEGNE E QUANTITÀ:
- Quando la domanda riguarda CONSEGNE, includi SEMPRE queste tre colonne:
  * Quantita (la quantità ordinata)
  * QuantitaEvasa (la quantità già consegnata/evasa)
  * NON creare colonne calcolate come QuantitaDaConsegnare o QuantitaDaEvadere
  * Il sistema calcolerà automaticamente (Quantita - QuantitaEvasa) nella visualizzazione
- Includi sempre la colonna UnitaMisura quando selezioni quantità
- NON usare alias come 'QuantitaDaConsegnare', 'QuantitaDaEvadere', 'Rimanente' - usa solo Quantita e QuantitaEvasa

ESEMPIO CORRETTO PER CONSEGNE:
SELECT DISTINCT
    CodiceCliente,
    RagioneSociale,
    NumeroOrdine,
    Quantita,           -- NON 'QuantitaOrdinata'
    QuantitaEvasa,      -- NON 'QuantitaConsegnata'
    UnitaMisura,        -- SEMPRE inclusa
    DataConsegna
FROM OrdiniRighe
WHERE TipoOrdine = 'R' AND DataConsegna < GETDATE()

ESEMPI DI FORMATTAZIONE IMPORTI:
- ROUND(CAST(ValoreRiga AS DECIMAL(18,2)), 2) AS Importo
- ROUND(CAST(SUM(ValoreRiga) AS DECIMAL(18,2)), 2) AS Fatturato
- ROUND(CAST(Prezzo AS DECIMAL(18,2)), 2) AS Prezzo

CONTESTO DATABASE:
" + databaseContext;

                var userPrompt = $"Domanda utente: {userQuestion}\n\nGenera la query SQL per rispondere a questa domanda.";

                // Prepara la richiesta
                var requestBody = new
                {
                    model = model,
                    messages = new[]
                    {
                        new { role = "system", content = systemPrompt },
                        new { role = "user", content = userPrompt }
                    },
                    temperature = 0.1, // Bassa temperatura per risposte più deterministiche
                    max_tokens = 1000
                };

                var jsonContent = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

                _logger.LogInformation("Invio richiesta a Mistral AI per domanda: {Question}", userQuestion);

                var response = await _httpClient.PostAsync(MISTRAL_API_URL, content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Errore Mistral AI: {StatusCode} - {Response}", response.StatusCode, responseContent);
                    throw new HttpRequestException($"Errore Mistral AI: {response.StatusCode}");
                }

                // Parse della risposta
                var responseJson = JsonDocument.Parse(responseContent);
                var sqlQuery = responseJson.RootElement
                    .GetProperty("choices")[0]
                    .GetProperty("message")
                    .GetProperty("content")
                    .GetString() ?? throw new InvalidOperationException("Risposta AI vuota");

                // Pulisci la query (rimuovi markdown se presente)
                sqlQuery = CleanSQLQuery(sqlQuery);

                _logger.LogInformation("Query SQL generata: {Query}", sqlQuery);

                return sqlQuery;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante la generazione della query SQL");
                throw;
            }
        }

        private string CleanSQLQuery(string sqlQuery)
        {
            // Rimuovi blocchi markdown ```sql ... ```
            sqlQuery = sqlQuery.Trim();
            
            if (sqlQuery.StartsWith("```sql", StringComparison.OrdinalIgnoreCase))
            {
                sqlQuery = sqlQuery.Substring(6);
            }
            else if (sqlQuery.StartsWith("```", StringComparison.OrdinalIgnoreCase))
            {
                sqlQuery = sqlQuery.Substring(3);
            }

            if (sqlQuery.EndsWith("```"))
            {
                sqlQuery = sqlQuery.Substring(0, sqlQuery.Length - 3);
            }

            return sqlQuery.Trim();
        }

        /// <summary>
        /// Valida che la query SQL non contenga comandi pericolosi
        /// </summary>
        public static bool IsSafeQuery(string sqlQuery)
        {
            var dangerousKeywords = new[]
            {
                "DROP", "DELETE", "UPDATE", "INSERT", "ALTER", "TRUNCATE",
                "EXEC", "EXECUTE", "xp_", "sp_", "CREATE", "GRANT", "REVOKE"
            };

            var upperQuery = sqlQuery.ToUpperInvariant();

            foreach (var keyword in dangerousKeywords)
            {
                if (upperQuery.Contains(keyword))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Metodo generico per ottenere completion da Mistral AI (compatibilità con AIQueryController)
        /// </summary>
        public async Task<string> GetCompletionAsync(object[] messages)
        {
            try
            {
                var apiKey = _configuration["MistralAI:ApiKey"];
                if (string.IsNullOrEmpty(apiKey))
                {
                    throw new InvalidOperationException("Mistral AI API Key non configurata");
                }

                var model = _configuration["MistralAI:Model"] ?? "mistral-large-latest";

                var requestBody = new
                {
                    model = model,
                    messages = messages,
                    temperature = 0.1,
                    max_tokens = 2000
                };

                var jsonContent = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

                var response = await _httpClient.PostAsync(MISTRAL_API_URL, content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Errore Mistral AI: {StatusCode} - {Response}", response.StatusCode, responseContent);
                    throw new HttpRequestException($"Errore Mistral AI: {response.StatusCode}");
                }

                var responseJson = JsonDocument.Parse(responseContent);
                var completion = responseJson.RootElement
                    .GetProperty("choices")[0]
                    .GetProperty("message")
                    .GetProperty("content")
                    .GetString() ?? "";

                return completion;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante GetCompletionAsync");
                throw;
            }
        }

        /// <summary>
        /// Metodo placeholder per analisi documenti (compatibilità con AISettingsController)
        /// </summary>
        public Task<DocumentAnalysisResult?> AnalyzeDocumentAsync(string content, string filename, string fileType)
        {
            // TODO: Implementare analisi documento se necessario
            _logger.LogWarning("AnalyzeDocumentAsync chiamato ma non implementato: {Filename}", filename);
            return Task.FromResult(new DocumentAnalysisResult 
            { 
                Categoria = "Test",
                DescrizioneCategoria = "Test",
                Descrizione = "Funzionalità non implementata",
                Tags = new List<string>(),
                Confidenziale = false
            })!;
        }

        /// <summary>
        /// Placeholder per recuperare i log di Mistral (compatibilità)
        /// </summary>
        public List<string> GetMistralLogs()
        {
            return new List<string> { "Nessun log disponibile" };
        }
    }
}
