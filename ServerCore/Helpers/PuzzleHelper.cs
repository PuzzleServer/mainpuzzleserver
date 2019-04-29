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
            foreach (Prerequisites thisPrerequisite in context.Prerequisites.Where((r) => r.Puzzle == puzzle || r.Prerequisite == puzzle))
            {
                context.Prerequisites.Remove(thisPrerequisite);
            }

            context.Puzzles.Remove(puzzle);
            await context.SaveChangesAsync();
        }

        public static string GetFormattedUrl(Puzzle puzzle, int eventId)
        {
            if (puzzle.CustomURL == null)
            {
                return null;
            }
            string formattedUrl = puzzle.CustomURL.Replace("{puzzleId}", $"{puzzle.ID}").Replace("{eventId}", $"{eventId}");
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

            List<Puzzle> puzzles = await query.OrderBy(p => p.Group).ThenBy(p => p.OrderInGroup).ThenBy(p => p.Name).ToListAsync();

            // Can't just .OrderBy(p => p.IsFinalPuzzle) because only the last meta in the final group has
            // that flag set but the entire group should be ordered last. Instead, find the name of the group
            //  that contains the final puzzle and then move that entire group to the end. And just to be
            // safe, we collect all the final groups in case there's more than one.  Note that this does not
            // move the final puzzle itself within the group since the author can control that themselves.
            HashSet<string> finalGroups = (from p in puzzles
                                           where p.IsFinalPuzzle
                                           select p.Group).ToHashSet();
            puzzles = puzzles.Where(p => !finalGroups.Contains(p.Group))
                .Concat(puzzles.Where(p => finalGroups.Contains(p.Group)))
                .ToList();
            return puzzles;
        }
    }
}
