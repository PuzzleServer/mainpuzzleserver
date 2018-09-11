﻿using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace ServerCore.DataModel
{
    /// <summary>
    /// The answer submission to a puzzle
    /// </summary>
    public class Submission
    {
        private string submission = string.Empty;
        
        /// <summary>
        /// The ID
        /// </summary>
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }

        /// <summary>
        /// The puzzle the answer is for
        /// </summary>
        public virtual Puzzle Puzzle { get; set; }

        /// <summary>
        /// The team giving the submission
        /// </summary>
        public virtual Team Team { get; set; }

        /// <summary>
        /// The exact user giving the submission
        /// </summary>
        public virtual User Submitter { get; set; }

        /// <summary>
        /// The time the submission was submitted
        /// </summary>
        public DateTime TimeSubmitted { get; set; }

        /// <summary>
        /// The actual submission text
        /// </summary>
        public string SubmissionText
        {
            get { return this.submission; }
            set { this.submission = Response.FormatSubmission(value); }
        }

        /// <summary>
        /// The response to the submission
        /// </summary>
        public virtual Response Response { get; set; }
    }
}
