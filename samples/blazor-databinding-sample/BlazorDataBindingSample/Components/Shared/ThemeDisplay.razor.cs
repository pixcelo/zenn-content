using Microsoft.AspNetCore.Components;

namespace BlazorDataBindingSample.Components.Shared;

/// <summary>
/// テーマ表示コンポーネント
/// </summary>
public partial class ThemeDisplay : ComponentBase
{
    /// <summary>
    /// カスケードされたテーマ
    /// </summary>
    [CascadingParameter]
    public string Theme { get; set; } = "(未カスケード)";

    /// <summary>
    /// アラートクラスを取得
    /// </summary>
    private string GetAlertClass()
    {
        return Theme == "dark" ? "alert-dark" : "alert-light";
    }
}
