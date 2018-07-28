using System;

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
        public Uri PuzzleUrl { get; set; }
        public Uri AnswerUrl { get; set; }
        public Uri MaterialsUrl { get; set; }

        // TODO: Whatever we need to allow unlocking
    }
}