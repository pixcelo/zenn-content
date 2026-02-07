using Microsoft.AspNetCore.Components;
using Radzen.Blazor;

namespace BlazorRadzenPlaywrightSample.Components.Pages;

/// <summary>
/// Radzen DataGrid コンポーネントのデモページ
/// </summary>
public partial class DataGridDemo : ComponentBase
{
    /// <summary>
    /// DataGrid の参照
    /// </summary>
    private RadzenDataGrid<GridItem>? _grid;

    /// <summary>
    /// 動的に表示するアイテムのリスト
    /// </summary>
    private List<GridItem> Items { get; set; } = new();

    /// <summary>
    /// 新規アイテムのID（自動採番）
    /// </summary>
    private int _nextId = 1;

    protected override void OnInitialized()
    {
        // 初期データを追加
        Items.Add(new GridItem { Id = _nextId++, Name = "サンプル商品1", Price = 1000, Quantity = 10 });
        Items.Add(new GridItem { Id = _nextId++, Name = "サンプル商品2", Price = 2000, Quantity = 5 });
        Items.Add(new GridItem { Id = _nextId++, Name = "サンプル商品3", Price = 1500, Quantity = 8 });
    }

    /// <summary>
    /// 新しい行を追加
    /// </summary>
    private async Task AddRow()
    {
        Items.Add(new GridItem
        {
            Id = _nextId++,
            Name = $"新規商品{_nextId - 1}",
            Price = 0,
            Quantity = 0
        });

        // DataGrid を再読み込み
        if (_grid != null)
        {
            await _grid.Reload();
        }
    }

    /// <summary>
    /// 行を削除
    /// </summary>
    private async Task DeleteRow(GridItem item)
    {
        Items.Remove(item);

        // DataGrid を再読み込み
        if (_grid != null)
        {
            await _grid.Reload();
        }
    }

    /// <summary>
    /// DataGrid に表示するアイテムのモデル
    /// </summary>
    public class GridItem
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Quantity { get; set; }

        /// <summary>
        /// 合計金額（計算プロパティ）
        /// </summary>
        public decimal Total => Price * Quantity;
    }
}
