using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ServerCore.DataModel;
using ServerCore.Helpers;
using Microsoft.AspNetCore.Authorization;
using ServerCore.ModelBases;
using Microsoft.AspNetCore.Identity;

namespace ServerCore.Pages.Events
{
    [Authorize(Policy = "IsEventAdmin")]
    public class PlayerClassModel : EventSpecificPageModel
    {
        public PlayerClassModel(PuzzleServerContext serverContext, UserManager<IdentityUser>
    manager) : base(serverContext, manager)
    {
    }


    public void OnGet()
    {
    }
    }
    }
