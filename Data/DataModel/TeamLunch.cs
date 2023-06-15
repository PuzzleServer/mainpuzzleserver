using System.ComponentModel.DataAnnotations;

namespace ServerCore.DataModel
{
    /// <summary>
    /// One lunch item being ordered by a team. One-to-many relationship
    /// </summary>
    public class TeamLunch
    {
        public int ID { get; set; }

        [Required]
        public int TeamId { get; set; }

        /// <summary>
        /// The team whose lunch this is
        /// </summary>
        public virtual Team Team { get; set; }

        /// <summary>
        /// Description of the lunch order
        /// </summary>
        public string Lunch { get; set; }

        /// <summary>
        /// Modifications like "no cheese"
        /// </summary>
        public string LunchModifications { get; set; }
    }
}
