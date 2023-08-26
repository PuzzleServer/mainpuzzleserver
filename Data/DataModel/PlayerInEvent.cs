using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ServerCore.DataModel
{
    [Table("Swag")]
    public class PlayerInEvent
    {
        public int ID { get; set; }

        [Required]
        public int EventId { get; set; }

        public virtual Event Event { get; set; }

        [Required]
        public int PlayerId { get; set; }

        public virtual PuzzleUser Player { get; set; }

        public string Lunch { get; set; }

        public string LunchModifications { get; set; }

        public string ShirtSize { get; set; }

        public bool IsRemote { get; set; }

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
