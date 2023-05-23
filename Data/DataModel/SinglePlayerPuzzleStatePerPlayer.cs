namespace ServerCore.DataModel
{
    /// <summary>
    /// The state of a single player puzzle for a particular player.
    /// </summary>
    public class SinglePlayerPuzzleStatePerPlayer : PuzzleStateBase
    {
        public int PlayerID { get; set; }
        public virtual PuzzleUser Player { get; set; }
    }
}
