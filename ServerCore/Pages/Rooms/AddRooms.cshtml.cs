using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ServerCore.DataModel;
using ServerCore.ModelBases;

namespace ServerCore.Pages.Rooms
{
    [Authorize(Policy = "IsEventAdmin")]
    public class AddRoomsModel : EventSpecificPageModel
    {
        public AddRoomsModel(PuzzleServerContext serverContext, UserManager<IdentityUser> manager) : base(serverContext, manager)
        {
        }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPost(string inputRooms, string group) 
        {
            string[] rooms = inputRooms.Split(Environment.NewLine);

            foreach (string room in rooms)
            {
                string[] splitRoom = room.Split(',');
                Room parsedRoom = new Room { EventID = Event.ID, Building = splitRoom[0], Number = splitRoom[1], Capacity = Int32.Parse(splitRoom[2]) };
                parsedRoom.Group = group;
                _context.Rooms.Add(parsedRoom);
            }

            await _context.SaveChangesAsync();

            return RedirectToPage("/Rooms/RoomList");
        }
    }
}
