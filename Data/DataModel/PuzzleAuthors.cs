using System.ComponentModel.DataAnnotations.Schema;

namespace ServerCore.DataModel
{
    public class PuzzleAuthors
    {
        // ID for row
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }

        [ForeignKey("Puzzle.ID")]
        public virtual Puzzle Puzzle { get; set; }

        [ForeignKey("User.ID")]
        public virtual PuzzleUser Author { get; set; }
    }
}
