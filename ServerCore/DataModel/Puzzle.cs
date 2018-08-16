using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ServerCore.DataModel
{
    public class Puzzle
    {
        public int ID { get; set; }
        public Event Event { get; set; }
        public string Name { get; set; }
        public bool IsPuzzle { get; set; }
        public bool IsMetaPuzzle { get; set; }
        public bool IsFinalPuzzle { get; set; }
        public int FirstSolveValue { get; set; }
        public int MinValue { get; set; }
        public int PerSolvePenalty { get; set; }

        [DataType(DataType.Url)]
        public string PuzzleUrlString { get; set; }

        [NotMapped]
        public Uri PuzzleUrl
        {
            get { return new Uri(PuzzleUrlString); }
            set { PuzzleUrlString = value.ToString(); }
        }

        [DataType(DataType.Url)]
        public string AnswerUrlString { get; set; }

        [NotMapped]
        public Uri AnswerUrl
        {
            get { return new Uri(AnswerUrlString); }
            set { AnswerUrlString = value.ToString(); }
        }

        [DataType(DataType.Url)]
        public string MaterialsUrlString { get; set; }

        [NotMapped]
        public Uri MaterialsUrl
        {
            get { return new Uri(MaterialsUrlString); }
            set { MaterialsUrlString = value.ToString(); }
        }

        // TODO: Whatever we need to allow unlocking
    }
}