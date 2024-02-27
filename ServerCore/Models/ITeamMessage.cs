using ServerCore.DataModel;

namespace ServerCore.Models
{
    /// <summary>
    /// Interface for all messages from a team rather than from a single player.
    /// </summary>
    public interface ITeamMessage
    {
        /// <summary>
        /// Gets the team.
        /// </summary>
        Team Team { get; }
    }
}
