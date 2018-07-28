using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ServerCore.DataModel
{
    public class PuzzleStatePerTeam
    {
        public Puzzle Puzzle { get; set; }
        public Team Team { get; set; }
        public State State { get; set; }
        public DateTime StateChanged { get; set; }
    }
}
