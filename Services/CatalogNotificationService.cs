using AiDbMaster.Hubs;
using Microsoft.AspNetCore.SignalR;
using System.Text;

namespace AiDbMaster.Services
{
    public class CatalogNotificationService
    {
        private readonly IHubContext<CatalogHub> _hubContext;
        private readonly ILogger<CatalogNotificationService> _logger;
        private readonly List<string> _logMessages = new List<string>();

        public CatalogNotificationService(
            IHubContext<CatalogHub> hubContext,
            ILogger<CatalogNotificationService> logger)
        {
            _hubContext = hubContext;
            _logger = logger;
        }

        public async Task SendUpdateAsync(string message)
        {
            try
            {
                // Aggiungi il timestamp al messaggio
                string timestampedMessage = $"[{DateTime.Now:HH:mm:ss}] {message}";
                
                // Salva il messaggio nella lista dei log
                _logMessages.Add(timestampedMessage);
                
                // Determina il tipo di messaggio in base al contenuto
                string messageType = "info";
                if (message.Contains("ERRORE") || message.Contains("❌"))
                {
                    messageType = "error";
                    _logger.LogError(message);
                }
                else if (message.Contains("Attenzione") || message.Contains("⚠️"))
                {
                    messageType = "warning";
                    _logger.LogWarning(message);
                }
                else if (message.Contains("completata") || message.Contains("successo") || message.Contains("✅"))
                {
                    messageType = "success";
                    _logger.LogInformation(message);
                }
                else
                {
                    _logger.LogInformation(message);
                }
                
                await _hubContext.Clients.All.SendAsync("ReceiveUpdate", timestampedMessage, messageType);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante l'invio dell'aggiornamento della catalogazione");
            }
        }

        public async Task SendProgressAsync(int processed, int total)
        {
            try
            {
                await _hubContext.Clients.All.SendAsync("ReceiveProgress", processed, total);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante l'invio del progresso della catalogazione");
            }
        }

        public async Task SendCompletedAsync(int processed, int errors)
        {
            try
            {
                // Crea un riepilogo dettagliato
                StringBuilder summary = new StringBuilder();
                summary.AppendLine($"Documenti elaborati: {processed}");
                summary.AppendLine($"Errori riscontrati: {errors}");
                
                if (errors > 0)
                {
                    summary.AppendLine("\nControlla i messaggi di log per i dettagli degli errori.");
                }
                
                if (processed > 0)
                {
                    summary.AppendLine("\nI documenti sono stati catalogati con successo.");
                }
                
                // Invia il riepilogo e il conteggio
                await _hubContext.Clients.All.SendAsync("ReceiveCompleted", processed, errors, summary.ToString());
                
                // Salva il log completo
                string logFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs", $"catalog-log-{DateTime.Now:yyyyMMdd-HHmmss}.txt");
                string? directoryPath = Path.GetDirectoryName(logFilePath);
                if (!string.IsNullOrEmpty(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }
                await File.WriteAllLinesAsync(logFilePath, _logMessages);
                
                // Invia il percorso del file di log
                await _hubContext.Clients.All.SendAsync("ReceiveLogFilePath", logFilePath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore durante l'invio del completamento della catalogazione");
            }
        }
    }
} 