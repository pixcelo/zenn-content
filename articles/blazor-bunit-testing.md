---
title: "BlazorコンポーネントをbUnitでテストする"
emoji: "🔄"
type: "tech"
topics: ["blazor", "csharp", "dotnet", "bunit", "web開発"]
published: false
publication_name: "nexta_"
---

ネクスタでsmartFの開発エンジニアをしている tetsu.k です。

ネクスタではBlazorを使ったWEBアプリを開発しています。今回は、開発した機能の品質を保証する手段として「bUnit」について調べた結果を共有します。

## bUnitの使いどころはどこか？

bUnitは「BlazorのUIコンポーネント」をテストするためのツールです。
xUnitやPlaywright（E2E）等のテストツールとの違いを表にしました。

| 項目 | xUnit | bUnit | Playwright |
|------|-------|-------|-----------|
| 対象レイヤー | 単体テスト | コンポーネントテスト | E2Eテスト |
| テスト対象 | C#クラス・メソッド | Blazorコンポーネント | ブラウザ操作全体 |
| ブラウザ | 不要 | 不要 | 必要（Chrome/Firefox/Safari） |
| 実行速度 | 超高速（数ms/テスト） | 高速（数十ms/テスト） | 低速（数秒/テスト） |
| 適用技術 | .NET全般 | Blazor専用 | あらゆるWebアプリ |
| JavaScript検証 | 不可 | 制限あり | 完全対応 |

じゃあどう使い分けるの？となると思うので、目安を以下に示します。

| テスト対象 | xUnit | bUnit | Playwright | 推奨ツール |
|-----------|-------|-------|-----------|----------|
| ビジネスロジック | ○ | - | - | xUnit |
| バリデーションルール | ○ | △ | - | xUnit |
| ドメインモデル | ○ | - | - | xUnit |
| コンポーネントのレンダリング | - | ○ | ○ | bUnit |
| パラメータバインディング | - | ○ | △ | bUnit |
| イベント処理 | - | ○ | ○ | bUnit |
| フォーム入力・送信 | - | ○ | ○ | bUnit |
| ページ遷移 | - | × | ○ | Playwright |
| 認証フロー | △ | △ | ○ | Playwright |
| クロスブラウザ検証 | - | - | ○ | Playwright |
| JavaScriptとの連携 | - | △ | ○ | Playwright |
| CSSレイアウト検証 | - | × | ○ | Playwright |

一般的な推奨配分です。

```
        /\
       /  \  E2E (Playwright)        10%   遅い・高コスト
      /────\
     /      \  統合 (bUnit)          30%   中速・中コスト
    /────────\
   /          \  単体 (xUnit)        60%   高速・低コスト
  /────────────\
```

下層のテストほど高速で安定性が高く、上層に行くほど実行時間が長く不安定になりやすい傾向があります。そのため、ビジネスロジックはxUnitで厚くテストし、クリティカルなユーザーフローのみPlaywrightで検証する戦略が効果的です。

テスト安定性の違いは、以下の通りです。
- xUnit: 非常に安定（外部要因に依存しない）
- bUnit: 安定（ブラウザ不要、C#のみで完結）
- Playwright: 比較的不安定（ブラウザレンダリング、タイミング、ネットワークに依存）

## bUnitの使用例

実務でよくある「受注入力フォーム」を例に、bUnitの使い方を見ていきます。

![受注入力デモ画面](/images/blazor-bunit-testing/order-entry-demo.png)

受注入力フォームには、以下の機能があります。

1. 基本入力（商品名、数量、単価、注文日、納期）
2. 値の連動（数量×単価→金額→消費税→合計の自動計算）
3. 組み合わせバリデーション（納期 > 注文日の関係性チェック）
4. 登録処理（入力完了後にボタン押下で登録）

:::details コンポーネント全コード（クリックで展開）

```razor
<div class="order-form">
    <h3>受注入力</h3>

    <!-- 商品名入力 -->
    <div class="form-field mb-3">
        <label>商品名: <span class="required">*</span></label>
        <RadzenTextBox Value="@_productName"
                       ValueChanged="@OnProductNameChanged"
                       Placeholder="商品名を入力"
                       MaxLength="50" />
    </div>

    <!-- 数量入力 -->
    <div class="form-field mb-3">
        <label>数量: <span class="required">*</span></label>
        <RadzenNumeric TValue="int?"
                       Value="@_quantity"
                       ValueChanged="@OnQuantityChanged"
                       Min="1"
                       ShowUpDown="true" />
    </div>

    <!-- 単価入力 -->
    <div class="form-field mb-3">
        <label>単価: <span class="required">*</span></label>
        <RadzenNumeric TValue="decimal?"
                       Value="@_unitPrice"
                       ValueChanged="@OnUnitPriceChanged"
                       Min="0"
                       Format="c0" />
    </div>

    <!-- 注文日入力 -->
    <div class="form-field mb-3">
        <label>注文日: <span class="required">*</span></label>
        <RadzenDatePicker TValue="DateTime?"
                          Value="@_orderDate"
                          ValueChanged="@OnOrderDateChanged"
                          DateFormat="yyyy/MM/dd" />
    </div>

    <!-- 納期入力 -->
    <div class="form-field mb-3">
        <label>納期: <span class="required">*</span></label>
        <RadzenDatePicker TValue="DateTime?"
                          Value="@_deliveryDate"
                          ValueChanged="@OnDeliveryDateChanged"
                          DateFormat="yyyy/MM/dd" />
    </div>

    <!-- 金額（自動計算） -->
    <div class="form-field mb-3">
        <label>金額:</label>
        <div class="calculated-value">¥@_amount.ToString("N0")</div>
    </div>

    <!-- 消費税額（自動計算） -->
    <div class="form-field mb-3">
        <label>消費税額 (10%):</label>
        <div class="calculated-value">¥@_taxAmount.ToString("N0")</div>
    </div>

    <!-- 合計金額（自動計算） -->
    <div class="form-field mb-3 total">
        <label>合計金額:</label>
        <div class="calculated-value total-amount">
            <strong>¥@_totalAmount.ToString("N0")</strong>
        </div>
    </div>

    <!-- 登録ボタン -->
    <RadzenButton Text="登録"
                  Click="@OnRegister"
                  Disabled="@(!IsValid)"
                  ButtonStyle="ButtonStyle.Success" />

    <!-- 登録完了メッセージ -->
    @if (_isRegistered)
    {
        <div class="alert alert-success mt-3 success-message">
            <p><strong>受注を登録しました</strong></p>
            <p>商品: @_productName</p>
            <p>数量: @_quantity 個</p>
            <p>合計金額: ¥@_totalAmount.ToString("N0")</p>
        </div>
    }
</div>

@code {
    // 入力値（単方向バインディング用）
    private string _productName = string.Empty;
    private int? _quantity;
    private decimal? _unitPrice;
    private DateTime? _orderDate;
    private DateTime? _deliveryDate;

    // 計算値（値の連動）
    private decimal _amount;
    private decimal _taxAmount;
    private decimal _totalAmount;
    private const decimal TaxRate = 0.10m;

    // 状態管理
    private bool _isRegistered;

    // バリデーション（組み合わせバリデーション含む）
    private bool IsValid =>
        !string.IsNullOrWhiteSpace(_productName) &&
        _quantity.HasValue && _quantity >= 1 &&
        _unitPrice.HasValue && _unitPrice >= 0 &&
        _orderDate.HasValue &&
        _deliveryDate.HasValue &&
        _deliveryDate.Value > _orderDate.Value; // 組み合わせバリデーション

    // 単方向バインディング
    private void OnProductNameChanged(string value)
    {
        _productName = value;
        _isRegistered = false;
    }

    private void OnQuantityChanged(int? value)
    {
        _quantity = value;
        CalculateAmounts(); // 金額を再計算
        _isRegistered = false;
    }

    private void OnUnitPriceChanged(decimal? value)
    {
        _unitPrice = value;
        CalculateAmounts(); // 金額を再計算
        _isRegistered = false;
    }

    private void OnOrderDateChanged(DateTime? value)
    {
        _orderDate = value;
        _isRegistered = false;
    }

    private void OnDeliveryDateChanged(DateTime? value)
    {
        _deliveryDate = value;
        _isRegistered = false;
    }

    // 値の連動：自動計算
    private void CalculateAmounts()
    {
        if (_quantity.HasValue && _unitPrice.HasValue)
        {
            _amount = _quantity.Value * _unitPrice.Value;
            _taxAmount = _amount * TaxRate;
            _totalAmount = _amount + _taxAmount;
        }
        else
        {
            _amount = 0;
            _taxAmount = 0;
            _totalAmount = 0;
        }
    }

    private void OnRegister()
    {
        if (IsValid)
        {
            _isRegistered = true;
            // 実際はAPIコール等
        }
    }
}
```

:::

テストを書く前に、Radzenコンポーネントを使う場合は以下のセットアップが必要です。

```csharp
public class OrderEntryFormTests : TestContext
{
    public OrderEntryFormTests()
    {
        // Radzenコンポーネントを使用するため必須
        Services.AddRadzenComponents();

        // Radzenコンポーネントが使用するJSInteropをモック
        JSInterop.Mode = JSRuntimeMode.Loose;
    }
}
```

コンポーネントが正しくレンダリングされ、登録ボタンが無効になっているかを確認します。

```csharp
[Fact]
public void OrderEntryForm_初期状態_登録ボタンが無効になっている()
{
    // Arrange & Act
    var cut = RenderComponent<OrderEntryForm>();

    // Assert: 登録ボタンがdisabled
    var registerButton = cut.FindComponent<RadzenButton>();
    Assert.True(registerButton.Instance.Disabled);
}
```

- `RenderComponent<T>()` でコンポーネントをレンダリング
- `FindComponent<T>()` でRadzenコンポーネントを取得
- `Instance.Disabled` でプロパティを検証


以下の例では、値の連動テストをしています。
数量と単価を入力すると、金額が自動計算されることを確認します。

```csharp
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
```
- `FindComponents<T>()` で複数の同じ型のコンポーネントを取得
- `ValueChanged.InvokeAsync()` で単方向バインディングのイベントを発火
- `FindAll()` でCSSセレクタで要素を検索
- `TextContent` でテキスト内容を検証



「注文日と納期の組み合わせ」をチェックするバリデーションをテストします。単純な必須チェックは`[Required]`属性で済みますが、bUnitなら「納期 > 注文日」のような「複数項目の組み合わせバリデーション」もテスト可能です。

この2つのテストは、登録ボタンが「注文日と納期によって有効・無効が切り替わること」を検証しています。

```csharp
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

        // Act: 注文日と納期を設定（納期が前）
        var datePickers = cut.FindComponents<RadzenDatePicker<DateTime?>>();
        await datePickers[0].Instance.ValueChanged.InvokeAsync(new DateTime(2024, 3, 15)); // 注文日
        await datePickers[1].Instance.ValueChanged.InvokeAsync(new DateTime(2024, 3, 10)); // 納期（前）
    });

    // Assert: 登録ボタンが無効（納期が注文日より前のため）
    var registerButton = cut.FindComponent<RadzenButton>();
    Assert.True(registerButton.Instance.Disabled);
}
```

```csharp
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
    cut.WaitForState(() => !cut.FindComponent<RadzenButton>().Instance.Disabled);
    Assert.False(cut.FindComponent<RadzenButton>().Instance.Disabled);
}
```

- `FindComponents<RadzenDatePicker<DateTime?>>()` で日付入力コンポーネントを取得
- `WaitForState()` で状態が変わるまで待機（非同期処理対応）


以下は、登録ボタンをクリックすると、成功メッセージが表示されることを確認するテストです。

```csharp
[Fact]
public async Task OrderEntryForm_登録ボタンクリック_成功メッセージが表示される()
{
    // Arrange
    var cut = RenderComponent<OrderEntryForm>();

    // すべての項目を入力
    await cut.InvokeAsync(async () =>
    {
        var textBox = cut.FindComponent<RadzenTextBox>();
        await textBox.Instance.ValueChanged.InvokeAsync("モニター");

        var numerics = cut.FindComponents<RadzenNumeric<int?>>();
        await numerics[0].Instance.ValueChanged.InvokeAsync(2);

        var decimalNumerics = cut.FindComponents<RadzenNumeric<decimal?>>();
        await decimalNumerics[0].Instance.ValueChanged.InvokeAsync(35000m);

        var datePickers = cut.FindComponents<RadzenDatePicker<DateTime?>>();
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
    Assert.Contains("77,000", successMessage.TextContent); // 35000*2*1.1=77000
}
```
- `Find("button").Click()` でボタンクリックをシミュレート
- 条件付きレンダリング（`@if`）の検証
- 計算結果の検証（金額 × 1.1 = 消費税込み合計）

## まとめ

実際に使ってみた結果、bUnitは以下のような用途で有効だと感じました

- ロジック検証: `ValueChanged` による値の連動や自動計算
- 複数項目の関係性チェック（`Required`属性では不可能な組み合わせバリデーション）
- イベント発火やプロパティ変更の検証

各ツールには住み分けがあり、bUnitは「xUnitでは難しいコンポーネントレベルの検証」と「Playwrightほど重くないUI検証」の中間を担うツールとして価値があります。

導入コストも低いため、プロダクトのコアな機能に限って部分導入するなど、柔軟な判断が良いのではと思います。

## サンプルコード

今回の記事で使用したサンプルプロジェクトはGitHubで公開しています。

https://github.com/pixcelo/zenn-content/tree/main/samples/blazor-radzen-bunit-testing


## 参考

- [bUnit - Testing Blazor Components](https://bunit.dev/)
- [xUnit - Unit Testing Tool for .NET](https://xunit.net/)
- [Playwright - End-to-End Testing](https://playwright.dev/)
