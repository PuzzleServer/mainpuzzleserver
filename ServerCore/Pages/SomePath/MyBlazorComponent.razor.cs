namespace ServerCore.Pages.SomePath
{
    /// <summary>
    /// Codebehind class for a component. Note that it's partial
    /// </summary>
    public partial class MyBlazorComponent
    {
        /// <summary>
        /// Property that can be referenced from the layout code
        /// </summary>
        public int CounterVal { get; set; } = 0;

        /// <summary>
        /// Event handler that can be referenced by the layout code
        /// </summary>
        void IncrementCounter()
        {
            // Modifying the property in the event handler automatically updates the rendered view
            // If this were called from something other than Blazor, it should call InvokeAsync(() => StateHasChanged());
            CounterVal++;
        }
    }
}
