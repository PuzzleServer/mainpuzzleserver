using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
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

        public static string GetFormattedUrl(Puzzle puzzle, int eventId, int? userId, string teamPassword = null, string playerClass = null)
        {
            return GetFormattedUrl(puzzle.CustomURL, puzzle.ID, eventId, userId, teamPassword, playerClass);
        }

        public static string GetFormattedSolutionUrl(Puzzle puzzle, int eventId, int? userId)
        {
            return GetFormattedUrl(puzzle.CustomSolutionURL, puzzle.ID, eventId, userId, null, null);
        }

        public static string GetFormattedUrl(string customUrl, int puzzleId, int eventId, int? userId, string teamPassword, string playerClass)
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

        /// <summary>
        /// De-obfuscate a file link, since the redirect approach used by FilesController doesn't mesh well with website copiers.
        /// </summary>
        /// <param name="context">The PuzzleServerContext</param>
        /// <param name="thisEvent">The current event</param>
        /// <param name="url">The URL to unobfuscate</param>
        /// <returns></returns>
        public static async Task<String> UnobfuscatePuzzleUrl(PuzzleServerContext context, Event thisEvent, string url)
        {
            if (!thisEvent.CanArchive || string.IsNullOrWhiteSpace(url))
            {
                return url;
            }

            Uri uri = new Uri(url, UriKind.RelativeOrAbsolute);
            string path = uri.AbsolutePath;
            string possibleContentFilename = null;
            string filenamePrefix;
            int prefixIndex;

            // see if the path contains an Event.ID-based route, and if so get the filename
            filenamePrefix = $"/{thisEvent.ID}/Files/";
            prefixIndex = path.IndexOf(filenamePrefix);
            if (prefixIndex >= 0)
            {
                possibleContentFilename = path.Substring(prefixIndex + filenamePrefix.Length);
            }

            // see if the path contains an Event.UrlString-based route, and if so get the filename
            filenamePrefix = $"/{thisEvent.UrlString}/Files/";
            prefixIndex = path.IndexOf(filenamePrefix);
            if (prefixIndex >= 0)
            {
                possibleContentFilename = path.Substring(prefixIndex + filenamePrefix.Length);
            }

            if (possibleContentFilename != null)
            {
                ContentFile content = await (from contentFile in context.ContentFiles
                                             where contentFile.Event == thisEvent &&
                                             contentFile.ShortName == possibleContentFilename
                                             select contentFile).SingleOrDefaultAsync();
                if (content != null)
                {
                    return String.IsNullOrEmpty(uri.Query) ? content.Url.OriginalString : $"{content.Url.OriginalString}?{uri.Query}";
                }
            }

            return url;
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
