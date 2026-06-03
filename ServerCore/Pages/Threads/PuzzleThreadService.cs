using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using NuGet.Packaging;
using ServerCore.DataModel;
using ServerCore.Exceptions;
using ServerCore.ServerMessages;

namespace ServerCore.Pages.Threads
{
    public class PuzzleThreadService
    {
        private readonly PuzzleServerContext context;
        private readonly IHubContext<ServerMessageHub> messageHub;

        public PuzzleThreadService(PuzzleServerContext context, IHubContext<ServerMessageHub> messageHub)
        {
            this.context = context;
            this.messageHub = messageHub;
        }

        public async Task<ThreadMessageDTO> SendMessageAsync(string threadId, int eventId, string subject, int puzzleId, int? teamId, int? playerId, bool isFromGameControl, int senderId, string text)
        {
            Event eventObj = await context.Events.Where(e => e.ID == eventId).FirstOrDefaultAsync();
            if (eventObj == null)
            {
                throw new InvalidOperationException("Event not found.");
            }

            if (eventObj.AreAnswersAvailableNow)
            {
                throw new UserOperationException("Answers are already available.");
            }

            ValidationResult validationResult = IsMessageTextValid(text);
            if (!validationResult.IsValid)
            {
                throw new UserOperationException(validationResult.FailureReason);
            }

            Puzzle puzzle = await context.Puzzles.Where(p => p.ID == puzzleId).FirstOrDefaultAsync();
            if (puzzle == null)
            {
                throw new InvalidOperationException("Puzzle not found.");
            }

            await ValidatePostPermissionAsync(eventObj, puzzle, isFromGameControl, senderId, teamId, playerId);

            PuzzleUser sender = await context.PuzzleUsers.Where(user => user.ID == senderId).FirstOrDefaultAsync();
            if (sender == null)
            {
                throw new InvalidOperationException("Sender not found.");
            }

            Message message = new Message();
            DateTime now = DateTime.UtcNow;
            message.ThreadId = threadId;
            message.IsFromGameControl = isFromGameControl;
            message.Subject = subject;
            message.EventID = eventId;
            message.Event = eventObj;
            message.CreatedDateTimeInUtc = now;
            message.ModifiedDateTimeInUtc = now;
            message.Text = text;
            message.SenderID = senderId;
            message.Sender = sender;
            message.PuzzleID = puzzleId;
            message.Puzzle = puzzle;
            message.TeamID = teamId;
            message.PlayerID = playerId;

            bool isMessageAdded = false;
            using (IDbContextTransaction transaction = context.Database.BeginTransaction(System.Data.IsolationLevel.Serializable))
            {
                Message newestMessage = await context.Messages
                    .Where(existingMessage => existingMessage.ThreadId == threadId)
                    .OrderBy(existingMessage => existingMessage.CreatedDateTimeInUtc)
                    .LastOrDefaultAsync();

                if (newestMessage == null || newestMessage.Text != text)
                {
                    context.Messages.Add(message);
                    await context.SaveChangesAsync();
                    await transaction.CommitAsync();
                    isMessageAdded = true;
                }
                else
                {
                    message = newestMessage;
                }
            }

            ThreadMessageDTO dto = await BroadcastMessageAsync(message.ID);

            if (isMessageAdded)
            {
                Message savedMessage = await GetMessageAsync(message.ID);
                savedMessage.Event = eventObj;
                await SendEmailNotifications(savedMessage, puzzle, eventObj);
            }

            return dto;
        }

        public async Task<ThreadMessageDTO> EditMessageAsync(int messageId, int currentUserId, string text)
        {
            Message message = await GetMessageAsync(messageId);
            if (message == null || !await IsAllowedToModifyMessageAsync(message, currentUserId))
            {
                throw new UserOperationException("You are not allowed to edit this message.");
            }

            ValidationResult validationResult = IsMessageTextValid(text);
            if (!validationResult.IsValid)
            {
                throw new UserOperationException($"Edit failed because {validationResult.FailureReason}");
            }

            message.Text = text;
            message.ModifiedDateTimeInUtc = DateTime.UtcNow;
            await context.SaveChangesAsync();

            return await BroadcastMessageAsync(message.ID);
        }

        public async Task<ThreadMessageDTO> DeleteMessageAsync(int messageId, int currentUserId)
        {
            Message message = await GetMessageAsync(messageId);
            if (message == null || !await IsAllowedToModifyMessageAsync(message, currentUserId))
            {
                throw new UserOperationException("You are not allowed to delete this message.");
            }

            message.Text = PuzzleThreadModel.DeletedMessage;
            message.ModifiedDateTimeInUtc = DateTime.UtcNow;
            await context.SaveChangesAsync();

            return await BroadcastMessageAsync(message.ID);
        }

        public async Task<ThreadMessageDTO> ClaimThreadAsync(int messageId, int currentUserId)
        {
            Message message = await GetMessageAsync(messageId);
            if (message == null || !await IsAllowedToClaimMessageAsync(message, currentUserId) || message.ClaimerID.HasValue)
            {
                throw new UserOperationException("You cannot claim this thread! It may have already been claimed.");
            }

            message.ClaimerID = currentUserId;
            await context.SaveChangesAsync();

            return await BroadcastMessageAsync(message.ID);
        }

        public async Task<ThreadMessageDTO> UnclaimThreadAsync(int messageId, int currentUserId)
        {
            Message message = await GetMessageAsync(messageId);
            if (message == null || !await IsAllowedToClaimMessageAsync(message, currentUserId))
            {
                throw new UserOperationException("You are not allowed to unclaim this thread.");
            }

            message.ClaimerID = null;
            await context.SaveChangesAsync();

            return await BroadcastMessageAsync(message.ID);
        }

        private async Task<bool> IsAllowedToModifyMessageAsync(Message message, int currentUserId)
        {
            Event eventObj = await context.Events.Where(e => e.ID == message.EventID).FirstOrDefaultAsync();
            return eventObj != null
                && !eventObj.AreAnswersAvailableNow
                && message.SenderID == currentUserId
                && message.Text != PuzzleThreadModel.DeletedMessage;
        }

        private async Task<bool> IsAllowedToClaimMessageAsync(Message message, int currentUserId)
        {
            Event eventObj = await context.Events.Where(e => e.ID == message.EventID).FirstOrDefaultAsync();
            PuzzleUser user = await context.PuzzleUsers.Where(u => u.ID == currentUserId).FirstOrDefaultAsync();
            return eventObj != null
                && user != null
                && (await user.IsAdminForEvent(context, eventObj) || await user.IsAuthorForEvent(context, eventObj));
        }

        private async Task ValidatePostPermissionAsync(Event eventObj, Puzzle puzzle, bool isFromGameControl, int senderId, int? teamId, int? playerId)
        {
            PuzzleUser sender = await context.PuzzleUsers.Where(user => user.ID == senderId).FirstOrDefaultAsync();
            if (sender == null)
            {
                throw new InvalidOperationException("Sender not found.");
            }

            if (isFromGameControl)
            {
                if (await sender.IsAdminForEvent(context, eventObj) || await sender.IsAuthorForEvent(context, eventObj))
                {
                    return;
                }

                throw new UserOperationException("You are not allowed to post to this thread.");
            }

            if (puzzle.IsForSinglePlayer)
            {
                if (playerId != senderId)
                {
                    throw new UserOperationException("You are not allowed to post to this thread.");
                }
            }
            else
            {
                bool isOnTeam = teamId.HasValue && await context.TeamMembers
                    .Where(teamMember => teamMember.Team.ID == teamId.Value && teamMember.Member.ID == senderId)
                    .AnyAsync();
                if (!isOnTeam)
                {
                    throw new UserOperationException("You are not allowed to post to this thread.");
                }
            }
        }

        private async Task<Message> GetMessageAsync(int messageId)
        {
            return await context.Messages
                .Include(m => m.Sender)
                .Include(m => m.Claimer)
                .Where(m => m.ID == messageId)
                .FirstOrDefaultAsync();
        }

        private async Task<ThreadMessageDTO> BroadcastMessageAsync(int messageId)
        {
            Message message = await GetMessageAsync(messageId);
            ThreadMessageDTO dto = ToDto(message);
            await messageHub.SendThreadMessage(dto);
            return dto;
        }

        private static ThreadMessageDTO ToDto(Message message)
        {
            return new ThreadMessageDTO
            {
                ID = message.ID,
                ThreadId = message.ThreadId,
                Text = message.Text,
                CreatedDateTimeInUtc = message.CreatedDateTimeInUtc,
                SenderName = message.Sender?.Name,
                SenderID = message.SenderID,
                IsFromGameControl = message.IsFromGameControl,
                PuzzleID = message.PuzzleID,
                TeamID = message.TeamID,
                PlayerID = message.PlayerID,
                ClaimerID = message.ClaimerID,
                ClaimerName = message.Claimer?.Name
            };
        }

        private async Task SendEmailNotifications(Message newMessage, Puzzle puzzle, Event eventObj)
        {
            string emailTitle = $"{newMessage.Subject} thread update!";
            string emailContent = newMessage.Text;
            string toastTitle = $"Help message from {(newMessage.IsFromGameControl ? "Game Control" : newMessage.Sender.Name)}";
            string toastContent = $"{newMessage.Subject}";
            string threadUrlSuffix = $"Threads/PuzzleThread/{newMessage.PuzzleID}?teamId={newMessage.TeamID}&playerId={newMessage.PlayerID}";

            var recipients = new HashSet<PuzzleUser>();

            if (newMessage.IsFromGameControl)
            {
                // remove the actual response for players, because some players are replying to these mails and nobody is reading those replies.
                emailContent = $"You got a response to your thread \"{newMessage.Subject}\", about the puzzle named ${puzzle.PlaintextName}. Visit the thread to read the reply. Do not reply to this message.";

                Message messageFromPlayer = await context.Messages
                    .Include(message => message.Sender)
                    .Include(message => message.Team)
                    .Where(message => message.ThreadId == newMessage.ThreadId && !message.IsFromGameControl)
                    .FirstOrDefaultAsync();
                if (messageFromPlayer != null)
                {
                    if (puzzle.IsForSinglePlayer)
                    {
                        recipients.Add(messageFromPlayer.Sender);
                        await messageHub.SendNotification(messageFromPlayer.Sender, toastTitle, toastContent, $"/{eventObj.EventID}/play/{threadUrlSuffix}");
                    }
                    else if (messageFromPlayer.TeamID != null)
                    {
                        recipients.AddRange(await context.TeamMembers
                            .Where(teamMember => teamMember.Team.ID == messageFromPlayer.TeamID)
                            .Select(teamMember => teamMember.Member).ToArrayAsync());
                        await messageHub.SendNotification(messageFromPlayer.Team, toastTitle, toastContent, $"/{eventObj.EventID}/play/{threadUrlSuffix}");
                    }
                }
            }
            else if (eventObj.ShouldSendHelpThreadMailToGameControl)
            {
                HashSet<PuzzleUser> staff = new HashSet<PuzzleUser>();
                staff.AddRange(await context.PuzzleAuthors
                    .Where(pa => pa.Puzzle.ID == puzzle.ID)
                    .Select(pa => pa.Author).ToArrayAsync());

                staff.AddRange(await context.Messages
                    .Where(message => message.ThreadId == newMessage.ThreadId && message.IsFromGameControl)
                    .Select(message => message.Sender).ToArrayAsync());

                HashSet<PuzzleUser> admins = new HashSet<PuzzleUser>();
                admins.AddRange(await context.EventAdmins
                    .Where(ea => ea.EventID == eventObj.ID)
                    .Select(ea => ea.Admin).ToArrayAsync());

                recipients.AddRange(staff);

                foreach (var staffer in staff)
                {
                    await messageHub.SendNotification(staffer, toastTitle, toastContent, $"/{eventObj.EventID}/{(admins.Contains(staffer) ? "admin" : "author")}/{threadUrlSuffix}");
                }
            }

            if (recipients.Any())
            {
                MailHelper.Singleton.SendPlaintextWithoutBcc(recipients.Select(r => r.Email), emailTitle, emailContent);
            }
        }

        private static ValidationResult IsMessageTextValid(string messageText)
        {
            if (string.IsNullOrWhiteSpace(messageText))
            {
                return ValidationResult.CreateFailure("Your message cannot be empty.");
            }

            return ValidationResult.CreateSuccess();
        }

        private class ValidationResult
        {
            public bool IsValid { get; set; }

            public string FailureReason { get; set; }

            public static ValidationResult CreateSuccess()
            {
                return new ValidationResult { IsValid = true };
            }

            public static ValidationResult CreateFailure(string reason)
            {
                return new ValidationResult { IsValid = false, FailureReason = reason };
            }
        }
    }
}