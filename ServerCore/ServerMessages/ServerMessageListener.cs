using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace ServerCore.ServerMessages
{
    /// <summary>
    /// Singleton that registers listeners for all server messages to distribute them to listening components
    /// </summary>
    public class ServerMessageListener : IDisposable
    {
        HubConnection HubConnection { get; set; }
        IHubContext<ServerMessageHub> Hub { get; set; }
        public IServiceProvider ServiceProvider { get; }

        List<IDisposable> subscriptionsToDispose = new List<IDisposable>();

        Task initTracker = null;

        public ServerMessageListener(IHubContext<ServerMessageHub> hub, IWebHostEnvironment env, IServiceProvider serviceProvider)
        {
            Hub = hub;
            ServiceProvider = serviceProvider;

            string localhostSignalRUrl;
            if (env.IsDevelopment())
            {
                localhostSignalRUrl = "http://localhost:44319/serverMessage";
            }
            else
            {
                // Workaround for the Azure server failing to connect to itself via localhost.
                // This means it can connect to a different instance if there are multiple instances,
                // but if there are, the Azure SignalR service will handle the message distribution.
                localhostSignalRUrl = "https://puzzlehunt.azurewebsites.net/serverMessage";
            }

            HubConnection = new HubConnectionBuilder().WithUrl(localhostSignalRUrl).WithAutomaticReconnect().Build();

            // Register listeners
            var onPresenceMessage = OnPresenceMessageAsync;
            subscriptionsToDispose.Add(HubConnection.On(nameof(PresenceMessage), onPresenceMessage));
            var onGetPresenceState = OnGetPresenceState;
            subscriptionsToDispose.Add(HubConnection.On(nameof(GetPresenceState), onGetPresenceState));
            var onAllPresenceState = OnAllPresenceState;
            subscriptionsToDispose.Add(HubConnection.On(nameof(AllPresenceState), onAllPresenceState));
            var onNotificationMessage = OnNotificationMessageAsync;
            subscriptionsToDispose.Add(HubConnection.On(nameof(Notification), onNotificationMessage));

            // We can't wait for this in a constructor, so defer waiting to EnsureInitializedAsync
            initTracker = TryInitAsync(hub);
            initTracker.ContinueWith(t => GetStateFromOtherServersAsync());
        }

        /// <summary>
        /// Call this from any components that need to wait for the listener to be ready
        /// </summary>
        public async Task EnsureInitializedAsync()
        {
            if (initTracker.IsCompletedSuccessfully)
            {
                return;
            }

            lock (this)
            {
                // If it failed, try again
                if (initTracker.IsFaulted)
                {
                    initTracker = TryInitAsync(Hub);
                }
            }

            await initTracker;
        }

        async Task GetStateFromOtherServersAsync()
        {
            await Hub.BroadcastGetPresenceStateAsync(HubConnection.ConnectionId);
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

        /// <summary>
        /// Fires when any presence message comes in
        /// </summary>
        public event Func<PresenceMessage, Task> OnPresence;

        /// <summary>
        /// Fires when any notification comes in
        /// </summary>
        public event Func<Notification, Task> OnNotification;

        private async Task OnPresenceMessageAsync(PresenceMessage message)
        {
            var onPresence = OnPresence;
            if (onPresence != null)
            {
                await onPresence.Invoke(message);
            }
        }

        private async Task OnGetPresenceState(GetPresenceState requestMessage)
        {
            List<PresenceMessage> allPresence = ServiceProvider.GetRequiredService<PresenceStore>().GetAllPresence();
            await Hub.SendAllPresenceState(requestMessage.Client, allPresence.ToArray());
        }

        private async Task OnAllPresenceState(AllPresenceState allPresenceState)
        {
            await ServiceProvider.GetRequiredService<PresenceStore>().MergePresenceState(allPresenceState.AllPresence);
        }

        private async Task OnNotificationMessageAsync(Notification notification)
        {
            var onNotification = OnNotification;
            if (onNotification != null)
            {
                await onNotification.Invoke(notification);
            }
        }

        public void Dispose()
        {
            foreach(IDisposable subscription in subscriptionsToDispose)
            {
                subscription.Dispose();
            }
            subscriptionsToDispose.Clear();
        }
    }
}