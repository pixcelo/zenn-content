import { test, expect } from '@playwright/test';

test('has title', async ({ page }) => {
  await page.goto('/');

  // Blazor Web Appのデフォルトタイトルを確認
  await expect(page).toHaveTitle(/Home/);
});

test('has welcome heading', async ({ page }) => {
  await page.goto('/');

  // Blazorアプリのホームページに"Hello, world!"が表示されているか確認
  await expect(page.getByRole('heading', { name: 'Hello, world!' })).toBeVisible();
});
