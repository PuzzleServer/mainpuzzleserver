﻿using System.Text;
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

        DateTimeOffset LastSyncUtc = DateTimeOffset.MinValue;

        TableClient TableClient { get; set; }

        List<string> ReceivedValues { get; set; } = new List<string>();

        Timer Timer { get; set; }

        protected override Task OnParametersSetAsync()
        {
            TableClient = new TableClient(TableSASUrl);
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
            await SyncAsync();
            string visibility = await JSRuntime.InvokeAsync<string>("getVisibility");
            Console.WriteLine("Timer ticked. Visibility is " + visibility);

            int newInterval = CalculateInterval();
            Timer.Change(newInterval, newInterval);
        }

        /// <summary>
        /// Heuristic to minimize load depending on how active a puzzle is
        /// </summary>
        /// <returns>Interval in milliseconds to poll storage at</returns>
        private int CalculateInterval()
        {
            // todo: get a multiplier or override from the server for central updates?
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
            else
            {
                return 60000;
            }
        }

        [JSInvokable]
        public void VisibilityChanged(string visibility)
        {
            Console.WriteLine($".NET Visibility changed to {visibility}");
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
        public void OnSyncablePuzzleLoaded()
        {
            Console.WriteLine("Syncable puzzle loaded, starting sync timer");
            if (Timer == null)
            {
                Timer = new Timer(OnTimer, null, 1000, 1000);
            }
        }

        [JSInvokable]
        public async void OnPuzzleChangedAsync(JsPuzzleChange[] puzzleChanges)
        {
            List<TableTransactionAction> actions = new List<TableTransactionAction>(puzzleChanges.Length);

            foreach (JsPuzzleChange change in puzzleChanges)
            {
                Console.WriteLine($"Puzzle changed: {change.puzzleId} {change.teamId} {change.playerId} {change.locationKey} {change.propertyKey} {change.value}");

                PuzzleEntry puzzleEntry = new PuzzleEntry(
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

        private async Task SyncAsync()
        {
            bool foundNewData = false;

            var newChanges = TableClient.QueryAsync<PuzzleEntry>(entry => entry.PartitionKey == PuzzleEntry.CreatePartitionKey(PuzzleId, TeamId) && entry.Timestamp > LastSyncUtc);
            
            List<JsPuzzleChange> jsChanges = new List<JsPuzzleChange>();
            await foreach (PuzzleEntry entry in newChanges)
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
            }

            if (foundNewData)
            {
                await JSRuntime.InvokeVoidAsync("onPuzzleSynced", [jsChanges.ToArray()]);
                await InvokeAsync(StateHasChanged);
            }
        }
    }
}
