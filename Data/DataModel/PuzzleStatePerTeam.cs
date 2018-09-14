using ServerCore.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace ServerCore.DataModel
{
    public class PuzzleStatePerTeam
    {
        /// <summary>
        /// Row ID
        /// </summary>
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }

        public int PuzzleID { get; set; }
        public virtual Puzzle Puzzle { get; set; }

        public int TeamID { get; set; }
        public virtual Team Team { get; set; }

        /// <summary>
        /// Whether or not the puzzle has been unlocked
        /// </summary>
        [NotMapped]
        public bool IsUnlocked
        {
            get { return UnlockedTime != null; }
            set
            {
                if (IsUnlocked != value)
                {
                    UnlockedTime = value ? (DateTime?)DateTime.UtcNow : null;
                }
            }
        }

        /// <summary>
        /// Whether or not the puzzle has been unlocked
        /// </summary>
        [NotMapped]
        public bool IsSolved
        {
            get { return SolvedTime != null; }
            set
            {
                if (IsSolved != value)
                {
                    SolvedTime = value ? (DateTime?)DateTime.UtcNow : null;
                }
            }
        }

        /// <summary>
        /// Whether or not the puzzle has been unlocked by this team, and if so when
        /// </summary>
        public DateTime? UnlockedTime { get; set; }

        /// <summary>
        /// Whether or not the puzzle has been solved by this team, and if so when
        /// </summary>
        public DateTime? SolvedTime { get; set; }

        /// <summary>
        /// Whether or not the team has checked the "Printed" checkbox for this puzzle
        /// </summary>
        public bool Printed { get; set; }

        /// <summary>
        /// Notes input by the team for this puzzle
        /// </summary>
        public string Notes { get; set; }

        #region Consistency Helpers
        public static async Task<bool> EnsureStateForPuzzleAsync(PuzzleServerContext context, int eventId, int puzzleId, bool saveChanges = true)
        {
            var teamsQ = context.Teams.Where(team => team.Event.ID == eventId).Select(team => team.ID);
            var puzzleStateTeamsQ = context.PuzzleStatePerTeam.Where(state => state.PuzzleID == puzzleId).Select(state => state.TeamID);
            var teamsWithoutState = await teamsQ.Except(puzzleStateTeamsQ).ToListAsync();

            if (teamsWithoutState.Count > 0)
            {
                for (int i = 0; i < teamsWithoutState.Count; i++)
                {
                    context.PuzzleStatePerTeam.Add(new DataModel.PuzzleStatePerTeam() { PuzzleID = puzzleId, TeamID = teamsWithoutState[i] });
                }

                if (saveChanges)
                {
                    await context.SaveChangesAsync();
                }
            }

            return teamsWithoutState.Count > 0;
        }

        public static async Task<bool> EnsureStateForTeamAsync(PuzzleServerContext context, int eventId, int teamId, bool saveChanges = true)
        {
            var puzzlesQ = context.Puzzles.Where(puzzle => puzzle.Event.ID == eventId).Select(puzzle => puzzle.ID);
            var puzzleStatePuzzlesQ = context.PuzzleStatePerTeam.Where(state => state.TeamID == teamId).Select(state => state.PuzzleID);
            var puzzlesWithoutState = await puzzlesQ.Except(puzzleStatePuzzlesQ).ToListAsync();

            if (puzzlesWithoutState.Count > 0)
            {
                for (int i = 0; i < puzzlesWithoutState.Count; i++)
                {
                    context.PuzzleStatePerTeam.Add(new DataModel.PuzzleStatePerTeam() { TeamID = teamId, PuzzleID = puzzlesWithoutState[i] });
                }

                if (saveChanges)
                {
                    await context.SaveChangesAsync();
                }
            }

            return puzzlesWithoutState.Count > 0;
        }

        public static async Task<bool> EnsureStateForEventAsync(PuzzleServerContext context, int eventId, bool saveChanges = true)
        {
            bool addedAnything = false;
            var puzzles = await context.Puzzles.Where(puzzle => puzzle.Event.ID == eventId).Select(puzzle => puzzle.ID).ToListAsync();

            for (int i = 0; i < puzzles.Count; i++)
            {
                addedAnything |= await EnsureStateForPuzzleAsync(context, eventId, puzzles[i], saveChanges: false);
            }

            if (addedAnything && saveChanges)
            {
                await context.SaveChangesAsync();
            }

            return addedAnything;
        }

        public static async Task<bool> EnsureStateForAllEventsAsync(PuzzleServerContext context, bool saveChanges = true)
        {
            bool addedAnything = false;
            var events = await context.Events.Select(e => e.ID).ToListAsync();

            for (int i = 0; i < events.Count; i++)
            {
                addedAnything |= await EnsureStateForEventAsync(context, events[i], saveChanges: false);
            }

            if (addedAnything && saveChanges)
            {
                await context.SaveChangesAsync();
            }

            return addedAnything;
        }

        public static Task<bool> RemoveStateForPuzzleAsync(PuzzleServerContext context, int eventId, int puzzleId, bool saveChanges = true)
        {
            return RemoveStateHelperAsync(context, context.PuzzleStatePerTeam.Where(state => state.PuzzleID == puzzleId), saveChanges);
        }

        public static Task<bool> RemoveStateForTeamAsync(PuzzleServerContext context, int eventId, int teamId, bool saveChanges = true)
        {
            return RemoveStateHelperAsync(context, context.PuzzleStatePerTeam.Where(state => state.TeamID == teamId), saveChanges);
        }

        public static Task<bool> RemoveStateForEventAsync(PuzzleServerContext context, int eventId, bool saveChanges = true)
        {
            return RemoveStateHelperAsync(context, context.PuzzleStatePerTeam.Where(state => state.Puzzle.Event.ID == eventId), saveChanges);
        }

        public static Task<bool> RemoveStateForAllEventsAsync(PuzzleServerContext context, bool saveChanges = true)
        {
            return RemoveStateHelperAsync(context, context.PuzzleStatePerTeam, saveChanges);
        }

        private static async Task<bool> RemoveStateHelperAsync(PuzzleServerContext context, IQueryable<PuzzleStatePerTeam> statesToRemoveQuery, bool saveChanges)
        {
            var statesToRemove = await statesToRemoveQuery.ToArrayAsync();

            if (statesToRemove.Length > 0)
            {
                context.PuzzleStatePerTeam.RemoveRange(statesToRemove);

                if (saveChanges)
                {
                    await context.SaveChangesAsync();
                }
            }

            return statesToRemove.Length > 0;
        }
        #endregion
    }
}
