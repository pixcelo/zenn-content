import { defineConfig, devices } from '@playwright/test';

export default defineConfig({
  testDir: './',
  testMatch: '**/*.spec.ts',

  // テスト実行設定
  fullyParallel: false, // フォームテストは順次実行
  forbidOnly: !!process.env.CI,
  retries: process.env.CI ? 2 : 0,
  workers: 1,

  // レポート設定
  reporter: 'html',

  use: {
    // ベースURL（必要に応じて変更）
    baseURL: 'http://localhost:5158',

    // スクリーンショット・ビデオ設定
    screenshot: 'only-on-failure',
    video: 'retain-on-failure',

    // トレース設定（デバッグ用）
    trace: 'on-first-retry',
  },

  // ブラウザ設定
  projects: [
    {
      name: 'chromium',
      use: {
        ...devices['Desktop Chrome'],
        // 動作を見やすくする設定
        launchOptions: {
          slowMo: 500, // 各操作を500ms遅延
        }
      },
    },
  ],

  // 開発サーバー設定（オプション）
  // Blazorアプリを自動起動したい場合はコメント解除
  // webServer: {
  //   command: 'dotnet run --project BlazorRadzenApp',
  //   url: 'http://localhost:5158',
  //   reuseExistingServer: !process.env.CI,
  // },
});
