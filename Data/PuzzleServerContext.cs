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
        public DbSet<SinglePlayerPuzzleHintStatePerPlayer> SinglePlayerPuzzleHintStatePerPlayer { get; set; }
        public DbSet<SinglePlayerPuzzleStatePerPlayer> SinglePlayerPuzzleStatePerPlayer { get; set; }
        public DbSet<SinglePlayerPuzzleUnlockState> SinglePlayerPuzzleUnlockStates { get; set; }
        public DbSet<Response> Responses { get; set; }
        public DbSet<Submission> Submissions { get; set; }
        public DbSet<SinglePlayerPuzzleSubmission> SinglePlayerPuzzleSubmissions { get; set; }
        public DbSet<Team> Teams { get; set; }
        public DbSet<TeamApplication> TeamApplications { get; set; }
        public DbSet<TeamMembers> TeamMembers { get; set; }
        public DbSet<PuzzleUser> PuzzleUsers { get; set; }
        public DbSet<Hint> Hints { get; set; }
        public DbSet<HintStatePerTeam> HintStatePerTeam { get; set; }
        public DbSet<Annotation> Annotations { get; set; }
        public DbSet<Piece> Pieces { get; set; }
        public DbSet<PlayerInEvent> PlayerInEvent { get; set; }
        public DbSet<TeamLunch> TeamLunch { get; set; }
        public DbSet<Room> Rooms { get; set; }
        public DbSet<Message> Messages { get; set; }

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
            modelBuilder.Entity<SinglePlayerPuzzleHintStatePerPlayer>().HasKey(state => new { state.PlayerID, state.HintID });
            modelBuilder.Entity<Event>().HasIndex(eventObj => new { eventObj.UrlString }).IsUnique();
            modelBuilder.Entity<Event>().Property(eventObj => eventObj.AllowBlazor).HasDefaultValue(true);
            modelBuilder.Entity<Annotation>().HasKey(state => new { state.PuzzleID, state.TeamID, state.Key });
            modelBuilder.Entity<Piece>().HasIndex(piece => new { piece.ProgressLevel });
            modelBuilder.Entity<Submission>().HasIndex(submission => new { submission.TeamID, submission.PuzzleID, submission.SubmissionText }).IsUnique();
            modelBuilder.Entity<SinglePlayerPuzzleSubmission>().HasIndex(submission => new { submission.SubmitterID, submission.PuzzleID, submission.SubmissionText }).IsUnique();
            modelBuilder.Entity<PuzzleStatePerTeam>().HasIndex(pspt => new { pspt.TeamID });
            modelBuilder.Entity<PuzzleStatePerTeam>().HasIndex(pspt => new { pspt.TeamID, pspt.SolvedTime });
            modelBuilder.Entity<SinglePlayerPuzzleStatePerPlayer>().HasKey(state => new { state.PuzzleID, state.PlayerID });
            modelBuilder.Entity<SinglePlayerPuzzleStatePerPlayer>().HasIndex(pspt => new { pspt.PlayerID });
            modelBuilder.Entity<SinglePlayerPuzzleStatePerPlayer>().HasIndex(pspt => new { pspt.PlayerID, pspt.SolvedTime });
            modelBuilder.Entity<SinglePlayerPuzzleUnlockState>().HasKey(state => new { state.PuzzleID });
            modelBuilder.Entity<Room>().HasIndex(room => new { room.EventID, room.Building, room.Number }).IsUnique();
            modelBuilder.Entity<Message>().HasIndex(message => message.ThreadId);
            modelBuilder.Entity<Message>().HasIndex(message => message.EventID);
            modelBuilder.Entity<Message>().HasIndex(message => message.PuzzleID);
            modelBuilder.Entity<Message>().HasIndex(message => message.SenderID);
            modelBuilder.Entity<Message>().HasIndex(message => message.TeamID);

            // SQL doesn't allow multiple cacasding delete paths from one entity to another, so cut links that cause those
            // For more info, see https://learn.microsoft.com/en-us/ef/core/saving/cascade-delete
            modelBuilder.Entity<ContentFile>().HasOne(contentFile => contentFile.Event).WithMany().OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<PuzzleStatePerTeam>().HasOne(state => state.Team).WithMany().OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<SinglePlayerPuzzleStatePerPlayer>().HasOne(state => state.Player).WithMany().OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<SinglePlayerPuzzleUnlockState>().HasOne(state => state.Puzzle).WithMany().OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<HintStatePerTeam>().HasOne(state => state.Team).WithMany().OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<SinglePlayerPuzzleHintStatePerPlayer>().HasOne(state => state.Player).WithMany().OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Submission>().HasOne(submission => submission.Team).WithMany().OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<SinglePlayerPuzzleSubmission>().HasOne(submission => submission.Submitter).WithMany().OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Annotation>().HasOne(annotation => annotation.Team).WithMany().OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Message>().HasOne(message => message.Puzzle).WithMany().OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Message>().HasOne(message => message.Sender).WithMany().OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Message>().HasOne(message => message.Team).WithMany().OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Message>().HasOne(message => message.Claimer).WithMany().OnDelete(DeleteBehavior.SetNull);

            base.OnModelCreating(modelBuilder);
        }
    }
}
