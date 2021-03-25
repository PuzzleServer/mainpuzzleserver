using System;
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
            public int TotalHintCost { get; set; }
        }

        public class ResponseData
        {
            public int ResponseCount { get; set; }
            public bool HasAnswer { get; set; }
        }

        public List<PuzzleView> PuzzleData { get; set; }

        public int TotalHints { get; set; }

        public int TotalHintCost { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            List<Puzzle> puzzles = await PuzzleHelper.GetPuzzles(_context, Event, LoggedInUser, EventRole);

            ILookup<int, string> puzzleAuthors = (await (from author in _context.PuzzleAuthors
                                                  where author.Puzzle.Event == Event
                                                  select author).ToListAsync()).ToLookup(author => author.PuzzleID, author => author.Author.Name);
            Dictionary<int, ContentFile> puzzleFiles = await (from file in _context.ContentFiles
                                                              where file.Event == Event && file.FileType == ContentFileType.Puzzle
                                                              select file).ToDictionaryAsync(file => file.PuzzleID);
            Dictionary<int, ContentFile> puzzleAnswers = await (from file in _context.ContentFiles
                                                                where file.Event == Event && file.FileType == ContentFileType.Answer
                                                                select file).ToDictionaryAsync(file => file.PuzzleID);
            ILookup<int, string> puzzlePrereqs = (await (from prerequisite in _context.Prerequisites
                                                  where prerequisite.Puzzle.Event == Event
                                                  select prerequisite).ToListAsync()).ToLookup(prerequisite => prerequisite.PuzzleID, prerequisite => prerequisite.Prerequisite.Name);

            HashSet<int> puzzlesWithSolutions = (await (from response in _context.Responses
                                        where response.Puzzle.Event == Event && response.IsSolution
                                        select response.PuzzleID).ToListAsync()).ToHashSet();
            Dictionary<int, int> puzzleResponses = await (from response in _context.Responses
                                                          where response.Puzzle.Event == Event
                                                          group response by response.PuzzleID into responseList
                                                          select new { Puzzle = responseList.Key, ResponseCount = responseList.Count() }).ToDictionaryAsync(x => x.Puzzle, x => x.ResponseCount);

            ILookup<int, int> puzzleHints = (await (from hint in _context.Hints
                                      where hint.Puzzle.Event == Event
                                      select new { PuzzleId = hint.PuzzleID, Cost = hint.Cost }).ToListAsync()).ToLookup(hint => hint.PuzzleId, hint => hint.Cost);

            PuzzleData = new List<PuzzleView>();
            foreach (Puzzle puzzle in puzzles)
            {
                IEnumerable<string> authors = puzzleAuthors[puzzle.ID];
                puzzleFiles.TryGetValue(puzzle.ID, out ContentFile puzzleFile);
                puzzleAnswers.TryGetValue(puzzle.ID, out ContentFile puzzleAnswer);
                IEnumerable<string> prereqs = puzzlePrereqs[puzzle.ID];
                puzzleResponses.TryGetValue(puzzle.ID, out int responses);

                int totalHintCostThisPuzzle = 0;
                int hintsCountThisPuzzle = 0;
                
                if (puzzleHints.Contains(puzzle.ID))
                {
                    IEnumerable<int> hints = puzzleHints[puzzle.ID];
                    int totalDiscount = 0;

                    hintsCountThisPuzzle = hints.Count();

                    // positive hints cost what they say they cost.
                    // negative hints apply discounts to each other, find the most negative hint.
                    foreach (int cost in hints)
                    {
                        totalDiscount = Math.Min(totalDiscount, cost);
                        totalHintCostThisPuzzle += Math.Max(0, cost);
                    }

                    // add the total available discount to the cost.
                    totalHintCostThisPuzzle += Math.Abs(totalDiscount);
                }

                PuzzleData.Add(new PuzzleView()
                {
                    Puzzle = puzzle,
                    Authors = authors != null ? string.Join(", ", authors) : "",
                    PuzzleFile = puzzleFile,
                    AnswerFile = puzzleAnswer,
                    Prerequisites = prereqs != null ? string.Join(", ", prereqs.OrderBy(prereq => prereq)) : "",
                    PrerequisitesCount = prereqs != null ? prereqs.Count() : 0,
                    Responses = new ResponseData { ResponseCount = responses, HasAnswer = puzzlesWithSolutions.Contains(puzzle.ID) },
                    Hints = hintsCountThisPuzzle,
                    TotalHintCost = totalHintCostThisPuzzle
                });

                TotalHints += hintsCountThisPuzzle;
                TotalHintCost += totalHintCostThisPuzzle;
            }

            return Page();
        }
    }
}
