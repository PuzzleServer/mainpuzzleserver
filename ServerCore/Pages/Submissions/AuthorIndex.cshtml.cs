using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ServerCore.DataModel;
using ServerCore.ModelBases;

namespace ServerCore.Pages.Submissions
{
    [Authorize(Policy = "IsEventAdminOrAuthorOfPuzzle")]
    public class AuthorIndexModel : EventSpecificPageModel
    {
        public AuthorIndexModel(PuzzleServerContext serverContext, UserManager<IdentityUser> userManager) : base(serverContext, userManager)
        {
        }

        public IList<Submission> Submissions { get; set; }
                        
        public async Task OnGetAsync(int? puzzleId)
        {
            if (puzzleId == null)
            {
                Submissions = await _context.Submissions.ToListAsync();
            }
            else
            {
                Submissions = await _context.Submissions.Where((s) => s.Puzzle != null && s.Puzzle.ID == puzzleId).ToListAsync();
            }
        }
    }
}