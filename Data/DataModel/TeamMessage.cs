namespace ServerCore.DataModel
{
    /// <summary>
    /// A message from a team.
    /// </summary>
    public class TeamMessage : Message
    {
        public TeamMessage()
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
