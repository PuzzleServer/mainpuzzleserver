using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ServerCore.DataModel
{
    /// <summary>
    /// Classes or categories that players can select as part of registration for the event
    /// These classes can be used for event flavor or to determine content shown to those players
    /// One instance of each class is available per team (enforced based on ID)
    /// </summary>
    public class PlayerClass
    {
        /// <summary>
        /// The ID
        /// </summary>
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }

        /// <summary>
        /// The event the puzzle is a part of
        /// </summary>
        public int EventID { get; set; }

        /// <summary>
        /// The event the puzzle is a part of
        /// </summary>
        [Required]
        public virtual Event Event { get; set; }

        /// <summary>
        /// The name of the player class (e.g. Warrior, Bard, Astronaut, etc)
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        /// <summary>
        /// A name that is unique for roles within the event.
        /// This is used in urls, etc so best practice is all lowercase without spaces or special characters
        /// </summary>
        [Required]
        [MaxLength(20)]
        public string UniqueName { get; set; }

        /// <summary>
        /// Relative order within the list of player classes (primarily used for ordering in UI)
        /// </summary>
        public int Order { get; set; }
    }
}