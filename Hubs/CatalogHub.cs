using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace AiDbMaster.Hubs
{
    public class CatalogHub : Hub
    {
        private readonly ILogger<CatalogHub> _logger;

        public CatalogHub(ILogger<CatalogHub> logger)
        {
            _logger = logger;
        }

        public override async Task OnConnectedAsync()
        {
            _logger.LogInformation($"Client connesso: {Context.ConnectionId}");
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            if (exception != null)
            {
                _logger.LogWarning(exception, $"Client disconnesso con errore: {Context.ConnectionId}");
            }
            else
            {
                _logger.LogInformation($"Client disconnesso: {Context.ConnectionId}");
            }
            
            await base.OnDisconnectedAsync(exception);
        }
    }
} 