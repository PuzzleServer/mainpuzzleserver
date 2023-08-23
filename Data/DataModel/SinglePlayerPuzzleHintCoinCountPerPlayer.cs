using System.ComponentModel.DataAnnotations.Schema;

namespace ServerCore.DataModel
{
    /// <summary>
    /// Tracks single player puzzle number of hint coins per player for a given event.
    /// </summary>
    public class SinglePlayerPuzzleHintCoinCountPerPlayer
    {
        // Note: No ID here. See PuzzleServerContext.OnModelCreating for the declaration of the key.

        /// <summary>
        /// The player the count applies to.
        /// </summary>
        [ForeignKey("PuzzleUser.ID")]
        public int PuzzleUserID { get; set; }

        public virtual PuzzleUser PuzzleUser { get; set; }

        /// <summary>
        /// The event.
        /// </summary>
        [ForeignKey("Event.ID")]
        public int EventID { get; set; }

        public virtual Event Event { get; set; }

        /// <summary>
        /// The number of hint coins the player has left.
        /// </summary>
        public int HintCoinCount { get; set; }

        /// <summary>
        /// The number of hint coins the player has used.
        /// </summary>
        public int HintCoinsUsed { get; set; }
    }
}
