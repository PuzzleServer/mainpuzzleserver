using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ServerCore.DataModel;
using ServerCore.ModelBases;

namespace ServerCore.Pages.Teams
{
    public class HintsModel : EventSpecificPageModel
    {
        private readonly PuzzleServerContext _context;

        public HintsModel(PuzzleServerContext context)
        {
            _context = context;
        }

        public IList<Hint> UnlockedHints { get; set; }
        public IList<Hint> LockedHints { get; set; }
        public Team Team { get; set; }
        public int PuzzleID { get; set; }

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
            var allHints =
                from Hint hint in _context.Hints
                join HintStatePerTeam state in _context.HintStatePerTeam on hint.Id equals state.HintID
                where state.TeamID == teamID && hint.Puzzle.ID == puzzleID
                orderby hint.DisplayOrder
                select new { Hint = hint, State = state };

            UnlockedHints = await (from hintState in allHints
                                   where hintState.State.IsUnlocked
                                   select hintState.Hint).ToListAsync();

            LockedHints = await (from hintState in allHints
                                 where !hintState.State.IsUnlocked
                                 select hintState.Hint).ToListAsync();
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
                await _context.SaveChangesAsync();
            }

            await PopulateUI(puzzleID, teamID);
            
            return Page();
        }
    }
}
