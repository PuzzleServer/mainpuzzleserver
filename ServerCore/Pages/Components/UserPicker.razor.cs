using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using ServerCore.DataModel;

namespace ServerCore.Pages.Components
{
    public abstract partial class UserPicker
    {
        List<PuzzleUser> AllUsers { get; set; } = new List<PuzzleUser>();
        List<PuzzleUser> SelectedUsers { get; set; } = new List<PuzzleUser>();
        string Filter { get; set; }

        [Parameter]
        public int EventId { get; set; }

        [Inject]
        public PuzzleServerContext _context { get; set; }

        protected abstract Task<List<PuzzleUser>> GetAllUsersAsync();

        protected abstract Task OnUserAddedAsync(int addedUserId);

        protected override async Task OnParametersSetAsync()
        {
            AllUsers = await GetAllUsersAsync();

            await base.OnParametersSetAsync();
        }

        private void OnFilterChanged(ChangeEventArgs e)
        {
            Filter = e.Value?.ToString();
            UpdateSelectedUsers();
        }

        private void UpdateSelectedUsers()
        {
            if (Filter?.Length < 3)
            {
                SelectedUsers = new List<PuzzleUser>();
                return;
            }

            SelectedUsers = (from user in AllUsers
                             where user.Name.Contains(Filter, StringComparison.OrdinalIgnoreCase) || user.Email.Contains(Filter, StringComparison.OrdinalIgnoreCase)
                             select user).ToList();
        }
        private async Task OnAddClick(int addedUserId)
        {
            await OnUserAddedAsync(addedUserId);
            AllUsers = await GetAllUsersAsync();
            UpdateSelectedUsers();
        }
    }
}
