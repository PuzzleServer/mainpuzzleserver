using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Client;

namespace ServerCore
{
    public class NotificationListener : IDisposable
    {
        HubConnection HubConnection { get; set; }
        IDisposable notificationSubscription = null;

        public NotificationListener(IHubContext<NotificationHub> hub)
        {
            // Todo: get the right url for produciton
            HubConnection = new HubConnectionBuilder().WithUrl("http://localhost:44319/notify").WithAutomaticReconnect().Build();

            // Register listeners
            var notification = OnNotification;
            notificationSubscription = HubConnection.On("notification", notification);

            TryInitAsync(hub).Wait();
        }

        async Task TryInitAsync(IHubContext<NotificationHub> hub)
        {
            for (int i = 0; i < 20; i++)
            {
                try
                {
                    await HubConnection.StartAsync();
                    Debug.WriteLine($"Hub connection succeeded on try {i}");

                    await hub.Groups.AddToGroupAsync(HubConnection.ConnectionId, "servers");
                    return;
                }
                catch (Exception e)
                {
                    if (i == 19)
                    {
                        throw;
                    }
                    Debug.WriteLine($"Hub connection failed with exception; retrying {e}");
                    // The hub might not have started yet, delay and retry
                    await Task.Delay(500);
                }
            }

            throw new Exception("Shouldn't get here");
        }

        private void OnNotification(Notification notification)
        {
            Debug.WriteLine($"Notification: team {notification.TeamId} message {notification.Message}");
        }

        public void Dispose()
        {
            notificationSubscription?.Dispose();
            notificationSubscription = null;
        }
    }
}