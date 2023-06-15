using System.ComponentModel.DataAnnotations;

namespace ServerCore.DataModel
{
    public class PlayerInEvent
    {
        public int ID { get; set; }

        [Required]
        public int EventId { get; set; }

        public virtual Event Event { get; set; }

        [Required]
        public int PlayerId { get; set; }

        public virtual PuzzleUser Player { get; set; }

        public string Lunch { get; set; }

        public string LunchModifications { get; set; }

        public string ShirtSize { get; set; }

        public bool IsRemote { get; set; }
    }
}
