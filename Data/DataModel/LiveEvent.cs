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
        public int ID { get; set; }

        [Required]
        public int AssociatedPuzzleId { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public virtual Puzzle AssociatedPuzzle { get; set; }

        /// <summary>
        /// The displayed location for the event (where to send people)
        /// </summary>
        [Required]
        public string Location { get; set; }

        /// <summary>
        /// The public time that the event ends (for all events)
        /// </summary>
        [Required]
        public DateTime EventEndTimeUtc { get; set; }

        /// <summary>
        /// A short description of the event, this should match the blurb on the puzzle page
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// The time that the event starts (for unscheduled events)
        /// </summary>
        public DateTime EventStartTimeUtc { get; set; }

        /// <summary>
        /// Whether or not the teams are assigned a specific time for this event
        /// </summary>
        public bool EventIsScheduled { get; set; }

        /// <summary>
        /// How many slots are available simultaneously
        /// Default is one, which allows for offset starts on multiple instances
        /// To have multiple instances start simultaneously set the number of instances
        /// </summary>
        public int NumberOfInstances { get; set; } = 1;

        /// <summary>
        /// How long each slot takes (include reset time)
        /// </summary>
        public TimeSpan TimePerSlot { get; set; }

        /// <summary>
        /// How many teams are scheduled in each slot
        /// </summary>
        public int TeamsPerSlot { get; set; }

        /// <summary>
        /// How long before the event opens to send a reminder for unscheduled events
        /// </summary>
        public TimeSpan OpeningReminderOffset { get; set; } = TimeSpan.FromMinutes(15);

        /// <summary>
        /// How long before an event closes to send a reminder (last call, for all events)
        /// </summary>
        public TimeSpan ClosingReminderOffset { get; set; } = TimeSpan.FromMinutes(30);

        /// <summary>
        /// How long before a team's scheduled time to send a first reminder
        /// </summary>
        public TimeSpan FirstReminderOffset { get; set; } = TimeSpan.FromMinutes(30);

        /// <summary>
        /// How long before a team's scheduled time to send a last reminder
        /// </summary>
        public TimeSpan LastReminderOffset { get; set; } = TimeSpan.FromMinutes(15);

        /// <summary>
        /// The last time that any notification was sent to all teams regarding this event
        /// </summary>
        public DateTime LastNotifiedAllTeamsUtc { get; set; } = DateTime.MinValue;
    }
}
