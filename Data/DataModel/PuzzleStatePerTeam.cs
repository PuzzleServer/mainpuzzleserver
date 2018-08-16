﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace ServerCore.DataModel
{
    public class PuzzleStatePerTeam
    {
        /// <summary>
        /// Row ID
        /// </summary>
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }

        public Puzzle Puzzle { get; set; }
        public Team Team { get; set; }
        public State State { get; set; }
        public DateTime StateChanged { get; set; }
    }
}
