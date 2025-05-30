﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using ServerCore.DataModel;
using ServerCore.ModelBases;

namespace ServerCore.Pages.Events
{
    [Authorize(Policy = "IsEventAdmin")]
    public class PlayerClassModel : EventSpecificPageModel
    {
        public PlayerClassModel(PuzzleServerContext serverContext, UserManager<IdentityUser> manager)
            : base(serverContext, manager)
        {
        }


        public void OnGet()
        {
        }
    }
}
