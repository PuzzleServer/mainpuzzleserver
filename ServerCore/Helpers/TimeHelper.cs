using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ServerCore.DataModel;

namespace ServerCore.Helpers
{
    public static class TimeHelper
    {
        private static Dictionary<int, TimeZoneInfo> TimeZoneLookup = new Dictionary<int, TimeZoneInfo>();
        public static bool TryGetTimeZone(int userId, out TimeZoneInfo result)
        {
            return TimeZoneLookup.TryGetValue(userId, out result);
        }

        public static void SetTimeZone(int userId, TimeZoneInfo timeZone)
        {
            TimeZoneLookup[userId] = timeZone;
        }

        /// <summary>
        /// returns a time with the formatting required for the jQuery code in site.js to convert it to browser local time
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public static string LocalTime(DateTime? date, int userId)
        {
            if (date == null)
            {
                return "&nbsp;";
            }
            else if (TryGetTimeZone(userId, out TimeZoneInfo timeZone) && timeZone != null)
            {
                // TODO is there a way to get a format in the user's culture etc
                // Note: DO NOT use <time> here because then it would be adjusted on both server and client!
                return (date.Value + timeZone.BaseUtcOffset).ToString();
            }
            return $"<time>{date.Value.ToString("yyyy-MM-ddTHH:mm:ssZ")}</time>";
        }
    }
}
