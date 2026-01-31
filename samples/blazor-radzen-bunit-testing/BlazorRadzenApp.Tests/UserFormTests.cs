using BlazorRadzenApp.Components.CustomComponents;
using Bunit;
using Xunit;
using static BlazorRadzenApp.Components.CustomComponents.UserForm;

namespace BlazorRadzenApp.Tests;

/// <summary>
/// HTMLベースのUserFormコンポーネントのテスト
/// </summary>
public class UserFormTests : TestContext
{
    [Fact]
    public void UserForm_初期表示_フォームが正しくレンダリングされる()
    {
        // Arrange & Act
        var cut = RenderComponent<UserForm>();

        // Assert
        Assert.NotNull(cut.Find("#username"));
        Assert.NotNull(cut.Find("#email"));
        Assert.NotNull(cut.Find("input[type='checkbox']"));
        Assert.NotNull(cut.Find("button.btn-success"));
    }

    [Fact]
    public void UserForm_初期状態_送信ボタンが無効()
    {
        // Arrange & Act
        var cut = RenderComponent<UserForm>();
        var submitButton = cut.Find("button.btn-success");

        // Assert
        Assert.True(submitButton.HasAttribute("disabled"));
    }

    [Fact]
    public void UserForm_入力_双方向バインディングが機能する()
    {
        // Arrange
        var cut = RenderComponent<UserForm>();
        var usernameInput = cut.Find("#username");
        var emailInput = cut.Find("#email");

        // Act
        usernameInput.Change("テストユーザー");
        emailInput.Change("test@example.com");

        // Assert
        Assert.Equal("テストユーザー", usernameInput.GetAttribute("value"));
        Assert.Equal("test@example.com", emailInput.GetAttribute("value"));
    }

    [Fact]
    public void UserForm_すべて入力完了_送信ボタンが有効化される()
    {
        // Arrange
        var cut = RenderComponent<UserForm>();
        var usernameInput = cut.Find("#username");
        var emailInput = cut.Find("#email");
        var checkbox = cut.Find("input[type='checkbox']");

        // Act
        usernameInput.Change("テストユーザー");
        emailInput.Change("test@example.com");
        checkbox.Change(true);

        // Assert
        var submitButton = cut.Find("button.btn-success");
        Assert.False(submitButton.HasAttribute("disabled"));
    }

    [Fact]
    public void UserForm_送信ボタンクリック_イベントコールバックが発火する()
    {
        // Arrange
        UserFormData? receivedData = null;
        var cut = RenderComponent<UserForm>(parameters => parameters
            .Add(p => p.OnSubmit, (UserFormData data) => receivedData = data));

        var usernameInput = cut.Find("#username");
        var emailInput = cut.Find("#email");
        var checkbox = cut.Find("input[type='checkbox']");

        // Act - フォーム入力
        usernameInput.Change("テストユーザー");
        emailInput.Change("test@example.com");
        checkbox.Change(true);

        // Act - 送信
        var submitButton = cut.Find("button.btn-success");
        submitButton.Click();

        // Assert
        Assert.NotNull(receivedData);
        Assert.Equal("テストユーザー", receivedData.Username);
        Assert.Equal("test@example.com", receivedData.Email);
        Assert.True(receivedData.AcceptedTerms);
    }

    [Fact]
    public void UserForm_送信後_成功メッセージが表示される()
    {
        // Arrange
        var cut = RenderComponent<UserForm>();
        var usernameInput = cut.Find("#username");
        var emailInput = cut.Find("#email");
        var checkbox = cut.Find("input[type='checkbox']");

        // Act
        usernameInput.Change("テストユーザー");
        emailInput.Change("test@example.com");
        checkbox.Change(true);

        var submitButton = cut.Find("button.btn-success");
        submitButton.Click();

        // Assert
        var successAlert = cut.Find(".alert-success");
        Assert.Contains("送信完了", successAlert.TextContent);
        Assert.Contains("テストユーザー", successAlert.TextContent);
        Assert.Contains("test@example.com", successAlert.TextContent);
    }

    [Fact]
    public void UserForm_チェックボックス未選択_送信ボタンが無効のまま()
    {
        // Arrange
        var cut = RenderComponent<UserForm>();
        var usernameInput = cut.Find("#username");
        var emailInput = cut.Find("#email");

        // Act - チェックボックス以外を入力
        usernameInput.Change("テストユーザー");
        emailInput.Change("test@example.com");

        // Assert
        var submitButton = cut.Find("button.btn-success");
        Assert.True(submitButton.HasAttribute("disabled"));
    }
}
