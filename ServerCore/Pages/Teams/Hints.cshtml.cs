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

namespace ServerCore.Pages.Teams
{
    [Authorize(Policy = "PlayerCanSeePuzzle")]
    [Authorize(Policy = "PlayerIsOnTeam")]
    public class HintsModel : EventSpecificPageModel
    {
        public HintsModel(PuzzleServerContext serverContext, UserManager<IdentityUser> userManager) : base(serverContext, userManager)
        {
        }

        public abstract class HintViewStateBase
        {
            public HintViewStateBase(
                string description,
                string content,
                int baseCost,
                bool isUnlocked)
            {
                this.Description = description;
                this.Content = content;
                this.BaseCost = baseCost;
                this.IsUnlocked = isUnlocked;
            }

            public string Description { get; }
            public string Content { get; }
            public bool IsUnlocked { get; set; }
            public int Discount { get; set; }
            public int BaseCost { get; }
            public int AdjustedCost { get { return Math.Max(0, Math.Abs(this.BaseCost) + Discount); } }
        }

        public class HintViewState : HintViewStateBase
        {
            public HintViewState(Hint hint, bool isUnlocked)
                : base(hint.Description, hint.Content, Math.Abs(hint.Cost), isUnlocked)
            {
                this.Id = hint.Id;
            }

            public int Id { get; }
        }

        public class PuzzleThreadViewState : HintViewStateBase
        {
            public PuzzleThreadViewState(string description, string content, int baseCost, bool isUnlocked)
                : base(description, content, baseCost, isUnlocked)
            {
            }
        }

        public IList<HintViewStateBase> HintViewStates { get; set; }
        public Team Team { get; set; }
        public int PuzzleID { get; set; }
        public string PuzzleName { get; set; }
        public PuzzleThreadViewState PuzzleThreadState { get; set; }

        public async Task<IActionResult> OnGetAsync(int puzzleId, int teamId)
        {
            PuzzleID = puzzleId;

            Team = await (from Team t in _context.Teams
                          where t.ID == teamId
                          select t).FirstOrDefaultAsync();
            if (Team == null)
            {
                return NotFound();
            }

            await PopulateUI(puzzleId, teamId);

            return Page();
        }

        private async Task<List<HintViewStateBase>> GetAllTeamHints(Puzzle puzzle, int teamID)
        {
            List<HintViewStateBase> hints = await (
                from Hint hint in _context.Hints
                join HintStatePerTeam state in _context.HintStatePerTeam on hint.Id equals state.HintID
                where state.TeamID == teamID && hint.Puzzle.ID == puzzle.ID
                orderby hint.DisplayOrder, hint.Description
                select new HintViewState(hint, state.IsUnlocked)).ToListAsync<HintViewStateBase>();

            PuzzleStateBase puzzleStateBase = await (
                from puzzleState in _context.PuzzleStatePerTeam
                where puzzleState.PuzzleID == puzzle.ID && puzzleState.TeamID == teamID
                select puzzleState).FirstOrDefaultAsync();

            if (Event.DefaultCostForHelpThread >= 0)
            {
                hints.Add(new PuzzleThreadViewState(
                    description: "Ask questions to the author directly",
                    content: string.Empty,
                    baseCost: puzzle.CostForHelpThread.HasValue ? puzzle.CostForHelpThread.Value : Event.DefaultCostForHelpThread,
                    isUnlocked: puzzleStateBase != null && puzzleStateBase.IsHelpThreadUnlockedByCoins));
            }

            bool solved = puzzleStateBase?.SolvedTime != null;

            if (hints.Count > 0)
            {
                int discount = hints.Min(hws => (hws.IsUnlocked && hws.BaseCost < 0) ? hws.BaseCost : 0);

                // During a beta, once a puzzle is solved, all other hints become free.
                // There's no IsBeta flag on an event, so check the name.
                // We can change this in the unlikely event there's a beta-themed hunt.
                bool allHintsFree = solved && Event.Name.ToLower().Contains("beta");

                foreach (HintViewStateBase hint in hints)
                {
                    if (allHintsFree)
                    {
                        hint.Discount = -hint.BaseCost;
                    }
                    else if (hint.BaseCost < 0)
                    {
                        hint.Discount = discount;
                    }
                }

                // if the event is over, show all hints
                if (Event.AreAnswersAvailableNow && Team.Name.Contains("Archive"))
                {
                    foreach (HintViewStateBase hint in hints)
                    {
                        hint.IsUnlocked = true;
                    }
                }
            }
            return hints;
        }

        private async Task PopulateUI(int puzzleId, int teamId)
        {
            Puzzle puzzle = await (from Puzzle in _context.Puzzles
                where Puzzle.ID == puzzleId
                select Puzzle).FirstOrDefaultAsync();

            PuzzleName = puzzle?.PlaintextName;

            bool shouldShowThreadOption = Event.DefaultCostForHelpThread >= 0;
            HintViewStates = await GetAllTeamHints(puzzle, teamId);
        }

        public async Task<IActionResult> OnPostUnlockHintAsync(int hintID, int puzzleId, int teamId)
        {
            Team = await (from Team t in _context.Teams
                               where t.ID == teamId
                               select t).FirstOrDefaultAsync();
            if (Team == null)
            {
                return NotFound();
            }

            Hint hint = await (from Hint h in _context.Hints
                               where h.Id == hintID
                               select h).FirstOrDefaultAsync();
            if (hint == null)
            {
                return NotFound();
            }

            HintStatePerTeam state = await (from HintStatePerTeam s in _context.HintStatePerTeam
                                            where s.HintID == hintID && s.TeamID == teamId
                                            select s).FirstOrDefaultAsync();
            if(state == null)
            {
                throw new Exception($"HintStatePerTeam missing for team {teamId} hint {hintID}");
            }

            if (!state.IsUnlocked)
            {
                Puzzle puzzle = await (from Puzzle in _context.Puzzles
                    where Puzzle.ID == puzzleId
                    select Puzzle).FirstOrDefaultAsync();

                IList<HintViewStateBase> hintViewStates = await GetAllTeamHints(puzzle, teamId);
                HintViewState hintToUnlock = hintViewStates.SingleOrDefault(hws => (hws as HintViewState)?.Id == hintID) as HintViewState;

                if (hintToUnlock == null || Team.HintCoinCount < hintToUnlock.AdjustedCost)
                {
                    return NotFound();
                }

                state.UnlockTime = DateTime.UtcNow;
                Team.HintCoinCount -= hintToUnlock.AdjustedCost;
                Team.HintCoinsUsed += hintToUnlock.AdjustedCost;
                await _context.SaveChangesAsync();
            }

            return RedirectToPage(new { puzzleId, teamId });
        }

        public async Task<IActionResult> OnPostUnlockPuzzleThreadAsync(int puzzleId, int teamId)
        {
            Team = await (from Team t in _context.Teams
                          where t.ID == teamId
                          select t).FirstOrDefaultAsync();
            if (Team == null)
            {
                return NotFound();
            }

            PuzzleStateBase state = await (
                from puzzleState in _context.PuzzleStatePerTeam
                where puzzleState.PuzzleID == puzzleId && puzzleState.TeamID == teamId
                select puzzleState).FirstOrDefaultAsync();

            if (state == null)
            {
                throw new Exception($"PuzzleStatePerTeam missing for puzzle {puzzleId} and team {teamId}");
            }

            if (!state.IsHelpThreadUnlockedByCoins)
            {
                Puzzle puzzle = await (from Puzzle in _context.Puzzles
                    where Puzzle.ID == puzzleId
                    select Puzzle).FirstOrDefaultAsync();

                IList<HintViewStateBase> hintViewStates = await GetAllTeamHints(puzzle, teamId);
                PuzzleThreadViewState puzzleThreadToUnlock = hintViewStates
                    .Select(hintViewState => hintViewState as PuzzleThreadViewState)
                    .SingleOrDefault(hintViewState => hintViewState != null);

                if (puzzleThreadToUnlock == null || Team.HintCoinCount < puzzleThreadToUnlock.AdjustedCost)
                {
                    return NotFound();
                }

                state.IsHelpThreadUnlockedByCoins = true;
                Team.HintCoinCount -= puzzleThreadToUnlock.AdjustedCost;
                Team.HintCoinsUsed += puzzleThreadToUnlock.AdjustedCost;
                await _context.SaveChangesAsync();
            }

            return RedirectToPage(new { puzzleId, teamId });
        }
    }
}
