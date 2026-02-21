using Microsoft.AspNetCore.Components;
using BlazorDataBindingSample.Models;

namespace BlazorDataBindingSample.Components.Shared;

/// <summary>
/// 参照型プロパティのテスト用子コンポーネント
/// </summary>
public partial class ReferenceTypeTestComponent : ComponentBase
{
    /// <summary>
    /// 親から受け取る参照型のオブジェクト
    /// </summary>
    [Parameter]
    public ReferenceTypeModel? Model { get; set; }

    /// <summary>
    /// オブジェクト変更時のコールバック
    /// </summary>
    [Parameter]
    public EventCallback<ReferenceTypeModel?> ModelChanged { get; set; }

    private void ReplaceObject()
    {
        // オブジェクト自体を新しいインスタンスに置き換え
        Model = new ReferenceTypeModel
        {
            Id = Model?.Id + 100 ?? 100,
            Name = "新しいオブジェクト",
            Value = 999.99m
        };
    }

    private async Task NotifyParent()
    {
        await ModelChanged.InvokeAsync(Model);
    }
}
