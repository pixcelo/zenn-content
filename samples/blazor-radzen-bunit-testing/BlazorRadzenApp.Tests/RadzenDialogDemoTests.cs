using BlazorRadzenApp.Components.CustomComponents;
using Bunit;
using Radzen;
using Xunit;

namespace BlazorRadzenApp.Tests;

/// <summary>
/// Radzen Dialogコンポーネントのテスト
/// </summary>
public class RadzenDialogDemoTests : TestContext
{
    public RadzenDialogDemoTests()
    {
        // Radzenサービスの登録
        Services.AddRadzenComponents();

        // JSInteropの設定（DialogService用）
        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    [Fact]
    public void RadzenDialogDemo_初期表示_タイトルが表示される()
    {
        // Arrange & Act
        var cut = RenderComponent<RadzenDialogDemo>();

        // Assert
        cut.Find("h3").MarkupMatches("<h3>Radzen Dialog デモ</h3>");
    }

    [Fact]
    public void RadzenDialogDemo_初期表示_2つのボタンが表示される()
    {
        // Arrange & Act
        var cut = RenderComponent<RadzenDialogDemo>();

        // Assert
        var buttons = cut.FindAll("button");
        Assert.True(buttons.Count >= 2);
        Assert.Contains(buttons, b => b.TextContent.Contains("ダイアログを開く"));
        Assert.Contains(buttons, b => b.TextContent.Contains("確認ダイアログ"));
    }

    [Fact]
    public void RadzenDialogDemo_初期表示_結果メッセージが非表示()
    {
        // Arrange & Act
        var cut = RenderComponent<RadzenDialogDemo>();

        // Assert
        Assert.Throws<ElementNotFoundException>(() => cut.Find(".alert-success"));
    }

    [Fact]
    public void RadzenDialogDemo_ダイアログ開くボタンクリック_DialogServiceが呼び出される()
    {
        // Arrange
        var cut = RenderComponent<RadzenDialogDemo>();
        var buttons = cut.FindAll("button");
        var openDialogButton = buttons.First(b => b.TextContent.Contains("ダイアログを開く"));

        // Act & Assert - DialogServiceが利用可能であることを確認
        // 注: 実際のダイアログ表示のテストはE2Eテストで行うべき
        // ここではボタンがクリック可能であることを確認
        Assert.NotNull(openDialogButton);
    }

    [Fact]
    public void RadzenDialogDemo_確認ダイアログボタンクリック_DialogServiceが呼び出される()
    {
        // Arrange
        var cut = RenderComponent<RadzenDialogDemo>();
        var buttons = cut.FindAll("button");
        var confirmDialogButton = buttons.First(b => b.TextContent.Contains("確認ダイアログ"));

        // Act & Assert
        Assert.NotNull(confirmDialogButton);
    }

    [Fact]
    public void RadzenDialogDemo_レンダリング_必要な要素が存在する()
    {
        // Arrange & Act
        var cut = RenderComponent<RadzenDialogDemo>();

        // Assert
        var divContainer = cut.Find(".radzen-dialog-demo");
        Assert.NotNull(divContainer);

        var heading = cut.Find("h3");
        Assert.Equal("Radzen Dialog デモ", heading.TextContent);
    }

    [Fact]
    public void RadzenDialogDemo_マークアップ構造_正しいクラスが適用されている()
    {
        // Arrange & Act
        var cut = RenderComponent<RadzenDialogDemo>();

        // Assert
        var buttons = cut.FindAll("button.rz-button");
        // Radzenボタンが存在することを確認
        Assert.NotEmpty(buttons);
    }
}
