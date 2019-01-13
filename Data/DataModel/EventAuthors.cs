using System.ComponentModel.DataAnnotations.Schema;

namespace ServerCore.DataModel
{
    /// <summary>
    /// The authors who have written for this event
    /// </summary>
    public class EventAuthors
    {
        public EventAuthors()
        {
        }

        public EventAuthors(EventAuthors source)
        {
            // do not fill out the ID
            Event = source.Event;
            Author = source.Author;
        }

        // ID for row
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }

        /// <summary>
        /// Foreign Key - event table
        /// </summary>
        [ForeignKey("Event.ID")]
        public virtual Event Event { get; set; }

        /// <summary>
        /// Foreign Key - user table (author)
        /// </summary>
        [ForeignKey("User.ID")]
        public virtual PuzzleUser Author { get; set; }
    }
}
