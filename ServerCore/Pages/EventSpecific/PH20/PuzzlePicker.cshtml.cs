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

namespace ServerCore.Pages.EventSpecific.PH20
{
    /// <summary>
    /// Page for choosing between different whistle stop options. This is done in the context
    /// of a "choice" puzzle so that this page can be locked and unlocked
    /// </summary>
    [Authorize(Policy = "PlayerCanSeePuzzle")]
    public class PuzzlePickerModel : EventSpecificPageModel
    {
        public PuzzlePickerModel(PuzzleServerContext serverContext, UserManager<IdentityUser> manager) : base(serverContext, manager)
        {
        }

        public Puzzle CurrentPuzzle { get; set; }
        public List<Puzzle> PuzzleOptions { get; set; }
        public int PrereqPuzzleId { get; set; }

        public async Task<IActionResult> OnGet(int puzzleId)
        {
            PrereqPuzzleId = puzzleId;

            Puzzle puzzle = await (from puzz in _context.Puzzles
                                   where puzz.ID == puzzleId
                                   select puzz).FirstOrDefaultAsync();
            if (puzzle == null)
            {
                return NotFound();
            }

            // Restrict this page to whistle stop non-puzzles
            if (puzzle.MinutesOfEventLockout == 0 || puzzle.IsPuzzle)
            {
                return NotFound();
            }

            Team team = await UserEventHelper.GetTeamForPlayer(_context, Event, LoggedInUser);
            var puzzleStateQuery = PuzzleStateHelper.GetFullReadOnlyQuery(_context, Event, null, team);
            PuzzleStatePerTeam state = await (from pspt in puzzleStateQuery
                                        where pspt.PuzzleID == puzzle.ID
                                        select pspt).FirstOrDefaultAsync();
            if (state == null)
            {
                return NotFound();
            }

            // Only move forward if the puzzle is open and unsolved
            if (state.UnlockedTime == null || state.SolvedTime != null)
            {
                return NotFound();
            }

            CurrentPuzzle = puzzle;

            // Treat all locked puzzles where this is a prerequisite as puzzles that can be unlocked
            PuzzleOptions = await (from pspt in puzzleStateQuery
                                   join prereq in _context.Prerequisites on pspt.PuzzleID equals prereq.PuzzleID
                                   where prereq.PrerequisiteID == puzzle.ID &&
                                   pspt.SolvedTime == null && pspt.UnlockedTime == null
                                   select prereq.Puzzle).ToListAsync();

            return Page();
        }

        public async Task<IActionResult> OnPostUnlock(int puzzleId, int unlockId)
        {
            Puzzle puzzle = await (from puzz in _context.Puzzles
                                   where puzz.ID == puzzleId
                                   select puzz).FirstOrDefaultAsync();
            if (puzzle == null)
            {
                return NotFound();
            }

            // Restrict this page to whistle stop non-puzzles
            if (puzzle.MinutesOfEventLockout == 0 || puzzle.IsPuzzle)
            {
                return NotFound();
            }

            Team team = await UserEventHelper.GetTeamForPlayer(_context, Event, LoggedInUser);

            using (var transaction = _context.Database.BeginTransaction())
            {
                var puzzleStateQuery = PuzzleStateHelper.GetFullReadOnlyQuery(_context, Event, null, team);
                PuzzleStatePerTeam prereqState = await (from pspt in puzzleStateQuery
                                                        where pspt.PuzzleID == puzzle.ID
                                                        select pspt).FirstOrDefaultAsync();
                if (prereqState == null)
                {
                    return NotFound();
                }

                // Only move forward if the prereq is open and unsolved
                if (prereqState.UnlockedTime == null || prereqState.SolvedTime != null)
                {
                    return NotFound("Your team has already chosen and can't choose again");
                }

                PuzzleStatePerTeam unlockState = await (from pspt in puzzleStateQuery
                                                        where pspt.PuzzleID == unlockId
                                                        select pspt).FirstOrDefaultAsync();
                if (unlockState == null)
                {
                    return NotFound();
                }

                // The chosen puzzle must be locked (and unsolved)
                if (unlockState.UnlockedTime != null || unlockState.SolvedTime != null)
                {
                    return NotFound("You've already chosen this puzzle");
                }

                // Ensure the puzzle is actually one of the unlock options
                Prerequisites prereq = await (from pre in _context.Prerequisites
                                              where pre.PrerequisiteID == puzzleId && pre.PuzzleID == unlockId
                                              select pre).FirstOrDefaultAsync();
                if (prereq == null)
                {
                    return NotFound();
                }

                await PuzzleStateHelper.SetSolveStateAsync(_context, Event, puzzle, team, DateTime.UtcNow);
                await PuzzleStateHelper.SetUnlockStateAsync(_context, Event, unlockState.Puzzle, team, DateTime.UtcNow);
                transaction.Commit();
            }

            Puzzle puzzleToUnlock = await (from p in _context.Puzzles
                                           where p.ID == unlockId
                                           select p).FirstOrDefaultAsync();

            string puzzleUrl;

            if (puzzleToUnlock.CustomURL != null)
            {
                puzzleUrl = PuzzleHelper.GetFormattedUrl(puzzleToUnlock, Event.ID);
            }
            else
            {
                puzzleUrl = puzzleToUnlock.PuzzleFile.UrlString;
            }

            return Redirect(puzzleUrl);
        }
    }
}