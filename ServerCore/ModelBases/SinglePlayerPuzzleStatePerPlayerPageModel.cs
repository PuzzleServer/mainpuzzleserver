using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServerCore.DataModel;

namespace ServerCore.ModelBases
{
    public abstract class SinglePlayerPuzzleStatePerPlayerPageModel : EventSpecificPageModel
    {
        public SortOrder? Sort { get; set; }

        public SinglePlayerPuzzleStatePerPlayerPageModel(
            PuzzleServerContext serverContext,
            UserManager<IdentityUser> userManager)
        : base(serverContext, userManager) { }

        public IList<SinglePlayerPuzzleStatePerPlayer> SinglePlayerPuzzleStatePerPlayer { get; set; }

        protected abstract SortOrder DefaultSort { get; }

        public async Task<IActionResult> InitializeModelAsync(SinglePlayerPuzzleUnlockState unlockState, SortOrder? sort)
        {
            if (unlockState == null)
            {
                return NotFound();
            }

            bool isAuthorOrAdmin = EventRole == EventRole.admin || EventRole == EventRole.author;
            IQueryable<SinglePlayerPuzzleStatePerPlayer> statesQ = SinglePlayerPuzzleStateHelper.GetFullReadOnlyQuery(
                _context,
                Event,
                unlockState.PuzzleID,
                playerId: isAuthorOrAdmin ? null : LoggedInUser.ID,
                author: EventRole == EventRole.admin ? null : LoggedInUser);
            Sort = sort;

            switch (sort ?? DefaultSort)
            {
                case SortOrder.PuzzleAscending:
                    statesQ = statesQ.OrderBy(state => state.Puzzle.Name);
                    break;
                case SortOrder.PuzzleDescending:
                    statesQ = statesQ.OrderByDescending(state => state.Puzzle.Name);
                    break;
                case SortOrder.UserAscending:
                    statesQ = statesQ.OrderBy(state => state.User.Name);
                    break;
                case SortOrder.UserDescending:
                    statesQ = statesQ.OrderByDescending(state => state.User.Name);
                    break;
                case SortOrder.SolveAscending:
                    statesQ = statesQ.OrderBy(state => state.SolvedTime ?? DateTime.MaxValue);
                    break;
                case SortOrder.SolveDescending:
                    statesQ = statesQ.OrderByDescending(state => state.SolvedTime ?? DateTime.MaxValue);
                    break;
                default:
                    throw new ArgumentException($"unknown sort: {sort}");
            }

            SinglePlayerPuzzleStatePerPlayer = await statesQ.Include(puzzleState => puzzleState.User).Include(puzzleState => puzzleState.Puzzle).ToListAsync();

            return Page();
        }

        public SortOrder? SortForColumnLink(SortOrder ascendingSort, SortOrder descendingSort)
        {
            SortOrder result = ascendingSort;

            if (result == (Sort ?? DefaultSort))
            {
                result = descendingSort;
            }

            if (result == DefaultSort)
            {
                return null;
            }

            return result;
        }

        public async Task SetSolveStateAsync(int puzzleId, int? puzzleUserId, bool value)
        {
            await SinglePlayerPuzzleStateHelper.SetSolveStateAsync(
                _context,
                Event,
                puzzleId,
                puzzleUserId,
                value ? (DateTime?)DateTime.UtcNow : null,
                EventRole == EventRole.admin ? null : LoggedInUser);
        }

        public async Task SetEmailModeAsync(Puzzle puzzle, Team team, bool value)
        {
            await PuzzleStateHelper.SetEmailOnlyModeAsync(
                _context,
                Event,
                puzzle,
                team,
                value,
                EventRole == EventRole.admin ? null : LoggedInUser);
        }

        public enum SortOrder
        {
            PuzzleAscending,
            PuzzleDescending,
            UserAscending,
            UserDescending,
            SolveAscending,
            SolveDescending
        }
    }
}
