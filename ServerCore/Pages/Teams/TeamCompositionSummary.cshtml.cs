using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServerCore.DataModel;
using ServerCore.Helpers;
using ServerCore.ModelBases;

namespace ServerCore.Pages.Teams
{
    /// <summary>
    /// Model for the author/admin's view of the feedback items for a specific puzzle
    /// /used for viewing and aggregating feedback for a specific puzzle
    /// </summary>
    [Authorize(Policy = "IsEventAdmin")]
    public class TeamCompositionSummaryModel : EventSpecificPageModel
    {
        // AlphaNumeric or dashes only
        public Regex aliasRegex = new Regex("^[a-zA-Z0-9-]*$");

        public TeamCompositionSummaryModel(PuzzleServerContext serverContext, UserManager<IdentityUser> userManager) : base(serverContext, userManager)
        {
        }

        public class TeamComposition
        {
            public TeamComposition(int teamId)
            {
                this.TeamID = teamId;
                this.PossibleEmployeeAliases = new List<string>();
            }

            public int TeamID { get; }
            public string TeamName { get; set; }
            public int MicrosoftNonInternCount { get; set; }
            public int InternCount { get; set; }
            public int NonMicrosoftCount { get; set; }
            public int Total { get; set; }
            public List<string> PossibleEmployeeAliases { get; set; }
        }

        public IEnumerable<TeamComposition> TeamCompositions { get; set; }

        public IActionResult OnGet()
        {
            TeamCompositions = _context.Teams.Where(team => team.EventID == Event.ID)
                .Join(
                    _context.TeamMembers,
                    team => team.ID,
                    teamMember => teamMember.Team.ID,
                    (team, teamMember) => new IntermediateTeamMember() { TeamID = team.ID, TeamName = team.Name, TeamMember = teamMember.Member })
                .GroupBy(intermediate => intermediate.TeamID)
                .Select(this.MapToTeamComposition);

            return Page();
        }

        private TeamComposition MapToTeamComposition(IGrouping<int, IntermediateTeamMember> group)
        {
            TeamComposition toReturn = new TeamComposition(group.Key);

            foreach (IntermediateTeamMember groupMember in group)
            {
                toReturn.TeamName = groupMember.TeamName;
                toReturn.Total += 1;

                string email = groupMember.TeamMember.Email;
                string alias = groupMember.TeamMember.EmployeeAlias;
                if (email.EndsWith("@microsoft.com"))
                {
                    if (email.StartsWith("t-"))
                    {
                        toReturn.InternCount += 1;
                    }
                    else
                    {
                        toReturn.MicrosoftNonInternCount += 1;
                    }
                }
                else if (!string.IsNullOrEmpty(alias) && aliasRegex.IsMatch(alias))
                {
                    toReturn.PossibleEmployeeAliases.Add(alias);
                }
                else
                {
                    toReturn.NonMicrosoftCount += 1;
                }
            }

            return toReturn;
        }

        private class IntermediateTeamMember
        {
            public int TeamID { get; set; }
            public string TeamName { get; set; }
            public PuzzleUser TeamMember { get; set; }
        }
    }
}