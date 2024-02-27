using System;
using ServerCore.DataModel;

namespace ServerCore.Models
{
    /// <summary>
    /// A message about a puzzle.
    /// </summary>
    public class PuzzleMessage : MessageBase, IPuzzleMessage
    {
        public PuzzleMessage(int id, string threadId, string subject, Event evt, DateTime dateTimeInUtc, string text, PuzzleUser sender, Puzzle puzzle) 
            : base(id, threadId, subject, evt, dateTimeInUtc, text, sender)
        {
            this.Puzzle = puzzle;
        }

        /// <summary>
        /// Gets or sets the puzzle.
        /// </summary>
        public Puzzle Puzzle { get; }
    }
}
