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
        public DateTime StandingsAvailableBegin { get; set; }
        public bool AllowFeedback { get; set; }
        public EventTeams Teams { get; set; }
        public EventAuthors Authors { get; set; }
        public EventAdmins Admins { get; set; }
    }
}
