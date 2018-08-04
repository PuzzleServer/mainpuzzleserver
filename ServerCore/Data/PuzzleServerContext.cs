using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ServerCore.DataModel;

namespace ServerCore.Models
{
    public class PuzzleServerContext : DbContext
    {
        public PuzzleServerContext (DbContextOptions<PuzzleServerContext> options)
            : base(options)
        {
        }

        public DbSet<ServerCore.DataModel.Puzzle> Puzzle { get; set; }
    }
}
