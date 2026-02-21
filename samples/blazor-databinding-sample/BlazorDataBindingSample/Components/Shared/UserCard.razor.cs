using Microsoft.AspNetCore.Components;

namespace BlazorDataBindingSample.Components.Shared;

/// <summary>
/// ユーザーカードコンポーネント
/// </summary>
public partial class UserCard : ComponentBase
{
    /// <summary>
    /// 名前
    /// </summary>
    [Parameter]
    public string Name { get; set; } = "";

    /// <summary>
    /// 年齢
    /// </summary>
    [Parameter]
    public int Age { get; set; }

    /// <summary>
    /// メールアドレス（オプション）
    /// </summary>
    [Parameter]
    public string? Email { get; set; }
}
