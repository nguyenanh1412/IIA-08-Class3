using Microsoft.AspNetCore.SignalR;

namespace IIA_01.HubSignalR
{
    public class LogHub : Hub
    {
        public override Task OnDisconnectedAsync(Exception exception)
        {
            Console.WriteLine($"Client disconnected: {Context.ConnectionId}");
            return base.OnDisconnectedAsync(exception);
        }
    }
}
