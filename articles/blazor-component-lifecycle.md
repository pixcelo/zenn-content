---
title: "Blazorで画面が表示される仕組みとコンポーネントのライフサイクル"
emoji: "🔄"
type: "tech" # tech: 技術記事 / idea: アイデア
topics: ["blazor", "csharp", "dotnet", "web", "web開発"]
published: false
publication_name: "nextanext"
---

ネクスタの tetsu.k です。
生産管理アプリ「スマートF」の開発に携わっています。

今回は、C#のBlazorについて調べました。
主にBlazorのライフサイクルに焦点を当てます。

## はじめに

Blazorでは、画面をコンポーネントという部品の組み合わせで構築します。

各コンポーネントには「生まれる → 動く → 消える」というライフサイクルがあり、
そのタイミングで自動的にメソッドが呼ばれます。

この記事では、どのタイミングでどのメソッドが呼ばれるのかを解説します。


## Blazor Serverで画面が表示される仕組み

Blazor Serverでは、以下の流れで画面が表示されます。

```mermaid
sequenceDiagram
    autonumber
    actor User as ユーザー
    participant Browser as ブラウザ
    participant Server as サーバー (Blazor Server)

    Note over User, Server: 1. 初回アクセス時 (プリレンダリング)
    User->>Browser: URLにアクセス
    Browser->>Server: HTTPリクエスト
    Server->>Server: 静的HTML生成<br/>(プリレンダリング)
    Server-->>Browser: 静的HTMLを返却
    Browser->>User: 画面表示<br/>(まだ動的な操作は不可)

    Note over Browser, Server: 2. リアルタイム接続の確立
    Browser->>Server: SignalR 接続開始
    Server->>Server: コンポーネントの再生成・初期化
    Server-->>Browser: 接続完了
    Browser->>Browser: DOMを動的更新可能な状態へ

    Note over User, Server: 3. ユーザー操作時 (イベントループ)
    User->>Browser: ボタンクリック等の操作
    Browser->>Server: イベント送信
    Server->>Server: 処理実行 <br/>DOMの差分計算
    Server-->>Browser: DOM更新指示
    Browser->>Browser: DOM反映
    Browser->>User: 画面更新
```

Blazor Serverでは、まず静的HTML（プリレンダリング）を表示し、次にSignalRで接続して動的に操作できるようになります。

## コンポーネントのライフサイクル

上記の各段階で、コンポーネント内では以下のメソッドが呼ばれます：

```mermaid
sequenceDiagram
    participant Blazor
    participant Component
    participant Browser

    note right of Blazor: 1. プリレンダリング時
    Blazor->>Component: SetParametersAsync()
    Component->>Component: OnInitialized(Async)()
    Component->>Component: OnParametersSet(Async)()
    Component->>Browser: BuildRenderTree()
    Browser-->>Blazor: 静的HTML

    note right of Blazor: 2. SignalR接続後
    Blazor->>Component: SetParametersAsync() (2回目)
    Component->>Component: OnInitialized(Async)() (2回目)
    Component->>Component: OnParametersSet(Async)() (2回目)
    Component->>Browser: BuildRenderTree()
    Browser->>Component: OnAfterRender(Async)(firstRender: true)
    Browser-->>Blazor: 対話型DOM更新

    note right of Blazor: 3. ユーザー操作時
    Blazor->>Component: イベントハンドラ実行
    Component->>Component: StateHasChanged()
    Component->>Component: ShouldRender()?
    alt ShouldRender = true
        Component->>Browser: BuildRenderTree()
        Browser->>Component: OnAfterRender(Async)(firstRender: false)
        Browser-->>Blazor: DOM差分更新
    end
```

シーケンス図のとおり、コンポーネントには「生まれる→動く→消える」のタイミングでメソッドが自動的に呼ばれます。

よく使うのは以下の4つです。
- OnInitialized - 初期化
- OnParametersSet - パラメータ受け取り後の処理
- OnAfterRender - 画面表示後の処理（動的行へのデータ設定など）
- StateHasChanged - 画面の再描画

詳しい実行順序は上のシーケンス図を参照してください。

※プリレンダリング：初回アクセス時にサーバーが静的HTMLを事前生成する仕組み

:::message alert
プリレンダリング有効時、`OnInitialized(Async)` は2回実行されます。
DB接続やAPI呼び出しを1回だけ実行したい場合は、`OnAfterRender(firstRender)` で `firstRender == true` の時のみ実行するようにします。
:::

すべてのrazorコンポーネントは、基本的にComponentBase（基底クラス）を継承しています。
ライフサイクルの各メソッドは、ComponentBaseの定義から確認が可能です。

これらのメソッドの動きやタイミングを理解することが、
Blazor開発の第一歩となります。

## サンプル

サンプルプロジェクトを用意しました。

[GitHubサンプルコード](https://github.com/pixcelo/zenn-content/tree/main/samples/blazor-lifecycle-demo)

各ライフサイクルメソッドの実行順序をログで確認できます。


## 参考リンク
- [ASP.NET Core Razor コンポーネントのライフサイクル](https://learn.microsoft.com/ja-jp/aspnet/core/blazor/components/lifecycle)
- [ASP.NET Core Razor コンポーネントのプリレンダリング](https://learn.microsoft.com/ja-jp/aspnet/core/blazor/components/prerender)
- [ASP.NET Core Razor コンポーネント](https://learn.microsoft.com/ja-jp/aspnet/core/blazor/components/)