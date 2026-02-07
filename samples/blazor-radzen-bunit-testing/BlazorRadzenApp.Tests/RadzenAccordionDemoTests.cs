using BlazorRadzenApp.Components.CustomComponents;
using Bunit;
using Radzen;
using Xunit;

namespace BlazorRadzenApp.Tests;

/// <summary>
/// Radzen Accordionコンポーネントのテスト
/// </summary>
public class RadzenAccordionDemoTests : TestContext
{
    public RadzenAccordionDemoTests()
    {
        // Radzenサービスの登録
        Services.AddRadzenComponents();
    }

    [Fact]
    public void RadzenAccordionDemo_初期表示_タイトルが表示される()
    {
        // Arrange & Act
        var cut = RenderComponent<RadzenAccordionDemo>();

        // Assert
        cut.Find("h3").MarkupMatches("<h3>Radzen Accordion デモ</h3>");
    }

    [Fact]
    public void RadzenAccordionDemo_初期表示_RadzenAccordionがレンダリングされる()
    {
        // Arrange & Act
        var cut = RenderComponent<RadzenAccordionDemo>();

        // Assert - RadzenAccordionのコンテナが存在する
        var accordion = cut.Find(".rz-accordion");
        Assert.NotNull(accordion);
    }

    [Fact]
    public void RadzenAccordionDemo_初期表示_4つのアイテムが存在する()
    {
        // Arrange & Act
        var cut = RenderComponent<RadzenAccordionDemo>();

        // Assert - アコーディオンヘッダーが4つ存在する
        var markup = cut.Markup;
        Assert.Contains("基本情報", markup);
        Assert.Contains("商品情報", markup);
        Assert.Contains("統計情報", markup);
        Assert.Contains("設定", markup);
    }

    [Fact]
    public void RadzenAccordionDemo_基本情報アイテム_アイコンが表示される()
    {
        // Arrange & Act
        var cut = RenderComponent<RadzenAccordionDemo>();

        // Assert - アイテムにアイコンが含まれている
        var markup = cut.Markup;
        Assert.Contains("person", markup); // 基本情報のアイコン
        Assert.Contains("inventory", markup); // 商品情報のアイコン
        Assert.Contains("bar_chart", markup); // 統計情報のアイコン
        Assert.Contains("settings", markup); // 設定のアイコン
    }

    [Fact]
    public void RadzenAccordionDemo_アイテムクリック_コンテンツが展開される()
    {
        // Arrange
        var cut = RenderComponent<RadzenAccordionDemo>();

        // Act - 基本情報アイテムをクリック
        var accordionHeaders = cut.FindAll(".rz-accordion-header");
        if (accordionHeaders.Count > 0)
        {
            accordionHeaders[0].Click();
        }

        // Assert - 基本情報のコンテンツが表示される
        var markup = cut.Markup;
        Assert.Contains("ユーザー基本情報", markup);
        Assert.Contains("ユーザー名", markup);
        Assert.Contains("メールアドレス", markup);
        Assert.Contains("電話番号", markup);
    }

    [Fact]
    public void RadzenAccordionDemo_基本情報展開_フォーム項目が表示される()
    {
        // Arrange
        var cut = RenderComponent<RadzenAccordionDemo>();

        // Act - 基本情報アイテムを展開
        var accordionHeaders = cut.FindAll(".rz-accordion-header");
        if (accordionHeaders.Count > 0)
        {
            accordionHeaders[0].Click();
        }

        // Assert - フォームフィールドが存在する
        var textBoxes = cut.FindAll(".rz-textbox");
        Assert.True(textBoxes.Count >= 3, "テキストボックスが3つ以上存在する");

        // デフォルト値が表示されている
        var markup = cut.Markup;
        Assert.Contains("山田太郎", markup);
        Assert.Contains("yamada@example.com", markup);
        Assert.Contains("03-1234-5678", markup);
    }

    [Fact]
    public void RadzenAccordionDemo_商品情報展開_DataGridが表示される()
    {
        // Arrange
        var cut = RenderComponent<RadzenAccordionDemo>();

        // Act - 商品情報アイテムを展開（2番目のアイテム）
        var accordionHeaders = cut.FindAll(".rz-accordion-header");
        if (accordionHeaders.Count > 1)
        {
            accordionHeaders[1].Click();
        }

        // Assert - DataGridが表示される
        var dataGrid = cut.Find(".rz-datatable");
        Assert.NotNull(dataGrid);

        // 商品データが表示される
        var markup = cut.Markup;
        Assert.Contains("ノートPC", markup);
        Assert.Contains("¥120,000", markup);
    }

    [Fact]
    public void RadzenAccordionDemo_商品情報展開_5件の商品が表示される()
    {
        // Arrange
        var cut = RenderComponent<RadzenAccordionDemo>();

        // Act - 商品情報アイテムを展開
        var accordionHeaders = cut.FindAll(".rz-accordion-header");
        if (accordionHeaders.Count > 1)
        {
            accordionHeaders[1].Click();
        }

        // Assert - 5件の商品データ
        var markup = cut.Markup;
        Assert.Contains("ノートPC", markup);
        Assert.Contains("マウス", markup);
        Assert.Contains("キーボード", markup);
        Assert.Contains("モニター", markup);
        Assert.Contains("Webカメラ", markup);
    }

    [Fact]
    public void RadzenAccordionDemo_統計情報展開_統計カードが表示される()
    {
        // Arrange
        var cut = RenderComponent<RadzenAccordionDemo>();

        // Act - 統計情報アイテムを展開（3番目のアイテム）
        var accordionHeaders = cut.FindAll(".rz-accordion-header");
        if (accordionHeaders.Count > 2)
        {
            accordionHeaders[2].Click();
        }

        // Assert - 統計情報が表示される
        var markup = cut.Markup;
        Assert.Contains("販売統計", markup);
        Assert.Contains("総商品数", markup);
        Assert.Contains("総在庫数", markup);
        Assert.Contains("総在庫金額", markup);
        Assert.Contains("平均単価", markup);
    }

    [Fact]
    public void RadzenAccordionDemo_統計情報展開_総商品数が正しく計算される()
    {
        // Arrange
        var cut = RenderComponent<RadzenAccordionDemo>();

        // Act - 統計情報アイテムを展開
        var accordionHeaders = cut.FindAll(".rz-accordion-header");
        if (accordionHeaders.Count > 2)
        {
            accordionHeaders[2].Click();
        }

        // Assert - 商品数は5件
        var markup = cut.Markup;
        Assert.Contains(">5<", markup);
    }

    [Fact]
    public void RadzenAccordionDemo_統計情報展開_総在庫数が正しく計算される()
    {
        // Arrange
        var cut = RenderComponent<RadzenAccordionDemo>();

        // Act - 統計情報アイテムを展開
        var accordionHeaders = cut.FindAll(".rz-accordion-header");
        if (accordionHeaders.Count > 2)
        {
            accordionHeaders[2].Click();
        }

        // Assert - 総在庫数: 5 + 20 + 15 + 8 + 12 = 60
        var markup = cut.Markup;
        Assert.Contains(">60<", markup);
    }

    [Fact]
    public void RadzenAccordionDemo_設定展開_設定オプションが表示される()
    {
        // Arrange
        var cut = RenderComponent<RadzenAccordionDemo>();

        // Act - 設定アイテムを展開（4番目のアイテム）
        var accordionHeaders = cut.FindAll(".rz-accordion-header");
        if (accordionHeaders.Count > 3)
        {
            accordionHeaders[3].Click();
        }

        // Assert - 設定項目が表示される
        var markup = cut.Markup;
        Assert.Contains("アプリケーション設定", markup);
        Assert.Contains("複数のアイテムを同時に開く", markup);
        Assert.Contains("展開モード", markup);
        Assert.Contains("ダークモード", markup);
    }

    [Fact]
    public void RadzenAccordionDemo_ボタン表示_すべて展開と折りたたみボタンが存在する()
    {
        // Arrange & Act
        var cut = RenderComponent<RadzenAccordionDemo>();

        // Assert
        var buttons = cut.FindAll("button");
        var buttonTexts = buttons.Select(b => b.TextContent).ToList();

        Assert.Contains("すべて展開", buttonTexts);
        Assert.Contains("すべて折りたたむ", buttonTexts);
    }

    [Fact]
    public void RadzenAccordionDemo_すべて展開ボタンクリック_メッセージが表示される()
    {
        // Arrange
        var cut = RenderComponent<RadzenAccordionDemo>();

        // Act - すべて展開ボタンをクリック
        var buttons = cut.FindAll("button");
        var expandAllButton = buttons.FirstOrDefault(b => b.TextContent.Contains("すべて展開"));
        expandAllButton?.Click();

        // Assert
        var markup = cut.Markup;
        Assert.Contains("すべてのアイテムを展開しました", markup);
    }

    [Fact]
    public void RadzenAccordionDemo_すべて折りたたむボタンクリック_メッセージが表示される()
    {
        // Arrange
        var cut = RenderComponent<RadzenAccordionDemo>();

        // Act - すべて折りたたむボタンをクリック
        var buttons = cut.FindAll("button");
        var collapseAllButton = buttons.FirstOrDefault(b => b.TextContent.Contains("すべて折りたたむ"));
        collapseAllButton?.Click();

        // Assert
        var markup = cut.Markup;
        Assert.Contains("すべてのアイテムを折りたたみました", markup);
    }

    [Fact]
    public void RadzenAccordionDemo_複数アイテム展開_各コンテンツが正しく表示される()
    {
        // Arrange
        var cut = RenderComponent<RadzenAccordionDemo>();
        var accordionHeaders = cut.FindAll(".rz-accordion-header");

        // Act - 基本情報を展開
        if (accordionHeaders.Count > 0)
        {
            accordionHeaders[0].Click();
            Assert.Contains("ユーザー基本情報", cut.Markup);
        }

        // Act - 商品情報を展開
        if (accordionHeaders.Count > 1)
        {
            accordionHeaders[1].Click();
            Assert.Contains("在庫商品一覧", cut.Markup);
        }

        // Act - 統計情報を展開
        if (accordionHeaders.Count > 2)
        {
            accordionHeaders[2].Click();
            Assert.Contains("販売統計", cut.Markup);
        }

        // Act - 設定を展開
        if (accordionHeaders.Count > 3)
        {
            accordionHeaders[3].Click();
            Assert.Contains("アプリケーション設定", cut.Markup);
        }
    }
}
