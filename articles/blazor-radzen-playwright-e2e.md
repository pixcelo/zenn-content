---
title: "PlaywrightでBlazorアプリのE2Eテストを始めました"
emoji: "🤖"
type: "tech" # tech: 技術記事 / idea: アイデア
topics: ["blazor", "csharp", "dotnet", "playwright", "web開発"]
published: false
publication_name: "nexta_" # 企業のPublication名を指定
---

ネクスタでsmartFの開発エンジニアをしている tetsu.k です。

私たちのチームでは Blazor を用いた Web アプリケーションを開発しています。プロダクトの成長に伴い、この度 E2E テストフレームワーク「Playwright」 をプロジェクトに試験導入しました。

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

この辺りの構成は調査しても良い例が見つからなかったため、書籍を参考にAIと作ったものです。

## 実装言語は組織に応じて選択できる

Playwrightは、複数の言語をサポートしています。

- TypeScript/JavaScript
- Python
- Java
- .NET

https://playwright.dokyumento.jp/docs/languages

筆者は、総合的に見て実装言語はTypeScript/JavaScriptが良いのではと考えています。
test.step・HTMLレポート・UI Modeなどの標準機能が充実しているためです。

### 主要機能の比較

| 項目 | TypeScript版 | C#版 |
|------|-------------|------|
| **test.step** | ✅ ステップ単位の構造化 | ❌ 標準サポートなし |
| **HTMLレポート** | ✅ リッチUIで自動生成 | ❌ TRX形式のみ |
| **UI Mode** | ✅ GUIデバッグ可能 | ❌ 提供なし |
| **VS統合** | ❌ なし | ✅ Test Explorer対応 |
| **学習コスト** | △ 2-3日必要 | ○ 既存スキルで即開始 |
| **型安全性** | △ 実行時エラー可能性 | ○ コンパイル時チェック |

PlaywrightはTypeScript生まれのツールのため、TypeScript版が最も機能が充実しています。
C#で統一するメリットもありますが、テストコードは自動生成（Codegen/AI/MCP）が基本となるため、言語による学習コストの差は小さいと考えています。

:::details 主要機能の比較

## 実装面での違い

TypeScript版（以下、TS版と省略）はテスト内の論理分割ができます。

具体的には、`test.describe`ブロックでグループ化し、
さらに`test.step`で「追加する」「保存する」などの手順を構造化することができます。

C#版では、`[Trait]`によるグループ化と`[Fact]`によるメソッド分割までとなり、結果分析の面で劣ります。

以下は、C#版での実装例です。(TS版は後述)

※サンプルでは使っていませんが、`PageTest`基底クラスが用意されています

```csharp
/// <summary>
/// ホーム画面の基本テスト
/// storageStateを使用して認証済み状態でテストします
/// </summary>
[Trait("TestType", "E2E")]
[Trait("Category", "Common")]
[Trait("Feature", "Home")]
public class HomeTests : IAsyncLifetime
{
    private TestSession _session = null!;
    private IPage Page => _session.Page;

    public async ValueTask InitializeAsync()
    {
        _session = await TestSession.CreateAuthenticatedAsync();
    }

    [Fact(DisplayName = "正常系: StorageStateを使用してホーム画面を表示できること")]
    public async Task Home_IsVisible_WithStorageState()
    {
        var homePage = new HomePage(Page);
        await homePage.GotoAsync();
        
        await Expect(Page).ToHaveURLAsync(new Regex(@"\/$"));
        await Expect(homePage.Logo).ToBeVisibleAsync();
    }
}
```

## レポートの違い

TS版では、組込のHTMLレポートが利用できます。

![HTMLレポート](/images/blazor-radzen-playwright-e2e/html-report.png)

各行をクリックすると、さらに詳細なレポート（エラー箇所、Test Steps、スクリーンショット、動画）等を確認できます。

![HTMLレポート詳細](/images/blazor-radzen-playwright-e2e/html-report-detail.png)

C#版にはレポート機能が不足しているため、`Trx（Test Results XML）`を独自に加工して出力する形になります。

```xml
<?xml version="1.0" encoding="utf-8"?>
<TestRun id="..." name="user@PC-DEV099 2026-03-29 04:50:54">
  <Results>
    <UnitTestResult testName="正常系: データグリッドで行を選択できること" 
                    outcome="Failed" 
                    duration="00:00:10.8551472">
      <Output>
        <ErrorInfo>
          <Message>System.InvalidOperationException : Authentication expired...</Message>
          <StackTrace>...</StackTrace>
        </ErrorInfo>
      </Output>
    </UnitTestResult>
  </Results>
</TestRun>
```
C#版の場合、visual studioのテストエクスプローラーでの表示であれば可能です。

![テストエクスプローラー](/images/blazor-radzen-playwright-e2e/test-explorer.png)

:::

## 実装イメージ（ページオブジェクトモデル）

ページオブジェクトパターンで構成しています。
（テストスイートを構造化するためのアプローチ）

ページオブジェクトに「要素の特定」と「操作」のみを持たせることで、テストケースを見ただけで「何を検証しているのか」が明確になります。

:::details ページオブジェクトモデル実装例

```typescript
import { type Locator, type Page } from '@playwright/test';

/**
 * 一覧ページのページオブジェクトモデル
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

このページオブジェクトモデルを使用したテストコードは以下のようになります。

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

ページオブジェクトモデルを使うことで、テストコードが読みやすく、保守しやすくなります。セレクタやBlazor特有の待機処理はモデルに隠蔽され、テストコードは「何をテストするか」に集中できます。


## テストコードの作り方

テストコードを作る方法を紹介します。

### 1. 画面操作からテストコードを生成

VSCodeの拡張メニュー「PLAYWRIGHT」→「Record new」をクリックすると、
画面操作をそのままテストコードに変換することが可能です。

![Playwright Codegen デモ](/images/blazor-radzen-playwright-e2e/playwright-codegen-demo.png)

画面をポチポチしながらテストコードを実装できます。

![Playwright Record モード](/images/blazor-radzen-playwright-e2e/vscode-playwright-record-mode.gif)

CLIから直接実行する場合は、以下のコマンドでも同様の操作が可能です：
```powershell
npx playwright codegen --browser=chromium http://localhost:8001
```

### 2. AIによるテストコード生成

AIに直接テストコードの出力を依頼します。
最初にある程度、どのようなロケーターでUIを操作できるかを
Recordモードで試しておくと良いです。

実装済みテストコードをAIにコンテキストとして渡せば、
既存に合わせて実装計画を立ててくれます。

### 3. MCPによる操作のテストコード出力

[Playwright MCP](https://github.com/microsoft/playwright-mcp) で操作した結果を、テストコードとして出力することも可能です。
AIと対話しながら画面を操作し、その結果をコードとして出力するように指示します。

:::details AIが生成したテストコード例（Claude Code - Sonnet 4.5での結果）
※AIの出力をそのまま掲載しています

ユーザー：先ほどの操作をPlaywrightのテストコードとして出力してください

AI：はい、先ほどの操作をPlaywrightのテストコードとして出力します。

```typescript
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
```

:::

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

実行すると、このようなログが表示されます

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

2026年現在では、AIにCLIから実行させてもOKですね。

## ログイン認証の仕組み

ログイン画面からトップメニューへの遷移は、以下のフローで実装しました。

1. `.env`から認証情報を読み込み
2. ログインフォームに入力してUI経由でログイン
3. 認証状態を保存
    - Cookie（認証トークン）
    - sessionStorage（アプリ状態）
    - localStorage（ユーザー設定）

初回以降は、認証状態を復元することでログイン処理をスキップしてテストを高速化しています。ユーザー権限ごとのテストを並列で回すことを想定して、`.env`には複数ユーザーを設定できるようにしました。

## Blazor×Radzen特有の課題と対策

### localStorageの上書き問題

一覧画面には、表示するデータを絞り込むための「UI設定パネル」があります。
ユーザーが選択したビュー設定はlocalStorageに保存されます。

通常のブラウザ操作では、前回の設定が自動復元されてデータが表示されますが、
Playwrightでは認証状態復元時にlocalStorageが上書きされるため、ビュー設定がクリアされてしまいます。

対処方法として、テスト開始時に「ビューが未選択なら自動選択」するメソッドを実装しました。
これにより、UI設定パネルに設定が無い状態でもデータを一覧表示できるようになりました。

### Radzenカスタムコンポーネントの操作

データを一覧表示しているUIコンポーネントは、Radzenをカスタマイズした独自コンポーネントです。

現状の課題:
- 動的に生成されるDOM要素の操作方法に悩む
- RadzenをカスタマイズしたUIをAIが一見で理解できない

対策として、Playwrightが推奨する getByRole や getByText を駆使し、ユーザーの視点に近いロケーターを定義することで安定性を向上させています。
単純なCSSセレクタに頼らず、Radzenのような高機能UIライブラリの複雑なHTML構造にも対応できる設計を意識しています。


## まとめ

PlaywrightをBlazorプロジェクトに試験導入し、以下を実現しました。

- Page Objectパターンによる保守性の高いテスト設計
- 認証状態の永続化による高速なテスト実行
- Codegen/AI/MCPによる効率的なテストコード生成

現在は、基本的な動作確認ができている段階です。
今後はテストの安定性向上と実行速度の改善に取り組んでいきたいと思います。

特にテストコード生成の効率化については、別記事でまとめる予定です。
E2E導入において同様の課題を抱えている方の参考になれば幸いです。

※参考にした書籍がとても役立ちました。これから導入する方におすすめです！


## 参考
- [［入門］Webフロントエンド E2E テスト PlaywrightによるWebアプリの自動テストから良いテストの書き方まで（技術評論社/渋川よしき）](https://gihyo.jp/book/2024/978-4-297-14220-9)
- [Software Design 2026年2月号](https://gihyo.jp/magazine/SD/archive/2026/202602)
- [Playwright docs](https://playwright.dokyumento.jp/docs/intro)
- [Playwright Test for VSCode](https://marketplace.visualstudio.com/items?itemName=ms-playwright.playwright)
- [Playwright for .NET](https://github.com/microsoft/playwright-dotnet)
- [Playwright MCP](https://github.com/microsoft/playwright-mcp)