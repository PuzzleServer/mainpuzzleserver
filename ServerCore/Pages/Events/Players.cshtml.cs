using System;
using System.Collections.Generic;
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
    [Authorize(Policy = "IsEventAdmin")]
    public class PlayersModel : EventSpecificPageModel
    {
        public IList<PuzzleUser> Admins { get; set; }

        public string AdminEmails { get; set; }

        public IList<Tuple<PuzzleUser, int>> Authors { get; set; }

        public string AuthorEmails { get; set; }

        public IList<MemberView> Players { get; set; }

        public string PlayerEmails { get; set; }

        public PlayersModel(PuzzleServerContext serverContext, UserManager<IdentityUser> userManager) : base(serverContext, userManager)
        {
        }

        public async Task<IActionResult> OnGetAsync()
        {
            // Get admins

            Admins = await _context.EventAdmins
                .Where(admin => admin.Event == Event).Select(admin => admin.Admin)
                .OrderBy(admin => admin.Email)
                .ToListAsync();

            AdminEmails = String.Join("; ", Admins.Select(a => a.Email));

            // Get authors

            IList<PuzzleUser> allAuthors = await (from author in _context.EventAuthors
                                                  where author.Event == Event
                                                  orderby author.Author.Email
                                                  select author.Author).ToListAsync();

            Dictionary<int, int> allPuzzles = await (from puzzleAuthor in _context.PuzzleAuthors
                                                     where puzzleAuthor.Puzzle.Event == Event
                                                     group puzzleAuthor by puzzleAuthor.Author.ID into puzzleGroup
                                                     select new { authorId = puzzleGroup.Key, count = puzzleGroup.Count() })
                                                     .ToDictionaryAsync(x => x.authorId, x => x.count);

            Authors = new List<Tuple<PuzzleUser, int>>();
            foreach (PuzzleUser author in allAuthors)
            {
                int puzzleCount = allPuzzles.ContainsKey(author.ID) ? allPuzzles[author.ID] : 0;
                Authors.Add(new Tuple<PuzzleUser, int>(author, puzzleCount));
            }

            AuthorEmails = String.Join("; ", allAuthors.Select(a => a.Email));

            // Get players

            Players = await _context.TeamMembers
                .Where(member => member.Team.Event == Event)
                .OrderBy(member => member.Member.Email)
                .Select(tm => new MemberView() { ID = tm.Member.ID, Name = tm.Member.Name, Email = tm.Member.Email, EmployeeAlias = tm.Member.EmployeeAlias, TeamID = tm.Team.ID, TeamName = tm.Team.Name })
                .ToListAsync();

            PlayerEmails = String.Join("; ", Players.Select(p => p.Email));

            // Return page
            return Page();
        }

        public async Task<IActionResult> OnGetRemoveAdminAsync(int userId)
        {
            EventAdmins Admin = await _context.EventAdmins.FirstOrDefaultAsync(admin => admin.Admin.ID == userId && admin.Event == Event);
            if (Admin == null)
            {
                return NotFound("Could not find event admin with ID '" + userId + "'. They may have already been removed from the admin role.");
            }

            _context.EventAdmins.Remove(Admin);
            await _context.SaveChangesAsync();
            return RedirectToPage("/Events/Players");
        }

        public async Task<IActionResult> OnGetRemoveAuthorAsync(int userId)
        {
            EventAuthors Author = await _context.EventAuthors.FirstOrDefaultAsync(author => author.Author.ID == userId && author.Event == Event);
            if (Author == null)
            {
                return NotFound("Could not find event author with ID '" + userId + "'. They may have already been removed from the author role.");
            }

            _context.EventAuthors.Remove(Author);
            await _context.SaveChangesAsync();
            return RedirectToPage("/Events/Players");
        }

        public class MemberView
        {
            public int ID { get; set; }
            public string Name { get; set; }
            public string Email { get; set; }
            public string EmployeeAlias { get; set; }
            public int TeamID { get; set; }
            public string TeamName { get; set; }
        }
    }
}