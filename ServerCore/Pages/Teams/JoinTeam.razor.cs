using Microsoft.AspNetCore.Components;
using ServerCore.DataModel;

namespace ServerCore.Pages.Teams
{
    public partial class JoinTeam
    {
        [Parameter]
        public Event Event { get; set; }
    }
}
