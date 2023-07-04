using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace ServerCore.DataModel
{
    /// <summary>
    /// Provides a structure for associating room and teams within an event
    /// </summary>
    public class Room
    {
        /// <summary>
        /// The database generated ID
        /// </summary>
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }

        /// <summary>
        /// The ID for the event the room is a part of
        /// </summary>
        [Required]
        public int EventID { get; set; }

        /// <summary>
        /// The event the room is a part of
        /// </summary>
        [Required]
        public virtual Event Event { get; set; }

        /// <summary>
        /// The building the room is in (optional - building can be included in room string if preferred)
        /// </summary>
        public string Building { get; set; }

        /// <summary>
        /// The room number (or building + room if preferred)
        /// </summary>
        [Required]
        public string Number { get; set; }

        /// <summary>
        /// Room capacity per GAL
        /// </summary>
        public int? Capacity { get; set; }

        /// <summary>
        /// The ID for the team that is associated with this room
        /// </summary>
        public int? TeamID { get; set; }

        /// <summary>
        /// The team that is associated with this room
        /// </summary>
        public virtual Team Team { get; set; }

        /// <summary>
        /// Determines whether the room is currently available for teams to choose (allows admin to control list without having to add/delete)
        /// </summary>
        [Required]
        [DefaultValue(false)]
        public bool CurrentlyOnline { get; set; }

        /// <summary>
        /// Optional value to group rooms together, groups can be set to online/offline as a unit
        /// </summary>
        public int Group { get; set; }
    }
}
