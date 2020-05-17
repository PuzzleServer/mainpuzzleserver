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

            ILookup<int, string> puzzleAuthors = (from author in _context.PuzzleAuthors
                                                  where author.Puzzle.Event == Event
                                                  select author).ToLookup(author => author.PuzzleID, author => author.Author.Name);
            Dictionary<int, ContentFile> puzzleFiles = await (from file in _context.ContentFiles
                                                              where file.Event == Event && file.FileType == ContentFileType.Puzzle
                                                              select file).ToDictionaryAsync(file => file.PuzzleID);
            Dictionary<int, ContentFile> puzzleAnswers = await (from file in _context.ContentFiles
                                                                where file.Event == Event && file.FileType == ContentFileType.Answer
                                                                select file).ToDictionaryAsync(file => file.PuzzleID);
            ILookup<int, string> puzzlePrereqs = (from prerequisite in _context.Prerequisites
                                                  where prerequisite.Puzzle.Event == Event
                                                  select prerequisite).ToLookup(prerequisite => prerequisite.PuzzleID, prerequisite => prerequisite.Prerequisite.Name);

            HashSet<int> puzzlesWithSolutions = (from response in _context.Responses
                                        where response.Puzzle.Event == Event && response.IsSolution
                                        select response.PuzzleID).ToHashSet();
            Dictionary<int, int> puzzleResponses = await (from response in _context.Responses
                                                          where response.Puzzle.Event == Event
                                                          group response by response.PuzzleID into responseList
                                                          select new { Puzzle = responseList.Key, ResponseCount = responseList.Count() }).ToDictionaryAsync(x => x.Puzzle, x => x.ResponseCount);
            var puzzleHints = await (from hint in _context.Hints
                                     where hint.Puzzle.Event == Event
                                     group hint by hint.Puzzle.ID into hints
                                     select new { Puzzle = hints.Key, Count = hints.Count(), TotalDiscount = hints.Min(hint => hint.Cost), TotalCost = hints.Sum(hint => Math.Abs(hint.Cost)) }).ToDictionaryAsync(x => x.Puzzle);

            PuzzleData = new List<PuzzleView>();
            foreach (Puzzle puzzle in puzzles)
            {
                IEnumerable<string> authors = puzzleAuthors[puzzle.ID];
                puzzleFiles.TryGetValue(puzzle.ID, out ContentFile puzzleFile);
                puzzleAnswers.TryGetValue(puzzle.ID, out ContentFile puzzleAnswer);
                IEnumerable<string> prereqs = puzzlePrereqs[puzzle.ID];
                puzzleResponses.TryGetValue(puzzle.ID, out int responses);
                puzzleHints.TryGetValue(puzzle.ID, out var hints);

                int totalHintCostThisPuzzle = 0;
                int hintsCountThisPuzzle = 0;
                
                if(hints != null)
                {
                    totalHintCostThisPuzzle = hints.TotalCost - hints.TotalDiscount;
                    hintsCountThisPuzzle = hints.Count;
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
