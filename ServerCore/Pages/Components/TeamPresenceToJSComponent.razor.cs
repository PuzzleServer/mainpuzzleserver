using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.JSInterop;
using ServerCore.DataModel;
using ServerCore.Helpers;
using ServerCore.ServerMessages;

namespace ServerCore.Pages.Components
{
    /// <summary>
    /// Monitors all presence changes for a team and sends them to JavaScript to display
    /// </summary>
    public partial class TeamPresenceToJSComponent : IDisposable
    {
        [Inject]
        PresenceStore PresenceStore { get; set; }

        [Inject]
        ServerMessageListener MessageListener { get; set; }

        [Inject]
        IJSRuntime JSRuntime { get; set; }

        [Inject]
        PuzzleServerContext PuzzleServerContext { get; set; }

        [Inject]
        IMemoryCache MemoryCache { get; set; }

        /// <summary>
        /// The team to show presence for
        /// </summary>
        [Parameter]
        public int TeamId { get; set; }

        /// <summary>
        /// Max number of users to show
        /// </summary>
        [Parameter]
        public int? MaxUsers { get; set; }

        TeamStore TeamStore { get; set; }

        public class PresenceObject
        {
            public int PuzzleId { get; }
            public string PresenceText { get; }

            public PresenceObject(int puzzleId, string presenceText)
            {
                PuzzleId = puzzleId;
                PresenceText = presenceText;
            }
        }

        protected override async Task OnInitializedAsync()
        {
            await MessageListener.EnsureInitializedAsync();
            await base.OnInitializedAsync();
        }

        protected override async Task OnParametersSetAsync()
        {
            TeamStore = PresenceStore.GetOrCreateTeamStore(TeamId);

            TeamStore.OnTeamPresenceChange += SendPresenceToJSAsync;

            List<PresenceObject> allPresence = new List<PresenceObject>();
            foreach (TeamPuzzleStore teamPuzzleStore in TeamStore.TeamPuzzleStores.Values)
            {
                PresenceObject presence = await GetPresenceObjectAsync(teamPuzzleStore.PuzzleId, teamPuzzleStore.GetPresentUsers());
                if (!string.IsNullOrEmpty(presence.PresenceText))
                {
                    allPresence.Add(presence);
                }
            }

            if (allPresence.Count > 0)
            {
                await JSRuntime.InvokeVoidAsync("showPresence", allPresence);
            }

            await base.OnParametersSetAsync();
        }

        private async Task SendPresenceToJSAsync(int puzzleId, IList<PresenceModel> presentUsers)
        {
            PresenceObject presence = await GetPresenceObjectAsync(puzzleId, presentUsers);
            await JSRuntime.InvokeVoidAsync("showPresence", new List<PresenceObject>() { presence });
        }

        private async Task<PresenceObject> GetPresenceObjectAsync(int puzzleId, IList<PresenceModel> presentUsers)
        {
            List<PresenceModel> namedUsers = new List<PresenceModel>(presentUsers.Count);
            foreach (var user in presentUsers)
            {
                PresenceModel presenceModel = new PresenceModel { UserId = user.UserId, PresenceType = user.PresenceType };
                presenceModel.Name = await UserEventHelper.GetUserNameAsync(PuzzleServerContext, MemoryCache, user.UserId);
                namedUsers.Add(presenceModel);
            }

            namedUsers = namedUsers
                .OrderBy(presence => presence.PresenceType)
                .ThenBy(presence => presence.Name)
                .ToList();

            string presenceString;
            if (MaxUsers.HasValue && namedUsers.Count > MaxUsers.Value)
            {
                int remainingUsers = namedUsers.Count - MaxUsers.Value + 1;
                string remainingUsersString = $"{remainingUsers}+";
                presenceString = string.Join(" | ", namedUsers.Take(MaxUsers.Value - 1).Select(u => u.Name)) + " | " + remainingUsersString;
            }
            else
            {
                presenceString = string.Join(" | ", namedUsers.Select(u => u.Name));
            }

            return new PresenceObject(puzzleId, presenceString);
        }

        public void Dispose()
        {
            TeamStore.OnTeamPresenceChange -= SendPresenceToJSAsync;
        }
    }
}
