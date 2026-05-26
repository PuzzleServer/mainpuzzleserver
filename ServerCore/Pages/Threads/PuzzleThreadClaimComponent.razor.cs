using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using ServerCore.ServerMessages;

namespace ServerCore.Pages.Threads
{
    public partial class PuzzleThreadClaimComponent : ComponentBase, IDisposable
    {
        [Parameter]
        public string ThreadId { get; set; }

        [Parameter]
        public bool CanClaim { get; set; }

        [Parameter]
        public ThreadMessageDTO InitialLatestMessage { get; set; }

        [Inject]
        public ServerMessageListener MessageListener { get; set; }

        public ThreadMessageDTO latestMessage;

        protected override async Task OnInitializedAsync()
        {
            latestMessage = InitialLatestMessage;
            await MessageListener.EnsureInitializedAsync();
            MessageListener.OnThreadMessage += OnThreadMessageReceived;
        }

        async Task OnThreadMessageReceived(ThreadMessageDTO incoming)
        {
            if (incoming?.ThreadId == ThreadId)
            {
                await InvokeAsync(() =>
                {
                    latestMessage = incoming;
                    StateHasChanged();
                });
            }
        }

        bool ShouldShowClaimAction()
        {
            return CanClaim
                && latestMessage != null
                && !latestMessage.ClaimerID.HasValue
                && !latestMessage.IsFromGameControl;
        }

        bool ShouldShowUnclaimAction()
        {
            return CanClaim
                && latestMessage != null
                && latestMessage.ClaimerID.HasValue;
        }

        string GetClaimAction()
        {
            return GetAction("ClaimThread");
        }

        string GetUnclaimAction()
        {
            return GetAction("UnclaimThread");
        }

        string GetAction(string handler)
        {
            string action = $"?handler={handler}&messageId={latestMessage.ID}&puzzleId={latestMessage.PuzzleID}";
            if (latestMessage.TeamID.HasValue)
            {
                action += $"&teamId={latestMessage.TeamID.Value}";
            }
            if (latestMessage.PlayerID.HasValue)
            {
                action += $"&playerId={latestMessage.PlayerID.Value}";
            }

            return action;
        }

        public void Dispose()
        {
            MessageListener.OnThreadMessage -= OnThreadMessageReceived;
        }
    }
}