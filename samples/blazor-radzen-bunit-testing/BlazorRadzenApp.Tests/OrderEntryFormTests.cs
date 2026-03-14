using BlazorRadzenApp.Components.CustomComponents;
using Bunit;
using Radzen;
using Radzen.Blazor;

namespace BlazorRadzenApp.Tests;

/// <summary>
/// 受注入力フォームのテスト - 単方向バインディング、値の連動、バリデーションの検証
/// </summary>
public class OrderEntryFormTests : TestContext
{
    public OrderEntryFormTests()
    {
        // Radzenコンポーネントを使用するため必須
        Services.AddRadzenComponents();

        // Radzenコンポーネントが使用するJSInteropをモック（全パターン対応）
        JSInterop.Mode = JSRuntimeMode.Loose; // 未定義のJSInterop呼び出しを自動的に許可
    }

    [Fact]
    public void OrderEntryForm_初期状態_登録ボタンが無効になっている()
    {
        // Arrange & Act
        var cut = RenderComponent<OrderEntryForm>();

        // Assert: 登録ボタンがdisabled
        var registerButton = cut.FindComponent<RadzenButton>();
        Assert.True(registerButton.Instance.Disabled);
    }

    [Fact]
    public async Task OrderEntryForm_商品名入力_値が反映される()
    {
        // Arrange
        var cut = RenderComponent<OrderEntryForm>();

        // Act: 商品名に「ノートPC」を入力
        var textBox = cut.FindComponent<RadzenTextBox>();
        await cut.InvokeAsync(async () => await textBox.Instance.ValueChanged.InvokeAsync("ノートPC"));

        // Assert: TextBoxのValueが更新されている
        cut.WaitForState(() => textBox.Instance.Value == "ノートPC", timeout: TimeSpan.FromSeconds(1));
        Assert.Equal("ノートPC", textBox.Instance.Value);
    }

    [Fact]
    public async Task OrderEntryForm_数量と単価入力_金額が自動計算される()
    {
        // Arrange
        var cut = RenderComponent<OrderEntryForm>();
        var numerics = cut.FindComponents<RadzenNumeric<int?>>();
        var quantityNumeric = numerics[0]; // 数量

        var decimalNumerics = cut.FindComponents<RadzenNumeric<decimal?>>();
        var unitPriceNumeric = decimalNumerics[0]; // 単価

        // Act: 数量=5, 単価=1000を入力
        await cut.InvokeAsync(async () =>
        {
            await quantityNumeric.Instance.ValueChanged.InvokeAsync(5);
            await unitPriceNumeric.Instance.ValueChanged.InvokeAsync(1000m);
        });

        // Assert: 金額=5000が表示される
        var amountField = cut.FindAll(".calculated-value")[0];
        Assert.Contains("5,000", amountField.TextContent);
    }

    [Fact]
    public async Task OrderEntryForm_金額計算_消費税額と合計金額が自動計算される()
    {
        // Arrange
        var cut = RenderComponent<OrderEntryForm>();
        var numerics = cut.FindComponents<RadzenNumeric<int?>>();
        var quantityNumeric = numerics[0];

        var decimalNumerics = cut.FindComponents<RadzenNumeric<decimal?>>();
        var unitPriceNumeric = decimalNumerics[0];

        // Act: 数量=10, 単価=2000を入力
        await cut.InvokeAsync(async () =>
        {
            await quantityNumeric.Instance.ValueChanged.InvokeAsync(10);
            await unitPriceNumeric.Instance.ValueChanged.InvokeAsync(2000m);
        });

        // Assert:
        // 金額 = 10 * 2000 = 20,000
        // 消費税額 = 20,000 * 0.1 = 2,000
        // 合計金額 = 20,000 + 2,000 = 22,000
        var calculatedValues = cut.FindAll(".calculated-value");
        Assert.Contains("20,000", calculatedValues[0].TextContent); // 金額
        Assert.Contains("2,000", calculatedValues[1].TextContent);  // 消費税額
        Assert.Contains("22,000", calculatedValues[2].TextContent); // 合計金額
    }

    [Fact]
    public async Task OrderEntryForm_単価変更_金額が再計算される()
    {
        // Arrange
        var cut = RenderComponent<OrderEntryForm>();
        var numerics = cut.FindComponents<RadzenNumeric<int?>>();
        var quantityNumeric = numerics[0];

        var decimalNumerics = cut.FindComponents<RadzenNumeric<decimal?>>();
        var unitPriceNumeric = decimalNumerics[0];

        // 初回入力: 数量=3, 単価=500
        await cut.InvokeAsync(async () =>
        {
            await quantityNumeric.Instance.ValueChanged.InvokeAsync(3);
            await unitPriceNumeric.Instance.ValueChanged.InvokeAsync(500m);
        });

        // Act: 単価を1000に変更
        await cut.InvokeAsync(async () => await unitPriceNumeric.Instance.ValueChanged.InvokeAsync(1000m));

        // Assert: 金額が再計算される (3 * 1000 = 3000)
        var amountField = cut.FindAll(".calculated-value")[0];
        Assert.Contains("3,000", amountField.TextContent);
    }

    [Fact]
    public async Task OrderEntryForm_すべての必須項目入力_登録ボタンが有効になる()
    {
        // Arrange
        var cut = RenderComponent<OrderEntryForm>();

        // Act: すべての項目を入力
        var textBox = cut.FindComponent<RadzenTextBox>();
        var numerics = cut.FindComponents<RadzenNumeric<int?>>();
        var decimalNumerics = cut.FindComponents<RadzenNumeric<decimal?>>();
        var datePickers = cut.FindComponents<RadzenDatePicker<DateTime?>>();

        await cut.InvokeAsync(async () =>
        {
            await textBox.Instance.ValueChanged.InvokeAsync("キーボード");
            await numerics[0].Instance.ValueChanged.InvokeAsync(2);
            await decimalNumerics[0].Instance.ValueChanged.InvokeAsync(3500m);
            await datePickers[0].Instance.ValueChanged.InvokeAsync(new DateTime(2024, 3, 15)); // 注文日
            await datePickers[1].Instance.ValueChanged.InvokeAsync(new DateTime(2024, 4, 1));  // 納期
        });

        // Assert: 登録ボタンが有効
        cut.WaitForState(() => !cut.FindComponent<RadzenButton>().Instance.Disabled, timeout: TimeSpan.FromSeconds(1));
        Assert.False(cut.FindComponent<RadzenButton>().Instance.Disabled);
    }

    [Fact]
    public async Task OrderEntryForm_数量が0_登録ボタンが無効のまま()
    {
        // Arrange
        var cut = RenderComponent<OrderEntryForm>();
        var textBox = cut.FindComponent<RadzenTextBox>();
        var numerics = cut.FindComponents<RadzenNumeric<int?>>();
        var quantityNumeric = numerics[0];
        var decimalNumerics = cut.FindComponents<RadzenNumeric<decimal?>>();

        await cut.InvokeAsync(async () =>
        {
            await textBox.Instance.ValueChanged.InvokeAsync("マウス");
            await decimalNumerics[0].Instance.ValueChanged.InvokeAsync(1000m);
        });

        // Act: 数量に0を設定（無効値）
        await cut.InvokeAsync(async () => await quantityNumeric.Instance.ValueChanged.InvokeAsync(0));

        // Assert: 登録ボタンは無効のまま
        var registerButton = cut.FindComponent<RadzenButton>();
        Assert.True(registerButton.Instance.Disabled);
    }

    [Fact]
    public async Task OrderEntryForm_登録ボタンクリック_成功メッセージが表示される()
    {
        // Arrange
        var cut = RenderComponent<OrderEntryForm>();

        // すべての項目を入力
        var textBox = cut.FindComponent<RadzenTextBox>();
        var numerics = cut.FindComponents<RadzenNumeric<int?>>();
        var decimalNumerics = cut.FindComponents<RadzenNumeric<decimal?>>();
        var datePickers = cut.FindComponents<RadzenDatePicker<DateTime?>>();

        await cut.InvokeAsync(async () =>
        {
            await textBox.Instance.ValueChanged.InvokeAsync("モニター");
            await numerics[0].Instance.ValueChanged.InvokeAsync(2);
            await decimalNumerics[0].Instance.ValueChanged.InvokeAsync(35000m);
            await datePickers[0].Instance.ValueChanged.InvokeAsync(new DateTime(2024, 3, 15)); // 注文日
            await datePickers[1].Instance.ValueChanged.InvokeAsync(new DateTime(2024, 4, 1));  // 納期
        });

        // Act: 登録ボタンをクリック
        var registerButton = cut.FindComponent<RadzenButton>();
        registerButton.Find("button").Click();

        // Assert: 成功メッセージが表示される
        var successMessage = cut.Find(".success-message");
        Assert.Contains("受注を登録しました", successMessage.TextContent);
        Assert.Contains("モニター", successMessage.TextContent);
        Assert.Contains("2 個", successMessage.TextContent);
        Assert.Contains("77,000", successMessage.TextContent); // 35000*2*1.1=77000
    }

    [Fact]
    public async Task OrderEntryForm_登録後_入力内容が成功メッセージに表示される()
    {
        // Arrange
        var cut = RenderComponent<OrderEntryForm>();

        // 入力
        var textBox = cut.FindComponent<RadzenTextBox>();
        var numerics = cut.FindComponents<RadzenNumeric<int?>>();
        var decimalNumerics = cut.FindComponents<RadzenNumeric<decimal?>>();
        var datePickers = cut.FindComponents<RadzenDatePicker<DateTime?>>();

        await cut.InvokeAsync(async () =>
        {
            await textBox.Instance.ValueChanged.InvokeAsync("Webカメラ");
            await numerics[0].Instance.ValueChanged.InvokeAsync(5);
            await decimalNumerics[0].Instance.ValueChanged.InvokeAsync(8000m);
            await datePickers[0].Instance.ValueChanged.InvokeAsync(new DateTime(2024, 3, 15)); // 注文日
            await datePickers[1].Instance.ValueChanged.InvokeAsync(new DateTime(2024, 4, 1));  // 納期
        });

        // Act: 登録
        var registerButton = cut.FindComponent<RadzenButton>();
        registerButton.Find("button").Click();

        // Assert: 成功メッセージに入力内容が表示される
        var successMessage = cut.Find(".success-message");
        Assert.Contains("Webカメラ", successMessage.TextContent);
        Assert.Contains("5 個", successMessage.TextContent);
        Assert.Contains("44,000", successMessage.TextContent); // 8000*5*1.1=44000
    }

    [Fact]
    public void OrderEntryForm_商品名が空_バリデーションエラーが表示されない()
    {
        // Arrange & Act
        var cut = RenderComponent<OrderEntryForm>();

        // Assert: 初期状態ではバリデーションエラーは表示されない
        Assert.Throws<Bunit.ElementNotFoundException>(() => cut.Find(".validation-error"));
    }

    [Fact]
    public async Task OrderEntryForm_値の連動_数量変更で消費税と合計が再計算される()
    {
        // Arrange
        var cut = RenderComponent<OrderEntryForm>();
        var numerics = cut.FindComponents<RadzenNumeric<int?>>();
        var quantityNumeric = numerics[0];

        var decimalNumerics = cut.FindComponents<RadzenNumeric<decimal?>>();
        var unitPriceNumeric = decimalNumerics[0];

        // 初回入力: 数量=2, 単価=10000
        await cut.InvokeAsync(async () =>
        {
            await quantityNumeric.Instance.ValueChanged.InvokeAsync(2);
            await unitPriceNumeric.Instance.ValueChanged.InvokeAsync(10000m);
        });

        // Act: 数量を5に変更
        await cut.InvokeAsync(async () => await quantityNumeric.Instance.ValueChanged.InvokeAsync(5));

        // Assert: すべての計算値が再計算される
        // 金額 = 5 * 10000 = 50,000
        // 消費税 = 50,000 * 0.1 = 5,000
        // 合計 = 50,000 + 5,000 = 55,000
        var calculatedValues = cut.FindAll(".calculated-value");
        Assert.Contains("50,000", calculatedValues[0].TextContent);
        Assert.Contains("5,000", calculatedValues[1].TextContent);
        Assert.Contains("55,000", calculatedValues[2].TextContent);
    }

    [Fact]
    public async Task OrderEntryForm_納期が注文日より前_登録ボタンが無効()
    {
        // Arrange
        var cut = RenderComponent<OrderEntryForm>();

        // すべての基本項目を入力
        await cut.InvokeAsync(async () =>
        {
            await cut.FindComponent<RadzenTextBox>().Instance.ValueChanged.InvokeAsync("商品A");
            await cut.FindComponents<RadzenNumeric<int?>>()[0].Instance.ValueChanged.InvokeAsync(5);
            await cut.FindComponents<RadzenNumeric<decimal?>>()[0].Instance.ValueChanged.InvokeAsync(1000m);
        });

        // Act: 注文日と納期を設定（納期が前）
        var datePickers = cut.FindComponents<RadzenDatePicker<DateTime?>>();
        await cut.InvokeAsync(async () =>
        {
            await datePickers[0].Instance.ValueChanged.InvokeAsync(new DateTime(2024, 3, 15)); // 注文日
            await datePickers[1].Instance.ValueChanged.InvokeAsync(new DateTime(2024, 3, 10)); // 納期（前）
        });

        // Assert: 登録ボタンが無効（納期が注文日より前のため）
        var registerButton = cut.FindComponent<RadzenButton>();
        Assert.True(registerButton.Instance.Disabled);
    }

    [Fact]
    public async Task OrderEntryForm_納期が注文日より後_バリデーション通過()
    {
        // Arrange
        var cut = RenderComponent<OrderEntryForm>();

        // Act: すべての項目を正しく入力
        await cut.InvokeAsync(async () =>
        {
            await cut.FindComponent<RadzenTextBox>().Instance.ValueChanged.InvokeAsync("商品B");
            await cut.FindComponents<RadzenNumeric<int?>>()[0].Instance.ValueChanged.InvokeAsync(3);
            await cut.FindComponents<RadzenNumeric<decimal?>>()[0].Instance.ValueChanged.InvokeAsync(2000m);

            var datePickers = cut.FindComponents<RadzenDatePicker<DateTime?>>();
            await datePickers[0].Instance.ValueChanged.InvokeAsync(new DateTime(2024, 3, 15)); // 注文日
            await datePickers[1].Instance.ValueChanged.InvokeAsync(new DateTime(2024, 4, 1));  // 納期（後）
        });

        // Assert: 登録ボタンが有効（すべてのバリデーション通過）
        cut.WaitForState(() => !cut.FindComponent<RadzenButton>().Instance.Disabled, timeout: TimeSpan.FromSeconds(1));
        Assert.False(cut.FindComponent<RadzenButton>().Instance.Disabled);
    }

    [Fact]
    public async Task OrderEntryForm_納期が注文日と同じ_登録ボタンが無効()
    {
        // Arrange
        var cut = RenderComponent<OrderEntryForm>();

        // すべての基本項目を入力
        await cut.InvokeAsync(async () =>
        {
            await cut.FindComponent<RadzenTextBox>().Instance.ValueChanged.InvokeAsync("商品C");
            await cut.FindComponents<RadzenNumeric<int?>>()[0].Instance.ValueChanged.InvokeAsync(2);
            await cut.FindComponents<RadzenNumeric<decimal?>>()[0].Instance.ValueChanged.InvokeAsync(3000m);
        });

        // Act: 注文日と納期を同じ日付に設定
        var datePickers = cut.FindComponents<RadzenDatePicker<DateTime?>>();
        var sameDate = new DateTime(2024, 3, 20);
        await cut.InvokeAsync(async () =>
        {
            await datePickers[0].Instance.ValueChanged.InvokeAsync(sameDate); // 注文日
            await datePickers[1].Instance.ValueChanged.InvokeAsync(sameDate); // 納期（同じ）
        });

        // Assert: 登録ボタンが無効（納期は注文日より後でなければならない）
        var registerButton = cut.FindComponent<RadzenButton>();
        Assert.True(registerButton.Instance.Disabled);
    }

    [Fact]
    public async Task OrderEntryForm_納期が3ヶ月以上先_警告メッセージ表示()
    {
        // Arrange
        var cut = RenderComponent<OrderEntryForm>();

        // すべての項目を入力
        await cut.InvokeAsync(async () =>
        {
            await cut.FindComponent<RadzenTextBox>().Instance.ValueChanged.InvokeAsync("商品D");
            await cut.FindComponents<RadzenNumeric<int?>>()[0].Instance.ValueChanged.InvokeAsync(1);
            await cut.FindComponents<RadzenNumeric<decimal?>>()[0].Instance.ValueChanged.InvokeAsync(5000m);
        });

        // Act: 注文日と納期を設定（納期が3ヶ月以上先）
        var datePickers = cut.FindComponents<RadzenDatePicker<DateTime?>>();
        await cut.InvokeAsync(async () =>
        {
            await datePickers[0].Instance.ValueChanged.InvokeAsync(new DateTime(2024, 3, 15)); // 注文日
            await datePickers[1].Instance.ValueChanged.InvokeAsync(new DateTime(2024, 7, 1));  // 納期（3ヶ月以上先）
        });

        // Assert: 警告メッセージが表示される
        var warnings = cut.FindAll(".validation-warning");
        Assert.NotEmpty(warnings);
        Assert.Contains("3ヶ月以上先", warnings[0].TextContent);
    }
}
