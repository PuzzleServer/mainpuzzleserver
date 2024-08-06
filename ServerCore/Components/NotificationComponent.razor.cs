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
        public int? UserID { get; set; }

        [Inject]
        protected IJSRuntime JS { get; set; } = default!;

        [Inject]
        protected NotificationHelper NotificationHelper { get; set; } = default!;

        public NotificationComponent()
        {
        }

        protected override void OnInitialized()
        {
            NotificationHelper.RegisterForNotifications(EventID, TeamID, UserID, this.OnNotify);
            base.OnInitialized();
        }

        private Task OnNotify(Notification notification)
        {
            _ = this.JS.InvokeVoidAsync("displayNotification", notification);
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            NotificationHelper.UnregisterFromNotifications(EventID, TeamID, UserID, this.OnNotify);
        }
    }
}
