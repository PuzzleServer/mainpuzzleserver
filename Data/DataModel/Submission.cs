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

        public int SubmitterID { get; set; }

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

        /// <summary>
        /// Feedback from freeform judging
        /// </summary>
        public string FreeformResponse { get; set; }

        /// <summary>
        /// True if freeform accepted, false if rejected, null if not judged yet
        /// </summary>
        public bool? FreeformAccepted { get; set; }

        /// <summary>
        /// The user who evaluated a freeform response
        /// </summary>
        public virtual PuzzleUser FreeformJudge { get; set; }

        /// <summary>
        /// True if the user allowed their freeform submission to be shared with other users
        /// </summary>
        public bool AllowFreeformSharing { get; set; }

        /// <summary>
        /// True if this submission was marked as a favorite
        /// </summary>
        public bool FreeformFavorited { get; set; }

        /// <summary>
        /// Submission text that should not be automatically formatted
        /// </summary>
        [NotMapped]
        public string UnformattedSubmissionText
        {
            get { return submissionText; }
            set { submissionText = value; }
        }
    }
}
