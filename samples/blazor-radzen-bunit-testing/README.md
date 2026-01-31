# Blazor Radzen bUnit Testing サンプルプロジェクト

このプロジェクトは、Blazor ServerアプリケーションでRadzen Blazorコンポーネントを使用し、bUnitでテストを実行する方法を示すサンプルです。

## プロジェクト構成

```
blazor-radzen-bunit-testing/
├── BlazorRadzenApp/                    # Blazor Serverアプリケーション
│   ├── Components/
│   │   └── CustomComponents/          # カスタムコンポーネント
│   │       ├── SimpleCounter.razor    # HTMLベースのカウンター
│   │       ├── UserForm.razor         # HTMLベースのフォーム
│   │       ├── RadzenButtonDemo.razor # Radzenボタンデモ
│   │       ├── RadzenDataGridDemo.razor # RadzenDataGridデモ
│   │       └── RadzenDialogDemo.razor # Radzenダイアログデモ
│   └── Program.cs
├── BlazorRadzenApp.Tests/             # bUnitテストプロジェクト
│   ├── SimpleCounterTests.cs          # HTMLコンポーネントのテスト
│   ├── UserFormTests.cs               # HTMLフォームのテスト
│   ├── RadzenButtonDemoTests.cs       # Radzenボタンのテスト
│   ├── RadzenDataGridDemoTests.cs     # RadzenDataGridのテスト
│   └── RadzenDialogDemoTests.cs       # Radzenダイアログのテスト
└── BlazorRadzenBunitTesting.sln       # ソリューションファイル
```

## 使用技術

- **.NET 9.0**
- **Blazor Server** - インタラクティブなWebアプリケーションフレームワーク
- **Radzen.Blazor 8.7.4** - UIコンポーネントライブラリ
- **bUnit 1.31.3** - Blazorコンポーネントのテストフレームワーク
- **xUnit** - テストランナー

## セットアップ

### 前提条件

- .NET 9.0 SDK 以上

### プロジェクトのビルド

```bash
cd samples/blazor-radzen-bunit-testing
dotnet restore
dotnet build
```

## アプリケーションの実行

```bash
cd BlazorRadzenApp
dotnet run
```

アプリケーションは `https://localhost:5001` で起動します。

## テストの実行

### すべてのテストを実行

```bash
cd BlazorRadzenApp.Tests
dotnet test
```

### 詳細な出力でテストを実行

```bash
dotnet test --verbosity normal
```

### 特定のテストクラスのみ実行

```bash
dotnet test --filter "FullyQualifiedName~SimpleCounterTests"
dotnet test --filter "FullyQualifiedName~RadzenButtonDemoTests"
```

## コンポーネント説明

### HTMLベースのコンポーネント

#### 1. SimpleCounter
シンプルなカウンターコンポーネント。基本的なイベント処理とパラメータバインディングを実装。

**主な機能:**
- カウントアップ
- リセット機能
- カスタムタイトルと初期値のサポート
- イベントコールバック

#### 2. UserForm
ユーザー情報入力フォーム。フォームバリデーションと送信処理を実装。

**主な機能:**
- 双方向データバインディング
- バリデーション（必須入力チェック）
- チェックボックス
- 送信後の成功メッセージ表示

### Radzenコンポーネントを使用したコンポーネント

#### 3. RadzenButtonDemo
Radzenのボタンコンポーネントを使用したデモ。

**主な機能:**
- 複数のボタンスタイル（Primary, Success, Warning）
- アイコン付きボタン
- クリックイベント処理

#### 4. RadzenDataGridDemo
Radzenのデータグリッドコンポーネントを使用した商品一覧。

**主な機能:**
- データバインディング
- フィルタリング機能
- ソート機能
- ページング（5件/ページ）
- カスタムテンプレート（価格フォーマット、在庫表示）

#### 5. RadzenDialogDemo
Radzenのダイアログサービスを使用したデモ。

**主な機能:**
- カスタムダイアログの表示
- 確認ダイアログ
- ダイアログの結果処理

## テスト戦略

### HTMLコンポーネントのテスト

HTMLベースのコンポーネント（SimpleCounter, UserForm）では以下をテスト:

1. **レンダリングテスト** - コンポーネントが正しく表示されるか
2. **パラメータテスト** - パラメータが正しく反映されるか
3. **イベントテスト** - ボタンクリックなどのイベントが正しく動作するか
4. **状態管理テスト** - コンポーネントの状態が正しく更新されるか
5. **バリデーションテスト** - フォームバリデーションが正しく機能するか

### Radzenコンポーネントのテスト

Radzenコンポーネント（RadzenButtonDemo, RadzenDataGridDemo, RadzenDialogDemo）では以下をテスト:

1. **サービス登録** - `Services.AddRadzenComponents()` の呼び出し
2. **コンポーネントレンダリング** - Radzenコンポーネントが正しくレンダリングされるか
3. **データバインディング** - データが正しく表示されるか
4. **イベント処理** - Radzenコンポーネントのイベントが正しく動作するか
5. **マークアップ検証** - Radzen固有のCSSクラスやHTML構造の確認

### テストパターン

各テストは **AAA (Arrange-Act-Assert)** パターンに従っています:

```csharp
[Fact]
public void コンポーネント名_テストシナリオ_期待される結果()
{
    // Arrange - テストの準備
    var cut = RenderComponent<MyComponent>();

    // Act - テスト対象の操作
    var button = cut.Find("button");
    button.Click();

    // Assert - 結果の検証
    Assert.Equal(expected, actual);
}
```

## 学習ポイント

### bUnitの基本

1. **TestContext** - すべてのテストクラスが継承する基底クラス
2. **RenderComponent** - コンポーネントをレンダリングするメソッド
3. **Find / FindAll** - DOM要素を検索するメソッド
4. **MarkupMatches** - HTML構造を検証するメソッド
5. **Services** - DIコンテナへのアクセス

### Radzenとbunitの統合

```csharp
public class RadzenComponentTests : TestContext
{
    public RadzenComponentTests()
    {
        // Radzenサービスの登録が必要
        Services.AddRadzenComponents();

        // DialogServiceが必要な場合
        Services.AddScoped<DialogService>();
    }
}
```

### イベントテスト

```csharp
// ボタンクリックのシミュレート
var button = cut.Find("button");
button.Click();

// 入力値の変更
var input = cut.Find("input");
input.Change("新しい値");

// チェックボックスの変更
var checkbox = cut.Find("input[type='checkbox']");
checkbox.Change(true);
```

## トラブルシューティング

### よくある問題

1. **Radzenコンポーネントが見つからない**
   - `Services.AddRadzenComponents()` を呼び出しているか確認

2. **DialogServiceが利用できない**
   - `Services.AddScoped<DialogService>()` を追加

3. **CSSクラスが適用されない**
   - bUnitはJavaScriptやCSSを実行しないため、クラスの存在確認のみ可能

## 参考リンク

- [bUnit公式ドキュメント](https://bunit.dev/)
- [Radzen Blazor公式](https://blazor.radzen.com/)
- [Blazor公式ドキュメント](https://docs.microsoft.com/aspnet/core/blazor)

## ライセンス

このサンプルプロジェクトはMITライセンスの下で公開されています。
