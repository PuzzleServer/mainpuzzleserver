using System;
using System.Collections.Generic;
using System.Text;

namespace ServerCore.DataModel
{
    /// <summary>
    /// Tracks a user's request to join a team
    /// </summary>
    class TeamApplication
    {
        /// <summary>
        /// The user who wants to join the team
        /// </summary>
        public PuzzleUser Player { get; set; }

        /// <summary>
        /// The team the player wants to join
        /// </summary>
        public Team Team { get; set; }
    }
}
