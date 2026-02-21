using Microsoft.AspNetCore.Components;

namespace BlazorDataBindingSample.Components.Pages;

/// <summary>
/// コンポーネント間バインディングのデモページ
/// </summary>
public partial class ComponentBinding : ComponentBase
{
    // 親から子へのデータ受け渡し
    private string parentMessage = "親からのメッセージ";
    private int childCount = 0;

    // 双方向バインディング
    private string parentControlledValue = "";

    // カスタム入力コンポーネント
    private string username = "";
    private string email = "";
    private string priceText = "";
    private string usernameValidation = "";
    private string emailValidation = "";
    private bool isUsernameValid = true;
    private bool isEmailValid = true;

    // 複数の子コンポーネント
    private int count1 = 0;
    private int count2 = 0;

    // 通知リスト
    private List<string> notifications = new();

    private void OnChildCountChanged(int newCount)
    {
        childCount = newCount;
        notifications.Add($"[{DateTime.Now:HH:mm:ss}] カウントが {newCount} に変更されました");
    }

    private void HandleChildNotification(string message)
    {
        notifications.Add($"[{DateTime.Now:HH:mm:ss}] {message}");
    }

    private void ValidateInputs()
    {
        // ユーザー名の検証
        isUsernameValid = !string.IsNullOrWhiteSpace(username) && username.Length >= 3;
        usernameValidation = isUsernameValid ? "" : "ユーザー名は3文字以上で入力してください";

        // メールアドレスの検証（簡易版）
        isEmailValid = !string.IsNullOrWhiteSpace(email) && email.Contains("@");
        emailValidation = isEmailValid ? "" : "有効なメールアドレスを入力してください";

        notifications.Add($"[{DateTime.Now:HH:mm:ss}] 検証実行: ユーザー名={isUsernameValid}, メール={isEmailValid}");
    }

    private void ResetAllCounts()
    {
        count1 = 0;
        count2 = 0;
        notifications.Add($"[{DateTime.Now:HH:mm:ss}] すべてのカウントをリセットしました");
    }
}
