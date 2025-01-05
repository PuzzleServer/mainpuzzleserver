using System.ComponentModel.DataAnnotations.Schema;

namespace ServerCore.DataModel
{
    public class PuzzleAuthors
    {
        public PuzzleAuthors()
        {
        }

        public PuzzleAuthors(PuzzleAuthors source)
        {
            // do not fill out the ID
            Puzzle = source.Puzzle;
            Author = source.Author;
            SupportOnly = source.SupportOnly;
        }

        // ID for row
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }

        public int PuzzleID { get; set; }

        public virtual Puzzle Puzzle { get; set; }

        public int AuthorID { get; set; }

        public virtual PuzzleUser Author { get; set; }

        public bool SupportOnly { get; set; }
    }
}
