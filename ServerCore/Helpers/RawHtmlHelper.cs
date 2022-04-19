using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ServerCore.Helpers
{
    public static class RawHtmlHelper
    {
        /// <summary>
        /// Helper for extracting raw HTML from a string if present.
        /// The syntax we look for is: {anything}Html.Raw({raw html})
        /// </summary>
        /// <param name="html">html to search for raw HTML in</param>
        public static IHtmlContent GetRawHtmlIfPresent<T>(string html, IHtmlHelper<T> helper)
        {
            if(html != null && html.EndsWith(")") && html.Contains("Html.Raw("))
            { 
                int start = html.IndexOf("Html.Raw(") + 9;
                return helper.Raw(html.Substring(start, html.Length - start - 1));
            }
            else
            {
                return helper.DisplayFor(m => html);
            }
        }
    }
}
