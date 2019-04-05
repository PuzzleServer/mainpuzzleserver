using System.Linq;
using System.Threading.Tasks;
using ServerCore.DataModel;

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
    }
}
