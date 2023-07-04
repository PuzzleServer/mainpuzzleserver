using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using ServerCore.DataModel;
using ServerCore.ModelBases;

namespace ServerCore.Pages.Room
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

        public void OnPost(string inputRooms, string group) 
        {
            List<DataModel.Room> roomsToAdd = new List<DataModel.Room>();
            string[] rooms = inputRooms.Split("\r\n");

            foreach(string room in rooms)
            {
                string[] splitRoom = room.Split(',');
                DataModel.Room parsedRoom = new DataModel.Room { EventID = Event.ID, Building = splitRoom[0], Number = splitRoom[1], Capacity = Int32.Parse(splitRoom[2]) };
                roomsToAdd.Add(parsedRoom);
            }

            int groupNum = 0;
            Int32.TryParse(group, out groupNum);
        }
    }
}
