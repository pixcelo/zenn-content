---
title: "Blazorのバインディングの仕組み"
emoji: "🔗"
type: "tech"
topics: ["blazor", "csharp", "dotnet", "web", "web開発"]
published: false
publication_name: "nexta_"
---

ネクスタの tetsu.k です。
基幹業務クラウド「SmartF」の開発に携わっています。

この記事では、Blazorにおけるバインディングの仕組みについて、
調べた結果を共有します。



## バインディングの種類

バインディングの全体像です。
それぞれに使いどころがあります。

| 種類 | 構文例 | 結びつけるもの | 方向 |
|---|---|---|---|
| 単方向データバインディング | `@変数名` | データ → UI | 単方向 |
| 双方向データバインディング | `@bind`/`@bind-Value`  | データ ↔ UI | 双方向 |
| 明示的な双方向バインディング | `Value` + `ValueChanged` | データ ↔ UI | 双方向（手動） |
| コンポーネント参照 | `@ref` | インスタンス ↔ 変数 | 単方向 |
| パラメーター | `[Parameter]` | 親 → 子 | 単方向 |
| カスケードパラメーター | `[CascadingParameter]` | 先祖 → 子孫 | 単方向 |
| イベント処理 (Event Handling) | `@onclick` | イベント → メソッド | 単方向 |
| EventCallback | `EventCallback<T>` | 子イベント → 親 | 単方向（逆流） |
| 属性スプラッティング | `@attributes` | 辞書 → 属性 | 単方向 |
| テンプレートコンポーネント (RenderFragment) | `ChildContent` | マークアップ → デリゲート | 単方向 |

以下で、個別に概念を紹介します。

## 単方向データバインディング（One-way）

データがUIに「反映」されるだけの、最も純粋な形です。

```mermaid
graph LR
    A["Variable / Property<br/>(C# Data Source)"]
    B["Display / Value<br/>(HTML UI)"]
    A -- Rendering --> B
```

## 双方向バインディング（Two-way / @bind）

「行き」と「帰り」がセットになった、循環する構造です。

```mermaid
graph TD
    A["Name = Alice<br/>(C# Data Source)"]
    B["input value=Alice<br/>(HTML Input Element)"]
    C{"Update Logic<br/>(Blazor自動生成)"}

    A -- Value --> B
    B -- onchange --> C
    C -- 再代入 --> A

    style C fill:#eee,stroke:#333,stroke-dasharray:5 5
```

## 明示的な双方向バインディング（Two-way）

`@bind`を使わず、`Value` と `ValueChanged` を個別に指定します。

```mermaid
graph TD
    A["MyValue Property<br/>(C# Data Source)"]
    B["Input Element<br/>(HTML UI)"]
    C["OnValueChanged Method<br/>(開発者が実装)"]

    A -- Value --> B
    B -- ValueChanged --> C
    C -- 再代入 --> A

    style C fill:#ffd,stroke:#333,stroke-width:2px
```

`@bind`との違い：
`OnValueChanged`メソッド内で、バリデーション・APIコール・条件付き更新など、変更時の処理を自由にカスタマイズできます。

## 環境

- .NET 8
- Blazor Web App (Interactive Server)
- プリレンダリング有効

---

## サンプル

サンプルプロジェクトを用意しました。

[GitHubサンプルコード](https://github.com/pixcelo/zenn-content/tree/main/samples/blazor-databinding-sample)