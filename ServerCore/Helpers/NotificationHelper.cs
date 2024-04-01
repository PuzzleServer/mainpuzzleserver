using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using ServerCore.ServerMessages;

/// <summary>
/// EventArgs for callers that register for events.
/// </summary>
public class NotificationEventArgs : EventArgs
{
    /// <summary>
    /// The notification data
    /// </summary>
    public Notification Notification { get; set; }
}

/// <summary>
/// Helper object for subscribing to notifications and sending notifications.
/// </summary>
public class NotificationHelper
{
    private ConcurrentDictionary<int, ConcurrentDictionary<NotificationListener, byte>> listenersByEvent = new ConcurrentDictionary<int, ConcurrentDictionary<NotificationListener, byte>>();
    private ConcurrentDictionary<int, ConcurrentDictionary<NotificationListener, byte>> listenersByTeam = new ConcurrentDictionary<int, ConcurrentDictionary<NotificationListener, byte>>();
    private ConcurrentDictionary<int, ConcurrentDictionary<NotificationListener, byte>> listenersByPlayer = new ConcurrentDictionary<int, ConcurrentDictionary<NotificationListener, byte>>();

    public NotificationHelper(ServerMessageListener messageListener)
    {
        messageListener.OnNotification += ReceiveNotification;
    }

    /// <summary>
    /// Notification receiver, called by the sender (locally if isDev, via SignalR if deployed)
    /// </summary>
    /// <param name="notification">The notification received</param>
    private Task ReceiveNotification(Notification notification)
    {
        ConcurrentDictionary<NotificationListener, byte> listeners = null;
        if (notification.TeamID.HasValue)
        {
            this.listenersByTeam.TryGetValue(notification.TeamID.Value, out listeners);
        }
        else if (notification.EventID.HasValue)
        {
            this.listenersByEvent.TryGetValue(notification.EventID.Value, out listeners);
        }
        else if (notification.PlayerID.HasValue)
        {
            this.listenersByPlayer.TryGetValue(notification.PlayerID.Value, out listeners);
        }

        if (listeners != null)
        {
            NotificationEventArgs args = new NotificationEventArgs() { Notification = notification };
            foreach (var listener in listeners.Keys)
            {
                listener.NotificationRecieved(this, args);
            }
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// Register a client for notifications
    /// </summary>
    /// <param name="eventID">The event the client is in</param>
    /// <param name="teamID">The team the client is in</param>
    /// <param name="playerID">The player</param>
    /// <param name="notificationRecieved">The EventHandler that will listen to events for the client</param>
    /// <returns>an IDisposable object. The client must explicitly call Dispose() when the client is disposed.</returns>
    public IDisposable RegisterForNotifications(int? eventID, int? teamID, int? playerID, EventHandler<NotificationEventArgs> notificationRecieved)
    {
        // helper function for each of the three object types
        void Register(NotificationListener listener, int? id, ConcurrentDictionary<int, ConcurrentDictionary<NotificationListener, byte>> listenerDictionary)
        {
            if (!id.HasValue) return;

            ConcurrentDictionary<NotificationListener, byte> listeners = listenerDictionary.GetOrAdd(id.Value, (id) =>
            {
                return new ConcurrentDictionary<NotificationListener, byte>();
            });
            listeners[listener] = 0;
        }

        // Blazor is turning nulls into -1's for some reason
        if (eventID == -1) { eventID = null; }
        if (teamID == -1) { teamID = null; }
        if (playerID == -1) { playerID = null; }

        NotificationListener listener = new NotificationListener() { Helper = this, EventID = eventID, TeamID = teamID, PlayerID = playerID, NotificationRecieved = notificationRecieved };
        Register(listener, listener.EventID, this.listenersByEvent);
        Register(listener, listener.TeamID, this.listenersByTeam);
        Register(listener, listener.PlayerID, this.listenersByPlayer);

        return listener;
    }
    
    /// <summary>
    /// Unregister from notifications. Called by the NotificationListener's Dispose().
    /// </summary>
    /// <param name="listener"></param>
    private void UnregisterForNotifications(NotificationListener listener)
    {
        if (listener.EventID.HasValue) { this.listenersByEvent[listener.EventID.Value].Remove(listener, out _); }
        if (listener.TeamID.HasValue) { this.listenersByTeam[listener.TeamID.Value].Remove(listener, out _); }
        if (listener.PlayerID.HasValue) { this.listenersByPlayer[listener.PlayerID.Value].Remove(listener, out _); }
    }

    /// <summary>
    /// private NotificationListener object that holds the client's desired event handler and helps unregister when the caller disposes it.
    /// </summary>
    private class NotificationListener : IDisposable
    {
        public NotificationHelper Helper { get; set; }
        public int? EventID { get; set; }
        public int? TeamID { get; set; }
        public int? PlayerID { get; set; }
        public EventHandler<NotificationEventArgs> NotificationRecieved { get; set; }

        public void Dispose()
        {
            Helper.UnregisterForNotifications(this);
        }

        public override int GetHashCode()
        {
            return (EventID?.GetHashCode() ?? 0) ^ (TeamID?.GetHashCode() ?? 0) ^ (PlayerID?.GetHashCode() ?? 0);
        }
    }
}
