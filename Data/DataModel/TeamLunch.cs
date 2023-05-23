using System.ComponentModel.DataAnnotations;

namespace ServerCore.DataModel
{
    public class TeamLunch
    {
        public int ID { get; set; }

        [Required]
        public int TeamId { get; set; }

        public virtual Team Team { get; set; }

        public string Lunch { get; set; }

        public string LunchModifications { get; set; }
    }
}
