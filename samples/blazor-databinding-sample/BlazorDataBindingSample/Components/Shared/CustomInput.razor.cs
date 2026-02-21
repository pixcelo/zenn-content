using Microsoft.AspNetCore.Components;

namespace BlazorDataBindingSample.Components.Shared;

/// <summary>
/// カスタム入力コンポーネント：双方向バインディング対応
/// </summary>
public partial class CustomInput : ComponentBase
{
    /// <summary>
    /// ラベルテキスト
    /// </summary>
    [Parameter]
    public string Label { get; set; } = "";

    /// <summary>
    /// 入力値（双方向バインディング）
    /// </summary>
    [Parameter]
    public string Value { get; set; } = "";

    /// <summary>
    /// 値変更時のコールバック（双方向バインディング用）
    /// </summary>
    [Parameter]
    public EventCallback<string> ValueChanged { get; set; }

    /// <summary>
    /// プレースホルダーテキスト
    /// </summary>
    [Parameter]
    public string Placeholder { get; set; } = "";

    /// <summary>
    /// input要素のtype属性
    /// </summary>
    [Parameter]
    public string InputType { get; set; } = "text";

    /// <summary>
    /// 入力フィールドの前に表示するテキスト
    /// </summary>
    [Parameter]
    public string Prefix { get; set; } = "";

    /// <summary>
    /// 入力フィールドの後ろに表示するテキスト
    /// </summary>
    [Parameter]
    public string Suffix { get; set; } = "";

    /// <summary>
    /// ヘルプテキスト
    /// </summary>
    [Parameter]
    public string HelpText { get; set; } = "";

    /// <summary>
    /// バリデーションメッセージ
    /// </summary>
    [Parameter]
    public string ValidationMessage { get; set; } = "";

    /// <summary>
    /// バリデーション状態
    /// </summary>
    [Parameter]
    public bool IsValid { get; set; } = true;

    private async Task OnInputChanged(ChangeEventArgs e)
    {
        Value = e.Value?.ToString() ?? "";
        await ValueChanged.InvokeAsync(Value);
    }
}
