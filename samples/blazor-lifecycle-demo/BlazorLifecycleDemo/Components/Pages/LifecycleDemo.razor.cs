namespace BlazorLifecycleDemo.Components.Pages
{
    public partial class LifecycleDemo
    {
        private int currentCount = 0;
        private string message = "初期メッセージ";
        private List<string> lifecycleLogs = new();

        protected override void OnInitialized()
        {
            var logMessage = $"[{DateTime.Now:HH:mm:ss.fff}] OnInitialized() - 同期版";
            lifecycleLogs.Add(logMessage);
            Logger.LogInformation(logMessage);
        }

        protected override async Task OnInitializedAsync()
        {
            var logMessage = $"[{DateTime.Now:HH:mm:ss.fff}] OnInitializedAsync() - 非同期版";
            lifecycleLogs.Add(logMessage);
            Logger.LogInformation(logMessage);

            // 非同期処理をシミュレート
            await Task.Delay(100);

            var completedMessage = $"[{DateTime.Now:HH:mm:ss.fff}] OnInitializedAsync() 完了";
            lifecycleLogs.Add(completedMessage);
            Logger.LogInformation(completedMessage);
        }

        protected override void OnParametersSet()
        {
            var logMessage = $"[{DateTime.Now:HH:mm:ss.fff}] OnParametersSet() - 同期版";
            lifecycleLogs.Add(logMessage);
            Logger.LogInformation(logMessage);
        }

        protected override async Task OnParametersSetAsync()
        {
            var logMessage = $"[{DateTime.Now:HH:mm:ss.fff}] OnParametersSetAsync() - 非同期版";
            lifecycleLogs.Add(logMessage);
            Logger.LogInformation(logMessage);
            await Task.CompletedTask;
        }

        protected override void OnAfterRender(bool firstRender)
        {
            var logMessage = $"[{DateTime.Now:HH:mm:ss.fff}] OnAfterRender(firstRender={firstRender}) - 同期版";
            lifecycleLogs.Add(logMessage);
            Logger.LogInformation(logMessage);
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            var logMessage = $"[{DateTime.Now:HH:mm:ss.fff}] OnAfterRenderAsync(firstRender={firstRender}) - 非同期版";
            lifecycleLogs.Add(logMessage);
            Logger.LogInformation(logMessage);
            await Task.CompletedTask;
        }

        private void IncrementCount()
        {
            currentCount++;
            Logger.LogInformation($"[{DateTime.Now:HH:mm:ss.fff}] IncrementCount() - カウント={currentCount}");
        }

        private void ForceRerender()
        {
            Logger.LogInformation($"[{DateTime.Now:HH:mm:ss.fff}] ForceRerender() - StateHasChanged()を呼び出し");
            StateHasChanged();
        }

        private void OnMessageChanged()
        {
            Logger.LogInformation($"[{DateTime.Now:HH:mm:ss.fff}] OnMessageChanged() - メッセージ変更: {message}");
        }

        public void Dispose()
        {
            var logMessage = $"[{DateTime.Now:HH:mm:ss.fff}] Dispose() - コンポーネント破棄";
            lifecycleLogs.Add(logMessage);
            Logger.LogInformation(logMessage);
        }
    }
}
