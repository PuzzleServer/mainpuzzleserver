using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace ServerCore.DataModel
{
    /// <summary>
    /// Tracks a user's request to join a team
    /// </summary>
    public class TeamApplication
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }

        public int TeamID { get; set; }

        /// <summary>
        /// The team the player wants to join
        /// </summary>
        [Required]
        public virtual Team Team { get; set; }

        public int PlayerID { get; set; }

        /// <summary>
        /// The user who wants to join the team
        /// </summary>
        [Required]
        public virtual PuzzleUser Player { get; set; }
    }
}
