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
    public class HintsModel : EventSpecificPageModel
    {
        public HintsModel(PuzzleServerContext serverContext, UserManager<IdentityUser> userManager) : base(serverContext, userManager)
        {
        }

        public class HintWithState
        {
            public Hint Hint { get; set; }
            public bool IsUnlocked { get; set; }
        }

        public IList<HintWithState> Hints { get; set; }
        public Team Team { get; set; }
        public int PuzzleID { get; set; }
        public string PuzzleName { get; set; }

        public async Task<IActionResult> OnGetAsync(int puzzleID, int teamID)
        {
            PuzzleID = puzzleID;

            Team = await (from Team t in _context.Teams
                          where t.ID == teamID
                          select t).FirstOrDefaultAsync();
            if (Team == null)
            {
                return NotFound();
            }

            await PopulateUI(puzzleID, teamID);

            return Page();
        }

        private async Task PopulateUI(int puzzleID, int teamID)
        {
            PuzzleName = await (from Puzzle in _context.Puzzles
                                where Puzzle.ID == puzzleID
                                select Puzzle.Name).FirstOrDefaultAsync();
            Hints = await
                (from Hint hint in _context.Hints
                 join HintStatePerTeam state in _context.HintStatePerTeam on hint.Id equals state.HintID
                 where state.TeamID == teamID && hint.Puzzle.ID == puzzleID
                 orderby hint.DisplayOrder, hint.Description
                 select new HintWithState { Hint = hint, IsUnlocked = state.IsUnlocked }).ToListAsync();
        }

        public async Task<IActionResult> OnPostUnlockAsync(int hintID, int puzzleID, int teamID)
        {
            Team = await (from Team t in _context.Teams
                               where t.ID == teamID
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
                                            where s.HintID == hintID && s.TeamID == teamID
                                            select s).FirstOrDefaultAsync();
            if(state == null)
            {
                throw new Exception($"HintStatePerTeam missing for team {teamID} hint {hintID}");
            }

            if (!state.IsUnlocked)
            {
                int cost = hint.Cost;
                if (Team.HintCoinCount < cost)
                {
                    return NotFound();
                }

                state.UnlockTime = DateTime.UtcNow;
                Team.HintCoinCount -= cost;
                Team.HintCoinsUsed += cost;
                await _context.SaveChangesAsync();
            }

            await PopulateUI(puzzleID, teamID);
            
            return RedirectToPage(new { puzzleID, teamID });
        }
    }
}
