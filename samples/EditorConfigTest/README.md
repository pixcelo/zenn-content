# EditorConfig自動修正検証プロジェクト

このプロジェクトは、Zenn記事「[EditorConfigでC#開発を効率化：Visual StudioとAI生成コードの品質向上](https://zenn.dev/articles/editorconfig-csharp-visualstudio)」の検証用プロジェクトです。

## 目的

各重大度レベル（`suggestion`、`warning`、`error`）で、保存時やコードクリーンアップ時に自動修正が動作するかを検証します。

## 検証結果のまとめ

### 重要な発見

**「EditorConfig で設定されたすべての警告とエラーを修正」をコードクリーンアッププロファイルに追加すると、`warning`と`error`レベルの設定が自動修正されます。**

| 重大度レベル | プロファイル設定 | 自動修正 |
|------------|--------------|---------|
| **suggestion** | 「警告とエラーを修正」追加 | ❌ されない（対象外） |
| **warning** | 「警告とエラーを修正」追加 | ✅ **される** |
| **error** | 「警告とエラーを修正」追加 | ✅ **される**（推測） |

## セットアップ

1. Visual Studioでプロジェクトを開く
2. **ツール → オプション → テキストエディター → C# → コードクリーンアップ**
3. 「コードクリーンアップの構成」を開く
4. **「EditorConfig で設定されたすべての警告とエラーを修正」**を「含まれる修正ツール」に追加
5. 「保存時にコードクリーンアッププロファイルを実行」にチェック

## 検証方法

### 1. `.editorconfig`の設定を変更

`.editorconfig`ファイルで重大度レベルを変更：

```ini
# warningレベルで検証
csharp_style_implicit_object_creation_when_type_is_apparent = true:warning
csharp_prefer_braces = true:warning
csharp_using_directive_placement = outside_namespace:warning
```

### 2. テストコードで検証

`TestAutoFix.cs`には以下の違反コードが含まれています：

- `using`がnamespace内にある
- ターゲット型new演算子未使用（`new List<string>()`）
- 中括弧の省略

### 3. 自動修正を確認

- **Ctrl+S** で保存 → 自動修正されるか確認
- **Ctrl+K, Ctrl+E** でコードクリーンアップ → 修正されるか確認

## ファイル構成

- **`.editorconfig`**: 検証用のEditorConfig設定
- **`TestAutoFix.cs`**: 検証用のテストコード
- **`EditorConfigTest.csproj`**: プロジェクトファイル（`ImplicitUsings`は`disable`に設定）
- **`最終検証結果.md`**: 検証結果の詳細

## 検証結果

詳細は `最終検証結果.md` を参照してください。

## 参考

- [Zenn記事: EditorConfigでC#開発を効率化](https://zenn.dev/articles/editorconfig-csharp-visualstudio)
