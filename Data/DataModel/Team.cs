using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ServerCore.DataModel
{
    [Flags]
    public enum AutoTeamType
    {
        // Experience level
        Beginner = 0x1,
        Expert = 0x2,

        // Commitment level
        Casual = 0x4,
        Serious = 0x8,
    }

    public class Team
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }

        /// <summary>
        /// The event the team is a part of
        /// </summary>
        public int EventID { get; set; }

        /// <summary>
        /// The event the team is a part of
        /// </summary>
        [Required]
        public virtual Event Event { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        /// <summary>
        /// String formatted rooms for events that don't pre-reserve rooms
        /// </summary>
        [MaxLength(200)]
        public string CustomRoom { get; set; }

        public virtual List<Invitation> Invitations { get; set; }

        [Required]
        [MaxLength(500)] // Needs to be long enough to support adding every team member's email
        public string PrimaryContactEmail { get; set; }
        [Phone]
        [MaxLength(20)]
        public string PrimaryPhoneNumber { get; set; }
        [Phone]
        [MaxLength(20)]
        public string SecondaryPhoneNumber { get; set; }
        
        /// <summary>
        /// True if a team should show up on the list of teams accepting applications
        /// </summary>
        public bool IsLookingForTeammates { get; set; }

        /// <summary>
        /// True if applications to a team should be automatically approved without their intervention
        /// </summary>
        public bool AutoApproveTeammates { get; set; }


        public virtual List<Submission> Submissions { get; set; }

        /// <summary>
        /// The number of hint coins this team currently has
        /// </summary>
        public int HintCoinCount { get; set; }

        /// <summary>
        /// The number of hint coins this team currently has used
        /// </summary>
        public int HintCoinsUsed { get; set; }

        /// <summary>
        /// The number of hint coins this team currently has earned since the event began
        /// </summary>
        [NotMapped]
        public int HintCoinsEarned
        {
            get
            {
                return this.HintCoinCount + this.HintCoinsUsed;
            }
        }

        /// <summary>
        /// Team bio for the signups page
        /// </summary>
        [MaxLength(500)]
        public string Bio { get; set; }

        /// <summary>
        /// Machine generated password for the autoinvite link
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// Indicates whether or not the team is disqualified. Disqualified teams
        /// can still view and solve puzzles; however, they are hidden from the
        /// standings and fastest solves pages.
        /// </summary>
        public bool IsDisqualified { get; set; }

        /// <summary>
        /// The list of team names that have been merged into this team.  
        /// If no teams have been merged into this one, this is blank.  
        /// Since multiple teams can be merged into a single team, 
        /// and team names can contain any character, each team name is
        /// URL encoded before being added to this string so that we can
        /// use spaces to delimit multiple team names in this single string.
        /// </summary>
        public string MergedTeams { get; set; }

        /// <summary>
        /// Show the team announcement for this team
        /// </summary>
        public bool ShowTeamAnnouncement { get; set; }

        /// <summary>
        /// If the team is automatically created, contains which type of players it's for.
        /// Null if the team is manually created.
        /// </summary>
        public AutoTeamType? AutoTeamType { get; set; }
    }
}
