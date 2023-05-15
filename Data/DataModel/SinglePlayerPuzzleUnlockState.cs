using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace ServerCore.DataModel
{
    /// <summary>
    /// The unlock state of a single player puzzle.
    /// Unlike puzzles for teams, which allows admins/authors to unlock the puzzle for a specific team,
    /// they will only be able to either lock or unlock for ALL players.
    /// </summary>
    public class SinglePlayerPuzzleUnlockState
    {
        // Note: No ID here. See PuzzleServerContext.OnModelCreating for the declaration of the key.

        [ForeignKey("Puzzle")]
        public int PuzzleID { get; set; }
        public virtual Puzzle Puzzle { get; set; }

        /// <summary>
        /// Whether or not the puzzle has been unlocked by this player, and if so when.
        /// </summary>
        public DateTime? UnlockedTime { get; set; }
    }
}
