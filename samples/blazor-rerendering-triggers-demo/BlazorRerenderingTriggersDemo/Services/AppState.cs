namespace BlazorRerenderingTriggersDemo.Services;

/// <summary>
/// StateContainerパターンの実装
/// 記事内「手動でStateHasChangedが必要なケース > 3. DIサービスからの通知」のサンプル
/// </summary>
public class AppState
{
    private string _currentUser = "ゲスト";

    /// <summary>
    /// 現在のユーザー名
    /// 値が変更されるとOnChangeイベントが発火される
    /// </summary>
    public string CurrentUser
    {
        get => _currentUser;
        set
        {
            _currentUser = value;
            NotifyStateChanged();
        }
    }

    /// <summary>
    /// 状態変更通知イベント
    /// コンポーネントはこのイベントを購読してStateHasChanged()を呼ぶ
    /// </summary>
    public event Action? OnChange;

    private void NotifyStateChanged() => OnChange?.Invoke();
}
