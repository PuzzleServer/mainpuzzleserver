using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.JSInterop;
using ServerCore.DataModel;
using ServerCore.Helpers;
using ServerCore.ServerMessages;
using static System.Runtime.InteropServices.JavaScript.JSType;

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


        protected override async Task OnInitializedAsync()
        {
            await MessageListener.EnsureInitializedAsync();
            await base.OnInitializedAsync();
        }

        protected override async Task OnParametersSetAsync()
        {
            TeamStore = PresenceStore.GetOrCreateTeamStore(TeamId);

            foreach (TeamPuzzleStore teamPuzzleStore in TeamStore.TeamPuzzleStores.Values)
            {
                teamPuzzleStore.OnTeamPuzzlePresenceChange += SendPresenceToJSAsync;
                await SendPresenceToJSAsync(teamPuzzleStore.PuzzleId, teamPuzzleStore.PresentPages);
            }

            await base.OnParametersSetAsync();
        }

        private async Task SendPresenceToJSAsync(int puzzleId, IDictionary<Guid, PresenceModel> presentPages)
        {
            List<PresenceModel> presentUsers = new List<PresenceModel>();
            if (presentPages.Count > 0)
            {
                var deduplicatedUsers = from model in presentPages.Values
                                        group model by model.UserId into userGroup
                                        select new { UserId = userGroup.Key, PresenceType = userGroup.Min(user => user.PresenceType) };

                foreach (var user in deduplicatedUsers)
                {
                    PresenceModel presenceModel = new PresenceModel { UserId = user.UserId, PresenceType = user.PresenceType };
                    presenceModel.Name = await UserEventHelper.GetUserNameAsync(PuzzleServerContext, MemoryCache, user.UserId);
                    presentUsers.Add(presenceModel);
                }

                presentUsers = presentUsers
                    .OrderBy(presence => presence.PresenceType)
                    .ThenBy(presence => presence.Name)
                    .ToList();
            }

            string presenceString;
            if (MaxUsers.HasValue && presentUsers.Count > MaxUsers.Value)
            {
                int remainingUsers = presentUsers.Count - MaxUsers.Value + 1;
                string remainingUsersString = $"{remainingUsers}+";
                presenceString = string.Join(" | ", presentUsers.Take(MaxUsers.Value - 1).Select(u => u.Name)) + " | " + remainingUsersString;
            }
            else
            {
                presenceString = string.Join(" | ", presentUsers.Select(u => u.Name));
            }

            await JSRuntime.InvokeVoidAsync("showPresence", puzzleId, presenceString);
        }


        public void Dispose()
        {
            foreach (TeamPuzzleStore teamPuzzleStore in TeamStore.TeamPuzzleStores.Values)
            {
                teamPuzzleStore.OnTeamPuzzlePresenceChange -= SendPresenceToJSAsync;
            }
        }
    }
}
