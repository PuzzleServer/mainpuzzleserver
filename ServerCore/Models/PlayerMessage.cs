namespace ServerCore.Models
{
    using System;
    using ServerCore.DataModel;

    public class PlayerMessage : MessageBase, INonGameControlMessage
    {
        public PlayerMessage(
            int id,
            string threadId,
            string subject,
            Event evt,
            DateTime dateTimeInUtc,
            string text,
            PuzzleUser sender,
            bool isClaimed,
            PuzzleUser claimer)
            : base(id, threadId, subject, evt, dateTimeInUtc, text, sender)
        {
            this.IsClaimed = isClaimed;
            this.Claimer = claimer;
        }

        public bool IsClaimed { get; private set; }

        public PuzzleUser Claimer { get; private set; }

        public void SetClaimer(PuzzleUser claimer)
        {
            this.IsClaimed = true;
            this.Claimer = claimer;
        }

        public void ClearClaimer()
        {
            this.IsClaimed = false;
            this.Claimer = null;
        }
    }
}
