using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ServerCore.DataModel
{
    /// <summary>
    /// Tracks hint unlocking for each team
    /// </summary>
    public class HintStatePerTeam
    {
        // Note: No ID here. See PuzzleServerContext.OnModelCreating for the declaration of the key.

        /// <summary>
        /// The team the hint applies to
        /// </summary>
        [ForeignKey("Team.ID")]
        public int TeamID { get; set; }

        public virtual Team Team { get; set; }

        /// <summary>
        /// The hint for the team
        /// </summary>
        [ForeignKey("Hint.ID")]
        public int HintID { get; set; }

        public virtual Hint Hint { get; set; }

        /// <summary>
        /// The time the hint was unlocked. Null if the hint is currently locked.
        /// </summary>
        public DateTime? UnlockTime { get; set; }

        /// <summary>
        /// True if the hint is unlocked for the team
        /// </summary>
        [NotMapped]
        public bool IsUnlocked
        {
            get
            {
                return UnlockTime != null;
            }
        }
    }
}
