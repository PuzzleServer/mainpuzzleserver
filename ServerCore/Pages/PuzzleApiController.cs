using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServerCore.DataModel;
using ServerCore.Helpers;

namespace ServerCore.Pages
{
    /// <summary>
    /// API controller for puzzles on external sites
    /// External sites don't have access to auth, so these APIs should rely on data given by this site
    /// and validate inputs as untrusted
    /// </summary>
    public class PuzzleApiController : Controller
    {
        private readonly PuzzleServerContext context;
        private readonly UserManager<IdentityUser> userManager;

        public PuzzleApiController(PuzzleServerContext context, UserManager<IdentityUser> userManager)
        {
            this.context = context;
            this.userManager = userManager;
        }

        /// <summary>
        /// Allows external puzzles to validate a team password
        /// </summary>
        /// <param name="teamPassword">Potential team password</param>
        /// <returns>True if the password belongs to a team, false otherwise</returns>
        [HttpGet]
        [Route("api/puzzleapi/validateteampassword")]
        public async Task<ActionResult<bool>> GetValidateTeamPasswordAsync(string teamPassword)
        {
            if (string.IsNullOrWhiteSpace(teamPassword))
            {
                return false;
            }

            return await (from team in context.Teams
                          where team.Password == teamPassword
                          select team).AnyAsync();
        }

        /// <summary>
        /// Allows external puzzles to validate a team password
        /// </summary>
        /// <param name="teamPassword">Potential team password</param>
        /// <returns>True if the password belongs to a team, false otherwise</returns>
        [HttpGet]
        [EnableCors("PuzzleApi")]
        [Route("api/puzzleapi/getmailinfo")]
        public async Task<ActionResult<MailInfo>> GetMailInfoAsync(string eventId, int puzzleId)
        {
            Event currentEvent = await EventHelper.GetEventFromEventId(context, eventId);
            Team team = await UserEventHelper.GetTeamForCurrentPlayer(context, currentEvent, User, userManager);

            if (team == null)
            {
                return Unauthorized();
            }

            Puzzle puzzle = await (from p in context.Puzzles where p.ID == puzzleId select p).FirstOrDefaultAsync();

            if (puzzle == null)
            {
                return NotFound();
            }

            PuzzleStatePerTeam puzzleState = await PuzzleStateHelper.GetFullReadOnlyQuery(context, currentEvent, puzzle, team, null).FirstOrDefaultAsync();
            if (puzzleState.UnlockedTime == null)
            {
                return Unauthorized();
            }

            MailInfo mailInfo = new MailInfo();

            mailInfo.PuzzleName = puzzle.Name;
            mailInfo.TeamName = team.Name;

            // replace commas with semicolons for better email support
            mailInfo.TeamContactEmail = team.PrimaryContactEmail?.Replace(',', ';');

            return mailInfo;
        }

        [HttpPost]
        [Authorize(Policy = "PlayerCanSeePuzzle")]
        [Route("api/puzzleapi/submitanswer/{eventId}/{puzzleId}")]
        public async Task<SubmissionResponse> PostSubmitAnswerAsync([FromBody] AnswerSubmission submission, [FromRoute] string eventId, [FromRoute] int puzzleId)
        {
            Event currentEvent = await EventHelper.GetEventFromEventId(context, eventId);

            PuzzleUser user = await PuzzleUser.GetPuzzleUserForCurrentUser(context, User, userManager);

            return await SubmissionEvaluator.EvaluateSubmission(context, user, currentEvent, puzzleId, submission.SubmissionText, submission.AllowFreeformSharing);
        }
    }

    public class AnswerSubmission
    {
        public string SubmissionText { get; set; }
        public bool AllowFreeformSharing { get; set; }
    }

    public class MailInfo
    {
        public string PuzzleName { get; set; }
        public string TeamName { get; set; }
        public string TeamContactEmail { get; set; }
    }
}
