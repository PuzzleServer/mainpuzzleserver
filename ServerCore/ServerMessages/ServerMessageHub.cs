using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using ServerCore.DataModel;

namespace ServerCore.ServerMessages
{
    /// <summary>
    /// Server hub for server messages. Note that the class is necessary even though it doesn't look like it does anything
    /// </summary>
    public class ServerMessageHub : Hub
    {
    }

    /// <summary>
    /// Extension methods to put strongly typed wrappers around broadcasting messages through the hub
    /// </summary>
    public static class ServerMessageHubExtensions
    {
        /// <summary>
        /// Group to always send messages to.
        /// Don't send messages to all clients as we don't prevent other clients from connecting, but they can't get added to this group.
        /// </summary>
        const string ServersGroup = "servers";

        public static async Task BroadcastPresenceMessageAsync(this IHubContext<ServerMessageHub> hub, PresenceMessage message)
        {
            await hub.Clients.Groups(ServersGroup).SendAsync(nameof(PresenceMessage), message);
        }

        public static async Task BroadcastGetPresenceStateAsync(this IHubContext<ServerMessageHub> hub, string clientId)
        {
            await hub.Clients.Groups(ServersGroup).SendAsync(nameof(GetPresenceState), new GetPresenceState { Client = clientId });
        }

        public static async Task SendAllPresenceState(this IHubContext<ServerMessageHub> hub, string client, PresenceMessage[] allPresence)
        {
            await hub.Clients.Clients(client).SendAsync(nameof(AllPresenceState), new AllPresenceState { AllPresence = allPresence });
        }

        /// <summary>
        /// Send a notification to all players in an event
        /// </summary>
        /// <param name="eventObj">The event</param>
        /// <param name="title">Notification title</param>
        /// <param name="content">Notification content</param>
        /// <param name="linkUrl">Link for the notification if the player clicks it</param>
        public static async Task SendNotification(this IHubContext<ServerMessageHub> hub, Event eventObj, string title, string content, string linkUrl = null)
        {
            await hub.Clients.Groups(ServersGroup).SendAsync(nameof(Notification), new Notification() { Time = DateTime.UtcNow, EventID = eventObj.ID, Title = title, Content = content, LinkUrl = linkUrl });
        }

        /// <summary>
        /// Send a notification to all players on a team
        /// </summary>
        /// <param name="team">The team</param>
        /// <param name="title">Notification title</param>
        /// <param name="content">Notification content</param>
        /// <param name="linkUrl">Link for the notification if the player clicks it</param>
        public static async Task SendNotification(this IHubContext<ServerMessageHub> hub, Team team, string title, string content, string linkUrl = null)
        {
            await hub.Clients.Groups(ServersGroup).SendAsync(nameof(Notification), new Notification() { Time = DateTime.UtcNow, TeamID = team.ID, Title = title, Content = content, LinkUrl = linkUrl });
        }

        /// <summary>
        /// Send a notification to an individual player
        /// </summary>
        /// <param name="player">The player</param>
        /// <param name="title">Notification title</param>
        /// <param name="content">Notification content</param>
        /// <param name="linkUrl">Link for the notification if the player clicks it</param>
        public static async Task SendNotification(this IHubContext<ServerMessageHub> hub, PlayerInEvent player, string title, string content, string linkUrl = null)
        {
            await hub.Clients.Groups(ServersGroup).SendAsync(nameof(Notification), new Notification() { Time = DateTime.UtcNow, PlayerID = player.ID, Title = title, Content = content, LinkUrl = linkUrl });
        }
    }
}
