using ServerCore.DataModel;

namespace ServerCore.Models
{
    /// <summary>
    /// Interface for all messages that do not come from game control.
    /// </summary>
    public interface INonGameControlMessage
    {
        /// <summary>
        /// Gets or sets if the puzzle is claimed by someone from game control.
        /// </summary>
        bool IsClaimed { get; }

        /// <summary>
        /// Gets or sets the game control user that claimed the message.
        /// </summary>
        PuzzleUser Claimer { get; }
    }
}

