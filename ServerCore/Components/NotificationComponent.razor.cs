using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace ServerCore.Components
{
    public partial class NotificationComponent : IDisposable
    {
        [Parameter]
        public int? EventID { get; set; }

        [Parameter]
        public int? TeamID { get; set; }

        [Parameter]
        public int? PlayerID { get; set; }

        [Inject]
        protected IJSRuntime JS { get; set; } = default!;

        [Inject]
        protected NotificationHelper NotificationHelper { get; set; } = default!;

        public NotificationComponent()
        {
        }

        protected override void OnInitialized()
        {
            NotificationHelper.RegisterForNotifications(EventID, TeamID, PlayerID, this.OnNotify);
            base.OnInitialized();
        }

        private async Task OnNotify(Notification notification)
        {
            await this.JS.InvokeVoidAsync("displayNotification", notification);
        }

        public void Dispose()
        {
            NotificationHelper.UnregisterFromNotifications(EventID, TeamID, PlayerID, this.OnNotify);
        }
    }
}
