using System;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ServerCore.ModelBases
{
    public class EventRole
    {
        public EventRoleType Type { get; private set; }
        public int? ImpersonatingTeamId { get; private set; }

        public override bool Equals(object obj)
        {
            if (!(obj is EventRole)) return false;
            EventRole objRole = obj as EventRole;
            return this.Type == objRole.Type && this.ImpersonatingTeamId == objRole.ImpersonatingTeamId;
        }

        public static bool operator ==(EventRole left, EventRole right)
        {
            if (Object.ReferenceEquals(left, null)) return Object.ReferenceEquals(right, null);
            return !Object.ReferenceEquals(right, null) && left.Type == right.Type && left.ImpersonatingTeamId == right.ImpersonatingTeamId;
        }

        public static bool operator !=(EventRole left, EventRole right)
        {
            if (Object.ReferenceEquals(left, null)) return !Object.ReferenceEquals(right, null);
            return Object.ReferenceEquals(right, null) || left.Type != right.Type || left.ImpersonatingTeamId != right.ImpersonatingTeamId;
        }

        public override string ToString()
        {
            return (Type == EventRoleType.impersonate) ? $"{Type}-{ImpersonatingTeamId}" : Type.ToString();
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public static EventRole Parse(string s)
        {
            if (string.IsNullOrEmpty(s)) return null;
            s = s.ToLower();

            if (s.StartsWith("impersonate-"))
            {
                bool parse = int.TryParse(s.Substring("impersonate-".Length), out int id);
                if (!parse)
                {
                    return new EventRole() { Type = EventRoleType.play };
                }
                return new EventRole() { Type = EventRoleType.impersonate, ImpersonatingTeamId = id };
            }
            else
            {
                EventRoleType type = Enum.IsDefined(typeof(EventRoleType), s) ? Enum.Parse<EventRoleType>(s): EventRoleType.play;
                return new EventRole() { Type = type };
            }
        }

        public static EventRole admin = new EventRole() { Type = EventRoleType.admin };
        public static EventRole author = new EventRole() { Type = EventRoleType.author };
        public static EventRole play = new EventRole() { Type = EventRoleType.play };
    }

    public enum EventRoleType
    {
        admin = 1,
        author,
        play,
        impersonate
    }
}