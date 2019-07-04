using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ServerCore.DataModel;
using ServerCore.ModelBases;

namespace ServerCore.Helpers
{
    /// <summary>
    /// Utilities for working with teams
    /// </summary>
    static class TeamHelper
    {
        /// <summary>
        /// Helper for deleting teams that correctly deletes dependent objects
        /// </summary>
        /// <param name="team">Team to delete</param>
        public static async Task DeleteTeamAsync(PuzzleServerContext context, Team team)
        {
            var memberEmails = await (from teamMember in context.TeamMembers
                                      where teamMember.Team == team
                                      select teamMember.Member.Email).ToListAsync();
            var applicationEmails = await (from app in context.TeamApplications
                                      where app.Team == team
                                      select app.Player.Email).ToListAsync();

            var puzzleStates = from puzzleState in context.PuzzleStatePerTeam
                               where puzzleState.TeamID == team.ID
                               select puzzleState;
            context.PuzzleStatePerTeam.RemoveRange(puzzleStates);

            var hintStates = from hintState in context.HintStatePerTeam
                             where hintState.TeamID == team.ID
                             select hintState;
            context.HintStatePerTeam.RemoveRange(hintStates);

            var submissions = from submission in context.Submissions
                              where submission.Team == team
                              select submission;
            context.Submissions.RemoveRange(submissions);

            var annotations = from annotation in context.Annotations
                              where annotation.Team == team
                              select annotation;
            context.Annotations.RemoveRange(annotations);

            context.Teams.Remove(team);

            await context.SaveChangesAsync();

            MailHelper.Singleton.SendPlaintextBcc(memberEmails.Union(applicationEmails),
                $"{team.Event.Name}: Team '{team.Name}' has been deleted",
                $"Sorry! You can apply to another team if you wish, or create your own.");
        }

        /// <summary>
        /// Adds a user to a team after performing a number of checks to make sure that the change is valid
        /// </summary>
        /// <param name="context">The context to update</param>
        /// <param name="Event">The event that the team is for</param>
        /// <param name="EventRole">The event role of the user that is making this change</param>
        /// <param name="teamId">The id of the team the player should be added to</param>
        /// <param name="userId">The user that should be added to the team</param>
        /// <returns>
        /// A tuple where the first element is a boolean that indicates whether the player was successfully
        /// added to the team and the second element is a message to display that explains the error in the
        /// case where the user was not successfully added to the team
        /// </returns>        
        public static async Task<Tuple<bool, string>> AddMemberAsync(PuzzleServerContext context, Event Event, EventRole EventRole, int teamId, int userId)
        {
            Team team = await context.Teams.FirstOrDefaultAsync(m => m.ID == teamId);
            if (team == null)
            {
                return new Tuple<bool, string>(false, $"Could not find team with ID '{teamId}'. Check to make sure the team hasn't been removed.");
            }

            var currentTeamMembers = await context.TeamMembers.Where(members => members.Team.ID == team.ID).ToListAsync();
            if (currentTeamMembers.Count >= Event.MaxTeamSize && EventRole != EventRole.admin)
            {
                return new Tuple<bool, string>(false, $"The team '{team.Name}' is full.");
            }

            PuzzleUser user = await context.PuzzleUsers.FirstOrDefaultAsync(m => m.ID == userId);
            if (user == null)
            {
                return new Tuple<bool, string>(false, $"Could not find user with ID '{userId}'. Check to make sure the user hasn't been removed.");
            }

            if (user.EmployeeAlias == null && currentTeamMembers.Where((m) => m.Member.EmployeeAlias == null).Count() >= Event.MaxExternalsPerTeam)
            {
                return new Tuple<bool, string>(false, $"The team '{team.Name}' is already at its maximum count of non-employee players, and '{user.Email}' has no registered alias.");
            }

            if (await (from teamMember in context.TeamMembers
                       where teamMember.Member == user &&
                       teamMember.Team.Event == Event
                       select teamMember).AnyAsync())
            {
                return new Tuple<bool, string>(false, $"'{user.Email}' is already on a team in this event.");
            }

            TeamMembers Member = new TeamMembers();
            Member.Team = team;
            Member.Member = user;

            // Remove any applications the user might have started for this event
            var allApplications = from app in context.TeamApplications
                                  where app.Player == user &&
                                  app.Team.Event == Event
                                  select app;
            context.TeamApplications.RemoveRange(allApplications);

            context.TeamMembers.Add(Member);
            await context.SaveChangesAsync();

            MailHelper.Singleton.SendPlaintextWithoutBcc(new string[] { team.PrimaryContactEmail, user.Email },
                $"{Event.Name}: {user.Name} has now joined {team.Name}!",
                $"Have a great time!");

            var teamCount = await context.TeamMembers.Where(members => members.Team.ID == team.ID).CountAsync();
            if (teamCount >= Event.MaxTeamSize)
            {
                var extraApplications = await (from app in context.TeamApplications
                                        where app.Team == team
                                        select app).ToListAsync();
                context.TeamApplications.RemoveRange(extraApplications);

                var extraApplicationMails = from app in extraApplications
                                            select app.Player.Email;

                MailHelper.Singleton.SendPlaintextBcc(extraApplicationMails.ToList(),
                    $"{Event.Name}: You can no longer join {team.Name} because it is now full",
                    $"Sorry! You can apply to another team if you wish.");
            }

            return new Tuple<bool, string>(true, "");
        }

        /// <summary>
        /// Returns whether or not a user is a microsoft employee that is not an intern (e.g. FTEs and contractors)
        /// </summary>
        /// <returns>True if the user a microsoft employee that is not an intern</returns>
        public static bool IsMicrosoftNonIntern(string email)
        {
            return email.EndsWith("@microsoft.com") && !email.StartsWith("t-");
        }

        public static async Task SetTeamQualificationAsync(
            PuzzleServerContext context,
            Team team,
            bool value)

        {

            team.IsDisqualified = value;
            await context.SaveChangesAsync();
        }
    }
}
