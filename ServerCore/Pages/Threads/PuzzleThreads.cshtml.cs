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
    public class PuzzleThreadsModel : EventSpecificPageModel
    {
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
        /// A dictionary with key Puzzle ID and value of concatenated list of authors
        /// </summary>
        public Dictionary<int, string> AuthorsForPuzzleID { get; set; }

        /// <summary>
        /// Gets or sets the input parameters passed to GET.
        /// This is needed so that we can replay requests for things like refresh.
        /// </summary>
        public InputGetParameters InputParameters { get; set; }

        /// <summary>
        /// Gets the view for list of all applicable puzzle threads.
        /// 
        /// Note: the puzzleId, teamId, playerId, showUnclaimedOnly, refreshInterval, and filterToSupportingPuzzlesOnly are only used for admin and author views and only one will ever be set at one time.
        /// Players will always see all messages they are allowed to see.
        /// </summary>
        /// <param name="puzzleId">The puzzle id to filter to if applicable.</param>
        /// <param name="teamId">The team id to filter to if applicable.</param>
        /// <param name="playerId">The player id to filter to if applicable.</param>
        /// <param name="showUnclaimedOnly">True if we want to filter only to unclaimed messages.</param>
        /// <param name="refreshInterval">Determines the interval in seconds that we should wait before we refresh the page.</param>
        /// <param name="filterToSupportingPuzzlesOnly">True if we want to filter only to puzzles current user is supporting (listed as author or supporting).</param>
        /// <returns>The Puzzle threads page view.</returns>
        /// <exception cref="NotSupportedException">Thrown when we receive an unexpected event role.</exception>
        public async Task<IActionResult> OnGetAsync(
            int? puzzleId,
            int? teamId,
            int? playerId,
            bool? showUnclaimedOnly,
            int? refreshInterval,
            bool? filterToSupportingPuzzlesOnly)
        {
            if (LoggedInUser == null)
            {
                return Challenge();
            }

            if (refreshInterval.HasValue)
            {
                Refresh = refreshInterval;
            }

            this.InputParameters = new InputGetParameters(
                puzzleId: puzzleId,
                teamId: teamId,
                playerId: playerId,
                showUnclaimedOnly: showUnclaimedOnly,
                refreshInterval: refreshInterval,
                filterToSupportingPuzzlesOnly: filterToSupportingPuzzlesOnly);

            IQueryable<Message> messages = this._context.Messages.Where(message => message.PuzzleID.HasValue
                    && message.EventID == Event.ID);
            this.Title = "All help threads";

            // Filter the messages further based on filters.
            if (puzzleId.HasValue)
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

            if (teamId.HasValue)
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

            if (playerId.HasValue)
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

            // Filter based on roles
            if (EventRole == EventRole.play)
            {
                Team team = await GetTeamAsync();
                messages = messages.Where(message =>
                    (team != null && message.TeamID.Value == team.ID)
                    || message.SenderID == LoggedInUser.ID);
            }
            else if ((EventRole == EventRole.author && Event.ShouldShowHelpMessageOnlyToAuthor)
                || (filterToSupportingPuzzlesOnly.HasValue && filterToSupportingPuzzlesOnly.Value))
            {
                HashSet<int> authorPuzzleIds = UserEventHelper.GetPuzzlesForAuthorAndEvent(_context, Event, LoggedInUser)
                    .Select(puzzle => puzzle.ID)
                    .ToHashSet();

                if (filterToSupportingPuzzlesOnly == true)
                {
                    messages = messages.Where(message => authorPuzzleIds.Contains(message.PuzzleID.Value));
                    this.Title = $"Help threads for puzzles you authored or are supporting";
                }
                else
                {
                    messages = messages.Where(message => authorPuzzleIds.Contains(message.PuzzleID.Value) || message.Puzzle.ShowHelpThreadsToAllGameControl);
                }
            }
            else if (EventRole == EventRole.admin || EventRole == EventRole.author)
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

            ILookup<int, string> puzzleAuthors = (await (from author in _context.PuzzleAuthors
                                                         where author.Puzzle.Event == Event
                                                         select author).ToListAsync()).ToLookup(author => author.PuzzleID, author => author.Author.Name);

            AuthorsForPuzzleID = new Dictionary<int, string>();

            foreach (var message in LatestMessagesFromEachThread)
            {
                if (message.PuzzleID.HasValue)
                {
                    IEnumerable<string> authorsForPuzzle = puzzleAuthors[message.PuzzleID.Value];
                    string authorList = authorsForPuzzle != null ? string.Join(", ", authorsForPuzzle) : "";
                    AuthorsForPuzzleID.Add(message.PuzzleID.Value, authorList);
                }
            }

            return Page();
        }

        public bool IsLatestMessageUnclaimed(Message message)
        {
            return !message.IsFromGameControl && !message.ClaimerID.HasValue;
        }

        /// <summary>
        /// The input parameters passed to GET.
        /// </summary>
        public class InputGetParameters
        {
            public InputGetParameters(
                int? puzzleId,
                int? playerId,
                int? teamId,
                bool? showUnclaimedOnly,
                int? refreshInterval,
                bool? filterToSupportingPuzzlesOnly)
            {
                this.PuzzleId = puzzleId;
                this.PlayerId = playerId;
                this.TeamId = teamId;
                this.ShowUnclaimedOnly = showUnclaimedOnly;
                this.RefreshInterval = refreshInterval;
                this.FilterToSupportingPuzzlesOnly = filterToSupportingPuzzlesOnly;
            }

            public int? TeamId { get; }
            public int? PlayerId { get; }
            public int? PuzzleId { get; }
            public bool? ShowUnclaimedOnly { get; }
            public int? RefreshInterval { get; }
            public bool? FilterToSupportingPuzzlesOnly { get; }

            public bool HasFilterApplied()
            {
                return this.TeamId.HasValue
                    || this.PlayerId.HasValue
                    || this.PuzzleId.HasValue
                    || (this.ShowUnclaimedOnly.HasValue && this.ShowUnclaimedOnly.Value)
                    || (this.FilterToSupportingPuzzlesOnly.HasValue && this.FilterToSupportingPuzzlesOnly.Value);
            }
        }
    }
}
