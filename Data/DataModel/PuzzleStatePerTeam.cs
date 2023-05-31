namespace ServerCore.DataModel
{
    /// <summary>
    /// The state of a non single-player puzzle for a particular team.
    /// </summary>
    public class PuzzleStatePerTeam : PuzzleStateBase
    {
        public int TeamID { get; set; }
        public virtual Team Team { get; set; }
    }
}
