using ServerCore.DataModel;
using ServerCore.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace ServerCore
{
    public class PuzzleStateHelper
    {
        /// <summary>
        /// Get a read-only query of puzzle state. You won't be able to write to these values, but the query will be resilient to state records that are missing on the server.
        /// </summary>
        /// <param name="context">The puzzle DB context</param>
        /// <param name="eventObj">The event we are querying from</param>
        /// <param name="puzzle">The puzzle; if null, get all puzzles in the event.</param>
        /// <param name="team">The team; if null, get all the teams in the event.</param>
        /// <returns>A query of PuzzleStatePerTeam objects that can be sorted and instantiated, but you can't edit the results.</returns>
        public static IQueryable<PuzzleStatePerTeam> GetReadOnlyQuery(PuzzleServerContext context, Event eventObj, Puzzle puzzle, Team team)
        {
            if (context == null)
            {
                throw new ArgumentNullException("Context required.");
            }

            if (eventObj == null)
            {
                throw new ArgumentNullException("Event required.");
            }

            if (puzzle != null && team != null)
            {
                return context.PuzzleStatePerTeam
                    .Where(state => state.Puzzle == puzzle && state.Team == team)
                    .DefaultIfEmpty(new DataModel.PuzzleStatePerTeam()
                    {
                        Puzzle = puzzle,
                        PuzzleID = puzzle.ID,
                        Team = team,
                        TeamID = team.ID
                    });
            }

            if (puzzle != null)
            {
                var teams = context.Teams.Where(t => t.Event == eventObj);
                var states = context.PuzzleStatePerTeam.Where(state => state.Puzzle == puzzle);

                return from t in teams
                       join state in states on t.ID equals state.TeamID into tmp
                       from teamstate in tmp.DefaultIfEmpty()
                       select new PuzzleStatePerTeam
                       {
                           Puzzle = puzzle,
                           PuzzleID = puzzle.ID,
                           Team = t,
                           TeamID = t.ID,
                           UnlockedTime = teamstate == null ? null : teamstate.UnlockedTime,
                           SolvedTime = teamstate == null ? null : teamstate.SolvedTime,
                           Printed = teamstate == null ? false : teamstate.Printed,
                           Notes = teamstate == null ? null : teamstate.Notes
                       };
            }

            if (team != null)
            {
                var puzzles = context.Puzzles.Where(p => p.Event == eventObj);
                var states = context.PuzzleStatePerTeam.Where(state => state.Team == team);

                return from p in puzzles
                       join state in states on p.ID equals state.PuzzleID into tmp
                       from teamstate in tmp.DefaultIfEmpty()
                       select new PuzzleStatePerTeam
                       {
                           Puzzle = p,
                           PuzzleID = p.ID,
                           Team = team,
                           TeamID = team.ID,
                           UnlockedTime = teamstate == null ? null : teamstate.UnlockedTime,
                           SolvedTime = teamstate == null ? null : teamstate.SolvedTime,
                           Printed = teamstate == null ? false : teamstate.Printed,
                           Notes = teamstate == null ? null : teamstate.Notes
                       };
            }

            throw new NotImplementedException("Full event query is NYI but needed for puzzle state map");
        }

        /// <summary>
        /// Get a writable query of puzzle state. In the course of constructing the query, it will instantiate any state records that are missing on the server.
        /// </summary>
        /// <param name="context">The puzzle DB context</param>
        /// <param name="eventObj">The event we are querying from</param>
        /// <param name="puzzle">The puzzle; if null, get all puzzles in the event.</param>
        /// <param name="team">The team; if null, get all the teams in the event.</param>
        /// <returns>A query of PuzzleStatePerTeam objects that can be sorted and instantiated, but you can't edit the results.</returns>
        public static async Task<IQueryable<PuzzleStatePerTeam>> GetReadWriteQueryAsync(PuzzleServerContext context, Event eventObj, Puzzle puzzle, Team team)
        {
            if (context == null)
            {
                throw new ArgumentNullException("Context required.");
            }

            if (eventObj == null)
            {
                throw new ArgumentNullException("Event required.");
            }

            if (puzzle != null && team != null)
            {
                PuzzleStatePerTeam state = await context.PuzzleStatePerTeam.Where(s => s.Puzzle == puzzle && s.Team == team).FirstOrDefaultAsync();
                if (state == null)
                {
                    context.PuzzleStatePerTeam.Add(new DataModel.PuzzleStatePerTeam() { Puzzle = puzzle, Team = team });
                    await context.SaveChangesAsync(); // query below will not return this unless we save
                }

                return context.PuzzleStatePerTeam.Where(s => s.Puzzle == puzzle && s.Team == team);
            }

            if (puzzle != null)
            {
                var teamIdsQ = context.Teams.Where(p => p.Event == eventObj).Select(p => p.ID);
                var puzzleStateTeamIdsQ = context.PuzzleStatePerTeam.Where(s => s.Puzzle == puzzle).Select(s => s.TeamID);
                var teamIdsWithoutState = await teamIdsQ.Except(puzzleStateTeamIdsQ).ToListAsync();

                if (teamIdsWithoutState.Count > 0)
                {
                    for (int i = 0; i < teamIdsWithoutState.Count; i++)
                    {
                        context.PuzzleStatePerTeam.Add(new DataModel.PuzzleStatePerTeam() { Puzzle = puzzle, TeamID = teamIdsWithoutState[i] });
                    }
                    await context.SaveChangesAsync(); // query below will not return these unless we save
                }

                return context.PuzzleStatePerTeam.Where(state => state.Puzzle == puzzle);
            }

            if (team != null)
            {
                var puzzleIdsQ = context.Puzzles.Where(p => p.Event == eventObj).Select(p => p.ID);
                var puzzleStatePuzzleIdsQ = context.PuzzleStatePerTeam.Where(s => s.Team == team).Select(s => s.PuzzleID);
                var puzzleIdsWithoutState = await puzzleIdsQ.Except(puzzleStatePuzzleIdsQ).ToListAsync();

                if (puzzleIdsWithoutState.Count > 0)
                {
                    for (int i = 0; i < puzzleIdsWithoutState.Count; i++)
                    {
                        context.PuzzleStatePerTeam.Add(new DataModel.PuzzleStatePerTeam() { Team = team, PuzzleID = puzzleIdsWithoutState[i] });
                    }
                    await context.SaveChangesAsync(); // query below will not return these unless we save
                }

                return context.PuzzleStatePerTeam.Where(state => state.Team == team);
            }

            throw new NotImplementedException("Full event query is NYI and may never be needed");
        }
    }
}
