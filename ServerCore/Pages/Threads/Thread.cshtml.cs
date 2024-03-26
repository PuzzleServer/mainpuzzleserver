using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using ServerCore.DataModel;
using ServerCore.Helpers;
using ServerCore.ModelBases;

namespace ServerCore.Pages.Threads
{
    [AllowAnonymous]
    public class ThreadModel : EventSpecificPageModel
    {
        public ThreadModel(PuzzleServerContext serverContext, UserManager<IdentityUser> userManager) : base(serverContext, userManager)
        {
        }

        [BindProperty]
        public Message NewMessage { get; set; }

        /// <summary>
        /// Gets or sets the thread id.
        /// </summary>
        public string ThreadId { get; set; }

        /// <summary>
        /// Gets or sets the team.
        /// </summary>
        public Team Team { get; set; }

        /// <summary>
        /// Gets or sets the puzzle id.
        /// </summary>
        public int PuzzleId { get; set; }

        /// <summary>
        /// Gets or sets the team.
        /// </summary>
        public Puzzle Puzzle { get; set; }

        /// <summary>
        /// Gets or sets the list of messages in the thread.
        /// </summary>
        public List<Message> Messages { get; set; }

        public async Task<IActionResult> OnGetAsync(int? puzzleId)
        {
            if (!puzzleId.HasValue)
            {
                throw new ArgumentException("Missing puzzle id.");
            }

            Puzzle = await _context.Puzzles.Where(m => m.ID == puzzleId).FirstOrDefaultAsync();
            if (Puzzle == null)
            {
                throw new ArgumentException("Not a valid puzzle.");
            }

            PuzzleId = Puzzle.ID;
            Team = await UserEventHelper.GetTeamForPlayer(_context, Event, LoggedInUser);
            Messages = this._context.Messages
                .Where(message => message.PuzzleID.Value == puzzleId && message.TeamID.Value == Team.ID)
                .ToList();

            if (Messages.Count > 0)
            {
                this.ThreadId = Messages[0].ThreadId;
            }

            string subject = Puzzle.IsForSinglePlayer
                ? $"[{LoggedInUser.Name}]{Puzzle.Name}"
                : $"[{Team.Name}]{Puzzle.Name}";

            NewMessage = new Message()
            {
                ThreadId = this.ThreadId,
                EventID = Event.ID,
                Event = Event,
                Subject = subject,
                PuzzleID = this.Puzzle.ID,
                TeamID = this.Team.ID,
                IsFromGameControl = EventRole == EventRole.author || EventRole == EventRole.admin,
                SenderID = LoggedInUser.ID,
                Sender = LoggedInUser,
            };

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return await this.OnGetAsync(PuzzleId);
            }

            var puzzle = await _context.Puzzles.Where(m => m.ID == NewMessage.PuzzleID).FirstOrDefaultAsync();
            if (puzzle == null)
            {
                return Page();
            }

            Message m = new Message();

            string threadId = NewMessage.ThreadId;
            if (string.IsNullOrEmpty(threadId))
            {
                threadId = puzzle.IsForSinglePlayer
                    ? $"SinglePlayerPuzzle_{NewMessage.PuzzleID}_{NewMessage.SenderID}"
                    : $"Puzzle_{NewMessage.PuzzleID}_{NewMessage.TeamID}";
            }

            m.ThreadId = threadId;
            m.IsFromGameControl = NewMessage.IsFromGameControl;
            m.Subject = NewMessage.Subject;
            m.EventID = NewMessage.EventID;
            m.DateTimeInUtc = DateTime.UtcNow;
            m.Text = NewMessage.Text;
            m.SenderID = NewMessage.SenderID;
            m.PuzzleID = NewMessage.PuzzleID;
            m.TeamID = NewMessage.TeamID;
            m.IsClaimed = NewMessage.IsClaimed;
            m.ClaimerID = NewMessage.ClaimerID;

            using (IDbContextTransaction transaction = _context.Database.BeginTransaction(System.Data.IsolationLevel.Serializable))
            {
                _context.Messages.Add(m);
                await _context.SaveChangesAsync();
                transaction.Commit();
            }

            return await this.OnGetAsync(m.PuzzleID);
        }

        public async Task<IActionResult> OnGetDeleteMessageAsync(int messageId, int puzzleId)
        {
            _context.Messages.Where(m => m.ID == messageId).ExecuteDelete();

            return await this.OnGetAsync(puzzleId);
        }
    }
}
