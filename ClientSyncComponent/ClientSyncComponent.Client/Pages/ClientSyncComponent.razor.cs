using System.Text;
using Azure;
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
        public bool ReadOnly { get; set; }

        [Parameter]
        public bool SyncEnabled { get; set; } = true;

        [Parameter]
        public int FastestSyncInterval { get; set; } = 0;

        static readonly DateTimeOffset TablesMinTime = new DateTimeOffset(2024, 1, 1, 1, 1, 1, TimeSpan.Zero);
        DateTimeOffset LastSyncUtc = TablesMinTime;

        DateTimeOffset DisplayLastSyncUtc = DateTimeOffset.MinValue;

        TableClient TableClient { get; set; }

        Timer Timer { get; set; }

        bool Paused { get; set; } = false;

        public bool SyncablePuzzleLoaded { get; set; } = false;

        bool IsDevStorage { get; set; } = false;

        protected override Task OnParametersSetAsync()
        {
            TableClient = new TableClient(TableSASUrl);
            if (TableClient.AccountName == "devstoreaccount1")
            {
                IsDevStorage = true;
            }

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
        public async void OnSyncablePuzzleLoadedAsync(string mode)
        {
            SyncablePuzzleLoaded = true;
            if (Timer == null)
            {
                Timer = new Timer(OnTimer, null, 0, 1000);
            }

            if (mode != "coop")
            {
                await OnPauseSyncAsync();
            }

            await InvokeAsync(StateHasChanged);
        }

        [JSInvokable]
        public async void OnPuzzleChangedAsync(JsPuzzleChange[] puzzleChanges)
        {
            if (!SyncEnabled || Paused || ReadOnly)
            {
                return;
            }

            foreach (JsPuzzleChange change in puzzleChanges)
            {
                PuzzleItemProperty puzzleEntry = new PuzzleItemProperty(
                                    puzzleId: PuzzleId,
                                    teamId: TeamId,
                                    playerId: PuzzleUserId,
                                    subPuzzleId: EncodeSubPuzzleId(change.puzzleId),
                                    locationKey: change.locationKey,
                                    propertyKey: change.propertyKey,
                                    value: change.value,
                                    channel: change.channel);

                await TableClient.UpsertEntityAsync(puzzleEntry, TableUpdateMode.Replace);
            }
        }

        [JSInvokable]
        public async Task OnPuzzleResetAsync(JsPuzzleReset resets)
        {
            if (!SyncEnabled || Paused || ReadOnly)
            {
                return;
            }

            // Can't run a transaction on dev storage, so pop an alert instead
            if (IsDevStorage)
            {
                await JSRuntime.InvokeVoidAsync("alert", "Can't reset puzzles with dev storage, set AzureStorageConnectionString to a real storage account.");
                return;
            }

            // Create a reset entry for each sub-puzzle
            foreach (string subPuzzleId in resets.puzzleIds)
            {
                // Delete all prior entries for this sub-puzzle transactionally
                string partitionKey = PuzzleItemProperty.CreatePartitionKey(PuzzleId, TeamId);

                // Azure tables limit transactions to 100 actions, iterate pages
                var pagesToDelete = TableClient.QueryAsync<PuzzleItemProperty>(entry => entry.PartitionKey == partitionKey && entry.SubPuzzleId == EncodeSubPuzzleId(subPuzzleId))
                    .AsPages(null, 100);

                await foreach (Page<PuzzleItemProperty> page in pagesToDelete)
                {
                    List<TableTransactionAction> transactionActions = new List<TableTransactionAction>();

                    foreach (PuzzleItemProperty entry in page.Values)
                    {
                        if (entry.IsReset)
                        {
                            continue;
                        }

                        transactionActions.Add(new TableTransactionAction(TableTransactionActionType.Delete, entry));
                    }

                    // Transactions throw if you submit empty ones
                    if (transactionActions.Count > 0)
                    {
                        await TableClient.SubmitTransactionAsync(transactionActions);
                    }
                }

                PuzzleItemProperty resetEntry = PuzzleItemProperty.CreateReset(PuzzleId, TeamId, Base64UrlTextEncoder.Encode(Encoding.UTF8.GetBytes(subPuzzleId)), PuzzleUserId, channel: resets.channel);
                await TableClient.UpsertEntityAsync(resetEntry);
            }
        }

        [JSInvokable]
        public async Task OnPauseSyncAsync()
        {
            Paused = true;
            if (Timer != null)
            {
                Timer.Change(Timeout.Infinite, Timeout.Infinite);
            }

            // Since we're pausing, we'll lose track of state and need to start from the beginning on unpause
            LastSyncUtc = TablesMinTime;
            DisplayLastSyncUtc = PuzzleUnlockTimeUtc ?? TablesMinTime;

            await JSRuntime.InvokeVoidAsync("onSetCoopMode", "solo");
            await InvokeAsync(StateHasChanged);
        }

        [JSInvokable]
        public async void OnResumeSyncAsync()
        {
            if (Paused)
            {
                bool confirmed = await JSRuntime.InvokeAsync<bool>("confirm", "Are you sure you want to switch to co-op mode? You will lose your local changes and sync up with the group.");
                if (!confirmed)
                {
                    return;
                }
            }

            await JSRuntime.InvokeVoidAsync("onSetCoopMode", "coop");

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

            var unsortedChanges = TableClient.QueryAsync<PuzzleItemProperty>(entry => entry.PartitionKey == PuzzleItemProperty.CreatePartitionKey(PuzzleId, TeamId) && entry.Timestamp > LastSyncUtc);

            List<PuzzleItemProperty> newChanges = new List<PuzzleItemProperty>();
            await foreach (PuzzleItemProperty entry in unsortedChanges)
            {
                newChanges.Add(entry);
            }
            newChanges.Sort((a, b) => a.Timestamp!.Value.CompareTo(b.Timestamp!.Value));

            List<JsPuzzleChange> jsChanges = new List<JsPuzzleChange>();
            foreach (PuzzleItemProperty entry in newChanges)
            {
                foundNewData = true;
                if (entry.IsReset)
                {
                    // If this is a reset, we need to send a reset message to the JS side
                    await JSRuntime.InvokeVoidAsync("onPuzzleResetSynced", new JsPuzzleReset()
                    {
                        puzzleIds = new[] { DecodeSubPuzzleId(entry.SubPuzzleId) },
                        channel = entry.Channel
                    });
                }
                else
                {
                    jsChanges.Add(new JsPuzzleChange()
                    {
                        locationKey = entry.LocationKey,
                        playerId = entry.PlayerId.ToString(),
                        playerIsSelf = (entry.PlayerId == PuzzleUserId) ? "yes" : "",
                        propertyKey = entry.PropertyKey,
                        puzzleId = DecodeSubPuzzleId(entry.SubPuzzleId),
                        teamId = TeamId.ToString(),
                        value = entry.Value,
                        channel = entry.Channel
                    });
                }

                LastSyncUtc = entry.Timestamp > LastSyncUtc ? entry.Timestamp.Value : LastSyncUtc;
                DisplayLastSyncUtc = entry.Timestamp > DisplayLastSyncUtc ? entry.Timestamp.Value : DisplayLastSyncUtc;
            }

            if (foundNewData)
            {
                await JSRuntime.InvokeVoidAsync("onPuzzleSynced", [jsChanges.ToArray()]);
                await InvokeAsync(StateHasChanged);
            }
        }

        /// <summary>
        /// Changes a sub-puzzle ID into a format that is safe to store in Azure Tables
        /// </summary>
        private string EncodeSubPuzzleId(string subPuzzleId)
        {
            return Base64UrlTextEncoder.Encode(Encoding.UTF8.GetBytes(subPuzzleId));
        }

        /// <summary>
        /// Decodes a Azure Tables sub-puzzle ID for use in puzzle.js
        /// </summary>
        private string DecodeSubPuzzleId(string encodedSubPuzzleId)
        {
            return Encoding.UTF8.GetString(Base64UrlTextEncoder.Decode(encodedSubPuzzleId));
        }
    }
}
