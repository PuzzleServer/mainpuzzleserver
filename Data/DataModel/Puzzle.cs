using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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

        [DataType(DataType.Url)]
        public string PuzzleUrlString { get; set; }

        [NotMapped]
        public Uri PuzzleUrl
        {
            get { Uri.TryCreate(PuzzleUrlString, UriKind.RelativeOrAbsolute, out Uri result); return result; }
            set { PuzzleUrlString = value?.ToString(); }
        }

        [DataType(DataType.Url)]
        public string AnswerUrlString { get; set; }

        [NotMapped]
        public Uri AnswerUrl
        {
            get { Uri.TryCreate(AnswerUrlString, UriKind.RelativeOrAbsolute, out Uri result); return result; }
            set { AnswerUrlString = value?.ToString(); }
        }

        [DataType(DataType.Url)]
        public string MaterialsUrlString { get; set; }

        [NotMapped]
        public Uri MaterialsUrl
        {
            get { Uri.TryCreate(MaterialsUrlString, UriKind.RelativeOrAbsolute, out Uri result); return result; }
            set { MaterialsUrlString = value?.ToString(); }
        }
    }
}