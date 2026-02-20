# Blazor データバインディング サンプル

このプロジェクトは、Blazor (.NET 8) のデータバインディングの仕組みを学習・検証するためのサンプルアプリケーションです。

## 概要

Blazorにおけるデータバインディングのさまざまなパターンを実際に動かして確認できます。

### 検証項目

#### 基礎編
1. **基本的なバインディング** - 単方向バインディング、HTML属性へのバインディング、条件付きレンダリング
2. **双方向バインディング** - @bind、@bind:event、@bind:after の使い方
3. **イベントバインディング** - マウス・キーボード・フォームイベント、イベント伝播制御
4. **フォームバインディング** - 各種input要素とのバインディング

#### 応用編
5. **コンポーネント間バインディング** - 親子コンポーネント間のデータ受け渡し
6. **EditFormバインディング** - データ検証付きフォーム

#### 重要概念（必修）
7. **値型 vs 参照型** ⭐ - バインディングにおける値型と参照型の違いを実験
8. **プリレンダリング検証** - ライフサイクル、JavaScript相互運用の制限

## プロジェクト構成

```
BlazorDataBindingSample/
├── Components/
│   ├── Pages/
│   │   ├── Home.razor                        # トップページ（学習ガイド）
│   │   ├── BasicBinding.razor                # 基本的なバインディング
│   │   ├── TwoWayBinding.razor               # 双方向バインディング
│   │   ├── EventBinding.razor                # イベントバインディング
│   │   ├── FormBinding.razor                 # フォーム要素のバインディング
│   │   ├── ComponentBinding.razor            # コンポーネント間のバインディング
│   │   ├── EditFormBinding.razor             # EditFormでの検証付きバインディング
│   │   ├── ValueTypeVsReferenceType.razor    # 値型vs参照型の実験 ⭐重要
│   │   └── PrerenderingTest.razor            # プリレンダリング動作確認
│   └── Shared/
│       ├── ChildComponent.razor              # 子コンポーネントサンプル
│       ├── CustomInput.razor                 # カスタム入力コンポーネント
│       ├── ValueTypeTestComponent.razor      # 値型バインディングテスト用
│       └── ReferenceTypeTestComponent.razor  # 参照型バインディングテスト用
├── Models/
│   ├── PersonModel.cs                        # EditForm用データモデル
│   └── ValueTypeModel.cs                     # 値型vs参照型実験用モデル
├── Program.cs
└── BlazorDataBindingSample.csproj
```

## 実行方法

### 1. 前提条件

- .NET 8 SDK がインストールされていること

### 2. プロジェクトのビルドと実行

```bash
cd BlazorDataBindingSample
dotnet run
```

### 3. ブラウザでアクセス

アプリケーションが起動したら、ブラウザで以下のURLにアクセスします：

```
https://localhost:5001
```

または

```
http://localhost:5000
```

### 4. 各ページへのアクセス

サイドバーのナビゲーションメニューから各サンプルページにアクセスできます：

#### 基礎編
- `/basic-binding` - 基本的なバインディング
- `/twoway-binding` - 双方向バインディング
- `/event-binding` - イベントバインディング
- `/form-binding` - フォームバインディング

#### 応用編
- `/component-binding` - コンポーネント間バインディング
- `/editform-binding` - EditFormバインディング

#### 重要概念
- `/valuetype-vs-referencetype` - **値型 vs 参照型** ⭐ 必修
- `/prerendering-test` - プリレンダリング検証

## 📚 推奨学習順序

1. **基本的なバインディング** → 2. **双方向バインディング** → 3. **イベントバインディング** → 4. **フォームバインディング**
2. 5. **コンポーネント間バインディング** → 6. **EditFormバインディング**
3. 7. **値型 vs 参照型** ⭐（必ず確認） → 8. **プリレンダリング検証**

## 学習のポイント

### 単方向バインディング（@変数名）

```razor
<p>現在の値: @currentValue</p>
<button @onclick="UpdateValue">更新</button>
```

### 双方向バインディング（@bind）

```razor
<input @bind="name" />
<input @bind="name" @bind:event="oninput" />
<input @bind="name" @bind:after="OnNameChanged" />
```

### イベントバインディング

```razor
<button @onclick="HandleClick">クリック</button>
<input @onkeydown="HandleKeyDown" />
<div @onclick="HandleClick" @onclick:stopPropagation="true">伝播停止</div>
```

### コンポーネント間のバインディング

```razor
<!-- 親コンポーネント -->
<ChildComponent @bind-Count="parentCount" />

<!-- 子コンポーネント -->
[Parameter] public int Count { get; set; }
[Parameter] public EventCallback<int> CountChanged { get; set; }
```

### EditFormでのバインディング

```razor
<EditForm Model="@person" OnValidSubmit="@HandleValidSubmit">
    <DataAnnotationsValidator />
    <ValidationSummary />
    <InputText @bind-Value="person.Name" />
    <ValidationMessage For="@(() => person.Name)" />
</EditForm>
```

### ⭐ 値型 vs 参照型（重要）

Blazorのバインディングで最も重要な概念の一つです：

| 種類 | 例 | 子コンポーネントでの挙動 |
|------|----|-----------------------|
| **値型** | int, decimal, DateTime, bool, struct | コピー渡し → 子での変更は親に影響しない |
| **参照型** | class, List, Dictionary | 参照渡し → プロパティ変更は親にも反映 |

```razor
<!-- 値型の場合：EventCallbackが必要 -->
<ChildComponent @bind-Value="count" />

<!-- 参照型の場合：プロパティ変更は自動反映、オブジェクト置換時のみEventCallback必要 -->
<ChildComponent @bind-Model="model" />
```

**詳細は `/valuetype-vs-referencetype` ページで実際に動かして確認してください。**

## プリレンダリングについて

このプロジェクトではプリレンダリングが有効になっています（`@rendermode InteractiveServer`）。

### プリレンダリングの特徴

1. **ライフサイクルの二重呼び出し**: OnInitialized などが2回呼ばれます
   - 1回目：プリレンダリング時（サーバー側）
   - 2回目：インタラクティブ化時（SignalR接続後）

2. **JavaScript相互運用の制限**: プリレンダリング時は `JSRuntime` が使用できません
   - `OnAfterRender(firstRender: true)` 以降で使用可能

3. **初期状態の復元**: プリレンダリング時の状態は保持されません

詳細は `/prerendering-test` ページで確認できます。

## 技術スタック

- **フレームワーク**: Blazor Web App (.NET 8)
- **レンダリングモード**: Interactive Server（プリレンダリング有効）
- **通信**: SignalR
- **バリデーション**: DataAnnotations

## トラブルシューティング

### ポートが既に使用されている場合

`Properties/launchSettings.json` でポート番号を変更してください。

### プリレンダリングを無効化したい場合

各 `.razor` ファイルの `@rendermode InteractiveServer` を `@rendermode @(new InteractiveServerRenderMode(prerender: false))` に変更してください。

## 参考資料

- [ASP.NET Core Blazor のデータ バインディング](https://learn.microsoft.com/ja-jp/aspnet/core/blazor/components/data-binding)
- [ASP.NET Core Blazor のフォームとバリデーション](https://learn.microsoft.com/ja-jp/aspnet/core/blazor/forms/)
- [ASP.NET Core Blazor レンダー モード](https://learn.microsoft.com/ja-jp/aspnet/core/blazor/components/render-modes)

## ライセンス

このサンプルコードは学習目的で自由に使用できます。
