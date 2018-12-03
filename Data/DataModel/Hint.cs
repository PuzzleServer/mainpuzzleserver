using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace ServerCore.DataModel
{
    /// <summary>
    /// Hint information for a puzzle
    /// </summary>
    public class Hint
    {
        /// <summary>
        /// The ID
        /// </summary>
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        
        /// <summary>
        /// The puzzle this is a hint for
        /// </summary>
        [Required]
        public virtual Puzzle Puzzle { get; set; }

        /// <summary>
        /// The description players will see before unlocking the hint
        /// </summary>
        [Required]
        public string Description { get; set; }

        /// <summary>
        /// The contents of the hint players will unlock
        /// </summary>
        [Required]
        public string Content { get; set; }

        /// <summary>
        /// The number of hint coins needed to unlock this hint
        /// </summary>
        public int Cost { get; set; }

        public int DisplayOrder { get; set; }
    }
}
