using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace ServerCore.DataModel
{
    public class Submission
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }
        public virtual Puzzle Puzzle { get; set; }
        public virtual Team Team { get; set; }
        public virtual User Submitter { get; set; }
        public DateTime TimeSubmitted { get; set; }
        public string SubmissionText { get; set; }
        public virtual Response Response { get; set; }
    }
}
