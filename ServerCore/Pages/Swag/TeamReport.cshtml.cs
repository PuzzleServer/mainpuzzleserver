using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServerCore.DataModel;
using ServerCore.ModelBases;

namespace ServerCore.Pages.Swag
{
    /// <summary>
    /// Model for the admin's view of the swag items for a team
    /// </summary>
    [Authorize(Policy = "IsEventAdmin")]
    public class TeamReportModel : EventSpecificPageModel
    {
        public TeamReportModel(PuzzleServerContext serverContext, UserManager<IdentityUser> userManager) : base(serverContext, userManager)
        {
        }

        /// <summary>
        /// Individual order for a team
        /// </summary>
        public class SwagView
        {
            public string ContactEmail { get; set; }
            public string TeamName { get; set; }
            public string Lunch { get; set; }
            public int TeamId { get; set; }
        }

        /// <summary>
        /// Aggregate orders for an item
        /// </summary>
        public class SummaryItem
        {
            public string Lunch { get; set; }
            public int Count { get; set; }
        }

        public List<SwagView> SwagViews { get; set; }
        public List<SummaryItem> Summary { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            // Fetch lunches the team chose
            SwagViews = await (from teamLunch in _context.TeamLunch
                         join team in _context.Teams on teamLunch.TeamId equals team.ID
                         where team.EventID == Event.ID
                         orderby team.Name
                         select new SwagView()
                         {
                             TeamId = team.ID,
                             ContactEmail = team.PrimaryContactEmail,
                             TeamName = team.Name,
                             Lunch = teamLunch.Lunch
                         }).ToListAsync();

            // Find out how many lunches weren't ordered and should be default:
            // 1. Find out how many got ordered
            var teamLunchCounts = from team in SwagViews
                             group team by team.TeamId into teamGroup
                             select new { TeamId = teamGroup.Key, Count = teamGroup.Count() };

            // 2. Find out how many lunches the team is allowed to order
            var teamEligibleMembers = await (from team in _context.Teams
                                      join member in _context.TeamMembers on team equals member.Team
                                      join player in _context.PlayerInEvent on member.Member equals player.Player
                                      where team.EventID == Event.ID &&
                                      !player.IsRemote
                                      group member by new { member.Team.ID, team.Name, team.PrimaryContactEmail } into teamMembers
                                      select new
                                      {
                                          TeamId = teamMembers.Key.ID,
                                          TeamName = teamMembers.Key.Name,
                                          TeamEmail = teamMembers.Key.PrimaryContactEmail,
                                          Count = teamMembers.Count(),
                                      }).ToListAsync();

            // 3. Find the difference (making sure to include teams that didn't order anything at all and don't have table entries)
            var unclaimedLunches = from eligibleMembers in teamEligibleMembers                
                                   join maybeLunchCount in teamLunchCounts on eligibleMembers.TeamId equals maybeLunchCount.TeamId into lunchCountOrDefault
                                   from lunchCount in lunchCountOrDefault.DefaultIfEmpty()
                                   let eligibleLunches = Math.Ceiling((double)eligibleMembers.Count / (double)Event.PlayersPerLunch)
                                   let orderedLunches = lunchCount?.Count ?? 0
                                   select new { Id = eligibleMembers.TeamId, eligibleMembers.TeamName, eligibleMembers.TeamEmail, UnclaimedLunchCount = eligibleLunches - orderedLunches };

            foreach (var unclaimedLunch in unclaimedLunches)
            {
                // Add a default lunch to the count for each unclaimed lunch
                for (int i = 0; i < unclaimedLunch.UnclaimedLunchCount; i++)
                {
                    SwagViews.Add(new()
                    {
                        TeamId = unclaimedLunch.Id,
                        TeamName = unclaimedLunch.TeamName,
                        ContactEmail = unclaimedLunch.TeamEmail,
                        Lunch = "[Default] " + Event.DefaultLunch
                    });
                }
            }

            SwagViews.Sort((view1, view2) => view1.TeamName.CompareTo(view2.TeamName));

            Summary = (from swag in SwagViews
                       group swag by swag.Lunch into lunchGroup
                       orderby lunchGroup.Key
                       select new SummaryItem()
                       {
                           Lunch = lunchGroup.Key,
                           Count = lunchGroup.Count()
                       }).ToList();

            return Page();
        }
    }
}
