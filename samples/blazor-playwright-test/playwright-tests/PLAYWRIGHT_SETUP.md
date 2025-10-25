# Playwright テストセットアップガイド

## 初回セットアップ

### 1. 依存関係のインストール
```bash
npm install
```

### 2. Playwrightブラウザのインストール
```bash
npx playwright install
```

## テストの実行方法

### 基本的な実行
```bash
npm test
```

### ブラウザを表示して実行（動作確認用）
```bash
npm run test:headed
```

### デバッグモードで実行
```bash
npm run test:debug
```

### UIモードで実行（インタラクティブ）
```bash
npm run test:ui
```

## 実行前の準備

テストを実行する前に、Blazorアプリが起動している必要があります：

```bash
cd BlazorRadzenApp
dotnet run
```

アプリが `http://localhost:5158` で起動していることを確認してください。

## テストファイル

- **test-form.spec.ts** - Test Formページの自動化テスト
  - フォーム入力（名前、メール、年齢）
  - ドロップダウン選択（国）
  - 送信ボタンクリック
  - 成功メッセージの検証
  - スクリーンショット保存

## 生成されるファイル

- `test-form-success.png` - テスト成功時のスクリーンショット
- `test-results/` - テスト実行結果
- `playwright-report/` - HTMLレポート

## トラブルシューティング

### ブラウザが起動しない
```bash
npx playwright install chromium
```

### ポートが違う場合
`playwright.config.ts` の `baseURL` を変更してください。
