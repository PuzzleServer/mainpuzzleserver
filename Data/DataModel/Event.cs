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

        /// <summary>
        /// The URL prefix of the event, as in: www.puzzlehunt.org/urlString
        /// </summary>
        public string UrlString { get; set; }

        [NotMapped]
        public string EventID => UrlString ?? ID.ToString();

        /// <summary>
        /// I was unable to put this in a separate ImportEvent model in import.cshtml.cs;
        /// for some reason SelectList did not like those type of items.
        /// Kludging the info here although I really shouldn't have to...
        /// </summary>
        [NotMapped]
        public string NameAndContainer => $"{Name} (evt{ID})";

        /// <summary>
        /// The prefix of the partial files that provide the Home, FAQ, and Rules content.
        /// </summary>
        public string HomePartial { get; set; }

        [DataType(DataType.EmailAddress)]
        public string ContactEmail { get; set; }

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
                return DateTime.UtcNow >= TeamRegistrationBegin &&
                    DateTime.UtcNow <= TeamRegistrationEnd;
            }
        }

        /// <summary>
        /// Returns whether or not teams are allowed to change their members
        /// </summary>
        /// <returns>True if the current date is between the team registration begin and membership change end times.</returns>
        [NotMapped]
        public bool IsTeamMembershipChangeActive
        {
            get
            {
                return DateTime.UtcNow >= TeamRegistrationBegin &&
                    DateTime.UtcNow <= TeamMembershipChangeEnd;
            }
        }

        public DateTime TeamNameChangeEnd { get; set; }

        /// <summary>
        /// Returns whether or not team names can be changed.
        /// </summary>
        /// <returns>True if the current date is before the name change cutoff.</returns>
        [NotMapped]
        public bool CanChangeTeamName
        {
            get
            {
                return DateTime.UtcNow.CompareTo(TeamNameChangeEnd) < 0;
            }
        }

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

        /// <summary>
        /// The window of time where if a team enters a certain number of
        /// incorrect answers would cause the team to be locked out of
        /// submitting additional answers for a brief amount of time.
        /// </summary>
        public double LockoutIncorrectGuessPeriod { get; set; }

        /// <summary>
        /// The amount of incorrect submissions required to initiate a lockout.
        /// </summary>
        public int LockoutIncorrectGuessLimit { get; set; }

        /// <summary>
        /// The multiplier for the lockout duration for consecutive lockouts.
        /// </summary>
        public double LockoutDurationMultiplier { get; set; }

        /// <summary>
        /// The maximum number of incorrect submissions a team can have for
        /// a single puzzle before being locked out entirely and being placed
        /// in email-only mode.
        /// </summary>
        public uint MaxSubmissionCount { get; set; }

        /// <summary>
        /// True if the hint system will not be used
        /// </summary>
        public bool HideHints { get; set; }

        /// <summary>
        /// Announcement to be added to every page
        /// </summary>
        public string Announcement { get; set; }

        /// <summary>
        /// content for the home page
        /// </summary>
        public string HomeContent { get; set; }

        /// <summary>
        /// content for the home page
        /// </summary>
        public string FAQContent { get; set; }

        /// <summary>
        /// content for the home page
        /// </summary>
        public string RulesContent { get; set; }
    }
}
