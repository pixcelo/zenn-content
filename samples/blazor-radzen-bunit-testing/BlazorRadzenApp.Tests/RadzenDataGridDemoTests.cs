using BlazorRadzenApp.Components.CustomComponents;
using Bunit;
using Radzen;
using Xunit;

namespace BlazorRadzenApp.Tests;

/// <summary>
/// Radzen DataGridコンポーネントのテスト
/// </summary>
public class RadzenDataGridDemoTests : TestContext
{
    public RadzenDataGridDemoTests()
    {
        // Radzenサービスの登録
        Services.AddRadzenComponents();
    }

    [Fact]
    public void RadzenDataGridDemo_初期表示_タイトルが表示される()
    {
        // Arrange & Act
        var cut = RenderComponent<RadzenDataGridDemo>();

        // Assert
        cut.Find("h3").MarkupMatches("<h3>Radzen DataGrid デモ</h3>");
    }

    [Fact]
    public void RadzenDataGridDemo_初期表示_DataGridがレンダリングされる()
    {
        // Arrange & Act
        var cut = RenderComponent<RadzenDataGridDemo>();

        // Assert - RadzenDataGridのコンテナが存在する
        var dataGrid = cut.Find(".rz-datatable");
        Assert.NotNull(dataGrid);
    }

    [Fact]
    public void RadzenDataGridDemo_初期表示_商品データが表示される()
    {
        // Arrange & Act
        var cut = RenderComponent<RadzenDataGridDemo>();

        // Assert - テーブル内に商品名が含まれている
        var markup = cut.Markup;
        Assert.Contains("ノートPC", markup);
        Assert.Contains("マウス", markup);
        Assert.Contains("キーボード", markup);
    }

    [Fact]
    public void RadzenDataGridDemo_データ表示_価格が正しくフォーマットされている()
    {
        // Arrange & Act
        var cut = RenderComponent<RadzenDataGridDemo>();

        // Assert - 価格表示のフォーマット確認
        var markup = cut.Markup;
        Assert.Contains("¥120,000", markup);
        Assert.Contains("¥2,500", markup);
    }

    [Fact]
    public void RadzenDataGridDemo_データ表示_在庫状態が日本語で表示される()
    {
        // Arrange & Act
        var cut = RenderComponent<RadzenDataGridDemo>();

        // Assert
        var markup = cut.Markup;
        Assert.Contains("あり", markup);
        Assert.Contains("なし", markup);
    }

    [Fact]
    public void RadzenDataGridDemo_初期表示_7件の商品が登録されている()
    {
        // Arrange & Act
        var cut = RenderComponent<RadzenDataGridDemo>();

        // Assert - すべての商品名が表示されている
        var markup = cut.Markup;
        Assert.Contains("ノートPC", markup);
        Assert.Contains("マウス", markup);
        Assert.Contains("キーボード", markup);
        Assert.Contains("モニター", markup);
        Assert.Contains("Webカメラ", markup);
        Assert.Contains("スピーカー", markup);
        Assert.Contains("ヘッドセット", markup);
    }

    [Fact]
    public void RadzenDataGridDemo_カラムヘッダー_正しいタイトルが表示される()
    {
        // Arrange & Act
        var cut = RenderComponent<RadzenDataGridDemo>();

        // Assert
        var markup = cut.Markup;
        Assert.Contains("ID", markup);
        Assert.Contains("商品名", markup);
        Assert.Contains("価格", markup);
        Assert.Contains("在庫", markup);
    }
}
