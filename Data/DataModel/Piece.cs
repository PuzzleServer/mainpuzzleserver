using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace ServerCore.DataModel
{
    //
    // A puzzle may have additional information associated with it
    // that isn't made available to a team just because they have the
    // puzzle unlocked.  For instance, they may not get to see some
    // pieces of the puzzle until they've made a certain amount of
    // progress in the event.  A "piece" is such a piece of a puzzle.
    //

    public class Piece
    {
        /// <summary>
        /// The ID of this puzzle piece
        /// </summary>
        public int ID { get; set; }

        /// <summary>
        /// The ID of the puzzle that this piece is for
        /// </summary>
        [Required]
        public int PuzzleID { get; set; }

        /// <summary>
        /// The puzzle corresponding to PuzzleID
        /// </summary>
        public virtual Puzzle Puzzle { get; set; }

        /// <summary>
        /// The minimum progress level for a team at which this piece should be
        /// revealed to that team
        /// </summary>
        [Required]
        public int ProgressLevel { get; set; }

        /// <summary>
        /// The contents of the piece
        /// </summary>
        [Required]
        public string Contents { get; set; }
    }
}
