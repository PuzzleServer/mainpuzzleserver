using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace ServerCore.DataModel
{
    /// <summary>
    /// A way to determine that one puzzle depends on another being solved before it can be unlocked.
    /// A puzzle will specify how many of its prerequisites need to be solved before it can be unlocked, via MinPrerequisiteCount.
    /// </summary>
    public class Prerequisites
    {
        // ID for row
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }

        /// <summary>
        /// The puzzle that depends on others.
        /// </summary>
        [ForeignKey("Puzzle.ID")]
        public Puzzle Puzzle { get; set; }

        /// <summary>
        /// A potential prerequisite of the puzzle named in Puzzle, which may need to be solved before Puzzle will be unlocked.
        /// </summary>
        [ForeignKey("Puzzle.ID")]
        public Puzzle Prerequisite { get; set; }
    }
}
