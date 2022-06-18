using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ServerCore.DataModel;

namespace ServerCore.Helpers
{
    /// <summary>
    /// Includes methods that construct a list of users to be used in add-user pages.
    /// </summary>
    public static class AddUserHelper
    {
        public static async Task<List<PuzzleUserView>> GetNonAdminsForEvent(PuzzleServerContext puzzleServerContext, Event thisEvent)
        {
            List<int> adminUserIDs = await (from eventAdmin in puzzleServerContext.EventAdmins
                                         where eventAdmin.EventID == thisEvent.ID
                                         orderby eventAdmin.AdminID
                                         select eventAdmin.AdminID).ToListAsync();

            return await GetAllUsersExcept(puzzleServerContext, adminUserIDs);
        }

        public static async Task<List<PuzzleUserView>> GetNonAuthorsForEvent(PuzzleServerContext puzzleServerContext, Event thisEvent)
        {
            List<int> authorUserIDs = await (from eventAuthor in puzzleServerContext.EventAuthors
                                            where eventAuthor.EventID == thisEvent.ID
                                            orderby eventAuthor.AuthorID
                                            select eventAuthor.AuthorID).ToListAsync();

            return await GetAllUsersExcept(puzzleServerContext, authorUserIDs);
        }

        public static async Task<List<PuzzleUserView>> GetNonTeamMembersForTeam(PuzzleServerContext puzzleServerContext, Team team)
        {
            List<int> memberUserIDs = await (from teamMember in puzzleServerContext.TeamMembers
                                             where teamMember.Team == team
                                             orderby teamMember.Member.ID
                                             select teamMember.Member.ID).ToListAsync();

            return await GetAllUsersExcept(puzzleServerContext, memberUserIDs);
        }

        private static async Task<List<PuzzleUserView>> GetAllUsersExcept(PuzzleServerContext puzzleServerContext, List<int> userIDsToSkip)
        {
            // get all users (many)
            List<PuzzleUserView> users = await (from user in puzzleServerContext.PuzzleUsers
                           orderby user.ID
                           select new PuzzleUserView { ID = user.ID, Name = user.Name, Email = user.Email }).ToListAsync();

            // remove the skip list; easy walk since we've sorted both lists by user ID, terminates quickly
            // since most of the IDs added are smaller #s
            // do this in a try/catch, just in case of a lack of DB integrity causing us to run out of users
            try
            {
                int fullListIndex = 0;
                int skipListIndex = 0;

                while (skipListIndex < userIDsToSkip.Count)
                {
                    int nextAddedID = userIDsToSkip[skipListIndex];

                    while (users[fullListIndex].ID != nextAddedID)
                    {
                        fullListIndex++;
                    }

                    users.RemoveAt(fullListIndex);
                    skipListIndex++;
                }
            }
            catch { }

            return users;
        }

        public class PuzzleUserView
        {
            public int ID { get; set; }
            public string Name { get; set; }
            public string Email { get; set; }
        }

    }
}
