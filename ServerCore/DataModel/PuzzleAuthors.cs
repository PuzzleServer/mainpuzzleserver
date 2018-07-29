using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ServerCore.DataModel
{
    public class PuzzleAuthors
    {
        // ID for row
        public int ID { get; set; }

        // Many to many table in db
        public Puzzle Puzzle { get; set; }
        public User Author { get; set; }
    }
}
