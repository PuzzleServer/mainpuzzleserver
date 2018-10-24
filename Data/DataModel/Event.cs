using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ServerCore.DataModel
{
    public class Event
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }

        [Required]
        public string Name { get; set; }

        [DataType(DataType.Url)]
        public string UrlString { get; set; }

        [DataType(DataType.EmailAddress)]
        public string ContactEmail { get; set; }

        [NotMapped]
        public Uri URL
        {
            get { Uri.TryCreate(UrlString, UriKind.RelativeOrAbsolute, out Uri result); return result; }
            set { UrlString = value?.ToString(); }
        }

        public int MaxNumberOfTeams { get; set; }
        public int MaxTeamSize { get; set; }
        public int MaxExternalsPerTeam { get; set; }
        public bool IsInternEvent { get; set; }
        public DateTime TeamRegistrationBegin { get; set; }
        public DateTime TeamRegistrationEnd { get; set; }

        /// <summary>
        /// Returns whether or not team registration is active.
        /// </summary>
        /// <returns>True if the current date is between the team registration begin and end times.</returns>
        [NotMapped]
        public bool IsTeamRegistrationActive
        {
            get
            {
                return DateTime.UtcNow.CompareTo(TeamRegistrationBegin) > 0 &&
                    DateTime.UtcNow.CompareTo(TeamRegistrationEnd) < 0;
            }
        }

        public DateTime TeamNameChangeEnd { get; set; }
        public DateTime TeamMembershipChangeEnd { get; set; }
        public DateTime TeamMiscDataChangeEnd { get; set; }
        public DateTime TeamDeleteEnd { get; set; }
        public DateTime EventBegin { get; set; }

        /// <summary>
        /// Returns whether or not the event has started. Does not necessarily indicate the event
        /// is currently active.
        /// </summary>
        /// <returns>True if the current date is after the EventBegin time</returns>
        [NotMapped]
        public bool EventHasStarted
        {
            get { return DateTime.UtcNow.CompareTo(EventBegin) > 0; }
        }

        public DateTime AnswerSubmissionEnd { get; set; }

        /// <summary>
        /// Returns whether or not answer submission is active.
        /// </summary>
        /// <returns>True if the current date is after the event start and before the answer submission end times.</returns>
        [NotMapped]
        public bool IsAnswerSubmissionActive
        {
            get
            {
                return DateTime.UtcNow.CompareTo(EventBegin) > 0 &&
                    DateTime.UtcNow.CompareTo(AnswerSubmissionEnd) < 0;
            }
        }

        public DateTime AnswersAvailableBegin { get; set; }

        /// <summary>
        /// Returns whether or not the puzzle answers should be available now.
        /// </summary>
        /// <returns>True if the current date is after the AnswersAvailableBegin time</returns>
        [NotMapped]
        public bool AreAnswersAvailableNow
        {
            get { return DateTime.UtcNow.CompareTo(AnswersAvailableBegin) > 0; }
        }

        /// <summary>
        /// Automatically makes the standings page available at the DateTime below
        /// </summary>
        public DateTime StandingsAvailableBegin { get; set; }

        /// <summary>
        /// Returns whether or not the standings page should be available now.
        /// </summary>
        /// <returns>True if the current date is after the StandingsAvailableBegin time</returns>
        [NotMapped]
        public bool AreStandingsAvailableNow
        {
            get { return DateTime.UtcNow.CompareTo(StandingsAvailableBegin) > 0; }
        }

        /// <summary>
        /// Allows event owners to hide standings during the event if they prefer - overrides the timed setting
        /// </summary>
        public bool StandingsOverride { get; set; }

        /// <summary>
        /// Determines whether or not the fastest solves page is visible to players
        /// </summary>
        public bool ShowFastestSolves { get; set; }

        /// <summary>
        /// Whether or not the suthors are accepting feedback for the event - commonly true for betas and false for live events
        /// </summary>
        public bool AllowFeedback { get; set; }

        // TODO: These might need to be collections that aren't a db column - check on the EF documentation for referencing join tables where it's a one to many
        public virtual EventTeams Teams { get; set; }
        public virtual EventAuthors Authors { get; set; }
        public virtual EventAdmins Admins { get; set; }

        /// <summary>
        /// The window of time where if a team spams enough incorrect answers
        /// causes the team to be locked out of submitting additional answers.
        /// </summary>
        public double LockoutSpamDuration { get; set; }

        /// <summary>
        /// The amount if incorrect answers required to cause a lockout.
        /// </summary>
        public uint LockoutSpamCount { get; set; }

        /// <summary>
        /// The multiplier for the lockout duration for consecutive lockouts.
        /// </summary>
        public double LockoutDurationMultiplier { get; set; }

        /// <summary>
        /// If this amount of time passes after a team's lockout ends, then
        /// the team's lockout stage resets to the initial value.
        /// </summary>
        public double LockoutForgivenessTime { get; set; }

        /// <summary>
        /// The maximum number of incorrect submissions a team can have for
        /// a single puzzle before being locked out entirely and being placed
        /// in email-only mode.
        /// </summary>
        public uint MaxSubmissionCount { get; set; }
    }
}
