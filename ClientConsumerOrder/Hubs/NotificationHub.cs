using Microsoft.AspNetCore.SignalR;

namespace ClientConsumerOrder.Hubs
{

    public class NotificationHub : Hub
    {
        private readonly ILogger<NotificationHub> _logger;

        public NotificationHub(ILogger<NotificationHub> logger)
        {
            _logger = logger;
        }

        public async Task SendOrderCreated(string orderId, string userName, decimal totalPrice)
        {
            _logger.LogInformation("📢 [HUB] Notificando orden creada: {OrderId}", orderId);
            await Clients.All.SendAsync("OrderCreated", orderId, userName, totalPrice);
        }

        public async Task SendOrderUpdated(string orderId, string status, decimal newTotal)
        {
            _logger.LogInformation("📢 [HUB] Notificando orden actualizada: {OrderId}", orderId);
            await Clients.All.SendAsync("OrderUpdated", orderId, status, newTotal);
        }

        public async Task SendOrderDeleted(string orderId, string reason)
        {
            _logger.LogInformation("📢 [HUB] Notificando orden eliminada: {OrderId}", orderId);
            await Clients.All.SendAsync("OrderDeleted", orderId, reason);
        }

        public override async Task OnConnectedAsync()
        {
            _logger.LogInformation("🔌 [HUB] Cliente conectado: {ConnectionId}", Context.ConnectionId);
            await Clients.Caller.SendAsync("Connected", Context.ConnectionId);
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            _logger.LogInformation("🔌 [HUB] Cliente desconectado: {ConnectionId}", Context.ConnectionId);
            await base.OnDisconnectedAsync(exception);
        }
    }
}
