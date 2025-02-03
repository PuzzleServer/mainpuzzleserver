
using Azure.Data.Tables;

namespace ClientSyncComponent.Client.Pages
{
    public partial class ClientSyncComponent
    {

        protected override async Task OnInitializedAsync()
        {
            TableClient tableClient = new TableClient("UseDevelopmentStorage=true", "WasmTestTable");
            await tableClient.CreateIfNotExistsAsync();
            await base.OnInitializedAsync();
        }
    }
}
