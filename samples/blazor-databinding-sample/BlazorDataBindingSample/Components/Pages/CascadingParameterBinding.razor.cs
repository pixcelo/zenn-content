using Microsoft.AspNetCore.Components;

namespace BlazorDataBindingSample.Components.Pages;

/// <summary>
/// カスケード型パラメーターのサンプルページ
/// </summary>
public partial class CascadingParameterBinding : ComponentBase
{
    // 基本的なカスケード
    private string currentTheme = "light";

    // 階層を越えたカスケード
    private string currentUserName = "Alice";

    // 複雑なオブジェクト
    private AppSettings appSettings = new()
    {
        ApplicationName = "Blazor Sample App",
        Version = "1.0.0"
    };

    /// <summary>
    /// アプリケーション設定クラス
    /// </summary>
    public class AppSettings
    {
        public string ApplicationName { get; set; } = "";
        public string Version { get; set; } = "";
    }
}
