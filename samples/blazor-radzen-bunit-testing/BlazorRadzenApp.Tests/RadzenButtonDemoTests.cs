using BlazorRadzenApp.Components.CustomComponents;
using Bunit;
using Radzen;
using Xunit;

namespace BlazorRadzenApp.Tests;

/// <summary>
/// Radzenコンポーネントを使用したRadzenButtonDemoのテスト
/// </summary>
public class RadzenButtonDemoTests : TestContext
{
    public RadzenButtonDemoTests()
    {
        // Radzenサービスの登録
        Services.AddRadzenComponents();
    }

    [Fact]
    public void RadzenButtonDemo_初期表示_タイトルが表示される()
    {
        // Arrange & Act
        var cut = RenderComponent<RadzenButtonDemo>();

        // Assert
        cut.Find("h3").MarkupMatches("<h3>Radzen ボタンデモ</h3>");
    }

    [Fact]
    public void RadzenButtonDemo_初期表示_メッセージが空()
    {
        // Arrange & Act
        var cut = RenderComponent<RadzenButtonDemo>();

        // Assert
        Assert.Throws<ElementNotFoundException>(() => cut.Find(".alert-info"));
    }

    [Fact]
    public void RadzenButtonDemo_プライマリボタンクリック_メッセージが表示される()
    {
        // Arrange
        var cut = RenderComponent<RadzenButtonDemo>();

        // Act - 最初のRadzenButtonを探してクリック
        var buttons = cut.FindAll("button");
        var primaryButton = buttons.First(b => b.TextContent.Contains("クリックしてください"));
        primaryButton.Click();

        // Assert
        var alert = cut.Find(".alert-info");
        Assert.Contains("ボタンがクリックされました (合計: 1回)", alert.TextContent);
    }

    [Fact]
    public void RadzenButtonDemo_複数回クリック_カウントが増加する()
    {
        // Arrange
        var cut = RenderComponent<RadzenButtonDemo>();
        var buttons = cut.FindAll("button");
        var primaryButton = buttons.First(b => b.TextContent.Contains("クリックしてください"));

        // Act - 3回クリック
        primaryButton.Click();
        primaryButton.Click();
        primaryButton.Click();

        // Assert
        var alert = cut.Find(".alert-info");
        Assert.Contains("合計: 3回", alert.TextContent);
    }

    [Fact]
    public void RadzenButtonDemo_成功ボタンクリック_適切なメッセージが表示される()
    {
        // Arrange
        var cut = RenderComponent<RadzenButtonDemo>();

        // Act
        var buttons = cut.FindAll("button");
        var successButton = buttons.First(b => b.TextContent.Contains("成功ボタン"));
        successButton.Click();

        // Assert
        var alert = cut.Find(".alert-info");
        Assert.Contains("Successボタンがクリックされました", alert.TextContent);
    }

    [Fact]
    public void RadzenButtonDemo_警告ボタンクリック_適切なメッセージが表示される()
    {
        // Arrange
        var cut = RenderComponent<RadzenButtonDemo>();

        // Act
        var buttons = cut.FindAll("button");
        var warningButton = buttons.First(b => b.TextContent.Contains("警告ボタン"));
        warningButton.Click();

        // Assert
        var alert = cut.Find(".alert-info");
        Assert.Contains("Warningボタンがクリックされました", alert.TextContent);
    }

    [Fact]
    public void RadzenButtonDemo_イベントコールバック_パラメータが正しく渡される()
    {
        // Arrange
        string? receivedMessage = null;
        var cut = RenderComponent<RadzenButtonDemo>(parameters => parameters
            .Add(p => p.OnClick, (string msg) => receivedMessage = msg));

        // Act
        var buttons = cut.FindAll("button");
        var primaryButton = buttons.First(b => b.TextContent.Contains("クリックしてください"));
        primaryButton.Click();

        // Assert
        Assert.NotNull(receivedMessage);
        Assert.Contains("ボタンがクリックされました", receivedMessage);
    }

    [Fact]
    public void RadzenButtonDemo_成功ボタンクリック_Successパラメータが渡される()
    {
        // Arrange
        string? receivedType = null;
        var cut = RenderComponent<RadzenButtonDemo>(parameters => parameters
            .Add(p => p.OnClick, (string type) => receivedType = type));

        // Act
        var buttons = cut.FindAll("button");
        var successButton = buttons.First(b => b.TextContent.Contains("成功ボタン"));
        successButton.Click();

        // Assert
        Assert.Equal("Success", receivedType);
    }
}
