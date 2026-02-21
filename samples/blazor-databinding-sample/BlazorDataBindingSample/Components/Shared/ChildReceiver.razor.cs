using Microsoft.AspNetCore.Components;

namespace BlazorDataBindingSample.Components.Shared;

/// <summary>
/// 子レシーバーコンポーネント（孫コンポーネント）
/// </summary>
public partial class ChildReceiver : ComponentBase
{
    /// <summary>
    /// カスケードされたユーザー名
    /// </summary>
    [CascadingParameter(Name = "UserName")]
    public string UserName { get; set; } = "";

    /// <summary>
    /// カスケードされたテーマ
    /// </summary>
    [CascadingParameter(Name = "Theme")]
    public string Theme { get; set; } = "";
}
