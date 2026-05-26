using System;
using System.Collections.Generic;
using System.Linq;
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
        public ThreadMessageDTO[] InitialMessages { get; set; }
        
        [Parameter]
        public bool IsPlayRole { get; set; }

        [Parameter]
        public int CurrentUserId { get; set; }

        [Parameter]
        public bool CanModifyMessages { get; set; }

        [Parameter]
        public bool CanSendMessage { get; set; }

        [Parameter]
        public bool IsEmailOnlyMode { get; set; }

        [Parameter]
        public string NewMessageText { get; set; }

        [Parameter]
        public int EventID { get; set; }

        [Parameter]
        public string Subject { get; set; }

        [Parameter]
        public int PuzzleID { get; set; }

        [Parameter]
        public int? TeamID { get; set; }

        [Parameter]
        public int? PlayerID { get; set; }

        [Parameter]
        public bool IsFromGameControl { get; set; }

        [Parameter]
        public bool CanClaim { get; set; }

        [Parameter]
        public string DeletedMessageText { get; set; }

        [Inject]
        public ServerMessageListener MessageListener { get; set; }
        
        [Inject]
        public IJSRuntime JSRuntime { get; set; }

        [Inject]
        public PuzzleThreadService PuzzleThreadService { get; set; }

        public List<ThreadMessageDTO> messages = new List<ThreadMessageDTO>();

        public Dictionary<int, string> editTexts = new Dictionary<int, string>();

        public string errorMessage;

        ThreadMessageDTO LatestMessage => messages.FirstOrDefault();

        protected override async Task OnInitializedAsync()
        {
            if (InitialMessages != null)
            {
                messages.AddRange(InitialMessages.OrderByDescending(message => message.CreatedDateTimeInUtc));
            }

            await MessageListener.EnsureInitializedAsync();
            MessageListener.OnThreadMessage += OnThreadMessageReceived;
        }

        async Task OnThreadMessageReceived(ThreadMessageDTO incoming)
        {
            if (incoming?.ThreadId == ThreadId)
            {
                await InvokeAsync(() =>
                {
                    UpsertMessage(incoming);
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

        bool IsAllowedToEditMessage(ThreadMessageDTO message)
        {
            return CanModifyMessages && message.SenderID == CurrentUserId && message.Text != DeletedMessageText;
        }

        bool IsAllowedToDeleteMessage(ThreadMessageDTO message)
        {
            return CanModifyMessages && message.SenderID == CurrentUserId && message.Text != DeletedMessageText;
        }

        string GetActionColor(ThreadMessageDTO message)
        {
            return message.IsFromGameControl ? "#0310d6" : "blue";
        }

        bool IsEditing(ThreadMessageDTO message)
        {
            return editTexts.ContainsKey(message.ID);
        }

        void StartEdit(ThreadMessageDTO message)
        {
            errorMessage = null;
            editTexts[message.ID] = message.Text;
        }

        void CancelEdit(ThreadMessageDTO message)
        {
            errorMessage = null;
            editTexts.Remove(message.ID);
        }

        bool ShouldShowClaimAction()
        {
            return CanClaim
                && LatestMessage != null
                && !LatestMessage.ClaimerID.HasValue
                && !LatestMessage.IsFromGameControl;
        }

        bool ShouldShowUnclaimAction()
        {
            return CanClaim
                && LatestMessage != null
                && LatestMessage.ClaimerID.HasValue;
        }

        async Task EditMessageAsync(ThreadMessageDTO message)
        {
            if (!editTexts.TryGetValue(message.ID, out string text))
            {
                return;
            }

            await RunMutationAsync(async () =>
            {
                ThreadMessageDTO updatedMessage = await PuzzleThreadService.EditMessageAsync(message.ID, CurrentUserId, text);
                editTexts.Remove(message.ID);
                UpsertMessage(updatedMessage);
            });
        }

        async Task SendMessageAsync()
        {
            await RunMutationAsync(async () =>
            {
                ThreadMessageDTO sentMessage = await PuzzleThreadService.SendMessageAsync(
                    ThreadId,
                    EventID,
                    Subject,
                    PuzzleID,
                    TeamID,
                    PlayerID,
                    IsFromGameControl,
                    CurrentUserId,
                    NewMessageText);

                UpsertMessage(sentMessage);
                NewMessageText = null;
            });
        }

        async Task DeleteMessageAsync(ThreadMessageDTO message)
        {
            bool confirmed = await JSRuntime.InvokeAsync<bool>("confirm", "Are you sure you want to delete this message?");
            if (!confirmed)
            {
                return;
            }

            await RunMutationAsync(async () =>
            {
                ThreadMessageDTO updatedMessage = await PuzzleThreadService.DeleteMessageAsync(message.ID, CurrentUserId);
                editTexts.Remove(message.ID);
                UpsertMessage(updatedMessage);
            });
        }

        async Task ClaimLatestMessageAsync()
        {
            if (LatestMessage == null)
            {
                return;
            }

            await RunMutationAsync(async () =>
            {
                ThreadMessageDTO updatedMessage = await PuzzleThreadService.ClaimThreadAsync(LatestMessage.ID, CurrentUserId);
                UpsertMessage(updatedMessage);
            });
        }

        async Task UnclaimLatestMessageAsync()
        {
            if (LatestMessage == null)
            {
                return;
            }

            await RunMutationAsync(async () =>
            {
                ThreadMessageDTO updatedMessage = await PuzzleThreadService.UnclaimThreadAsync(LatestMessage.ID, CurrentUserId);
                UpsertMessage(updatedMessage);
            });
        }

        async Task RunMutationAsync(Func<Task> mutation)
        {
            try
            {
                errorMessage = null;
                await mutation();
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
            }
        }

        void UpsertMessage(ThreadMessageDTO message)
        {
            int existingIndex = messages.FindIndex(existingMessage => existingMessage.ID == message.ID);
            if (existingIndex >= 0)
            {
                messages[existingIndex] = message;
            }
            else
            {
                messages.Insert(0, message);
            }

            messages = messages
                .OrderByDescending(existingMessage => existingMessage.CreatedDateTimeInUtc)
                .ToList();
        }

        public void Dispose()
        {
            MessageListener.OnThreadMessage -= OnThreadMessageReceived;
        }
    }
}