using Microsoft.AspNetCore.Components;
using static BlazorDataBindingSample.Components.Pages.CascadingParameterBinding;

namespace BlazorDataBindingSample.Components.Shared;

/// <summary>
/// アプリ情報表示コンポーネント
/// </summary>
public partial class AppInfoDisplay : ComponentBase
{
    /// <summary>
    /// カスケードされたアプリケーション設定
    /// </summary>
    [CascadingParameter]
    public AppSettings? AppSettings { get; set; }
}
