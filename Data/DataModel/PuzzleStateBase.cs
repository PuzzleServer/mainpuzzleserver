using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace ServerCore.DataModel
{
    public abstract class PuzzleStateBase
    {
        // Note: No ID here. See PuzzleServerContext.OnModelCreating for the declaration of the key.

        public int PuzzleID { get; set; }
        public virtual Puzzle Puzzle { get; set; }

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
        public DateTime? LockoutExpiryTime { get; set; }

        /// <summary>
        /// Returns whether or not the team is currently locked out of a puzzle.
        /// </summary>
        [NotMapped]
        public bool IsLockedOut
        {
            get
            {
                return LockoutExpiryTime == null ?
                    false :
                    DateTime.UtcNow.CompareTo(LockoutExpiryTime) < 0;
            }
        }

        /// <summary>
        /// Returns how much lockout time is left.
        /// </summary>
        [NotMapped]
        public TimeSpan LockoutTimeRemaining
        {
            get
            {
                return LockoutExpiryTime == null ?
                    TimeSpan.Zero :
                    LockoutExpiryTime.Value - DateTime.UtcNow;
            }
        }

        /// <summary>
        /// If the team submits too many wrong answers, they become permanently
        /// locked out of the puzzle and must email the admins.
        /// </summary>
        public bool IsEmailOnlyMode { get; set; }

        /// <summary>
        /// The number of additional incorrect guesses a team may have before
        /// reaching the maximum submission limit for this puzzle.
        /// </summary>
        public uint WrongSubmissionCountBuffer { get; set; }
    }
}
