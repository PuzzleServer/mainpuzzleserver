using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using ServerCore.Pages.Components;
using ServerCore.ServerMessages;

/// <summary>
/// Helper object for subscribing to notifications and sending notifications.
/// </summary>
public class NotificationHelper
{
    private ConcurrentDictionary<int, NotificationListener> listenersByEvent = new ConcurrentDictionary<int, NotificationListener>();
    private ConcurrentDictionary<int, NotificationListener> listenersByTeam = new ConcurrentDictionary<int, NotificationListener>();
    private ConcurrentDictionary<int, NotificationListener> listenersByUser = new ConcurrentDictionary<int, NotificationListener>();

    public NotificationHelper(ServerMessageListener messageListener)
    {
        messageListener.OnNotification += ReceiveNotification;
    }

    /// <summary>
    /// Notification receiver, called by the sender (locally if isDev, via SignalR if deployed)
    /// </summary>
    /// <param name="notification">The notification received</param>
    private async Task ReceiveNotification(Notification notification)
    {
        NotificationListener listener = null;

        if (notification.TeamID.HasValue)
        {
            this.listenersByTeam.TryGetValue(notification.TeamID.Value, out listener);
        }
        else if (notification.EventID.HasValue)
        {
            this.listenersByEvent.TryGetValue(notification.EventID.Value, out listener);
        }
        else if (notification.UserID.HasValue)
        {
            this.listenersByUser.TryGetValue(notification.UserID.Value, out listener);
        }

        if (listener != null)
        {
            await listener.NotifyAsync(notification);
        }
    }

    /// <summary>
    /// Register a client for notifications
    /// </summary>
    /// <param name="eventID">The event the client is in</param>
    /// <param name="teamID">The team the client is in</param>
    /// <param name="userID">The user</param>
    /// <param name="notificationRecieved">The EventHandler that will listen to events for the client</param>
    public void RegisterForNotifications(int? eventID, int? teamID, int? userID, Func<Notification, Task> onNotify)
    {
        // helper function for each of the three object types
        void Register(int? id, ConcurrentDictionary<int, NotificationListener> listenerDictionary, Func<Notification, Task> onNotify)
        {
            if (!id.HasValue) return;

            NotificationListener listener = listenerDictionary.GetOrAdd(id.Value, (id) =>
            {
                return new NotificationListener();
            });

            listener.OnNotify += onNotify;
        }

        Register(eventID, this.listenersByEvent, onNotify);
        Register(teamID, this.listenersByTeam, onNotify);
        Register(userID, this.listenersByUser, onNotify);
    }

    /// <summary>
    /// Unregister a client from notifications
    /// </summary>
    /// <param name="eventID">The event the client is in</param>
    /// <param name="teamID">The team the client is in</param>
    /// <param name="userID">The user</param>
    /// <param name="notificationRecieved">The EventHandler that will listen to events for the client</param>
    public void UnregisterFromNotifications(int? eventID, int? teamID, int? userID, Func<Notification, Task> onNotify)
    {
        // helper function for each of the three object types
        void Unregister(int? id, ConcurrentDictionary<int, NotificationListener> listenerDictionary, Func<Notification, Task> onNotify)
        {
            if (!id.HasValue) return;

            NotificationListener listener = listenerDictionary.GetOrAdd(id.Value, (id) =>
            {
                return new NotificationListener();
            });

            listenerDictionary[id.Value].OnNotify -= onNotify;
        }

        Unregister(eventID, this.listenersByEvent, onNotify);
        Unregister(teamID, this.listenersByTeam, onNotify);
        Unregister(userID, this.listenersByUser, onNotify);
    }

    private class NotificationListener
    {
        public event Func<Notification, Task> OnNotify;

        public async Task NotifyAsync(Notification notification)
        {
            var onNotify = OnNotify;
            if (onNotify != null)
            {
                await onNotify?.Invoke(notification);
            }
        }
    }
}
