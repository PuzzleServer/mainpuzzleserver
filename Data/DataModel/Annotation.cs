using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace ServerCore.DataModel
{
    //
    // For most puzzles, teams annotate them by writing on a print-out
    // of their PDF and teammates share those annotations with each
    // other by showing each other that print-out.
    //
    // However, for puzzles with a significant online component, i.e.,
    // puzzles that are applications or websites, this isn't a viable
    // strategy.
    //
    // Such an online puzzle may want a way to store per-team
    // annotations on the puzzle server itself.  This table stores
    // such annotations.  Each annotation has a key, which the online
    // puzzle can use to reference a particular annotation.
    //
    // For instance, the answer a team member "writes" in the "space"
    // for clue number 17 of a puzzle might be stored as an annotation
    // with key 17 for that puzzle.
    //
    // To limit the amount of storage space any team can take up on
    // the server, annotations are limited to 255 bytes each.  Also,
    // teams may not store annotations with keys outside the range 1
    // to n, where n is the MaxAnnotationKey field of the associated
    // puzzle.
    //
    
    public class Annotation
    {
        /// <summary>
        /// The ID of the puzzle that this annotation is for
        /// </summary>
        [Required]
        public int PuzzleID { get; set; }

        /// <summary>
        /// The puzzle corresponding to PuzzleID
        /// </summary>
        public virtual Puzzle Puzzle { get; set; }

        /// <summary>
        /// The ID of the team that created this annotation
        /// </summary>
        [Required]
        public int TeamID { get; set; }

        /// <summary>
        /// The team corresponding to TeamID
        /// </summary>
        public virtual Team Team { get; set; }

        /// <summary>
        /// The annotation key the team can use to look up the annotation
        /// </summary>
        [Required]
        public int Key { get; set; }

        /// <summary>
        /// The version number, which increases with each change to the same annotation key
        /// </summary>
        [Required]
        public int Version { get; set; }

        /// <summary>
        /// The annotation contents
        /// </summary>
        [Required]
        [StringLength(255)]
        public string Contents { get; set; }

        /// <summary>
        /// When the annotation was last updated
        /// </summary>
        [Required]
        [DataType(DataType.Date)]
        public DateTime Timestamp { get; set; }
    }
}
