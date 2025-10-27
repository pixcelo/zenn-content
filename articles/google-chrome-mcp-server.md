---
title: "Chrome MCP vs Playwright MCP - どちらを選ぶべき？実測で比較"
emoji: "🔍"
type: "tech"
topics: ["mcp", "chrome", "playwright", "claudecode", "blazor"]
published: false
publication_name: "nexta_"
---

## はじめに

Claude Codeでブラウザテストするとき、Chrome MCPとPlaywright MCPのどちらを使うべきか迷っていませんか？

この記事では、同じBlazorアプリで両方を実際に使い、選び方の基準を示します。

## 結論を先に：選び方の基準

| 用途 | おすすめ | 理由 |
|------|---------|------|
| デバッグ・要素特定 | **Chrome MCP** | UIDで要素を確実に指定 |
| パフォーマンス分析 | **Chrome MCP** | Core Web Vitals測定可能 |
| 標準的なフォーム操作 | **Playwright MCP** | AIが自動で要素を検出 |
| 探索的テスト | **Playwright MCP** | 操作手順がシンプル |
| CI/CD自動テスト | **どちらも不向き** | 従来のPlaywright/Selenium推奨 |

## 検証環境

Blazor ServerとRadzen Componentsで作成したTest Formページを使用：

- URL: `https://localhost:7286/test-form`
- フィールド: Name、Email、Age、Country（ドロップダウン）
- 動作: Submit後に成功メッセージ表示

:::message
この検証環境の詳細セットアップ手順は、別記事「[Playwright MCPでBlazor UIをテストする](https://zenn.dev/nexta_/articles/playwright-mcp-blazor-test)」を参照してください。本記事では、両MCPの比較に焦点を当てます。
:::

## 実際の使用感：同じテストでの比較

Blazorのフォーム（Name、Email、Age、Country）に入力してSubmitするテストで比較しました。

### Playwright MCP：AIにお任せ

**指示**（自然言語）：
> フォームに John Doe、john@example.com、30、USA を入力して送信

**実行内容**：
- AIが自動でセレクターを生成（`#Name`、`#Email`等）
- ドロップダウンも2ステップで完了
- ✅ **手軽だが、AIの推測に依存**

### Chrome MCP：UID指定で確実

**指示**（同じ内容）：
> フォームに John Doe、john@example.com、30、USA を入力して送信

**実行内容**：
- スナップショット取得 → UID確認（`uid=1_22 (Name)`等）
- UIDでフォーム入力
- ドロップダウンは3ステップ（展開 → スナップショット → 選択）
- ✅ **確実だが、ステップ数が多い**

![Chrome MCPでのフォーム送信成功](/images/google-chrome-mcp/chrome-mcp-form-success.png)

## 詳細比較：6つの観点

### 1. 操作方法の違い

:::message
**共通点**: 両MCPとも、内部的には**アクセシビリティツリー**を使ってページ構造を取得しています。
:::

**本質的な違いは「要素の識別方法」**：

**Chrome MCP**：
- `take_snapshot` でアクセシビリティツリーを取得し、UID（一意識別子）を付与
- 操作時にはUIDで要素を明示的に指定
- 例: `{"uid": "1_22", "value": "John Doe"}`

**Playwright MCP**：
- アクセシビリティツリーからAIがセレクターを自動生成
- 操作時にはAIが適切なセレクター（name、role、text）を選択
- 例: `await page.locator('#Name').fill('John Doe')`

### 2. フォーム入力

**Chrome MCP**：
```text
❌ 事前にスナップショットでUIDを確認する必要がある

mcp__chrome-devtools__take_snapshot()
→ 結果: uid=1_22 (Name), uid=1_24 (Email), uid=1_26 (Age)

mcp__chrome-devtools__fill_form([
  {"uid": "1_22", "value": "John Doe"},
  {"uid": "1_24", "value": "john@example.com"},
  {"uid": "1_26", "value": "30"}
])
```

**Playwright MCP**：
```text
✅ AIが自動でセレクターを推測

mcp__playwright__browser_fill_form
→ 実行されるPlaywrightコード:
  await page.locator('#Name').fill('John Doe');
  await page.locator('#Email').fill('john@example.com');
  await page.locator('#Age').fill('30');
```

### 3. ドロップダウン操作

**Chrome MCP**：
```text
❌ 3ステップ必要

mcp__chrome-devtools__click(uid="3_30")  // コンテナクリック
mcp__chrome-devtools__take_snapshot(verbose=true)  // オプション一覧を取得
mcp__chrome-devtools__click(uid="5_135")  // 目的のオプションをクリック
```

**Playwright MCP**：
```text
✅ 2ステップで完了（AIが自動で要素を推測）

mcp__playwright__browser_click  // ドロップダウン展開
mcp__playwright__browser_click  // USAオプション選択
→ 実行されるPlaywrightコード:
  await page.getByRole('option', { name: 'USA' }).click();
```

### 4. スナップショット（ページ構造の確認）

**Chrome MCP**：
```text
uid=1_0 RootWebArea "Test Form"
  uid=1_22 textbox "  "
  uid=1_23 StaticText "Name"
  uid=1_24 textbox "  "
  uid=1_25 StaticText "Email"
  uid=1_26 textbox "  " value="0"
  uid=1_27 StaticText "Age"
```
→ **UIDが明確、デバッグしやすい**

**Playwright MCP**：
```text
textbox "Name"
textbox "Email"
textbox "Age" [value=0]
combobox "Country"
```
→ **人間が読みやすい、直感的**

### 5. 使いやすさ

**Chrome MCP**：
- ✅ 細かい制御が可能
- ✅ デバッグしやすい（UIDが明確）
- ✅ 要素の特定が確実
- ❌ AIの実行ステップが多い（操作の前に必ずスナップショット取得が必要）
- ❌ UIDが動的に変わる（ページ更新や要素追加でUIDが変化する）

**Playwright MCP**：
- ✅ 操作手順がシンプル
- ✅ AIによる要素検出が便利
- ✅ 標準的なUI要素に強い
- ❌ AIの推測に依存（誤った要素を選ぶ可能性）
- ❌ デバッグが難しい（どのセレクターを使ったか不明瞭）
- ❌ 繰り返し実行の精度にばらつき

### 6. 共通機能と本質的な違い

**両MCPの共通点**：
- ✅ 内部的に**アクセシビリティツリー**を使用
- ✅ 基本的なブラウザ操作（ドラッグ&ドロップ、キーボード操作、ホバー、ファイルアップロード）は**同等に実行可能**

| 操作 | Chrome MCP | Playwright MCP |
|-----|-----------|---------------|
| ドラッグ&ドロップ | ✅ `drag` | ✅ `browser_drag` |
| キーボード操作 | ✅ `press_key` | ✅ `browser_press_key` |
| ホバー | ✅ `hover` | ✅ `browser_hover` |
| ファイルアップロード | ✅ `upload_file` | ✅ `browser_file_upload` |

**本質的な違い**は「要素の識別方法」：
- **Chrome MCP**: アクセシビリティツリーにUID（一意識別子）を付与し、UIDで要素を明示的に指定
- **Playwright MCP**: アクセシビリティツリーからAIがセレクターを自動生成して要素を特定

どちらを選ぶかは、「確実性を取るか（Chrome MCP）」「手軽さを取るか（Playwright MCP）」の判断になります。

## Claude Codeでのセットアップ

### Chrome MCPのセットアップ

```bash
# MCPサーバーを追加
claude mcp add chrome-devtools npx chrome-devtools-mcp@latest

# 追加されたか確認
claude mcp
```

### Playwright MCPのセットアップ

```bash
# MCPサーバーを追加
claude mcp add playwright npx @playwright/mcp@latest

# 追加されたか確認
claude mcp
```

:::message
**補足**: `claude mcp add`コマンドは、Claude Codeの設定ファイルに自動でMCPサーバー情報を追加します。手動で設定ファイルを編集する必要はありません。
:::

## Chrome MCP特有の機能：パフォーマンス分析

Chrome MCPの最大の特徴は、Playwright MCPにない**パフォーマンス分析機能**です。

Blazor Server（`https://localhost:7286/test-form`）で実際に検証した結果：

### パフォーマンストレース結果

```text
mcp__chrome-devtools__performance_start_trace({
  reload: true,
  autoStop: true
})
```

**取得できたCore Web Vitals**：

:::message
**Core Web Vitalsとは**？
Googleが定めたWebページの品質を測る3つの重要指標です：

- **LCP (Largest Contentful Paint)**：最大コンテンツの表示速度
  - 目安：2.5秒以内が良好
  - ページで一番大きな要素（画像やテキストブロック）が表示されるまでの時間

- **CLS (Cumulative Layout Shift)**：レイアウトの安定性
  - 目安：0.1以下が良好
  - ページ読み込み中に要素がガタガタ動かないかを測る指標

- **TTFB (Time to First Byte)**：サーバー応答速度
  - 目安：800ms以内が良好
  - ブラウザがサーバーから最初のデータを受け取るまでの時間
:::

```text
LCP: 138 ms ✅ 良好（2.5秒以内）
  - TTFB: 25 ms ✅ 優秀（800ms以内）
  - Render delay: 114 ms（描画遅延）
CLS: 0.00 ✅ 完璧（0.1以下）
```

**Chrome MCPが自動生成したパフォーマンス改善提案（6種類）**：

パフォーマンストレースを実行すると、Chrome MCPが測定結果を分析し、具体的な改善アクションを**自動的に提案**してくれます：

1. **LCPBreakdown**: LCP最適化の提案
2. **CLSCulprits**: レイアウトシフト防止策
3. **RenderBlocking**: レンダリングブロック削減
4. **NetworkDependencyTree**: ネットワーク依存関係最適化
5. **DocumentLatency**: 初期ロード遅延削減（推定削減: 8.3 kB）
6. **Cache**: キャッシュ戦略の提案

:::message
**Playwright MCPとの違い**：
Playwright MCPではこれらのパフォーマンス指標は取得できません。Chrome DevToolsの機能を使った詳細な分析はChrome MCPだけの機能です。
:::

### コンソールログ取得結果

```text
mcp__chrome-devtools__list_console_messages()
```

**取得できたBlazor特有のログ**：
```bash
[info] Normalizing '_blazor' to 'https://localhost:7286/_blazor'
[info] WebSocket connected to wss://localhost:7286/_blazor
[debug] CSS Hot Reload ignoring bootstrap.min.css (7000+ rules)
[debug] CSS Hot Reload ignoring material-base.css (7000+ rules)
```

→ Blazorの内部動作（WebSocket接続、CSS Hot Reload）を監視できます。

### ネットワークリクエスト取得結果

```text
mcp__chrome-devtools__list_network_requests({
  resourceTypes: ["document", "script", "xhr", "fetch"]
})
```

**取得できたBlazor初期化リクエスト（10件）**：
```text
GET /test-form [200]
GET /_framework/blazor.web.js [304]
GET /_content/Radzen.Blazor/Radzen.Blazor.js [200]
POST /_blazor/negotiate [200]
```

→ Blazorの初期化プロセス（SignalR negotiation、WebSocket接続）を詳細に確認できます。

### 実用例：パフォーマンス問題の検出

この検証で以下のことが分かりました：

- ✅ **LCPは良好**（138ms < 2.5秒）
- ✅ **CLSは完璧**（0.00 = レイアウトシフトなし）
- ⚠️ **Render delayがやや長い**（114ms / 138ms = 82%）
  - 改善提案：レンダリングブロックリソースの削減

Chrome MCPを使えば、このような具体的な改善ポイントが自動で提示されます。

## まとめ

Chrome MCPとPlaywright MCPは、どちらもアクセシビリティツリーを使いますが、**要素の識別方法**が異なります：

- **Chrome MCP**: UID指定で確実。パフォーマンス分析も可能
- **Playwright MCP**: AIが推測して手軽。標準UIに強い

用途に応じて使い分けることで、効率的なブラウザテストが実現できます。

## 参考リンク

- [Chrome DevTools MCP（GitHub）](https://github.com/ChromeDevTools/chrome-devtools-mcp)
- [Chrome DevTools MCP 公式ブログ（日本語）](https://developer.chrome.com/blog/chrome-devtools-mcp?hl=ja)
- [Playwright MCP（GitHub）](https://github.com/microsoft/playwright-mcp)
- [Playwright MCPでBlazor UIをテストする](https://zenn.dev/nexta_/articles/playwright-mcp-blazor-test)
- [サンプルコード（GitHub）](https://github.com/pixcelo/zenn-content/tree/main/samples/blazor-playwright-test)
