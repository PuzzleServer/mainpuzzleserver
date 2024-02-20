using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;

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
        public ConcurrentDictionary<Guid, PresenceModel> PresentPages { get; set; } = new ConcurrentDictionary<Guid, PresenceModel>();
        public IEnumerable<PresenceModel> PresentUsers { get; set; } = Array.Empty<PresenceModel>();
        public Dictionary<int, string> TeamMemberNames { get; set; }

        [Parameter]
        public int PuzzleUserId { get; set; }

        [Parameter]
        public int TeamId { get; set; }

        Guid pageInstance = Guid.NewGuid();

        private async void OnPresenceChange(object sender, PresenceMessage presenceMessage)
        {
            if (presenceMessage.PresenceType == PresenceType.Disconnected)
            {
                PresentPages.Remove(presenceMessage.PageInstance, out _);
            }
            else
            {
                PresentPages[presenceMessage.PageInstance] = new PresenceModel { UserId = presenceMessage.PuzzleUserId, Name = presenceMessage.PageInstance.ToString(), PresenceType = presenceMessage.PresenceType };
            }

            if (PresentPages.Count > 0)
            {
                var deduplicatedUsers = (from model in PresentPages.Values
                                         group model by model.UserId into userGroup
                                         select new { UserId = userGroup.Key, PresenceType = userGroup.Min(user => user.PresenceType) }).ToArray();

                PresentUsers = from user in deduplicatedUsers
                               let name = TeamMemberNames.TryGetValue(user.UserId, out var userName) ? userName : String.Empty
                               orderby user.PresenceType, name
                               select new PresenceModel { Name = name, PresenceType = user.PresenceType, UserId = user.UserId };
            }
            else
            {
                PresentUsers = Array.Empty<PresenceModel>();
            }

            await InvokeAsync(StateHasChanged);
        }

        protected override async Task OnInitializedAsync()
        {
            TeamMemberNames = await (from user in PuzzleServerContext.PuzzleUsers
                                     join teamMember in PuzzleServerContext.TeamMembers on user.ID equals teamMember.Member.ID
                                     where teamMember.Team.ID == TeamId
                                     select new { user.ID, user.Name }).ToDictionaryAsync(user => user.ID, user => user.Name);

            MessageListener.OnPresence += OnPresenceChange;

            await base.OnInitializedAsync();
        }

        protected override async Task OnParametersSetAsync()
        {
            // todo: fill in the rest of the message
            await MessageHub.BroadcastPresenceMessageAsync(new PresenceMessage { PageInstance = pageInstance, PuzzleUserId = PuzzleUserId, PresenceType = PresenceType.Active });
            await base.OnParametersSetAsync();
        }

        public async ValueTask DisposeAsync()
        {
            MessageListener.OnPresence -= OnPresenceChange;

            await MessageHub.BroadcastPresenceMessageAsync(new PresenceMessage { PageInstance = pageInstance, PuzzleUserId = PuzzleUserId, PresenceType = PresenceType.Disconnected });
        }
    }
}
