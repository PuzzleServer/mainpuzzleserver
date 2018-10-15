using System.ComponentModel.DataAnnotations.Schema;

namespace ServerCore.DataModel
{
    /// <summary>
    /// The users who are owners for this event
    /// </summary>
    public class EventOwners
    {
        // ID for row
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }

        /// <summary>
        /// Foreign Key - event table
        /// </summary>
        [ForeignKey("Event.ID")]
        public virtual Event Event { get; set; }

        /// <summary>
        /// Foreign Key - user table (owner)
        /// </summary>
        [ForeignKey("User.ID")]
        public virtual User Owner { get; set; }
    }
}
