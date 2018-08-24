using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace ServerCore.DataModel
{
    public class Prerequisites
    {
        // ID for row
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }

        [ForeignKey("Puzzle.ID")]
        public Puzzle Puzzle { get; set; }

        [ForeignKey("Puzzle.ID")]
        public Puzzle Prerequisite { get; set; }
    }
}
