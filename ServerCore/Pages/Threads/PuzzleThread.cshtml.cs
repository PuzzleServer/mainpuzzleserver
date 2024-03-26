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
    public class PuzzleThreadModel : EventSpecificPageModel
    {
        public const int MessageCharacterLimit = 3000;

        public PuzzleThreadModel(PuzzleServerContext serverContext, UserManager<IdentityUser> userManager) : base(serverContext, userManager)
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
        /// Gets or sets the team.
        /// </summary>
        public Puzzle Puzzle { get; set; }

        /// <summary>
        /// Gets or sets the list of messages in the thread.
        /// </summary>
        public List<Message> Messages { get; set; }

        public async Task<IActionResult> OnGetAsync(int? puzzleId)
        {
            if (LoggedInUser == null)
            {
                return Challenge();
            }

            if (!puzzleId.HasValue)
            {
                return NotFound();
            }

            Puzzle = await _context.Puzzles.Where(m => m.ID == puzzleId).FirstOrDefaultAsync();
            if (Puzzle == null)
            {
                return NotFound();
            }

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
            if (string.IsNullOrWhiteSpace(NewMessage.Text))
            {
                ModelState.AddModelError("NewMessage.Text", "Your message cannot be empty.");
            }
            else if (NewMessage.Text.Length > MessageCharacterLimit)
            {
                ModelState.AddModelError("NewMessage.Text", "Your message should not be longer than 3000 characters.");
            }

            if (!ModelState.IsValid)
            {
                return await this.OnGetAsync(NewMessage.PuzzleID);
            }

            var puzzle = await _context.Puzzles.Where(m => m.ID == NewMessage.PuzzleID).FirstOrDefaultAsync();
            if (puzzle == null)
            {
                return RedirectToPage("/Index");
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
            var message = await _context.Messages.Where(m => m.ID == messageId).FirstOrDefaultAsync();
            if (message != null && IsAllowedToDeleteMessage(message))
            {
                _context.Messages.Remove(message);
                await _context.SaveChangesAsync();
            }

            return await this.OnGetAsync(puzzleId);
        }

        private bool IsAllowedToDeleteMessage(Message message)
        {
            return EventRole == EventRole.admin
                || EventRole == EventRole.author
                || message.SenderID == LoggedInUser.ID;
        }
    }
}
