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

        /// <summary>
        /// Helper for extracting plaintext from a string if present.
        /// The syntax we look for is: {anything}Html.Raw({raw html})
        /// If we see that syntax, we return the {anything} part only.
        /// If we see anything else, we return the entire string, processed by the standard ASP.NET IHtmlHelper.
        /// </summary>
        /// <param name="text">text which might contain raw HTML in</param>
        /// <returns>string containing either the plaintext part before the Html.Raw or the entire string</returns>
        public static string Plaintext(string text, int eventId)
        {
            if (text != null && text.EndsWith(")") && text.Contains("Html.Raw("))
            {
                text = text.Replace("{eventId}", $"{eventId}");
                return text.Substring(0, text.IndexOf("Html.Raw(")).TrimEnd();
            }
            else
            {
                return text;
            }
        }
    }
}
