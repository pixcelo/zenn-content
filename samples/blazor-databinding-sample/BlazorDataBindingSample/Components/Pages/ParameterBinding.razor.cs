using Microsoft.AspNetCore.Components;

namespace BlazorDataBindingSample.Components.Pages;

/// <summary>
/// パラメーターのサンプルページ
/// </summary>
public partial class ParameterBinding : ComponentBase
{
    // 基本的なパラメーター
    private string userName = "Alice";
    private int userAge = 25;

    // 複数のパラメーター
    private string userEmail = "alice@example.com";

    // 条件付きレンダリング
    private string timeOfDay = "Morning";

    // カスタムクラス
    private UserInfo userInfo = new()
    {
        Name = "Bob",
        Age = 30,
        Email = "bob@example.com"
    };

    /// <summary>
    /// ユーザー情報クラス
    /// </summary>
    public class UserInfo
    {
        public string Name { get; set; } = "";
        public int Age { get; set; }
        public string Email { get; set; } = "";
    }
}
