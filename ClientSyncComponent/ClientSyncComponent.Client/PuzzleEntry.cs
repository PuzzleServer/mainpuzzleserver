using Azure;
using Azure.Data.Tables;

namespace ClientSyncComponent.Client
{
    public class PuzzleEntry : ITableEntity
    {
        // Default constructor required by Azure query API
        public PuzzleEntry()
        {
        }
        
        // todo morganb: translate from string player name to playerId
        public PuzzleEntry(int puzzleId, int teamId, int playerId, string locationKey, string propertyKey, string value, string channel)
        {
            PartitionKey = CreatePartitionKey(puzzleId, teamId);
            RowKey = CreateRowKey(locationKey, propertyKey);
            LocationKey = locationKey;
            PropertyKey = propertyKey;
            Value = value;
            PlayerId = playerId;
            Channel = channel;
        }

        private static string CreateRowKey(string locationKey, string propertyKey)
        {
            return $"{propertyKey}|{locationKey}";
        }

        public static string CreatePartitionKey(int puzzleId, int teamId)
        {
            return $"{puzzleId}_{teamId}";
        }

        public string PartitionKey { get; set; }
        public string RowKey { get; set; }
        public string LocationKey { get; set; }
        public string PropertyKey { get; set; }
        public string Value { get; set; }
        public int PlayerId { get; set; }
        public string Channel { get; set; }

        // Set automatically by the Table service
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }
    }
}
