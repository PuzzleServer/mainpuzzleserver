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
    public class ContentFile
    {
        public ContentFile()
        {
        }

        public ContentFile(ContentFile source)
        {
            FileType = source.FileType;
            EventID = source.EventID;
            ShortName = source.ShortName;
            UrlString = source.UrlString;
            PuzzleID = source.PuzzleID;
        }

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }

        /// <summary>
        /// Identifies how this content should be handled
        /// </summary>
        public ContentFileType FileType { get; set; }

        /// <summary>
        /// Required for indexing. Use <see cref="Event"/> instead.
        /// </summary>
        public int EventID { get; set; }

        /// <summary>
        /// Event this content is part of. Combined with <see cref="ShortName"/>, this will form the
        /// friendly URL and thus must be unique
        /// </summary>
        public virtual Event Event { get; set; }

        /// <summary>
        /// The name of the file that users will see without a full blob URL
        /// </summary>
        [Required]
        public string ShortName { get; set; }

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
        /// Foreign key to the Puzzles table. Do not use directly; use <see cref="Puzzle"/>
        /// </summary>
        public int PuzzleID { get; set; }

        /// <summary>
        /// The puzzle this file belongs to
        /// </summary>
        [Required]
        [ForeignKey("PuzzleID")]
        public virtual Puzzle Puzzle { get; set; }
    }

    /// <summary>
    /// Type of file the content represents
    /// </summary>
    public enum ContentFileType
    {
        /// <summary>
        /// The main puzzle file (typically a PDF). There should only be one of these
        /// per puzzle. This unlocks when the puzzle unlocks.
        /// </summary>
        Puzzle,

        /// <summary>
        /// Extra files used in a puzzle, such as media files. These unlock at the same time
        /// as the Puzzle file, but will not be linked directly.
        /// </summary>
        PuzzleMaterial,

        /// <summary>
        /// The main answer file (typically a PDF). This unlocks when all answers become available.
        /// </summary>
        Answer,

        /// <summary>
        /// Files made available after the puzzle is solved, often for metapuzzles.
        /// </summary>
        SolveToken,
    }
}
