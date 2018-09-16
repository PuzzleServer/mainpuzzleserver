using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ServerCore.DataModel;
using ServerCore.ModelBases;
using ServerCore.Models;

namespace ServerCore.Pages.Teams
{
    public class PlayModel : EventSpecificPageModel
    {
        private readonly ServerCore.Models.PuzzleServerContext _context;

        public PlayModel(ServerCore.Models.PuzzleServerContext context)
        {
            _context = context;
        }

        public IList<PuzzleWithState> PuzzlesWithState { get;set; }

        public async Task OnGetAsync(int id)
        {
            // all puzzles for this event that are real puzzles
            var puzzlesInEventQ = _context.Puzzles.Where(puzzle => puzzle.Event.ID == this.Event.ID && puzzle.IsPuzzle);

            // all puzzle states for this team that are unlocked (note: IsUnlocked bool is going to harm perf, just null check the time here)
            var stateForTeamQ = _context.PuzzleStatePerTeam.Where(state => state.TeamID == id && state.UnlockedTime != null);

            // join 'em (note: just getting all properties for max flexibility, can pick and choose columns for perf later)
            var visiblePuzzlesQ = puzzlesInEventQ.Join(stateForTeamQ, (puzzle => puzzle.ID), (state => state.PuzzleID), (puzzle, state) => new PuzzleWithState(puzzle, state));

            PuzzlesWithState = await visiblePuzzlesQ.ToListAsync();
        }

        public class PuzzleWithState
        {
            public Puzzle Puzzle { get; }
            public PuzzleStatePerTeam State { get; }

            public PuzzleWithState(Puzzle puzzle, PuzzleStatePerTeam state)
            {
                this.Puzzle = puzzle;
                this.State = state;
            }
        }
    }
}
