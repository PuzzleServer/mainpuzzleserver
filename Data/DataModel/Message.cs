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
        public bool IsFromGameControl { get; }

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
        /// Gets or sets the value indicating whether a user has marked the message as "read".
        /// This will likely be used by game control to "claim" a question so no one else tries to answer it.
        /// </summary>
        public bool IsMarkedAsRead { get; set; }
    }
}
