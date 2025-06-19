

using Microsoft.AspNetCore.SignalR;

namespace ClientWebApp.Hubs
{
    public class NotificationHub : Hub
    {
        public async Task SendNotification(string message)
        {
            await Clients.All.SendAsync("ReceiveNotification", message);
        }


    }
}
