using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServerCore.DataModel;
using ServerCore.Helpers;
using ServerCore.ModelBases;

namespace ServerCore.Pages.Teams
{
    [Authorize("IsEventAdmin")]
    public class MergeIntoModel : EventSpecificPageModel
    {
        public MergeIntoModel(PuzzleServerContext serverContext, UserManager<IdentityUser> userManager) : base(serverContext, userManager)
        {
        }

        [BindProperty]
        public Team Team { get; set; }

        [BindProperty]
        public int MergeIntoID { get; set; }

        public List<Team> OtherTeams { get; set; }

        public async Task<IActionResult> OnGetAsync(int teamId)
        {
            Team = await _context.Teams.FirstOrDefaultAsync(t => t.ID == teamId);

            if (Team == null || Team.Event != Event)
            {
                return NotFound();
            }

            // TODO: Display team sizes
            OtherTeams = await _context.Teams.Where(t => t.EventID == Team.EventID && t.ID != teamId).OrderBy(t => t.Name).ToListAsync();

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int teamId)
        {
            Team = await _context.Teams.FindAsync(teamId);
            var mergeIntoTeam = await _context.Teams.FindAsync(MergeIntoID);

            if (Team == null || mergeIntoTeam == null || Team.Event != Event || mergeIntoTeam.Event != Event)
            {
                return NotFound();
            }

            List<string> memberEmails = null;
            List<string> mergeIntoMemberEmails = null;

            using (var transaction = await _context.Database.BeginTransactionAsync(IsolationLevel.Serializable))
            {
                var members = await _context.TeamMembers.Where(tm => tm.Team.ID == teamId).ToListAsync();
                memberEmails = await _context.TeamMembers.Where(tm => tm.Team.ID == teamId).Select(tm => tm.Member.Email).ToListAsync();
                mergeIntoMemberEmails = await _context.TeamMembers.Where(tm => tm.Team.ID == MergeIntoID).Select(m => m.Member.Email).ToListAsync();

                var states = await PuzzleStateHelper.GetSparseQuery(_context, Team.Event, null, Team).ToListAsync();
                var mergeIntoStates = await PuzzleStateHelper.GetSparseQuery(_context, Team.Event, null, mergeIntoTeam).ToDictionaryAsync(s => s.PuzzleID);

                // copy all the team members over
                foreach (var member in members)
                {
                    member.Team = mergeIntoTeam;
                }

                // also copy puzzle solves over
                foreach (var state in states)
                {
                    if (state.SolvedTime != null && mergeIntoStates.TryGetValue(state.PuzzleID, out var mergeIntoState) && mergeIntoState.SolvedTime == null)
                    {
                        await PuzzleStateHelper.SetSolveStateAsync(_context, Team.Event, mergeIntoState.Puzzle, mergeIntoTeam, state.UnlockedTime);
                    }
                }

                await TeamHelper.DeleteTeamAsync(_context, Team, sendEmail: false);

                await _context.SaveChangesAsync();
                transaction.Commit();
            }

            MailHelper.Singleton.SendPlaintextWithoutBcc(memberEmails.Union(mergeIntoMemberEmails),
                $"{Event.Name}: Team '{Team.Name}' has been merged into '{mergeIntoTeam.Name}'",
                $"These two teams have been merged into one superteam. Please welcome your new teammates!");

            return RedirectToPage("./Index");
        }
    }
}
