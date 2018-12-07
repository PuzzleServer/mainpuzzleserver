using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ServerCore.DataModel
{
    public class PuzzleServerContext : IdentityDbContext, IPuzzleServerContext
    {
        public PuzzleServerContext(DbContextOptions<PuzzleServerContext> options)
            : base(options)
        {
        }

        // These are the objects that EF uses to create/update tables
        // In general these won't be used directly
        public DbSet<ContentFile> ContentFiles { get; set; }
        public DbSet<Event> Events { get; set; }
        public DbSet<EventAdmins> EventAdmins { get; set; }
        public DbSet<EventAuthors> EventAuthors { get; set; }
        public DbSet<EventTeams> EventTeams { get; set; }
        public DbSet<Feedback> Feedback { get; set; }
        public DbSet<Invitation> Invitations { get; set; }
        public DbSet<Prerequisites> Prerequisites { get; set; }
        public DbSet<Puzzle> Puzzles { get; set; }
        public DbSet<PuzzleAuthors> PuzzleAuthors { get; set; }
        public DbSet<PuzzleStatePerTeam> PuzzleStatePerTeam { get; set; }
        public DbSet<Response> Responses { get; set; }
        public DbSet<Submission> Submissions { get; set; }
        public DbSet<Team> Teams { get; set; }
        public DbSet<TeamMembers> TeamMembers { get; set; }
        public DbSet<PuzzleUser> PuzzleUsers { get; set; }
        public DbSet<Hint> Hints { get; set; }
        public DbSet<HintStatePerTeam> HintStatePerTeam { get; set; }

        public static void UpdateDatabase(IApplicationBuilder app)
        {
            var appService = app.ApplicationServices
                .GetRequiredService<IServiceScopeFactory>();

            using (var serviceScope = appService.CreateScope())
            {
                using (var context = serviceScope.ServiceProvider.GetService<PuzzleServerContext>())
                {
                    context.Database.Migrate();
                }
            }
        }

        /// <summary>
        /// Customizations to database creation that cannot be done with attributes
        /// </summary>
        /// <param name="modelBuilder"></param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ContentFile>().HasIndex(contentFile => new { contentFile.EventID, contentFile.ShortName }).IsUnique();
            modelBuilder.Entity<PuzzleStatePerTeam>().HasKey(state => new { state.PuzzleID, state.TeamID });
            modelBuilder.Entity<HintStatePerTeam>().HasKey(state => new { state.TeamID, state.HintID });

            base.OnModelCreating(modelBuilder);
        }
    }
}
