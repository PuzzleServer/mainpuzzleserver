using System.ComponentModel.DataAnnotations.Schema;

namespace ServerCore.DataModel
{
    /// <summary>
    /// The teams that are participating in this event
    /// </summary>
    public class EventTeams
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
        /// Foreign Key - teams table
        /// </summary>
        [ForeignKey("Teams.ID")]
        public virtual Team Team { get; set; }
    }
}