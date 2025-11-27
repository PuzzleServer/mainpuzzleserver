using System;
using System.Diagnostics;

namespace ServerCore.ModelBases
{
    public class EventRole
    {
        // TODO: Expand to add impersonateplayer, when there is an advantage to doing so.
        // Current sticking point is whether the perf of sync is such that we can turn it on for single player pregame.
        // If sync is off for single payer puzzles, there's not much to see.

        public EventRoleType Type { get; private set; }
        public int ImpersonationId { get; private set; }
        public bool IsImpersonating => Type == EventRoleType.impersonateteam;

        public override bool Equals(object obj)
        {
            if (!(obj is EventRole)) return false;
            return this == (obj as EventRole);
        }

        public static bool operator ==(EventRole left, EventRole right)
        {
            if (Object.ReferenceEquals(left, null)) return Object.ReferenceEquals(right, null);
            return !Object.ReferenceEquals(right, null) && left.Type == right.Type && left.ImpersonationId == right.ImpersonationId;
        }

        public static bool operator !=(EventRole left, EventRole right)
        {
            return !(left == right);
        }

        public override string ToString()
        {
            return (IsImpersonating) ? $"{Type}-{ImpersonationId}" : Type.ToString();
        }

        public override int GetHashCode()
        {
            // revisit if we ever get more than 200M teams or players lol
            Debug.Assert((1 << 3) > (int)Type);
            return (ImpersonationId << 3) + (int)Type;
        }

        public static EventRole Parse(string s)
        {
            if (string.IsNullOrEmpty(s)) return null;
            s = s.ToLower();

            if (s.StartsWith("impersonateteam-"))
            {
                bool parse = int.TryParse(s.Substring("impersonateteam-".Length), out int id);
                if (!parse)
                {
                    return new EventRole() { Type = EventRoleType.play };
                }
                return new EventRole() { Type = EventRoleType.impersonateteam, ImpersonationId = id };
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
        impersonateteam
    }
}