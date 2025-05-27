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
            public AutoTeamType? AutoTeamType { get; set; }
            public string AutoTeamTypeString { get; set; }
            public bool TeamIsRemote { get; set; }
        }

        public IEnumerable<TeamComposition> TeamCompositions { get; set; }

        public int TotalEmployeeCount { get; set; }
        public int TotalInternCount { get; set; }
        public int TotalNonMicrosoftCount { get; set; }
        public int TotalTotal { get; set; }
        public int TotalPossibleEmployeeAliasesCount { get; set; }
        public int TotalRemoteCount { get; set; }

        public int TotalAutoTeamCount { get; set; }
        public int TotalManualTeamCount { get; set; }
        public int TotalAutoPlayerCount { get; set; }
        public int TotalManualPlayerCount { get; set; }
        public int TotalBeginnerPlayerCount { get; set; }
        public int TotalExpertPlayerCount { get; set; }
        public int TotalCasualPlayerCount { get; set; }
        public int TotalSeriousPlayerCount { get; set; }

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
                        AutoTeamType = team.AutoTeamType,
                        TeamMember = teamMember.Member,
                        IsRemoteTeam = team.IsRemoteTeam,
                    })
                .ToList()
                .GroupBy(intermediate => intermediate.TeamID)
                .Select(this.MapToTeamComposition)
                .ToList();

            foreach (TeamComposition t in TeamCompositions)
            {
                TotalTotal += t.Total;
                TotalNonMicrosoftCount += t.NonMicrosoftCount;
                TotalInternCount += t.InternCount;
                TotalEmployeeCount += t.EmployeeCount;
                TotalPossibleEmployeeAliasesCount += t.PossibleEmployeeAliases.Count;
                TotalRemoteCount += t.TeamIsRemote ? 1 : 0;

                if (t.AutoTeamType != null)
                {
                    TotalAutoTeamCount += 1;
                    TotalAutoPlayerCount += t.Total;

                    if ((t.AutoTeamType.Value & AutoTeamType.Beginner) == AutoTeamType.Beginner)
                    {
                        TotalBeginnerPlayerCount += t.Total;
                        t.AutoTeamTypeString = "Beginner";
                    }
                    else if ((t.AutoTeamType.Value & AutoTeamType.Expert) == AutoTeamType.Expert)
                    {
                        TotalExpertPlayerCount += t.Total;
                        t.AutoTeamTypeString = "Expert";
                    }

                    t.AutoTeamTypeString += " | ";

                    if ((t.AutoTeamType.Value & AutoTeamType.Casual) == AutoTeamType.Casual)
                    {
                        TotalCasualPlayerCount += t.Total;
                        t.AutoTeamTypeString += "Casual";
                    }
                    else if ((t.AutoTeamType.Value & AutoTeamType.Serious) == AutoTeamType.Serious)
                    {
                        TotalSeriousPlayerCount += t.Total;
                        t.AutoTeamTypeString += "Serious";
                    }
                }
                else
                {
                    TotalManualTeamCount += 1;
                    TotalManualPlayerCount += t.Total;
                }
            }

            this.SortTeamComposition();
            if (SortDirection == SortDirectionEnum.Descend)
            {
                TeamCompositions = TeamCompositions.Reverse();
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

        private void SortTeamComposition()
        {
            switch (SortBy)
            {
                case SortEnum.EmployeeCount:
                    TeamCompositions = TeamCompositions.OrderBy(teamComp => teamComp.EmployeeCount);
                    break;
                case SortEnum.InternCount:
                    TeamCompositions = TeamCompositions.OrderBy(teamComp => teamComp.InternCount);
                    break;
                case SortEnum.NonMicrosoftAndPossibleAliases:
                    TeamCompositions = TeamCompositions.OrderBy(teamComp => teamComp.NonMicrosoftCount + teamComp.PossibleEmployeeAliases.Count);
                    break;
                case SortEnum.NonMicrosoftCount:
                    TeamCompositions = TeamCompositions.OrderBy(teamComp => teamComp.NonMicrosoftCount);
                    break;
                case SortEnum.PossibleEmployeeAliasas:
                    TeamCompositions = TeamCompositions.OrderBy(teamComp => teamComp.PossibleEmployeeAliases.Count);
                    break;
                case SortEnum.Total:
                    TeamCompositions = TeamCompositions.OrderBy(teamComp => teamComp.Total);
                    break;
                case SortEnum.Title:
                default:
                    TeamCompositions = TeamCompositions.OrderBy(teamComp => teamComp.TeamName);
                    break;
            }
        }

        private TeamComposition MapToTeamComposition(IGrouping<int, IntermediateTeamMember> group)
        {
            TeamComposition toReturn = new TeamComposition(group.Key);

            foreach (IntermediateTeamMember groupMember in group)
            {
                toReturn.TeamName = groupMember.TeamName;
                toReturn.TeamPrimaryContactEmail = groupMember.TeamPrimaryContactEmail;
                toReturn.AutoTeamType = groupMember.AutoTeamType;
                toReturn.TeamIsRemote = groupMember.IsRemoteTeam;
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
            public AutoTeamType? AutoTeamType { get; set; }
            public PuzzleUser TeamMember { get; set; }
            public bool IsRemoteTeam { get; set; }
        }
    }
}