
using System.Diagnostics;
using Azure.Data.Tables;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;

namespace ClientSyncComponent.Client.Pages
{
    public partial class ClientSyncComponent
    {
        [Inject]
        IJSRuntime JSRuntime { get; set; }

        DateTimeOffset LastSyncUtc = DateTimeOffset.MinValue;

        TableClient TableClient { get; set; }

        List<string> ReceivedValues { get; set; } = new List<string>();

        Timer Timer { get; set; }

        protected override Task OnParametersSetAsync()
        {
            TableClient = new TableClient("UseDevelopmentStorage=true", "WasmTestTable");
            return base.OnParametersSetAsync();
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                await JSRuntime.InvokeVoidAsync("registerDotNet", DotNetObjectReference.Create(this));

                Timer = new Timer(OnTimer, null, 1000, 1000);
            }

            await base.OnAfterRenderAsync(firstRender);
        }

        private async void OnTimer(object? state)
        {
            await SyncAsync(null);
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
            // todo morganb: get a multiplier or override from the server for central updates?
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

        const int testPuzzleId = 1;
        const int testTeamId = 2;

        private async Task SyncAsync(MouseEventArgs? _)
        {
            bool foundNewData = false;

            var newChanges = TableClient.QueryAsync<PuzzleEntry>(entry => entry.PartitionKey == PuzzleEntry.CreatePartitionKey(testPuzzleId, testTeamId) && entry.Timestamp > LastSyncUtc);
            await foreach (PuzzleEntry entry in newChanges)
            {
                foundNewData = true;
                ReceivedValues.Add(entry.Value);
                LastSyncUtc = entry.Timestamp > LastSyncUtc ? entry.Timestamp.Value : LastSyncUtc;
            }

            if (foundNewData)
            {
                await InvokeAsync(StateHasChanged);
            }
        }

        // todo morganb: changes are an array and sometimes have multiple items. Try to batch
        private async Task PushChangeAsync(MouseEventArgs? _)
        {
            PuzzleEntry entry = new PuzzleEntry(
                puzzleId: testPuzzleId,
                teamId: testTeamId,
                playerId: 3,
                locationKey: "location",
                propertyKey: "property",
                value: "value" + Random.Shared.Next(),
                channel: "MTV");
            await TableClient.UpsertEntityAsync(entry);
        }
    }
}
