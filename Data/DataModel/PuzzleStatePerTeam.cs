using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace ServerCore.DataModel
{
    public class PuzzleStatePerTeam
    {
        // Note: No ID here. See PuzzleServerContext.OnModelCreating for the declaration of the key.

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
        /// Whether or not the puzzle has been solved
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

        /// <summary>
        /// If set, represents the time when a team can begin submitting answers for the
        /// puzzle again. If a team spams answers to a puzzle, they will be locked-out
        /// from submitting additional answers for a brief period of time.
        /// </summary>
        public DateTime? LockoutTime { get; set; }

        /// <summary>
        /// Returns whether or not the team is currently locked out of a puzzle.
        /// </summary>
        [NotMapped]
        public bool IsTeamLockedOut
        {
            get { return LockoutTime == null ? false : DateTime.UtcNow.CompareTo(LockoutTime) < 0; }
        }

        /// <summary>
        /// Returns how much lockout time is left.
        /// </summary>
        [NotMapped]
        public TimeSpan? LockoutTimeRemaining
        {
            get { return LockoutTime == null ? null : LockoutTime - DateTime.UtcNow; }
        }

        /// <summary>
        /// Represents the duration in minutes of the next lockout period for the team on
        /// this puzzle. This should decrease over time to 0 if a team hasn't been locked
        /// out in a while.
        /// </summary>
        public double LockoutStage { get; set; }

        /// <summary>
        /// If the team submits too many wrong answers, they become permanently locked out
        /// of the puzzle and must email the admins.
        /// </summary>
        public bool IsEmailOnlyMode { get; set; }

        /// <summary>
        /// Sets the puzzle state lockout fields.
        /// </summary>
        /// <param name="lockoutTime">The number of minutes this puzzle should be locked out.</param>
        public void SetPuzzleLockout (double lockoutTime)
        {
            LockoutStage = lockoutTime;
            LockoutTime = DateTime.UtcNow.AddMinutes(lockoutTime);
        }
    }
}
