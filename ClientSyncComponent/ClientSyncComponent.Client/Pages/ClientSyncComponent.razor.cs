using System.Text;
using Azure.Data.Tables;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.JSInterop;

namespace ClientSyncComponent.Client.Pages
{
    /// <summary>
    /// Component that integrates with puzzle.js to sync changes to Azure Tables
    /// </summary>
    public partial class ClientSyncComponent
    {
        [Inject]
        IJSRuntime JSRuntime { get; set; }

        [Parameter]
        public int PuzzleId { get; set; }

        [Parameter]
        public int TeamId { get; set; }

        [Parameter]
        public int PuzzleUserId { get; set; }

        [Parameter]
        public Uri TableSASUrl { get; set; }

        [Parameter]
        public DateTimeOffset? PuzzleUnlockTimeUtc { get; set; }

        [Parameter]
        public DateTimeOffset EventEndTimeUtc { get; set; }

        [Parameter]
        public bool SyncEnabled { get; set; } = true;

        [Parameter]
        public int FastestSyncInterval { get; set; } = 0;

        static readonly DateTimeOffset TablesMinTime = new DateTimeOffset(2024, 1, 1, 1, 1, 1, TimeSpan.Zero);
        DateTimeOffset LastSyncUtc = TablesMinTime;

        DateTimeOffset DisplayLastSyncUtc = DateTimeOffset.MinValue;

        TableClient TableClient { get; set; }

        List<string> ReceivedValues { get; set; } = new List<string>();

        Timer Timer { get; set; }

        bool Paused { get; set; } = false;

        public bool SyncablePuzzleLoaded { get; set; } = false;

        protected override Task OnParametersSetAsync()
        {
            TableClient = new TableClient(TableSASUrl);
            DisplayLastSyncUtc = PuzzleUnlockTimeUtc ?? TablesMinTime;

            if (!SyncEnabled)
            {
                Timer?.Change(Timeout.Infinite, Timeout.Infinite);
            }
            else
            {
                // Sync immediately and then change the timer as necessary
                Timer?.Change(0, CalculateInterval());
            }

            return base.OnParametersSetAsync();
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                await JSRuntime.InvokeVoidAsync("registerDotNet", DotNetObjectReference.Create(this));
            }

            await base.OnAfterRenderAsync(firstRender);
        }

        private async void OnTimer(object? state)
        {
            if (Paused)
            {
                return;
            }

            await SyncAsync();

            int newInterval = CalculateInterval();
            Timer.Change(newInterval, newInterval);
        }

        /// <summary>
        /// Heuristic to minimize load depending on how active a puzzle is
        /// </summary>
        /// <returns>Interval in milliseconds to poll storage at</returns>
        private int CalculateInterval()
        {
            return Math.Max(FastestSyncInterval, CalculateIntervalFromLastSync());
        }

        private int CalculateIntervalFromLastSync()
        {
            TimeSpan timeSinceUpdate = DateTimeOffset.UtcNow - LastSyncUtc;
            if (timeSinceUpdate.TotalSeconds < 120)
            {
                return 1000;
            }
            else if (timeSinceUpdate.TotalMinutes < 10)
            {
                return 2000;
            }
            else if (timeSinceUpdate.TotalHours < 1)
            {
                return 5000;
            }
            else if (timeSinceUpdate.TotalHours < 6)
            {
                return 10000;
            }
            else if (EventEndTimeUtc > DateTimeOffset.UtcNow)
            {
                return 20000;
            }
            else
            {
                return 60000;
            }
        }

        [JSInvokable]
        public void VisibilityChanged(string visibility)
        {
            if (visibility == "visible")
            {
                Timer.Change(0, CalculateInterval());
            }
            else
            {
                Timer.Change(Timeout.Infinite, Timeout.Infinite);
            }
        }

        [JSInvokable]
        public async void OnSyncablePuzzleLoadedAsync()
        {
            SyncablePuzzleLoaded = true;
            if (Timer == null)
            {
                Timer = new Timer(OnTimer, null, 0, 1000);
            }
            
            await InvokeAsync(StateHasChanged);
        }

        [JSInvokable]
        public async void OnPuzzleChangedAsync(JsPuzzleChange[] puzzleChanges)
        {
            if (!SyncEnabled)
            {
                return;
            }

            foreach (JsPuzzleChange change in puzzleChanges)
            {
                PuzzleItemProperty puzzleEntry = new PuzzleItemProperty(
                                    puzzleId: PuzzleId,
                                    teamId: TeamId,
                                    playerId: PuzzleUserId,
                                    subPuzzleId: Base64UrlTextEncoder.Encode(Encoding.UTF8.GetBytes(change.puzzleId)),
                                    locationKey: change.locationKey,
                                    propertyKey: change.propertyKey,
                                    value: change.value,
                                    channel: change.channel);

                await TableClient.UpsertEntityAsync(puzzleEntry);
            }
        }

        [JSInvokable]
        public async void OnPauseSyncAsync()
        {
            Paused = true;
            if (Timer != null)
            {
                Timer.Change(Timeout.Infinite, Timeout.Infinite);
            }

            // Since we're pausing, we'll lose track of state and need to start from the beginning on unpause
            LastSyncUtc = TablesMinTime;
            DisplayLastSyncUtc = PuzzleUnlockTimeUtc ?? TablesMinTime;
            await InvokeAsync(StateHasChanged);
        }

        [JSInvokable]
        public async void OnResumeSyncAsync()
        {
            Paused = false;
            if (Timer != null)
            {
                Timer.Change(0, CalculateInterval());
            }
            await InvokeAsync(StateHasChanged);
        }

        private async Task SyncAsync()
        {
            bool foundNewData = false;

            var newChanges = TableClient.QueryAsync<PuzzleItemProperty>(entry => entry.PartitionKey == PuzzleItemProperty.CreatePartitionKey(PuzzleId, TeamId) && entry.Timestamp > LastSyncUtc);
            
            List<JsPuzzleChange> jsChanges = new List<JsPuzzleChange>();
            await foreach (PuzzleItemProperty entry in newChanges)
            {
                foundNewData = true;
                ReceivedValues.Add(entry.Value);
                jsChanges.Add(new JsPuzzleChange()
                {
                    locationKey = entry.LocationKey,
                    playerId = PuzzleUserId.ToString(),
                    propertyKey = entry.PropertyKey,
                    puzzleId = Encoding.UTF8.GetString(Base64UrlTextEncoder.Decode(entry.SubPuzzleId)),
                    teamId = TeamId.ToString(),
                    value = entry.Value
                });
                LastSyncUtc = entry.Timestamp > LastSyncUtc ? entry.Timestamp.Value : LastSyncUtc;
                DisplayLastSyncUtc = entry.Timestamp > DisplayLastSyncUtc ? entry.Timestamp.Value : DisplayLastSyncUtc;
            }

            if (foundNewData)
            {
                await JSRuntime.InvokeVoidAsync("onPuzzleSynced", [jsChanges.ToArray()]);
                await InvokeAsync(StateHasChanged);
            }
        }
    }
}
