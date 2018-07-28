using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ServerCore.DataModel
{
    public class Feedback
    {
        public int ID { get; set; }
        public Puzzle Puzzle { get; set; }
        public User Submitter {get; set;}
        public DateTime SubmissionTime { get; set; }
        public string WrittenFeedback { get; set; }
        public int Difficulty { get; set; }
        public int Fun { get; set; }
    }
}
