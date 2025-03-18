using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using ServerCore.DataModel;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using ServerCore.Helpers;
using ServerCore.ModelBases;

namespace ServerCore.Pages.Teams
{
    public partial class AddMemberPicker
    {
        [Parameter]
        public int TeamId { get; set; }

        List<PuzzleUser> CurrentMembers { get; set; } = new List<PuzzleUser>();

        protected override async Task OnParametersSetAsync()
        {
            await UpdateCurrentMembersAsync();

            await base.OnParametersSetAsync();
        }

        protected override async Task<List<PuzzleUser>> GetAllUsersAsync()
        {
            return await (from user in _context.PuzzleUsers
                          where !((from teamMember in _context.TeamMembers
                                   where teamMember.Team.EventID == EventId
                                   where teamMember.Member == user
                                   select teamMember).Any())
                          select user).ToListAsync();
        }

        private async Task UpdateCurrentMembersAsync()
        {
            CurrentMembers = await (from member in _context.TeamMembers
                                    where member.Team.ID == TeamId
                                    select member.Member).ToListAsync();
        }

        protected override async Task OnUserAddedAsync(int addedUserId)
        {
            Event ev = await (from evt in _context.Events
                              where evt.ID == EventId
                              select evt).SingleAsync();
            await TeamHelper.AddMemberAsync(_context, ev, EventRole.admin, TeamId, addedUserId);
            await UpdateCurrentMembersAsync();
        }
    }
}
