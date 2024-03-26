using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

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
    }
}
