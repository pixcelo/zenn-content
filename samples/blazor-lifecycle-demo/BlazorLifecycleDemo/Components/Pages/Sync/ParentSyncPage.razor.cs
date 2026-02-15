using Microsoft.AspNetCore.Components;

namespace BlazorLifecycleDemo.Components.Pages.Sync;

public class ParentSyncPageBase : ComponentBase, IDisposable
{
    [Inject] protected ILogger<ParentSyncPageBase> Logger { get; set; } = default!;

    protected string Message { get; set; } = "初期メッセージ";

    protected override void OnInitialized()
    {
        base.OnInitialized();

        Logger.LogInformation($"[親-{DateTime.Now:HH:mm:ss.fff}] ====================");
        Logger.LogInformation($"[親-{DateTime.Now:HH:mm:ss.fff}] OnInitialized() 開始");
        Logger.LogInformation($"[親-{DateTime.Now:HH:mm:ss.fff}] OnInitialized() 完了");
    }

    protected override void OnParametersSet()
    {
        base.OnParametersSet();

        Logger.LogInformation($"[親-{DateTime.Now:HH:mm:ss.fff}] OnParametersSet() 開始");
        Logger.LogInformation($"[親-{DateTime.Now:HH:mm:ss.fff}] OnParametersSet() 完了");
    }

    protected override void OnAfterRender(bool firstRender)
    {
        base.OnAfterRender(firstRender);

        Logger.LogInformation($"[親-{DateTime.Now:HH:mm:ss.fff}] OnAfterRender(firstRender={firstRender}) 開始");
        Logger.LogInformation($"[親-{DateTime.Now:HH:mm:ss.fff}] OnAfterRender(firstRender={firstRender}) 完了");
    }

    public void Dispose()
    {
        Logger.LogInformation($"[親-{DateTime.Now:HH:mm:ss.fff}] Dispose()");
    }
}
