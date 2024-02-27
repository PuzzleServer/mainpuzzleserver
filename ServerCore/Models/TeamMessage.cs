namespace ServerCore.Models
{
    using System;
    using ServerCore.DataModel;

    public class TeamMessage : MessageBase, ITeamMessage, INonGameControlMessage
    {
        public TeamMessage(
            int id,
            string threadId,
            string subject,
            Event evt,
            DateTime dateTimeInUtc,
            string text,
            PuzzleUser sender,
            Team team,
            bool isClaimed,
            PuzzleUser claimer)
            : base(id, threadId, subject, evt, dateTimeInUtc, text, sender)
        {
            this.Team = team;
            this.IsClaimed = isClaimed;
            this.Claimer = claimer;
        }

        public Team Team { get; }

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
