using System;
using ServerCore.DataModel;

namespace ServerCore.Models
{
    /// <summary>
    /// A core message type used to interact with the database <see cref="DataModel.Message"/> in C#.
    /// We are using these core types becaues the database type has many nullable columns.
    /// </summary>
    public abstract class MessageBase
    {
        public MessageBase(int id, string threadId, string subject, Event evt, DateTime dateTimeInUtc, string text, PuzzleUser sender)
        {
            this.ID = id;
            this.ThreadId = threadId;
            this.Subject = subject;
            this.Event = evt;
            this.DateTimeInUtc = dateTimeInUtc;
            this.Text = text;
            this.Sender = sender;
        }

        /// <summary>
        /// Gets or sets the id.
        /// </summary>
        public int ID { get; }

        /// <summary>
        /// Gets or sets a thread id.
        /// </summary>
        public string ThreadId { get; }

        /// <summary>
        /// Gets or sets the subject.
        /// </summary>
        public string Subject { get; }

        /// <summary>
        /// Gets or sets the event.
        /// </summary>
        public Event Event { get; }

        /// <summary>
        /// Gets or sets the date time (in UTC) of when the message was sent.
        /// </summary>
        public DateTime DateTimeInUtc { get; }

        /// <summary>
        /// Gets or sets the actual message text.
        /// </summary>
        public string Text { get; }

        /// <summary>
        /// Gets or sets the user that sent the message.
        /// </summary>
        public virtual PuzzleUser Sender { get; }
    }
}
