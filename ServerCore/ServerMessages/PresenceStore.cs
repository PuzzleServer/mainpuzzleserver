using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using ServerCore.Pages.Components;

namespace ServerCore.ServerMessages
{
    /// <summary>
    /// Stores player presence on team puzzle pages and notifies clients when it changes
    /// </summary>
    public class PresenceStore
    {
        ConcurrentDictionary<int, TeamStore> TeamStores { get; } = new ConcurrentDictionary<int, TeamStore>();

        public PresenceStore(ServerMessageListener messageListener)
        {
            messageListener.OnPresence += OnPresenceChange;
        }

        public TeamPuzzleStore GetOrCreateTeamPuzzleStore(int teamId, int puzzleId)
        {
            TeamStore teamStore = new TeamStore();
            if (!TeamStores.TryAdd(teamId, teamStore))
            {
                teamStore = TeamStores[teamId];
            }

            TeamPuzzleStore teamPuzzleStore = new TeamPuzzleStore();
            if (!teamStore.TeamPuzzleStores.TryAdd(puzzleId, teamPuzzleStore))
            {
                teamPuzzleStore = teamStore.TeamPuzzleStores[puzzleId];
            }

            return teamPuzzleStore;
        }

        private async Task OnPresenceChange(PresenceMessage message)
        {
            TeamPuzzleStore teamPuzzleStore = GetOrCreateTeamPuzzleStore(message.TeamId, message.PuzzleId);

            if (message.PresenceType == PresenceType.Disconnected)
            {
                teamPuzzleStore.PresentPages.TryRemove(message.PageInstance, out _);
            }
            else
            {
                teamPuzzleStore.PresentPages[message.PageInstance] = new PresenceModel { UserId = message.PuzzleUserId, Name = message.PageInstance.ToString(), PresenceType = message.PresenceType };
            }

            await teamPuzzleStore.InvokeTeamPuzzlePresenceChange();
        }

        private class TeamStore
        {
            public ConcurrentDictionary<int, TeamPuzzleStore> TeamPuzzleStores { get; } = new ConcurrentDictionary<int, TeamPuzzleStore>();
        }
    }

    /// <summary>
    /// Has the pages present for a single team puzzle
    /// </summary>
    public class TeamPuzzleStore
    {
        /// <summary>
        /// Event for a person joining or leaving a team puzzle
        /// </summary>
        public event Func<IDictionary<Guid, PresenceModel>, Task> OnTeamPuzzlePresenceChange;

        /// <summary>
        /// The pages present on the team puzzle
        /// </summary>
        public ConcurrentDictionary<Guid, PresenceModel> PresentPages { get; set; } = new ConcurrentDictionary<Guid, PresenceModel>();

        public async Task InvokeTeamPuzzlePresenceChange()
        {
            await OnTeamPuzzlePresenceChange?.Invoke(PresentPages);
        }
    }
}
