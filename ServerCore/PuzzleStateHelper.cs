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

            // Award hint coins
            if (value != null)
            {
                if (team != null)
                {
                    team.HintCoinCount += puzzle.HintCoinsForSolve;
                }
                else
                {
                    var allTeams = from Team curTeam in context.Teams
                                   where curTeam.Event == eventObj
                                   select curTeam;
                    foreach(Team curTeam in allTeams)
                    {
                        curTeam.HintCoinCount += puzzle.HintCoinsForSolve;
                    }
                }
            }

            await context.SaveChangesAsync();

            // if this puzzle got solved, look for others to unlock
            if (value != null)
            {
                await UnlockAnyPuzzlesThatThisSolveUnlockedAsync(context, eventObj, puzzle, team, value.Value);
            }
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

        /// <summary>
        /// Unlock any puzzles that need to be unlocked due to the recent solve of a prerequisite.
        /// </summary>
        /// <param name="context">The puzzle DB context</param>
        /// <param name="eventObj">The event we are working in</param>
        /// <param name="puzzleJustSolved">The puzzle just solved; if null, all the puzzles in the event (which will make more sense once we add per author filtering)</param>
        /// <param name="team">The team that just solved; if null, all the teams in the event.</param>
        /// <param name="unlockTime">The time that the puzzle should be marked as unlocked.</param>
        /// <returns></returns>
        private static async Task UnlockAnyPuzzlesThatThisSolveUnlockedAsync(PuzzleServerContext context, Event eventObj, Puzzle puzzleJustSolved, Team team, DateTime unlockTime)
        {
            // a simple query for all puzzle IDs in the event - will be used at least once below
            IQueryable<int> allPuzzleIDsQ = context.Puzzles.Where(p => p.Event == eventObj).Select(p => p.ID);

            // if we solved a group of puzzles, every puzzle needs an update.
            // if we solved a single puzzle, only update the puzzles that have that one as a prerequisite.
            IQueryable<int> needsUpdatePuzzleIDsQ =
                puzzleJustSolved == null ?
                allPuzzleIDsQ :
                context.Prerequisites.Where(pre => pre.Prerequisite == puzzleJustSolved).Select(pre => pre.PuzzleID).Distinct();

            // get the prerequisites for all puzzles that need an update
            // information we get per puzzle: { id, min count, prerequisite IDs }
            var prerequisiteDataForNeedsUpdatePuzzles = await context.Prerequisites
                .Where(pre => needsUpdatePuzzleIDsQ.Contains(pre.PuzzleID))
                .GroupBy(pre => pre.Puzzle)
                .Select(g => new {
                    PuzzleID = g.Key.ID,
                    g.Key.MinPrerequisiteCount,
                    PrerequisiteIDs = g.Select(pre => pre.PrerequisiteID)
                })
                .ToListAsync();

            // Are we updating one team or all teams?
            List<Team> teamsToUpdate = team == null ? await context.Teams.Where(t => t.Event == eventObj).ToListAsync() : new List<Team>() { team };

            // Update teams one at a time
            foreach (Team t in teamsToUpdate)
            {
                // Collect the IDs of all solved/unlocked puzzles for this team
                // sparse lookup is fine since if the state is missing it isn't unlocked or solved!
                var puzzleStateForTeamT = await PuzzleStateHelper.GetSparseQuery(context, eventObj, null, t)
                    .Select(state => new { state.PuzzleID, state.UnlockedTime, state.SolvedTime })
                    .ToListAsync();

                // Make a hash set out of them for easy lookup in case we have several prerequisites to chase
                HashSet<int> unlockedPuzzleIDsForTeamT = new HashSet<int>();
                HashSet<int> solvedPuzzleIDsForTeamT = new HashSet<int>();

                foreach (var puzzleState in puzzleStateForTeamT)
                {
                    if (puzzleState.UnlockedTime != null)
                    {
                        unlockedPuzzleIDsForTeamT.Add(puzzleState.PuzzleID);
                    }

                    if (puzzleState.SolvedTime != null)
                    {
                        solvedPuzzleIDsForTeamT.Add(puzzleState.PuzzleID);
                    }
                }

                // now loop through all puzzles and count up who needs to be unlocked
                foreach (var puzzleToUpdate in prerequisiteDataForNeedsUpdatePuzzles)
                {
                    // already unlocked? skip
                    if (unlockedPuzzleIDsForTeamT.Contains(puzzleToUpdate.PuzzleID))
                    {
                        continue;
                    }

                    // Enough puzzles unlocked by count? Let's unlock it
                    if (puzzleToUpdate.PrerequisiteIDs.Where(id => solvedPuzzleIDsForTeamT.Contains(id)).Count() >= puzzleToUpdate.MinPrerequisiteCount)
                    {
                        PuzzleStatePerTeam state = await context.PuzzleStatePerTeam.Where(s => s.PuzzleID == puzzleToUpdate.PuzzleID && s.Team == t).FirstOrDefaultAsync();
                        if (state == null)
                        {
                            context.PuzzleStatePerTeam.Add(new DataModel.PuzzleStatePerTeam() { PuzzleID = puzzleToUpdate.PuzzleID, Team = t, UnlockedTime = unlockTime });
                        }
                        else
                        {
                            state.UnlockedTime = unlockTime;
                        }
                    }
                }
            }

            // after looping through all teams, send one update with all changes made
            await context.SaveChangesAsync();
        }
    }
}
