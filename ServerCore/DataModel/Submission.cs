using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ServerCore.DataModel
{
    public class Submission
    {
        public int ID { get; set; }
        public Puzzle Puzzle { get; set; }
        public Team Team { get; set; }
        public User Submitter { get; set; }
        public DateTime TimeSubmitted { get; set; }
        public string SubmissionText { get; set; }
        public Response Response { get; set; }
    }
}
