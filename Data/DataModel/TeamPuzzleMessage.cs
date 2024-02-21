namespace ServerCore.DataModel
{
    /// <summary>
    /// A message about a specific puzzle from a team.
    /// </summary>
    public class TeamPuzzleMessage : PuzzleMessage
    {
        public TeamPuzzleMessage() 
        {
            this.IsFromGameControl = false;
        }

        /// <summary>
        /// Gets or sets the team id.
        /// </summary>
        public int TeamID { get; set; }

        /// <summary>
        /// Gets or sets the team.
        /// </summary>
        public virtual Team Team { get; set; }
    }
}
