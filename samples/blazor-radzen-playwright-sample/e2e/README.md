# Blazor Radzen Playwright Sample - E2E Tests

Blazor Radzen アプリケーションの E2E テストプロジェクトです。

## セットアップ

### 1. 依存関係のインストール

```bash
npm install
```

### 2. Playwright ブラウザのインストール

```bash
npx playwright install
```

### 3. 環境変数の設定

`.env` ファイルを作成し、以下の設定を追加してください：

```env
BASE_URL=http://localhost:5000
HEADLESS=false
```

`.env.example` を参考にしてください。

**注意**: `HEADLESS=false` を設定すると、テスト実行時にブラウザが表示されます。

## テストの実行

### すべてのテストを実行

```bash
npm test
```

### UI モードで実行

```bash
npm run test:ui
```

### ヘッドモードで実行（ブラウザを表示）

```bash
npm run test:headed
```

### デバッグモードで実行

```bash
npm run test:debug
```

## テスト内容

### example.spec.ts
Blazor アプリケーションの基本的な動作確認テスト
- ページタイトルの確認
- ウェルカムメッセージの表示確認

### datagrid-status.spec.ts
DataGrid 状態列のE2Eテスト

#### smoke カテゴリ（基本動作確認）
- 初期データに3つの異なる状態が表示される
- 完了状態の行をクリックして選択できる
- 完了状態の行に正しい表示テキストが含まれる
- 新規行を追加すると未着手状態になる
- すべての状態列にdata-status属性が存在する

#### regression カテゴリ（詳細機能テスト）
- 複数行を順番に選択できる
- 状態ごとの行数が正しくカウントされる

## ファイル構成

```
e2e/
├── package.json              # プロジェクト設定
├── playwright.config.ts      # Playwright 設定
├── .env.example              # 環境変数のテンプレート
├── .gitignore               # Git 除外設定
├── pages/                    # Page Objectクラス
│   └── DataGridDemoPage.ts  # DataGridDemo ページのPage Object
├── tests/
│   ├── example.spec.ts      # 基本動作確認テスト
│   └── datagrid-status.spec.ts  # DataGrid 状態列テスト
└── README.md                # このファイル
```

## 注意事項

- テストを実行する前に、Blazor アプリケーションが `http://localhost:5000` で起動していることを確認してください
- 環境変数は `.env` ファイルに設定してください（`.gitignore` に追加済み）
