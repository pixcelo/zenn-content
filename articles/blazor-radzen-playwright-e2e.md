---
title: "PlaywrightでBlazorアプリのE2Eテストを始めました"
emoji: "🤖"
type: "tech" # tech: 技術記事 / idea: アイデア
topics: ["blazor", "csharp", "dotnet", "playwright", "web開発"]
published: false
publication_name: "nexta_" # 企業のPublication名を指定
---

私たちのチームでは Blazor を用いた Web アプリケーションを開発しています。プロダクトの成長に伴い、この度 E2E テストフレームワーク「Playwright」 をプロジェクトに導入しました。

これまでは UI/UX の変動が激しかったこともあり、テストコードは主に xUnit によるロジック検証が中心でした。しかし、データ一覧や詳細入力といったメイン機能の UX 方針が固まってきたこのタイミングで、ブラウザを介したユーザー視点でのテストを本格的に検証し、導入に踏み切りました。

本記事では、Blazor プロジェクトにおける E2E の構成や、実際に動かしてみて分かった知見を共有します。

:::message
本記事で紹介するPlaywrightの構成やツール選定は、筆者の検証結果に基づくものです。導入初期段階のため、より良い実装方法が存在する可能性があります。あくまで一例として参考にしていただければと思います。
:::

## プロジェクト構成

Blazorアプリ本体から独立したE2Eテストプロジェクトとして構成しています。

```
my-app/
├── src/              # アプリケーション本体
└── e2e/              # E2Eテスト（独立プロジェクト）
    ├── pages/        # ページオブジェクトモデル
    ├── tests/
    │   ├── basic/    # 基本動作確認
    │   └── scenario/  # テストシナリオ検証
    ├── fixtures/     # 認証状態復元
    ├── utils/        # ユーティリティ
    └── playwright.config.ts
```

- Page Objectパターンでテストコードとセレクタを分離した
- 認証状態の永続化できるようにした
- 段階的テスト（basic（基本）→ scenario（テストシナリオ検証））

この辺りの構成は調査しても良い例がなかったため、書籍を参考にAIと作りました。

## 実装言語は TypeScript/JavaScript

PlaywrightはBlazorに合わせてC#でも書けるのですが、TypeScriptを使用しています。

理由として、以下のものが挙げられます。
- VSCode拡張の機能が充実している
  - UI Modeでの対話的なデバッグ
  - Codegenによるテストコード自動生成
  - Locatorのリアルタイム編集とプレビュー
- 他プロジェクトでも知見がそのまま使える
- ネットなどで調べたときの記事が多く、学習コストが低い

C#で統一するメリットもありますが、様々な機能をフルで活用するならTypeScript/JavaScriptで書くのが一番良いのではという判断です。

## 実装イメージ（ページオブジェクトモデル）

ページオブジェクトモデルで構成しています。
（テストスイートを構造化するためのアプローチ）

ページオブジェクトに「要素の特定」と「操作」のみを持たせることで、テストケースを見ただけで「何を検証しているのか」が明確になります。

:::details ページオブジェクトモデル実装例

```typescript
import { type Locator, type Page } from '@playwright/test';

/**
 * 一覧ページのページオブジェクト
 *
 * Thin POMスタイル: ロケータ定義 + goto() + 基本アクションのみ
 * アサーションはテスト側で実行
 */
export class ListPage {
  readonly page: Page;
  readonly newButton: Locator;
  readonly dataGrid: Locator;

  constructor(page: Page) {
    this.page = page;
    this.newButton = page.getByRole('button', { name: /新規|add/i });
    this.dataGrid = page.getByRole('table').first();
  }

  /**
   * ページに遷移します
   */
  async goto(): Promise<void> {
    await this.page.goto('/list', {
      waitUntil: 'domcontentloaded',
      timeout: 60_000
    });

    await this.page.waitForURL(/\/list\/?$/, { timeout: 60_000 });

    await this.dataGrid.waitFor({ state: 'visible', timeout: 30_000 });
  }

  /**
   * グリッドの最初の行を選択します
   */
  async selectFirstRow(): Promise<void> {    
    const dataRows = this.dataGrid.getByRole('row').filter({ hasNotText: /列名|Column/ });

    await dataRows.first().waitFor({ state: 'visible', timeout: 30_000 });

    await dataRows.first().click();
  }

  /**
   * 新規ボタンをクリックします
   */
  async clickNewButton(): Promise<void> {
    await this.newButton.click();
  }
}
```

:::

このページオブジェクトを使用したテストコードは以下のようになります。

:::details テストコード例

```typescript
import { test, expect } from '@playwright/test';
import { ListPage } from '../pages/ListPage';

test.describe('一覧ページのテスト', () => {
  test('新規ボタンで明細ダイアログが開く', async ({ page }) => {
    const listPage = new ListPage(page);

    await test.step('ページに移動', async () => {
      await listPage.goto();
      // Thin POMスタイルなので、アサーションはテスト側で実行
      await expect(listPage.dataGrid).toBeVisible({ timeout: 30_000 });
    });

    await test.step('新規ボタンをクリック', async () => {
      await expect(listPage.newButton.first()).toBeEnabled({ timeout: 30_000 });
      await listPage.clickNewButton();
    });

    await test.step('詳細ダイアログが表示されることを確認', async () => {
      const dialog = page.getByRole('dialog').first();
      await expect(dialog).toBeVisible({ timeout: 10_000 });
    });
  });

});
```

:::

Page Objectパターンを使うことで、テストコードが読みやすく、保守しやすくなります。セレクタやBlazor特有の待機処理はPage Objectに隠蔽され、テストコードは「何をテストするか」に集中できます。


## テストコードの実行

テストコードはCLIから実行できます。

ファイル指定
```powershell
npx playwright test tests/smoke/home.spec.ts
```

または

```powershell
# ブラウザ表示
npx playwright test tests/smoke/home.spec.ts --headed
```

実行ログ（ここは折りたたむ）
実行するとこのようなログが表示されます

```powershell
PS C:\Users\user\repos\sample\Sample.E2E> npx playwright test tests/smoke/home.spec.ts    
[dotenv@17.2.3] injecting env (4) from .env -- tip: 👥 sync secrets across teammates & machines: https://dotenvx.com/ops

Running 2 tests using 1 worker
[dotenv@17.2.3] injecting env (0) from .env -- tip: 🔑 add access controls to secrets: https://dotenvx.com/ops
[dotenv@17.2.3] injecting env (0) from .env -- tip: 🗂️ backup and recover secrets: https://dotenvx.com/ops
  2 passed (11.4s)

To open last HTML report run:

  npx playwright show-report artifacts\playwright-report
```

[VSCodeの拡張](https://marketplace.visualstudio.com/items?itemName=ms-playwright.playwright)がインストール済みであれば、
VSCodeの左メニューにフラスコのアイコンが表示されます。

ここから各テストをクリックして、実行することが可能です。

![VSCodeのテスト実行パネル](/images/blazor-radzen-playwright-e2e/vscode-test-explorer.png)

他にもUIモードでの実行もあるのですが、本記事では割愛します。

```powershell
npx playwright test --ui
```

今（2026年）なら、AIにCLIから実行させてもOKですね。

## テストコードの作り方

テストコードを作る方法を紹介します。

1. 画面操作からテストコードを生成

VSCodeの拡張メニュー「PLAYWRIGHT」→「Record new」をクリックすると、
画面操作をそのままテストコードに変換することが可能です。

![Playwright Codegen デモ](/images/blazor-radzen-playwright-e2e/playwright-codegen-demo.png)

画面をポチポチしながらテストコードを実装できます。

![Playwright Record モード](/images/blazor-radzen-playwright-e2e/vscode-playwright-record-mode.gif)

CLIから直接実行する場合は、以下のコマンドでも同様の操作が可能です：
```powershell
npx playwright codegen --browser=chromium http://localhost:8001
```

2. AIによるテストコード生成

AIに直接テストコードの出力を依頼します。
最初にある程度、どのようなロケーターでUIを操作できるかを
Recordモードで試しておくと良いです。

実装済みテストコードをAIにコンテキストとして渡せば、
既存に合わせて実装計画を立ててくれます。

3. MCPによる操作のテストコード出力

[Playwright MCP](https://github.com/microsoft/playwright-mcp) で操作した結果を、テストコードとして出力することも可能です。
AIと対話しながら画面を操作し、その結果をコードとして出力するように指示します。

:::details AIが生成したテストコード例

```typescript
はい、先ほどの操作をPlaywrightのテストコードとして出力します。

  import { test, expect } from '@playwright/test';

  test('ログイン操作', async ({ page }) => {
    // ログインページへ移動
    await page.goto('http://localhost:PORT/login');

    // フォームに入力    
    await page.getByRole('textbox').filter({ hasText: 'ユーザーコード' }).fill('USER_ID');
    await page.locator('input[type="password"]').fill('PASSWORD');

    // Enterキーでログイン実行
    await page.keyboard.press('Enter');

    // または、ログインボタンをクリックする場合
    // await page.getByRole('button', { name: 'check ログイン' }).click();

    // ログイン後のページ遷移を待機
    await page.waitForURL('http://localhost:PORT/**');

    // ログイン成功の確認（例：URLチェック）
    expect(page.url()).not.toContain('/login');
  });

  より堅牢な書き方（Page Object使用）

  既存のE2Eプロジェクトのスタイルに合わせるなら：

  import { test, expect } from '@playwright/test';

  test('環境でログイン', async ({ page }) => {
    // ログインページへ移動
    await page.goto('http://localhost:PORT/login');

    // 読み込み完了を待機
    await page.waitForLoadState('networkidle');

    // ユーザーコード入力
    const userFields = page.getByRole('textbox');
    await userFields.nth(1).fill('USER_ID');

    // パスワード入力
    await page.locator('input[type="password"]').fill('PASSWORD');

    // ログインボタンが有効になるまで待機
    const loginButton = page.getByRole('button', { name: /ログイン/ });
    await expect(loginButton).toBeEnabled();

    // ログインボタンをクリック
    await loginButton.click();

    // ログイン後のページ遷移を待機（タイムアウト延長）
    await page.waitForURL('http://localhost:PORT/**', { timeout: 10000 });

    // ログイン成功の確認
    expect(page.url()).not.toContain('/login');
  });

```

:::

## ログイン認証の仕組み

ログイン画面からトップメニューへの遷移は、以下のフローで実装しました。

1. `.env`から認証情報を読み込み
2. ログインフォームに入力してUI経由でログイン
3. 認証状態を保存
    - Cookie（認証トークン）
    - sessionStorage（アプリ状態）
    - localStorage（ユーザー設定）

初回以降は、認証状態を復元することでログイン処理をスキップしてテストを高速化しています。ユーザー権限ごとのテストを並列で回すことを想定して、`.env`には複数ユーザーを設定できるようにしました。

## 一覧ページの表示

一覧画面には、表示するデータを絞り込むための「UI設定パネル」があります。
ユーザーが選択したビュー設定はlocalStorageに保存されます。

通常のブラウザ操作では、前回の設定が自動復元されてデータが表示されますが、
Playwrightでは認証状態復元時にlocalStorageが上書きされるため、ビュー設定がクリアされてしまいます。

対処方法として、テスト開始時に「ビューが未選択なら自動選択」するメソッドを実装しました。
これにより、UI表示パネルに設定が無い状態でもデータを一覧表示できるようになりました。

## 一覧ページのデータ操作

データを一覧表示しているUIコンポーネントは、Radzenをカスタマイズした独自コンポーネントです。データに対して操作をしたいと思ったら、このテーブルに動的に表示される各行に対して、何らかの条件でデータを操作する必要があります。

現状の課題

- 動的に生成されるDOM要素の操作方法に悩む
- RadzenをカスタマイズしたUIをAIが一見で理解できない

Radzenのような高機能なUIライブラリは、生成されるHTML構造が複雑なため、単純なCSSセレクタではなく、Playwrightが推奨する getByRole や getByText を駆使して、ユーザーの視点に近いロケーターを定義するのが安定化の近道だと感じています。


## まとめ

現在は基本的な動作確認ができている段階です。

今後はテストの安定性向上と実行速度の改善に取り組みます。また、AI連携によるテストコード生成の効率化や、CI/CDパイプラインへの組み込みを進め、プロダクトの品質向上に活かしていきたいです。

※参考にした書籍がとても役立ちました。これから導入する方におすすめの一冊です！


## 参考
- [［入門］Webフロントエンド E2E テスト PlaywrightによるWebアプリの自動テストから良いテストの書き方まで（技術評論社/渋川よしき）](https://gihyo.jp/book/2024/978-4-297-14220-9)
- [Software Design 2026年2月号](https://gihyo.jp/magazine/SD/archive/2026/202602)
- [Playwright docs](https://playwright.dokyumento.jp/docs/intro)
- [Playwright Test for VSCode](https://marketplace.visualstudio.com/items?itemName=ms-playwright.playwright)
- [Playwright MCP](https://github.com/microsoft/playwright-mcp)