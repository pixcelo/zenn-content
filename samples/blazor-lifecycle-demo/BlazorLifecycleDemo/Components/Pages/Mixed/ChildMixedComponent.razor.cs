using Microsoft.AspNetCore.Components;

namespace BlazorLifecycleDemo.Components.Pages.Mixed;

public class ChildMixedComponentBase : ComponentBase, IDisposable
{
    [Inject] protected ILogger<ChildMixedComponentBase> Logger { get; set; } = default!;

    /// <summary>
    /// 親コンポーネントから渡されるメッセージ。
    /// [Parameter] 属性により、親からの値がSetParametersAsyncで自動的に設定される。
    /// この属性がない場合、親から値を渡しても設定されない。
    /// </summary>
    [Parameter] public string Message { get; set; } = string.Empty;

    protected override void OnInitialized()
    {
        base.OnInitialized();

        Logger.LogInformation($"[子-{DateTime.Now:HH:mm:ss.fff}] OnInitialized() 開始");
        Logger.LogInformation($"[子-{DateTime.Now:HH:mm:ss.fff}] OnInitialized() 完了");
    }

    protected override void OnParametersSet()
    {
        base.OnParametersSet();

        Logger.LogInformation($"[子-{DateTime.Now:HH:mm:ss.fff}] OnParametersSet() 開始 - Message={Message}");
        Logger.LogInformation($"[子-{DateTime.Now:HH:mm:ss.fff}] OnParametersSet() 完了");
    }

    protected override void OnAfterRender(bool firstRender)
    {
        base.OnAfterRender(firstRender);

        Logger.LogInformation($"[子-{DateTime.Now:HH:mm:ss.fff}] OnAfterRender(firstRender={firstRender}) 開始");
        Logger.LogInformation($"[子-{DateTime.Now:HH:mm:ss.fff}] OnAfterRender(firstRender={firstRender}) 完了");
    }

    public void Dispose()
    {
        Logger.LogInformation($"[子-{DateTime.Now:HH:mm:ss.fff}] Dispose()");
    }
}
