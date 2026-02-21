using Microsoft.AspNetCore.Components;

namespace BlazorDataBindingSample.Components.Shared;

/// <summary>
/// 子コンポーネント：親からパラメータを受け取り、イベントコールバックで親に通知
/// </summary>
public partial class ChildComponent : ComponentBase
{
    /// <summary>
    /// 親コンポーネントから受け取る値
    /// </summary>
    [Parameter]
    public string ParentValue { get; set; } = "";

    /// <summary>
    /// カウント値
    /// </summary>
    [Parameter]
    public int Count { get; set; }

    /// <summary>
    /// カウント変更時のコールバック
    /// </summary>
    [Parameter]
    public EventCallback<int> CountChanged { get; set; }

    /// <summary>
    /// 親への通知用コールバック
    /// </summary>
    [Parameter]
    public EventCallback<string> OnNotify { get; set; }

    /// <summary>
    /// 背景色のCSSクラス
    /// </summary>
    [Parameter]
    public string BackgroundClass { get; set; } = "bg-light";

    private string localValue = "";

    private async Task IncrementCount()
    {
        Count++;
        // 双方向バインディングのために CountChanged を呼び出す
        await CountChanged.InvokeAsync(Count);
    }

    private async Task NotifyParent()
    {
        await OnNotify.InvokeAsync($"子コンポーネントから通知: {localValue}");
    }
}
