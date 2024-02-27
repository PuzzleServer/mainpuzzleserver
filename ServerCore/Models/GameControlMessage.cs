namespace ServerCore.Models
{
    using System;
    using ServerCore.DataModel;

    public class GameControlMessage : MessageBase, IGameControlMessage
    {
        public GameControlMessage(int id, string threadId, string subject, Event evt, DateTime dateTimeInUtc, string text, PuzzleUser sender)
            : base(id, threadId, subject, evt, dateTimeInUtc, text, sender)
        {
        }
    }
}
