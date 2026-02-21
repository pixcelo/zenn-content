using Microsoft.AspNetCore.Components;

namespace BlazorDataBindingSample.Components.Pages;

/// <summary>
/// 明示的な双方向バインディングのサンプルページ
/// </summary>
public partial class ExplicitTwoWayBinding : ComponentBase
{
    // 基本的な例
    private string name = "Alice";

    // バリデーション付き
    private string email = "";
    private string emailError = "";

    // ログ出力付き
    private string message = "";
    private int changeCount = 0;

    // 数値入力（条件付き更新）
    private decimal price = 0;
    private string priceError = "";

    /// <summary>
    /// 名前変更イベント
    /// </summary>
    private void OnNameChanged(ChangeEventArgs e)
    {
        var newValue = e.Value?.ToString() ?? "";

        // バリデーション：空白チェック
        if (string.IsNullOrWhiteSpace(newValue))
        {
            return;
        }

        name = newValue;
    }

    /// <summary>
    /// メールアドレス変更イベント
    /// </summary>
    private void OnEmailChanged(ChangeEventArgs e)
    {
        var newValue = e.Value?.ToString() ?? "";

        // バリデーション：文字数チェック
        if (newValue.Length < 5)
        {
            emailError = "5文字以上で入力してください";
            email = newValue; // エラーでも値は更新
            return;
        }

        emailError = "";
        email = newValue;
    }

    /// <summary>
    /// メッセージ変更イベント（ログ出力付き）
    /// </summary>
    private void OnMessageChanged(ChangeEventArgs e)
    {
        var newValue = e.Value?.ToString() ?? "";

        // ログ出力
        Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] メッセージ変更: '{message}' → '{newValue}'");

        message = newValue;
        changeCount++;
    }

    /// <summary>
    /// 価格変更イベント（条件付き更新）
    /// </summary>
    private void OnPriceChanged(ChangeEventArgs e)
    {
        var newValue = e.Value?.ToString() ?? "0";

        if (!decimal.TryParse(newValue, out var parsedPrice))
        {
            priceError = "数値を入力してください";
            return;
        }

        // 条件付き更新：範囲チェック
        if (parsedPrice < 0 || parsedPrice > 10000)
        {
            priceError = "0〜10000の範囲で入力してください";
            return;
        }

        priceError = "";
        price = parsedPrice;

        // カスタム処理：価格変更のログ
        Console.WriteLine($"価格更新: {price:C}");
    }
}
