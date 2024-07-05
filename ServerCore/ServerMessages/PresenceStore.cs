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

        public TeamStore GetOrCreateTeamStore(int teamId)
        {
            TeamStore teamStore = new TeamStore();
            if (!TeamStores.TryAdd(teamId, teamStore))
            {
                teamStore = TeamStores[teamId];
            }

            return teamStore;
        }

        public TeamPuzzleStore GetOrCreateTeamPuzzleStore(int teamId, int puzzleId)
        {
            TeamStore teamStore = GetOrCreateTeamStore(teamId);
            return teamStore.GetOrCreateTeamPuzzleStore(puzzleId);
        }

        /// <summary>
        /// Returns a list of all present users on puzzles
        /// </summary>
        public List<PresenceMessage> GetAllPresence()
        {
            List<PresenceMessage> presenceMessages = new List<PresenceMessage>();
            foreach(KeyValuePair<int, TeamStore> teamStore in TeamStores)
            {
                foreach(KeyValuePair<int, TeamPuzzleStore> teamPuzzleStore in teamStore.Value.TeamPuzzleStores)
                {
                    foreach(KeyValuePair<Guid, PresenceModel> presence in teamPuzzleStore.Value.PresentPages)
                    {
                        presenceMessages.Add(new PresenceMessage
                        {
                            PageInstance = presence.Key,
                            PuzzleUserId = presence.Value.UserId,
                            TeamId = teamStore.Key,
                            PuzzleId = teamPuzzleStore.Key,
                            PresenceType = presence.Value.PresenceType
                        });
                    }
                }
            }

            return presenceMessages;
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

        /// <summary>
        /// Incorporates a new set of presence state into the store
        /// </summary>
        public async Task MergePresenceState(PresenceMessage[] allPresence)
        {
            foreach(PresenceMessage message in allPresence)
            {
                await OnPresenceChange(message);
            }
        }
    }

    /// <summary>
    /// Model for a user's presence on a puzzle page
    /// </summary>
    public class PresenceModel
    {
        public int UserId { get; set; }
        public string Name { get; set; }
        public PresenceType PresenceType { get; set; }
    }

    /// <summary>
    /// Has the pages present for all puzzles on a team
    /// </summary>
    public class TeamStore
    {
        public ConcurrentDictionary<int, TeamPuzzleStore> TeamPuzzleStores { get; } = new ConcurrentDictionary<int, TeamPuzzleStore>();

        /// <summary>
        /// Event for a person joining or leaving any team puzzle. Aggregates all team puzzles.
        /// </summary>
        public event Func<int, IDictionary<Guid, PresenceModel>, Task> OnTeamPresenceChange;

        internal TeamPuzzleStore GetOrCreateTeamPuzzleStore(int puzzleId)
        {
            TeamPuzzleStore teamPuzzleStore = new TeamPuzzleStore(puzzleId);
            if (TeamPuzzleStores.TryAdd(puzzleId, teamPuzzleStore))
            {
                teamPuzzleStore.OnTeamPuzzlePresenceChange += InvokeTeamPresenceChange;
            }
            else
            {
                teamPuzzleStore = TeamPuzzleStores[puzzleId];
            }

            return teamPuzzleStore;
        }

        private async Task InvokeTeamPresenceChange(int puzzleId, IDictionary<Guid, PresenceModel> presentPages)
        {
            var onTeamPuzzlePresenceChange = OnTeamPresenceChange;
            if (onTeamPuzzlePresenceChange != null)
            {
                await onTeamPuzzlePresenceChange?.Invoke(puzzleId, presentPages);
            }
        }
    }

    /// <summary>
    /// Has the pages present for a single team puzzle
    /// </summary>
    public class TeamPuzzleStore
    {
        public TeamPuzzleStore(int puzzleId)
        {
            PuzzleId = puzzleId;
        }

        /// <summary>
        /// Event for a person joining or leaving a team puzzle
        /// </summary>
        public event Func<int, IDictionary<Guid, PresenceModel>, Task> OnTeamPuzzlePresenceChange;

        /// <summary>
        /// The pages present on the team puzzle
        /// </summary>
        public ConcurrentDictionary<Guid, PresenceModel> PresentPages { get; set; } = new ConcurrentDictionary<Guid, PresenceModel>();

        /// <summary>
        /// Puzzle this is a store for
        /// </summary>
        public int PuzzleId { get; }

        public async Task InvokeTeamPuzzlePresenceChange()
        {
            var onTeamPuzzlePresenceChange = OnTeamPuzzlePresenceChange;
            if (onTeamPuzzlePresenceChange != null)
            {
                await onTeamPuzzlePresenceChange?.Invoke(PuzzleId, PresentPages);
            }
        }
    }
}
