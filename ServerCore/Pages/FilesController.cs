using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServerCore.DataModel;
using ServerCore.Helpers;

namespace ServerCore.Pages
{
    /// <summary>
    /// Controller to redirect users to files in blob storage
    /// </summary>
    [Authorize]
    public class FilesController : Controller
    {
        private readonly PuzzleServerContext context;
        private readonly UserManager<IdentityUser> userManager;

        public FilesController(PuzzleServerContext context, UserManager<IdentityUser> userManager)
        {
            this.context = context;
            this.userManager = userManager;
        }

        [Route("{eventId}/Files/{*filename}")]
        public async Task<IActionResult> Index(string eventId, string filename)
        {
            Event eventObj = await EventHelper.GetEventFromEventId(context, eventId);

            ContentFile content = await (from contentFile in context.ContentFiles
                                         where contentFile.Event == eventObj &&
                                         contentFile.ShortName == filename
                                         select contentFile).SingleOrDefaultAsync();
            if (content == null)
            {
                return NotFound();
            }

            // TODO: check whether the user is authorized to see this file.
            // This should be based on the FileType:
            // * Admins have access to all files in their event
            // * Authors have access to all files attached to puzzles they own
            // * Players can see puzzles and materials on puzzles they've unlocked
            // * Players can see puzzles and materials in alpha-testing events when the puzzle needs alpha tests
            // * Players can see answers after the event's AnswersAvailable time
            // * Players can see solve tokens on puzzles they've solved
            if (!(eventObj.IsAlphaTestingEvent && content.Puzzle.AlphaTestsNeeded > 0) && !await IsAuthorized(eventObj.ID, content.Puzzle, content))
            {
                return Unauthorized();
            }

            return Redirect($"{content.Url}{HttpContext.Request.QueryString}");
        }

        /// <summary>
        /// Returns whether the user is authorized to view the file
        /// </summary>
        /// <param name="eventId">The current event</param>
        /// <param name="puzzle">The puzzle the file belongs to</param>
        /// <param name="content">The file</param>
        private async Task<bool> IsAuthorized(int eventId, Puzzle puzzle, ContentFile content)
        {
            Event currentEvent = await (from ev in context.Events
                                        where ev.ID == eventId
                                        select ev).SingleAsync();
            PuzzleUser user = await PuzzleUser.GetPuzzleUserForCurrentUser(context, User, userManager);

            // Admins can see all files
            if (await user.IsAdminForEvent(context, currentEvent))
            {
                return true;
            }

            // Authors can see all files attached to their puzzles
            if (await UserEventHelper.IsAuthorOfPuzzle(context, puzzle, user))
            {
                return true;
            }

            // Once answers are available, so are all other files
            if (currentEvent.AreAnswersAvailableNow)
            {
                return true;
            }

            DateTime? unlockedTime = null;
            DateTime? solvedTime = null;

            if (puzzle.IsForSinglePlayer)
            {
                // get unlock/solve data from player if available
                SinglePlayerPuzzleStatePerPlayer playerState = await SinglePlayerPuzzleStateHelper.GetFullReadOnlyQuery(context, currentEvent, puzzle.ID, user.ID).FirstOrDefaultAsync();

                if (playerState?.UnlockedTime != null)
                {
                    unlockedTime = playerState.UnlockedTime;
                    solvedTime = playerState.SolvedTime;
                }
                else
                {
                    // not available? Check global state
                    SinglePlayerPuzzleUnlockState globalState = await SinglePlayerPuzzleUnlockStateHelper.GetFullReadOnlyQuery(context, currentEvent, puzzle.ID).FirstOrDefaultAsync();
                    unlockedTime = globalState?.UnlockedTime;
                }
            }
            else
            {
                Team team = await UserEventHelper.GetTeamForPlayer(context, currentEvent, user);
                if (team == null)
                {
                    return false;
                }

                PuzzleStatePerTeam teamState = await PuzzleStateHelper.GetFullReadOnlyQuery(context, currentEvent, puzzle, team).SingleAsync();
                unlockedTime = teamState?.UnlockedTime;
                solvedTime = teamState?.SolvedTime;
            }

            switch (content.FileType)
            {
                case ContentFileType.Answer:
                    // The positive case is already handled above by checking AreAnswersAvailableNow
                    return false;

                case ContentFileType.Puzzle:
                case ContentFileType.PuzzleMaterial:
                    if (unlockedTime != null)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }

                case ContentFileType.SolveToken:
                    if (solvedTime != null)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }

                default:
                    throw new NotImplementedException();
            }
        }
    }
}