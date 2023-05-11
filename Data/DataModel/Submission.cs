using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ServerCore.DataModel
{
    /// <summary>
    /// The answer submission to a puzzle
    /// </summary>
    public class Submission : SubmissionBase
    {
        /// <summary>
        /// The team giving the submission
        /// </summary>
        [Required]
        public virtual Team Team { get; set; }

        [ForeignKey("Team")]
        public int TeamID { get; set; }
    }
}
