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

        public bool HasIndividualLunch { get; set; }
        public bool HasTShirts { get; set; }
        public bool AllowsRemote { get; set; }
        public bool IsRemote { get; set; }

        /// <summary>
        /// True if puzzles will be shown in an iframe on the answer submission page
        /// </summary>
        public bool EmbedPuzzles { get; set; }

        /// <summary>
        /// True if the event has individual swag
        /// </summary>
        [Column("EventHasSwag")]
        public bool HasSwag { get; set; }

        /// <summary>
        /// True if teams can sign up for collective swag
        /// </summary>
        public bool EventHasTeamSwag { get; set; }

        public DateTime TeamRegistrationBegin { get; set; }
        public DateTime TeamRegistrationEnd { get; set; }

        /// <summary>
        /// True if we should show puzzle help messages only to the authors of that puzzle (along with their support and admins).
        /// </summary>
        public bool ShouldShowHelpMessageOnlyToAuthor { get; set; }

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
        /// Time when lunches can no longer change (whether by users or automatically) so the reports produce a consistent result
        /// </summary>
        public DateTime LunchReportDate { get; set; }

        /// <summary>
        /// True if the current date is before the lunch report cutoff
        /// </summary>
        [NotMapped]
        public bool CanChangeLunch
        {
            get
            {
                return DateTime.UtcNow <= LunchReportDate;
            }
        }

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
        /// Announcement to be added to every page, once the team has their state configured to show it
        /// </summary>
        public string TeamAnnouncement { get; set; }

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

        /// <summary>
        /// Entity copyrighting this event
        /// </summary>
        public string Copyright { get; set; }

        /// <summary>
        /// Publicly-visible term for "group"
        /// </summary>
        public string TermForGroup { get; set; }

        /// <summary>
        /// How many players share a lunch order, with rounding up
        /// For example, if this is 3, then 1-3 player teams get 1 lunch order,
        /// 4-6 player teams get 2 lunch orders etc
        /// </summary>
        public int? PlayersPerLunch { get; set; }

        /// <summary>
        /// If no lunch is chosen, what to fill in with
        /// </summary>
        public string DefaultLunch { get; set; }

        /// <summary>
        /// List of lunch options to show on individual registration and/or team details page
        /// Each option is composed of a "Lunch Name":"Lunch Description"; pair, including the quotes
        /// Each option is delimited by a ; semicolon, so whitespace outside of quotes is ignored
        /// The Lunch Name is also used as the value that's written to the database
        /// A Lunch Name == "noneoftheabove" after removing spaces and converting to lowercase
        ///   will make the "Custom Order" textbox appear on the Swag Registration and Player Create/Edit pages
        /// Lunch Details are not displayed for team lunches
        /// </summary>
        public string LunchOptions { get; set; }

        /// <summary>
        /// Paragraph of text shown on the Team Details page to describe lunch
        /// </summary>
        public string LunchDescription { get; set; }

        /// <summary>
        /// Title for single player puzzles.
        /// Note: If this field is empty or null, the tab will not be shown.
        /// </summary>
        public string SinglePlayerPuzzleTitle { get; set; }

        /// <summary>
        /// True if the single player puzzles should be shown.
        /// </summary>
        [NotMapped]
        public bool ShouldShowSinglePlayerPuzzles => !string.IsNullOrEmpty(SinglePlayerPuzzleTitle);

        /// <summary>
        /// True if Blazor can run on pages for this event. Should normally be true, but can be disabled in an emergency
        /// </summary>
        public bool AllowBlazor { get; set; }

        /// <summary>
        /// Short-term hacks for modifying behavior without adding new properties all the time.
        /// Note: If a hack lasts more than a few months, it should probably be promoted to a real property.
        /// </summary>
        public string EphemeralHacks { get; set; }

        /// <summary>
        /// true if toast notifications should be turned off.
        /// </summary>
        [NotMapped]
        public bool EphemeralHackKillNotifications => EphemeralHacks?.Contains("kill-notifications") == true;

        /// <summary>
        /// true if player presence should be turned off everywhere.
        /// </summary>
        [NotMapped]
        public bool EphemeralHackKillPresence => EphemeralHacks?.Contains("kill-presence") == true;

        /// <summary>
        /// true if player presence should be turned off specifically in the Puzzles list.
        /// </summary>
        [NotMapped]
        public bool EphemeralHackKillListPresence => EphemeralHacks?.Contains("kill-list-presence") == true;

        /// <summary>
        /// true if the link to the live events page on the puzzle page should be hidden regardless of live event status
        /// </summary>
        [NotMapped]
        public bool EphemeralHackKillLiveEventPage => EphemeralHacks?.Contains("kill-live-event-page") == true;
    }
}
