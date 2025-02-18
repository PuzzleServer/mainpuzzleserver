﻿namespace ClientSyncComponent.Client
{
    /// <summary>
    /// Javascript entity for puzzle state matching puzzle.js
    /// </summary>
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
}
