using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using ServerCore.DataModel;
using ServerCore.ServerMessages;

namespace ServerCore.Pages.Components
{
    public class PresenceModel
    {
        public int UserId { get; set; }
        public string Name { get; set; }
        public PresenceType PresenceType { get; set; }
    }

    /// <summary>
    /// Live widget that shows which users are active on a puzzle page
    /// </summary>
    public partial class PresenceComponent : IAsyncDisposable
    {
        public IEnumerable<PresenceModel> PresentUsers { get; set; } = Array.Empty<PresenceModel>();

        [Parameter]
        public int PuzzleUserId { get; set; }

        [Parameter]
        public int TeamId { get; set; }

        [Parameter]
        public int PuzzleId { get; set; }

        [Parameter]
        public bool IsReadOnly { get; set; }

        Guid pageInstance = Guid.NewGuid();
        TeamPuzzleStore teamPuzzleStore;

        private async Task OnPresenceChange(IDictionary<Guid, PresenceModel> presentPages)
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
                    presenceModel.Name = await GetUserNameAsync(user.UserId);
                    presentUsers.Add(presenceModel);
                }

                PresentUsers = presentUsers.OrderBy(presence => presence.PresenceType).ThenBy(presence => presence.Name);
            }
            else
            {
                PresentUsers = Array.Empty<PresenceModel>();
            }

            await InvokeAsync(StateHasChanged);
        }

        /// <summary>
        /// Gets a puzzle user's name
        /// </summary>
        /// <param name="puzzleUserId"></param>
        /// <returns></returns>
        private async Task<string> GetUserNameAsync(int puzzleUserId)
        {
            string userName = await MemoryCache.GetOrCreateAsync<string>(puzzleUserId, async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1);
                string userName = await (from user in PuzzleServerContext.PuzzleUsers
                                         where user.ID == puzzleUserId
                                         select user.Name).SingleAsync();
                if (userName is null)
                {
                    userName = String.Empty;
                }

                entry.SetValue(userName);
                entry.SetSize(userName.Length);
                return userName;
            });

            return userName;
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

            await MessageHub.BroadcastPresenceMessageAsync(new PresenceMessage { PageInstance = pageInstance, PuzzleUserId = PuzzleUserId, TeamId = TeamId, PuzzleId = PuzzleId, PresenceType = PresenceType.Disconnected });
        }
    }
}
