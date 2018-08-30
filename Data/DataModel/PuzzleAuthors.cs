using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace ServerCore.DataModel
{
    public class PuzzleAuthors
    {
        // ID for row
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }

        [ForeignKey("Puzzle.ID")]
        public virtual Puzzle Puzzle { get; set; }

        [ForeignKey("User.ID")]
        public virtual User Author { get; set; }
    }
}
