using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using ServerCore.DataModel;
using ServerCore.Helpers;
using ServerCore.ServerMessages;

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
        private readonly IHubContext<ServerMessageHub> hubContext;

        public PuzzleApiController(PuzzleServerContext context, UserManager<IdentityUser> userManager, IHubContext<ServerMessageHub> hubContext)
        {
            this.context = context;
            this.userManager = userManager;
            this.hubContext = hubContext;
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
        /// Allows external puzzles to get information pertaining to how to identify that team in email
        /// </summary>
        /// <param name="eventId">ID of the event</param>
        /// <param name="puzzleId">ID of the puzzle</param>
        /// <param name="teamPassword">Team password</param>
        /// <returns>Identifying information for that team/puzzle</returns>
        [HttpGet]
        [EnableCors("PuzzleApi")]
        [Route("api/puzzleapi/getmailinfo")]
        public async Task<ActionResult<MailInfo>> GetMailInfoAsync(string eventId, int puzzleId, string? teamPassword)
        {
            Event currentEvent = await EventHelper.GetEventFromEventId(context, eventId);

            Team team;

            if (!string.IsNullOrEmpty(teamPassword))
            {
                team = await (from t in context.Teams where t.Event == currentEvent && t.Password == teamPassword select t).FirstOrDefaultAsync();
            }
            else
            {
                team = await UserEventHelper.GetTeamForCurrentPlayer(context, currentEvent, User, userManager);
            }

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

            mailInfo.PuzzleName = puzzle.PlaintextName;
            mailInfo.PuzzleSupportAlias = puzzle.SupportEmailAlias ?? currentEvent.ContactEmail ?? "puzzhunt@microsoft.com";
            mailInfo.TeamName = team.Name;

            // replace commas with semicolons for better email support
            mailInfo.TeamContactEmail = "";
            if (!System.String.IsNullOrEmpty(team.PrimaryContactEmail))
            {
                mailInfo.TeamContactEmail = team.PrimaryContactEmail.Replace(',', ';');
            }

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

        [HttpPost]
        [Route ("api/puzzleapi/liveevent/triggernotifications")]
        public async Task TriggerLiveEventNotifications(string eventId, int timerWindow)
        {
            Event currentEvent = await EventHelper.GetEventFromEventId(context, eventId);

           await LiveEventHelper.TriggerNotifications(context, currentEvent, timerWindow, hubContext);
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
        public string PuzzleSupportAlias { get; set; }
        public string TeamName { get; set; }
        public string TeamContactEmail { get; set; }
    }
}
