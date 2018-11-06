using System.ComponentModel.DataAnnotations.Schema;

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
        /// The puzzle ID that depends on others to be solved before it will be unlocked.
        /// </summary>
        public int PuzzleID { get; set; }

        /// <summary>
        /// The puzzle that depends on others to be solved before it will be unlocked.
        /// </summary>
        public virtual Puzzle Puzzle { get; set; }

        /// <summary>
        /// A potential prerequisite ID of the puzzle named in Puzzle, which may need to be solved before Puzzle will be unlocked.
        /// </summary>
        public int PrerequisiteID { get; set; }

        /// <summary>
        /// A potential prerequisite of the puzzle named in Puzzle, which may need to be solved before Puzzle will be unlocked.
        /// </summary>
        public virtual Puzzle Prerequisite { get; set; }
    }
}
