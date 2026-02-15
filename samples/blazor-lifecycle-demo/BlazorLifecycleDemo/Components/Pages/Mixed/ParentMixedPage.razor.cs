using Microsoft.AspNetCore.Components;

namespace BlazorLifecycleDemo.Components.Pages.Mixed;

public class ParentMixedPageBase : ComponentBase, IAsyncDisposable
{
    [Inject] protected ILogger<ParentMixedPageBase> Logger { get; set; } = default!;

    protected string Message { get; set; } = "初期メッセージ";

    protected override void OnInitialized()
    {
        base.OnInitialized();

        Logger.LogInformation($"[親-{DateTime.Now:HH:mm:ss.fff}] ====================");
        Logger.LogInformation($"[親-{DateTime.Now:HH:mm:ss.fff}] OnInitialized() 開始");
        Logger.LogInformation($"[親-{DateTime.Now:HH:mm:ss.fff}] OnInitialized() 完了");
    }

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();

        Logger.LogInformation($"[親-{DateTime.Now:HH:mm:ss.fff}] OnInitializedAsync() 開始");

        await Task.Delay(100);

        Logger.LogInformation($"[親-{DateTime.Now:HH:mm:ss.fff}] OnInitializedAsync() 完了");
    }

    protected override void OnParametersSet()
    {
        base.OnParametersSet();

        Logger.LogInformation($"[親-{DateTime.Now:HH:mm:ss.fff}] OnParametersSet() 開始");
        Logger.LogInformation($"[親-{DateTime.Now:HH:mm:ss.fff}] OnParametersSet() 完了");
    }

    protected override async Task OnParametersSetAsync()
    {
        await base.OnParametersSetAsync();

        Logger.LogInformation($"[親-{DateTime.Now:HH:mm:ss.fff}] OnParametersSetAsync() 開始");

        await Task.Delay(100);

        Logger.LogInformation($"[親-{DateTime.Now:HH:mm:ss.fff}] OnParametersSetAsync() 完了");
    }

    protected override void OnAfterRender(bool firstRender)
    {
        base.OnAfterRender(firstRender);

        Logger.LogInformation($"[親-{DateTime.Now:HH:mm:ss.fff}] OnAfterRender(firstRender={firstRender}) 開始");
        Logger.LogInformation($"[親-{DateTime.Now:HH:mm:ss.fff}] OnAfterRender(firstRender={firstRender}) 完了");
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);

        Logger.LogInformation($"[親-{DateTime.Now:HH:mm:ss.fff}] OnAfterRenderAsync(firstRender={firstRender}) 開始");

        await Task.Delay(100);

        Logger.LogInformation($"[親-{DateTime.Now:HH:mm:ss.fff}] OnAfterRenderAsync(firstRender={firstRender}) 完了");
    }

    public async ValueTask DisposeAsync()
    {
        Logger.LogInformation($"[親-{DateTime.Now:HH:mm:ss.fff}] DisposeAsync()");
        await Task.CompletedTask;
    }
}
