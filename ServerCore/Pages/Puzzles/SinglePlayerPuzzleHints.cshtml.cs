using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Data.Migrations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServerCore.DataModel;
using ServerCore.ModelBases;

namespace ServerCore.Pages.Puzzles
{
    [Authorize(Policy = "PlayerCanSeePuzzle")]
    public class SinglePlayerPuzzleHintsModel : EventSpecificPageModel
    {
        public SinglePlayerPuzzleHintsModel(PuzzleServerContext serverContext, UserManager<IdentityUser> userManager) : base(serverContext, userManager)
        {
        }

        public class HintWithState
        {
            public Hint Hint { get; set; }
            public bool IsUnlocked { get; set; }
            public int Discount { get; set; }

            public int BaseCost { get { return Math.Abs(Hint.Cost); } }
            public int AdjustedCost { get { return Math.Max(0, Math.Abs(Hint.Cost) + Discount); } }
        }

        public string ErrorMessage { get; set; }
        public IList<HintWithState> Hints { get; set; }
        public int PuzzleID { get; set; }
        public string PuzzleName { get; set; }

        public PlayerInEvent PlayerInEvent { get; set; }

        public async Task<IActionResult> OnGetAsync(int puzzleId)
        {
            this.ErrorMessage = null;
            if (!await IsRegisteredUser())
            {
                this.ErrorMessage = "You need to register for the event to see hints!";
            }

            await this.PopulateUI(puzzleId);

            return Page();
        }

        public async Task<IActionResult> OnPostUnlockAsync(int hintID, int puzzleId)
        {
            Hint hint = await (from Hint h in _context.Hints
                               where h.Id == hintID
                               select h).FirstOrDefaultAsync();
            if (hint == null)
            {
                return NotFound();
            }

            SinglePlayerPuzzleHintStatePerPlayer state = await (from SinglePlayerPuzzleHintStatePerPlayer s in _context.SinglePlayerPuzzleHintStatePerPlayer
                                                                where s.HintID == hintID && s.PlayerID == LoggedInUser.ID
                                                                select s).FirstOrDefaultAsync();
            if (state == null)
            {
                throw new Exception($"SinglePlayerPuzzleHintStatePerPlayer missing for player {LoggedInUser.ID} hint {hintID}");
            }

            if (!state.IsUnlocked)
            {
                Hints = await GetHints(puzzleId);
                HintWithState hintToUnlock = Hints.SingleOrDefault(hws => hws.Hint.Id == hintID);
                this.PlayerInEvent = await this.GetPlayer();

                if (this.PlayerInEvent == null || this.PlayerInEvent.HintCoinCount < hintToUnlock.AdjustedCost)
                {
                    return NotFound();
                }

                state.UnlockTime = DateTime.UtcNow;
                this.PlayerInEvent.HintCoinCount -= hintToUnlock.AdjustedCost;
                this.PlayerInEvent.HintCoinsUsed += hintToUnlock.AdjustedCost;
                await _context.SaveChangesAsync();
            }

            await PopulateUI(puzzleId);

            return RedirectToPage(new { puzzleId });
        }

        private async Task<IList<HintWithState>> GetHints(int puzzleID)
        {
            List<HintWithNullableState> hintsWithNullableStates = await (from Hint hint in _context.Hints
                           where hint.Puzzle.ID == puzzleID
                           join SinglePlayerPuzzleHintStatePerPlayer state in _context.SinglePlayerPuzzleHintStatePerPlayer on hint.Id equals state.HintID into leftJoinedTable
                           from nullableHintStatePerPlayer in leftJoinedTable.DefaultIfEmpty()
                           where nullableHintStatePerPlayer == null || nullableHintStatePerPlayer.PlayerID == LoggedInUser.ID
                           orderby hint.DisplayOrder, hint.Description
                           select new HintWithNullableState(hint, nullableHintStatePerPlayer)).ToListAsync();

            // Fill in any hint states we are missing.
            foreach (HintWithNullableState missingState in hintsWithNullableStates.Where(state => state.NullableState == null))
            {
                var newState = new SinglePlayerPuzzleHintStatePerPlayer()
                {
                    Hint = missingState.Hint,
                    PlayerID = LoggedInUser.ID
                };

                _context.SinglePlayerPuzzleHintStatePerPlayer.Add(newState);
                missingState.NullableState = newState;
            }

            await _context.SaveChangesAsync();
            Hints = hintsWithNullableStates
                .Select(item => new HintWithState()
                { 
                    Hint = item.Hint,
                    IsUnlocked = item.NullableState.IsUnlocked 
                })
                .ToList();

            bool isSolved = SinglePlayerPuzzleStateHelper.GetFullReadOnlyQuery(_context, Event, puzzleID, LoggedInUser.ID)
                .Any(puzzleState => puzzleState.SolvedTime.HasValue);

            if (Hints.Count > 0)
            {
                int discount = Hints.Min(hws => (hws.IsUnlocked && hws.Hint.Cost < 0) ? hws.Hint.Cost : 0);

                // During a beta, once a puzzle is solved, all other hints become free.
                // There's no IsBeta flag on an event, so check the name.
                // We can change this in the unlikely event there's a beta-themed hunt.
                bool allHintsFree = isSolved && Event.Name.ToLower().Contains("beta");

                foreach (HintWithState hint in Hints)
                {
                    if (allHintsFree)
                    {
                        hint.Discount = -hint.BaseCost;
                    }
                    else if (hint.Hint.Cost < 0)
                    {
                        hint.Discount = discount;
                    }
                }

                // if the event is over, show all hints
                if (Event.AreAnswersAvailableNow)
                {
                    foreach (HintWithState hint in Hints)
                    {
                        hint.IsUnlocked = true;
                    }
                }
            }
            return Hints;
        }

        private Task<PlayerInEvent> GetPlayer()
        {
            return (from PlayerInEvent player in _context.PlayerInEvent
                   where player.EventId == Event.ID && player.PlayerId == LoggedInUser.ID
                   select player).FirstOrDefaultAsync();
        }

        private async Task PopulateUI(int puzzleId)
        {
            PuzzleID = puzzleId;
            PuzzleName = await (from Puzzle in _context.Puzzles
                                where Puzzle.ID == puzzleId
                                select Puzzle.Name).FirstOrDefaultAsync();
            Hints = await GetHints(puzzleId);

            if (this.PlayerInEvent == null)
            {
                this.PlayerInEvent = await GetPlayer();
            }
        }

        private class HintWithNullableState
        {
            public HintWithNullableState(Hint hint, SinglePlayerPuzzleHintStatePerPlayer nullableState)
            {
                this.Hint = hint;
                this.NullableState = nullableState;
            }

            public Hint Hint { get; set; }

            public SinglePlayerPuzzleHintStatePerPlayer NullableState { get; set; }
        }
    }
}
