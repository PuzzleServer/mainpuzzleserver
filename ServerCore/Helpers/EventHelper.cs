using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ServerCore.DataModel;

namespace ServerCore.Helpers
{
    /// <summary>
    /// Helper for anything event-specific that's used beyond the EventSpecificPageModel
    /// </summary>
    public static class EventHelper
    {
        /// <summary>
        /// Returns the event that matches a given eventId
        /// </summary>
        /// <param name="eventId">The eventId from the URL - either the ID of an event of the UrlString of an event.</param>
        public static async Task<Event> GetEventFromEventId(PuzzleServerContext puzzleServerContext, string eventId)
        {
            Event result = null;

            // Decide whether to look up by friendly string or ID
            if (int.TryParse(eventId, out int eventIdAsInt))
            {
                result = await puzzleServerContext.Events.Where(e => e.ID == eventIdAsInt).FirstOrDefaultAsync();
            }
            else
            {
                // first, lookup by UrlString - this is the friendly name
                result = await puzzleServerContext.Events.Where(e => e.UrlString == eventId).FirstOrDefaultAsync();
            }

            return result;
        }
    }
}
