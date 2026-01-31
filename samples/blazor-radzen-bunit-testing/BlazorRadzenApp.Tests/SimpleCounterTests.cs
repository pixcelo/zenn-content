using BlazorRadzenApp.Components.CustomComponents;
using Bunit;
using Xunit;

namespace BlazorRadzenApp.Tests;

/// <summary>
/// HTMLベースのSimpleCounterコンポーネントのテスト
/// </summary>
public class SimpleCounterTests : TestContext
{
    [Fact]
    public void SimpleCounter_初期表示_デフォルト値が表示される()
    {
        // Arrange & Act
        var cut = RenderComponent<SimpleCounter>();

        // Assert
        cut.Find("h3").MarkupMatches("<h3>カウンター</h3>");
        cut.Find("strong").MarkupMatches("<strong>0</strong>");
    }

    [Fact]
    public void SimpleCounter_タイトルパラメータ設定_カスタムタイトルが表示される()
    {
        // Arrange & Act
        var cut = RenderComponent<SimpleCounter>(parameters => parameters
            .Add(p => p.Title, "カスタムカウンター"));

        // Assert
        cut.Find("h3").MarkupMatches("<h3>カスタムカウンター</h3>");
    }

    [Fact]
    public void SimpleCounter_初期値パラメータ設定_指定した初期値が表示される()
    {
        // Arrange & Act
        var cut = RenderComponent<SimpleCounter>(parameters => parameters
            .Add(p => p.InitialValue, 10));

        // Assert
        cut.Find("strong").MarkupMatches("<strong>10</strong>");
    }

    [Fact]
    public void SimpleCounter_カウントアップボタンクリック_カウントが増加する()
    {
        // Arrange
        var cut = RenderComponent<SimpleCounter>();
        var button = cut.Find("button.btn-primary");

        // Act
        button.Click();

        // Assert
        cut.Find("strong").MarkupMatches("<strong>1</strong>");

        // Act - 再度クリック
        button.Click();

        // Assert
        cut.Find("strong").MarkupMatches("<strong>2</strong>");
    }

    [Fact]
    public void SimpleCounter_リセットボタンクリック_初期値に戻る()
    {
        // Arrange
        var cut = RenderComponent<SimpleCounter>(parameters => parameters
            .Add(p => p.InitialValue, 5));
        var incrementButton = cut.Find("button.btn-primary");
        var resetButton = cut.Find("button.btn-secondary");

        // Act - カウントアップ
        incrementButton.Click();
        incrementButton.Click();

        // Assert
        cut.Find("strong").MarkupMatches("<strong>7</strong>");

        // Act - リセット
        resetButton.Click();

        // Assert
        cut.Find("strong").MarkupMatches("<strong>5</strong>");
    }

    [Fact]
    public void SimpleCounter_カウント変更時_イベントコールバックが発火する()
    {
        // Arrange
        int? receivedValue = null;
        var cut = RenderComponent<SimpleCounter>(parameters => parameters
            .Add(p => p.OnCountChanged, (int value) => receivedValue = value));

        var button = cut.Find("button.btn-primary");

        // Act
        button.Click();

        // Assert
        Assert.NotNull(receivedValue);
        Assert.Equal(1, receivedValue);
    }
}
