using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using ServerCore.DataModel;
using ServerCore.Helpers;
using ServerCore.ModelBases;

namespace ServerCore.Pages.Components
{
    public partial class PlayerClassSelectComponent
    {
        public int SelectedPlayerClassID { get; set; }
        public List<PlayerClass> AllPlayerClasses { get; set; }
        public List<PlayerClass> AvailablePlayerClasses { get; set; }

        [Parameter]
        public int UserId { get; set; }

        public TeamMembers CurrentTeamMember { get; set; }

        [Parameter]
        public EventRole CurrentUserEventRole { get; set; }

        [Parameter]
        public int EventId { get; set; }
        Event Event { get; set; }

        [Parameter]
        public bool IsTempClass { get; set; }

        public const int NoClassSetValue = PlayerClassHelper.NoClassSetValue;

        PuzzleServerContext context
        {
            get
            {
                return Service;
            }
        }

        protected override async Task OnParametersSetAsync()
        {
            Event = await context.Events.FindAsync(EventId);
            CurrentTeamMember = await UserEventHelper.GetTeamMemberForPlayer(context, Event, UserId);
            AllPlayerClasses = await PlayerClassHelper.GetAllPlayerClassesSorted(context, EventId);

            // If a player doesn't have a class in the DB, then set the bound value to the ID for "no class selected"
            if (CurrentTeamMember.Class == null)
            {
                SelectedPlayerClassID = NoClassSetValue;
            }

            if (IsTempClass)
            {
                if (CurrentTeamMember.TemporaryClass != null)
                {
                    SelectedPlayerClassID = CurrentTeamMember.TemporaryClass.ID;
                }

                AvailablePlayerClasses = AllPlayerClasses;
            }
            else
            {
                SelectedPlayerClassID = CurrentTeamMember.Class?.ID ?? NoClassSetValue;

                // This will get unassigned classes for a player or all classes for an admin
                AvailablePlayerClasses = await PlayerClassHelper.GetAvailablePlayerClassesSorted(context, EventId, CurrentUserEventRole, CurrentTeamMember.Team.ID);
            }

            await base.OnParametersSetAsync();
        }

        private async Task UpdatePlayerClassID()
        {
            // Update the database with the new PlayerClass, if it's "Unassigned" then set it to null
            await PlayerClassHelper.SetPlayerClass(context, Event, CurrentUserEventRole, CurrentTeamMember, SelectedPlayerClassID, IsTempClass);

            if (IsTempClass)
            {
                AvailablePlayerClasses = await PlayerClassHelper.GetAllPlayerClassesSorted(context, EventId);
            }
            else
            {
                // Get the new list of unassigned PlayerClasses
                AvailablePlayerClasses = await PlayerClassHelper.GetAvailablePlayerClassesSorted(context, EventId, CurrentUserEventRole, CurrentTeamMember.Team.ID);
            }
        }
    }
}
