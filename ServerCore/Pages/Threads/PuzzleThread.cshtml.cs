using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using NuGet.Packaging;
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

            bool isFromGameControl = EventRole == EventRole.author || EventRole == EventRole.admin;
            if (!isFromGameControl)
            {
                Team = await UserEventHelper.GetTeamForPlayer(_context, Event, LoggedInUser);
            }

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
                TeamID = this.Team?.ID,
                IsFromGameControl = isFromGameControl,
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

            this.SendEmailNotifications(m, puzzle);

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

        public async Task<IActionResult> OnPostClaimThreadAsync(int messageId, int puzzleId)
        {
            var message = await _context.Messages.Where(m => m.ID == messageId).FirstOrDefaultAsync();
            if (message != null && IsAllowedToClaimMessage())
            {
                message.IsClaimed = true;
                message.ClaimerID = LoggedInUser.ID;
                message.Claimer = LoggedInUser;
                _context.Messages.Update(message);
                await _context.SaveChangesAsync();
            }

            return await this.OnGetAsync(puzzleId);
        }

        public async Task<IActionResult> OnPostUnclaimThreadAsync(int messageId, int puzzleId)
        {
            var message = await _context.Messages.Where(m => m.ID == messageId).FirstOrDefaultAsync();
            if (message != null && IsAllowedToClaimMessage())
            {
                message.IsClaimed = false;
                message.ClaimerID = null;
                message.Claimer = null;
                _context.Messages.Update(message);
                await _context.SaveChangesAsync();
            }

            return await this.OnGetAsync(puzzleId);
        }

        public bool IsAllowedToClaimMessage()
        {
            return EventRole == EventRole.admin
                || EventRole == EventRole.author;
        }

        public bool IsAllowedToDeleteMessage(Message message)
        {
            return EventRole == EventRole.admin
                || EventRole == EventRole.author
                || message.SenderID == LoggedInUser.ID;
        }

        private void SendEmailNotifications(Message newMessage, Puzzle puzzle)
        {
            // Send a notification email to further alert both authors and players.
            var recipients = new HashSet<string>();

            if (newMessage.IsFromGameControl)
            {
                // Send notification to team if message from game control.
                Message messageFromPlayer = _context.Messages.Where(message => message.ThreadId == newMessage.ThreadId && !message.IsFromGameControl).FirstOrDefault();
                if (messageFromPlayer != null)
                {
                    if (puzzle.IsForSinglePlayer)
                    {
                        recipients.Add(messageFromPlayer.Sender.Email);
                    }
                    else if (messageFromPlayer.TeamID != null)
                    {
                        recipients.AddRange(_context.TeamMembers
                            .Where(teamMember => teamMember.Team.ID == messageFromPlayer.TeamID)
                            .Select(teamMember => teamMember.Member.Email));
                    }
                }
            }
            else
            {
                // Send notification to authors and any game control person on the thread if message from player.
                recipients.AddRange(_context.PuzzleAuthors
                    .Where(pa => pa.Puzzle.ID == puzzle.ID)
                    .Select(pa => pa.Author.Email));

                recipients.AddRange(_context.Messages
                    .Where(message => message.ThreadId == newMessage.ThreadId && message.IsFromGameControl)
                    .Select(message => message.Sender.Email));
            }

            if (recipients.Any())
            {
                MailHelper.Singleton.SendPlaintextWithoutBcc(recipients,
                    $"{newMessage.Subject} thread update!",
                    "You have an update on your help message thread.");
            }
        }
    }
}
