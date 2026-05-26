using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServerCore.DataModel;
using ServerCore.Helpers;
using ServerCore.ModelBases;

namespace ServerCore.Pages.Threads
{
    [AllowAnonymous]
    public class PuzzleThreadModel : EventSpecificPageModel
    {
        public const string DeletedMessage = "This message has been deleted";

        private readonly PuzzleThreadService puzzleThreadService;

        public PuzzleThreadModel(PuzzleServerContext serverContext, UserManager<IdentityUser> userManager, PuzzleThreadService puzzleThreadService) : base(serverContext, userManager)
        {
            this.puzzleThreadService = puzzleThreadService;
        }

        /// <summary>
        /// The newly created message when the user creates a message.
        /// </summary>
        [BindProperty]
        public Message NewMessage { get; set; }

        /// <summary>
        /// Gets or sets the puzzle.
        /// </summary>
        public Puzzle Puzzle { get; set; }

        /// <summary>
        /// Gets or sets the puzzle state.
        /// </summary>
        public PuzzleStateBase PuzzleState { get; set; }

        /// <summary>
        /// Gets or sets the list of messages in the thread.
        /// </summary>
        public List<Message> Messages { get; set; }

        /// <summary>
        /// Gets or sets the query parameters to add to the ReturnToThreads URI.
        /// </summary>
        public string ReturnThreadQueryParams { get; set; }

        public async Task<IActionResult> OnGetAsync(int? puzzleId, int? teamId, int? playerId, string returnThreadQueryParams, string messageDraft = null)
        {
            // Validate parameters
            if (LoggedInUser == null)
            {
                return Challenge();
            }

            if (!puzzleId.HasValue)
            {
                return NotFound();
            }

            this.ReturnThreadQueryParams = returnThreadQueryParams;
            Puzzle = await _context.Puzzles.Where(m => m.ID == puzzleId).FirstOrDefaultAsync();
            if (Puzzle == null
                || (Puzzle.IsForSinglePlayer && !playerId.HasValue)
                || (!Puzzle.IsForSinglePlayer && !teamId.HasValue))
            {
                return NotFound();
            }

            // If we are showing hints, only allow the thread to be seen if the user has unlocked it
            if (!Event.HideHints)
            {
                if (Event.DefaultCostForHelpThread < 0)
                {
                    return Forbid();
                }

                PuzzleStateBase puzzleStateBase = await (
                    from puzzleState in _context.PuzzleStatePerTeam
                    where puzzleState.PuzzleID == Puzzle.ID && puzzleState.TeamID == teamId
                    select puzzleState).FirstOrDefaultAsync();

                if (puzzleStateBase == null || !puzzleStateBase.IsHelpThreadUnlockedByCoins)
                {
                    return Forbid();
                }
            }

            Team team = null;
            PuzzleUser singlePlayerPuzzlePlayer = null;
            string subject = null;
            string threadId = null;
            bool isGameControl = IsGameControlRole();
            if (Event.ShouldShowHelpMessageOnlyToAuthor && (EventRole == EventRole.author || EventRole.IsImpersonating) && !Puzzle.ShowHelpThreadsToAllGameControl)
            {
                PuzzleAuthors author = _context.PuzzleAuthors.Where(puzzleAuthor => puzzleAuthor.PuzzleID == puzzleId && puzzleAuthor.AuthorID == LoggedInUser.ID).FirstOrDefault();
                if (author == null && (EventRole == EventRole.author || !await LoggedInUser.IsAdminForEvent(_context, Event)))
                {
                    throw new InvalidOperationException("You are not an author of this puzzle.");
                }
            }

            if (Puzzle.IsForSinglePlayer)
            {
                singlePlayerPuzzlePlayer = _context.PuzzleUsers.Where(user => user.ID == playerId).FirstOrDefault();
                if (singlePlayerPuzzlePlayer == null)
                {
                    return NotFound();
                }

                if (!isGameControl && playerId != LoggedInUser.ID)
                {
                    // If is player, they can only see this thread if they match the single player puzzle player id.
                    return NotFound();
                }

                subject = $"[{singlePlayerPuzzlePlayer.Name}]{Puzzle.PlaintextName}";
                threadId = MessageHelper.GetSinglePlayerPuzzleThreadId(Puzzle.ID, playerId.Value);
                teamId = null;
                PuzzleState = await SinglePlayerPuzzleStateHelper.GetOrAddStateIfNotThere(
                    _context,
                    Event,
                    Puzzle,
                    playerId.Value);
            }
            else
            {
                team = await _context.Teams.Where(t => t.ID == teamId).FirstOrDefaultAsync();
                if (team == null)
                {
                    return NotFound();
                }

                if (!isGameControl 
                    && (await this.GetTeamId()) != team.ID)
                {
                    // If is player, they can only see this thread if they are on the team.
                    return NotFound();
                }

                subject = $"[{team.Name}]{Puzzle.PlaintextName}";
                threadId = MessageHelper.GetTeamPuzzleThreadId(Puzzle.ID, teamId.Value);
                playerId = null;
                PuzzleState = await PuzzleStateHelper
                    .GetFullReadOnlyQuery(
                        _context,
                        Event,
                        Puzzle,
                        team)
                    .FirstAsync();
            }

            Messages = this._context.Messages
                .Where(message => message.ThreadId == threadId)
                .ToList();

            NewMessage = new Message()
            {
                ThreadId = threadId,
                EventID = Event.ID,
                Event = Event,
                Subject = subject,
                PuzzleID = this.Puzzle.ID,
                TeamID = teamId,
                IsFromGameControl = isGameControl,
                SenderID = LoggedInUser.ID,
                Sender = LoggedInUser,
                PlayerID = playerId,
                Player = singlePlayerPuzzlePlayer,
                Text = messageDraft
            };

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            await puzzleThreadService.SendMessageAsync(
                NewMessage.ThreadId,
                NewMessage.EventID,
                NewMessage.Subject,
                NewMessage.PuzzleID.Value,
                NewMessage.TeamID,
                NewMessage.PlayerID,
                NewMessage.IsFromGameControl,
                LoggedInUser.ID,
                NewMessage.Text);

            return RedirectToPage("/Threads/PuzzleThread", new { puzzleId = NewMessage.PuzzleID, teamId = NewMessage.TeamID, playerId = NewMessage.PlayerID });
        }

        public bool IsAllowedToClaimMessage()
        {
            return EventRole == EventRole.admin
                || EventRole == EventRole.author;
        }
    }
}
