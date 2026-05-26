using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using ServerCore.ServerMessages;

namespace ServerCore.Pages.Threads
{
    public partial class PuzzleThreadComponent : ComponentBase, IDisposable
    {
        [Parameter]
        public string ThreadId { get; set; }
        
        [Parameter]
        public bool IsPlayRole { get; set; }

        [Inject]
        public ServerMessageListener MessageListener { get; set; }
        
        [Inject]
        public IJSRuntime JSRuntime { get; set; }

        public List<ThreadMessageDTO> messages = new List<ThreadMessageDTO>();

        protected override async Task OnInitializedAsync()
        {
            await MessageListener.EnsureInitializedAsync();
            MessageListener.OnThreadMessage += OnThreadMessageReceived;
        }

        async Task OnThreadMessageReceived(ThreadMessageDTO incoming)
        {
            if (incoming?.ThreadId == ThreadId)
            {
                await InvokeAsync(() =>
                {
                    messages.Insert(0, incoming);
                    StateHasChanged();
                });
            }
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (messages.Count > 0)
            {
                await JSRuntime.InvokeVoidAsync("renderLocalTimes");
            }
        }

        string GetSenderName(ThreadMessageDTO message)
        {
            if (message.IsFromGameControl)
            {
                return IsPlayRole ? "Game control" : $"Game control ({message.SenderName})";
            }

            return message.SenderName ?? "Unknown";
        }

        public void Dispose()
        {
            MessageListener.OnThreadMessage -= OnThreadMessageReceived;
        }
    }
}