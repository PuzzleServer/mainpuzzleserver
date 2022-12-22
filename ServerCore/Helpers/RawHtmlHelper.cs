using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ServerCore.Helpers
{
    public static class RawHtmlHelper
    {
        /// <summary>
        /// Helper for extracting raw HTML from a string if present.
        /// The syntax we look for is: {anything}Html.Raw({raw html})
        /// If we see that syntax, we return the {raw html} part only.
        /// If we see anything else, we return the entire string, processed by the standard ASP.NET IHtmlHelper.
        /// </summary>
        /// <param name="text">text which might contain raw HTML in</param>
        /// <returns>IHtmlContent containing either the raw html processed by Html.Raw or the entire string processed by Html.DisplayFor</returns>
        public static IHtmlContent Display<T>(string text, int eventId, IHtmlHelper<T> helper)
        {
            if (text != null && text.EndsWith(")") && text.Contains("Html.Raw("))
            {
                text = text.Replace("{eventId}", $"{eventId}");
                int start = text.IndexOf("Html.Raw(") + 9;
                return helper.Raw(text.Substring(start, text.Length - start - 1));
            }
            else
            {
                return helper.DisplayFor(m => text);
            }
        }
    }
}
