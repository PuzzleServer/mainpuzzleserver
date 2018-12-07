using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using ServerCore.DataModel;
using ServerCore.ModelBases;

namespace ServerCore.Pages.Teams
{
    public class CreateModel : EventSpecificPageModel
    {
        private readonly PuzzleServerContext _context;

        public CreateModel(PuzzleServerContext context)
        {
            _context = context;
        }

        public IActionResult OnGet()
        {        
            if (!Event.IsTeamRegistrationActive)
            {
                return NotFound();
            }

            return Page();
        }

        [BindProperty]
        public Team Team { get; set; }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!Event.IsTeamRegistrationActive)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return Page();
            }

            Team.Event = Event;

            using (IDbContextTransaction transaction = _context.Database.BeginTransaction(System.Data.IsolationLevel.Serializable))
            {
                _context.Teams.Add(Team);

                var hints = from Hint hint in _context.Hints
                            where hint.Puzzle.Event == Event
                            select hint;

                foreach (Hint hint in hints)
                {
                    _context.HintStatePerTeam.Add(new HintStatePerTeam() { Hint = hint, Team = Team });
                }

                await _context.SaveChangesAsync();
                transaction.Commit();
            }

            return RedirectToPage("./Index");
        }
    }
}