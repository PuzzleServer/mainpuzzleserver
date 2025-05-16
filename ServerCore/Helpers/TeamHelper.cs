using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using ServerCore.DataModel;
using ServerCore.ModelBases;

namespace ServerCore.Helpers
{
    /// <summary>
    /// Utilities for working with teams
    /// </summary>
    static class TeamHelper
    {
        public static async Task CreateTeamAsync(PuzzleServerContext context, Team team, Event ev, int? userId)
        {
            team.Event = ev;
            team.Password = Guid.NewGuid().ToString();

            using (IDbContextTransaction transaction = context.Database.BeginTransaction(System.Data.IsolationLevel.Serializable))
            {
                context.Teams.Add(team);

                if (userId.HasValue)
                {
                    if (await (from member in context.TeamMembers
                               where member.Member.ID == userId &&
                               member.Team.Event == ev
                               select member).AnyAsync())
                    {
                        throw new Exception("You are already on a team. Leave that team before creating a new one");
                    }

                    if (await context.Teams.Where((t) => t.Event == ev).CountAsync() >= ev.MaxNumberOfTeams)
                    {
                        throw new Exception("Registration is full. No further teams may be created at the present time.");
                    }

                    PuzzleUser user = await context.PuzzleUsers.SingleAsync(m => m.ID == userId);

                    if (!(await user.IsRegisteredForEvent(context, ev)) && !(EventHelper.EventRequiresActivePlayerRegistration(ev)))
                    {
                        await EventHelper.RegisterPlayerForEvent(context, ev, user);
                    }

                    TeamMembers teamMember = new TeamMembers()
                    {
                        Team = team,
                        Member = user
                    };

                    if (ev.HasPlayerClasses)
                    {
                        await PlayerClassHelper.AssignRandomPlayerClass(context, teamMember, ev, EventRole.play);
                    }

                    context.TeamMembers.Add(teamMember);
                    await TeamHelper.OnTeamMemberChange(context, team);
                }

                var teamHints = await (from hint in context.Hints
                                       where hint.Puzzle.Event == ev && !hint.Puzzle.IsForSinglePlayer
                                       select hint).ToListAsync();

                foreach (Hint hint in teamHints)
                {
                    context.HintStatePerTeam.Add(new HintStatePerTeam() { Hint = hint, Team = team });
                }

                var teamPuzzleIDs = await (from puzzle in context.Puzzles
                                           where puzzle.Event == ev && !puzzle.IsForSinglePlayer
                                           select puzzle.ID).ToListAsync();
                foreach (int puzzleID in teamPuzzleIDs)
                {
                    context.PuzzleStatePerTeam.Add(new PuzzleStatePerTeam() { PuzzleID = puzzleID, Team = team });
                }

                await context.SaveChangesAsync();
                transaction.Commit();
            }
        }


        /// <summary>
        /// Helper for deleting teams that correctly deletes dependent objects
        /// </summary>
        /// <param name="team">Team to delete</param>
        public static async Task DeleteTeamAsync(PuzzleServerContext context, Team team, bool sendEmail = true)
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

            var liveEventSchedules = from liveEventSchedule in context.LiveEventsSchedule
                                     where liveEventSchedule.Team == team
                                     select liveEventSchedule;
            context.LiveEventsSchedule.RemoveRange(liveEventSchedules);

            IEnumerable<Message> messages = from message in context.Messages
                                            where message.Team == team
                                            select message;
            context.Messages.RemoveRange(messages);

            var rooms = context.Rooms.Where(r => r.TeamID == team.ID).ToList();
            foreach (var r in rooms)
            {
                r.TeamID = null;
                r.Team = null;
            }

            context.Teams.Remove(team);

            await context.SaveChangesAsync();

            if (sendEmail)
            {
                MailHelper.Singleton.SendPlaintextBcc(memberEmails.Union(applicationEmails),
                    $"{team.Event.Name}: Team '{team.Name}' has been deleted",
                    $"Sorry! You can apply to another team if you wish, or create your own.");
            }
        }

        /// <summary>
        /// Performs any housekeeping tasks associated with adding or removing a team member.
        /// </summary>
        /// <param name="context">The context to update</param>
        /// <param name="team">The team whose membership is changing</param>
        /// <returns></returns>
        public static async Task OnTeamMemberChange(PuzzleServerContext context, Team team)
        {
            if (team.AutoTeamType != null)
            {
                await context.SaveChangesAsync();
                var currentTeamMemberNames = await context.TeamMembers.Where(members => members.Team.ID == team.ID).Select(m => m.Member.Email).ToListAsync();
                team.PrimaryContactEmail = string.Join(';', currentTeamMemberNames);
                await context.SaveChangesAsync();
            }
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

            // If event has PlayerClasses: Assign the user a random available PlayerClass
            // This ensures that they have some class when the event starts
            // Better flow for picking their own class TBD
            PlayerClass playerClass = null;

            if (Event.HasPlayerClasses)
            {
                playerClass = await PlayerClassHelper.GetRandomPlayerClassFromAvailable(context, Event.ID, EventRole, teamId);
                Member.Class = playerClass;
            }

            context.TeamMembers.Add(Member);
            await TeamHelper.OnTeamMemberChange(context, team);
            await context.SaveChangesAsync();

            string emailBody = $"Have a great time!";

            if (Event.HasPlayerClasses)
            {
                if (playerClass != null)
                {
                    emailBody = $"This event has assigned a {Event.PlayerClassName} for each player. " +
                        $"You have been assigned the {Event.PlayerClassName} of {playerClass.Name}. " +
                        $"To change this {Event.PlayerClassName} go to Teams -> My Team on the website.\r\n{emailBody}";
                }
                else
                {
                    emailBody = $"This event has assigned a {Event.PlayerClassName} for each player. " +
                        $"Please go to Teams -> My Team on the website to choose your {Event.PlayerClassName}!\r\n{emailBody}";
                }
            }

            MailHelper.Singleton.SendPlaintextWithoutBcc(new string[] { team.PrimaryContactEmail, user.Email },
                $"{Event.Name}: {user.Name} has now joined {team.Name}!",
                emailBody);

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

        /// <summary>
        /// Detects whether another team has claimed this name, modulo capitalization and spaces.
        /// </summary>
        /// <param name="context">The context</param>
        /// <param name="eventId">The event the team name is requested in</param>
        /// <param name="name">The name being requested</param>
        /// <returns>True if the name is in use, false otherwise.</returns>
        public static bool IsTeamNameTaken(
            PuzzleServerContext context,
            int eventId,
            string name)
        {
            string TruncateName(string name)
            {
                return name.Replace(" ", "").ToLowerInvariant();
            }

            string truncatedName = TruncateName(name);

            List<string> existingNames = (from t in context.Teams where t.EventID == eventId select t.Name).ToList();

            foreach (string existingName in existingNames)
            {
                if (truncatedName == TruncateName(existingName))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Sanitizes the incoming team name by removing any non-printable or non-standard space Unicode characters. 
        /// Put another way, the only remaining white space will be the standard space character.
        /// </summary>
        /// <param name="incomingName">The team name before being sanitized</param>
        /// <returns>The incomingName after being sanitized</returns>
        public static string UnicodeSanitizeTeamName(string incomingName)
        {
            StringBuilder newString = new();
            StringRuneEnumerator nameEnumerator = incomingName.EnumerateRunes();
            foreach (Rune rune in nameEnumerator)
            {
                switch (Rune.GetUnicodeCategory(rune))
                {
                    case UnicodeCategory.PrivateUse:
                    case UnicodeCategory.ParagraphSeparator:
                    case UnicodeCategory.OtherNotAssigned:
                    case UnicodeCategory.LineSeparator:
                    case UnicodeCategory.Control:
                        // Disallow all of these characters
                        break;
                    case UnicodeCategory.Format:
                        if (rune.Value != 0x200B && // Disallow zero-width space
                            rune.Value != 0x2062 && // Disallow invisible times
                            rune.Value != 0x2063 && // Disallow invisible separator
                            rune.Value != 0x2064 && // Disallow invisible plus
                            rune.Value != 0xFEFF)
                        { // Disallow zero-width no-break space
                            newString.Append(rune.ToString());
                        }
                        break;
                    case UnicodeCategory.SpaceSeparator:
                        if (rune.Value == 0x20)
                        { // Allow regular spaces
                            newString.Append(rune.ToString());
                        }
                        break;
                    default: // Allow all other characters through
                        newString.Append(rune.ToString());
                        break;
                }
            }
            return newString.ToString();
        }
    }
}
