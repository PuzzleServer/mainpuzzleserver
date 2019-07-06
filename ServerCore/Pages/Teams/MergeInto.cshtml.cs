using System.Collections.Generic;
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
            Team = await _context.Teams.FirstOrDefaultAsync(m => m.ID == teamId);

            if (Team == null)
            {
                return NotFound();
            }

            OtherTeams = await _context.Teams.Where(m => m.EventID == Team.EventID && m.ID != teamId).OrderBy(m => m.Name).ToListAsync();

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int teamId)
        {
            Team = await _context.Teams.FindAsync(teamId);
            var mergeIntoTeam = await _context.Teams.FindAsync(MergeIntoID);

            if (Team == null || mergeIntoTeam == null)
            {
                return NotFound();
            }

            var members = await _context.TeamMembers.Where(tm => tm.Team.ID == teamId).ToListAsync();
            var memberEmails = members.Select(m => m.Member.Email).ToList();
            var mergeIntoMemberEmails = await _context.TeamMembers.Where(tm => tm.Team.ID == MergeIntoID).Select(m => m.Member.Email).ToListAsync();

            var states = await PuzzleStateHelper.GetSparseQuery(_context, Team.Event, null, Team).ToListAsync();
            var mergeIntoStates = await PuzzleStateHelper.GetSparseQuery(_context, Team.Event, null, mergeIntoTeam).ToDictionaryAsync(s => s.PuzzleID);

            // copy all the team members over
            foreach (var member in members)
            {
                member.Team = mergeIntoTeam;
            }

            await _context.SaveChangesAsync();

            // also copy puzzle solves over
            foreach (var state in states)
            {
                if (state.SolvedTime != null && mergeIntoStates.TryGetValue(state.PuzzleID, out var mergeIntoState) && mergeIntoState.SolvedTime == null)
                {
                    await PuzzleStateHelper.SetSolveStateAsync(_context, Team.Event, mergeIntoState.Puzzle, mergeIntoTeam, state.UnlockedTime);
                }
            }

            await _context.SaveChangesAsync();

            await TeamHelper.DeleteTeamAsync(_context, Team);

            MailHelper.Singleton.SendPlaintextBcc(memberEmails.Union(mergeIntoMemberEmails),
                $"{Event.Name}: Team '{Team.Name}' has been merged into '{mergeIntoTeam.Name}'",
                $"These two teams have been merged into one superteam. Please welcome your new teammates!");

            return RedirectToPage("./Index");
        }
    }
}
