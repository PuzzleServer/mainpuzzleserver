using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace ServerCore.DataModel
{
    /// <summary>
    /// Database tracking for puzzle files stored on the server
    /// </summary>
    public class PuzzleFile
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }

        /// <summary>
        /// Url of the puzzle file
        /// </summary>
        [NotMapped]
        public Uri Url
        {
            get { Uri.TryCreate(UrlString, UriKind.RelativeOrAbsolute, out Uri result); return result; }
            set { UrlString = value?.ToString(); }
        }

        /// <summary>
        /// String representation of the blob URL. Use <see cref="Url"/> instead 
        /// </summary>
        [DataType(DataType.Url)]
        [Required]
        public string UrlString { get; set; }

        /// <summary>
        /// True if this is an answer file that should be unlocked
        /// when answers become available
        /// </summary>
        public bool IsAnswer { get; set; }

        /// <summary>
        /// If this is set to a puzzle, the file will only become available
        /// when the puzzle does
        /// </summary>
        public virtual Puzzle UnlocksWithPuzzle { get; set; }

        /// <summary>
        /// If this is set to a puzzle, the file will only become available
        /// when the puzzle has been solved
        /// </summary>
        public virtual Puzzle UnlocksWithSolve { get; set; }
    }
}
