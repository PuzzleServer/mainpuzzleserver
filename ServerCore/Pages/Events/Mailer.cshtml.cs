using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServerCore.DataModel;
using ServerCore.ModelBases;

namespace ServerCore.Pages.Events
{
    // Note: Mailer is restricted to admins because the number of messages we can send is a limited resource.
    // This prevents author abuse/ignorance.
    [Authorize(Policy = "IsEventAdmin")]
    public class MailerModel : EventSpecificPageModel
    {
        public enum MailGroup
        {
            Players,
            PrimaryContacts,
            NonSolvers,
            SmallTeams
        }

        [BindProperty]
        public int? TeamID { get; set; }

        [BindProperty]
        public int? PuzzleID { get; set; }

        public Team Team { get; set; }

        public Puzzle Puzzle { get; set; }

        [BindProperty]
        public MailGroup Group { get; set; }

        [BindProperty]
        [Required]
        public string MailSubject { get; set; }

        [BindProperty]
        [Required]
        public string MailBody { get; set; }

        public string MailAddresses { get; set; }

        public MailerModel(PuzzleServerContext serverContext, UserManager<IdentityUser> userManager) : base(serverContext, userManager)
        {
        }

        public async Task<IActionResult> OnGetAsync(MailGroup group, int? puzzleId, int? teamId, int? smallTeamThreshold)
        {
            Group = group;
            PuzzleID = puzzleId;
            TeamID = teamId;

            if (!IsPageUsageValid(group, puzzleId, teamId))
            {
                return NotFound("Incorrect page usage");
            }

            if (puzzleId != null)
            {
                Puzzle = await _context.Puzzles.FindAsync(puzzleId);
            }
            if (teamId != null)
            {
                Team = await _context.Teams.FindAsync(teamId);
            }

            IEnumerable<string> addresses = await BuildAddresses(smallTeamThreshold);

            MailAddresses = string.Join(';', addresses);

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int? smallTeamThreshold)
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            if (!IsPageUsageValid(Group, PuzzleID, TeamID))
            {
                return NotFound("Incorrect page usage");
            }
            IEnumerable<string> addresses = await BuildAddresses(smallTeamThreshold);

            MailHelper.Singleton.SendPlaintextBcc(addresses, MailSubject, MailBody);

            return RedirectToPage("./Players");
        }

        private async Task<IEnumerable<string>> BuildAddresses(int? smallTeamThreshold)
        {
            IEnumerable<string> addresses = Enumerable.Empty<string>();

            switch (Group)
            {
                case MailGroup.Players:
                    if (TeamID == null)
                    {
                        addresses = await _context.TeamMembers.Where(tm => tm.Team.Event == Event).Select(tm => tm.Member.Email).ToListAsync();
                    }
                    else
                    {
                        addresses = await _context.TeamMembers.Where(tm => tm.Team.ID == TeamID).Select(tm => tm.Member.Email).ToListAsync();
                    }
                    break;
                case MailGroup.NonSolvers:
                    if (PuzzleID != null)
                    {
                        addresses = await (from pspt in _context.PuzzleStatePerTeam
                                           join m in _context.TeamMembers on pspt.TeamID equals m.Team.ID
                                           where pspt.PuzzleID == PuzzleID && pspt.SolvedTime == null
                                           select m.Member.Email).ToListAsync();
                    }
                    break;
                case MailGroup.PrimaryContacts:
                    List<string> primaries;

                    if (TeamID == null)
                    {
                        primaries = await _context.Teams.Where(t => t.Event == Event).Select(t => t.PrimaryContactEmail).ToListAsync();
                    }
                    else
                    {
                        primaries = await _context.Teams.Where(t => t.ID == TeamID).Select(t => t.PrimaryContactEmail).ToListAsync();
                    }

                    List<string> primariesSplit = new List<string>();

                    foreach (string p in primaries)
                    {
                        if (p != null)
                        {
                            primariesSplit.AddRange(p.Split(',', ';'));
                        }
                    }

                    addresses = primariesSplit;
                    break;

                case MailGroup.SmallTeams:
                    List<string> smallTeamPlayers = new List<string>();

                    var players = await (from teamMember in _context.TeamMembers
                                       where teamMember.Team.EventID == Event.ID
                                       select new { Email = teamMember.Member.Email, TeamId = teamMember.Team.ID }).ToListAsync();

                    if (smallTeamThreshold == null)
                    {
                        smallTeamThreshold = 6;
                    }

                    var teams = (from player in players
                                group player by player.TeamId into team
                                where team.Count() <= smallTeamThreshold
                                select new { TeamId = team.Key, Members = team.ToList() }).ToDictionary(team => team.TeamId);

                    List<string> members = new List<string>();
                    foreach (var teamGroup in teams)
                    {
                        members.AddRange(teamGroup.Value.Members.Select(member => member.Email));
                    }

                    addresses = members;

                    break;
            }

            return addresses;
        }

        private bool IsPageUsageValid(MailGroup group, int? puzzleId, int? teamId)
        {
            if (group == MailGroup.NonSolvers)
            {
                if (puzzleId == null || teamId != null)
                {
                    return false;
                }
            }
            else
            {
                // null teamId is valid for the other mail groups; mails everyone in the event
                if (puzzleId != null)
                {
                    return false;
                }
            }

            return true;
        }
    }
}