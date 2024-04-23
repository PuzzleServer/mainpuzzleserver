using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServerCore.DataModel;
using ServerCore.ModelBases;

namespace ServerCore.Pages.Threads
{
    [AllowAnonymous]
    public class PuzzleThreadsModel : EventSpecificPageModel
    {
        public const int PreviewTextLength = 100;

        public PuzzleThreadsModel(PuzzleServerContext serverContext, UserManager<IdentityUser> userManager) : base(serverContext, userManager)
        {
        }

        /// <summary>
        /// The title of the page.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the list of thread views.
        /// </summary>
        public List<Message> LatestMessagesFromEachThread { get; set; }

        /// <summary>
        /// Gets the view for list of all applicable puzzle threads.
        /// 
        /// Note: the puzzleId, teamId, and playerId are only used for admina and author views and only one will ever be set at one time.
        /// Players will always see all messages they are allowed to see.
        /// </summary>
        /// <param name="puzzleId">The puzzle id to filter to if applicable.</param>
        /// <param name="teamId">The team id to filter to if applicable.</param>
        /// <param name="playerId">The player id to filter to if applicable.</param>
        /// <param name="showUnclaimedOnly">True if we want to filter only to unclaimed messages.</param>
        /// <returns>The Puzzle threads page view.</returns>
        /// <exception cref="NotSupportedException">Thrown when we receive an unexpected event role.</exception>
        public async Task<IActionResult> OnGetAsync(int? puzzleId, int? teamId, int? playerId, bool? showUnclaimedOnly)
        {
            if (LoggedInUser == null)
            {
                return Challenge();
            }

            IQueryable<Message> messages = this._context.Messages.Where(message => message.Puzzle != null
                    && message.Puzzle.EventID == Event.ID);
            this.Title = "All help threads";

            // Based on the event role, filter the messages further.
            if (EventRole == EventRole.play)
            {
                Team team = await GetTeamAsync();
                messages = messages.Where(message => 
                    (team != null && message.TeamID.Value == team.ID) 
                    || message.SenderID == LoggedInUser.ID);
            }
            else if (puzzleId.HasValue)
            {
                Puzzle puzzle = await this._context.Puzzles
                    .Where(puzzle => puzzle.EventID == this.Event.ID && puzzle.ID == puzzleId.Value)
                    .FirstOrDefaultAsync();
                if (puzzle == null)
                {
                    return NotFound();
                }

                messages = messages.Where(message => message.PuzzleID == puzzleId.Value);
                this.Title = $"Help threads for puzzle {puzzle.Name}";
            }
            else if (teamId.HasValue)
            {
                Team team = await this._context.Teams
                    .Where(team => team.EventID == this.Event.ID && team.ID == teamId.Value)
                    .FirstOrDefaultAsync();
                if (team == null)
                {
                    return NotFound();
                }

                messages = messages.Where(message => message.TeamID == teamId.Value);
                this.Title = $"Help threads for team {team.Name}";
            }
            else if (playerId.HasValue)
            {
                PuzzleUser player = await this._context.PuzzleUsers
                    .Where(user => user.ID == playerId.Value)
                    .FirstOrDefaultAsync();
                if (player == null)
                {
                    return NotFound();
                }

                messages = messages.Where(message => message.PlayerID == playerId.Value);
                this.Title = $"Help threads for player {player.Name}";
            }
            else if (EventRole == EventRole.author)
            {
                // TODO 969: For things like puzzlehunt, we don't want authors seeing each other's threads. We need to add an event level flag for this.
                /*HashSet<int> authorPuzzleIds = UserEventHelper.GetPuzzlesForAuthorAndEvent(_context, Event, LoggedInUser)
                    .Select(puzzle => puzzle.ID)
                    .ToHashSet();
                messages = messages.Where(message => authorPuzzleIds.Contains(message.Puzzle.ID));*/
            }
            else if (EventRole == EventRole.admin)
            {
                // Admins should be able to see everything, so no further filterning needs to be done
                // no-op;
            }
            else
            {
                throw new NotSupportedException($"EventRole [{EventRole}] is not supported in PuzzleThreads");
            }

            // Filter down to only latest messages
            LatestMessagesFromEachThread = await messages
                .GroupBy(message => message.ThreadId)
                .Select(group => group.OrderByDescending(message => message.CreatedDateTimeInUtc).First())
                .ToListAsync();

            if (showUnclaimedOnly.HasValue && showUnclaimedOnly.Value)
            {
                this.Title += " (unclaimed only)";
                LatestMessagesFromEachThread = LatestMessagesFromEachThread
                    .Where(IsLatestMessageUnclaimed)
                    .ToList();
            }

            return Page();
        }

        public bool IsLatestMessageUnclaimed(Message message)
        {
            return !message.IsFromGameControl && !message.ClaimerID.HasValue;
        }
    }
}
