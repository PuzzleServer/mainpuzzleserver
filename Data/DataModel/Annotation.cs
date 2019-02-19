using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace ServerCore.DataModel
{
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
