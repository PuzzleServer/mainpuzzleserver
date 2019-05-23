using System.ComponentModel.DataAnnotations;

namespace ServerCore.DataModel
{
    public class Swag
    {
        public int ID { get; set; }

        [Required]
        public int EventId { get; set; }

        public virtual Event Event { get; set; }

        [Required]
        public int PlayerId { get; set; }

        public virtual PuzzleUser Player { get; set; }

        [Display(Name ="Lunch Selection")]
        public string Lunch { get; set; }

        [Display(Name = "Dietary Modifications (e.g. no cheese)")]
        public string LunchModifications { get; set; }

        [Display(Name = "T-Shirt Size")]
        public string ShirtSize { get; set; }
    }
}
