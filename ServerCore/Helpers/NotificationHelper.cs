using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using ServerCore.DataModel;

/// <summary>
/// Content of a notification, intended to be broadcast to all players in a specific event, all players on a specific team, or a specific player.
/// </summary>
public class Notification
{
    public string ID { get; } = Guid.NewGuid().ToString();

    /// <summary>
    /// The event the notification is for.
    /// </summary>
    public int? EventID { get; set; }

    /// <summary>
    /// The team the notification is for.
    /// </summary>
    public int? TeamID { get; set; }

    /// <summary>
    /// The player the notification is for.
    /// </summary>
    public int? PlayerID { get; set; }

    /// <summary>
    /// The title of the notification.
    /// </summary>
    public string Title { get; set; }

    /// <summary>
    /// The content of the notification.
    /// </summary>
    public string Content { get; set; }

    /// <summary>
    /// The link that should be followed if the notification is clicked.
    /// </summary>
    public string LinkUrl { get; set; }
}

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
    /// <summary>
    /// Singleton for the NotificationHelper object. TODO: Pattern copied from MailHelper; is there any actual reason for this or can it all just be static?
    /// </summary>
    private static NotificationHelper Singleton { get; set; }

    private bool IsDev;
    private Dictionary<int, List<NotificationListener>> listenersByEvent = new Dictionary<int, List<NotificationListener>>();
    private Dictionary<int, List<NotificationListener>> listenersByTeam = new Dictionary<int, List<NotificationListener>>();
    private Dictionary<int, List<NotificationListener>> listenersByPlayer = new Dictionary<int, List<NotificationListener>>();

    /// <summary>
    /// Initialize the notification engine.
    /// </summary>
    /// <param name="configuration">Configuration object for the site (containing SignalR info)</param>
    /// <param name="isDev">Whether this is a local dev deployment</param>
    public static void Initialize(IConfiguration configuration, bool isDev)
    {
        Singleton = new NotificationHelper(configuration, isDev);
    }

    /// <summary>
    /// Private constructor for the singleton.
    /// </summary>
    /// <param name="configuration">Configuration object for the site (containing SignalR info)</param>
    /// <param name="isDev">Whether this is a local dev deployment</param>
    private NotificationHelper(IConfiguration configuration, bool isDev)
    {
        if (!isDev)
        {
            // TODO: get SignalR info out of configuration
        }
        this.IsDev = isDev;
    }

    /// <summary>
    /// Send a notification to all players in an event
    /// </summary>
    /// <param name="eventObj">The event</param>
    /// <param name="title">Notification title</param>
    /// <param name="content">Notification content</param>
    /// <param name="linkUrl">Link for the notification if the player clicks it</param>
    public static void SendNotification(Event eventObj, string title, string content, string linkUrl = null)
    {
        Singleton.SendNotification(new Notification() { EventID = eventObj.ID, Title = title, Content = content, LinkUrl = linkUrl });
    }

    /// <summary>
    /// Send a notification to all players on a team
    /// </summary>
    /// <param name="team">The team</param>
    /// <param name="title">Notification title</param>
    /// <param name="content">Notification content</param>
    /// <param name="linkUrl">Link for the notification if the player clicks it</param>
    public static void SendNotification(Team team, string title, string content, string linkUrl = null)
    {
        Singleton.SendNotification(new Notification() { TeamID = team.ID, Title = title, Content = content, LinkUrl = linkUrl });
    }

    /// <summary>
    /// Send a notification to an individual player
    /// </summary>
    /// <param name="player">The player</param>
    /// <param name="title">Notification title</param>
    /// <param name="content">Notification content</param>
    /// <param name="linkUrl">Link for the notification if the player clicks it</param>
    public static void SendNotification(PlayerInEvent player, string title, string content, string linkUrl = null)
    {
        Singleton.SendNotification(new Notification() { PlayerID = player.ID, Title = title, Content = content, LinkUrl = linkUrl });
    }

    /// <summary>
    /// Notification sender
    /// </summary>
    /// <param name="notification">The notification to send</param>
    private void SendNotification(Notification notification)
    {
        if (this.IsDev)
        {
            this.ReceiveNotification(notification);
        }
        else
        {
            // TODO: RPC this call via SignalR
            this.ReceiveNotification(notification);
        }
    }

    /// <summary>
    /// Notification receiver, called by the sender (locally if isDev, via SignalR if deployed)
    /// </summary>
    /// <param name="notification">The notification received</param>
    public void ReceiveNotification(Notification notification)
    {
        List<NotificationListener> listeners = null;
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
            foreach (var listener in listeners)
            {
                listener.NotificationRecieved(this, args);
            }
        }
    }

    /// <summary>
    /// Register a client for notifications
    /// </summary>
    /// <param name="eventID">The event the client is in</param>
    /// <param name="teamID">The team the client is in</param>
    /// <param name="playerID">The player</param>
    /// <param name="notificationRecieved">The EventHandler that will listen to events for the client</param>
    /// <returns>an IDisposable object. The client must explicitly call Dispose() when the client is disposed.</returns>
    public static IDisposable RegisterForNotifications(int? eventID, int? teamID, int? playerID, EventHandler<NotificationEventArgs> notificationRecieved)
    {
        // helper function for each of the three object types
        void Register(NotificationListener listener, int? id, Dictionary<int, List<NotificationListener>> listenerDictionary)
        {
            if (!id.HasValue) return;

            listenerDictionary.TryGetValue(id.Value, out List<NotificationListener> listeners);
            if (listeners == null)
            {
                listeners = new List<NotificationListener>();
                listenerDictionary.Add(id.Value, listeners);
            }
            listeners.Add(listener);
        }

        // Blazor is turning nulls into -1's for some reason
        if (eventID == -1) { eventID = null; }
        if (teamID == -1) { teamID = null; }
        if (playerID == -1) { playerID = null; }

        NotificationListener listener = new NotificationListener() { EventID = eventID, TeamID = teamID, PlayerID = playerID, NotificationRecieved = notificationRecieved };
        Register(listener, listener.EventID, NotificationHelper.Singleton.listenersByEvent);
        Register(listener, listener.TeamID, NotificationHelper.Singleton.listenersByTeam);
        Register(listener, listener.PlayerID, NotificationHelper.Singleton.listenersByPlayer);

        return listener;
    }
    
    /// <summary>
    /// Unregister from notifications. Called by the NotificationListener's Dispose().
    /// </summary>
    /// <param name="listener"></param>
    private void UnregisterForNotifications(NotificationListener listener)
    {
        if (listener.EventID.HasValue) { this.listenersByEvent[listener.EventID.Value].Remove(listener); }
        if (listener.TeamID.HasValue) { this.listenersByTeam[listener.TeamID.Value].Remove(listener); }
        if (listener.PlayerID.HasValue) { this.listenersByPlayer[listener.PlayerID.Value].Remove(listener); }
    }

    /// <summary>
    /// private NotificationListener object that holds the client's desired event handler and helps unregister when the caller disposes it.
    /// </summary>
    private class NotificationListener : IDisposable
    {
        public int? EventID { get; set; }
        public int? TeamID { get; set; }
        public int? PlayerID { get; set; }
        public EventHandler<NotificationEventArgs> NotificationRecieved { get; set; }

        public void Dispose()
        {
            NotificationHelper.Singleton.UnregisterForNotifications(this);
        }
    }
}
