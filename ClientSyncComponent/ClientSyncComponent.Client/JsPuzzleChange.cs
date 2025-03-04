namespace ClientSyncComponent.Client
{
    /// <summary>
    /// Javascript entity for puzzle state matching puzzle.js
    /// Naming convention matches Javascript for interoperability
    /// </summary>
#pragma warning disable IDE1006 // Naming Styles
    public class JsPuzzleChange
    {
        public string locationKey { get; set; }
        public string playerId { get; set; }
        public string propertyKey { get; set; }
        public string puzzleId { get; set; }
        public string teamId { get; set; }
        public string value { get; set; }
        public string channel { get; set; }
    }
#pragma warning restore IDE1006 // Naming Styles
}
