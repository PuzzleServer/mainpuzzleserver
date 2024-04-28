using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using ServerCore.DataModel;
using ServerCore.Helpers;
using ServerCore.ModelBases;

namespace ServerCore.Pages.Teams
{
    public partial class AutoTeam
    {
        [Parameter]
        public Event Event { get; set; }

        [Parameter]
        public int LoggedInUserId { get; set; }

        [Parameter]
        public EventRole EventRole { get; set; }

        [Inject]
        public PuzzleServerContext _context { get; set; }

        [Inject]
        public NavigationManager NavigationManager { get; set; }

        [Required]
        public AutoTeamType? PlayerType { get; set; } = null;

        bool CantCreateTeam { get; set; }

        public async Task OnSubmit()
        {
            CantCreateTeam = false;

            int existingAutoTeam = await (from team in _context.Teams
                                           join tempTeamMember in _context.TeamMembers on team.ID equals tempTeamMember.Team.ID into teamMembers
                                           from teamMember in teamMembers.DefaultIfEmpty()
                                           where team.Event == Event && team.AutoTeamType == PlayerType
                                           group teamMember by team.ID into teamGroup
                                           where teamGroup.Count() < Event.MaxTeamSize
                                           select teamGroup.Key).FirstOrDefaultAsync();

            // Found a team, put the player on it
            if (existingAutoTeam != 0)
            {
                await TeamHelper.AddMemberAsync(_context, Event, EventRole, existingAutoTeam, LoggedInUserId);
            }
            else if (Event.IsTeamRegistrationActive && Event.MaxNumberOfTeams > await _context.Teams.Where(team => team.Event == Event).CountAsync())
            {
                PuzzleUser captain = await (from user in _context.PuzzleUsers
                                             where user.ID == LoggedInUserId
                                             select user).SingleAsync();

                string name = string.Empty;

                do
                {
                    string adjective = adjectives[Random.Shared.Next(adjectives.Length)];
                    string noun = nouns[Random.Shared.Next(nouns.Length)];
                    name = $"{adjective} {noun}";
                } while (TeamHelper.IsTeamNameTaken(_context, Event.ID, name));

                // Create a new auto team
                Team team = new()
                {
                    Event = Event,
                    AutoTeamType = PlayerType.Value,
                    Name = name,
                    PrimaryContactEmail = captain.Email,
                };

                await TeamHelper.CreateTeamAsync(_context, team, Event, LoggedInUserId, EventRole == EventRole.play);
            }
            else
            {
                CantCreateTeam = true;
                return;
            }

            Team newTeam = await UserEventHelper.GetTeamForPlayer(_context, Event, LoggedInUserId);
            NavigationManager.NavigateTo($"/{Event.EventID}/{EventRole}/Teams/{newTeam.ID}/Details", true);
        }

        private string[] adjectives = new string[]
        {
            "Adventurous",
            "Brave",
            "Clever",
            "Daring",
            "Eager",
            "Fearless",
            "Gallant",
            "Heroic",
            "Intrepid",
            "Jolly",
            "Keen",
            "Loyal",
            "Magnificent",
            "Noble",
            "Optimistic",
            "Puzzling",
            "Quick",
            "Resourceful",
            "Stalwart",
            "Trusty",
            "Unafraid",
            "Valiant",
            "Wise",
            "Xenial",
            "Yammering",
            "Zealous"
        };

        private string[] nouns = new string[]
        {
            "Axlotls",
            "Bears",
            "Cats",
            "Dogs",
            "Elephants",
            "Foxes",
            "Giraffes",
            "Horses",
            "Iguanas",
            "Jaguars",
            "Kangaroos",
            "Lions",
            "Mongeese",
            "Narwhals",
            "Owls",
            "Penguins",
            "Quokkas",
            "Rabbits",
            "Sloths",
            "Tigers",
            "Unicorns",
            "Vultures",
            "Wombats",
            "Xerus",
            "Yaks",
            "Zebras"
        };
    }
}
