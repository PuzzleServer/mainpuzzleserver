using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ServerCore.DataModel
{
    public class TeamMembers
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }

        /// <summary>
        /// Populates with Team.ID
        /// </summary>
        [ForeignKey("Team.ID")]
        [Column("Team.ID")]
        public int TeamID { get; set; }

        /// <summary>
        /// Foreign Key - Team table
        /// </summary>
        [Required]
        public virtual Team Team { get; set; }

        /// <summary>
        /// Populates with User.ID
        /// </summary>
        [ForeignKey("User.ID")]
        [Column("User.ID")]
        public int MemberID { get; set; }

        /// <summary>
        /// Foreign Key - User table
        /// </summary>
        [Required]
        public virtual PuzzleUser Member { get; set; }

        /// <summary>
        /// Populates with Class.ID
        /// </summary>
        public int? ClassID { get; set; }

        /// <summary>
        /// The class or category that the player falls into (classes are defined per event if used).
        /// This is for player categories unique to an event (e.g. character class for an RPG event or region if relevant for an international event)
        /// </summary>
        public virtual PlayerClass Class { get; set; }

        /// <summary>
        /// Populates with TemporaryClass.ID
        /// </summary>
        public int? TemporaryClassID { get; set; }

        /// <summary>
        /// If the player or event allows an override class that applies to the player temporarily
        /// This can be set and unset by the player on the TeamDetails page
        /// </summary>
        public virtual PlayerClass TemporaryClass { get; set; }
    }
}
