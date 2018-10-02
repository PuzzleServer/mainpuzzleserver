﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ServerCore.DataModel;
using ServerCore.ModelBases;

namespace ServerCore.Pages.Events
{
    public class StandingsModel : EventSpecificPageModel
    {
        private readonly ServerCore.Models.PuzzleServerContext _context;

        public List<TeamStats> Teams { get; private set; }

        public StandingsModel(ServerCore.Models.PuzzleServerContext context)
        {
            _context = context;
        }

        public async Task OnGetAsync()
        {
            var teamsData = await PuzzleStateHelper.GetSparseQuery(_context, this.Event, null, null)
                .Where(s => s.SolvedTime != null)
                .GroupBy(state => state.Team)
                .Select(g => new {
                    Team = g.Key,
                    SolveCount = g.Count(),
                    Score = g.Sum(s => s.Puzzle.SolveValue),
                    FinalMetaSolveTime = g.Where(s => s.Puzzle.IsFinalPuzzle).Select(s => s.SolvedTime).FirstOrDefault()
                })
                .OrderBy(t => t.FinalMetaSolveTime).ThenByDescending(t => t.Score).ThenByDescending(t => t.SolveCount).ThenBy(t => t.Team.Name)
                .ToListAsync();

            var teams = new List<TeamStats>(teamsData.Count);
            for (int i = 0; i < teamsData.Count; i++)
            {
                var data = teamsData[i];
                teams.Add(new TeamStats() { Team = data.Team, SolveCount = data.SolveCount, Score = data.Score, SortOrder = i, FinalMetaSolveTime = data.FinalMetaSolveTime ?? DateTime.MaxValue });
            }

            this.Teams = teams;
        }

        public class TeamStats
        {
            public Team Team;
            public int SolveCount;
            public int Score;
            public int SortOrder;
            public DateTime FinalMetaSolveTime = DateTime.MaxValue;
        }
    }
}
