namespace ServerCore.Helpers
{
    using System.Collections.Generic;
    using System.Linq;
    using ServerCore.DataModel;

    /// <summary>
    /// Utilities for working with Messages
    /// </summary>
    static class MessageHelper
    {
        public static string GetTeamPuzzleThreadId(int puzzleId, int teamId)
        {
            return $"Puzzle_{puzzleId}_{teamId}";
        }

        public static string GetSinglePlayerPuzzleThreadId(int puzzleId, int playerId)
        {
            return $"SinglePlayerPuzzle_{puzzleId}_{playerId}";
        }
    }
}
