using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ServerCore.DataModel
{
    public class PuzzleStatePerTeam
    {
        /// <summary>
        /// Row ID
        /// </summary>
        public int ID { get; set; }

        public Puzzle Puzzle { get; set; }
        public Team Team { get; set; }
        public State State { get; set; }
        public DateTime StateChanged { get; set; }

        /// <summary>
        /// Whether or not the team has checked the "Printed" checkbox for this puzzle
        /// </summary>
        public bool Printed { get; set;}

        /// <summary>
        /// Notes input by the team for this puzzle
        /// </summary>
        public string Notes { get; set; }
    }
}
