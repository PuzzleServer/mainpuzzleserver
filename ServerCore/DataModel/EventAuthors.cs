using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ServerCore.DataModel
{
    public class EventAuthors
    {
        // ID for row
        public int ID { get; set; }

        // Foreign Key Event table
        public int EventID { get; set; }

        // Foreign Key User table
        public int UserID { get; set; }
    }
}
