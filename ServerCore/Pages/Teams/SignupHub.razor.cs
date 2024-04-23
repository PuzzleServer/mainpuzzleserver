using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
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

        public SignupStrategy Strategy { get; set; } = SignupStrategy.None;

        protected override void OnParametersSet()
        {
            base.OnParametersSet();
        }
    }
}
