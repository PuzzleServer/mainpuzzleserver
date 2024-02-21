using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Hosting;

namespace ServerCore
{
    /// <summary>
    /// Singleton that registers listeners for all server messages to distribute them to listening components
    /// </summary>
    public class ServerMessageListener : IDisposable
    {
        HubConnection HubConnection { get; set; }
        IDisposable exampleSubscription = null;
        IDisposable presenceSubscription = null;

        public ServerMessageListener(IHubContext<ServerMessageHub> hub, IWebHostEnvironment env)
        {
            string localhostSignalRUrl;            
            if (env.IsDevelopment())
            {
                localhostSignalRUrl = "http://localhost:44319/serverMessage";
            }
            else
            {
                localhostSignalRUrl = "http://localhost/serverMessage";
            }

            HubConnection = new HubConnectionBuilder().WithUrl(localhostSignalRUrl).WithAutomaticReconnect().Build();

            // Register listeners
            var onExampleMessage = OnExampleMessage;
            exampleSubscription = HubConnection.On(nameof(ExampleMessage), onExampleMessage);
            var onPresenceMessage = OnPresenceMessage;
            presenceSubscription = HubConnection.On(nameof(PresenceMessage), onPresenceMessage);

            TryInitAsync(hub).Wait();
        }

        async Task TryInitAsync(IHubContext<ServerMessageHub> hub)
        {
            for (int i = 0; i < 20; i++)
            {
                try
                {
                    await HubConnection.StartAsync().ConfigureAwait(false);
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

        private async Task OnExampleMessage(ExampleMessage message)
        {
            Debug.WriteLine($"Example message recieved: player {message.PuzzleUserId} team {message.TeamId} someinfo {message.SomeInfo}");
            // Distribute the message to the relevant components for the player/team/everyone
        }

        public event Func<PresenceMessage, Task> OnPresence;
        private async Task OnPresenceMessage(PresenceMessage message)
        {
            await OnPresence?.Invoke(message);
        }

        public void Dispose()
        {
            exampleSubscription?.Dispose();
            exampleSubscription = null;
            presenceSubscription?.Dispose();
            presenceSubscription = null;
        }
    }
}