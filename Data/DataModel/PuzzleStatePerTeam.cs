namespace ServerCore.DataModel
{
    public class PuzzleStatePerTeam : PuzzleStateBase
    {
        public int TeamID { get; set; }
        public virtual Team Team { get; set; }
    }
}
