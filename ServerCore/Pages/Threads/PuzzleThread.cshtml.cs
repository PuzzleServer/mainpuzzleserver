using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using NuGet.Packaging;
using ServerCore.DataModel;
using ServerCore.Helpers;
using ServerCore.ModelBases;
using ServerCore.ServerMessages;

namespace ServerCore.Pages.Threads
{
    [AllowAnonymous]
    public class PuzzleThreadModel : EventSpecificPageModel
    {
        public const int MessageCharacterLimit = 3000;

        private IHubContext<ServerMessageHub> messageHub;

        public PuzzleThreadModel(PuzzleServerContext serverContext, UserManager<IdentityUser> userManager, IHubContext<ServerMessageHub> messageHub) : base(serverContext, userManager)
        {
            this.messageHub = messageHub;
        }

        /// <summary>
        /// The newly created message when the user creates a message.
        /// </summary>
        [BindProperty]
        public Message NewMessage { get; set; }

        /// <summary>
        /// The edited message.
        /// </summary>
        [BindProperty]
        public Message EditMessage { get; set; }

        /// <summary>
        /// Gets or sets the team.
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

        public async Task<IActionResult> OnGetAsync(int? puzzleId, int? teamId, int? playerId)
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

            Puzzle = await _context.Puzzles.Where(m => m.ID == puzzleId).FirstOrDefaultAsync();
            if (Puzzle == null
                || (Puzzle.IsForSinglePlayer && !playerId.HasValue)
                || (!Puzzle.IsForSinglePlayer && !teamId.HasValue))
            {
                return NotFound();
            }

            Team team = null;
            PuzzleUser singlePlayerPuzzlePlayer = null;
            string subject = null;
            string threadId = null;
            bool isGameControl = IsGameControlRole();
            if (Event.ShouldShowHelpMessageOnlyToAuthor && EventRole == EventRole.author)
            {
                PuzzleAuthors author = _context.PuzzleAuthors.Where(puzzleAuthor => puzzleAuthor.PuzzleID == puzzleId && puzzleAuthor.AuthorID == LoggedInUser.ID).FirstOrDefault();
                if (author == null)
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

                subject = $"[{singlePlayerPuzzlePlayer.Name}]{Puzzle.Name}";
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

                subject = $"[{team.Name}]{Puzzle.Name}";
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
            };

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (Event.AreAnswersAvailableNow)
            {
                ModelState.AddModelError("NewMessage.Text", "Answers are already available.");
            }

            ValidationResult validationResult = IsMessageTextValid(NewMessage.Text);
            if (!validationResult.IsValid)
            {
                ModelState.AddModelError("NewMessage.Text", validationResult.FailureReason);
            }

            ModelState.Remove("EventId");
            if (!ModelState.IsValid)
            {
                return await this.OnGetAsync(NewMessage.PuzzleID, NewMessage.TeamID, NewMessage.PlayerID);
            }

            var puzzle = await _context.Puzzles.Where(m => m.ID == NewMessage.PuzzleID).FirstOrDefaultAsync();
            if (puzzle == null)
            {
                return RedirectToPage("/Index");
            }

            // Validate to make sure user actually allowed to post
            if (!this.IsGameControlRole())
            {
                if ((puzzle.IsForSinglePlayer && NewMessage.PlayerID != LoggedInUser.ID)
                    || (!puzzle.IsForSinglePlayer && NewMessage.TeamID != (await this.GetTeamId())))
                {
                    throw new InvalidOperationException("You are not allowed to post to this thread.");
                }
            }

            Message m = new Message();
            DateTime now = DateTime.UtcNow;
            m.ThreadId = NewMessage.ThreadId;
            m.IsFromGameControl = NewMessage.IsFromGameControl;
            m.Subject = NewMessage.Subject;
            m.EventID = NewMessage.EventID;
            m.CreatedDateTimeInUtc = now;
            m.ModifiedDateTimeInUtc = now;
            m.Text = NewMessage.Text;
            m.SenderID = NewMessage.SenderID;
            m.PuzzleID = NewMessage.PuzzleID;
            m.TeamID = NewMessage.TeamID;
            m.ClaimerID = NewMessage.ClaimerID;
            m.PlayerID = NewMessage.PlayerID;

            using (IDbContextTransaction transaction = _context.Database.BeginTransaction(System.Data.IsolationLevel.Serializable))
            {
                _context.Messages.Add(m);
                await _context.SaveChangesAsync();
                transaction.Commit();
            }

            await this.SendEmailNotifications(m, puzzle);

            return RedirectToPage("/Threads/PuzzleThread", new { puzzleId = m.PuzzleID, teamId = m.TeamID, playerId = m.PlayerID });
        }

        public async Task<IActionResult> OnPostEditMessageAsync()
        {
            if (Event.AreAnswersAvailableNow)
            {
                ModelState.AddModelError("EditMessage.Text", $"Edit failed because no updates can be made after answers are available");
            }

            ValidationResult validationResult = IsMessageTextValid(EditMessage.Text);
            if (!validationResult.IsValid)
            {
                ModelState.AddModelError("EditMessage.Text", $"Edit failed because {validationResult.FailureReason}");
            }

            ModelState.Remove("EventId");
            if (!ModelState.IsValid)
            {
                return await this.OnGetAsync(EditMessage.PuzzleID, EditMessage.TeamID, EditMessage.PlayerID);
            }

            var message = await _context.Messages.Where(m => m.ID == EditMessage.ID).FirstOrDefaultAsync();
            if (message != null && IsAllowedToEditMessage(message))
            {
                message.Text = EditMessage.Text;
                message.ModifiedDateTimeInUtc = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }

            return RedirectToPage("/Threads/PuzzleThread", new { puzzleId = EditMessage.PuzzleID, teamId = EditMessage.TeamID, playerId = EditMessage.PlayerID });
        }

        public async Task<IActionResult> OnGetDeleteMessageAsync(int messageId, int puzzleId, int? teamId, int? playerId)
        {
            var message = await _context.Messages.Where(m => m.ID == messageId).FirstOrDefaultAsync();
            if (message != null && IsAllowedToDeleteMessage(message))
            {
                _context.Messages.Remove(message);
                await _context.SaveChangesAsync();
            }

            return RedirectToPage("/Threads/PuzzleThread", new { puzzleId = puzzleId, teamId = teamId, playerId = playerId });
        }

        public async Task<IActionResult> OnPostClaimThreadAsync(int messageId, int puzzleId, int? teamId, int? playerId)
        {
            var message = await _context.Messages.Where(m => m.ID == messageId).FirstOrDefaultAsync();
            if (message != null 
                && IsAllowedToClaimMessage()
                && !message.ClaimerID.HasValue)
            {
                message.ClaimerID = LoggedInUser.ID;
                message.Claimer = LoggedInUser;
                _context.Messages.Update(message);
                await _context.SaveChangesAsync();
            }
            else
            {
                throw new InvalidOperationException("You cannot claim this thread! It may have already been claimed.");
            }

            return RedirectToPage("/Threads/PuzzleThread", new { puzzleId = puzzleId, teamId = teamId, playerId = playerId });
        }

        public async Task<IActionResult> OnPostUnclaimThreadAsync(int messageId, int puzzleId, int? teamId, int? playerId)
        {
            var message = await _context.Messages.Where(m => m.ID == messageId).FirstOrDefaultAsync();
            if (message != null && IsAllowedToClaimMessage())
            {
                message.ClaimerID = null;
                message.Claimer = null;
                _context.Messages.Update(message);
                await _context.SaveChangesAsync();
            }

            return RedirectToPage("/Threads/PuzzleThread", new { puzzleId = puzzleId, teamId = teamId, playerId = playerId });
        }

        public bool IsAllowedToClaimMessage()
        {
            return EventRole == EventRole.admin
                || EventRole == EventRole.author;
        }

        public bool IsAllowedToEditMessage(Message message)
        {
            return !Event.AreAnswersAvailableNow && message.SenderID == LoggedInUser.ID;
        }

        public bool IsAllowedToDeleteMessage(Message message)
        {
            return !Event.AreAnswersAvailableNow && message.SenderID == LoggedInUser.ID;
        }

        private async Task SendEmailNotifications(Message newMessage, Puzzle puzzle)
        {
            // Send a notification email to further alert both authors and players.
            string emailTitle = $"{newMessage.Subject} thread update!";
            string emailContent = "You have an update on your help message thread.";
            string toastTitle = $"Help message from {(newMessage.IsFromGameControl ? "Game Control" : newMessage.Sender.Name)}";
            string toastContent = $"{newMessage.Subject}";
            string threadUrlSuffix = $"Threads/PuzzleThread/{newMessage.PuzzleID}?teamId={newMessage.TeamID}&playerId={newMessage.PlayerID}";

            var recipients = new HashSet<PuzzleUser>();

            if (newMessage.IsFromGameControl)
            {
                // Send notification to team if message from game control.
                Message messageFromPlayer = _context.Messages.Where(message => message.ThreadId == newMessage.ThreadId && !message.IsFromGameControl).FirstOrDefault();
                if (messageFromPlayer != null)
                {
                    if (puzzle.IsForSinglePlayer)
                    {
                        recipients.Add(messageFromPlayer.Sender);
                        await messageHub.SendNotification(messageFromPlayer.Sender, toastTitle, toastContent, $"/{newMessage.Event.EventID}/play/{threadUrlSuffix}");
                    }
                    else if (messageFromPlayer.TeamID != null)
                    {
                        recipients.AddRange(await _context.TeamMembers
                            .Where(teamMember => teamMember.Team.ID == messageFromPlayer.TeamID)
                            .Select(teamMember => teamMember.Member).ToArrayAsync());
                        await messageHub.SendNotification(messageFromPlayer.Team, toastTitle, toastContent, $"/{newMessage.Event.EventID}/play/{threadUrlSuffix}");
                    }
                }
            }
            else
            {
                // Send notification to authors and any game control person on the thread if message from player.

                HashSet<PuzzleUser> staff = new HashSet<PuzzleUser>();
                staff.AddRange(await _context.PuzzleAuthors
                    .Where(pa => pa.Puzzle.ID == puzzle.ID)
                    .Select(pa => pa.Author).ToArrayAsync());

                staff.AddRange(await _context.Messages
                    .Where(message => message.ThreadId == newMessage.ThreadId && message.IsFromGameControl)
                    .Select(message => message.Sender).ToArrayAsync());

                HashSet<PuzzleUser> admins = new HashSet<PuzzleUser>();
                admins.AddRange(await _context.EventAdmins
                    .Where(ea => ea.EventID == Event.ID)
                    .Select(ea => ea.Admin).ToArrayAsync());

                recipients.AddRange(staff);
                
                foreach (var staffer in staff)
                {
                    await messageHub.SendNotification(staffer, toastTitle, toastContent, $"/{newMessage.Event.EventID}/{(admins.Contains(staffer) ? "admin" : "author")}/{threadUrlSuffix}");
                }
            }

            if (recipients.Any())
            {
                MailHelper.Singleton.SendPlaintextWithoutBcc(recipients.Select(r => r.Email), emailTitle, emailContent);
            }
        }

        private ValidationResult IsMessageTextValid(string messageText)
        {
            if (string.IsNullOrWhiteSpace(messageText))
            {
                return ValidationResult.CreateFailure("Your message cannot be empty.");
            }
            else if (messageText.Length > MessageCharacterLimit)
            {
                return ValidationResult.CreateFailure($"Your message should not be longer than {MessageCharacterLimit} characters.");
            }
            else
            {
                return ValidationResult.CreateSuccess();
            }
        }

        private class ValidationResult
        {
            public bool IsValid { get; set; }

            public string FailureReason { get; set; }

            public static ValidationResult CreateSuccess()
            {
                return new ValidationResult { IsValid = true };
            }

            public static ValidationResult CreateFailure(string failureReason)
            {
                return new ValidationResult { IsValid = false, FailureReason = failureReason };
            }
        }
    }
}
