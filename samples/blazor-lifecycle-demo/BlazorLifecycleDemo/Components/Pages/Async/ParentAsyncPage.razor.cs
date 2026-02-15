using Microsoft.AspNetCore.Components;

namespace BlazorLifecycleDemo.Components.Pages.Async;

public class ParentAsyncPageBase : ComponentBase, IAsyncDisposable
{
    [Inject] protected ILogger<ParentAsyncPageBase> Logger { get; set; } = default!;

    protected string Message { get; set; } = "初期メッセージ";

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();

        Logger.LogInformation($"[親-{DateTime.Now:HH:mm:ss.fff}] ====================");
        Logger.LogInformation($"[親-{DateTime.Now:HH:mm:ss.fff}] OnInitializedAsync() 開始");

        await Task.Delay(100); // 非同期処理をシミュレート

        Logger.LogInformation($"[親-{DateTime.Now:HH:mm:ss.fff}] OnInitializedAsync() 完了");
    }

    protected override async Task OnParametersSetAsync()
    {
        await base.OnParametersSetAsync();

        Logger.LogInformation($"[親-{DateTime.Now:HH:mm:ss.fff}] OnParametersSetAsync() 開始");

        await Task.Delay(100); // 非同期処理をシミュレート

        Logger.LogInformation($"[親-{DateTime.Now:HH:mm:ss.fff}] OnParametersSetAsync() 完了");
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);

        Logger.LogInformation($"[親-{DateTime.Now:HH:mm:ss.fff}] OnAfterRenderAsync(firstRender={firstRender}) 開始");

        await Task.Delay(100); // 非同期処理をシミュレート

        Logger.LogInformation($"[親-{DateTime.Now:HH:mm:ss.fff}] OnAfterRenderAsync(firstRender={firstRender}) 完了");
    }

    public async ValueTask DisposeAsync()
    {
        Logger.LogInformation($"[親-{DateTime.Now:HH:mm:ss.fff}] DisposeAsync()");
        await Task.CompletedTask;
    }
}
