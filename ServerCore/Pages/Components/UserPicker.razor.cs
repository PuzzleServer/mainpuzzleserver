using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using ServerCore.DataModel;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace ServerCore.Pages.Components
{
    public partial class UserPicker
    {
        List<PuzzleUser> AllUsers { get; set; } = new List<PuzzleUser>();
        List<PuzzleUser> SelectedUsers { get; set; } = new List<PuzzleUser>();

        [Parameter]
        public int EventId { get; set; }

        [Parameter]
        public bool AddAdmin { get; set; }

        [Inject]
        public PuzzleServerContext _context { get; set; }

        protected override async Task OnParametersSetAsync()
        {
            if (AddAdmin)
            {
                AllUsers = await (from user in _context.PuzzleUsers
                                       where !((from eventAdmin in _context.EventAdmins
                                                where eventAdmin.EventID == EventId
                                                where eventAdmin.Admin == user
                                                select eventAdmin).Any())
                                       select user).ToListAsync();
            }
            else
            {
                AllUsers = await (from user in _context.PuzzleUsers
                                       where !((from eventAuthor in _context.EventAuthors
                                                where eventAuthor.EventID == EventId
                                                where eventAuthor.Author == user
                                                select eventAuthor).Any())
                                       select user).ToListAsync();
            }

            await base.OnParametersSetAsync();
        }

        private void OnFilterChanged(ChangeEventArgs e)
        {
            string filter = e.Value?.ToString();
            if (filter?.Length < 3)
            {
                SelectedUsers = new List<PuzzleUser>();
                return;
            }

            SelectedUsers = (from user in AllUsers
                             where user.Name.Contains(filter) || user.Email.Contains(filter)
                             select user).ToList();
        }
    }
}
