using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace ServerCore.DataModel
{
    /// <summary>
    /// Tracks hint unlocking for each player for a single player puzzle.
    /// </summary>
    public class SinglePlayerPuzzleHintStatePerPlayer
    {
        // Note: No ID here. See PuzzleServerContext.OnModelCreating for the declaration of the key.

        /// <summary>
        /// The player the hint applies to
        /// </summary>
        [ForeignKey("PuzzleUser.ID")]
        public int PlayerID { get; set; }

        public virtual PuzzleUser Player { get; set; }

        /// <summary>
        /// The hint for the player
        /// </summary>
        [ForeignKey("Hint.ID")]
        public int HintID { get; set; }

        public virtual Hint Hint { get; set; }

        /// <summary>
        /// The time the hint was unlocked. Null if the hint is currently locked.
        /// </summary>
        public DateTime? UnlockTime { get; set; }

        /// <summary>
        /// True if the hint is unlocked for the player.
        /// </summary>
        [NotMapped]
        public bool IsUnlocked
        {
            get
            {
                return UnlockTime != null;
            }
        }
    }
}
