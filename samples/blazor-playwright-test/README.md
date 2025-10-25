# Blazor + Radzen + Playwright MCP テスト環境

Blazor Server、Radzen Blazor Components、Playwright MCPを組み合わせた、AIエージェントによる自動テストの検証環境です。

## 概要

このプロジェクトは、以下の技術スタックを使用して構築されています：

- **.NET 8** - Blazor Serverアプリケーション
- **Radzen Blazor Components** - リッチなUIコンポーネントライブラリ
- **Playwright MCP** - AIエージェントによるブラウザ自動化

## プロジェクト構成

```
blazor-playwright-test/
├── BlazorRadzenApp/              # Blazor Serverアプリケーション
│   ├── Components/
│   │   ├── Pages/
│   │   │   ├── TestForm.razor    # テスト用フォームページ
│   │   │   ├── DataGrid.razor    # データグリッドページ
│   │   │   └── Dashboard.razor   # ダッシュボードページ
│   │   ├── Layout/
│   │   │   └── MainLayout.razor  # Radzenコンポーネント配置
│   │   └── App.razor
│   ├── wwwroot/
│   ├── Program.cs                # Radzenサービス登録済み
│   └── BlazorRadzenApp.csproj
├── playwright-config.json        # Playwright MCP設定
├── TEST_SCENARIOS.md            # 10個のテストシナリオ集
└── README.md                    # このファイル
```

## セットアップ

### 前提条件

- .NET 8 SDK 以降
- Node.js (Playwright MCP用)
- Claude Code または MCP対応AIアシスタント

### 1. アプリケーションのビルドと実行

```bash
cd BlazorRadzenApp
dotnet restore
dotnet build
dotnet run
```

アプリケーションは `https://localhost:7000` (または `http://localhost:5000`) で起動します。

### 2. Playwright MCPの設定

Claude Codeを使用している場合：

```bash
# プロジェクトディレクトリで
claude mcp add playwright-mcp
```

または、`playwright-config.json` をMCP設定ファイルに追加してください。

### 3. 動作確認

ブラウザで `http://localhost:5000/test-form` にアクセスし、フォームが表示されることを確認します。

## 機能

### Test Formページ

Radzen Blazor Componentsを使用したサンプルフォームページです：

- **テキスト入力**: Name, Email
- **数値入力**: Age (0-120)
- **ドロップダウン**: Country選択
- **チェックボックス**: Newsletter購読
- **テキストエリア**: Comments
- **バリデーション**: 必須項目チェック、メール形式検証
- **送信/クリア**: フォーム送信とリセット機能

### Data Gridページ

Radzen DataGridを使用した商品一覧管理ページです：

- **データ表示**: 12件のサンプル商品データ（初回自動読み込み）
- **フィルタリング**: 各列でのデータ絞り込み
- **ソート**: 列ヘッダークリックで昇順/降順切り替え
- **ページング**: 10件ごとのページ表示
- **編集機能**: 行ごとの編集フォーム
- **削除機能**: 商品の削除（即座にグリッド更新）
- **バッジ表示**: 在庫状況の視覚的表示（色分け）
- **データ操作**: サンプルデータ追加/全削除/CSV出力
- **成功メッセージ**: 全操作で視覚的フィードバック

### Dashboardページ

業務アプリケーションでよく使われるRadzenコンポーネントを集約したダッシュボードです：

**Overviewタブ**
- **KPIカード**: 売上、ユーザー数、タスク、収益の4つのメトリクス
- **棒グラフ**: 月次売上推移（RadzenChart）
- **円グラフ**: カテゴリ別売上分布（RadzenPieSeries）

**Progress Trackingタブ**
- **プログレスバー**: 4つのプロジェクト進捗（色分け）
- **ファイルアップロード**: シミュレーション機能

**Organizationタブ**
- **ツリー表示**: 3階層の部門構造（IT/Sales/HR）

**Notificationsタブ**
- **通知テスト**: Success/Info/Warning/Error の4種類
- **ダイアログ**: 通常ダイアログと確認ダイアログ

### 使用しているRadzen Components

**フォーム関連**
- `RadzenCard` - コンテナ
- `RadzenTemplateForm` - フォーム
- `RadzenFormField` - フォームフィールド
- `RadzenTextBox` - テキスト入力
- `RadzenNumeric` - 数値入力
- `RadzenDropDown` - ドロップダウン選択
- `RadzenCheckBox` - チェックボックス
- `RadzenTextArea` - テキストエリア
- `RadzenButton` - ボタン
- `RadzenAlert` - アラート表示
- バリデーター (`RadzenRequiredValidator`, `RadzenEmailValidator`)

**DataGrid関連**
- `RadzenDataGrid` - データグリッド
- `RadzenDataGridColumn` - グリッド列定義
- `RadzenBadge` - バッジ表示
- `RadzenStack` - レイアウトスタック

**Dashboard関連**
- `RadzenTabs` / `RadzenTabsItem` - タブ切り替え
- `RadzenChart` - グラフ表示
- `RadzenColumnSeries` - 棒グラフ
- `RadzenPieSeries` - 円グラフ
- `RadzenProgressBar` - 進捗バー
- `RadzenTree` - ツリー表示
- `RadzenDialog` - ダイアログ
- `RadzenNotification` - 通知（トースト）
- `RadzenIcon` - アイコン
- `RadzenRow` / `RadzenColumn` - レスポンシブグリッド
- `RadzenText` - テキスト表示

## AIエージェントによるテスト

### Playwright MCPの使い方

Claude Codeなどのツールで、自然言語でテスト指示を出すことができます：

**例1: 基本的なフォーム入力**
```
http://localhost:5000/test-form にアクセスして、
Nameに "John Doe"、Emailに "john@example.com" を入力し、
Submitボタンをクリックしてください
```

**例2: バリデーションテスト**
```
Test Formページで、何も入力せずにSubmitボタンを押して、
エラーメッセージが表示されることを確認してください
```

**例3: フォームのリセット**
```
フォームに適当な値を入力した後、Clearボタンをクリックして、
すべてのフィールドがリセットされることを確認してください
```

詳細なテストシナリオは [TEST_SCENARIOS.md](TEST_SCENARIOS.md) を参照してください。

## テストシナリオ

以下の10個のテストシナリオが用意されています：

**Test Form関連**
1. **フォームの基本入力テスト** - すべてのフィールドへの入力と送信
2. **バリデーションテスト** - 必須項目とメール形式の検証
3. **クリア機能のテスト** - フォームリセット機能の確認
4. **ナビゲーションテスト** - ページ遷移の確認
5. **Radzen Componentsの動作確認** - UIコンポーネントの動作検証

**Data Grid関連**
6. **DataGridの表示とフィルタリング** - データ表示とフィルタ機能
7. **DataGridのソートとページング** - ソートとページ切り替え
8. **DataGridの編集機能** - 行の編集とデータ更新
9. **DataGridの削除機能** - データ削除機能
10. **DataGridのデータ操作** - サンプルデータ追加/クリア/エクスポート

各シナリオの詳細は [TEST_SCENARIOS.md](TEST_SCENARIOS.md) を参照してください。

## アーキテクチャ

### Blazor Server

- サーバーサイドでUIロジックを実行
- SignalR経由でクライアントと通信
- リアルタイムなUI更新

### Radzen Blazor Components

- 70以上のUIコンポーネント
- Material Designテーマ
- 組み込みバリデーション機能

### Playwright MCP

- MCP (Model Context Protocol) によるAIエージェント統合
- Playwrightによるブラウザ自動化
- 自然言語でのテスト記述

## トラブルシューティング

### アプリケーションが起動しない

- .NET 8 SDKがインストールされているか確認: `dotnet --version`
- ポート競合を確認: 5000番ポートが使用中でないか確認

### Playwright MCPが動作しない

- Node.jsがインストールされているか確認: `node --version`
- MCPサーバーの設定を確認: `playwright-config.json`

### テストが失敗する

- アプリケーションが起動しているか確認
- 正しいURLにアクセスしているか確認（ポート番号など）
- Blazorの動的レンダリングの完了を待つ

## 参考資料

- [Blazor Documentation](https://learn.microsoft.com/ja-jp/aspnet/core/blazor/)
- [Radzen Blazor Components](https://blazor.radzen.com/)
- [Playwright](https://playwright.dev/)
- [Model Context Protocol](https://modelcontextprotocol.io/)

## ライセンス

このサンプルプロジェクトはMITライセンスの下で公開されています。

## 次のステップ

1. `dotnet run` でアプリケーションを起動
2. ブラウザで動作を確認
3. [TEST_SCENARIOS.md](TEST_SCENARIOS.md) のシナリオを参考に、AIエージェントでテストを実行
4. 独自のテストケースを追加して検証

Happy Testing! 🚀
