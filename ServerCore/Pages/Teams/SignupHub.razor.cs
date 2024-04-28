using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using ServerCore.DataModel;
using ServerCore.ModelBases;

namespace ServerCore.Pages.Teams
{
    public enum SignupStrategy
    {
        None,
        Create,
        Join,
        Auto
    }

    public partial class SignupHub
    {
        [Parameter]
        public int EventId { get; set; }

        [Parameter]
        public EventRole EventRole { get; set; }

        [Parameter]
        public int LoggedInUserId { get; set; }

        [Inject]
        public PuzzleServerContext _context { get; set; }

        Event Event { get; set; }

        public SignupStrategy Strategy { get; set; } = SignupStrategy.None;

        protected override async Task OnParametersSetAsync()
        {
            Event = await _context.Events.Where(ev => ev.ID == EventId).SingleAsync();

            await base.OnParametersSetAsync();
        }
    }
}
