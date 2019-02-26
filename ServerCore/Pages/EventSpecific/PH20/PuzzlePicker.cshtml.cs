using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ServerCore.DataModel;
using ServerCore.ModelBases;

namespace ServerCore.Pages.EventSpecific.PH20
{
    public class PuzzlePickerModel : EventSpecificPageModel
    {
        public PuzzlePickerModel(PuzzleServerContext serverContext, UserManager<IdentityUser> manager) : base(serverContext, manager)
        {
        }

        public void OnGet()
        {

        }
    }
}