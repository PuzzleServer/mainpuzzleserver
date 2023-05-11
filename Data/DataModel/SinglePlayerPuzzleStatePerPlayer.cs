namespace ServerCore.DataModel
{
    public class SinglePlayerPuzzleStatePerPlayer : PuzzleStateBase
    {
        public int UserID { get; set; }
        public virtual PuzzleUser User { get; set; }
    }
}
