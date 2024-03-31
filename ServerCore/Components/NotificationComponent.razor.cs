using System;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace ServerCore.Components
{
    public partial class NotificationComponent : IDisposable
    {
        [Parameter]
        public int EventID { get; set; }

        [Parameter]
        public int TeamID { get; set; }

        [Parameter]
        public int PlayerID { get; set; }

        [Inject]
        protected IJSRuntime JS { get; set; } = default!;

        private IDisposable listener = null;

        public NotificationComponent()
        {
        }

        protected override void OnInitialized()
        {
            this.listener = NotificationHelper.RegisterForNotifications(EventID, TeamID, PlayerID, this.OnNotification);
            base.OnInitialized();
        }

        private void OnNotification(object sender, NotificationEventArgs args)
        {
            _ = this.JS.InvokeVoidAsync("displayNotification", args.Notification);
        }

        public void Dispose()
        {
            this.listener?.Dispose();
        }
    }
}
