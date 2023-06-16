﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ServerCore.DataModel;
using ServerCore.ModelBases;

namespace ServerCore.Helpers
{
    public static class PuzzleHelper
    {
        /// <summary>
        /// Helper for deleting puzzles that correctly deletes dependent objects
        /// </summary>
        /// <param name="puzzle">Puzzle to delete</param>
        public static async Task DeletePuzzleAsync(PuzzleServerContext context, Puzzle puzzle)
        {
            // Delete all files associated with this puzzle
            foreach (ContentFile content in puzzle.Contents)
            {
                await FileManager.DeleteBlobAsync(content.Url);
            }

            // Delete all Prerequisites where this puzzle depends on or is depended upon by another puzzle
            foreach (Prerequisites thisPrerequisite in context.Prerequisites.Where((r) => r.Puzzle == puzzle || r.Prerequisite == puzzle))
            {
                context.Prerequisites.Remove(thisPrerequisite);
            }

            if (puzzle.IsForSinglePlayer)
            {
                var unlockStates = from unlockState in context.SinglePlayerPuzzleUnlockStates
                                  where unlockState.PuzzleID == puzzle.ID
                                  select unlockState;
                context.SinglePlayerPuzzleUnlockStates.RemoveRange(unlockStates);

                var puzzleStates = from puzzleState in context.SinglePlayerPuzzleStatePerPlayer
                                   where puzzleState.PuzzleID == puzzle.ID
                                   select puzzleState;
                context.SinglePlayerPuzzleStatePerPlayer.RemoveRange(puzzleStates);

                var hintStates = from hintState in context.SinglePlayerPuzzleHintStatePerPlayer
                                 where hintState.Hint.PuzzleID == puzzle.ID
                                 select hintState;
                context.SinglePlayerPuzzleHintStatePerPlayer.RemoveRange(hintStates);

                var submissions = from submission in context.SinglePlayerPuzzleSubmissions
                                  where submission.PuzzleID == puzzle.ID
                                  select submission;
                context.SinglePlayerPuzzleSubmissions.RemoveRange(submissions);
            }

            context.Puzzles.Remove(puzzle);
            await context.SaveChangesAsync();
        }

        public static string GetFormattedUrl(Puzzle puzzle, int eventId)
        {
            return GetFormattedUrl(puzzle.CustomURL, puzzle.ID, eventId, null);
        }

        public static string GetFormattedSolutionUrl(Puzzle puzzle, int eventId)
        {
            return GetFormattedUrl(puzzle.CustomSolutionURL, puzzle.ID, eventId, null);
        }

        public static string GetFormattedUrl(string customUrl, int puzzleId, int eventId, string teamPassword)
        {
            if (customUrl == null)
            {
                return null;
            }
            string formattedUrl = customUrl.Replace("{puzzleId}", $"{puzzleId}").Replace("{eventId}", $"{eventId}").Replace("{teamPass}", teamPassword);
            return formattedUrl;
        }

        public static async Task<List<Puzzle>> GetPuzzles(PuzzleServerContext context, Event Event, PuzzleUser user, EventRole role)
        {
            IQueryable<Puzzle> query;

            if (role == EventRole.admin)
            {
                query = context.Puzzles.Where(p => p.Event == Event);
            }
            else
            {
                query = UserEventHelper.GetPuzzlesForAuthorAndEvent(context, Event, user);
            }

            return await query.OrderBy(p => p.Group).ThenBy(p => p.OrderInGroup).ThenBy(p => p.Name).ToListAsync();
        }
    }
}
