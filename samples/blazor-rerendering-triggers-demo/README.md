# Blazor再描画トリガー検証デモ

このプロジェクトは、Zenn記事「[Blazorはいつ再描画されるのか?](https://zenn.dev/nexta_/articles/blazor-rerendering-triggers)」のサンプルコードです。

Blazorの再描画タイミングを実際に確認できるデモアプリケーションです。

## 📋 検証内容

### 1. 自動再描画の検証
- **AutoRerenderDemo**: イベントハンドラー、パラメーター更新、ライフサイクルメソッドでの自動再描画

### 2. 手動StateHasChangedが必要なケース
- **AsyncProgressDemo**: 非同期処理の進捗表示（ループ内でのStateHasChanged）
- **TimerDemo**: System.Timers.TimerとInvokeAsync
- **StateContainerDemo**: DIサービスからの通知（StateContainerパターン）
- **InvokeAsyncComparisonDemo**: `InvokeAsync(StateHasChanged)` vs `InvokeAsync(() => StateHasChanged())` の比較

### 3. 最適化技術
- **ShouldRenderDemo**: ShouldRenderのオーバーライドによる再描画制御
- **KeyDirectiveDemo**: @keyディレクティブによるリスト要素の最適化

### 4. アンチパターン
- **InfiniteLoopDemo**: OnAfterRenderでの無限ループ（安全版）

## 🚀 実行方法

### 必要な環境
- .NET 8 SDK

### 実行手順

1. リポジトリのクローン（またはZIPダウンロード）
```bash
git clone https://github.com/yourusername/zenn-content.git
cd zenn-content/samples/blazor-rerendering-triggers-demo
```

2. プロジェクトの復元とビルド
```bash
cd BlazorRerenderingTriggersDemo
dotnet restore
dotnet build
```

3. アプリケーションの起動
```bash
dotnet run
```

4. ブラウザで以下のURLにアクセス
```
https://localhost:5001
```

または

```
http://localhost:5000
```

## 📖 使い方

1. ホームページから各デモページへのリンクをクリック
2. 各デモページでボタンをクリックして再描画の動作を確認
3. 「動かないコード」と「正しいコード」を比較して違いを体感

## 🔍 ポイント

### InvokeAsyncの2つの書き方について

記事で疑問に思われていた以下の2つの書き方について、このデモで検証できます：

1. `await InvokeAsync(StateHasChanged)` - メソッドグループ構文
2. `await InvokeAsync(() => StateHasChanged())` - ラムダ式

**結論**: どちらも機能的に同じです。

- メソッドグループ構文（パターン1）の方が簡潔で読みやすい
- ラムダ式（パターン2）は複数のステートメントを実行する場合に便利
- どちらを使用しても、UIスレッドで安全にStateHasChanged()を呼び出せる

詳細は `/invoke-async-comparison` ページで確認できます。

## 📂 プロジェクト構造

```
BlazorRerenderingTriggersDemo/
├── Components/
│   ├── Pages/
│   │   ├── Home.razor                      # メインページ
│   │   ├── AutoRerenderDemo.razor          # 自動再描画デモ
│   │   ├── AsyncProgressDemo.razor         # 非同期処理デモ
│   │   ├── TimerDemo.razor                 # タイマーデモ
│   │   ├── StateContainerDemo.razor        # StateContainerデモ
│   │   ├── InvokeAsyncComparisonDemo.razor # InvokeAsync比較デモ
│   │   ├── ShouldRenderDemo.razor          # ShouldRenderデモ
│   │   ├── KeyDirectiveDemo.razor          # @keyデモ
│   │   └── InfiniteLoopDemo.razor          # 無限ループデモ
│   ├── ChildComponent.razor                # パラメーター更新検証用
│   └── StateContainerChildComponent.razor  # StateContainer検証用
├── Services/
│   └── AppState.cs                         # StateContainerパターン実装
└── Program.cs                              # アプリケーション設定
```

## 📝 関連記事

- [Blazorはいつ再描画されるのか?](https://zenn.dev/nexta_/articles/blazor-rerendering-triggers)
- [Blazorのレンダリングの仕組みとコンポーネントのライフサイクル](https://zenn.dev/nexta_/articles/blazor-component-lifecycle)
- [Blazorのデータフローとコンポーネント連携](https://zenn.dev/nexta_/articles/blazor-databinding)

## 📄 ライセンス

MIT License
