using Microsoft.AspNetCore.Components;

namespace BlazorDataBindingSample.Components.Shared;

/// <summary>
/// 挨拶メッセージコンポーネント
/// </summary>
public partial class GreetingMessage : ComponentBase
{
    /// <summary>
    /// ユーザー名
    /// </summary>
    [Parameter]
    public string UserName { get; set; } = "";

    /// <summary>
    /// 時間帯（Morning, Afternoon, Evening）
    /// </summary>
    [Parameter]
    public string TimeOfDay { get; set; } = "Morning";

    /// <summary>
    /// 挨拶メッセージを取得
    /// </summary>
    private string GetGreeting()
    {
        var greeting = TimeOfDay switch
        {
            "Morning" => "おはようございます",
            "Afternoon" => "こんにちは",
            "Evening" => "こんばんは",
            _ => "こんにちは"
        };

        return string.IsNullOrEmpty(UserName)
            ? $"{greeting}！"
            : $"{greeting}、{UserName}さん！";
    }

    /// <summary>
    /// アラートクラスを取得
    /// </summary>
    private string GetAlertClass()
    {
        return TimeOfDay switch
        {
            "Morning" => "alert-info",
            "Afternoon" => "alert-success",
            "Evening" => "alert-warning",
            _ => "alert-primary"
        };
    }
}
