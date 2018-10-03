using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ServerCore.DataModel;
using ServerCore.ModelBases;

namespace ServerCore.Pages.Events
{
    public class FastestSolvesModel : EventSpecificPageModel
    {
        private readonly ServerCore.Models.PuzzleServerContext _context;

        public List<PuzzleStats> Puzzles { get; private set; }

        public FastestSolvesModel(ServerCore.Models.PuzzleServerContext context)
        {
            _context = context;
        }

        public async Task OnGetAsync()
        {
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
                    SortOrder = i
                };

                var fastEnum = data.Fastest.GetEnumerator();
                if (fastEnum.MoveNext())
                {
                    stats.FirstID = fastEnum.Current.ID;
                    stats.FirstName = fastEnum.Current.Name;
                    stats.FirstTime = fastEnum.Current.Time;
                }
                if (fastEnum.MoveNext())
                {
                    stats.SecondID = fastEnum.Current.ID;
                    stats.SecondName = fastEnum.Current.Name;
                    stats.SecondTime = fastEnum.Current.Time;
                }
                if (fastEnum.MoveNext())
                {
                    stats.ThirdID = fastEnum.Current.ID;
                    stats.ThirdName = fastEnum.Current.Name;
                    stats.ThirdTime = fastEnum.Current.Time;
                }

                puzzles.Add(stats);
            }

            this.Puzzles = puzzles;
        }

        public class PuzzleStats
        {
            public Puzzle Puzzle;
            public int SolveCount;
            public int SortOrder;
            public int? FirstID;
            public string FirstName;
            public TimeSpan? FirstTime;
            public int? SecondID;
            public string SecondName;
            public TimeSpan? SecondTime;
            public int? ThirdID;
            public string ThirdName;
            public TimeSpan? ThirdTime;
        }
    }
}
