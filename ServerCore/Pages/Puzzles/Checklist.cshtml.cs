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

namespace ServerCore.Pages.Puzzles
{
    [Authorize(Policy = "IsEventAdminOrEventAuthor")]
    public class PuzzleChecklistModel : EventSpecificPageModel
    {
        public PuzzleChecklistModel(PuzzleServerContext serverContext, UserManager<IdentityUser> userManager) : base(serverContext, userManager)
        {
        }

        public class PuzzleView
        {
            public Puzzle Puzzle { get; set; }
            public string Authors { get; set; }
            public ContentFile PuzzleFile { get; set; }
            public ContentFile AnswerFile { get; set; }
            public string Prerequisites { get; set; }
            public int PrerequisitesCount { get; set; }
            public ResponseData Responses { get; set; }
            public int Hints { get; set; }
        }

        public class ResponseData
        {
            public int ResponseCount { get; set; }
            public bool HasAnswer { get; set; }
        }

        public List<PuzzleView> PuzzleData { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            List<Puzzle> puzzles = await PuzzleHelper.GetPuzzles(_context, Event, LoggedInUser, EventRole);

            Dictionary<int, List<string>> puzzleAuthors = await (from author in _context.PuzzleAuthors
                                                                        where author.Puzzle.Event == Event
                                                                        group author by author.PuzzleID into authorList
                                                                        select new { Puzzle = authorList.Key, Authors = (from a in authorList select a.Author.Name).ToList() }).ToDictionaryAsync(x => x.Puzzle, x => x.Authors);
            Dictionary<int, ContentFile> puzzleFiles = await (from file in _context.ContentFiles
                                                              where file.Event == Event && file.FileType == ContentFileType.Puzzle
                                                              select file).ToDictionaryAsync(file => file.Puzzle.ID);
            Dictionary<int, ContentFile> puzzleAnswers = await (from file in _context.ContentFiles
                                                                where file.Event == Event && file.FileType == ContentFileType.Answer
                                                                select file).ToDictionaryAsync(file => file.Puzzle.ID);
            Dictionary<int, List<string>> puzzlePrereqs = await (from prerequisite in _context.Prerequisites
                                                                 where prerequisite.Puzzle.Event == Event
                                                                 group prerequisite by prerequisite.PuzzleID into prereqs
                                                                 select new { Puzzle = prereqs.Key, Prereqs = (from p in prereqs select p.Prerequisite.Name).ToList() }).ToDictionaryAsync(x => x.Puzzle, x => x.Prereqs);
            Dictionary<int, ResponseData> puzzleResponses = await (from response in _context.Responses
                                                                   where response.Puzzle.Event == Event
                                                                   group response by response.PuzzleID into responseList
                                                                   select new { Puzzle = responseList.Key, Responses = new ResponseData { ResponseCount = responseList.Count(), HasAnswer = (from r in responseList where r.IsSolution select r).Count() > 0 } }).ToDictionaryAsync(x => x.Puzzle, x => x.Responses);
            Dictionary<int, int> puzzleHints = await (from hint in _context.Hints
                                                      where hint.Puzzle.Event == Event
                                                      group hint by hint.Puzzle.ID into hints
                                                      select new { Puzzle = hints.Key, HintCount = hints.Count() }).ToDictionaryAsync(x => x.Puzzle, x => x.HintCount);

            PuzzleData = new List<PuzzleView>();
            foreach (Puzzle puzzle in puzzles)
            {
                puzzleAuthors.TryGetValue(puzzle.ID, out List<string> authors);
                puzzleFiles.TryGetValue(puzzle.ID, out ContentFile puzzleFile);
                puzzleAnswers.TryGetValue(puzzle.ID, out ContentFile puzzleAnswer);
                puzzlePrereqs.TryGetValue(puzzle.ID, out List<string> prereqs);
                puzzleResponses.TryGetValue(puzzle.ID, out ResponseData responses);
                puzzleHints.TryGetValue(puzzle.ID, out int hints);
                PuzzleData.Add(new PuzzleView()
                {
                    Puzzle = puzzle,
                    Authors = authors != null ? string.Join(", ", authors) : "",
                    PuzzleFile = puzzleFile,
                    AnswerFile = puzzleAnswer,
                    Prerequisites = prereqs != null ? string.Join(", ", prereqs) : "",
                    PrerequisitesCount = prereqs != null ? prereqs.Count() : 0,
                    Responses = responses != null ? responses : new ResponseData { ResponseCount = 0, HasAnswer = false },
                    Hints = hints
                });
            }

            return Page();
        }
    }
}
