using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using ServerCore.DataModel;
using ServerCore.ModelBases;

namespace ServerCore.Pages.Teams
{
    public enum SignupStrategy
    {
        None,
        Create,
        Join,
        Auto
    }

    public partial class SignupHub
    {
        [Parameter]
        public int EventId { get; set; }

        [Parameter]
        public EventRole EventRole { get; set; }

        [Parameter]
        public int LoggedInUserId { get; set; }

        [Parameter]
        public bool IsMicrosoft { get; set; }

        [Inject]
        public PuzzleServerContext _context { get; set; }

        Event Event { get; set; }
        bool CanCreateTeam { get; set; }
        bool CanJoinTeam { get; set; }

        public SignupStrategy Strategy { get; set; } = SignupStrategy.None;

        protected override async Task OnParametersSetAsync()
        {
            Event = await _context.Events.Where(ev => ev.ID == EventId).SingleAsync();
            int teamCount = await _context.Teams.Where(team => team.Event == Event).CountAsync();

            CanCreateTeam = (EventRole == EventRole.admin) ||
                (Event.IsTeamRegistrationActive &&
                teamCount < Event.MaxNumberOfTeams);

            CanJoinTeam = (EventRole == EventRole.admin) ||
                Event.IsTeamMembershipChangeActive;

            await base.OnParametersSetAsync();
        }
    }
}
