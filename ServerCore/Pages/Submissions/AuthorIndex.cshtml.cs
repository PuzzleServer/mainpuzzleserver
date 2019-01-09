using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using ServerCore.DataModel;
using ServerCore.ModelBases;

namespace ServerCore.Pages.Submissions
{
    [Authorize(Policy = "IsEventAdminOrAuthorOfPuzzle")]
    public class AuthorIndexModel : EventSpecificPageModel
    {
        private readonly PuzzleServerContext dbContext;

        public AuthorIndexModel(PuzzleServerContext context)
        {
            dbContext = context;
        }

        public IList<Submission> Submissions { get; set; }
                        
        public async Task OnGetAsync(int? puzzleId)
        {
            if (puzzleId == null)
            {
                Submissions = await dbContext.Submissions.ToListAsync();
            }
            else
            {
                Submissions = await dbContext.Submissions.Where((s) => s.Puzzle != null && s.Puzzle.ID == puzzleId).ToListAsync();
            }
        }
    }
}