using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServerCore.DataModel;
using ServerCore.Helpers;
using ServerCore.ModelBases;

namespace ServerCore.Pages.Teams
{
    // Includes redirects which requires custom auth - always call the AuthChecks method & check the passed value at the start of any method
    [AllowAnonymous]
    public class DetailsModel : EventSpecificPageModel
    {
        public DetailsModel(PuzzleServerContext context, UserManager<IdentityUser> manager) : base(context, manager)
        {
        }

        public Team Team { get; set; }
        public IList<TeamMembers> Members { get; set; }
        public string Emails { get; set; }
        public IList<Tuple<PuzzleUser, int>> Users { get; set; }

        public int PlayersPerLunch { get; set; }
        /// <summary>
        /// The most lunches any team can have
        /// </summary>
        public int GlobalMaxLunches { get; set; }

        /// <summary>
        /// The most lunches this team could have given the current number of remote members
        /// </summary>
        public int SoftMaxLunches { get; set; }

        /// <summary>
        /// The number of lunches this team can order given the current number of local members
        /// </summary>
        public int EditableLunches { get; set; }

        /// <summary>
        /// Number of team members that can have lunch
        /// </summary>
        public int EligibleForLunch { get; set; }

        public string TeamRoom { get; set; }
        public IList<TeamLunch> Lunches { get; set; }
        public string NewLunch { get; set; }
        public static string[] LunchOptions { get; set; }

        private async Task<(bool passed, IActionResult redirect)> AuthChecks(int teamId)
        {
            // Force the user to log in
            if (LoggedInUser == null)
            {
                return (false, Challenge());
            }

            //Only allow admins OR players who are on the team)
            int playerTeamId = await GetTeamId();
            if (!((EventRole == EventRole.admin && await IsEventAdmin()) || (EventRole == EventRole.play && playerTeamId == teamId)))
            {
                // Redirect players to the relevant page
                if (EventRole == EventRole.play)
                {
                    if (playerTeamId != -1)
                    {
                        return (false, RedirectToPage("./Details", new { teamId = playerTeamId }));
                    }
                    return (false, RedirectToPage("./Apply", new { teamId = teamId }));
                }
                // Everyone else gets an error
                return (false, NotFound("You do not have permission to view the details of this team."));
            }
            return (true, null);
        }

        public async Task<IActionResult> OnGetAsync(int teamId = -1)
        {
            var authResult = await AuthChecks(teamId);
            if (!authResult.passed)
            {
                return authResult.redirect;
            }

            if (teamId == -1)
            {
                return NotFound("Missing team id");
            }

            Team = await _context.Teams.FirstOrDefaultAsync(m => m.ID == teamId);
            if (Team == null)
            {
                return NotFound("No team found with id '" + teamId + "'.");
            }

            // Existing team members
            Members = await _context.TeamMembers.Where(members => members.Team.ID == Team.ID).ToListAsync();
            StringBuilder emailList = new StringBuilder("");
            foreach (TeamMembers Member in Members)
            {
                emailList.Append(Member.Member.Email + "; ");
            }
            Emails = emailList.ToString();

            // Team applicants
            Users = await (from application in _context.TeamApplications
                           where application.Team == Team &&
                           !((from teamMember in _context.TeamMembers
                              where teamMember.Member == application.Player &&
                         teamMember.Team.Event == Event
                              select teamMember).Any())
                           select new Tuple<PuzzleUser, int>(application.Player, application.ID)).ToListAsync();

            // Lunch
            if (Event.EventHasTeamSwag)
            {
                PlayersPerLunch = 1;
                if (Event.PlayersPerLunch != null)
                {
                    PlayersPerLunch = Event.PlayersPerLunch.Value;
                }

                GlobalMaxLunches = (int)Math.Ceiling((double)Event.MaxTeamSize / (double)PlayersPerLunch);
                int totalMembers = Members.Count;
                int remoteMembers = await (from player in _context.PlayerInEvent
                                           join member in _context.TeamMembers on player.PlayerId equals member.Member.ID
                                           where member.Team.ID == Team.ID &&
                                           player.IsRemote
                                           select player).CountAsync();
                EligibleForLunch = totalMembers - remoteMembers;

                EditableLunches = (int)Math.Ceiling((double)EligibleForLunch / (double)PlayersPerLunch);

                Lunches = await (from lunch in _context.TeamLunch
                                 where lunch.TeamId == teamId
                                 orderby lunch.ID
                                 select lunch).ToListAsync();

                double possibleInPersonMembers = Event.MaxTeamSize - remoteMembers;
                SoftMaxLunches = (int)Math.Ceiling(possibleInPersonMembers / (double)PlayersPerLunch);
            }
            LunchOptions = (!string.IsNullOrWhiteSpace(Event.LunchOptions)) ? Event.LunchOptions.Split(";") : Array.Empty<string>();
            for (int i = 0; i < LunchOptions.Length; i++) 
            {
                // Note that the lunch details are not displayed for team lunches
                // These options are also used in Swag\Register.cshtml, Event\Details.cshtml, and Player\Create and Edit.cshtml
                // If the way they're displayed ever changes, this needs to be made to match as well
                LunchOptions[i] = LunchOptions[i].Split(":")[0];
                LunchOptions[i] = LunchOptions[i].Trim().Trim('\"');
            }

            // Get team room
            Room teamRoom = _context.Rooms.Where(r => r.TeamID == teamId).FirstOrDefault();
            if (teamRoom != null)
            {
                TeamRoom = $"{teamRoom.Building}/{teamRoom.Number}({teamRoom.Capacity})";
            }

            return Page();
        }

        public async Task<IActionResult> OnGetRemoveMemberAsync(int teamId, int teamMemberId)
        {
            var authResult = await AuthChecks(teamId);
            if (!authResult.passed)
            {
                return authResult.redirect;
            }

            if (EventRole == EventRole.play && !Event.IsTeamMembershipChangeActive)
            {
                return NotFound("Team membership change is not currently active.");
            }

            TeamMembers member = await _context.TeamMembers.FirstOrDefaultAsync(m => m.ID == teamMemberId && m.Team.ID == teamId);
            if (member == null)
            {
                return NotFound("Could not find team member with ID '" + teamMemberId + "'. They may have already been removed from the team.");
            }

            int teamCount = await (from count in _context.TeamMembers
                                   where count.Team.ID == teamId
                                   select count).CountAsync();
            Team memberTeam = await (from team in _context.Teams where team.ID == teamId select team).FirstOrDefaultAsync();

            if (EventRole == EventRole.play)
            {
                if (memberTeam.AutoTeamType.HasValue && member.Member != LoggedInUser)
                {
                    return NotFound("On a 'choose a team for me' team, you cannot remove other players. Ask them to remove themselves.");
                }

                if (teamCount == 1)
                {
                    if (!memberTeam.AutoTeamType.HasValue)
                    {
                        return NotFound("Cannot remove the last member of a team. Delete the team instead.");
                    }
                    else
                    {
                        // removing the last member of an auto team can remove the team
                        await TeamHelper.DeleteTeamAsync(_context, memberTeam, false);
                        memberTeam = null;
                    }
                }
            }

            if (memberTeam != null)
            {
                _context.TeamMembers.Remove(member);

                await TeamHelper.OnTeamMemberChange(_context, memberTeam);

                // If the team fell below eligibility for a lunch, remove the most recent one
                if (Event.EventHasTeamSwag && Event.CanChangeLunch && ((Event.PlayersPerLunch ?? 0) != 0))
                {
                    int newLunchesAllowed = (int)Math.Ceiling((double)(teamCount - 1) / (double)Event.PlayersPerLunch.Value);
                    List<TeamLunch> curLunches = await (from lunch in _context.TeamLunch
                                                        where lunch.TeamId == teamId
                                                        orderby lunch.ID descending
                                                        select lunch).ToListAsync();

                    if (newLunchesAllowed < curLunches.Count)
                    {
                        _context.TeamLunch.Remove(curLunches[0]);
                    }
                }
            }

            await _context.SaveChangesAsync();

            if (EventRole == EventRole.play && member.Member == LoggedInUser)
            {
                return RedirectToPage("./Signup");
            }
            return RedirectToPage("./Details", new { teamId = teamId });
        }

        public async Task<IActionResult> OnGetAddMemberAsync(int teamId, int applicationId)
        {
            var authResult = await AuthChecks(teamId);
            if (!authResult.passed)
            {
                return authResult.redirect;
            }

            if (EventRole == EventRole.play && !Event.IsTeamMembershipChangeActive)
            {
                return NotFound("Team membership change is not currently active.");
            }

            TeamApplication application = await (from app in _context.TeamApplications
                                                 where app.ID == applicationId
                                                 select app).FirstOrDefaultAsync();
            if (application == null)
            {
                return NotFound("Could not find application");
            }

            Team team = await _context.Teams.FirstOrDefaultAsync(m => m.ID == teamId);
            if (application.Team != team)
            {
                return Forbid();
            }

            Tuple<bool, string> result = TeamHelper.AddMemberAsync(_context, Event, EventRole, teamId, application.Player.ID).Result;
            if (result.Item1)
            {
                return RedirectToPage("./Details", new { teamId = teamId });
            }
            return NotFound(result.Item2);
        }

        public async Task<IActionResult> OnGetRejectMemberAsync(int teamId, int applicationId)
        {
            var authResult = await AuthChecks(teamId);
            if (!authResult.passed)
            {
                return authResult.redirect;
            }

            TeamApplication application = await (from app in _context.TeamApplications
                                                 where app.ID == applicationId
                                                 select app).FirstOrDefaultAsync();
            if (application == null)
            {
                return NotFound("Could not find application");
            }

            Team team = await _context.Teams.FirstOrDefaultAsync(m => m.ID == teamId);
            if (application.Team != team)
            {
                return Forbid();
            }

            var allApplications = from app in _context.TeamApplications
                                  where app.Player == application.Player &&
                                  app.Team.Event == Event
                                  select app;
            _context.TeamApplications.RemoveRange(allApplications);
            await _context.SaveChangesAsync();

            MailHelper.Singleton.SendPlaintextWithoutBcc(new string[] { application.Player.Email },
                $"{Event.Name}: Your application to {team.Name} was not approved",
                $"Sorry! You can apply to another team if you wish.");

            return RedirectToPage("./Details", new { teamId = teamId });
        }

        public async Task<IActionResult> OnPostAddLunchAsync(int teamId, string newLunch)
        {
            var authResult = await AuthChecks(teamId);
            if (!authResult.passed)
            {
                return authResult.redirect;
            }

            if (!String.IsNullOrWhiteSpace(newLunch))
            {
                TeamLunch teamLunch = new() { Lunch = newLunch, TeamId = teamId };
                _context.TeamLunch.Add(teamLunch);
                await _context.SaveChangesAsync();
            }

            return RedirectToPage("./Details", new { teamId = teamId });
        }

        public async Task<IActionResult> OnGetRemoveLunchAsync(int teamId, int lunchId)
        {
            var authResult = await AuthChecks(teamId);
            if (!authResult.passed)
            {
                return authResult.redirect;
            }

            TeamLunch teamLunchToRemove = await (from teamLunch in _context.TeamLunch
                                          where teamLunch.ID == lunchId &&
                                          teamLunch.TeamId == teamId // the teamId check isn't necessary for finding the row, but prevents removing other teams' lunches
                                          select teamLunch).SingleOrDefaultAsync();

            if (teamLunchToRemove != null)
            {
                _context.TeamLunch.Remove(teamLunchToRemove);
                await _context.SaveChangesAsync();
            }

            return RedirectToPage("./Details", new { teamId = teamId });
        }
    }
}
