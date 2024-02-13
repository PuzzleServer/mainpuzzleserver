using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace ServerCore
{
    public class Notification
    {
        public int TeamId { get; set; }
        public string Message { get; set; }
    }

    /// <summary>
    /// Server hub for notifications. Note that the class is necessary even though it doesn't look like it does anything
    /// </summary>
    public class NotificationHub : Hub
    {
    }

    /// <summary>
    /// Extension methods to put strongly typed wrappers around broadcasting messages through the hub
    /// </summary>
    public static class NotificationHubExtensions
    {
        public static async Task BroadcastNotificationAsync(this IHubContext<NotificationHub> hub, Notification notification)
        {
            await hub.Clients.Group("servers").SendAsync("notification", notification);
        }
    }
}
