---
title: "Chrome DevTools MCP vs Playwright MCP - どちらを選ぶべき？実測で比較"
emoji: "🔍"
type: "tech"
topics: ["mcp", "chrome", "playwright", "claudecode", "blazor"]
published: true
publication_name: "nexta_"
---

## 訂正とお詫び（2025/11/09追記）

記事公開後、Playwright MCPコントリビューターの[ogadra](https://zenn.dev/ogady)様より、技術的な誤りをご指摘いただきました。深くお詫び申し上げます。

### 誤った記述
**「Playwright MCPはAIがセレクターを自動生成する」**

### 正しい動作
Playwright MCPも**Chrome DevTools MCPと同様に、スナップショットに含まれる識別子（ref）を使用**して要素を特定します。

- LLMはセレクターを生成していません
- `mcp__playwright__browser_fill_form`などの関数呼び出し時、引数として`ref`（Chrome DevTools MCPの`uid`に相当）が渡されます
- 両MCPの**要素識別方法に本質的な違いはありません**

### 実際の違い
主な違いは以下の2点です：

1. **操作の実行方法**
   - Chrome DevTools MCP: CDP（Chrome DevTools Protocol）で直接操作
   - Playwright MCP: 内部でPlaywrightライブラリを使って操作を実行後、同等の処理を行うPlaywrightコード例をレスポンスに含める（説明用）

2. **アクセシビリティツリーの情報量の差**
   - 取得できる要素情報の詳細度が異なる

以下、修正済みの内容となります。貴重なご指摘をいただいたogadra様に改めて感謝申し上げます。

---

## はじめに

Claude Codeでブラウザテストするとき、Chrome DevTools MCPとPlaywright MCPのどちらを使うべきか迷っていませんか？

この記事では、同じBlazorアプリで両方を実際に使い、選び方の基準を示します。

## 結論を先に：選び方の基準

| 用途 | おすすめ | 理由 |
|------|---------|------|
| デバッグ・要素特定 | **Chrome DevTools MCP** | UIDで要素を確実に指定 |
| パフォーマンス分析 | **Chrome DevTools MCP** | Core Web Vitals測定可能 |
| 標準的なフォーム操作 | **Playwright MCP** | Playwrightコード形式で操作 |
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

## 実際の検証例：Playwright MCPでBlazorフォーム操作

上記のBlazor Radzenアプリで、実際にPlaywright MCPを使ってフォーム操作を検証しました。

### 検証シナリオ
Name、Email、Age、Countryフィールドにデータを入力してSubmitするまでの一連の操作を実行。

:::details 実際に実行したMCPツールとレスポンス

**1. ページに移動**

MCPツール呼び出し：
```js
mcp__playwright__browser_navigate({ url: "http://localhost:5158/test-form" })
```

レスポンス（Ran Playwright code）：
```js
await page.goto('http://localhost:5158/test-form');
```

---

**2. フォーム入力（Name、Email、Age）**

MCPツール呼び出し：
```js
mcp__playwright__browser_fill_form({
  fields: [
    { name: "Name", type: "textbox", ref: "e104", value: "John Doe" },
    { name: "Email", type: "textbox", ref: "e107", value: "john@example.com" },
    { name: "Age", type: "textbox", ref: "e111", value: "30" }
  ]
})
```

レスポンス（Ran Playwright code）：
```js
await page.locator('#Name').fill('John Doe');
await page.locator('#Email').fill('john@example.com');
await page.locator('#Age').fill('30');
```

---

**3. Countryドロップダウンを開く**

MCPツール呼び出し：
```js
mcp__playwright__browser_click({ element: "Country dropdown", ref: "e114" })
```

レスポンス（Ran Playwright code）：
```js
await page.getByText('Japan USAJapanUKGermanyFranceCanada').click();
```

---

**4. USAを選択**

MCPツール呼び出し：
```js
mcp__playwright__browser_click({ element: "USA option", ref: "e135" })
```

レスポンス（Ran Playwright code）：
```js
await page.getByRole('option', { name: 'USA' }).click();
```

---

**5. Submitボタンをクリック**

MCPツール呼び出し：
```js
mcp__playwright__browser_click({ element: "Submit button", ref: "e126" })
```

レスポンス（Ran Playwright code）：
```js
await page.getByRole('button', { name: 'Submit' }).click();
```

結果：
```
Success! Form submitted successfully! Name: John Doe, Email: john@example.com
```

:::

### 検証から得られた知見

1. **MCPツール使用後、すぐにPlaywrightコードが確認できる**
   - `await page.locator('#Name').fill('John Doe')` など、そのまま使えるコード
   - テストコード作成の参考になる

2. **`getByRole()`など、Playwrightのベストプラクティスが使われる**
   - アクセシビリティを意識したセレクター
   - メンテナンスしやすいテストコードの書き方を学べる

3. **MCPで動作確認→コードをテストスイートに追加、という流れが効率的**
   - 手動検証とテスト自動化を同時に進められる
   - レスポンスのコードをコピペするだけ

## 実際の検証例：Chrome DevTools MCPでBlazorフォーム操作

同じBlazor Radzenアプリで、Chrome DevTools MCPを使ってフォーム操作を検証しました。

### 検証シナリオ
Playwright MCPと同じく、Name、Email、Age、Countryフィールドにデータを入力してSubmitするまでの一連の操作を実行。

:::details 実際に実行したMCPツールとレスポンス

**1. ページに移動**

MCPツール呼び出し：
```js
mcp__chrome-devtools__navigate_page({ 
  type: "url", 
  url: "http://localhost:5158/test-form" 
})
```

レスポンス：
```
Successfully navigated to http://localhost:5158/test-form.
## Pages
0: http://localhost:5158/test-form [selected]
```

---

**2. スナップショット取得（UID確認）**

MCPツール呼び出し：
```js
mcp__chrome-devtools__take_snapshot()
```

レスポンス（一部抜粋）：
```
uid=1_22 textbox "  "
uid=1_23 StaticText "Name"
uid=1_24 textbox "  "
uid=1_25 StaticText "Email"
uid=1_26 textbox "  " value="0"
uid=1_27 StaticText "Age"
uid=1_28 generic
  uid=1_29 textbox "Japan" haspopup="listbox" readonly value="Japan"
uid=1_32 StaticText "Country"
uid=1_38 button "SUBMIT"
```

---

**3. フォーム入力（Name、Email、Age）**

MCPツール呼び出し：
```js
mcp__chrome-devtools__fill_form({
  elements: [
    { uid: "1_22", value: "John Doe" },
    { uid: "1_24", value: "john@example.com" },
    { uid: "1_26", value: "30" }
  ]
})
```

レスポンス：
```
Successfully filled out the form
## Page content
uid=2_22 textbox "  " value="John Doe"
uid=2_24 textbox "  " value="john@example.com"
uid=2_26 textbox "  " value="30"
```

---

**4. Countryドロップダウンを開く**

MCPツール呼び出し：
```js
mcp__chrome-devtools__click({ 
  element: "Country dropdown container", 
  uid: "3_30" 
})
```

レスポンス：
```
Successfully clicked on the element
## Page content
uid=4_28 generic "Japan Japan arrow_drop_down" focusable focused
uid=4_40 listbox orientation="vertical"
```

---

**5. 詳細スナップショットでUSAのUIDを確認**

MCPツール呼び出し：
```js
mcp__chrome-devtools__take_snapshot({ verbose: true })
```

レスポンス（一部抜粋）：
```
uid=6_134 listbox orientation="vertical"
  uid=6_135 option "USA" value="USA"
  uid=6_138 option "Japan" value="Japan"
  uid=6_141 option "UK" value="UK"
  ...
```

---

**6. USAを選択**

MCPツール呼び出し：
```js
mcp__chrome-devtools__click({ 
  element: "USA option", 
  uid: "6_135" 
})
```

レスポンス：
```
Successfully clicked on the element
## Page content
uid=7_28 generic "USA USA arrow_drop_down" focusable focused
uid=7_29 textbox "USA" haspopup="listbox" readonly value="USA"
```

---

**7. Submitボタンをクリック**

MCPツール呼び出し：
```js
mcp__chrome-devtools__click({ 
  element: "SUBMIT button", 
  uid: "7_38" 
})
```

レスポンス：
```
Successfully clicked on the element
## Page content
uid=8_21 StaticText "check_circle"
uid=8_22 StaticText "Success!"
uid=8_24 StaticText "Form submitted successfully! Name: John Doe, Email: john@example.com"
```

:::

### 検証から得られた知見

1. **UID指定による確実な操作**
   - スナップショットで要素のUIDを明示的に確認
   - 誤操作のリスクが低い

2. **ドロップダウン操作に複数ステップが必要**
   - 開く → verbose スナップショット → 選択の3ステップ
   - Playwright MCPは2ステップで完了

3. **レスポンスは簡潔**
   - コード例は含まれない
   - 操作結果とページ状態のみ表示

## 実際の使用感：同じテストでの比較

Blazorのフォーム（Name、Email、Age、Country）に入力してSubmitするテストで比較しました。

### Playwright MCP：Playwrightコード形式で操作

**指示**（自然言語）：
> フォームに John Doe、john@example.com、30、USA を入力して送信

**実行内容**：
- スナップショットから要素の`ref`を取得
- `ref`を使ってPlaywrightコード形式で操作
- ドロップダウンも2ステップで完了
- ✅ **手軽で操作手順がシンプル**

### Chrome DevTools MCP：UID指定で確実

**指示**（同じ内容）：
> フォームに John Doe、john@example.com、30、USA を入力して送信

**実行内容**：
- スナップショット取得 → UID確認（`uid=1_22 (Name)`等）
- UIDでフォーム入力
- ドロップダウンは3ステップ（展開 → スナップショット → 選択）
- ✅ **確実だが、ステップ数が多い**

![Chrome DevTools MCPでのフォーム送信成功](/images/google-chrome-mcp/chrome-mcp-form-success.png)

## 詳細比較：6つの観点

### 1. 操作方法の違い

:::message
**共通点**: 両MCPとも、内部的には**アクセシビリティツリー**を使ってページ構造を取得しています。
:::

**本質的な違いは「操作コードの形式」**：

**Chrome DevTools MCP**：
- `take_snapshot` でアクセシビリティツリーを取得し、UID（一意識別子）を付与
- 操作時にはUIDで要素を明示的に指定
- CDP（Chrome DevTools Protocol）で直接操作
- 例: `{"uid": "1_22", "value": "John Doe"}`

**Playwright MCP**：
- アクセシビリティツリーから要素の`ref`（識別子）を取得
- 操作時には`ref`を使ってPlaywrightライブラリで実際の操作を実行
- 実行後、MCPのレスポンスに同等の処理を行うPlaywrightコード例が含まれる（説明用）
- 例: 内部で操作実行後、`await page.locator('#Name').fill('John Doe')`という相当するコードをレスポンスに含める

### 2. フォーム入力

**Chrome DevTools MCP**：
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
✅ スナップショットから要素のrefを取得して入力

mcp__playwright__browser_fill_form({
  name: "Name input",
  type: "textbox",
  ref: "xxxxx",  // スナップショットで取得した識別子
  value: "John Doe"
})
→ 実行後、レスポンスに含まれるPlaywrightコード例（説明用）:
  await page.locator('#Name').fill('John Doe');
  await page.locator('#Email').fill('john@example.com');
  await page.locator('#Age').fill('30');
```

:::message
**Playwright MCPのメリット：コード学習**

Playwright MCPは操作後に**実際のPlaywrightコード例**をレスポンスに含めます。

**このコードの活用方法**：
- ✅ **そのままテストコードとして使える**：コピペして自分のテストスイートに追加
- ✅ **デバッグが簡単**：何が実行されたか一目瞭然

MCPツールで動作確認→レスポンスのコードを自動テストに追加、という流れで効率的にテスト開発ができます。
:::

### 3. ドロップダウン操作

**Chrome DevTools MCP**：
```text
❌ 3ステップ必要

mcp__chrome-devtools__click(uid="3_30")  // コンテナクリック
mcp__chrome-devtools__take_snapshot(verbose=true)  // オプション一覧を取得
mcp__chrome-devtools__click(uid="5_135")  // 目的のオプションをクリック
```

**Playwright MCP**：
```text
✅ 2ステップで完了（スナップショットから要素のrefを取得）

mcp__playwright__browser_click({
  ref: "xxxxx"  // ドロップダウンのref
})
mcp__playwright__browser_click({
  ref: "yyyyy"  // USAオプションのref
})
→ 実行後、レスポンスに含まれるPlaywrightコード例（説明用）:
  // ドロップダウンを開く
  await page.getByText('Japan USAJapanUKGermanyFranceCanada').click();
  // USAオプションを選択
  await page.getByRole('option', { name: 'USA' }).click();
```

**注目ポイント**：`getByRole('option')`はPlaywrightの推奨パターン。アクセシビリティを意識した要素選択方法を自然に学べます。

### 4. スナップショット（ページ構造の確認）

**Chrome DevTools MCP**：
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

**Chrome DevTools MCP**：
- ✅ 細かい制御が可能
- ✅ デバッグしやすい（UIDが明確）
- ✅ 要素の特定が確実
- ❌ AIの実行ステップが多い（操作の前に必ずスナップショット取得が必要）
- ❌ UIDが動的に変わる（ページ更新や要素追加でUIDが変化する）

**Playwright MCP**：
- ✅ 操作手順がシンプル
- ✅ Playwrightコード形式で実行内容が分かりやすい
- ✅ 標準的なUI要素に強い
- ❌ 実行コードの詳細はMCPレスポンスに依存
- ❌ デバッグ時はPlaywrightコードを確認する必要がある

### 6. 共通機能と本質的な違い

**両MCPの共通点**：
- ✅ 内部的に**アクセシビリティツリー**を使用
- ✅ 基本的なブラウザ操作（ドラッグ&ドロップ、キーボード操作、ホバー、ファイルアップロード）は**同等に実行可能**

| 操作 | Chrome DevTools MCP | Playwright MCP |
|-----|-----------|---------------|
| ドラッグ&ドロップ | ✅ `drag` | ✅ `browser_drag` |
| キーボード操作 | ✅ `press_key` | ✅ `browser_press_key` |
| ホバー | ✅ `hover` | ✅ `browser_hover` |
| ファイルアップロード | ✅ `upload_file` | ✅ `browser_file_upload` |

**本質的な違い**は「操作の実行方法とレスポンス形式」：
- **Chrome DevTools MCP**: アクセシビリティツリーにUID（一意識別子）を付与し、CDP（Chrome DevTools Protocol）で直接操作
- **Playwright MCP**: アクセシビリティツリーから要素の`ref`（識別子）を取得し、Playwrightライブラリで操作を実行。実行後、同等の処理を行うPlaywrightコード例をレスポンスに含める

**要素の識別方法に本質的な違いはありません**。どちらもスナップショットに含まれる識別子を使用します。

どちらを選ぶかは、「CDP直接操作（Chrome DevTools MCP）」「Playwrightライブラリ実行+コード例提供（Playwright MCP）」の判断になります。

## Claude Codeでのセットアップ

### Chrome DevTools MCPのセットアップ

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

## Chrome DevTools MCP特有の機能：パフォーマンス分析

Chrome DevTools MCPの最大の特徴は、Playwright MCPにない**パフォーマンス分析機能**です。

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

パフォーマンストレースを実行すると、Chrome DevTools MCPが測定結果を分析し、具体的な改善アクションを**自動的に提案**してくれます：

1. **LCPBreakdown**: LCP最適化の提案
2. **CLSCulprits**: レイアウトシフト防止策
3. **RenderBlocking**: レンダリングブロック削減
4. **NetworkDependencyTree**: ネットワーク依存関係最適化
5. **DocumentLatency**: 初期ロード遅延削減（推定削減: 8.3 kB）
6. **Cache**: キャッシュ戦略の提案

:::message
**Playwright MCPとの違い**：
Playwright MCPではこれらのパフォーマンス指標は取得できません。Chrome DevToolsの機能を使った詳細な分析はChrome DevTools MCPだけの機能です。
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

Chrome DevTools MCPを使えば、このような具体的な改善ポイントが自動で提示されます。

## まとめ

Chrome DevTools MCPとPlaywright MCPは、どちらもアクセシビリティツリーを使い、**要素の識別方法に本質的な違いはありません**。主な違いは**操作の実行方法とレスポンス形式**です：

- **Chrome DevTools MCP**: UID指定でCDP直接操作。パフォーマンス分析も可能
- **Playwright MCP**: `ref`を使ってPlaywrightライブラリで操作を実行。実行後、同等の処理を行うPlaywrightコード例をレスポンスに含める（説明用）

用途に応じて使い分けることで、効率的なブラウザテストが実現できます。

## 参考リンク

- [Chrome DevTools MCP（GitHub）](https://github.com/ChromeDevTools/chrome-devtools-mcp)
- [Chrome DevTools MCP 公式ブログ（日本語）](https://developer.chrome.com/blog/chrome-devtools-mcp?hl=ja)
- [Playwright MCP（GitHub）](https://github.com/microsoft/playwright-mcp)
- [Playwright MCPでBlazor UIをテストする](https://zenn.dev/nexta_/articles/playwright-mcp-blazor-test)
- [サンプルコード（GitHub）](https://github.com/pixcelo/zenn-content/tree/main/samples/blazor-playwright-test)
