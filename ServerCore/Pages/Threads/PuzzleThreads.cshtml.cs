using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ServerCore.DataModel;
using ServerCore.Helpers;
using ServerCore.ModelBases;
using System.Collections.Generic;
using System;

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
        /// The team of the current user. 
        /// This value will only be filled in the play route and if the player actually has a team.
        /// </summary>
        public Team Team { get; set; }

        /// <summary>
        /// Gets or sets the list of thread views.
        /// </summary>
        public List<Message> LatestMessagesFromEachThread { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            if (LoggedInUser == null)
            {
                return Challenge();
            }

            IEnumerable<Message> messages = this._context.Messages.Where(message => message.Puzzle != null
                    && message.Puzzle.EventID == Event.ID);

            if (EventRole == EventRole.play)
            {
                Team = await GetTeamAsync();
                messages = messages.Where(message => 
                    (Team != null && message.TeamID.Value == Team.ID) 
                    || message.SenderID == LoggedInUser.ID);
            }
            else if (EventRole == EventRole.author)
            {
                HashSet<int> authorPuzzleIds = UserEventHelper.GetPuzzlesForAuthorAndEvent(_context, Event, LoggedInUser)
                    .Select(puzzle => puzzle.ID)
                    .ToHashSet();
                messages = messages.Where(message => authorPuzzleIds.Contains(message.Puzzle.ID));
            }
            else if (EventRole != EventRole.admin)
            {
                throw new NotSupportedException($"EventRole [{EventRole}] is not supported in PuzzleThreads");
            }

            LatestMessagesFromEachThread = messages
                .GroupBy(message => message.ThreadId)
                .Select(group => group.OrderByDescending(message => message.CreatedDateTimeInUtc).First())
                .ToList();

            return Page();
        }
    }
}
