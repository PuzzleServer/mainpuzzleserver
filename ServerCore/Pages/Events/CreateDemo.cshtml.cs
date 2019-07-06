using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using ServerCore.DataModel;

namespace ServerCore.Pages.Events
{
    [Authorize(Policy = "IsGlobalAdmin")]
    public class CreateDemoModel : PageModel
    {
        private readonly PuzzleServerContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        [BindProperty]
        public Event Event { get; set; }

        [BindProperty]
        public bool StartTheEvent { get; set; }

        [BindProperty]
        public bool AddCreatorToLoneWolfTeam { get; set; }

        public CreateDemoModel(PuzzleServerContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        private Piece MakePiece(Puzzle puzzle, int progressLevel, int clueID, string answerPattern, int puzzlePos, string clue)
        {
            var clue_info = new { clue_id = clueID, answer_pattern = answerPattern, puzzle_pos = puzzlePos, clue = clue };
            string contents = JsonConvert.SerializeObject(clue_info);
            return new Piece { Puzzle = puzzle, ProgressLevel = progressLevel, Contents = contents };
        }

        public IActionResult OnGet()
        {
            // Populate default fields
            Event = new Event();

            for (int i = 1; ; i++)
            {
                string name = $"Watership Demo {i}";
                if (_context.Events.Where(e => e.Name == name).FirstOrDefault() == null)
                {
                    Event.Name = name;
                    break;
                }
            }

            DateTime now = DateTime.UtcNow;
            Event.TeamRegistrationBegin = now;
            Event.StandingsAvailableBegin = now;
            Event.EventBegin = now;
            Event.AnswerSubmissionEnd = now.AddDays(1);
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            using (var transaction = _context.Database.BeginTransaction())
            {
                //
                // Add the event and save, so the event gets an ID.
                //
                Event.TeamRegistrationBegin = DateTime.UtcNow;
                Event.TeamRegistrationEnd = Event.AnswerSubmissionEnd;
                Event.TeamNameChangeEnd = Event.AnswerSubmissionEnd;
                Event.TeamMembershipChangeEnd = Event.AnswerSubmissionEnd;
                Event.TeamMiscDataChangeEnd = Event.AnswerSubmissionEnd;
                Event.TeamDeleteEnd = Event.AnswerSubmissionEnd;
                Event.AnswersAvailableBegin = Event.AnswerSubmissionEnd;
                Event.StandingsAvailableBegin = DateTime.UtcNow;
                Event.LockoutIncorrectGuessLimit = 5;
                Event.LockoutIncorrectGuessPeriod = 15;
                Event.LockoutDurationMultiplier = 2;
                Event.MaxSubmissionCount = 50;
                Event.MaxNumberOfTeams = 120;
                Event.MaxExternalsPerTeam = 9;
                Event.MaxTeamSize = 12;
                _context.Events.Add(Event);

                await _context.SaveChangesAsync();

                //
                // Add start puzzle, three module puzzles, and one module meta (marked as the final event puzzle for this demo)
                //
                Puzzle start = new Puzzle
                {
                    Name = "!!!Get Hopping!!!",
                    Event = Event,
                    IsPuzzle = false,
                    IsGloballyVisiblePrerequisite = true,
                    Description = "Start the event",
                };
                _context.Puzzles.Add(start);

                Puzzle easy = new Puzzle
                {
                    Name = "Bunny Slope",
                    Event = Event,
                    IsPuzzle = true,
                    SolveValue = 10,
                    HintCoinsForSolve = 1,
                    Group = "Thumper's Stumpers",
                    OrderInGroup = 1,
                    MinPrerequisiteCount = 1,
                    Description = "Bunsweeper",
                };
                _context.Puzzles.Add(easy);

                Puzzle intermediate = new Puzzle
                {
                    Name = "Rabbit Run (automatically solves in ~3 mins)",
                    Event = Event,
                    IsPuzzle = true,
                    SolveValue = 10,
                    HintCoinsForSolve = 2,
                    Group = "Thumper's Stumpers",
                    OrderInGroup = 2,
                    MinPrerequisiteCount = 1,
                    MinutesToAutomaticallySolve = 3,
                    Description = "Rabbit's Cube",
                };
                _context.Puzzles.Add(intermediate);

                Puzzle hard = new Puzzle
                {
                    Name = "Hare-Raising",
                    Event = Event,
                    IsPuzzle = true,
                    SolveValue = 10,
                    HintCoinsForSolve = 3,
                    Group = "Thumper's Stumpers",
                    OrderInGroup = 3,
                    MinPrerequisiteCount = 1,
                    Description = "Lateral Leaping",
                };
                _context.Puzzles.Add(hard);

                Puzzle meta = new Puzzle
                {
                    Name = "Lagomorph Meta",
                    Event = Event,
                    IsPuzzle = true,
                    IsMetaPuzzle = true,
                    IsFinalPuzzle = true,
                    SolveValue = 100,
                    Group = "Thumper's Stumpers",
                    OrderInGroup = 99,
                    MinPrerequisiteCount = 2,
                    Description = "Word Hutch",
                };
                _context.Puzzles.Add(meta);

                Puzzle other = new Puzzle
                {
                    Name = "Rabbit Season",
                    Event = Event,
                    IsPuzzle = true,
                    SolveValue = 10,
                    Group = "Daffy's Delights",
                    OrderInGroup = 1,
                    MinPrerequisiteCount = 1,
                    Description = "Hip Hop Identification",
                    CustomURL = "https://www.bing.com/images/search?q=%22rabbit%22",
                };
                _context.Puzzles.Add(other);

                Puzzle cheat = new Puzzle
                {
                    Name = "You're Despicable (cheat code)",
                    Event = Event,
                    IsPuzzle = true,
                    IsCheatCode = true,
                    SolveValue = -1,
                    Group = "Daffy's Delights",
                    OrderInGroup = 2,
                    MinPrerequisiteCount = 1,
                    Description = "Duck Konundrum",
                };
                _context.Puzzles.Add(cheat);

                Puzzle lockIntro = new Puzzle
                {
                    Name = "Wouldn't you know... (whistle stop intro)",
                    Event = Event,
                    IsPuzzle = true,
                    SolveValue = 0,
                    Group = "Roger's Railway",
                    OrderInGroup = 1,
                    MinPrerequisiteCount = 1,
                    Description = "Whistle Hop Intro",
                };
                _context.Puzzles.Add(lockIntro);

                Puzzle lockPuzzle = new Puzzle
                {
                    Name = "...Locked! (whistle stop, lasts 5 minutes)",
                    Event = Event,
                    IsPuzzle = true,
                    SolveValue = 0,
                    Group = "Roger's Railway",
                    OrderInGroup = 2,
                    MinPrerequisiteCount = 1,
                    MinutesOfEventLockout = 5,
                    Description = "Whistle Hop",
                };
                _context.Puzzles.Add(lockPuzzle);

                Puzzle kitchenSyncPuzzle = new Puzzle
                {
                    Name = "Kitchen Sync",
                    Event = Event,
                    IsPuzzle = true,
                    SolveValue = 10,
                    Group = "Sync Test",
                    OrderInGroup = 1,
                    MinPrerequisiteCount = 1
                };
                _context.Puzzles.Add(kitchenSyncPuzzle);

                Puzzle heatSyncPuzzle = new Puzzle
                {
                    Name = "Heat Sync",
                    Event = Event,
                    IsPuzzle = true,
                    SolveValue = 10,
                    Group = "Sync Test",
                    OrderInGroup = 2,
                    MinPrerequisiteCount = 1
                };
                _context.Puzzles.Add(heatSyncPuzzle);

                Puzzle lipSyncPuzzle = new Puzzle
                {
                    Name = "Lip Sync",
                    Event = Event,
                    IsPuzzle = true,
                    SolveValue = 10,
                    Group = "Sync Test",
                    OrderInGroup = 3,
                    MinPrerequisiteCount = 1
                };
                _context.Puzzles.Add(lipSyncPuzzle);

                Puzzle syncTestMetapuzzle = new Puzzle
                {
                    Name = "Sync Test",
                    Event = Event,
                    IsPuzzle = true,
                    IsMetaPuzzle = true,
                    SolveValue = 50,
                    Group = "Sync Test",
                    OrderInGroup = 99,
                    MinPrerequisiteCount = 1,
                    MaxAnnotationKey = 400
                };
                _context.Puzzles.Add(syncTestMetapuzzle);

                await _context.SaveChangesAsync();

                //
                // Add responses, PARTIAL is a partial, ANSWER is the answer.
                //
                _context.Responses.Add(new Response() { Puzzle = easy, SubmittedText = "PARTIAL", ResponseText = "Keep going..." });
                _context.Responses.Add(new Response() { Puzzle = easy, SubmittedText = "ANSWER", ResponseText = "Correct!", IsSolution = true });
                _context.Responses.Add(new Response() { Puzzle = intermediate, SubmittedText = "PARTIAL", ResponseText = "Keep going..." });
                _context.Responses.Add(new Response() { Puzzle = intermediate, SubmittedText = "ANSWER", ResponseText = "Correct!", IsSolution = true });
                _context.Responses.Add(new Response() { Puzzle = hard, SubmittedText = "PARTIAL", ResponseText = "Keep going..." });
                _context.Responses.Add(new Response() { Puzzle = hard, SubmittedText = "ANSWER", ResponseText = "Correct!", IsSolution = true });
                _context.Responses.Add(new Response() { Puzzle = meta, SubmittedText = "PARTIAL", ResponseText = "Keep going..." });
                _context.Responses.Add(new Response() { Puzzle = meta, SubmittedText = "ANSWER", ResponseText = "Correct!", IsSolution = true });
                _context.Responses.Add(new Response() { Puzzle = other, SubmittedText = "PARTIAL", ResponseText = "Keep going..." });
                _context.Responses.Add(new Response() { Puzzle = other, SubmittedText = "ANSWER", ResponseText = "Correct!", IsSolution = true });
                _context.Responses.Add(new Response() { Puzzle = cheat, SubmittedText = "PARTIAL", ResponseText = "Keep going..." });
                _context.Responses.Add(new Response() { Puzzle = cheat, SubmittedText = "ANSWER", ResponseText = "Correct!", IsSolution = true });
                _context.Responses.Add(new Response() { Puzzle = lockIntro, SubmittedText = "PARTIAL", ResponseText = "Keep going..." });
                _context.Responses.Add(new Response() { Puzzle = lockIntro, SubmittedText = "ANSWER", ResponseText = "Correct!", IsSolution = true });
                _context.Responses.Add(new Response() { Puzzle = lockPuzzle, SubmittedText = "PARTIAL", ResponseText = "Keep going..." });
                _context.Responses.Add(new Response() { Puzzle = lockPuzzle, SubmittedText = "ANSWER", ResponseText = "Correct!", IsSolution = true });
                _context.Responses.Add(new Response() { Puzzle = kitchenSyncPuzzle, SubmittedText = "PARTIAL", ResponseText = "Keep going..." });
                _context.Responses.Add(new Response() { Puzzle = kitchenSyncPuzzle, SubmittedText = "SYNC", ResponseText = "Correct!", IsSolution = true });
                _context.Responses.Add(new Response() { Puzzle = heatSyncPuzzle, SubmittedText = "PARTIAL", ResponseText = "Keep going..." });
                _context.Responses.Add(new Response() { Puzzle = heatSyncPuzzle, SubmittedText = "OR", ResponseText = "Correct!", IsSolution = true });
                _context.Responses.Add(new Response() { Puzzle = lipSyncPuzzle, SubmittedText = "PARTIAL", ResponseText = "Keep going..." });
                _context.Responses.Add(new Response() { Puzzle = lipSyncPuzzle, SubmittedText = "SWIM", ResponseText = "Correct!", IsSolution = true });
                _context.Responses.Add(new Response() { Puzzle = syncTestMetapuzzle, SubmittedText = "PARTIAL", ResponseText = "Keep going..." });
                _context.Responses.Add(new Response() { Puzzle = syncTestMetapuzzle, SubmittedText = "SYNCORSWIM", ResponseText = "Correct!", IsSolution = true });

                string hint1Description = "Tell me about the rabbits, George.";
                string hint1Content = "O.K. Some day – we’re gonna get the jack together and we’re gonna have a little house and a couple of acres an’ a cow and some pigs and...";
                string hint2Description = "Go on... George. How I get to tend the rabbits.";
                string hint2Content = "Well, we’ll have a big vegetable patch and a rabbit-hutch and chickens.";
                _context.Hints.Add(new Hint() { Puzzle = easy, Description = hint1Description, DisplayOrder = 0, Cost = 0, Content = hint1Content });
                _context.Hints.Add(new Hint() { Puzzle = easy, Description = hint2Description, DisplayOrder = 1, Cost = 1, Content = hint2Content });
                _context.Hints.Add(new Hint() { Puzzle = intermediate, Description = hint1Description, DisplayOrder = 0, Cost = 0, Content = hint1Content });
                _context.Hints.Add(new Hint() { Puzzle = intermediate, Description = hint2Description, DisplayOrder = 1, Cost = 1, Content = hint2Content });
                _context.Hints.Add(new Hint() { Puzzle = hard, Description = hint1Description, DisplayOrder = 0, Cost = 0, Content = hint1Content });
                _context.Hints.Add(new Hint() { Puzzle = hard, Description = hint2Description, DisplayOrder = 1, Cost = 1, Content = hint2Content });
                _context.Hints.Add(new Hint() { Puzzle = meta, Description = hint1Description, DisplayOrder = 0, Cost = 0, Content = hint1Content });
                _context.Hints.Add(new Hint() { Puzzle = meta, Description = hint2Description, DisplayOrder = 1, Cost = 1, Content = hint2Content });

                await _context.SaveChangesAsync();

                //
                // Set up prequisite links.
                // The first two depend on start puzzle, then the third depends on one of the first two, then the meta depends on two of the first three.
                //
                _context.Prerequisites.Add(new Prerequisites() { Puzzle = easy, Prerequisite = start });
                _context.Prerequisites.Add(new Prerequisites() { Puzzle = intermediate, Prerequisite = start });
                _context.Prerequisites.Add(new Prerequisites() { Puzzle = hard, Prerequisite = easy });
                _context.Prerequisites.Add(new Prerequisites() { Puzzle = hard, Prerequisite = intermediate });
                _context.Prerequisites.Add(new Prerequisites() { Puzzle = meta, Prerequisite = easy });
                _context.Prerequisites.Add(new Prerequisites() { Puzzle = meta, Prerequisite = intermediate });
                _context.Prerequisites.Add(new Prerequisites() { Puzzle = meta, Prerequisite = hard });
                _context.Prerequisites.Add(new Prerequisites() { Puzzle = other, Prerequisite = start });
                _context.Prerequisites.Add(new Prerequisites() { Puzzle = cheat, Prerequisite = start });
                _context.Prerequisites.Add(new Prerequisites() { Puzzle = lockIntro, Prerequisite = start });
                _context.Prerequisites.Add(new Prerequisites() { Puzzle = lockPuzzle, Prerequisite = lockIntro });
                _context.Prerequisites.Add(new Prerequisites() { Puzzle = kitchenSyncPuzzle, Prerequisite = start });
                _context.Prerequisites.Add(new Prerequisites() { Puzzle = heatSyncPuzzle, Prerequisite = start });
                _context.Prerequisites.Add(new Prerequisites() { Puzzle = lipSyncPuzzle, Prerequisite = start });
                _context.Prerequisites.Add(new Prerequisites() { Puzzle = syncTestMetapuzzle, Prerequisite = start });

                await _context.SaveChangesAsync();

                //
                // Create puzzle pieces.
                //
                _context.Pieces.Add(MakePiece(syncTestMetapuzzle, 0, 1, "xxx xxxx'x x xXxxx!", 3, "What Crocodile Dundee might say"));
                _context.Pieces.Add(MakePiece(syncTestMetapuzzle, 1, 2, "xxx xxxx xxxx xxx Xxxxx", 1, "In <i>Hey Diddle Diddle</i>, what the dish did"));
                _context.Pieces.Add(MakePiece(syncTestMetapuzzle, 1, 3, "xxXx", 1, "It smells"));
                _context.Pieces.Add(MakePiece(syncTestMetapuzzle, 2, 4, "xxX", 4, "You reach things by extending it"));
                _context.Pieces.Add(MakePiece(syncTestMetapuzzle, 2, 5, "Xxx xxxxx", 1, "Result of the <i>Exxon Valdez</i> crash"));
                _context.Pieces.Add(MakePiece(syncTestMetapuzzle, 2, 6, "xxxxX", 2, "What you make out of turkey drippings"));
                _context.Pieces.Add(MakePiece(syncTestMetapuzzle, 3, 7, "xxxXx xxx", 4, "Another name for a slow cooker"));
                _context.Pieces.Add(MakePiece(syncTestMetapuzzle, 4, 8, "xXxxxx", 3, "The index is one of them"));
                _context.Pieces.Add(MakePiece(syncTestMetapuzzle, 4, 9, "xxxxX", 2, "It can suffer when you play tennis"));
                _context.Pieces.Add(MakePiece(syncTestMetapuzzle, 4, 10, "xxxxxxX xxxxxxx", 2, "What Chekov asked strangers the location of in Star Trek IV, garnering suspicion due to his Russian accent"));

                await _context.SaveChangesAsync();

                //
                // Create teams. Can we add players to these?
                //
                Team team1 = new Team { Name = "Team Bugs", Event = Event, IsLookingForTeammates = true, PrimaryContactEmail = "bugs@bugs.bugs" };
                _context.Teams.Add(team1);

                Team team2 = new Team { Name = "Team Babs", Event = Event, IsLookingForTeammates = true, PrimaryContactEmail = "babs@babs.babs" };
                _context.Teams.Add(team2);

                Team team3 = new Team { Name = "Team Buster", Event = Event, IsLookingForTeammates = false, PrimaryContactEmail = "buster@buster.buster" };
                _context.Teams.Add(team3);

                Team teamLoneWolf = null;
                if (AddCreatorToLoneWolfTeam)
                {
                    teamLoneWolf = new Team { Name = "Lone Wolf", Event = Event, IsLookingForTeammates = true, PrimaryContactEmail = "wolf@wolf.wolf" };
                    _context.Teams.Add(teamLoneWolf);
                }

                var demoCreatorUser = await PuzzleUser.GetPuzzleUserForCurrentUser(_context, User, _userManager);
                if (demoCreatorUser != null)
                {
                    demoCreatorUser.MayBeAdminOrAuthor = true;

                    //
                    // Event admin/author
                    //
                    _context.EventAdmins.Add(new EventAdmins() { Event = Event, Admin = demoCreatorUser });
                    _context.EventAuthors.Add(new EventAuthors() { Event = Event, Author = demoCreatorUser });

                    //
                    // Puzzle author (for Thumper module only)
                    //
                    _context.PuzzleAuthors.Add(new PuzzleAuthors() { Puzzle = easy, Author = demoCreatorUser });
                    _context.PuzzleAuthors.Add(new PuzzleAuthors() { Puzzle = intermediate, Author = demoCreatorUser });
                    _context.PuzzleAuthors.Add(new PuzzleAuthors() { Puzzle = hard, Author = demoCreatorUser });
                    _context.PuzzleAuthors.Add(new PuzzleAuthors() { Puzzle = meta, Author = demoCreatorUser });
                }

                // TODO: Files (need to know how to detect whether local blob storage is configured)
                // Is there a point to adding Feedback or is that quick/easy enough to demo by hand?

                await _context.SaveChangesAsync();

                if (teamLoneWolf != null)
                {
                    _context.TeamMembers.Add(new TeamMembers() { Team = teamLoneWolf, Member = demoCreatorUser });
                }

                // line up all hints
                var teams = await _context.Teams.Where((t) => t.Event == Event).ToListAsync();
                var hints = await _context.Hints.Where((h) => h.Puzzle.Event == Event).ToListAsync();
                var puzzles = await _context.Puzzles.Where(p => p.Event == Event).ToListAsync();

                foreach (Team team in teams)
                {
                    foreach (Hint hint in hints)
                    {
                        _context.HintStatePerTeam.Add(new HintStatePerTeam() { Hint = hint, Team = team });
                    }
                    foreach (Puzzle puzzle in puzzles)
                    {
                        _context.PuzzleStatePerTeam.Add(new PuzzleStatePerTeam() { PuzzleID = puzzle.ID, TeamID = team.ID });
                    }
                }

                await _context.SaveChangesAsync();

                //
                // Mark the start puzzle as solved if we were asked to.
                //
                if (StartTheEvent)
                {
                    await PuzzleStateHelper.SetSolveStateAsync(_context, Event, start, null, DateTime.UtcNow);
                }

                transaction.Commit();
            }

            return RedirectToPage("./Index");
        }
    }
}
