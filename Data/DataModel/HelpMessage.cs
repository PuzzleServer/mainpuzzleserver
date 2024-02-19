using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ServerCore.DataModel
{
    /// <summary>
    /// Message in help thread when a player asks for help.
    /// </summary>
    public class HelpMessage
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }

        /// <summary>
        /// The date time (in UTC) of when the message was sent.
        /// </summary>
        public DateTime DateTimeInUtc { get; set; }

        /// <summary>
        /// The puzzle id that this message is for.
        /// </summary>
        [Required]
        public int PuzzleID { get; set; }

        /// <summary>
        /// The puzzle this is a help message for.
        /// </summary>
        [Required]
        public virtual Puzzle Puzzle { get; set; }

        /// <summary>
        /// The actual message text.
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// True if message was from the player (as opposed to a response from puzzle control).
        /// </summary>
        public bool IsPlayerMessage { get; set; }

        /// <summary>
        /// The team ID of the sender (will be empty if not a player of if it is for a SinglePlayerPuzzle).
        /// </summary>
        public int TeamID { get; set; }

        /// <summary>
        /// The team of the sender (will be empty if not a player or if it is for a SinglePlayerPuzzle).
        /// </summary>
        public virtual Team Team { get; set; }

        /// <summary>
        /// The id of the sender.
        /// </summary>
        public int SenderID { get; set; }

        /// <summary>
        /// The user that sent the message.
        /// </summary>
        public virtual PuzzleUser Sender { get; set; }
    }
}
