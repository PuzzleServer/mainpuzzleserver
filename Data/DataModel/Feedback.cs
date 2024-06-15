using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace ServerCore.DataModel
{
    public class Feedback
    {
        public Feedback()
        {
            Fun = AvgRating;
            Difficulty = AvgRating;
        }

        public const int MinRating = 1;
        public const int AvgRating = 5;
        public const int MaxRating = 10;

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }

        public int PuzzleID { get; set; }

        [Required]
        public virtual Puzzle Puzzle { get; set; }

        public int SubmitterID { get; set; }

        /// <summary>
        /// The user who submitted the feedback>
        /// </summary>
        [Required]
        public virtual PuzzleUser Submitter {get; set;}

        /// <summary>
        /// The time the feedback was submitted.
        /// </summary>
        public DateTime SubmissionTime { get; set; }

        /// <summary>
        /// The feedback text submitted by the user.
        /// </summary>
        [MaxLength(500)]
        public string WrittenFeedback { get; set; }

        /// <summary>
        /// The user submitted difficulty score for the puzzle.
        /// </summary>
        [Range(MinRating, MaxRating)]
        public int Difficulty { get; set; }

        /// <summary>
        /// The user submitted fun score for the puzzle.
        /// </summary>
        [Range(MinRating, MaxRating)]
        public int Fun { get; set; }
    }
}
