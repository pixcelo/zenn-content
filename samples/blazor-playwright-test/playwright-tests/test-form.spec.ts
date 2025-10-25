import { test, expect } from '@playwright/test';

/**
 * Test Form自動化テスト
 * http://localhost:5158/test-form でのフォーム入力を自動化
 */
test('フォーム入力から送信までの一連の操作', async ({ page }) => {
  // 1. Test Formページに移動
  console.log('1. Test Formページに移動中...');
  await page.goto('http://localhost:5158/test-form');

  // Blazorアプリの初期化を待つ
  await page.waitForLoadState('networkidle');

  // 2. 名前を入力
  console.log('2. 名前を入力中...');
  await page.locator('#Name').fill('John Doe');

  // 3. メールアドレスを入力
  console.log('3. メールアドレスを入力中...');
  await page.locator('#Email').fill('john@example.com');

  // 4. 年齢を入力
  console.log('4. 年齢を入力中...');
  await page.locator('#Age').fill('30');

  // 5. 国を選択
  console.log('5. 国を選択中...');
  // ドロップダウンをクリックして開く
  await page.getByText('Japan USAJapanUKGermanyFranceCanada').click();
  // USAオプションを選択
  await page.getByRole('option', { name: 'USA' }).click();

  // 6. Submitボタンを押下
  console.log('6. Submitボタンを押下中...');
  await page.getByRole('button', { name: 'Submit' }).click();

  // 7. 成功メッセージを確認
  console.log('7. 成功メッセージを確認中...');
  const successMessage = page.getByText('Success!');
  await expect(successMessage).toBeVisible();

  const fullMessage = page.getByText('Form submitted successfully! Name: John Doe, Email: john@example.com');
  await expect(fullMessage).toBeVisible();

  // 8. スクリーンショットを保存
  console.log('8. スクリーンショットを保存中...');
  await page.screenshot({
    path: 'test-form-success.png',
    fullPage: true
  });

  console.log('✅ すべての操作が完了しました！');
});
