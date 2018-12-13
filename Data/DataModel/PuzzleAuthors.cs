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
        }

        // ID for row
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }

        [ForeignKey("Puzzle.ID")]
        public virtual Puzzle Puzzle { get; set; }

        [ForeignKey("User.ID")]
        public virtual PuzzleUser Author { get; set; }
    }
}
