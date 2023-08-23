using System;
using Microsoft.AspNetCore.Components;

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

        private IDisposable listener = null;

        // TODO this is just a placeholder until Bootstrap 5
        public string NotificationText { get; set; } = "";

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
            // TODO this is just a placeholder until Bootstrap 5
            NotificationText += $"{args.Notification.Title}: {args.Notification.Content} ";
            InvokeAsync(() => StateHasChanged());
        }

        public void Dispose()
        {
            this.listener?.Dispose();
        }
    }
}
