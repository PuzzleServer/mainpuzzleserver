using System.Linq;
using Microsoft.AspNetCore.Mvc;
using ServerCore.DataModel;
using ServerCore.Models;

namespace ServerCore.Pages
{
    /// <summary>
    /// Controller to redirect users to files in blob storage
    /// </summary>
    public class FilesController : Controller
    {
        private readonly PuzzleServerContext _context;

        public FilesController(PuzzleServerContext context)
        {
            _context = context;
        }

        [Route("{eventId}/Files/{filename}")]
        public IActionResult Index(int eventId, string filename)
        {
            ContentFile content = (from contentFile in _context.ContentFiles
                                   where contentFile.EventID == eventId &&
                                   contentFile.ShortName == filename
                                   select contentFile).SingleOrDefault();
            if (content == null)
            {
                return NotFound();
            }

            // TODO: check whether the user is authorized to see this file.
            // This should be based on the FileType:
            // * Admins have access to all files in their event
            // * Authors have access to all files attached to puzzles they own
            // * Players can see puzzles and materials on puzzles they've unlocked
            // * Players can see answers after the event's AnswersAvailable time
            // * Players can see solve tokens on puzzles they've solved

            return Redirect(content.Url.ToString());
        }
    }
}