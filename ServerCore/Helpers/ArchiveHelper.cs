using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ServerCore.DataModel;
using ServerCore.ModelBases;

namespace ServerCore.Helpers
{
    public static class ArchiveHelper
    {
        /// <summary>
        /// De-obfuscate a file link, since the redirect approach used by FilesController doesn't mesh well with website copiers.
        /// </summary>
        /// <param name="context">The PuzzleServerContext</param>
        /// <param name="thisEvent">he current event</param>
        /// <param name="url">The URL to unobfuscate</param>
        /// <returns></returns>
        public static async Task<String> UnobfuscateLink(PuzzleServerContext context, Event thisEvent, string url)
        {
            if (!thisEvent.CanArchive || string.IsNullOrWhiteSpace(url))
            {
                return url;
            }

            Uri uri = new Uri(url, UriKind.RelativeOrAbsolute);
            string path = uri.AbsolutePath;

            // TODO: validate that this is a file from our event somehow?
            int filesIndex = path.IndexOf("/Files/");
            if (filesIndex >= 0)
            {
                string filename = path.Substring(filesIndex + "/Files/".Length);
                ContentFile content = await (from contentFile in context.ContentFiles
                                                where contentFile.Event == thisEvent &&
                                                contentFile.ShortName == filename
                                                select contentFile).SingleOrDefaultAsync();
                if (content != null)
                {
                    return String.IsNullOrEmpty(uri.Query) ? content.Url.OriginalString : $"{content.Url.OriginalString}?{uri.Query}";
                }
            }

            return url;
        }
    }
}
