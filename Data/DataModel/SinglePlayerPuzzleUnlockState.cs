using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace ServerCore.DataModel
{
    public class SinglePlayerPuzzleUnlockState
    {
        // Note: No ID here. See PuzzleServerContext.OnModelCreating for the declaration of the key.

        [ForeignKey("Puzzle")]
        public int PuzzleID { get; set; }
        public virtual Puzzle Puzzle { get; set; }

        /// <summary>
        /// Whether or not the puzzle has been unlocked by this team, and if so when
        /// </summary>
        public DateTime? UnlockedTime { get; set; }
    }
}
