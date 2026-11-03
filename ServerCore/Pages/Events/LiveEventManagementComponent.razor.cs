using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using ServerCore.DataModel;
using ServerCore.Helpers;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace ServerCore.Pages.Events
{
    public partial class LiveEventManagementComponent
    {
        [Parameter]
        public int EventId { get; set; }

        Event Event { get; set; }

        [Inject]
        PuzzleServerContext PuzzleServerContext { get; set; }

        string ScheduleCSV { get; set; }

        protected override async Task OnParametersSetAsync()
        {
            Event = await PuzzleServerContext.Events.FindAsync(EventId);
            await base.OnParametersSetAsync();
        }

        private async Task RegenerateScheduleAsync(MouseEventArgs _)
        {
            await LiveEventHelper.DeleteLiveEventSchedule(PuzzleServerContext, EventId);
            await LiveEventHelper.GenerateScheduleForLiveEvents(PuzzleServerContext, Event, bigTeamsFirst: true);
        }

        private async Task RegenerateScheduleRandomlyAsync(MouseEventArgs _)
        {
            await LiveEventHelper.DeleteLiveEventSchedule(PuzzleServerContext, EventId);
            await LiveEventHelper.GenerateScheduleForLiveEvents(PuzzleServerContext, Event, bigTeamsFirst: false);
        }

        private async Task RegenerateScheduleWithRotationAsync(MouseEventArgs _)
        {
            await LiveEventHelper.DeleteLiveEventSchedule(PuzzleServerContext, EventId);
            await LiveEventHelper.GenerateScheduleForLiveEvents(PuzzleServerContext, Event, bigTeamsFirst: true, rotateTeams: true);
        }

        private async Task RegenerateScheduleRandomlyWithRotationAsync(MouseEventArgs _)
        {
            await LiveEventHelper.DeleteLiveEventSchedule(PuzzleServerContext, EventId);
            await LiveEventHelper.GenerateScheduleForLiveEvents(PuzzleServerContext, Event, bigTeamsFirst: false, rotateTeams: true);
        }

        private async Task DisplayScheduleCSV(MouseEventArgs _)
        {
            ScheduleCSV = await LiveEventHelper.ExportLiveEventScheduleToCsv(PuzzleServerContext, Event);
        }
    }
}
