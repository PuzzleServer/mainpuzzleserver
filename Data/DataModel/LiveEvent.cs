using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System;

namespace ServerCore.DataModel
{
    public class LiveEvent
    {
        /// <summary>
        /// The ID
        /// </summary>
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public int AssociatedPuzzleId { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public Puzzle AssociatedPuzzle { get; set; }

        public string Location { get; set; }

        public string Description { get; set; }

        public DateTime EventStartTime { get; set; }

        public DateTime EventEndTime { get; set; }

        public bool EventIsScheduled { get; set; }

        public TimeSpan TimePerSlot { get; set; }

        public int TeamsPerSlot { get; set; }

        public TimeSpan ClosingReminderOffset { get; set; }

        public TimeSpan FirstReminderOffset { get; set; }

        public TimeSpan LastReminderOffset { get; set; }
    }
}
