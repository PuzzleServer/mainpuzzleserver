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
        public DbSet<Feedback> Feedback { get; set; }
        public DbSet<Invitation> Invitations { get; set; }
        public DbSet<Prerequisites> Prerequisites { get; set; }
        public DbSet<Puzzle> Puzzles { get; set; }
        public DbSet<PuzzleAuthors> PuzzleAuthors { get; set; }
        public DbSet<PuzzleStatePerTeam> PuzzleStatePerTeam { get; set; }
        public DbSet<Response> Responses { get; set; }
        public DbSet<Submission> Submissions { get; set; }
        public DbSet<Team> Teams { get; set; }
        public DbSet<TeamApplication> TeamApplications { get; set; }
        public DbSet<TeamMembers> TeamMembers { get; set; }
        public DbSet<PuzzleUser> PuzzleUsers { get; set; }
        public DbSet<Hint> Hints { get; set; }
        public DbSet<HintStatePerTeam> HintStatePerTeam { get; set; }
        public DbSet<Annotation> Annotations { get; set; }
        public DbSet<Piece> Pieces { get; set; }
        public DbSet<Swag> Swag { get; set; }

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
            modelBuilder.Entity<Event>().HasIndex(eventObj => new { eventObj.UrlString }).IsUnique();
            modelBuilder.Entity<Annotation>().HasKey(state => new { state.PuzzleID, state.TeamID, state.Key });
            modelBuilder.Entity<Piece>().HasIndex(piece => new { piece.ProgressLevel });
            modelBuilder.Entity<Submission>().HasIndex(submission => new { submission.TeamID, submission.PuzzleID, submission.SubmissionText }).IsUnique();

            // SQL doesn't allow multiple cacasding delete paths from one entity to another, so cut links that cause those
            modelBuilder.Entity<ContentFile>().HasOne(contentFile => contentFile.Event).WithMany().OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<PuzzleStatePerTeam>().HasOne(state => state.Team).WithMany().OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<HintStatePerTeam>().HasOne(state => state.Team).WithMany().OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Submission>().HasOne(submission => submission.Team).WithMany().OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Annotation>().HasOne(annotation => annotation.Team).WithMany().OnDelete(DeleteBehavior.Restrict);

            base.OnModelCreating(modelBuilder);
        }
    }
}
