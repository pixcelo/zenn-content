---
title: "Playwright MCPでBlazor UIをテストする - コード不要のE2E検証"
emoji: "🎭"
type: "tech"
topics: ["blazor", "playwright", "mcp", "claudecode", "csharp"]
published: false
publication_name: "nexta_" # 企業のPublication名を指定
---

## はじめに

「このボタン押したら正しく動くか確認して」

PdMやデザイナーからこんな依頼を受けたとき、どうしてますか？「自分で確認して」とは言いづらいし、手動確認は時間がかかる。かといってテストコード書くほどでもない...。

これまでブラウザ操作の自動化といえば、PlaywrightやSeleniumでコードを書くしかなかった。プログラマー以外には手が出ない領域でした。

でも、Playwright MCPを使えば話が変わります。

```
「商品一覧のページで、一番上の商品を削除して、
削除完了って出るか確認して」
```

この指示をAIエージェントに渡すだけで、ブラウザ操作が実行されて結果が返ってきます。コードもセレクター指定も要りません。

この記事では、Blazor ServerとRadzen Componentsで作った検証環境を使って、「Playwright MCPで実際に何ができるのか」を検証します。

## Playwright MCPで何ができるのか

まず最初に、一番知りたいことから。実際に使ってみて分かった「できること」「できないこと」をまとめます。

### できること

**基本的なブラウザ操作**：
- ページ遷移、リンククリック
- フォーム入力（テキスト、数値、日付）
- ドロップダウン選択、チェックボックス、ラジオボタン
- ボタンクリック、送信

**要素の検証**：
- テキストの存在確認（「〇〇って表示されてる？」）
- エラーメッセージの確認
- 要素の表示/非表示チェック
- スクリーンショット取得

**複雑な操作**：
- テーブル（グリッド）の編集・削除
- タブ切り替え
- モーダル/ダイアログ操作
- 動的に生成される要素の操作

**曖昧な指示への対応**（ここが重要）：
- 「一番上の商品」→ 最初の行を特定
- 「名前の欄」→ Name/氏名/お名前などのフィールドを推測
- 「削除完了って出るか確認」→ 成功メッセージを探す

### できないこと・苦手なこと

**精密な制御が必要な操作**：
- ドラッグ&ドロップ（位置指定が曖昧）
- キーボードショートカット（Ctrl+C等）
- マウスホバーでのツールチップ表示
- 複雑なスクロール操作

**パフォーマンステスト**：
- ページ読み込み時間の測定
- レスポンスタイムの計測
- 並列実行によるストレステスト

**繰り返し実行**：
- CI/CDでの完全自動化（AIの解釈にばらつきがある）
- 同じ結果を100%保証する必要があるテスト
- 大量のテストケースの一括実行

**細かいセレクター指定**：
- XPathやCSSセレクターを直接的に細かく制御したい場合（AIの解釈に依存するため）
- 複雑な条件式でのセレクター指定

**認証・セッション管理**：
- ログイン状態の保持（毎回ログインが必要）
- Cookie/LocalStorageの直接操作
- Basic認証のヘッダー設定

**ファイル操作・iframe**：
- ファイルのアップロード/ダウンロード（自然言語での指示が難しい）
- iframe内の要素操作（コンテキスト切り替えが必要）
- Shadow DOM内の要素アクセス

### 従来のPlaywrightとの比較

| やりたいこと | 従来のPlaywright | Playwright MCP |
|------------|----------------|---------------|
| 新機能のリリース前確認（1回きり） | コード書くのは過剰 | ✅ 最適 |
| PdMの受け入れテスト | コード書けない | ✅ 最適 |
| 顧客報告の不具合再現 | コード書くのは過剰 | ✅ 最適 |
| 毎日実行するリグレッションテスト | ✅ 最適 | AIの解釈にばらつき |
| CI/CDパイプラインで自動実行 | ✅ 最適 | 不向き |
| パフォーマンス測定 | ✅ 最適 | できない |

## 実際に試してみた

Blazor ServerとRadzen Componentsで検証環境を作って、実際にPlaywright MCPでテストしてみました。

### 検証環境

3つのページを用意しました：

1. **Test Formページ** - フォーム入力とバリデーション
2. **Data Gridページ** - データグリッドのCRUD操作
3. **Dashboardページ** - タブ、グラフ、ダイアログなど

### テストシナリオ1：フォームの基本入力

AIへの指示：
```
http://localhost:5158/test-form にアクセスして、以下の操作を実行してください：

1. 名前の欄に "John Doe" って入力
2. メールアドレスの欄に "john@example.com" って入力
3. 年齢に "30" って入力
4. 国のドロップダウンで "USA" を選ぶ
5. Submitボタンを押す
6. 成功メッセージが出るか確認
```

期待結果：
- すべてのフィールドに値が入る
- 送信後、緑色の成功アラートが出る
- アラートに入力したNameとEmailが表示される

**ポイント**：
従来のPlaywrightだと、各フィールドのセレクター（`input[name="Name"]`等）を書く必要がありました。Playwright MCPなら「名前の欄」「メールアドレスの欄」って普通に言うだけで、AIが要素を見つけてくれます。

:::details 実際のPlaywright MCPセッション例を見る

**AIへの指示**：
```
http://localhost:5158/test-form にアクセスして、以下の操作を実行してください：

1. 名前の欄に "John Doe" って入力
2. メールアドレスの欄に "john@example.com" って入力
3. 年齢に "30" って入力
4. 国のドロップダウンで "USA" を選ぶ
5. Submitボタンを押す
6. 成功メッセージが出るか確認
7. スクリーンショットを撮って
```

**AIの実行内容**：

1. ページにアクセス
```
mcp__playwright__browser_navigate
→ await page.goto('http://localhost:5158/test-form');
```

2. フォームフィールドに入力
```
mcp__playwright__browser_fill_form
→ await page.locator('#Name').fill('John Doe');
→ await page.locator('#Email').fill('john@example.com');
→ await page.locator('#Age').fill('30');
```

3. ドロップダウンを展開して「USA」を選択
```
mcp__playwright__browser_click (ドロップダウンクリック)
mcp__playwright__browser_click (USAオプション選択)
→ await page.getByRole('option', { name: 'USA' }).click();
```

4. Submitボタンをクリック
```
mcp__playwright__browser_click
→ await page.getByRole('button', { name: 'Submit' }).click();
```

5. 成功メッセージを確認
```
Success! Form submitted successfully! Name: John Doe, Email: john@example.com
```

6. スクリーンショット保存
```
mcp__playwright__browser_take_screenshot
→ await page.screenshot({ fullPage: true });
```

**結果**：
✅ すべてのフィールドに正しく値が入力された
✅ 送信後、緑色の成功アラートが表示された
✅ アラートに入力したNameとEmailが含まれていた

![フォーム送信成功画面](/images/playwright-mcp-blazor-test/blazor-form-submission-success.png)

自然言語の指示だけで、AIが適切なセレクターを見つけて操作を完了しました。

:::

:::details 参考：従来のPlaywright（TypeScript）でのコード例

同じ操作を従来のPlaywrightでコードで書くとこうなります：

```typescript
import { test, expect } from '@playwright/test';

test('フォーム入力から送信までの一連の操作', async ({ page }) => {
  // 1. Test Formページに移動
  await page.goto('http://localhost:5158/test-form');
  await page.waitForLoadState('networkidle');

  // 2. 名前を入力
  await page.locator('#Name').fill('John Doe');

  // 3. メールアドレスを入力
  await page.locator('#Email').fill('john@example.com');

  // 4. 年齢を入力
  await page.locator('#Age').fill('30');

  // 5. 国を選択
  await page.getByText('Japan USAJapanUKGermanyFranceCanada').click();
  await page.getByRole('option', { name: 'USA' }).click();

  // 6. Submitボタンを押下
  await page.getByRole('button', { name: 'Submit' }).click();

  // 7. 成功メッセージを確認
  const successMessage = page.getByText('Success!');
  await expect(successMessage).toBeVisible();

  // 8. スクリーンショットを保存
  await page.screenshot({
    path: 'test-form-success.png',
    fullPage: true
  });
});
```

**実行結果**：
```
> playwright test --headed

Running 1 test using 1 worker
[chromium] › test-form.spec.ts:7:5 › フォーム入力から送信までの一連の操作
1. Test Formページに移動中...
2. 名前を入力中...
3. メールアドレスを入力中...
4. 年齢を入力中...
5. 国を選択中...
6. Submitボタンを押下中...
7. 成功メッセージを確認中...
8. スクリーンショットを保存中...
✅ すべての操作が完了しました！
  1 passed (12.5s)

To open last HTML report run:

  npx playwright show-report
```

**違い**：
- Playwright MCP：自然言語で指示するだけ（約7行）
- 従来のPlaywright：TypeScriptコードを書く必要がある（約30行）

どちらも同じ操作を実行しますが、**Playwright MCPならプログラミング知識不要**です。

:::

### テストシナリオ2：バリデーションテスト

AIへの指示：
```
http://localhost:5158/test-form にアクセスして、以下を確認してください：

1. 何も入力しないでSubmitボタンを押す
2. "Name is required" ってエラーが出るか確認
3. 名前の欄に "Test User" って入力
4. メールアドレスの欄に "invalid-email" って入力（わざと変な形式で）
5. Submitボタンを押す
6. "Invalid email format" ってエラーが出るか確認
```

期待結果：
- 必須フィールドが空だとエラーメッセージが出る
- メール形式が違うとバリデーションエラーが出る

**ポイント**：
バリデーションエラーみたいな動的な挙動も、「エラーメッセージが表示されることを確認」って言うだけでAIが検証してくれます。

### テストシナリオ3：DataGridの編集・削除

AIへの指示：
```
http://localhost:5158/data-grid にアクセスして、以下を実行してください：

1. 一番上の商品（Laptop）の編集ボタンを押す
2. 値段を "1499.99" に変える
3. Saveボタンを押す
4. 成功メッセージが出るか確認
5. 表の値段が変わったか確認
```

期待結果：
- 編集ボタン押すと編集フォームが出る
- 変更が保存される
- グリッドの表示が更新される

**ポイント**：
複雑なUIコンポーネント（RadzenDataGrid）でも、「一番上の商品の編集ボタン」「値段を変える」って普通に言えばAIが要素を見つけます。セレクター書かなくていいんです。

## Playwright MCPのセットアップ

実際に試したい人向けに、セットアップ手順を簡単に。

詳細は [Playwright MCP公式リポジトリ](https://github.com/microsoft/playwright-mcp) を参照してください。

### 前提条件

- Node.js（v18以上）
- Claude Code、Claude Desktop、またはMCP対応のAIクライアント
- テスト対象のアプリケーションが起動していること

### MCPサーバーの設定

`playwright-config.json` を作成：

```json
{
  "mcpServers": {
    "playwright": {
      "command": "npx",
      "args": [
        "@playwright/mcp@latest"
      ]
    }
  }
}
```

### Claude Codeでの使い方

```bash
# MCP Serverを追加
claude mcp add playwright npx @playwright/mcp@latest

# 設定を確認
claude mcp

# アプリを起動
dotnet run  # または npm start 等
```

Claude Codeで以下のように指示するだけ：

```
http://localhost:5158/test-form にアクセして、
名前の欄に "Test User" って入力して、Submitボタンを押してください
```

### 実行許可の確認について

初回実行時、Claude Codeはブラウザ操作の実行許可を求めます：

![実行許可の確認画面](/images/playwright-mcp-blazor-test/claude-code-execution-approval.png)

この確認をスキップしたい場合、Claude Codeの設定で事前承認することも可能です。ただし、ブラウザ操作は重要な操作を伴う可能性があるため、セキュリティ上、初回は確認することを推奨します。

特に以下のような操作を行う場合は注意が必要です：
- フォーム送信（データの登録・更新・削除）
- 外部サイトへのアクセス
- 認証情報の入力

テスト環境で繰り返し実行する場合のみ、事前承認の設定を検討してください。

## Playwright MCPの仕組み

Playwright MCPは、AIエージェントとPlaywrightをつなぐ橋渡し役です。

**従来のPlaywright**：
```
人間 → JavaScriptコード → Playwright → ブラウザ
```

**Playwright MCP**：
```
人間 → 自然言語 → AI → MCP → Playwright → ブラウザ
```

データの流れ：
```
人間 → AIエージェント → MCP Server → Playwright → ブラウザ
   ←      ←           ←         ←       ←
  (結果が戻ってくる)
```

MCP（Model Context Protocol）は、Anthropicが開発した、AIエージェントが外部ツールと連携するための標準プロトコルです。Playwright MCP Serverは、このプロトコルを使ってPlaywrightの機能をAIから使えるようにしています。

## 使い分けの基準

### Playwright MCPを使うべき場面

- 新機能のリリース前の動作確認（1回きり）
- PdMの受け入れテスト
- 顧客報告の不具合再現
- UIの見た目確認
- 探索的テスト

### 従来のPlaywrightを使うべき場面

- ログイン → 操作 → ログアウトの定型フロー
- 毎日実行するリグレッションテスト
- パフォーマンス測定
- 複雑な条件分岐があるテスト
- CI/CDパイプラインでの自動実行

## まとめ

Playwright MCPで、ブラウザテストのやり方が変わりつつあります。

従来：
- テスト自動化 = プログラマーの仕事
- JavaScriptコード書く（※TypeScript、Python、Java、C#など多言語でも書けます）
- セレクター細かく指定

Playwright MCP：
- 誰でも自然言語でテストできる
- コード書かない
- AIが要素を推測

品質保証の民主化が進んでいます。開発、QA、PdM、デザイナー、カスタマーサポート、誰でもブラウザテストできる時代になりました。

従来型のPlaywrightとPlaywright MCPをうまく使い分けることで、チーム全体の生産性と品質が上がります。

## 参考リンク

- [サンプルコード（GitHub）](https://github.com/pixcelo/zenn-content/tree/main/samples/blazor-playwright-test)
- [Playwright MCP Server](https://github.com/microsoft/playwright-mcp)
- [Playwright公式ドキュメント](https://playwright.dev/)
- [Radzen Blazor Components](https://blazor.radzen.com/)
- [Blazor | Microsoft Learn](https://learn.microsoft.com/ja-jp/aspnet/core/blazor/)
