using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AiDbMaster.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Linq;
using Newtonsoft.Json;
using JsonException = System.Text.Json.JsonException;

namespace AiDbMaster.Services
{
    public class MistralAIService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<MistralAIService> _logger;
        private readonly string _apiKey;
        private readonly string _apiEndpoint;
        private readonly string _modelName;
        private readonly string _logFilePath;

        public MistralAIService(
            HttpClient httpClient,
            IConfiguration configuration,
            ILogger<MistralAIService> logger)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _logger = logger;
            
            _apiKey = _configuration["MistralAI:ApiKey"] ?? throw new InvalidOperationException("MistralAI:ApiKey non configurata");
            _apiEndpoint = _configuration["MistralAI:ApiEndpoint"] ?? throw new InvalidOperationException("MistralAI:ApiEndpoint non configurata");
            _modelName = _configuration["MistralAI:ModelName"] ?? throw new InvalidOperationException("MistralAI:ModelName non configurata");
            
            var logsDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs");
            if (!Directory.Exists(logsDirectory))
            {
                Directory.CreateDirectory(logsDirectory);
            }
            _logFilePath = Path.Combine(logsDirectory, "mistral-api-logs.txt");
        }

        /// <summary>
        /// Analizza il contenuto di un documento e suggerisce una categoria e dei tag
        /// </summary>
        public async Task<DocumentAnalysisResult> AnalyzeDocumentAsync(string documentContent, string fileName, string fileType)
        {
            try
            {
                _logger.LogInformation($"Analisi del documento {fileName} con Mistral AI");

                // Limita la dimensione del contenuto del documento per evitare problemi con l'API
                if (documentContent.Length > 10000)
                {
                    documentContent = documentContent.Substring(0, 10000) + "... [testo troncato per dimensioni]";
                }

                // Prepara il prompt per l'analisi del documento
                string prompt = $@"Analizza il seguente documento e suggerisci una categoria appropriata e dei tag rilevanti.
Nome file: {fileName}
Tipo file: {fileType}

Contenuto del documento:
{documentContent}

Rispondi SOLO in formato JSON con la seguente struttura:
{{
    ""categoria"": ""nome della categoria"",
    ""descrizioneCategoria"": ""breve descrizione della categoria"",
    ""tags"": [""tag1"", ""tag2"", ""tag3""],
    ""confidenziale"": true/false,
    ""spiegazione"": ""breve spiegazione della classificazione""
}}";

                // Prepara la richiesta per l'API di Mistral AI
                var requestData = new
                {
                    model = _modelName,
                    messages = new[]
                    {
                        new { role = "system", content = "Sei un assistente specializzato nella classificazione di documenti aziendali. Il tuo compito è analizzare il contenuto dei documenti e suggerire la categoria più appropriata e i tag più rilevanti." },
                        new { role = "user", content = prompt }
                    },
                    temperature = 0.3,
                    max_tokens = 1000
                };

                var requestContent = new StringContent(
                    System.Text.Json.JsonSerializer.Serialize(requestData),
                    Encoding.UTF8,
                    "application/json");

                // Log della richiesta inviata all'API
                var requestLog = $"RICHIESTA a Mistral AI per il documento {fileName}:\n{System.Text.Json.JsonSerializer.Serialize(requestData, new System.Text.Json.JsonSerializerOptions { WriteIndented = true })}";
                LogToFile(requestLog);

                // Aggiungi l'API key all'header
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);

                // Invia la richiesta all'API di Mistral AI
                var response = await _httpClient.PostAsync(_apiEndpoint, requestContent);
                response.EnsureSuccessStatusCode();

                // Leggi la risposta
                var responseContent = await response.Content.ReadAsStringAsync();
                
                // Log della risposta completa dell'API
                var responseLog = $"RISPOSTA da Mistral AI per il documento {fileName}:\n{responseContent}";
                LogToFile(responseLog);
                
                // Estrai il contenuto testuale dalla risposta
                string aiResponse = string.Empty;
                try
                {
                    // Prova a estrarre il contenuto dalla risposta JSON
                    var tempObject = System.Text.Json.JsonSerializer.Deserialize<MistralAIResponse>(responseContent, new System.Text.Json.JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                    
                    if (tempObject?.Choices != null && tempObject.Choices.Count > 0 && 
                        tempObject.Choices[0]?.Message != null)
                    {
                        aiResponse = tempObject.Choices[0].Message.Content ?? string.Empty;
                        LogToFile($"Contenuto estratto dalla risposta JSON: {aiResponse.Substring(0, Math.Min(100, aiResponse.Length))}...");
                    }
                }
                catch (Exception ex)
                {
                    LogToFile($"Errore durante l'estrazione del contenuto dalla risposta JSON: {ex.Message}");
                    // Se fallisce, usa la risposta completa
                    aiResponse = responseContent;
                }
                
                if (string.IsNullOrEmpty(aiResponse))
                {
                    _logger.LogWarning($"Risposta vuota dall'API di Mistral AI per il documento {fileName}");
                    return new DocumentAnalysisResult
                    {
                        Categoria = "Da Catalogare",
                        DescrizioneCategoria = "Documenti in attesa di catalogazione",
                        Tags = new List<string> { "da_catalogare" },
                        Confidenziale = false,
                        Spiegazione = "Nessuna analisi disponibile"
                    };
                }
                
                // Prima prova a estrarre e deserializzare il JSON
                string jsonContent = ExtractJsonFromResponse(aiResponse);
                DocumentAnalysisResult? analysisResult = null;
                
                try
                {
                    // Configura le opzioni di deserializzazione per essere più tolleranti
                    var jsonOptions = new System.Text.Json.JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true,
                        ReadCommentHandling = System.Text.Json.JsonCommentHandling.Skip,
                        AllowTrailingCommas = true
                    };
                    
                    // Prova a deserializzare il JSON
                    try
                    {
                        analysisResult = System.Text.Json.JsonSerializer.Deserialize<DocumentAnalysisResult>(jsonContent, jsonOptions) ?? new DocumentAnalysisResult();
                        if (analysisResult != null && !string.IsNullOrEmpty(analysisResult.Categoria))
                        {
                            LogToFile("Deserializzazione JSON completata con successo");
                        }
                        else
                        {
                            LogToFile("Deserializzazione JSON ha restituito un oggetto non valido");
                            analysisResult = new DocumentAnalysisResult();
                        }
                    }
                    catch (Exception jsonEx)
                    {
                        LogToFile($"Errore durante la deserializzazione JSON: {jsonEx.Message}");
                    }
                    
                    // Se la deserializzazione JSON fallisce, prova a estrarre direttamente dal testo
                    if (analysisResult == null || string.IsNullOrEmpty(analysisResult.Categoria))
                    {
                        LogToFile("Tentativo di estrazione diretta dal testo della risposta");
                        analysisResult = ExtractResultFromText(aiResponse);
                    }
                    
                    // Log del risultato dell'analisi
                    if (analysisResult != null)
                    {
                        var resultLog = $"RISULTATO dell'analisi per il documento {fileName}:\n" +
                            $"Categoria: {analysisResult.Categoria}\n" +
                            $"Descrizione: {analysisResult.DescrizioneCategoria}\n" +
                            $"Confidenziale: {analysisResult.Confidenziale}\n" +
                            $"Tags: {string.Join(", ", analysisResult.Tags)}\n" +
                            $"Spiegazione: {analysisResult.Spiegazione}";
                        LogToFile(resultLog);
                    }
                    
                    // Verifica che il risultato sia valido
                    if (analysisResult == null || string.IsNullOrEmpty(analysisResult.Categoria))
                    {
                        _logger.LogWarning($"Risultato dell'analisi non valido per il documento {fileName}");
                        return new DocumentAnalysisResult
                        {
                            Categoria = "Da Catalogare",
                            DescrizioneCategoria = "Documenti in attesa di catalogazione",
                            Tags = new List<string> { "da_catalogare" },
                            Confidenziale = false,
                            Spiegazione = "Risultato dell'analisi non valido"
                        };
                    }
                    
                    // Assicurati che Tags non sia null
                    if (analysisResult.Tags == null)
                    {
                        analysisResult.Tags = new List<string> { "da_catalogare" };
                    }
                    
                    return analysisResult;
                }
                catch (JsonException ex)
                {
                    _logger.LogError(ex, $"Errore durante la deserializzazione del JSON per il documento {fileName}");
                    return new DocumentAnalysisResult
                    {
                        Categoria = "Da Catalogare",
                        DescrizioneCategoria = "Documenti in attesa di catalogazione",
                        Tags = new List<string> { "da_catalogare" },
                        Confidenziale = false,
                        Spiegazione = $"Errore durante la deserializzazione del JSON: {ex.Message}"
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Errore durante l'analisi del documento {fileName}");
                return new DocumentAnalysisResult
                {
                    Categoria = "Da Catalogare",
                    DescrizioneCategoria = "Documenti in attesa di catalogazione",
                    Tags = new List<string> { "da_catalogare" },
                    Confidenziale = false,
                    Spiegazione = $"Errore durante l'analisi: {ex.Message}"
                };
            }
        }

        /// <summary>
        /// Estrae il contenuto JSON dalla risposta dell'AI
        /// </summary>
        private string ExtractJsonFromResponse(string response)
        {
            LogToFile($"Risposta completa da elaborare:\n{response}");
            
            try
            {
                // Cerca l'inizio e la fine del JSON nella risposta
                int startIndex = response.IndexOf('{');
                int endIndex = response.LastIndexOf('}');
                
                if (startIndex >= 0 && endIndex >= 0 && endIndex > startIndex)
                {
                    string extractedJson = response.Substring(startIndex, endIndex - startIndex + 1);
                    LogToFile($"JSON estratto con metodo standard: {extractedJson}");
                    return extractedJson;
                }
                
                // Se non trova un JSON valido, cerca un formato alternativo
                // Cerca pattern comuni nella risposta di Mistral
                var jsonPatterns = new[]
                {
                    @"```json\s*({.*?})\s*```",
                    @"```\s*({.*?})\s*```",
                    @"\{[^{}]*""categoria""[^{}]*\}",
                    @"\{[^{}]*""Categoria""[^{}]*\}"
                };
                
                foreach (var pattern in jsonPatterns)
                {
                    var match = System.Text.RegularExpressions.Regex.Match(response, pattern, System.Text.RegularExpressions.RegexOptions.Singleline);
                    if (match.Success)
                    {
                        string extractedJson = match.Groups[1].Value;
                        LogToFile($"JSON estratto con pattern {pattern}: {extractedJson}");
                        return extractedJson;
                    }
                }
                
                // Se ancora non trova un JSON, prova a costruirlo dai dati nella risposta
                LogToFile("Tentativo di costruzione JSON dai dati nella risposta");
                
                // Cerca pattern chiave-valore comuni
                var categoria = ExtractValue(response, "categoria", "categoria:");
                var descrizione = ExtractValue(response, "descrizione", "descrizione categoria:");
                var confidenziale = response.ToLower().Contains("confidenziale: true") || 
                                   response.ToLower().Contains("confidenziale:true") || 
                                   response.ToLower().Contains("documento confidenziale");
                
                // Estrai i tag
                var tagsList = new List<string>();
                var tagMatch = System.Text.RegularExpressions.Regex.Match(response, @"tags:?\s*\[(.*?)\]", System.Text.RegularExpressions.RegexOptions.Singleline);
                if (tagMatch.Success)
                {
                    var tagsString = tagMatch.Groups[1].Value;
                    tagsList = tagsString.Split(',')
                        .Select(t => t.Trim().Trim('"', '\''))
                        .Where(t => !string.IsNullOrEmpty(t))
                        .ToList();
                }
                
                // Costruisci il JSON manualmente
                var manualJson = $@"{{
                    ""categoria"": ""{categoria}"",
                    ""descrizioneCategoria"": ""{descrizione}"",
                    ""confidenziale"": {confidenziale.ToString().ToLower()},
                    ""tags"": [{string.Join(",", tagsList.Select(t => $"\"{t}\""))}],
                    ""spiegazione"": ""Estratto manualmente dalla risposta""
                }}";
                
                LogToFile($"JSON costruito manualmente: {manualJson}");
                return manualJson;
            }
            catch (Exception ex)
            {
                LogToFile($"Errore durante l'estrazione del JSON: {ex.Message}");
                
                // Se non trova un JSON valido, restituisce un JSON di default
                return @"{""categoria"": ""Da Catalogare"", ""descrizioneCategoria"": ""Documenti in attesa di catalogazione"", ""tags"": [""da_catalogare""], ""confidenziale"": false, ""spiegazione"": ""Non è stato possibile estrarre un JSON valido dalla risposta""}";
            }
        }
        
        /// <summary>
        /// Estrae un valore dalla risposta in base a pattern comuni
        /// </summary>
        private string ExtractValue(string response, string defaultValue, params string[] patterns)
        {
            foreach (var pattern in patterns)
            {
                var index = response.ToLower().IndexOf(pattern.ToLower());
                if (index >= 0)
                {
                    // Trova la fine della riga
                    var endOfLine = response.IndexOf('\n', index);
                    if (endOfLine < 0) endOfLine = response.Length;
                    
                    // Estrai il valore dopo il pattern
                    var valueStart = index + pattern.Length;
                    var value = response.Substring(valueStart, endOfLine - valueStart).Trim();
                    
                    // Rimuovi virgolette e altri caratteri non necessari
                    value = value.Trim('"', '\'', ':', ' ', '\r', '\n');
                    
                    if (!string.IsNullOrEmpty(value))
                    {
                        return value;
                    }
                }
            }
            
            return defaultValue;
        }

        // Metodo per scrivere nel file di log
        private void LogToFile(string message)
        {
            try
            {
                var logEntry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}{Environment.NewLine}{Environment.NewLine}";
                File.AppendAllText(_logFilePath, logEntry);
                _logger.LogInformation(message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante la scrittura nel file di log");
            }
        }
        
        /// <summary>
        /// Legge i log delle interazioni con Mistral AI
        /// </summary>
        /// <param name="maxLines">Numero massimo di linee da leggere (0 per tutte)</param>
        /// <returns>Il contenuto del file di log</returns>
        public string GetMistralLogs(int maxLines = 0)
        {
            try
            {
                if (!File.Exists(_logFilePath))
                {
                    return "Nessun log disponibile";
                }
                
                var allLines = File.ReadAllLines(_logFilePath);
                if (maxLines <= 0 || maxLines >= allLines.Length)
                {
                    return File.ReadAllText(_logFilePath);
                }
                
                return string.Join(Environment.NewLine, allLines.Skip(allLines.Length - maxLines));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante la lettura del file di log");
                return $"Errore durante la lettura del file di log: {ex.Message}";
            }
        }

        /// <summary>
        /// Estrae il risultato direttamente dal testo della risposta
        /// </summary>
        private DocumentAnalysisResult ExtractResultFromText(string text)
        {
            LogToFile($"Estrazione diretta dal testo: {text.Substring(0, Math.Min(100, text.Length))}...");
            
            var result = new DocumentAnalysisResult();
            
            // Estrai la categoria
            result.Categoria = ExtractValue(text, "Da Catalogare", 
                "categoria:", "categoria è", "categoria:", "categoria :", 
                "la categoria del documento è", "il documento appartiene alla categoria");
            
            // Estrai la descrizione della categoria
            result.DescrizioneCategoria = ExtractValue(text, "Documenti in attesa di catalogazione", 
                "descrizione categoria:", "descrizione della categoria:", "descrizione:", 
                "la categoria si riferisce a", "questa categoria comprende");
            
            // Estrai la confidenzialità
            result.Confidenziale = text.ToLower().Contains("confidenziale: true") || 
                                  text.ToLower().Contains("confidenziale:true") || 
                                  text.ToLower().Contains("documento confidenziale") ||
                                  text.ToLower().Contains("il documento è confidenziale");
            
            // Estrai i tag
            var tagsList = new List<string>();
            
            // Cerca pattern comuni per i tag
            var tagPatterns = new[]
            {
                @"tags:?\s*\[(.*?)\]",
                @"tag:?\s*\[(.*?)\]",
                @"tags:?\s*(.*?)(?:\n|$)",
                @"tag:?\s*(.*?)(?:\n|$)"
            };
            
            foreach (var pattern in tagPatterns)
            {
                var match = System.Text.RegularExpressions.Regex.Match(text, pattern, System.Text.RegularExpressions.RegexOptions.Singleline);
                if (match.Success)
                {
                    var tagsString = match.Groups[1].Value;
                    tagsList = tagsString.Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries)
                        .Select(t => t.Trim().Trim('"', '\'', '[', ']'))
                        .Where(t => !string.IsNullOrEmpty(t))
                        .ToList();
                    
                    if (tagsList.Count > 0)
                    {
                        break;
                    }
                }
            }
            
            // Se non sono stati trovati tag, estrai parole chiave dal testo
            if (tagsList.Count == 0)
            {
                // Cerca parole dopo "parole chiave", "keywords", ecc.
                var keywordMatch = System.Text.RegularExpressions.Regex.Match(text, @"(?:parole chiave|keywords|tag)(?:[:\s]+)(.*?)(?:\n|$)", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                if (keywordMatch.Success)
                {
                    var keywordsString = keywordMatch.Groups[1].Value;
                    tagsList = keywordsString.Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries)
                        .Select(t => t.Trim().Trim('"', '\''))
                        .Where(t => !string.IsNullOrEmpty(t))
                        .ToList();
                }
            }
            
            // Se ancora non ci sono tag, usa la categoria come tag
            if (tagsList.Count == 0 && !string.IsNullOrEmpty(result.Categoria))
            {
                tagsList.Add(result.Categoria.ToLower().Replace(" ", "_"));
            }
            
            // Se ancora non ci sono tag, usa un tag predefinito
            if (tagsList.Count == 0)
            {
                tagsList.Add("da_catalogare");
            }
            
            result.Tags = tagsList;
            
            // Estrai la spiegazione
            result.Spiegazione = ExtractValue(text, "Estratto direttamente dal testo della risposta", 
                "spiegazione:", "motivazione:", "ragionamento:", "analisi:");
            
            LogToFile($"Risultato estratto dal testo: Categoria={result.Categoria}, " +
                     $"Confidenziale={result.Confidenziale}, " +
                     $"Tags=[{string.Join(", ", result.Tags)}]");
            
            return result;
        }

        public async Task<string> GetCompletionAsync(object[] messages)
        {
            try
            {
                var requestData = new
                {
                    model = _modelName,
                    messages = messages,
                    temperature = 0.3,
                    max_tokens = 1000,
                    stream = false
                };

                var requestContent = new StringContent(
                    System.Text.Json.JsonSerializer.Serialize(requestData),
                    Encoding.UTF8,
                    "application/json");

                // Log della richiesta
                var requestLog = $"RICHIESTA a Mistral AI per generazione query SQL:\n{System.Text.Json.JsonSerializer.Serialize(requestData, new System.Text.Json.JsonSerializerOptions { WriteIndented = true })}";
                LogToFile(requestLog);

                // Aggiungi l'API key all'header
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);

                var response = await _httpClient.PostAsync(_apiEndpoint, requestContent);
                var responseContent = await response.Content.ReadAsStringAsync();
                
                // Log della risposta completa
                var responseLog = $"RISPOSTA da Mistral AI (Status: {response.StatusCode}):\n{responseContent}";
                LogToFile(responseLog);

                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception($"Errore API Mistral: {response.StatusCode} - {responseContent}");
                }

                try
                {
                    var mistralResponse = System.Text.Json.JsonSerializer.Deserialize<MistralAIResponse>(responseContent, new System.Text.Json.JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    LogToFile($"Risposta deserializzata: {System.Text.Json.JsonSerializer.Serialize(mistralResponse, new System.Text.Json.JsonSerializerOptions { WriteIndented = true })}");

                    if (mistralResponse == null)
                    {
                        throw new Exception("Impossibile deserializzare la risposta dell'API");
                    }

                    if (mistralResponse.Choices == null || mistralResponse.Choices.Count == 0)
                    {
                        // Se non ci sono scelte, prova a estrarre il contenuto direttamente dalla risposta
                        var directResponse = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(responseContent);
                        if (directResponse != null && directResponse.ContainsKey("content"))
                        {
                            var directContent = directResponse["content"]?.ToString();
                            if (!string.IsNullOrWhiteSpace(directContent))
                            {
                                return directContent;
                            }
                        }
                        throw new Exception("Risposta non valida dall'API di Mistral AI: nessuna scelta disponibile");
                    }

                    var content = mistralResponse.Choices[0].Message?.Content;
                    if (string.IsNullOrWhiteSpace(content))
                    {
                        throw new Exception("Risposta non valida dall'API di Mistral AI: contenuto vuoto");
                    }

                    return content;
                }
                catch (JsonException ex)
                {
                    LogToFile($"Errore durante la deserializzazione della risposta: {ex.Message}");
                    // Se la deserializzazione fallisce, prova a estrarre il contenuto direttamente
                    var directResponse = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(responseContent);
                    if (directResponse != null && directResponse.ContainsKey("content"))
                    {
                        var directContent = directResponse["content"]?.ToString();
                        if (!string.IsNullOrWhiteSpace(directContent))
                        {
                            return directContent;
                        }
                    }
                    throw new Exception($"Errore durante la deserializzazione della risposta: {ex.Message}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante la generazione della query SQL con Mistral AI");
                throw;
            }
        }
    }

    /// <summary>
    /// Classe per deserializzare la risposta dell'API di Mistral AI
    /// </summary>
    public class MistralAIResponse
    {
        public string Id { get; set; } = "";
        public string Object { get; set; } = "";
        public long Created { get; set; }
        public string Model { get; set; } = "";
        public List<Choice> Choices { get; set; } = new List<Choice>();
        public Usage Usage { get; set; } = new Usage();    
    }

    public class Choice
    {
        public int Index { get; set; }
        public Message Message { get; set; } = new Message();
        public string FinishReason { get; set; } = "";
    }

    public class Message
    {
        public string Role { get; set; } = "";
        public string Content { get; set; } = "";
    }

    public class Usage
    {
        public int PromptTokens { get; set; }
        public int CompletionTokens { get; set; }
        public int TotalTokens { get; set; }
    }

    /// <summary>
    /// Classe per rappresentare il risultato dell'analisi di un documento
    /// </summary>
    public class DocumentAnalysisResult
    {
        public string Categoria { get; set; } = "";
        public string DescrizioneCategoria { get; set; } = "";
        public List<string> Tags { get; set; } = new List<string>();
        public bool Confidenziale { get; set; } = false;
        public string Spiegazione { get; set; } = "";
    }
} 