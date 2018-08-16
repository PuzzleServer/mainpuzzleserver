using System.ComponentModel.DataAnnotations.Schema;

namespace ServerCore.DataModel
{
    public class State
    {
        /// <summary>
        /// Database ID for the State
        /// </summary>
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }

        /// <summary>
        /// Whether or not the puzzle has been unlocked by this team
        /// </summary>
        public bool IsUnlocked { get; set; }

        /// <summary>
        /// Whether or not the puzzle has been solved by this team
        /// </summary>
        public bool IsSolved { get; set; }

        /// <summary>
        /// Whether or not the team has checked the "Printed" checkbox for this puzzle
        /// </summary>
        public bool Printed { get; set; }

        /// <summary>
        /// Notes input by the team for this puzzle
        /// </summary>
        public string Notes { get; set; }
    }
}