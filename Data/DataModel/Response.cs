using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using System.Text.RegularExpressions;

namespace ServerCore.DataModel
{
    /// <summary>
    /// The response to a particular submission for a given puzzle
    /// </summary>
    public class Response
    {
        public Response()
        {
        }

        public Response(Response source)
        {
            // do not fill out the ID
            Puzzle = source.Puzzle;
            IsSolution = source.IsSolution;
            SubmittedText = source.SubmittedText;
            ResponseText = source.ResponseText;
            Note = source.Note;
        }

        [NotMapped]
        private string submittedText = string.Empty;

        /// <summary>
        /// The Id
        /// </summary>
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }
        
        /// <summary>
        /// The id of the puzzle the response is for
        /// </summary>
        public int PuzzleID { get; set; }

        /// <summary>
        /// The puzzle the response is for
        /// </summary>
        public virtual Puzzle Puzzle { get; set; }

        /// <summary>
        /// True if the submitted text is considered a solution to the puzzle
        /// </summary>
        public bool IsSolution { get; set; }

        /// <summary>
        /// The submitted text
        /// </summary>
        [Required]
        public string SubmittedText
        {
            get { return submittedText; }
            set { submittedText = FormatSubmission(value); }
        }

        /// <summary>
        /// The response text
        /// </summary>
        [Required]
        public string ResponseText { get; set; }

        /// <summary>
        /// Any additional notes from the author
        /// </summary>
        public string Note { get; set; }

        /// <summary>
        /// The plaintext responseText of the puzzle, which is a subset of the responseText when the responseText is of the form:
        /// {plaintextResponseText}Html.Raw({htmlResponseText})
        /// </summary>
        public string GetPlaintextResponseText(int eventId)
        {
            string responseText = this.ResponseText;
            if (responseText != null && responseText.EndsWith(")") && responseText.Contains("Html.Raw("))
            {
                responseText = responseText.Replace("{eventId}", $"{eventId}");
                responseText = responseText.Substring(0, responseText.IndexOf("Html.Raw(")).TrimEnd();
            }
            return string.IsNullOrEmpty(responseText) ? this.ResponseText : responseText;
        }
        
        /// <summary>
        /// Converts the submission to the common spaceless uppercase format used by the website
        /// </summary>
        /// <param name="submission">The submission text</param>
        /// <returns>The formatted submission</returns>
        public static string FormatSubmission(string submission)
        {
            if (submission == null)
            {
                return string.Empty;
            }

            // Use Compatibility Decomposition (FormKD) so that
            // 1. Compatible equivalents like exponents, ligatures, and Roman numerals are converted into preserved common characters.
            // 2. Canonical composite characters like the accented e (\u00E9) characters in re'sume' are decomposed to preserve the starter characters.
            if (!submission.IsNormalized(NormalizationForm.FormKD))
            {
                submission = submission.Normalize(NormalizationForm.FormKD);
            }

            return Regex.Replace(submission, @"[^a-zA-Z\d]", string.Empty).ToUpperInvariant();
        }
    }
}
