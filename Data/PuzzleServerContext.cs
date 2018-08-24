using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ServerCore.DataModel;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;

namespace ServerCore.Models
{
    public class PuzzleServerContext : DbContext, IPuzzleServerContext
    {
        public PuzzleServerContext(DbContextOptions<PuzzleServerContext> options)
            : base(options)
        {
        }

        // These are the objects that EF uses to create/update tables
        // In general these won't be used directly
        public DbSet<Event> Event { get; set; }
        public DbSet<EventAdmins> EventAdmins { get; set; }
        public DbSet<EventAuthors> EventAuthors { get; set; }
        public DbSet<EventOwners> EventOwners { get; set; }
        public DbSet<EventTeams> EventTeams { get; set; }
        public DbSet<Feedback> Feedback { get; set; }
        public DbSet<Invitation> Invitations { get; set; }
        public DbSet<Puzzle> Puzzle { get; set; }
        public DbSet<PuzzleAuthors> PuzzleAuthors { get; set; }
        public DbSet<PuzzleStatePerTeam> PuzzleStatePerTeam { get; set; }
        public DbSet<Response> Responses { get; set; }
        public DbSet<State> States { get; set; }
        public DbSet<Submission> Submissions { get; set; }
        public DbSet<Team> Teams { get; set; }
        public DbSet<TeamMembers> TeamMembers { get; set; }
        public DbSet<User> Users { get; set; }

        public static void UpdateDatebase(IApplicationBuilder app)
        {
            using (var serviceScope = app.ApplicationServices
                .GetRequiredService<IServiceScopeFactory>()
                .CreateScope())
            {
                using (var context = serviceScope.ServiceProvider.GetService<PuzzleServerContext>())
                {
                    List<string> pendingMigrations = new List<string>(context.Database.GetPendingMigrations());
                    if (context.Database.GetPendingMigrations().Any())
                    {
                        try
                        {
                            context.Database.Migrate();
                        }
                        catch (SqlException ex)
                        {
                            // Run empty migration
                            Debug.WriteLine(ex.Message);
                        }
                    }
                }
            }
        }
    }
}
