# Blazorライフサイクル検証プロジェクト

## 概要
Blazorコンポーネントのライフサイクルメソッドの動作を検証するためのサンプルプロジェクトです。

## 検証できるライフサイクルメソッド

### 初期化フェーズ
- `OnInitialized()` / `OnInitializedAsync()`
  - コンポーネントが初期化される際に呼ばれる
  - **プリレンダリング有効時は2回実行される**（プリレンダリング時とSignalR接続後）
  - 非同期版は同期版の後に実行される

### パラメータ設定フェーズ
- `OnParametersSet()` / `OnParametersSetAsync()`
  - 初回レンダリング時と、パラメータが変更された時に呼ばれる
  - 親コンポーネントから渡されるパラメータの変更を検知

### レンダリング後フェーズ
- `OnAfterRender(bool firstRender)` / `OnAfterRenderAsync(bool firstRender)`
  - コンポーネントのレンダリングが完了した後に呼ばれる
  - `firstRender` で初回レンダリングかどうかを判定可能
  - JavaScript相互運用が必要な場合はこのタイミングで実行

### 破棄フェーズ
- `Dispose()` / `DisposeAsync()`
  - コンポーネントが破棄される際に呼ばれる
  - リソースのクリーンアップに使用

## プロジェクト構成

```
BlazorLifecycleDemo/
├── Components/
│   ├── Pages/
│   │   ├── LifecycleDemo.razor        # メインの検証ページ
│   │   ├── ChildComponent.razor       # 子コンポーネント
│   │   ├── Sync/                       # 同期ライフサイクル検証
│   │   ├── Async/                      # 非同期ライフサイクル検証
│   │   └── Mixed/                      # 混合ライフサイクル検証
│   └── Layout/
│       └── NavMenu.razor               # ナビゲーションメニュー
├── Program.cs                          # エントリーポイント
└── BlazorLifecycleDemo.csproj         # プロジェクトファイル
```

## 実行方法

### 1. プロジェクトのビルド
```bash
cd samples/blazor-lifecycle-demo/BlazorLifecycleDemo
dotnet build
```

### 2. アプリケーションの起動
```bash
dotnet run
```

### 3. ブラウザでアクセス
デフォルトでは `https://localhost:5001` または `http://localhost:5000` で起動します。

### 4. ライフサイクルデモページへ移動
ナビゲーションメニューから「Lifecycle Demo」をクリック

### 5. 開発者ツールでログを確認
- ブラウザで F12 キーを押して開発者ツールを開く
- コンソールタブでライフサイクルメソッドの実行順序を確認

## 検証シナリオ

### 1. 初回レンダリング時のライフサイクル

プリレンダリング有効時、ライフサイクルメソッドは2回実行されます。

実際のログ出力を確認するには：
1. アプリケーションを起動
2. ブラウザで開発者ツール（F12）を開く
3. コンソールタブでライフサイクルメソッドの実行順序を確認

**重要なポイント**:
- `OnInitialized` / `OnInitializedAsync` / `OnParametersSet` / `OnParametersSetAsync` は2回実行される
- `OnAfterRender` / `OnAfterRenderAsync` はプリレンダリング時には実行されず、SignalR接続後のみ実行される

### 2. 状態変更による再レンダリング
「カウントアップ」ボタンをクリックして状態を変更：
- `OnParametersSet` / `OnParametersSetAsync` は呼ばれない
- `OnAfterRender(firstRender=false)` のみ実行される

### 3. パラメータ変更による再レンダリング
メッセージ入力欄を変更して子コンポーネントのパラメータを更新：
- 子コンポーネントの `OnParametersSet` / `OnParametersSetAsync` が実行される
- 続いて `OnAfterRender(firstRender=false)` が実行される

### 4. 明示的な再レンダリング
「再レンダリング」ボタンをクリックして `StateHasChanged()` を実行：
- パラメータ変更がなくても再レンダリングが発生
- `OnAfterRender` / `OnAfterRenderAsync` が実行される

### 5. コンポーネントの破棄
別のページに移動してコンポーネントを破棄：
- `Dispose()` メソッドが呼ばれることを確認

## 技術仕様

- **.NET**: 10.0 Preview
- **フレームワーク**: Blazor Server (Interactive Server Mode)
- **ログ出力**: `ILogger<T>` を使用してコンソールに出力

## ログの見方

各ログには以下の情報が含まれます：
```
[HH:mm:ss.fff] メソッド名(引数) - 詳細
```

例：
```
[14:30:25.123] OnInitializedAsync() - 非同期版
[14:30:25.224] OnParametersSet() - Message=初期メッセージ
[14:30:25.335] OnAfterRender(firstRender=true) - 同期版
```

## 注意事項

1. **プリレンダリングによる二重実行**
   - このプロジェクトはプリレンダリングが有効です
   - `OnInitialized` / `OnInitializedAsync` / `OnParametersSet` / `OnParametersSetAsync` は2回実行されます
   - DB接続やAPI呼び出しは `OnAfterRender(firstRender == true)` で実行することを推奨

2. **Server-side Blazorの動作**
   - このプロジェクトはBlazor Serverモードで動作します
   - SignalR接続を介してサーバーと通信します

3. **ログの出力場所**
   - サーバー側のログ: コンソールウィンドウ（`dotnet run` を実行した場所）
   - クライアント側のログ: ブラウザの開発者ツールのコンソール

4. **非同期処理**
   - 各非同期メソッド内で `Task.Delay()` を使用して処理時間を可視化
   - 実際の非同期処理（API呼び出しなど）の動作をシミュレート

## ブログ記事での活用方法

このサンプルプロジェクトは以下の内容をカバーしています：

1. ✅ 各ライフサイクルメソッドの実行順序
2. ✅ 同期版と非同期版の違い
3. ✅ 親子コンポーネント間のライフサイクル連携
4. ✅ パラメータ変更時の動作
5. ✅ 再レンダリングのトリガー
6. ✅ コンポーネントの破棄処理

## 参考資料

- [ASP.NET Core Blazor ライフサイクル](https://learn.microsoft.com/ja-jp/aspnet/core/blazor/components/lifecycle)
- [Blazor Server とは](https://learn.microsoft.com/ja-jp/aspnet/core/blazor/hosting-models#blazor-server)
