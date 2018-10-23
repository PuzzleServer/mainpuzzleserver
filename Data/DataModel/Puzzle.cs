﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using System.Linq;

namespace ServerCore.DataModel
{
    /// <summary>
    /// A Puzzle is the record of a solvable puzzle in the database
    /// Sometimes a Puzzle is used as a workaround for things like time prerequisites
    /// </summary>
    public class Puzzle
    {
        /// <summary>
        /// The ID
        /// </summary>
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }

        /// <summary>
        /// The event the puzzle is a part of
        /// </summary>
        public virtual Event Event { get; set; }

        /// <summary>
        /// The name of the puzzle
        /// </summary>
        [Required]
        public string Name { get; set; }

        /// <summary>
        /// True only if not a "fake" puzzle like "READ THIS INSTRUCTION PAGE" or "START THE EVENT"
        /// </summary>
        public bool IsPuzzle { get; set; } = false;

        /// <summary>
        /// True if this is a meta puzzle
        /// </summary>
        public bool IsMetaPuzzle { get; set; } = false;

        /// <summary>
        /// True if this is the final puzzle that would lock a team's rank in the standings
        /// </summary>
        public bool IsFinalPuzzle { get; set; } = false;

        /// <summary>
        /// The solve value
        /// </summary>
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
        /// The order in the group (commonly used for the intended release order of the puzzle)
        /// </summary>
        public int OrderInGroup { get; set; } = 0;

        /// <summary>
        /// If true, all authors can see this puzzle when picking prerequisites
        /// </summary>
        public bool IsGloballyVisiblePrerequisite { get; set; } = false;

        /// <summary>
        /// Minimum number of <see cref="Prerequisites.cs"/> that must be satisfied
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
        public ContentFile PuzzleFile
        {
            get
            {
                var PuzzleFiles = from contentFile in Contents ?? Enumerable.Empty<ContentFile>()
                                 where contentFile.FileType == ContentFileType.Puzzle
                                 select contentFile;
                Debug.Assert(PuzzleFiles.Count() <= 1);
                return PuzzleFiles.FirstOrDefault();
            }
        }

        /// <summary>
        /// File for the main answer (typically a PDF containing the answer)
        /// </summary>
        [NotMapped]
        public ContentFile AnswerFile
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
        public IEnumerable<ContentFile> SolveTokenFiles
        {
            get
            {
                return from contentFile in Contents ?? Enumerable.Empty<ContentFile>()
                       where contentFile.FileType == ContentFileType.SolveToken
                       select contentFile;
            }
        }        
    }
}