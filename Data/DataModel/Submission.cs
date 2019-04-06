using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ServerCore.DataModel
{
    /// <summary>
    /// The answer submission to a puzzle
    /// </summary>
    public class Submission
    {
        private string submissionText = string.Empty;
        
        /// <summary>
        /// The ID
        /// </summary>
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }

        /// <summary>
        /// The puzzle the answer is for
        /// </summary>
        [Required]
        public virtual Puzzle Puzzle { get; set; }

        [ForeignKey("Puzzle")]
        public int PuzzleID { get; set; }

        /// <summary>
        /// The team giving the submission
        /// </summary>
        [Required]
        public virtual Team Team { get; set; }

        [ForeignKey("Team")]
        public int TeamID { get; set; }

        /// <summary>
        /// The exact user giving the submission
        /// </summary>
        [Required]
        public virtual PuzzleUser Submitter { get; set; }

        /// <summary>
        /// The time the submission was submitted
        /// </summary>
        public DateTime TimeSubmitted { get; set; }

        /// <summary>
        /// The actual submission text
        /// </summary>
        [Required]
        public string SubmissionText
        {
            get { return submissionText; }
            set { submissionText = Response.FormatSubmission(value); }
        }

        /// <summary>
        /// The response to the submission
        /// </summary>
        public virtual Response Response { get; set; }
    }
}
