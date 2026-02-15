using Microsoft.AspNetCore.Components;

namespace BlazorLifecycleDemo.Components.Pages.Async;

public class ChildAsyncComponentBase : ComponentBase, IAsyncDisposable
{
    [Inject] protected ILogger<ChildAsyncComponentBase> Logger { get; set; } = default!;

    /// <summary>
    /// 親コンポーネントから渡されるメッセージ。
    /// [Parameter] 属性により、親からの値がSetParametersAsyncで自動的に設定される。
    /// この属性がない場合、親から値を渡しても設定されない。
    /// </summary>
    [Parameter] public string Message { get; set; } = string.Empty;

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();

        Logger.LogInformation($"[子-{DateTime.Now:HH:mm:ss.fff}] OnInitializedAsync() 開始");

        await Task.Delay(50); // 非同期処理をシミュレート

        Logger.LogInformation($"[子-{DateTime.Now:HH:mm:ss.fff}] OnInitializedAsync() 完了");
    }

    protected override async Task OnParametersSetAsync()
    {
        await base.OnParametersSetAsync();

        Logger.LogInformation($"[子-{DateTime.Now:HH:mm:ss.fff}] OnParametersSetAsync() 開始 - Message={Message}");

        await Task.Delay(50); // 非同期処理をシミュレート

        Logger.LogInformation($"[子-{DateTime.Now:HH:mm:ss.fff}] OnParametersSetAsync() 完了");
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);

        Logger.LogInformation($"[子-{DateTime.Now:HH:mm:ss.fff}] OnAfterRenderAsync(firstRender={firstRender}) 開始");

        await Task.Delay(50); // 非同期処理をシミュレート

        Logger.LogInformation($"[子-{DateTime.Now:HH:mm:ss.fff}] OnAfterRenderAsync(firstRender={firstRender}) 完了");
    }

    public async ValueTask DisposeAsync()
    {
        Logger.LogInformation($"[子-{DateTime.Now:HH:mm:ss.fff}] DisposeAsync()");
        await Task.CompletedTask;
    }
}
