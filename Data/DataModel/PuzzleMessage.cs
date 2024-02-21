using System;
using System.ComponentModel.DataAnnotations;

namespace ServerCore.DataModel
{
    /// <summary>
    /// A message about a specific puzzle.
    /// </summary>
    public class PuzzleMessage : Message
    {
        /// <summary>
        /// The puzzle id that this message is for.
        /// </summary>
        [Required]
        public int PuzzleID { get; set; }

        /// <summary>
        /// The puzzle this is a help message for.
        /// </summary>
        [Required]
        public virtual Puzzle Puzzle { get; set; }
    }
}
