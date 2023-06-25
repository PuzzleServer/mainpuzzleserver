using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace ServerCore.DataModel
{
    public class TeamMembers
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }

        /// <summary>
        /// Foreign Key - Team table
        /// </summary>
        [ForeignKey("Team.ID")]
        public Team Team { get; set; }

        /// <summary>
        /// Foreign Key - User table
        /// </summary>
        [ForeignKey("User.ID")]
        public User Member { get; set; }
    }
}
