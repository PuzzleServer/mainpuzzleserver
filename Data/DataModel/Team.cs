using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ServerCore.DataModel
{
    public class Team
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }
        
        public int EventID { get; set; }
        [Required]
        public virtual Event Event { get; set; }

        [Required]
        [RegularExpression("\\S+.*")]
        public string Name { get; set; }

        /// <summary>
        /// Cross reference for pre-reserved rooms (PD only)
        /// </summary>
        public int? RoomID { get; set; }

        /// <summary>
        /// String formatted rooms for events that don't pre-reserve rooms
        /// </summary>
        public string CustomRoom { get; set; }

        public virtual List<Invitation> Invitations { get; set; }

        public string PrimaryContactEmail { get; set; }
        public string PrimaryPhoneNumber { get; set; }
        public string SecondaryPhoneNumber { get; set; }
        public bool IsLookingForTeammates { get; set; }

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
        /// Team bio for the signups page
        /// </summary>
        public string Bio { get; set; }

        /// <summary>
        /// Machine generated password for the autoinvite link
        /// </summary>
        public string Password { get; set; }
    }
}
