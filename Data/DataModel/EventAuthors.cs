using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace ServerCore.DataModel
{
    /// <summary>
    /// The authors who have written for this event
    /// </summary>
    public class EventAuthors
    {
        // ID for row
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }

        /// <summary>
        /// Foreign Key - event table
        /// </summary>
        [ForeignKey("Event.ID")]
        public virtual Event Event { get; set; }

        /// <summary>
        /// Foreign Key - user table (author)
        /// </summary>
        [ForeignKey("User.ID")]
        public virtual User Author { get; set; }
    }
}
