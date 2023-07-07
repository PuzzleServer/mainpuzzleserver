using Microsoft.EntityFrameworkCore;

namespace ServerCore.DataModel
{
    public interface IPuzzleServerContext
    {
        DbSet<Event> Events { get; set; }
        DbSet<EventAdmins> EventAdmins { get; set; }
        DbSet<EventAuthors> EventAuthors { get; set; }
        DbSet<Feedback> Feedback { get; set; }
        DbSet<Invitation> Invitations { get; set; }
        DbSet<Prerequisites> Prerequisites { get; set; }
        DbSet<Puzzle> Puzzles { get; set; }
        DbSet<PuzzleAuthors> PuzzleAuthors { get; set; }
        DbSet<ContentFile> ContentFiles { get; set; }
        DbSet<PuzzleStatePerTeam> PuzzleStatePerTeam { get; set; }
        DbSet<SinglePlayerPuzzleHintStatePerPlayer> SinglePlayerPuzzleHintStatePerPlayer { get; set; }
        DbSet<SinglePlayerPuzzleStatePerPlayer> SinglePlayerPuzzleStatePerPlayer { get; set; }
        DbSet<SinglePlayerPuzzleUnlockState> SinglePlayerPuzzleUnlockStates { get; set; }
        DbSet<Response> Responses { get; set; }
        DbSet<Submission> Submissions { get; set; }
        DbSet<SinglePlayerPuzzleSubmission> SinglePlayerPuzzleSubmissions { get; set; }
        DbSet<TeamMembers> TeamMembers { get; set; }
        DbSet<Team> Teams { get; set; }
        DbSet<PuzzleUser> PuzzleUsers { get; set; }
        DbSet<PlayerInEvent> PlayerInEvent { get; set; }
        DbSet<TeamLunch> TeamLunch { get; set; }
        DbSet<Room> Room { get; set; }
    }
}