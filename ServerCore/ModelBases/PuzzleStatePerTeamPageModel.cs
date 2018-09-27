﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ServerCore.DataModel;
using ServerCore.Models;

namespace ServerCore.ModelBases
{
    public abstract class PuzzleStatePerTeamPageModel : EventSpecificPageModel
    {
        protected PuzzleServerContext Context { get; }

        public SortOrder? Sort { get; set; }

        public PuzzleStatePerTeamPageModel(ServerCore.Models.PuzzleServerContext context)
        {
            this.Context = context;
        }

        public IList<PuzzleStatePerTeam> PuzzleStatePerTeam { get; set; }

        protected abstract SortOrder DefaultSort { get; }

        public async Task InitializeModelAsync(int? puzzleId, int? teamId, SortOrder? sort)
        {
            if (puzzleId.HasValue)
            {
                await this.EnsureStateForPuzzleAsync(puzzleId.Value);
            }

            if (teamId.HasValue)
            {
                await this.EnsureStateForTeamAsync(teamId.Value);
            }

            var statesQ = this.GetPuzzleStatePerTeamQuery(puzzleId, teamId);
            this.Sort = sort;

            switch(sort ?? this.DefaultSort)
            {
                case SortOrder.PuzzleAscending:
                    statesQ = statesQ.OrderBy(state => state.Puzzle.Name);
                    break;
                case SortOrder.PuzzleDescending:
                    statesQ = statesQ.OrderByDescending(state => state.Puzzle.Name);
                    break;
                case SortOrder.TeamAscending:
                    statesQ = statesQ.OrderBy(state => state.Team.Name);
                    break;
                case SortOrder.TeamDescending:
                    statesQ = statesQ.OrderByDescending(state => state.Team.Name);
                    break;
                case SortOrder.UnlockAscending:
                    statesQ = statesQ.OrderBy(state => state.UnlockedTime ?? DateTime.MaxValue);
                    break;
                case SortOrder.UnlockDescending:
                    statesQ = statesQ.OrderByDescending(state => state.UnlockedTime ?? DateTime.MaxValue);
                    break;
                case SortOrder.SolveAscending:
                    statesQ = statesQ.OrderBy(state => state.SolvedTime ?? DateTime.MaxValue);
                    break;
                case SortOrder.SolveDescending:
                    statesQ = statesQ.OrderByDescending(state => state.SolvedTime ?? DateTime.MaxValue);
                    break;
                default:
                    throw new ArgumentException($"unknown sort: {sort}");
            }

            PuzzleStatePerTeam = await statesQ.ToListAsync();
        }

        public SortOrder? SortForColumnLink(SortOrder ascendingSort, SortOrder descendingSort)
        {
            SortOrder result = ascendingSort;

            if (result == (this.Sort ?? DefaultSort))
            {
                result = descendingSort;
            }

            if (result == DefaultSort)
            {
                return null;
            }

            return result;
        }

        public async Task SetUnlockStateAsync(int? puzzleId, int? teamId, bool value)
        {
            var states = await this.GetPuzzleStatePerTeamQuery(puzzleId, teamId).ToListAsync();

            for (int i = 0; i < states.Count; i++)
            {
                states[i].IsUnlocked = value;
            }
            await Context.SaveChangesAsync();
        }

        public async Task SetSolveStateAsync(int? puzzleId, int? teamId, bool value)
        {
            var states = await this.GetPuzzleStatePerTeamQuery(puzzleId, teamId).ToListAsync();

            for (int i = 0; i < states.Count; i++)
            {
                states[i].IsSolved = value;
            }
            await Context.SaveChangesAsync();
        }

        private IQueryable<PuzzleStatePerTeam> GetPuzzleStatePerTeamQuery(int? puzzleId, int? teamId)
        {
            if (!puzzleId.HasValue && !teamId.HasValue)
            {
                throw new ArgumentException("Should never query all states across all events");
            }

            IQueryable<PuzzleStatePerTeam> puzzleStatePerTeamQ = Context.PuzzleStatePerTeam;

            if (puzzleId.HasValue)
            {
                puzzleStatePerTeamQ = puzzleStatePerTeamQ.Where(state => state.PuzzleID == puzzleId.Value);
            }
            if (teamId.HasValue)
            {
                puzzleStatePerTeamQ = puzzleStatePerTeamQ.Where(state => state.TeamID == teamId.Value);
            }

            return puzzleStatePerTeamQ;
        }

        private async Task EnsureStateForPuzzleAsync(int puzzleId)
        {
            var teamsQ = this.Context.Teams.Where(team => team.Event == this.Event).Select(team => team.ID);
            var puzzleStateTeamsQ = this.Context.PuzzleStatePerTeam.Where(state => state.PuzzleID == puzzleId).Select(state => state.TeamID);
            var teamsWithoutState = await teamsQ.Except(puzzleStateTeamsQ).ToListAsync();

            if (teamsWithoutState.Count > 0)
            {
                for (int i = 0; i < teamsWithoutState.Count; i++)
                {
                    this.Context.PuzzleStatePerTeam.Add(new DataModel.PuzzleStatePerTeam() { PuzzleID = puzzleId, TeamID = teamsWithoutState[i] });
                }

                await this.Context.SaveChangesAsync();
            }
        }

        private async Task EnsureStateForTeamAsync(int teamId)
        {
            var puzzlesQ = this.Context.Puzzles.Where(puzzle => puzzle.Event == this.Event).Select(puzzle => puzzle.ID);
            var puzzleStatePuzzlesQ = this.Context.PuzzleStatePerTeam.Where(state => state.TeamID == teamId).Select(state => state.PuzzleID);
            var puzzlesWithoutState = await puzzlesQ.Except(puzzleStatePuzzlesQ).ToListAsync();

            if (puzzlesWithoutState.Count > 0)
            {
                for (int i = 0; i < puzzlesWithoutState.Count; i++)
                {
                    this.Context.PuzzleStatePerTeam.Add(new DataModel.PuzzleStatePerTeam() { TeamID = teamId, PuzzleID = puzzlesWithoutState[i] });
                }

                await this.Context.SaveChangesAsync();
            }
        }

        public enum SortOrder
        {
            PuzzleAscending,
            PuzzleDescending,
            TeamAscending,
            TeamDescending,
            UnlockAscending,
            UnlockDescending,
            SolveAscending,
            SolveDescending
        }
    }
}
