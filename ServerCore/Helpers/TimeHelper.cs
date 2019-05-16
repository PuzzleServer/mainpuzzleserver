using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ServerCore.DataModel;

namespace ServerCore.Helpers
{
    public static class TimeHelper
    {
        /// <summary>
        /// returns a time with the formatting required for the jQuery code in site.js to convert it to browser local time
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public static string LocalTime(DateTime? date)
        {
            return date == null ? "&nbsp;" : $"<time>{date.Value.ToString("yyyy-MM-ddTHH:mm:ssZ")}</time>";
        }
    }
}
