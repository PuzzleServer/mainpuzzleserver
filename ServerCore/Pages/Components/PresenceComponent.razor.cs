using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using ServerCore.DataModel;
using ServerCore.Helpers;
using ServerCore.ServerMessages;

namespace ServerCore.Pages.Components
{
    /// <summary>
    /// Live widget that shows which users are active on a puzzle page
    /// </summary>
    public partial class PresenceComponent : IAsyncDisposable
    {
        public List<PresenceModel> PresentUsers { get; set; } = new List<PresenceModel>(0);

        [Parameter]
        public int PuzzleUserId { get; set; }

        [Parameter]
        public int TeamId { get; set; }

        [Parameter]
        public int PuzzleId { get; set; }

        /// <summary>
        /// True if the component will only read the presence and not write.
        /// </summary>
        [Parameter]
        public bool IsReadOnly { get; set; }

        /// <summary>
        /// True if we only want to show the presence and no other UI.
        /// </summary>
        [Parameter]
        public bool ShowPresenceOnly { get; set; }

        /// <summary>
        /// Max number of users to show.
        /// If more than this, then replace with the count (e.g. "3+")
        /// </summary>
        [Parameter]
        public int? MaxUsers { get; set; }

        Guid pageInstance = Guid.NewGuid();
        TeamPuzzleStore teamPuzzleStore;

        private async Task OnPresenceChange(int _, IDictionary<Guid, PresenceModel> presentPages)
        {
            await UpdateModelAsync(presentPages);
        }

        private async Task UpdateModelAsync(IDictionary<Guid, PresenceModel> presentPages)
        {
            if (presentPages.Count > 0)
            {
                var deduplicatedUsers = from model in presentPages.Values
                                         group model by model.UserId into userGroup
                                         select new { UserId = userGroup.Key, PresenceType = userGroup.Min(user => user.PresenceType) };

                List<PresenceModel> presentUsers = new List<PresenceModel>();
                foreach(var user in deduplicatedUsers)
                {
                    PresenceModel presenceModel = new PresenceModel { UserId = user.UserId, PresenceType = user.PresenceType };
                    presenceModel.Name = await UserEventHelper.GetUserNameAsync(PuzzleServerContext, MemoryCache, user.UserId);
                    presentUsers.Add(presenceModel);
                }

                PresentUsers = presentUsers
                    .OrderBy(presence => presence.PresenceType)
                    .ThenBy(presence => presence.Name)
                    .ToList();
            }
            else
            {
                PresentUsers = new List<PresenceModel>(0);
            }

            await InvokeAsync(StateHasChanged);
        }

        protected override async Task OnInitializedAsync()
        {
            await MessageListener.EnsureInitializedAsync();
            await base.OnInitializedAsync();
        }

        protected override async Task OnParametersSetAsync()
        {
            teamPuzzleStore = PresenceStore.GetOrCreateTeamPuzzleStore(TeamId, PuzzleId);
            teamPuzzleStore.OnTeamPuzzlePresenceChange += OnPresenceChange;

            await UpdateModelAsync(teamPuzzleStore.PresentPages);

            if (!this.IsReadOnly)
            {
                await MessageHub.BroadcastPresenceMessageAsync(new PresenceMessage { PageInstance = pageInstance, PuzzleUserId = PuzzleUserId, TeamId = TeamId, PuzzleId = PuzzleId, PresenceType = PresenceType.Active });
            }

            await base.OnParametersSetAsync();
        }

        public async ValueTask DisposeAsync()
        {
            if (teamPuzzleStore is not null)
            {
                teamPuzzleStore.OnTeamPuzzlePresenceChange -= OnPresenceChange;
            }

            if (!this.IsReadOnly)
            {
                await MessageHub.BroadcastPresenceMessageAsync(new PresenceMessage { PageInstance = pageInstance, PuzzleUserId = PuzzleUserId, TeamId = TeamId, PuzzleId = PuzzleId, PresenceType = PresenceType.Disconnected });
            }
        }
    }
}
