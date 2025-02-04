using Azure;
using Azure.Data.Tables;

namespace ClientSyncComponent.Client
{
    public class PuzzleEntry : ITableEntity
    {
        public PuzzleEntry(int puzzleId, int teamId, int playerId, string locationKey, string propertyKey, string value)
        {
            PartitionKey = $"{puzzleId}_{teamId}";
            RowKey = $"{locationKey}_{propertyKey}";
            LocationKey = locationKey;
            PropertyKey = propertyKey;
            Value = value;
        }

        public string PartitionKey { get; set; }
        public string RowKey { get; set; }
        public string LocationKey { get; set; }
        public string PropertyKey { get; set; }
        public string Value { get; private set; }

        // Set automatically by the Table service
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }
    }
}
