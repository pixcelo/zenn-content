---
title: "C#開発者向け .editorconfigでAI生成コードを整形 - Visual Studioでの実践ガイド"
emoji: "🛠️"
type: "tech" # tech: 技術記事 / idea: アイデア
topics: ["editorconfig", "csharp", "visualstudio", "ai"]
published: true
published_at: 2025-12-08 16:00
publication_name: "nexta_" # 企業のPublication名を指定
---

## はじめに - AI時代のC#コード品質管理

GitHub CopilotやClaude Codeなど、AI支援ツールがC#開発に浸透してきました。しかし、AI生成コードには「スタイルの不統一」という課題があります。

- あるメソッドでは `var` を使い、別のメソッドでは明示的な型指定
- 古いC#スタイルの提案（`new List<string>()` vs `new()`）
- privateフィールドの命名規則が統一されない（`_field` vs `field`）

これらを毎回手動で修正するのは非効率です。`.editorconfig`を使えば、AI生成後のコードをIDE（Visual Studio）が自動整形し、プロジェクトの規約に従わせることができます。

### この記事で学べること

- `.editorconfig`による AI生成コードの自動整形とスタイル統一
- 重大度レベル（severity）の使い分けと実践的な設定方法
- Visual Studioでの導入手順とコードクリーンアップ設定
- 既存プロジェクトへの段階的導入戦略
- すぐに使える実用的な設定テンプレート

## 前提条件

- **Visual Studio**: 2019 version 16.8以降（ビルドエラー化機能に必要）
- **C#**: 8.0以降
- **.NET**: .NET Core 3.1以降、または.NET 5+

## EditorConfigとは？

`.editorconfig`は、エディターに依存しないコーディングスタイル設定ファイルです。プロジェクトのルートに配置することで、チーム全体で統一されたコーディング規約を設定できます。

Visual Studio 2017以降でネイティブサポートされており、特にC#では、Roslynアナライザーと連携することでビルドエラー化が可能です。

## C#で何ができるのか？

`.editorconfig`でC#では以下のような設定が可能です：

- **基本設定**: インデント、改行、文字コード
- **命名規則**: privateフィールドに`_`、インターフェースに`I`プレフィックス
- **コードスタイル**: `var`の使用、式形式メンバー、using配置
- **フォーマット**: 中括弧の改行位置、スペース設定

具体的な設定例は、後述の「推奨設定テンプレート」を参照してください。

## EditorConfigの「強制力」- 重大度レベルとは

`.editorconfig`の最も重要な特徴は、設定の重要度レベル（severity）を制御できることです。

### 重大度レベルの全体像（5段階）

Visual Studioでは、以下の5段階の重大度レベル（severity）が定義されています：

| 設定値 | 日本語表示 | エディター表示 | ビルド | 用途 |
|--------|-----------|--------------|--------|------|
| `none` | 無効 | 表示なし | 成功 | ルールを無効化 |
| `silent` | リファクタリング | 表示なし（自動整形） | 成功 | 静かに修正 |
| `suggestion` | 提案事項 | 💡 灰色点線 | 成功 | 推奨スタイル |
| `warning` | 警告 | ⚠️ 緑波線 | 成功 | 警告表示 |
| `error` | エラー | ❌ 赤波線 | **失敗** | 必須ルール |

実務では、`silent`（リファクタリング）、`suggestion`（提案事項）、`warning`（警告）、`error`（エラー）の4つを使い分けます。`none`（無効）はルールを書かないのと同じ効果のため、ほぼ使用しません。

### Level 1: リファクタリングのみ（silent）

```ini
[*.cs]
dotnet_diagnostic.IDE0055.severity = silent
```

動作:
- エディター上に視覚的な警告なし
- クイックアクション（電球アイコン）から個別に適用可能
- コードクリーンアップには参加しない（保存時の自動適用には含まれない）

### Level 2: 推奨（suggestion）

```ini
[*.cs]
csharp_style_var_when_type_is_apparent = true:suggestion
```

動作:
- エディター上に灰色の点線
- 電球アイコンでクイックフィックス提案
- ビルドは成功する

### Level 3: 警告（warning）

```ini
[*.cs]
csharp_prefer_braces = true:warning
```

動作:
- エディター上に緑の波線
- 「エラー一覧」ウィンドウに警告表示
- ビルドは成功する
- CI/CDで `/warnaserror` を使うとエラー化可能

### Level 4: エラー（error）

```ini
[*.cs]
dotnet_naming_rule.private_fields_with_underscore.severity = error
csharp_using_directive_placement = outside_namespace:error
```

動作:
- エディター上に赤の波線
- コンパイルが失敗する
- CI/CDで自動的にブロック

```csharp
// ❌ ビルドエラー: IDE0065
namespace MyApp
{
    using System;  // error: usingはnamespace外に配置すべき
}

// ✅ 正しい
using System;

namespace MyApp
{
}
```

`severity = error`に設定することで、usingの配置ミスがビルドエラーになり、規約違反のコードをコミット前に検出できます。

### Roslynアナライザーとの連携

Visual StudioのC#コンパイラー（Roslyn）は、`.editorconfig`の設定を診断ルールとして認識します。

主要な診断ID：

| 診断ID | 内容 | 推奨severity |
|--------|------|--------------|
| IDE0055 | フォーマット規則違反 | warning |
| IDE0001 | 名前の簡略化 | suggestion |
| IDE0005 | 不要なusing | warning |
| IDE0065 | usingの配置 | error |
| IDE0011 | 中括弧の追加 | warning |

```ini
[*.cs]
# すべてのフォーマット違反をエラー化（厳格）
dotnet_diagnostic.IDE0055.severity = error

# 不要なusingは警告のみ
dotnet_diagnostic.IDE0005.severity = warning
```

## どんなメリットがあるのか？

コードレビュー時間の削減

機械的にチェックできるスタイル指摘（命名規則、インデント、using配置など）がゼロになり、人間のレビュアーはアーキテクチャやロジックなど本質的な部分に集中できます。

CI/CD連携で品質担保

CI/CD環境で `/warnaserror` オプションを使用することで、警告をエラー化してビルドを失敗させることができます。

```yaml
# .github/workflows/build.yml
- name: Build
  run: dotnet build --configuration Release /p:TreatWarningsAsErrors=true
```

これにより、ローカルでは柔軟に開発しつつ、マージ時には厳格にチェックする運用が可能です。

## AIとEditorConfigの実践的な関係

AIエージェント（GitHub Copilot、Claude Code等）は、通常`.editorconfig`ファイルを自動的に読み取りません。既存コードのパターンやトレーニングデータから推測してコードを生成します。

### AI生成コードの課題と解決策

AI生成コードは、プロジェクトの命名規則に従わないことがあります。`.editorconfig`で`severity = error`に設定することでビルドエラーとして検出でき、クイックフィックスで一括修正が可能です。

### AIエージェントにEditorConfigを参照させる方法

明示的にファイル参照
- GitHub Copilot: `@workspace #file:.editorconfig`でコンテキストに追加
- Claude Code/Cursor: プロンプトで`.editorconfig`を明示的に指定

AGENTS.md（推奨）
- AIコーディングエージェント向けの標準規格として、プロジェクトルートに配置することでCopilot、Cursor、Windsurf等が自動参照

### Visual Studioの整形機能

Visual StudioでEditorConfig設定を適用する方法：

- **保存時の自動フォーマット**: ツール → オプション → コードクリーンアップ → 「保存時に自動実行する」
- **コードのクリーンアップ**: `Ctrl + K, Ctrl + E`
- **ドキュメントのフォーマット**: `Ctrl + K, Ctrl + D`

コードクリーンアッププロファイルの設定（重要）

`warning`や`error`レベルの設定を自動修正するには、以下の設定が必要です：

1. ツール → オプション → テキストエディター → C# → コードクリーンアップ
2. 「使用可能な修正ツール」から「EditorConfig で設定されたすべての警告とエラーを修正」を選択
3. 「保存時にコードクリーンアッププロファイルを実行」にチェック

![コードクリーンアップの構成ダイアログ](/images/editorconfig-csharp-visualstudio/vs-code-cleanup-config.png)
*Visual Studioのコードクリーンアップ構成画面*

### AIエージェントとEditorConfigの役割分担

| 主体 | 役割 | EditorConfig参照 | 備考 |
|------|------|-----------------|------|
| AIエージェント（通常） | コード生成・提案 | ❌ 自動参照しない | 既存コードから推測 |
| AIエージェント（明示的参照） | コード生成・提案 | ✅ 参照可能 | `#file`や直接指示で参照 |
| AIエージェント（AGENTS.md） | コード生成・提案 | ✅ 自動参照 | クロスツール標準設定 |
| IDE（Visual Studio） | 整形・フォーマット | ✅ 自動読み取り | 保存時/手動整形で適用 |

`.editorconfig`の活用には3つのレベルがあります：

1. IDE経由の自動整形（基本）: AIエージェントは参照せず、IDE側の保存時フォーマットで整形
2. AIエージェント設定（応用）: AGENTS.mdや明示的参照でAI生成時に規約適用
3. 多層防御: AI生成時→IDE自動整形→CI/CDビルドの3段階チェック

## Visual Studioでの実践 - プロジェクトへの導入手順

既存プロジェクトに手動追加

1. ソリューションエクスプローラーでソリューションまたはプロジェクトを右クリック
2. 追加 → 新しい項目
3. 検索ボックスに「editorconfig」と入力
4. editorconfig ファイル（既定値）を選択
5. 追加をクリック

Visual Studioの設定から`.editorconfig`を生成

ツール → オプション → テキストエディター → C# → コードスタイル から、現在の設定を`.editorconfig`として出力できます。

### 推奨設定テンプレート

プロジェクトでよく使われる設定の抜粋版です。必要に応じて追加・調整してください。

```ini
# トップレベル .editorconfig
root = true

# すべてのファイル
[*]
charset = utf-8
insert_final_newline = true
trim_trailing_whitespace = true

# C#ファイル
[*.cs]
# 基本設定
indent_style = space
indent_size = 4
end_of_line = crlf

# using の整理
dotnet_sort_system_directives_first = true

# コードスタイル
dotnet_style_qualification_for_field = false:warning
dotnet_style_require_accessibility_modifiers = always:warning
csharp_prefer_braces = true:warning
csharp_using_directive_placement = outside_namespace:error

# 書式ルール
csharp_new_line_before_open_brace = all

# 命名規則
# インターフェースは I プレフィックス
dotnet_naming_rule.interface_should_be_begins_with_i.severity = error
dotnet_naming_rule.interface_should_be_begins_with_i.symbols = interface
dotnet_naming_rule.interface_should_be_begins_with_i.style = begins_with_i

dotnet_naming_symbols.interface.applicable_kinds = interface
dotnet_naming_style.begins_with_i.required_prefix = I
dotnet_naming_style.begins_with_i.capitalization = pascal_case

# privateフィールドは _ プレフィックス
dotnet_naming_rule.private_field_should_be_begins_with__.severity = error
dotnet_naming_rule.private_field_should_be_begins_with__.symbols = private_field
dotnet_naming_rule.private_field_should_be_begins_with__.style = begins_with__

dotnet_naming_symbols.private_field.applicable_kinds = field
dotnet_naming_symbols.private_field.applicable_accessibilities = private
dotnet_naming_style.begins_with__.required_prefix = _
dotnet_naming_style.begins_with__.capitalization = camel_case
```

:::details Microsoft公式推奨設定の完全版を表示（約200行）

上記の抜粋版で足りない場合は、Microsoft公式推奨設定の完全版を参照してください。すべての設定項目を含む完全版は、[.NET のコード スタイル規則オプション | Microsoft Learn](https://learn.microsoft.com/ja-jp/dotnet/fundamentals/code-analysis/code-style-rule-options) で確認できます。

Visual Studioの設定から`.editorconfig`を生成する方法：
1. **ツール → オプション → テキストエディター → C# → コードスタイル**
2. 各設定項目を希望の値に変更
3. **設定から .editorconfig ファイルを生成**ボタンをクリック

:::

## まとめ

本記事で解説した要点：

- AI生成コードをプロジェクト規約に従わせる（コードクリーンアッププロファイル設定により自動修正）
- コードレビューの効率化（スタイル指摘がゼロになる）
- チーム全体のスタイル統一（新メンバーも規約に従える）
- ビルドエラー化で品質担保（Visual StudioのRoslyn連携）

新規プロジェクトでは推奨設定テンプレートから、既存プロジェクトでは段階的導入から始めてください。

### 参考リンク

- [.NET のコード スタイル規則オプション | Microsoft Learn](https://learn.microsoft.com/ja-jp/dotnet/fundamentals/code-analysis/code-style-rule-options)
- [EditorConfig.org](https://editorconfig.org/)
- [Visual Studio での EditorConfig | Microsoft Learn](https://learn.microsoft.com/ja-jp/visualstudio/ide/create-portable-custom-editor-options)
