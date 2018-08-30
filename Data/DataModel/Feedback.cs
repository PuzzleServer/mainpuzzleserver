using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace ServerCore.DataModel
{
    public class Feedback
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }
        public virtual Puzzle Puzzle { get; set; }
        public virtual User Submitter {get; set;}
        public DateTime SubmissionTime { get; set; }
        public string WrittenFeedback { get; set; }
        public int Difficulty { get; set; }
        public int Fun { get; set; }
    }
}
