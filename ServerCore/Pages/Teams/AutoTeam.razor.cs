using Microsoft.AspNetCore.Components;
using ServerCore.DataModel;

namespace ServerCore.Pages.Teams
{
    public partial class AutoTeam
    {
        [Parameter]
        public Event Event { get; set; }
    }
}
