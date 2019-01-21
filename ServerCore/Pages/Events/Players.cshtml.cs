using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServerCore.DataModel;
using ServerCore.ModelBases;

namespace ServerCore.Pages.Events
{
    public class PlayersModel : EventSpecificPageModel
    {
        public IList<PuzzleUser> Admins { get; set; }

        public string AdminEmails { get; set; }

        public IList<Tuple<PuzzleUser, int>> Authors { get; set; }

        public string AuthorEmails { get; set; }

        public IList<TeamMembers> Players { get; set; }

        public string PlayerEmails { get; set; }

        public PlayersModel(PuzzleServerContext serverContext, UserManager<IdentityUser> userManager) : base(serverContext, userManager)
        {
        }

        public async Task<IActionResult> OnGetAsync()
        {
            // Get admins

            Admins = await _context.EventAdmins
                .Where(admin => admin.Event == Event).Select(admin => admin.Admin)
                .ToListAsync();

            StringBuilder adminEmailList = new StringBuilder("");
            foreach (PuzzleUser admin in Admins)
            {
                adminEmailList.Append(admin.Email + "; ");
            }
            AdminEmails = adminEmailList.ToString();

            // Get authors

            IList<PuzzleUser> allAuthors = await (from author in _context.EventAuthors
                                                  where author.Event == Event
                                                  select author.Author).ToListAsync();

            // note - if there's a faster way to count the puzzles / join all this data please let me know - Jenna
            IList<IGrouping<int, PuzzleAuthors>> allPuzzles = await (from puzzleAuthor in _context.PuzzleAuthors
                                                     where puzzleAuthor.Puzzle.Event == Event
                                                     group puzzleAuthor by puzzleAuthor.Author.ID into puzzleGroup
                                                     select puzzleGroup)
                                                     .ToListAsync();

            Dictionary<int, int> allPuzzlesDict = new Dictionary<int, int>();
            foreach (IGrouping<int, PuzzleAuthors> puzzles in allPuzzles)
            {
                allPuzzlesDict.Add(puzzles.Key, puzzles.Count());
            }

            StringBuilder authorEmailList = new StringBuilder("");
            Authors = new List<Tuple<PuzzleUser, int>>();
            foreach (PuzzleUser author in allAuthors)
            {
                authorEmailList.Append(author.Email + "; ");
                int puzzleCount = allPuzzlesDict.ContainsKey(author.ID) ? allPuzzlesDict[author.ID] : 0;
                Authors.Add(new Tuple<PuzzleUser, int>(author, puzzleCount));
            }
            AuthorEmails = authorEmailList.ToString();

            // Get players

            Players = await _context.TeamMembers
                .Where(member => member.Team.Event == Event)
                .ToListAsync();

            StringBuilder playerEmailList = new StringBuilder("");
            foreach (TeamMembers Player in Players)
            {
                playerEmailList.Append(Player.Member.Email + "; ");
            }
            PlayerEmails = playerEmailList.ToString();

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
    }
}