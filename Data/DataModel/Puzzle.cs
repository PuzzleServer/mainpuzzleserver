using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using System.Linq;

namespace ServerCore.DataModel
{
    public class Puzzle
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }
        public virtual Event Event { get; set; }
        public string Name { get; set; }
        public bool IsPuzzle { get; set; } = false;
        public bool IsMetaPuzzle { get; set; } = false;
        public bool IsFinalPuzzle { get; set; } = false;
        public int SolveValue { get; set; } = 0;

        /// <summary>
        /// Reward if solved: Sometimes displayed publicly, sometimes used internally by meta engine
        /// </summary>
        public string Token { get; set; }

        /// <summary>
        /// Grouping key.
        /// Likely Puzzlehunt usage: name of the puzzle's module
        /// Likely Puzzleday usage: "Pregame" or blank
        /// </summary>
        public string Group { get; set; }

        /// <summary>
        /// Order within group
        /// </summary>
        public int OrderInGroup { get; set; } = 0;

        /// <summary>
        /// If true, all authors can see this puzzle when picking prerequisites
        /// </summary>
        public bool IsGloballyVisiblePrerequisite { get; set; } = false;

        /// <summary>
        /// Minimum number of prerequisites that must be satisfied.
        /// TODO: When the system is mature, set the default to 1 so new puzzles are not accidentally displayed.
        /// </summary>
        public int MinPrerequisiteCount { get; set; } = 0;

        /// <summary>
        /// All of the content files associated with this puzzle
        /// </summary>
        public virtual ICollection<ContentFile> Contents { get; set; }

        /// <summary>
        /// File for the main puzzle (typically a PDF containing the puzzle)
        /// </summary>
        [NotMapped]
        public ContentFile PuzzlePDF
        {
            get
            {
                var puzzlePDFs = from contentFile in Contents ?? Enumerable.Empty<ContentFile>()
                                 where contentFile.FileType == ContentFileType.Puzzle
                                 select contentFile;
                Debug.Assert(puzzlePDFs.Count() <= 1);
                return puzzlePDFs.FirstOrDefault();
            }
        }

        /// <summary>
        /// File for the main answer (typically a PDF containing the answer)
        /// </summary>
        [NotMapped]
        public ContentFile AnswerPDF
        {
            get
            {
                var answerPDFs = from contentFile in Contents ?? Enumerable.Empty<ContentFile>()
                                 where contentFile.FileType == ContentFileType.Answer
                                 select contentFile;
                Debug.Assert(answerPDFs.Count() <= 1);
                return answerPDFs.FirstOrDefault();
            }
        }


        /// <summary>
        /// Files associated with the puzzle, such as media
        /// </summary>
        [NotMapped]
        public IEnumerable<ContentFile> Materials
        {
            get
            {
                return from contentFile in Contents ?? Enumerable.Empty<ContentFile>()
                       where contentFile.FileType == ContentFileType.PuzzleMaterial
                       select contentFile;
            }
        }

        /// <summary>
        /// Files unlocked by solving this puzzle, often for metapuzzles
        /// </summary>
        [NotMapped]
        public IEnumerable<ContentFile> SolveTokens
        {
            get
            {
                return from contentFile in Contents ?? Enumerable.Empty<ContentFile>()
                       where contentFile.FileType == ContentFileType.PuzzleMaterial
                       select contentFile;
            }
        }        
    }
}