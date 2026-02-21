using Microsoft.AspNetCore.Components;

namespace BlazorDataBindingSample.Components.Shared;

/// <summary>
/// 値型プロパティのテスト用子コンポーネント
/// </summary>
public partial class ValueTypeTestComponent : ComponentBase
{
    /// <summary>
    /// 親から受け取る値型の値
    /// </summary>
    [Parameter]
    public int Value { get; set; }

    /// <summary>
    /// 値変更時のコールバック
    /// </summary>
    [Parameter]
    public EventCallback<int> ValueChanged { get; set; }

    private int localValue;

    protected override void OnParametersSet()
    {
        localValue = Value;
    }

    private async Task UpdateValue()
    {
        await ValueChanged.InvokeAsync(localValue);
    }
}
