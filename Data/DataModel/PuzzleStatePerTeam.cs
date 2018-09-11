using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace ServerCore.DataModel
{
    public class PuzzleStatePerTeam
    {
        /// <summary>
        /// Row ID
        /// </summary>
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }

        public virtual Puzzle Puzzle { get; set; }
        public virtual Team Team { get; set; }

        /// <summary>
        /// Whether or not the puzzle has been unlocked
        /// </summary>
        [NotMapped]
        public bool IsUnlocked
        {
            get { return UnlockedTime != null; }
            set { UnlockedTime = value ? (DateTime?)DateTime.UtcNow : null; }
        }

        /// <summary>
        /// Whether or not the puzzle has been unlocked
        /// </summary>
        [NotMapped]
        public bool IsSolved
        {
            get { return SolvedTime != null; }
            set { SolvedTime = value ? (DateTime?)DateTime.UtcNow : null; }
        }

        /// <summary>
        /// Whether or not the puzzle has been unlocked by this team, and if so when
        /// </summary>
        public DateTime? UnlockedTime { get; set; }

        /// <summary>
        /// Whether or not the puzzle has been solved by this team, and if so when
        /// </summary>
        public DateTime? SolvedTime { get; set; }

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
