using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using ServerCore.DataModel;
using ServerCore.Helpers;
using ServerCore.ModelBases;

namespace ServerCore.Pages.Teams
{
    [Authorize(Policy = "IsEventAdminOrPlayerOnTeam")]
    public class LiveEventScheduleModel : EventSpecificPageModel
    {
        public List<LiveEventSchedule> TeamSchedule { get; set; }
        public Team ThisTeam { get; set; }
        public List<LiveEvent> UnscheduledLiveEvents { get; set; }

        public LiveEventScheduleModel(PuzzleServerContext serverContext, UserManager<IdentityUser> manager) : base(serverContext, manager)
        {
        }

        public async Task OnGetAsync()
        {
            ThisTeam = await UserEventHelper.GetTeamForPlayer(_context, Event, LoggedInUser);
            TeamSchedule = LiveEventHelper.GetTeamSchedule(_context, Event, ThisTeam);
            UnscheduledLiveEvents = await LiveEventHelper.GetLiveEventsForEvent(_context, Event, false, true);
        }
    }
}
