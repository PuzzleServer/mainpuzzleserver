using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ServerCore.DataModel
{
    public class Invitation
    {
        public int ID { get; set; }
        public Guid InvitationCode {get; set;}

        // TODO: What is this do we really need it?
        public string InvitationType { get; set; }
        public string EmailAddress { get; set; }
        public DateTime Expiration { get; set; }
    }
}
