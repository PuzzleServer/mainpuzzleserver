using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using ServerCore.DataModel;
using ServerCore.Helpers;
using ServerCore.ModelBases;
using ServerCore.ServerMessages;

namespace ServerCore.Pages.Puzzles
{
    [Authorize(Policy = "IsEventAdminOrAuthorOfPuzzle")]
    public class EditModel : EventSpecificPageModel
    {
        private IHubContext<ServerMessageHub> messageHub;

        public EditModel(PuzzleServerContext serverContext, UserManager<IdentityUser> userManager, IHubContext<ServerMessageHub> messageHub) : base(serverContext, userManager)
        {
            this.messageHub = messageHub;
        }

        [BindProperty]
        public Puzzle Puzzle { get; set; }

        [BindProperty]
        public int NewAuthorID { get; set; }

        [BindProperty]
        public int NewPrerequisiteID { get; set; }

        [BindProperty]
        public int NewPrerequisiteOfID { get; set; }

        public List<PuzzleUser> PotentialAuthors { get; set; }

        public List<PuzzleUser> CurrentAuthors { get; set; }

        public List<Puzzle> PotentialPrerequisites { get; set; }

        public List<Puzzle> CurrentPrerequisites { get; set; }

        public List<Puzzle> PotentialPrerequisitesOf { get; set; }

        public List<Puzzle> CurrentPrerequisitesOf { get; set; }

        public async Task<IActionResult> OnGetAsync(int puzzleId)
        {
            Puzzle = await _context.Puzzles.Where(m => m.ID == puzzleId).FirstOrDefaultAsync();

            if (Puzzle == null)
            {
                return NotFound();
            }
            await PopulateUIAsync();

            return Page();
        }

        private async Task PopulateUIAsync()
        {
            IQueryable<PuzzleUser> currentAuthorsQ = _context.PuzzleAuthors.Where(m => m.Puzzle == Puzzle).Select(m => m.Author);
            IQueryable<PuzzleUser> potentialAuthorsQ = _context.EventAuthors.Where(m => m.Event == Event).Select(m => m.Author).Except(currentAuthorsQ);

            CurrentAuthors = await currentAuthorsQ.OrderBy(p => p.Name).ToListAsync();
            PotentialAuthors = await potentialAuthorsQ.OrderBy(p => p.Name).ToListAsync();

            IQueryable<Puzzle> allVisiblePuzzles;
            IQueryable<Puzzle> allVisiblePuzzlesAndGlobalPrerequisites;

            if (EventRole == EventRole.admin)
            {
                allVisiblePuzzles = allVisiblePuzzlesAndGlobalPrerequisites = _context.Puzzles.Where(m => m.Event == Event && m != Puzzle);
            }
            else
            {
                IQueryable<Puzzle> authorPuzzles = UserEventHelper.GetPuzzlesForAuthorAndEvent(_context, Event, LoggedInUser);

                allVisiblePuzzles = authorPuzzles.Where(puzzle => puzzle != Puzzle);
                allVisiblePuzzlesAndGlobalPrerequisites = authorPuzzles.Union(_context.Puzzles.Where(m => m.Event == Event && m.IsGloballyVisiblePrerequisite)).Where(m=> m != Puzzle);
            }

            IQueryable<Puzzle> currentPrerequisitesQ = _context.Prerequisites.Where(m => m.Puzzle == Puzzle).Select(m => m.Prerequisite);
            IQueryable<Puzzle> potentialPrerequitesQ = allVisiblePuzzlesAndGlobalPrerequisites.Except(currentPrerequisitesQ);

            CurrentPrerequisites = await currentPrerequisitesQ.OrderBy(p => p.Name).ToListAsync();
            PotentialPrerequisites = await potentialPrerequitesQ.OrderBy(p => p.Name).ToListAsync();

            IQueryable<Puzzle> currentPrerequisitesOfQ = _context.Prerequisites.Where(m => m.Prerequisite == Puzzle).Select(m => m.Puzzle);
            IQueryable<Puzzle> potentialPrerequitesOfQ = allVisiblePuzzles.Except(currentPrerequisitesOfQ);

            CurrentPrerequisitesOf = await currentPrerequisitesOfQ.OrderBy(p => p.Name).ToListAsync();
            PotentialPrerequisitesOf = await potentialPrerequitesOfQ.OrderBy(p => p.Name).ToListAsync();
        }

        public async Task<IActionResult> OnPostAsync(int puzzleId)
        {
            ModelState.Remove("Puzzle.Event");
            if (!ModelState.IsValid)
            {
                await PopulateUIAsync();
                return Page();
            }

            Puzzle.ID = puzzleId; // to be safe
            Puzzle.EventID = Event.ID;

            OldPuzzleView oldPuzzleView = await (from Puzzle puzzle in _context.Puzzles
                                      where puzzle.ID == puzzleId
                                      select new OldPuzzleView
                                      {
                                          ID = puzzle.ID,
                                          IsForSinglePlayer = puzzle.IsForSinglePlayer,
                                          Errata = puzzle.Errata
                                      }).FirstOrDefaultAsync();

            _context.Attach(Puzzle).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();

                // Check if the errata was updated, and notify teams accordingly if so.
                // Changing from null to an empty string or vice-versa doesn't count.
                string oldErrata = oldPuzzleView.Errata;
                if (!(String.IsNullOrEmpty(Puzzle.Errata) && String.IsNullOrEmpty(oldErrata)) && Puzzle.Errata != oldErrata)
                {
                    // Only notify teams who have unlocked this puzzle
                    List<string> teamMembers = await (from TeamMembers tm in _context.TeamMembers
                                                      join PuzzleStatePerTeam pspt in _context.PuzzleStatePerTeam on tm.Team equals pspt.Team
                                                      where pspt.PuzzleID == Puzzle.ID && pspt.UnlockedTime != null
                                                      select tm.Member.Email).ToListAsync();
                    List<Team> teams = await (from PuzzleStatePerTeam pspt in _context.PuzzleStatePerTeam
                                                      where pspt.PuzzleID == Puzzle.ID && pspt.UnlockedTime != null
                                                      select pspt.Team).ToListAsync();

                    string subject, body;
                    string puzzleName = Puzzle.PlaintextName;

                    if (String.IsNullOrEmpty(Puzzle.Errata))
                    {
                        subject = $"{Event.Name}: Errata removed for {puzzleName}";
                        body = $"The errata for {puzzleName} has been removed.";
                    }
                    else
                    {
                        subject = $"{Event.Name}: Errata updated for {puzzleName}";
                        body = $"{puzzleName} has been updated with the following errata:\n\n{Puzzle.Errata}";
                    }

                    MailHelper.Singleton.SendPlaintextBcc(teamMembers, subject, body);
                    foreach (Team team in teams)
                    {
                        await this.messageHub.SendNotification(team, subject, body);
                    }
                }
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PuzzleExists(Puzzle.ID))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            if (oldPuzzleView.IsForSinglePlayer != Puzzle.IsForSinglePlayer)
            {
                if (Puzzle.IsForSinglePlayer) // If switch from team puzzle to single player puzzle
                {
                    // Remove old team puzzle states
                    IEnumerable<HintStatePerTeam> hintStatesToRemove = _context.HintStatePerTeam.Where(hintState => hintState.Hint.PuzzleID == oldPuzzleView.ID);
                    _context.HintStatePerTeam.RemoveRange(hintStatesToRemove);

                    IEnumerable<PuzzleStatePerTeam> puzzleStatesToRemove = _context.PuzzleStatePerTeam.Where(puzzleState => puzzleState.PuzzleID == oldPuzzleView.ID);
                    _context.PuzzleStatePerTeam.RemoveRange(puzzleStatesToRemove);

                    IEnumerable<Submission> submissionsToRemove = _context.Submissions.Where(submissions => submissions.PuzzleID == oldPuzzleView.ID);
                    _context.Submissions.RemoveRange(submissionsToRemove);

                    // Create new SinglePlayerPuzzle states
                    if (!_context.SinglePlayerPuzzleUnlockStates.Where(unlockState => unlockState.PuzzleID == oldPuzzleView.ID).Any())
                    {
                        _context.SinglePlayerPuzzleUnlockStates.Add(new SinglePlayerPuzzleUnlockState() { PuzzleID = oldPuzzleView.ID });
                    }
                }
                else // If switch from single player puzzle to team puzzle
                {
                    // Remove all old single player puzzle states.
                    IEnumerable<SinglePlayerPuzzleHintStatePerPlayer> hintStatesToRemove = _context.SinglePlayerPuzzleHintStatePerPlayer.Where(hintState => hintState.Hint.PuzzleID == oldPuzzleView.ID);
                    _context.SinglePlayerPuzzleHintStatePerPlayer.RemoveRange(hintStatesToRemove);

                    IEnumerable<SinglePlayerPuzzleStatePerPlayer> puzzleStatesToRemove = _context.SinglePlayerPuzzleStatePerPlayer.Where(puzzleState => puzzleState.PuzzleID == oldPuzzleView.ID);
                    _context.SinglePlayerPuzzleStatePerPlayer.RemoveRange(puzzleStatesToRemove);

                    IEnumerable<SinglePlayerPuzzleSubmission> submissionsToRemove = _context.SinglePlayerPuzzleSubmissions.Where(submissions => submissions.PuzzleID == oldPuzzleView.ID);
                    _context.SinglePlayerPuzzleSubmissions.RemoveRange(submissionsToRemove);

                    IEnumerable<SinglePlayerPuzzleUnlockState> unlockStatesToRemove = _context.SinglePlayerPuzzleUnlockStates.Where(unlockState => unlockState.PuzzleID == oldPuzzleView.ID);
                    _context.SinglePlayerPuzzleUnlockStates.RemoveRange(unlockStatesToRemove);

                    List<PuzzleStatePerTeam> missingPuzzleStates = await this.GetMissingPuzzleStatesPerTeam(_context, Puzzle.EventID, oldPuzzleView.ID);
                    foreach (PuzzleStatePerTeam missingPuzzleState in missingPuzzleStates)
                    {
                        _context.PuzzleStatePerTeam.Add(missingPuzzleState);
                    }
                }
            }
            await _context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }

        public async Task<IActionResult> OnPostAddAuthorAsync(int puzzleId)
        {
            if (!(await _context.EventAuthors.Where(m => m.Author.ID == NewAuthorID && m.Event == Event).AnyAsync()))
            {
                return NotFound();
            }

            if (!(await _context.PuzzleAuthors.Where(m => m.PuzzleID == puzzleId && m.AuthorID == NewAuthorID).AnyAsync()))
            {
                _context.PuzzleAuthors.Add(new PuzzleAuthors() { PuzzleID = puzzleId, AuthorID = NewAuthorID });
                await _context.SaveChangesAsync();
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostAddPrerequisiteAsync(int puzzleId)
        {
            if (!PuzzleExists(NewPrerequisiteID))
            {
                return NotFound();
            }

            if (!PrerequisiteExists(puzzleId, NewPrerequisiteID))
            {
                _context.Prerequisites.Add(new Prerequisites() { PuzzleID = puzzleId, PrerequisiteID = NewPrerequisiteID });
                await _context.SaveChangesAsync();
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostAddPrerequisiteOfAsync(int puzzleId)
        {
            if (!PuzzleExists(NewPrerequisiteOfID))
            {
                return NotFound();
            }

            if (!PrerequisiteExists(NewPrerequisiteOfID, puzzleId))
            {
                _context.Prerequisites.Add(new Prerequisites() { PuzzleID = NewPrerequisiteOfID, PrerequisiteID = puzzleId });
                await _context.SaveChangesAsync();
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnGetRemoveAuthorAsync(int puzzleId, int author)
        {
            PuzzleAuthors toRemove = await _context.PuzzleAuthors.Where(m => m.PuzzleID == puzzleId && m.AuthorID == author).FirstOrDefaultAsync();

            if (toRemove != null)
            {
                _context.PuzzleAuthors.Remove(toRemove);
                await _context.SaveChangesAsync();
            }

            // redirect without the prerequisite info to keep the URL clean
            return RedirectToPage(new { puzzleId });
        }

        public async Task<IActionResult> OnGetRemovePrerequisiteAsync(int puzzleId, int prerequisite)
        {
            Prerequisites toRemove = await _context.Prerequisites.Where(m => m.PuzzleID == puzzleId && m.PrerequisiteID == prerequisite).FirstOrDefaultAsync();

            if (toRemove != null)
            {
                _context.Prerequisites.Remove(toRemove);
                await _context.SaveChangesAsync();
            }

            // redirect without the prerequisite info to keep the URL clean
            return RedirectToPage(new { puzzleId });
        }

        public async Task<IActionResult> OnGetRemovePrerequisiteOfAsync(int puzzleId, int prerequisiteOf)
        {
            Prerequisites toRemove = await _context.Prerequisites.Where(m => m.PuzzleID == prerequisiteOf && m.PrerequisiteID == puzzleId).FirstOrDefaultAsync();

            if (toRemove != null)
            {
                _context.Prerequisites.Remove(toRemove);
                await _context.SaveChangesAsync();
            }

            // redirect without the prerequisite info to keep the URL clean
            return RedirectToPage(new { puzzleId });
        }

        private bool PuzzleExists(int puzzleId)
        {
            return _context.Puzzles.Any(e => e.ID == puzzleId);
        }

        private bool PrerequisiteExists(int puzzleId, int prerequisiteId)
        {
            return _context.Prerequisites.Any(pr => pr.Puzzle.ID == puzzleId && pr.Prerequisite.ID == prerequisiteId);
        }

        private async Task<List<PuzzleStatePerTeam>> GetMissingPuzzleStatesPerTeam(PuzzleServerContext context, int eventId, int puzzleId)
        {
            var newPspts = new List<PuzzleStatePerTeam>();

            var allPspts = await (from pspt in _context.PuzzleStatePerTeam
                                  where pspt.Puzzle.EventID == eventId
                                  select new { pspt.PuzzleID, pspt.TeamID }).ToDictionaryAsync(pspt => (((ulong)pspt.PuzzleID) << 32) | (uint)pspt.TeamID);

            List<int> allTeams = await (from team in context.Teams
                                        where team.EventID == eventId
                                        select team.ID).ToListAsync();

            foreach (int team in allTeams)
            {
                ulong key = (((ulong)puzzleId) << 32) | (uint)team;
                if (!allPspts.ContainsKey(key))
                {
                    PuzzleStatePerTeam newPspt = new PuzzleStatePerTeam()
                    {
                        PuzzleID = puzzleId,
                        TeamID = team,
                    };

                    newPspts.Add(newPspt);
                }
            }

            return newPspts;
        }

        private class OldPuzzleView
        {
            public int ID { get; init; }

            public bool IsForSinglePlayer { get; init; }

            public string Errata { get; init; }
        }
    }
}
