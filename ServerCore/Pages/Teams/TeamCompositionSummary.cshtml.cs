using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ServerCore.DataModel;
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

        public enum SortEnum
        {
            NonMicrosoftAndPossibleAliases,
            InternCount,
            EmployeeCount,
            NonMicrosoftCount,
            PossibleEmployeeAliasas,
            Total,
            Title
        }

        public enum SortDirectionEnum
        {
            Descend,
            Ascend
        }

        public TeamCompositionSummaryModel(
            PuzzleServerContext serverContext, 
            UserManager<IdentityUser> userManager) 
            : base(serverContext, userManager)
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
            public string TeamPrimaryContactEmail { get; set; }
            public int EmployeeCount { get; set; }
            public int InternCount { get; set; }
            public int NonMicrosoftCount { get; set; }
            public int Total { get; set; }
            public List<string> PossibleEmployeeAliases { get; set; }
        }

        public IEnumerable<TeamComposition> TeamCompositions { get; set; }

        public SortEnum SortBy { get; set; }

        public SortDirectionEnum SortDirection { get; set; }

        public IActionResult OnGet(SortEnum? sortBy, SortDirectionEnum? sortDirection)
        {
            SortBy = sortBy ?? SortEnum.NonMicrosoftAndPossibleAliases;
            SortDirection = sortDirection ?? SortDirectionEnum.Descend;

            TeamCompositions = _context.Teams.Where(team => team.EventID == Event.ID)
                .Join(
                    _context.TeamMembers,
                    team => team.ID,
                    teamMember => teamMember.Team.ID,
                    (team, teamMember) => new IntermediateTeamMember()
                    {
                        TeamID = team.ID,
                        TeamName = team.Name,
                        TeamPrimaryContactEmail = team.PrimaryContactEmail,
                        TeamMember = teamMember.Member
                    })
                .ToList()
                .GroupBy(intermediate => intermediate.TeamID)
                .Select(this.MapToTeamComposition);

            if (SortDirection == SortDirectionEnum.Ascend)
            {
                TeamCompositions = TeamCompositions.OrderBy(this.GetSortSelector());
            }
            else
            {
                TeamCompositions = TeamCompositions.OrderByDescending(this.GetSortSelector());
            }

            return Page();
        }

        public IActionResult OnPost(SortEnum sortBy, SortDirectionEnum sortDirection)
        {
            return RedirectToPage(
                "./TeamCompositionSummary", 
                new {
                    sortBy = sortBy,
                    sortDirection = sortDirection
                });
        }

        private Func<TeamComposition, string> GetSortSelector()
        {
            switch (SortBy)
            {
                case SortEnum.EmployeeCount:
                    return teamComp => teamComp.EmployeeCount.ToString();
                case SortEnum.InternCount:
                    return teamComp => teamComp.InternCount.ToString();
                case SortEnum.NonMicrosoftAndPossibleAliases:
                    return teamComp => (teamComp.NonMicrosoftCount + teamComp.PossibleEmployeeAliases.Count).ToString();
                case SortEnum.NonMicrosoftCount:
                    return teamComp => teamComp.NonMicrosoftCount.ToString();
                case SortEnum.PossibleEmployeeAliasas:
                    return teamComp => teamComp.PossibleEmployeeAliases.Count.ToString();
                case SortEnum.Total:
                    return teamComp => teamComp.Total.ToString();
                case SortEnum.Title:
                default:
                    return teamComp => teamComp.TeamName;
            }
        }

        private TeamComposition MapToTeamComposition(IGrouping<int, IntermediateTeamMember> group)
        {
            TeamComposition toReturn = new TeamComposition(group.Key);

            foreach (IntermediateTeamMember groupMember in group)
            {
                toReturn.TeamName = groupMember.TeamName;
                toReturn.TeamPrimaryContactEmail = groupMember.TeamPrimaryContactEmail;
                toReturn.Total += 1;

                string email = groupMember.TeamMember.Email;
                string alias = groupMember.TeamMember.EmployeeAlias;
                if (email != null && email.EndsWith("@microsoft.com"))
                {
                    if (email.StartsWith("t-"))
                    {
                        toReturn.InternCount += 1;
                    }
                    else
                    {
                        toReturn.EmployeeCount += 1;
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
            public string TeamPrimaryContactEmail { get; set; }
            public PuzzleUser TeamMember { get; set; }
        }
    }
}