using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ServerCore.DataModel;
using ServerCore.Helpers;
using ServerCore.ModelBases;

namespace ServerCore.Pages.Teams
{
    [Authorize(Policy = "PlayerIsOnTeam")]
    public class PlayProtoModel : EventSpecificPageModel
    {
        public PlayProtoModel(PuzzleServerContext serverContext, UserManager<IdentityUser> manager) : base(serverContext, manager)
        {
        }

        public IList<PuzzleView> PuzzleViews { get; set; }

        public int TeamID { get; set; }

        public string TeamPassword { get; set; }

        public async Task OnGetAsync(int teamId)
        {
            TeamID = teamId;
            Team myTeam = await UserEventHelper.GetTeamForPlayer(_context, Event, LoggedInUser);
            if (myTeam != null)
            {
                TeamID = myTeam.ID;
                TeamPassword = myTeam.Password;
                await PuzzleStateHelper.CheckForTimedUnlocksAsync(_context, Event, myTeam);
            }
            else
            {
                throw new Exception("Not currently registered for a team");
            }

            // all puzzles for this event that are real puzzles
            var puzzlesInEventQ = _context.Puzzles.Where(puzzle => puzzle.Event.ID == this.Event.ID && puzzle.IsPuzzle);

            // unless we're in a global lockout, then filter to those!
            var puzzlesCausingGlobalLockoutQ = PuzzleStateHelper.PuzzlesCausingGlobalLockout(_context, Event, myTeam);
            if (await puzzlesCausingGlobalLockoutQ.AnyAsync())
            {
                puzzlesInEventQ = puzzlesCausingGlobalLockoutQ;
            }

            // all puzzle states for this team that are unlocked (note: IsUnlocked bool is going to harm perf, just null check the time here)
            // Note that it's OK if some puzzles do not yet have a state record; those puzzles are clearly still locked and hence invisible.
            // All puzzles will show if all answers have been released)
            var stateForTeamQ = _context.PuzzleStatePerTeam.Where(state => state.TeamID == this.TeamID && (state.UnlockedTime != null));

            // join 'em (note: just getting all properties for max flexibility, can pick and choose columns for perf later)
            // Note: EF gotcha is that you have to join into anonymous types in order to not lose valuable stuff
            var visiblePuzzlesQ = from Puzzle puzzle in puzzlesInEventQ
                                  join PuzzleStatePerTeam pspt in stateForTeamQ on puzzle.ID equals pspt.PuzzleID
                                  select new PuzzleView { ID = puzzle.ID, Name = puzzle.Name, CustomUrl = puzzle.CustomURL };

            PuzzleViews = await visiblePuzzlesQ.ToListAsync();

            Dictionary<int, ContentFile> files = await (from file in _context.ContentFiles
                                                        where file.Event == Event && file.FileType == ContentFileType.Puzzle
                                                        select file).ToDictionaryAsync(file => file.PuzzleID);

            foreach (var puzzleView in PuzzleViews)
            {
                files.TryGetValue(puzzleView.ID, out ContentFile content);
                puzzleView.Content = content;
            }
        }

        public class PuzzleView
        {
            public int ID { get; set; }
            public string Name { get; set; }
            public string CustomUrl { get; set; }
            public ContentFile Content { get; set; }
        }
    }
}
