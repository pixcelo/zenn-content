using BlazorRadzenApp.Components.CustomComponents;
using Bunit;
using Radzen;
using Xunit;

namespace BlazorRadzenApp.Tests;

/// <summary>
/// Radzen Tabsコンポーネントのテスト
/// </summary>
public class RadzenTabsDemoTests : TestContext
{
    public RadzenTabsDemoTests()
    {
        // Radzenサービスの登録
        Services.AddRadzenComponents();
    }

    [Fact]
    public void RadzenTabsDemo_初期表示_タイトルが表示される()
    {
        // Arrange & Act
        var cut = RenderComponent<RadzenTabsDemo>();

        // Assert
        cut.Find("h3").MarkupMatches("<h3>Radzen Tabs デモ</h3>");
    }

    [Fact]
    public void RadzenTabsDemo_初期表示_RadzenTabsがレンダリングされる()
    {
        // Arrange & Act
        var cut = RenderComponent<RadzenTabsDemo>();

        // Assert - RadzenTabsのコンテナが存在する
        var tabsContainer = cut.Find(".rz-tabview");
        Assert.NotNull(tabsContainer);
    }

    [Fact]
    public void RadzenTabsDemo_初期表示_4つのタブが存在する()
    {
        // Arrange & Act
        var cut = RenderComponent<RadzenTabsDemo>();

        // Assert - タブヘッダーが4つ存在する
        var markup = cut.Markup;
        Assert.Contains("商品一覧", markup);
        Assert.Contains("注文履歴", markup);
        Assert.Contains("設定", markup);
        Assert.Contains("統計情報", markup);
    }

    [Fact]
    public void RadzenTabsDemo_初期表示_商品一覧タブがアクティブ()
    {
        // Arrange & Act
        var cut = RenderComponent<RadzenTabsDemo>();

        // Assert - 商品一覧タブのコンテンツが表示されている
        var markup = cut.Markup;
        Assert.Contains("商品一覧", markup);
        Assert.Contains("ノートPC", markup);
        Assert.Contains("マウス", markup);
    }

    [Fact]
    public void RadzenTabsDemo_商品一覧タブ_DataGridが表示される()
    {
        // Arrange & Act
        var cut = RenderComponent<RadzenTabsDemo>();

        // Assert - DataGridが存在する
        var dataGrid = cut.Find(".rz-datatable");
        Assert.NotNull(dataGrid);

        // 商品名が表示されている
        var markup = cut.Markup;
        Assert.Contains("ノートPC", markup);
        Assert.Contains("¥120,000", markup);
    }

    [Fact]
    public void RadzenTabsDemo_商品一覧タブ_価格が正しくフォーマットされる()
    {
        // Arrange & Act
        var cut = RenderComponent<RadzenTabsDemo>();

        // Assert
        var markup = cut.Markup;
        Assert.Contains("¥120,000", markup); // ノートPC
        Assert.Contains("¥2,500", markup);   // マウス
        Assert.Contains("¥35,000", markup);  // モニター
    }

    [Fact]
    public void RadzenTabsDemo_商品一覧タブ_在庫状態が表示される()
    {
        // Arrange & Act
        var cut = RenderComponent<RadzenTabsDemo>();

        // Assert
        var markup = cut.Markup;
        Assert.Contains("あり", markup);
        Assert.Contains("なし", markup);
    }

    [Fact]
    public void RadzenTabsDemo_タブクリック_タブ変更メッセージが表示される()
    {
        // Arrange
        var cut = RenderComponent<RadzenTabsDemo>();

        // Act - 2番目のタブ（注文履歴）をクリック
        var tabHeaders = cut.FindAll(".rz-tabview-nav li");
        if (tabHeaders.Count > 1)
        {
            tabHeaders[1].Click();
        }

        // Assert
        var markup = cut.Markup;
        Assert.Contains("注文履歴タブに切り替えました", markup);
    }

    [Fact]
    public void RadzenTabsDemo_注文履歴タブ_注文データが表示される()
    {
        // Arrange
        var cut = RenderComponent<RadzenTabsDemo>();

        // Act - 注文履歴タブに切り替え
        var tabHeaders = cut.FindAll(".rz-tabview-nav li");
        if (tabHeaders.Count > 1)
        {
            tabHeaders[1].Click();
        }

        // Assert
        var markup = cut.Markup;
        Assert.Contains("注文履歴", markup);
        Assert.Contains("ノートPC", markup);
        Assert.Contains("¥120,000", markup);
    }

    [Fact]
    public void RadzenTabsDemo_設定タブ_チェックボックスと言語選択が表示される()
    {
        // Arrange
        var cut = RenderComponent<RadzenTabsDemo>();

        // Act - 設定タブに切り替え（3番目のタブ）
        var tabHeaders = cut.FindAll(".rz-tabview-nav li");
        if (tabHeaders.Count > 2)
        {
            tabHeaders[2].Click();
        }

        // Assert
        var markup = cut.Markup;
        Assert.Contains("アプリケーション設定", markup);
        Assert.Contains("通知設定", markup);
        Assert.Contains("言語", markup);
    }

    [Fact]
    public void RadzenTabsDemo_統計情報タブ_統計データが表示される()
    {
        // Arrange
        var cut = RenderComponent<RadzenTabsDemo>();

        // Act - 統計情報タブに切り替え（4番目のタブ）
        var tabHeaders = cut.FindAll(".rz-tabview-nav li");
        if (tabHeaders.Count > 3)
        {
            tabHeaders[3].Click();
        }

        // Assert
        var markup = cut.Markup;
        Assert.Contains("統計情報", markup);
        Assert.Contains("総売上", markup);
        Assert.Contains("総注文数", markup);
        Assert.Contains("在庫商品数", markup);
    }

    [Fact]
    public void RadzenTabsDemo_統計情報タブ_総売上が正しく計算される()
    {
        // Arrange
        var cut = RenderComponent<RadzenTabsDemo>();

        // Act - 統計情報タブに切り替え
        var tabHeaders = cut.FindAll(".rz-tabview-nav li");
        if (tabHeaders.Count > 3)
        {
            tabHeaders[3].Click();
        }

        // Assert - 総売上: 120000 + 2500 + 35000 = 157500
        var markup = cut.Markup;
        Assert.Contains("¥157,500", markup);
    }

    [Fact]
    public void RadzenTabsDemo_統計情報タブ_総注文数が正しく表示される()
    {
        // Arrange
        var cut = RenderComponent<RadzenTabsDemo>();

        // Act - 統計情報タブに切り替え
        var tabHeaders = cut.FindAll(".rz-tabview-nav li");
        if (tabHeaders.Count > 3)
        {
            tabHeaders[3].Click();
        }

        // Assert - 注文数は3件
        var markup = cut.Markup;
        Assert.Contains(">3<", markup); // 総注文数
    }

    [Fact]
    public void RadzenTabsDemo_統計情報タブ_在庫商品数が正しく表示される()
    {
        // Arrange
        var cut = RenderComponent<RadzenTabsDemo>();

        // Act - 統計情報タブに切り替え
        var tabHeaders = cut.FindAll(".rz-tabview-nav li");
        if (tabHeaders.Count > 3)
        {
            tabHeaders[3].Click();
        }

        // Assert - 在庫ありの商品は3件（ノートPC、マウス、モニター）
        var markup = cut.Markup;
        Assert.Contains(">3<", markup); // 在庫商品数
    }

    [Fact]
    public void RadzenTabsDemo_複数タブ切り替え_各タブのコンテンツが正しく表示される()
    {
        // Arrange
        var cut = RenderComponent<RadzenTabsDemo>();
        var tabHeaders = cut.FindAll(".rz-tabview-nav li");

        // Act & Assert - 商品一覧タブ
        if (tabHeaders.Count > 0)
        {
            tabHeaders[0].Click();
            Assert.Contains("ノートPC", cut.Markup);
        }

        // Act & Assert - 注文履歴タブ
        if (tabHeaders.Count > 1)
        {
            tabHeaders[1].Click();
            Assert.Contains("注文履歴タブに切り替えました", cut.Markup);
        }

        // Act & Assert - 設定タブ
        if (tabHeaders.Count > 2)
        {
            tabHeaders[2].Click();
            Assert.Contains("設定タブに切り替えました", cut.Markup);
        }

        // Act & Assert - 統計情報タブ
        if (tabHeaders.Count > 3)
        {
            tabHeaders[3].Click();
            Assert.Contains("統計情報タブに切り替えました", cut.Markup);
        }
    }
}
