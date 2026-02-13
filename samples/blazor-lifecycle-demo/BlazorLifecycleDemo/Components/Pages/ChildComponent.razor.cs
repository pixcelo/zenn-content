using Microsoft.AspNetCore.Components;

namespace BlazorLifecycleDemo.Components.Pages
{
    public partial class ChildComponent
    {
        [Parameter]
        public string Message { get; set; } = string.Empty;

        private List<string> childLifecycleLogs = new();

        protected override void OnInitialized()
        {
            var logMessage = $"[子-{DateTime.Now:HH:mm:ss.fff}] OnInitialized()";
            childLifecycleLogs.Add(logMessage);
            Logger.LogInformation(logMessage);
        }

        protected override async Task OnInitializedAsync()
        {
            var logMessage = $"[子-{DateTime.Now:HH:mm:ss.fff}] OnInitializedAsync()";
            childLifecycleLogs.Add(logMessage);
            Logger.LogInformation(logMessage);
            await Task.Delay(50); // 非同期処理をシミュレート
        }

        protected override void OnParametersSet()
        {
            var logMessage = $"[子-{DateTime.Now:HH:mm:ss.fff}] OnParametersSet() - Message={Message}";
            childLifecycleLogs.Add(logMessage);
            Logger.LogInformation(logMessage);
        }

        protected override async Task OnParametersSetAsync()
        {
            var logMessage = $"[子-{DateTime.Now:HH:mm:ss.fff}] OnParametersSetAsync()";
            childLifecycleLogs.Add(logMessage);
            Logger.LogInformation(logMessage);
            await Task.CompletedTask;
        }

        protected override void OnAfterRender(bool firstRender)
        {
            var logMessage = $"[子-{DateTime.Now:HH:mm:ss.fff}] OnAfterRender(firstRender={firstRender})";
            childLifecycleLogs.Add(logMessage);
            Logger.LogInformation(logMessage);
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            var logMessage = $"[子-{DateTime.Now:HH:mm:ss.fff}] OnAfterRenderAsync(firstRender={firstRender})";
            childLifecycleLogs.Add(logMessage);
            Logger.LogInformation(logMessage);
            await Task.CompletedTask;
        }

        public void Dispose()
        {
            var logMessage = $"[子-{DateTime.Now:HH:mm:ss.fff}] Dispose()";
            childLifecycleLogs.Add(logMessage);
            Logger.LogInformation(logMessage);
        }
    }
}
