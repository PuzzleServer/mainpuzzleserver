﻿using System;
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

        public class HintWithState
        {
            public Hint Hint { get; set; }
            public bool IsUnlocked { get; set; }
            public int Discount { get; set; }

            public int BaseCost { get { return Math.Abs(Hint.Cost); } }
            public int AdjustedCost { get { return Math.Max(0, Math.Abs(Hint.Cost) + Discount); } }
        }

        public IList<HintWithState> Hints { get; set; }
        public Team Team { get; set; }
        public int PuzzleID { get; set; }
        public string PuzzleName { get; set; }

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

        private async Task<IList<HintWithState>> GetAllTeamHints(int puzzleID, int teamID)
        {
            Hints = await (from Hint hint in _context.Hints
                             join HintStatePerTeam state in _context.HintStatePerTeam on hint.Id equals state.HintID
                             where state.TeamID == teamID && hint.Puzzle.ID == puzzleID
                             orderby hint.DisplayOrder, hint.Description
                             select new HintWithState { Hint = hint, IsUnlocked = state.IsUnlocked }).ToListAsync();
            bool solved = await PuzzleStateHelper.IsPuzzleSolved(_context, puzzleID, teamID);

            if (Hints.Count > 0)
            {
                int discount = Hints.Min(hws => (hws.IsUnlocked && hws.Hint.Cost < 0) ? hws.Hint.Cost : 0);

                // During a beta, once a puzzle is solved, all other hints become free.
                // There's no IsBeta flag on an event, so check the name.
                // We can change this in the unlikely event there's a beta-themed hunt.
                bool allHintsFree = solved && Event.Name.ToLower().Contains("beta");

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
                if (Event.AreAnswersAvailableNow && Team.Name.Contains("Archive"))
                {
                    foreach (HintWithState hint in Hints)
                    {
                        hint.IsUnlocked = true;
                    }
                }
            }
            return Hints;
        }

        private async Task PopulateUI(int puzzleId, int teamId)
        {
            PuzzleName = await (from Puzzle in _context.Puzzles
                                where Puzzle.ID == puzzleId
                                select Puzzle.Name).FirstOrDefaultAsync();
            Hints = await GetAllTeamHints(puzzleId, teamId);
        }

        public async Task<IActionResult> OnPostUnlockAsync(int hintID, int puzzleId, int teamId)
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
                Hints = await GetAllTeamHints(puzzleId, teamId);
                HintWithState hintToUnlock = Hints.SingleOrDefault(hws => hws.Hint.Id == hintID);

                if (Team.HintCoinCount < hintToUnlock.AdjustedCost)
                {
                    return NotFound();
                }

                state.UnlockTime = DateTime.UtcNow;
                Team.HintCoinCount -= hintToUnlock.AdjustedCost;
                Team.HintCoinsUsed += hintToUnlock.AdjustedCost;
                await _context.SaveChangesAsync();
            }

            await PopulateUI(puzzleId, teamId);
            
            return RedirectToPage(new { puzzleId, teamId });
        }
    }
}
