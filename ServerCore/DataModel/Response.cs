using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace ServerCore.DataModel
{
    public class Response
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }
        public Puzzle Puzzle {get; set;}
        public bool IsSolution { get; set; }
        public string SubmittedText { get; set; }
        public string Note { get; set; }
    }
}
