
using Azure.Data.Tables;
using Microsoft.AspNetCore.Components.Web;

namespace ClientSyncComponent.Client.Pages
{
    public partial class ClientSyncComponent
    {
        DateTime LastSyncUtc = DateTime.MinValue;
        TableClient TableClient { get; set; }

        protected override Task OnParametersSetAsync()
        {
            TableClient = new TableClient("UseDevelopmentStorage=true", "WasmTestTable");
            return base.OnParametersSetAsync();
        }

        private async Task SyncAsync(MouseEventArgs e)
        {
        }

        private async Task PushChangeAsync(MouseEventArgs e)
        {
            PuzzleEntry entry = new PuzzleEntry(
                puzzleId: 1,
                teamId: 2,
                playerId: 3,
                locationKey: "location",
                propertyKey: "property",
                value: "value" + Random.Shared.Next());
            await TableClient.UpsertEntityAsync(entry);
        }
    }
}
