using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace ServerCore.DataModel
{
    public class Event
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }
        public string Name { get; set; }

        [DataType(DataType.Url)]
        public string UrlString { get; set; }

        [NotMapped]
        public Uri URL
        {
            get { return new Uri(UrlString); }
            set { UrlString = value.ToString(); }
        }

        public int MaxNumberOfTeams { get; set; }
        public int MaxTeamSize { get; set; }
        public int MaxExternalsPerTeam { get; set; }
        public bool IsInternEvent { get; set; }
        public DateTime TeamRegistrationBegin { get; set; }
        public DateTime TeamRegistrationEnd { get; set; }
        public DateTime TeamNameChangeEnd { get; set; }
        public DateTime TeamMembershipChangeEnd { get; set; }
        public DateTime TeamMiscDataChangeEnd { get; set; }
        public DateTime TeamDeleteEnd { get; set; }
        public DateTime EventBegin { get; set; }
        public DateTime AnswerSubmissionEnd { get; set; }
        public DateTime AnswersAvailableBegin { get; set; }

        /// <summary>
        /// Automatically makes the standings page available at the DateTime below
        /// </summary>
        public DateTime StandingsAvailableBegin { get; set; }

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
        public EventTeams Teams { get; set; }
        public EventAuthors Authors { get; set; }
        public EventAdmins Admins { get; set; }
    }
}
