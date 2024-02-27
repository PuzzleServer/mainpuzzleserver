namespace ServerCore.Models
{
    using System;
    using ServerCore.DataModel;

    public class GameControlPuzzleMessage : MessageBase, IGameControlMessage, IPuzzleMessage
    {
        public GameControlPuzzleMessage(int id, string threadId, string subject, Event evt, DateTime dateTimeInUtc, string text, PuzzleUser sender, Puzzle puzzle)
            : base(id, threadId, subject, evt, dateTimeInUtc, text, sender)
        {
            this.Puzzle = puzzle;
        }

        public Puzzle Puzzle { get; }
    }
}
