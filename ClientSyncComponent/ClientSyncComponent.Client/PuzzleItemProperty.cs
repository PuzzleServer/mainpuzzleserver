using Azure;
using Azure.Data.Tables;

namespace ClientSyncComponent.Client
{
    /// <summary>
    /// Azure Table entity for puzzle state
    /// </summary>
    public class PuzzleItemProperty : ITableEntity
    {
        // Default constructor required by Azure query API
        public PuzzleItemProperty()
        {
        }
        
        // todo morganb: translate from string player name to playerId
        public PuzzleItemProperty(int puzzleId, int teamId, int playerId, string subPuzzleId, string locationKey, string propertyKey, string value, string channel)
        {
            PartitionKey = CreatePartitionKey(puzzleId, teamId);
            RowKey = CreateRowKey(subPuzzleId, locationKey, propertyKey);
            SubPuzzleId = subPuzzleId;
            LocationKey = locationKey;
            PropertyKey = propertyKey;

            // Azure tables can't store null values, so substitute empty string (which puzzle.js can handle equivalently to null)
            Value = value ?? String.Empty;
            PlayerId = playerId;
            Channel = channel;
        }

        private static string CreateRowKey(string subPuzzleId, string locationKey, string propertyKey)
        {
            return $"{subPuzzleId}|{propertyKey}|{locationKey}";
        }

        public static string CreatePartitionKey(int puzzleId, int teamId)
        {
            return $"{puzzleId}_{teamId}";
        }

        public string PartitionKey { get; set; }
        public string RowKey { get; set; }
        public string SubPuzzleId { get; set; }
        public string LocationKey { get; set; }
        public string PropertyKey { get; set; }
        public string Value { get; set; }
        public int PlayerId { get; set; }
        /// <summary>
        /// TODO: Implement channels beyond having the property
        /// </summary>
        public string Channel { get; set; }

        // Set automatically by the Table service
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }
    }
}
