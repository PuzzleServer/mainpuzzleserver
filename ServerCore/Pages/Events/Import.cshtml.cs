﻿using System;
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

namespace ServerCore.Pages.Events
{
    [Authorize(Policy = "IsEventAdmin")]
    public class ImportModel : EventSpecificPageModel
    {
        public ImportModel(PuzzleServerContext context, UserManager<IdentityUser> userManager, BackgroundFileUploader backgroundUploader) : base(context, userManager)
        {
            _backgroundUploader = backgroundUploader;
        }

        public IList<Event> Events { get; set; }

        [BindProperty]
        public int ImportEventID { get; set; }

        BackgroundFileUploader _backgroundUploader;

        public async Task<IActionResult> OnGetAsync(int id)
        {
            Events = await _context.EventAdmins.Where(ea => ea.Admin == LoggedInUser && ea.Event != Event).Select((ea) => ea.Event).ToListAsync();

            return Page();
        }

        public async Task<IActionResult> OnPostImportAsync()
        {
            // the BindProperty only binds the event ID, let's get the rest
            if (await _context.Events.Where((e) => e.ID == ImportEventID).FirstOrDefaultAsync() == null)
            {
                return NotFound();
            }

            // verify that we're an admin of the import event. current event administratorship is already validated.
            if (!await _context.EventAdmins.Where(ea => ea.Event.ID == ImportEventID && ea.Admin == LoggedInUser).AnyAsync())
            {
                return Forbid();
            }

            var sourceEventAdmins = await _context.EventAdmins.Where((a) => a.Event.ID == ImportEventID).ToListAsync();
            var sourceEventAuthors = await _context.EventAuthors.Where((a) => a.Event.ID == ImportEventID).ToListAsync();
            var sourcePuzzles = await _context.Puzzles.Where((p) => p.Event.ID == ImportEventID).ToListAsync();

            // TODO: replace this with a checkbox and sufficient danger warnings about duplicate titles
            bool deletePuzzleIfPresent = true;

            using (var transaction = _context.Database.BeginTransaction(System.Data.IsolationLevel.Serializable))
            {
                // Step 0: Make sure all admins exist
                var destEventAdminIDs = await (from ea in _context.EventAdmins where ea.EventID == Event.ID select ea.AdminID).ToListAsync();
                foreach (var sourceEventAdmin in sourceEventAdmins)
                {
                    if (!destEventAdminIDs.Contains(sourceEventAdmin.AdminID))
                    {
                        var destEventAdmin = new EventAdmins();
                        destEventAdmin.Admin = sourceEventAdmin.Admin;
                        destEventAdmin.Event = Event;
                        _context.EventAdmins.Add(destEventAdmin);
                    }
                }

                // Step 1: Make sure all authors exist
                var destEventAuthorIDs = await (from ea in _context.EventAuthors where ea.EventID == Event.ID select ea.AuthorID).ToListAsync();
                foreach (var sourceEventAuthor in sourceEventAuthors)
                {
                    if (!destEventAuthorIDs.Contains(sourceEventAuthor.AuthorID))
                    {
                        var destEventAuthor = new EventAuthors(sourceEventAuthor);
                        destEventAuthor.Event = Event;
                        _context.EventAuthors.Add(destEventAuthor);
                    }
                }

                // Step 2: Make sure all puzzles exist
                Dictionary<int, Puzzle> puzzleCloneMap = new Dictionary<int, Puzzle>();

                foreach (var sourcePuzzle in sourcePuzzles)
                {
                    // delete the puzzle if it exists
                    if (deletePuzzleIfPresent)
                    {
                        foreach (Puzzle p in _context.Puzzles.Where((p) => p.Event == Event && p.Name == sourcePuzzle.Name))
                        {
                            await PuzzleHelper.DeletePuzzleAsync(_context, p);
                        }
                    }

                    var destPuzzle = new Puzzle(sourcePuzzle);
                    destPuzzle.Event = Event;

                    puzzleCloneMap[sourcePuzzle.ID] = destPuzzle;
                    _context.Puzzles.Add(destPuzzle);
                }

                // Step 3: Save so that all our new objects have valid IDs
                await _context.SaveChangesAsync();

                Dictionary<ContentFile, Task<Uri>> allNewFiles = new Dictionary<ContentFile, Task<Uri>>();

                // Step 4: Ancillary tables referring to puzzles
                foreach (var sourcePuzzle in sourcePuzzles)
                {
                    // PuzzleAuthors
                    foreach (PuzzleAuthors sourcePuzzleAuthor in _context.PuzzleAuthors.Where((p) => p.Puzzle == sourcePuzzle))
                    {
                        var destPuzzleAuthor = new PuzzleAuthors(sourcePuzzleAuthor);
                        destPuzzleAuthor.Puzzle = puzzleCloneMap[sourcePuzzleAuthor.Puzzle.ID];
                        _context.PuzzleAuthors.Add(destPuzzleAuthor);
                    }

                    // Responses
                    foreach (Response sourceResponse in _context.Responses.Where((r) => r.Puzzle == sourcePuzzle))
                    {
                        var destResponse = new Response(sourceResponse);
                        destResponse.Puzzle = puzzleCloneMap[sourceResponse.Puzzle.ID];
                        _context.Responses.Add(destResponse);
                    }

                    // Prerequisites
                    foreach (Prerequisites sourcePrerequisite in _context.Prerequisites.Where((r) => r.Puzzle == sourcePuzzle))
                    {
                        var destPrerequisite = new Prerequisites(sourcePrerequisite);
                        destPrerequisite.Puzzle = puzzleCloneMap[sourcePrerequisite.Puzzle.ID];
                        destPrerequisite.Prerequisite = puzzleCloneMap[sourcePrerequisite.Prerequisite.ID];
                        _context.Prerequisites.Add(destPrerequisite);
                    }

                    // Hints
                    foreach (Hint sourceHint in _context.Hints.Where((h) => h.Puzzle == sourcePuzzle))
                    {
                        var destHint = new Hint(sourceHint);
                        destHint.Puzzle = puzzleCloneMap[sourceHint.Puzzle.ID];
                        _context.Hints.Add(destHint);

                        if (!sourcePuzzle.IsForSinglePlayer)
                        {
                            foreach (Team team in _context.Teams.Where(t => t.Event == Event))
                            {
                                _context.HintStatePerTeam.Add(new HintStatePerTeam() { Hint = destHint, TeamID = team.ID });
                            }
                        }
                    }

                    // PuzzleStatePerTeam
                    int newPuzzleId = puzzleCloneMap[sourcePuzzle.ID].ID;
                    if (sourcePuzzle.IsForSinglePlayer)
                    {
                        var destSinglePlayerPuzzleUnlockState = new SinglePlayerPuzzleUnlockState() { PuzzleID = newPuzzleId };
                        _context.SinglePlayerPuzzleUnlockStates.Add(destSinglePlayerPuzzleUnlockState);
                    }
                    else
                    {
                        foreach (Team team in _context.Teams.Where(t => t.Event == Event))
                        {
                            bool hasPuzzleStatePerTeam = await (from pspt in _context.PuzzleStatePerTeam
                                                                where pspt.PuzzleID == newPuzzleId &&
                                                                pspt.TeamID == team.ID
                                                                select pspt).AnyAsync();
                            if (!hasPuzzleStatePerTeam)
                            {
                                PuzzleStatePerTeam newPspt = new PuzzleStatePerTeam() { TeamID = team.ID, PuzzleID = newPuzzleId };
                                _context.PuzzleStatePerTeam.Add(newPspt);
                            }
                        }
                    }

                    // ContentFiles
                    foreach (ContentFile contentFile in _context.ContentFiles.Where((c) => c.Puzzle == sourcePuzzle))
                    {
                        ContentFile newFile = new ContentFile(contentFile);
                        newFile.EventID = Event.ID;
                        newFile.PuzzleID = puzzleCloneMap[contentFile.Puzzle.ID].ID;

                        // new
                        // does not copy the files; you have to use Azure Storage Explorer for that afterwards
                        newFile.UrlString = contentFile.UrlString.Replace("evt" + ImportEventID, "evt" + Event.ID);
                        _context.ContentFiles.Add(newFile);

                        // old
                        //_backgroundUploader.CloneInBackground(newFile, contentFile.ShortName, Event.ID, contentFile.Url);
                    }

                    // Pieces
                    foreach (Piece piece in _context.Pieces.Where((p) => p.Puzzle == sourcePuzzle))
                    {
                        Piece newPiece = new Piece(piece);
                        newPiece.Puzzle = puzzleCloneMap[piece.PuzzleID];
                        newPiece.PuzzleID = puzzleCloneMap[piece.PuzzleID].ID; // unsure why I need this line for pieces but not others <shrug/>
                        _context.Pieces.Add(newPiece);
                    }
                }

                // Step 5: Final save and commit
                await _context.SaveChangesAsync();
                transaction.Commit();
            }

            return RedirectToPage("./Details");
        }
    }
}
