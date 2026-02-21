using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace BlazorDataBindingSample.Components.Pages;

/// <summary>
/// プリレンダリング検証のデモページ
/// </summary>
public partial class PrerenderingTest : ComponentBase
{
    [Inject]
    private IJSRuntime JSRuntime { get; set; } = default!;

    private List<string> lifecycleLogs = new();
    private string jsResult = "";
    private string jsError = "";
    private int count = 0;
    private string lastUpdate = "";
    private string realtimeValue = "";
    private string onChangeValue = "";
    private int afterRenderCount = 0;
    private string connectionState = "不明";
    private bool isFirstRenderComplete = false;

    protected override void OnInitialized()
    {
        lifecycleLogs.Add($"OnInitialized() - {DateTime.Now:HH:mm:ss.fff}");
        base.OnInitialized();
    }

    protected override async Task OnInitializedAsync()
    {
        lifecycleLogs.Add($"OnInitializedAsync() 開始 - {DateTime.Now:HH:mm:ss.fff}");
        await Task.Delay(10); // 非同期処理をシミュレート
        lifecycleLogs.Add($"OnInitializedAsync() 終了 - {DateTime.Now:HH:mm:ss.fff}");
    }

    protected override void OnParametersSet()
    {
        lifecycleLogs.Add($"OnParametersSet() - {DateTime.Now:HH:mm:ss.fff}");
        base.OnParametersSet();
    }

    protected override async Task OnParametersSetAsync()
    {
        lifecycleLogs.Add($"OnParametersSetAsync() 開始 - {DateTime.Now:HH:mm:ss.fff}");
        await Task.Delay(10);
        lifecycleLogs.Add($"OnParametersSetAsync() 終了 - {DateTime.Now:HH:mm:ss.fff}");
    }

    protected override void OnAfterRender(bool firstRender)
    {
        afterRenderCount++;
        lifecycleLogs.Add($"OnAfterRender(firstRender: {firstRender}) - {DateTime.Now:HH:mm:ss.fff}");

        if (firstRender)
        {
            isFirstRenderComplete = true;
            connectionState = "インタラクティブ";
            StateHasChanged(); // 状態を更新
        }

        base.OnAfterRender(firstRender);
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        lifecycleLogs.Add($"OnAfterRenderAsync(firstRender: {firstRender}) 開始 - {DateTime.Now:HH:mm:ss.fff}");

        if (firstRender)
        {
            // JavaScript が使えるのは OnAfterRender の firstRender 以降のみ
            try
            {
                await JSRuntime.InvokeVoidAsync("console.log", "Blazor is now interactive!");
                lifecycleLogs.Add($"JavaScript実行成功 - {DateTime.Now:HH:mm:ss.fff}");
            }
            catch (Exception ex)
            {
                lifecycleLogs.Add($"JavaScript実行失敗: {ex.Message}");
            }
        }

        await Task.Delay(10);
        lifecycleLogs.Add($"OnAfterRenderAsync(firstRender: {firstRender}) 終了 - {DateTime.Now:HH:mm:ss.fff}");
    }

    private void AddLog()
    {
        lifecycleLogs.Add($"ボタンがクリックされました - {DateTime.Now:HH:mm:ss.fff}");
    }

    private async Task CallJavaScript()
    {
        jsResult = "";
        jsError = "";

        try
        {
            var userAgent = await JSRuntime.InvokeAsync<string>("eval", "navigator.userAgent");
            jsResult = userAgent;
        }
        catch (Exception ex)
        {
            jsError = ex.Message;
        }
    }

    private void IncrementCount()
    {
        count++;
        lastUpdate = DateTime.Now.ToString("HH:mm:ss.fff");
    }

    private async Task IncrementWithDelay()
    {
        await Task.Delay(1000);
        count++;
        lastUpdate = DateTime.Now.ToString("HH:mm:ss.fff");
    }
}
