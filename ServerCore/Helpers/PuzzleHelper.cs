using System.Collections.Generic;
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
            IEnumerable<Prerequisites> prerequisitesToRemove = context.Prerequisites.Where((r) => r.Puzzle == puzzle || r.Prerequisite == puzzle);
            context.Prerequisites.RemoveRange(prerequisitesToRemove);

            // Delete all puzzle messages related to this puzzle
            IEnumerable<Message> messagesToRemove = context.Messages.Where(m => m.Puzzle == puzzle);
            context.Messages.RemoveRange(messagesToRemove);

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

        public static string GetFormattedUrl(Puzzle puzzle, int eventId, int userId, string teamPassword = null, string playerClass = null)
        {
            return GetFormattedUrl(puzzle.CustomURL, puzzle.ID, eventId, userId, teamPassword, playerClass);
        }

        public static string GetFormattedSolutionUrl(Puzzle puzzle, int eventId, int userId)
        {
            return GetFormattedUrl(puzzle.CustomSolutionURL, puzzle.ID, eventId, userId, null, null);
        }

        public static string GetFormattedUrl(string customUrl, int puzzleId, int eventId, int userId, string teamPassword, string playerClass)
        {
            if (customUrl == null)
            {
                return null;
            }

            // Admins/authors/anyone who doesn't currently have a PlayerClass set needs a fallback url path
            // A file for this path needs to be included in any puzzles that use playerClass as part of the route
            if(playerClass == null)
            {
                playerClass = "noPlayerClass";
            }

            string formattedUrl = customUrl.Replace("{puzzleId}", $"{puzzleId}").Replace("{eventId}", $"{eventId}").Replace("{userId}", $"{userId}")
                .Replace("{teamPass}", teamPassword).Replace("{playerClass}", playerClass);
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
