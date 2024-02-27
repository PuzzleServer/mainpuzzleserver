using ServerCore.DataModel;

namespace ServerCore.Models
{
    /// <summary>
    /// Interface for all messages about puzzles.
    /// </summary>
    public interface IPuzzleMessage
    {
        /// <summary>
        /// Gets the puzzle.
        /// </summary>
        Puzzle Puzzle { get; }
    }
}
