using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ServerCore.DataModel;

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
        public static IQueryable<PuzzleStatePerTeam> GetFullReadOnlyQuery(PuzzleServerContext context, Event eventObj, Puzzle puzzle, Team team)
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

#pragma warning disable IDE0031 // despite the compiler message, "teamstate?.UnlockedTime", etc does not compile here

            if (puzzle != null)
            {
                IQueryable<Team> teams = context.Teams.Where(t => t.Event == eventObj);
                IQueryable<PuzzleStatePerTeam> states = context.PuzzleStatePerTeam.Where(state => state.Puzzle == puzzle);

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
                IQueryable<Puzzle> puzzles = context.Puzzles.Where(p => p.Event == eventObj);
                IQueryable<PuzzleStatePerTeam> states = context.PuzzleStatePerTeam.Where(state => state.Team == team);

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
#pragma warning restore IDE0031

            throw new NotImplementedException("Full event query is NYI and may never be needed; use the sparse one");
        }

        /// <summary>
        /// Get a read-write query of puzzle state without any missing values. This is the most performant query, but its data is incomplete. Use with caution!
        /// </summary>
        /// <param name="context">The puzzle DB context</param>
        /// <param name="eventObj">The event we are querying from</param>
        /// <param name="puzzle">The puzzle; if null, get all puzzles in the event.</param>
        /// <param name="team">The team; if null, get all the teams in the event.</param>
        /// <returns>A query of PuzzleStatePerTeam objects that currently exist in the table.</returns>
        public static IQueryable<PuzzleStatePerTeam> GetSparseQuery(PuzzleServerContext context, Event eventObj, Puzzle puzzle, Team team)
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
                return context.PuzzleStatePerTeam.Where(state => state.Puzzle == puzzle && state.Team == team);
            }

            if (puzzle != null)
            {
                return context.PuzzleStatePerTeam.Where(state => state.Puzzle == puzzle);
            }

            if (team != null)
            {
                return context.PuzzleStatePerTeam.Where(state => state.Team == team);
            }

            return context.PuzzleStatePerTeam.Where(state => state.Puzzle.Event == eventObj);
        }

        /// <summary>
        /// Set the unlock state of some puzzle state records. In the course of setting the state, instantiate any state records that are missing on the server.
        /// </summary>
        /// <param name="context">The puzzle DB context</param>
        /// <param name="eventObj">The event we are querying from</param>
        /// <param name="puzzle">The puzzle; if null, get all puzzles in the event.</param>
        /// <param name="team">The team; if null, get all the teams in the event.</param>
        /// <param name="value">The unlock time (null if relocking)</param>
        /// <returns>A task that can be awaited for the unlock/lock operation</returns>
        public static async Task SetUnlockStateAsync(PuzzleServerContext context, Event eventObj, Puzzle puzzle, Team team, DateTime? value)
        {
            IQueryable<PuzzleStatePerTeam> statesQ = await PuzzleStateHelper.GetFullReadWriteQueryAsync(context, eventObj, puzzle, team);
            List<PuzzleStatePerTeam> states = await statesQ.ToListAsync();

            for (int i = 0; i < states.Count; i++)
            {
                states[i].UnlockedTime = value;
            }
            await context.SaveChangesAsync();
        }

        /// <summary>
        /// Set the solve state of some puzzle state records. In the course of setting the state, instantiate any state records that are missing on the server.
        /// </summary>
        /// <param name="context">The puzzle DB context</param>
        /// <param name="eventObj">The event we are querying from</param>
        /// <param name="puzzle">The puzzle; if null, get all puzzles in the event.</param>
        /// <param name="team">The team; if null, get all the teams in the event.</param>
        /// <param name="value">The solve time (null if unsolving)</param>
        /// <returns>A task that can be awaited for the solve/unsolve operation</returns>
        public static async Task SetSolveStateAsync(PuzzleServerContext context, Event eventObj, Puzzle puzzle, Team team, DateTime? value)
        {
            IQueryable<PuzzleStatePerTeam> statesQ = await PuzzleStateHelper.GetFullReadWriteQueryAsync(context, eventObj, puzzle, team);
            List<PuzzleStatePerTeam> states = await statesQ.ToListAsync();

            for (int i = 0; i < states.Count; i++)
            {
                states[i].SolvedTime = value;
            }

            if (value != null)
            {
                // TODO: Unlock puzzles here when prerequisites are solved!
            }

            await context.SaveChangesAsync();
        }

        /// <summary>
        /// Get a writable query of puzzle state. In the course of constructing the query, it will instantiate any state records that are missing on the server.
        /// </summary>
        /// <param name="context">The puzzle DB context</param>
        /// <param name="eventObj">The event we are querying from</param>
        /// <param name="puzzle">The puzzle; if null, get all puzzles in the event.</param>
        /// <param name="team">The team; if null, get all the teams in the event.</param>
        /// <returns>A query of PuzzleStatePerTeam objects that can be sorted and instantiated, but you can't edit the results.</returns>
        private static async Task<IQueryable<PuzzleStatePerTeam>> GetFullReadWriteQueryAsync(PuzzleServerContext context, Event eventObj, Puzzle puzzle, Team team)
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
                }
            }
            else if (puzzle != null)
            {
                IQueryable<int> teamIdsQ = context.Teams.Where(p => p.Event == eventObj).Select(p => p.ID);
                IQueryable<int> puzzleStateTeamIdsQ = context.PuzzleStatePerTeam.Where(s => s.Puzzle == puzzle).Select(s => s.TeamID);
                List<int> teamIdsWithoutState = await teamIdsQ.Except(puzzleStateTeamIdsQ).ToListAsync();

                if (teamIdsWithoutState.Count > 0)
                {
                    for (int i = 0; i < teamIdsWithoutState.Count; i++)
                    {
                        context.PuzzleStatePerTeam.Add(new DataModel.PuzzleStatePerTeam() { Puzzle = puzzle, TeamID = teamIdsWithoutState[i] });
                    }
                }
            }
            else if (team != null)
            {
                IQueryable<int> puzzleIdsQ = context.Puzzles.Where(p => p.Event == eventObj).Select(p => p.ID);
                IQueryable<int> puzzleStatePuzzleIdsQ = context.PuzzleStatePerTeam.Where(s => s.Team == team).Select(s => s.PuzzleID);
                List<int> puzzleIdsWithoutState = await puzzleIdsQ.Except(puzzleStatePuzzleIdsQ).ToListAsync();

                if (puzzleIdsWithoutState.Count > 0)
                {
                    for (int i = 0; i < puzzleIdsWithoutState.Count; i++)
                    {
                        context.PuzzleStatePerTeam.Add(new DataModel.PuzzleStatePerTeam() { Team = team, PuzzleID = puzzleIdsWithoutState[i] });
                    }
                }
            }
            else if (puzzle == null && team == null)
            {
                throw new NotImplementedException("Full event query is NYI and may never be needed");
            }

            await context.SaveChangesAsync(); // query below will not return these unless we save

            // now this query is no longer sparse because we just filled it all out!
            return GetSparseQuery(context, eventObj, puzzle, team);
        }
    }
}
