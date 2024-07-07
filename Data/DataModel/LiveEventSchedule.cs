using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System;

namespace ServerCore.DataModel
{
    public class LiveEventSchedule
    {
        /// <summary>
        /// The ID
        /// </summary>
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public int LiveEventId { get; set; }

        [Required]
        public int TeamId { get; set; }

        [Required]
        public DateTime StartTime { get; set; }

        public LiveEvent LiveEvent { get; set; }
    }
}
