using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace ServerCore.DataModel
{
    /// <summary>
    /// A message.
    /// </summary>
    public class Message
    {
        /// <summary>
        /// Gets or sets the id.
        /// </summary>
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }

        /// <summary>
        /// Gets or sets a thread id.
        /// </summary>
        public string ThreadId { get; set; }

        /// <summary>
        /// Gets or sets if sender is from game control.
        /// </summary>
        public bool IsFromGameControl { get; set; }

        /// <summary>
        /// Gets or sets the subject.
        /// </summary>
        public string Subject { get; set; }

        /// <summary>
        /// Gets or sets the event ID.
        /// </summary>
        public int EventID { get; set; }

        /// <summary>
        /// Gets or sets the event.
        /// </summary>
        public virtual Event Event { get; set; }

        /// <summary>
        /// Gets or sets the date time (in UTC) of when the message was sent.
        /// </summary>
        public DateTime DateTimeInUtc { get; set; }

        /// <summary>
        /// Gets or sets the actual message text.
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// Gets or sets the id of the sender.
        /// </summary>
        public int SenderID { get; set; }

        /// <summary>
        /// Gets or sets the user that sent the message.
        /// </summary>
        public virtual PuzzleUser Sender { get; set; }

        /// <summary>
        /// Gets or sets the puzzle id.
        /// </summary>
        public int PuzzleID { get; set; }

        /// <summary>
        /// Gets or sets the puzzle.
        /// </summary>
        public virtual Puzzle Puzzle { get; set; }

        /// <summary>
        /// Gets or sets the team id.
        /// </summary>
        public int TeamID { get; set; }

        /// <summary>
        /// Gets or sets the team.
        /// </summary>
        public Team Team { get; set; }

        /// <summary>
        /// Gets or sets the value indicating whether the message has been claimed by someone in Game Control.
        /// </summary>
        public bool IsClaimed { get; set; }

        /// <summary>
        /// Gets or sets the id of the user that claimed the message.
        /// </summary>
        public int ClaimerID { get; set; }

        /// <summary>
        /// Gets or sets the user claimed the message.
        /// </summary>
        public virtual PuzzleUser Claimer { get; set; }
    }
}
