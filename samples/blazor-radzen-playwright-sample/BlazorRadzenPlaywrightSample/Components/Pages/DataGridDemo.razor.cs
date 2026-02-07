using Microsoft.AspNetCore.Components;
using Radzen.Blazor;

namespace BlazorRadzenPlaywrightSample.Components.Pages;

/// <summary>
/// 商品の状態
/// </summary>
public enum ItemStatus
{
    /// <summary>
    /// 未着手
    /// </summary>
    NotStarted = 0,

    /// <summary>
    /// 進行中
    /// </summary>
    InProgress = 1,

    /// <summary>
    /// 完了
    /// </summary>
    Completed = 2
}

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

    /// <summary>
    /// 選択された行のリスト
    /// </summary>
    private List<GridItem> SelectedItems { get; set; } = new();

    /// <summary>
    /// 全選択状態
    /// </summary>
    private bool _selectAll = false;

    /// <summary>
    /// 選択された行の情報
    /// </summary>
    private string SelectedInfo
    {
        get
        {
            if (!SelectedItems.Any())
            {
                return "行が選択されていません";
            }

            if (SelectedItems.Count == 1)
            {
                var item = SelectedItems.First();
                return $"選択: {item.Name} (ID: {item.Id}, 合計: {item.Total:N0}円)";
            }

            return $"{SelectedItems.Count}行が選択されています";
        }
    }

    protected override void OnInitialized()
    {
        // 初期データを追加
        Items.Add(new GridItem
        {
            Id = _nextId++,
            Name = "サンプル商品1",
            Price = 1000,
            Quantity = 10,
            Status = ItemStatus.NotStarted  // 未着手
        });

        Items.Add(new GridItem
        {
            Id = _nextId++,
            Name = "サンプル商品2",
            Price = 2000,
            Quantity = 5,
            Status = ItemStatus.InProgress  // 進行中
        });

        Items.Add(new GridItem
        {
            Id = _nextId++,
            Name = "サンプル商品3",
            Price = 1500,
            Quantity = 8,
            Status = ItemStatus.Completed  // 完了
        });
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
            Quantity = 0,
            Status = ItemStatus.NotStarted  // デフォルトは未着手
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
    /// 行の選択/選択解除をトグル
    /// </summary>
    private void ToggleSelection(GridItem item)
    {
        if (SelectedItems.Contains(item))
        {
            SelectedItems.Remove(item);
        }
        else
        {
            SelectedItems.Add(item);
        }

        // 全選択状態を更新
        _selectAll = Items.Count > 0 && SelectedItems.Count == Items.Count;
    }

    /// <summary>
    /// 全選択/全解除
    /// </summary>
    private void OnSelectAllChange(bool value)
    {
        if (value)
        {
            // 全選択
            SelectedItems = Items.ToList();
        }
        else
        {
            // 全解除
            SelectedItems.Clear();
        }

        _selectAll = value;
    }

    /// <summary>
    /// 行クリック時のイベントハンドラ
    /// </summary>
    private void OnRowClick(GridItem item)
    {
        // 行クリックで選択をトグル
        ToggleSelection(item);
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
        /// 商品の状態
        /// </summary>
        public ItemStatus Status { get; set; } = ItemStatus.NotStarted;

        /// <summary>
        /// 合計金額（計算プロパティ）
        /// </summary>
        public decimal Total => Price * Quantity;

        /// <summary>
        /// 状態の表示テキスト
        /// </summary>
        public string StatusText => Status switch
        {
            ItemStatus.NotStarted => "未着手",
            ItemStatus.InProgress => "進行中",
            ItemStatus.Completed => "完了",
            _ => "不明"
        };
    }
}
