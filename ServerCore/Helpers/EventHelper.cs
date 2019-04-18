using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
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
        private static ConcurrentDictionary<int, Event> eventCacheByInt = new ConcurrentDictionary<int, Event>();
        private static ConcurrentDictionary<string, Event> eventCacheByString = new ConcurrentDictionary<string, Event>();
        private static DateTime expiryTime = DateTime.MinValue;
        private static TimeSpan expiryWindow = TimeSpan.FromMinutes(1);

        /// <summary>
        /// Returns the event that matches a given eventId
        /// </summary>
        /// <param name="eventId">The eventId from the URL - either the ID of an event of the UrlString of an event.</param>
        public static async Task<Event> GetEventFromEventId(PuzzleServerContext puzzleServerContext, string eventId)
        {
            Event result = null;

            DateTime now = DateTime.UtcNow;
            if (now > expiryTime)
            {
                eventCacheByInt.Clear();
                eventCacheByString.Clear();
                expiryTime = now + expiryWindow;
            }
            else
            {
                if (eventCacheByString.TryGetValue(eventId, out result) || (Int32.TryParse(eventId, out int eventIdInt) && eventCacheByInt.TryGetValue(eventIdInt, out result)))
                {
                    return result;
                }
            }

            // first, lookup by UrlString - this is the friendly name
            result = await puzzleServerContext.Events.Where(e => e.UrlString == eventId).FirstOrDefaultAsync();

            if (result != null)
            {
                eventCacheByString[eventId] = result;
            }
            else if (Int32.TryParse(eventId, out int eventIdAsInt))
            {
                result = await puzzleServerContext.Events.Where(e => e.ID == eventIdAsInt).FirstOrDefaultAsync();

                if (result != null)
                {
                    eventCacheByInt[eventIdAsInt] = result;
                }
            }

            return result;
        }
    }
}
