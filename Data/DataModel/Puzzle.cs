using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ServerCore.DataModel
{
    public class Puzzle
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }
        public Event Event { get; set; }
        public string Name { get; set; }
        public bool IsPuzzle { get; set; } = false;
        public bool IsMetaPuzzle { get; set; } = false;
        public bool IsFinalPuzzle { get; set; } = false;
        public int FirstSolveValue { get; set; } = 0;
        public int MinValue { get; set; } = 0;
        public int PerSolvePenalty { get; set; } = 0;

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

        // TODO: Whatever we need to allow unlocking
    }
}