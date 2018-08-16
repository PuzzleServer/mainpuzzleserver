using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ServerCore.DataModel
{
    public class TeamMembers
    {
        //Foreign Key Team table
        public int TeamID { get; set; }
        
        // Foreign Key User table
        public int UserID { get; set; }
    }
}
