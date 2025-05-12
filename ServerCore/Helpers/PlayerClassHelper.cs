using ServerCore.DataModel;
using ServerCore.ModelBases;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace ServerCore.Helpers
{
    public class PlayerClassHelper
    {
        /// <summary>
        /// Get the PlayerClass associated with a given classId
        /// </summary>
        public static async Task<PlayerClass> GetPlayerClassFromID(PuzzleServerContext puzzleServerContext, int classId)
        {
            return await puzzleServerContext.PlayerClasses.Where(pc => pc.ID == classId).FirstOrDefaultAsync();
        }

        /// <summary>
        /// Gets a list of PlayerClasses that are unassigned for the given team
        /// </summary>
        public static async Task<List<PlayerClass>> GetUnassignedPlayerClasses(PuzzleServerContext context, int eventId, int teamId)
        {
            List<PlayerClass> allClasses = await context.PlayerClasses.Where(c => c.EventID == eventId).ToListAsync();
            var assignedClasses = await context.TeamMembers.Where(tm => tm.Team.ID == teamId).Select(tm => tm.Class).ToListAsync();
            List<PlayerClass> unassignedClasses = allClasses.Except(assignedClasses).ToList();
            return unassignedClasses;
        }

        /// <summary>
        /// Gets a list of the PlayerClasses that can be assigned to a player on a given team.
        /// If the user is an admin then the list contains all classes (allowing the admin to assign multiple players to the same class if needed).
        /// If the list is going to be used for Temporary Classes then it contains all classes (since multiple players can pick the same temporary class).
        /// Otherwise the list contains only the unassigned classes.
        /// </summary>
        /// <returns>A list of all unassigned PlayerClasses</returns>
        public static async Task<List<PlayerClass>> GetAvailablePlayerClasses(PuzzleServerContext context, int eventId, EventRole eventRole, int teamId)
        {
            if (eventRole != EventRole.admin)
            {
                List<PlayerClass> unassignedClasses = await GetUnassignedPlayerClasses(context, eventId, teamId);
                return unassignedClasses;
            }

            List<PlayerClass> allClasses = await context.PlayerClasses.Where(c => c.EventID == eventId).ToListAsync();
            return allClasses;
        }

        /// <summary>
        /// Gets a list of the PlayerClasses that can be assigned to a player on a given team, sorted by Order.
        /// If the user is an admin then the list contains all classes (allowing the admin to assign multiple players to the same class if needed).
        /// If the list is going to be used for Temporary Classes then it contains all classes (since multiple players can pick the same temporary class).
        /// Otherwise the list contains only the unassigned classes.
        /// This is intended for UI elements that don't sort for display and need pre-ordered data inputs.
        /// </summary>
        public static async Task<List<PlayerClass>> GetAvailablePlayerClassesSorted(PuzzleServerContext context, int eventId, EventRole eventRole, int teamId)
        {
            List<PlayerClass> availableClasses = await GetAvailablePlayerClasses(context, eventId, eventRole, teamId);
            availableClasses = availableClasses.OrderBy(pc => pc.Order).ToList();
            return availableClasses;
        }

        /// <summary>
        /// Gets the complete list of PlayerClasses for the given event, sorted by Order
        /// </summary>
        public static async Task<List<PlayerClass>> GetAllPlayerClassesSorted(PuzzleServerContext context, int eventId)
        {
            List<PlayerClass> allClasses = await context.PlayerClasses.Where(c => c.EventID == eventId).OrderBy(pc => pc.Order).ToListAsync();
            return allClasses;
        }

        /// <summary>
        /// Gets a random PlayerClass from the classes that are currently unassigned on the team
        /// This overload queries the database to find out which classes are available
        /// </summary>
        /// <returns>A single PlayerClass that has not been assigned to any players on the given team</returns>
        public static async Task<PlayerClass> GetRandomPlayerClassFromAvailable(PuzzleServerContext context, int eventId, EventRole eventRole, int teamId)
        {
            Team team = await context.Teams.FirstOrDefaultAsync(m => m.ID == teamId);
            if (team == null)
            {
                return null;
            }

            List<PlayerClass> availableClasses = await GetAvailablePlayerClasses(context, eventId, eventRole, teamId);
            Random rand = new Random();
            return availableClasses[rand.Next(availableClasses.Count - 1)];
        }

        /// <summary>
        /// Gets a random PlayerClass from the classes that are currently unassigned on the team
        /// This overload uses a local copy of the available classes instead of a database query
        /// </summary>
        /// <param name="availableClasses">A list of PlayerClasses that the function can choose from</param>
        /// <returns>A single PlayerClass from the provided list</returns>
        public static PlayerClass GetRandomPlayerClassFromAvailable(List<PlayerClass> availableClasses)
        {
            Random rand = new Random();
            return availableClasses[rand.Next(availableClasses.Count - 1)];
        }

        /// <summary>
        /// Assigns the current TeamMember a random PlayerClass from the classes that are currently unassigned on the team
        /// This function will not assign a PlayerClass if the player already has a class selected and it is available on the given team
        /// </summary>
        /// <param name="member">The TeamMember who needs to be assigned a PlayerClass</param>
        public static async Task AssignRandomPlayerClass(PuzzleServerContext context, TeamMembers member, Event currentEvent, EventRole eventRole)
        {
            List<PlayerClass> availableClasses = await GetUnassignedPlayerClasses(context, member.Team.EventID, member.Team.ID);

            // Don't change their class if they already have one and no one else on the team does
            // This is needed for team merging logic, to prevent changing the class the player picked unless necessary
            if (member.Class != null && availableClasses.Contains(member.Class))
            {
                return;
            }

            if (member.Class == null && eventRole == EventRole.admin && availableClasses.IsNullOrEmpty())
            {
                // Admins can set multiple players to the same class, so pick any random class
                // If this feature gets a lot of use we'll want to make sure the additional players are distributed,
                // but in most cases there won't be more than one or two extra players so it's not a likely case
                member.Class = GetRandomPlayerClassFromAvailable(await GetAllPlayerClassesSorted(context, currentEvent.ID));
            }
            else
            {
                member.Class = GetRandomPlayerClassFromAvailable(availableClasses);
            }

            await SetPlayerClass(context, currentEvent, eventRole, member, member.Class.ID);
        }

        /// <summary>
        /// Sets the given PlayerClass (based on ID) for the given TeamMember
        /// If the ID is set to 123456789 the PlayerClass is unset (set to null)
        /// </summary>
        public static async Task<Tuple<bool, string>> SetPlayerClass(PuzzleServerContext context, Event currentEvent, EventRole eventRole, TeamMembers member, int playerClassId, bool IsTempClass = false)
        {
            // Once the event has started the player's class is locked in unless changed by a admin
            // Temporary classes can be changed at any time and to any class, whether or not the class is assigned to another player on the team
            if (currentEvent.EventHasStarted && eventRole != EventRole.admin && !IsTempClass)
            {
                return new Tuple<bool, string>(false, $"The event has started and your {currentEvent.PlayerClassName} cannot be changed. Temporary overrides to your {currentEvent.PlayerClassName} can be made using the {currentEvent.PlayerClassName} Override menu on the Team Details page.");
            }
            else
            {
                // This value is set in the UI and can be changed if needed in the future,
                // it just needs to be a number big enough that there won't be a matching valid ID in the database
                if (playerClassId == 123456789)
                {
                    if (IsTempClass)
                    {
                        member.TemporaryClass = member.Class;
                    }
                    else
                    {
                        member.Class = null;
                    }
                }
                else
                {
                    PlayerClass playerClass = await PlayerClassHelper.GetPlayerClassFromID(context, playerClassId);

                    if (IsTempClass)
                    {
                        member.TemporaryClass = playerClass;
                    }
                    else
                    {
                        List<PlayerClass> availableClasses = await GetAvailablePlayerClasses(context, currentEvent.ID, eventRole, member.Team.ID);

                        // Only one player per team can have a specific class unless an admin is the one assigning it
                        if (!availableClasses.Contains(playerClass) && eventRole != EventRole.admin)
                        {
                            return new Tuple<bool, string>(false, $"Sorry, {playerClass.Name} is already assigned for this team. Please select another {currentEvent.PlayerClassName}!");
                        }

                        member.Class = playerClass;
                    }
                }

                await context.SaveChangesAsync();
            }

            return new Tuple<bool, string>(true, "");
        }
    }
}
