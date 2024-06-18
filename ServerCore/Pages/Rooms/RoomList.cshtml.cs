using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServerCore.DataModel;
using ServerCore.ModelBases;

namespace ServerCore.Pages.Rooms
{
    [Authorize(Policy = "IsEventAdmin")]
    public class RoomListModel : EventSpecificPageModel
    {
        public RoomListModel(PuzzleServerContext serverContext, UserManager<IdentityUser> manager) : base(serverContext, manager)
        {
        }

        public IList<Room> Rooms { get; set; }
        public IList<Tuple<string, bool>> Groups { get; set; }
        private Dictionary<string, bool> groupMapping = new Dictionary<string, bool>();
        public bool AssignRooms { get; set; }

        public async Task OnGetAsync()
        {
            await FillCollectionsAsync();
        }

        public async Task<IActionResult> OnPostAsync(List<string> GroupToToggle)
        {
            await FillCollectionsAsync();

            // Update the rooms that are online if that's changed

            foreach (Tuple<string, bool> gr in Groups)
            {
                if (GroupToToggle.Contains(gr.Item1) && !gr.Item2)
                {
                    groupMapping.Add(gr.Item1, true);
                }
                else if (!GroupToToggle.Contains(gr.Item1) && gr.Item2)
                {
                    // TODO: Add notification or something for rooms going offline that have been assigned to teams
                    groupMapping.Add(gr.Item1, false);
                }
            }

            foreach (Room r in Rooms)
            {
                if (groupMapping.ContainsKey(r.Group) && r.CurrentlyOnline != groupMapping[r.Group])
                {
                    r.CurrentlyOnline = groupMapping[r.Group];
                }
            }

            await _context.SaveChangesAsync();
            return RedirectToPage("/Rooms/RoomList");
        }

        public async Task<IActionResult> OnPostAssignRoomsAsync()
        {
            using (var transaction = _context.Database.BeginTransaction(IsolationLevel.Serializable))
            {
                // Get all of the teams that don't have rooms
                var teamsWithoutRooms = await (from team in _context.Teams
                                               where team.EventID == Event.ID
                                               join room in _context.Rooms on team.ID equals room.TeamID into gj
                                               from roomOrNull in gj.DefaultIfEmpty()
                                               where roomOrNull == null
                                               select team).ToListAsync();

                // Assign them rooms starting at the top of the list of active rooms and moving down until there are no more teams or no more rooms
                // Teams are pulled in no particular order, so probably the order they registered in

                List<Room> unassignedRooms = _context.Rooms.Where(r => r.TeamID == null && r.CurrentlyOnline).ToList();
                int roomsIndex = 0;

                foreach (var team in teamsWithoutRooms)
                {
                    if (roomsIndex < unassignedRooms.Count)
                    {
                        unassignedRooms[roomsIndex].TeamID = team.ID;
                        roomsIndex++;
                    }
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            return RedirectToPage("/Rooms/RoomList");
        }

        private async Task FillCollectionsAsync()
        {
            Rooms = await _context.Rooms
                    .Where(r => r.EventID == Event.ID)
                    .Include(r => r.Event)
                    .Include(r => r.Team).ToListAsync();
            var groupsTemp = await _context.Rooms.Where(r => r.EventID == Event.ID ).GroupBy(r => new Tuple<string, bool>(r.Group, r.CurrentlyOnline)).Select(r => r.First()).ToListAsync();
            Groups = groupsTemp.Select(g => new Tuple<string, bool>(g.Group, g.CurrentlyOnline)).ToList();
        }
    }
}
