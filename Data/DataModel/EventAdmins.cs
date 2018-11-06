using System.ComponentModel.DataAnnotations.Schema;

namespace ServerCore.DataModel
{
    public class EventAdmins
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }

        /// <summary>
        /// Foreign key - event table
        /// </summary
        [ForeignKey("Event.ID")]
        public virtual Event Event { get; set; }

        /// <summary>
        /// Foreign key - user table
        /// </summary>
        [ForeignKey("User.ID")]
        public virtual PuzzleUser Admin { get; set; }

    }
}