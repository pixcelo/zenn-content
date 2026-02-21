using BlazorDataBindingSample.Models;
using Microsoft.AspNetCore.Components;

namespace BlazorDataBindingSample.Components.Pages;

/// <summary>
/// 値型vs参照型のバインディング実験ページのコードビハインド
/// </summary>
public partial class ValueTypeVsReferenceType : ComponentBase
{
    // 値型の例
    private int valueTypeInt = 10;

    // 参照型の例
    private ReferenceTypeModel? referenceTypeModel;

    // 複雑な例
    private NestedModel? nestedModel;

    // ログ
    private List<(string Time, string Message)> logs = new();

    protected override void OnInitialized()
    {
        referenceTypeModel = new ReferenceTypeModel(1, "初期オブジェクト", 100.50m);
        nestedModel = new NestedModel
        {
            Counter = 0,
            Item = new ReferenceTypeModel(1, "ネストされたアイテム", 50.0m)
        };

        AddLog("コンポーネント初期化完了");
    }

    private void ResetReferenceModel()
    {
        var oldHash = referenceTypeModel?.GetHashCode().ToString("X8") ?? "null";
        referenceTypeModel = new ReferenceTypeModel(1, "リセットされたオブジェクト", 100.50m);
        var newHash = referenceTypeModel.GetHashCode().ToString("X8");

        AddLog($"参照型モデルをリセット: {oldHash} → {newHash}");
    }

    private void IncrementNested()
    {
        if (nestedModel != null)
        {
            nestedModel.Counter++;
            AddLog($"Counter を {nestedModel.Counter} にインクリメント");
        }
    }

    private void ChangeNestedItemName()
    {
        if (nestedModel?.Item != null)
        {
            nestedModel.Item.Name = $"変更_{DateTime.Now:HHmmss}";
            AddLog($"Item.Name を変更: {nestedModel.Item.Name}");
        }
    }

    private void AddLog(string message)
    {
        logs.Add((DateTime.Now.ToString("HH:mm:ss.fff"), message));
    }

    private void ClearLogs()
    {
        logs.Clear();
    }
}
