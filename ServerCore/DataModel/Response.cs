using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ServerCore.DataModel
{
    public class Response
    {
        public int ID { get; set; }
        public Puzzle Puzzle {get; set;}
        public bool IsSolution { get; set; }
        public string SubmittedText { get; set; }
        public string Note { get; set; }
    }
}
