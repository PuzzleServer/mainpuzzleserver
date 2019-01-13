using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ServerCore.DataModel;
using ServerCore.ModelBases;

namespace ServerCore.Pages.Events
{
    public class FastestSolvesModel : EventSpecificPageModel
    {
        public List<PuzzleStats> Puzzles { get; private set; }

        public FastestSolvesModel(PuzzleServerContext serverContext, UserManager<IdentityUser> userManager) : base(serverContext, userManager)
        {
        }

        public async Task OnGetAsync()
        {
            // get the page data: puzzle, solve count, top three fastest
            var puzzlesData = await PuzzleStateHelper.GetSparseQuery(_context, this.Event, null, null)
                .Where(s => s.SolvedTime != null && s.Puzzle.IsPuzzle)
                .GroupBy(state => state.Puzzle)
                .Select(g => new {
                    Puzzle = g.Key,
                    SolveCount = g.Count(),
                    Fastest = g.OrderBy(s => s.SolvedTime - s.UnlockedTime).Take(3).Select(s => new { s.Team.ID, s.Team.Name, Time = s.SolvedTime - s.UnlockedTime})
                })
                .OrderByDescending(p => p.SolveCount).ThenBy(p => p.Puzzle.Name)
                .ToListAsync();

            var puzzles = new List<PuzzleStats>(puzzlesData.Count);
            for (int i = 0; i < puzzlesData.Count; i++)
            {
                var data = puzzlesData[i];
                var stats = new PuzzleStats()
                {
                    Puzzle = data.Puzzle,
                    SolveCount = data.SolveCount,
                    SortOrder = i,
                    Fastest = data.Fastest.Select(f => new FastRecord() { ID = f.ID, Name = f.Name, Time = f.Time }).ToArray()
                };

                puzzles.Add(stats);
            }

            this.Puzzles = puzzles;
        }

        public class PuzzleStats
        {
            public Puzzle Puzzle;
            public int SolveCount;
            public int SortOrder;
            public FastRecord[] Fastest;
        }

        public class FastRecord
        {
            public int ID;
            public string Name;
            public TimeSpan? Time;
        }
    }
}
