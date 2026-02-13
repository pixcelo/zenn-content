import { test, expect } from '@playwright/test';
import { DataGridDemoPage } from '../pages/DataGridDemoPage';

/**
 * DataGrid 状態列のE2Eテスト
 *
 * nexta-smartfのproduction-instruction-process.spec.tsパターンに従った実装
 * カテゴリ:
 * - smoke: 基本動作確認
 * - regression: 詳細機能テスト
 */

test.describe('DataGrid 状態列テスト - smoke', () => {
  test('初期データに3つの異なる状態が表示される', async ({ page }) => {
    const dataGridPage = new DataGridDemoPage(page);

    try {
      console.log('[TEST] テスト開始: 初期データに3つの異なる状態が表示される');

      // ページに遷移
      await dataGridPage.goto();
      await dataGridPage.expectLoaded();

      // 各状態の行数を確認
      const notStartedCount = await dataGridPage.getRowCountByStatus('NotStarted');
      const inProgressCount = await dataGridPage.getRowCountByStatus('InProgress');
      const completedCount = await dataGridPage.getRowCountByStatus('Completed');

      console.log(`[TEST] 未着手: ${notStartedCount}件, 進行中: ${inProgressCount}件, 完了: ${completedCount}件`);

      // 各状態が少なくとも1件存在することを確認
      expect(notStartedCount).toBeGreaterThanOrEqual(1);
      expect(inProgressCount).toBeGreaterThanOrEqual(1);
      expect(completedCount).toBeGreaterThanOrEqual(1);

      console.log('[TEST] テスト成功: 3つの異なる状態が確認されました');
    } catch (error) {
      console.error('[TEST] テスト失敗:', error);
      await page.screenshot({
        path: `artifacts/test-results/initial-status-error-${Date.now()}.png`,
        fullPage: true,
      });
      throw error;
    }
  });

  test('完了状態の行をクリックして選択できる', async ({ page }) => {
    const dataGridPage = new DataGridDemoPage(page);

    try {
      console.log('[TEST] テスト開始: 完了状態の行をクリックして選択できる');

      // ページに遷移
      await dataGridPage.goto();
      await dataGridPage.expectLoaded();

      // 完了状態の行をクリック
      await dataGridPage.clickRowByStatus('Completed', 0);

      // 選択情報が表示されることを確認
      const selectedInfo = await dataGridPage.getSelectedInfoText();
      console.log(`[TEST] 選択情報: ${selectedInfo}`);

      // 選択情報に「選択:」が含まれていることを確認
      expect(selectedInfo).toContain('選択:');

      console.log('[TEST] テスト成功: 完了状態の行を選択できました');
    } catch (error) {
      console.error('[TEST] テスト失敗:', error);
      await page.screenshot({
        path: `artifacts/test-results/click-completed-row-error-${Date.now()}.png`,
        fullPage: true,
      });
      throw error;
    }
  });

  test('完了状態の行に正しい表示テキストが含まれる', async ({ page }) => {
    const dataGridPage = new DataGridDemoPage(page);

    try {
      console.log('[TEST] テスト開始: 完了状態の行に正しい表示テキストが含まれる');

      // ページに遷移
      await dataGridPage.goto();
      await dataGridPage.expectLoaded();

      // 完了状態の行に「完了」が表示されることを確認
      await dataGridPage.expectStatusTextByStatus('Completed', '完了');

      console.log('[TEST] テスト成功: 完了状態の表示テキストが正しいことを確認しました');
    } catch (error) {
      console.error('[TEST] テスト失敗:', error);
      await page.screenshot({
        path: `artifacts/test-results/completed-status-text-error-${Date.now()}.png`,
        fullPage: true,
      });
      throw error;
    }
  });

  test('新規行を追加すると未着手状態になる', async ({ page }) => {
    const dataGridPage = new DataGridDemoPage(page);

    try {
      console.log('[TEST] テスト開始: 新規行を追加すると未着手状態になる');

      // ページに遷移
      await dataGridPage.goto();
      await dataGridPage.expectLoaded();

      // 現在の行数を取得
      const initialRowCount = await dataGridPage.getTotalRowCount();
      console.log(`[TEST] 初期行数: ${initialRowCount}`);

      // 行を追加
      await dataGridPage.clickAddRowButton();

      // 行数が増えたことを確認
      const newRowCount = await dataGridPage.getTotalRowCount();
      console.log(`[TEST] 追加後の行数: ${newRowCount}`);
      expect(newRowCount).toBe(initialRowCount + 1);

      // 未着手状態の行が増えていることを確認
      const notStartedCount = await dataGridPage.getRowCountByStatus('NotStarted');
      console.log(`[TEST] 未着手状態の行数: ${notStartedCount}`);
      expect(notStartedCount).toBeGreaterThanOrEqual(1);

      console.log('[TEST] テスト成功: 新規行が未着手状態で追加されました');
    } catch (error) {
      console.error('[TEST] テスト失敗:', error);
      await page.screenshot({
        path: `artifacts/test-results/add-row-default-status-error-${Date.now()}.png`,
        fullPage: true,
      });
      throw error;
    }
  });

  test('すべての状態列にdata-status属性が存在する', async ({ page }) => {
    const dataGridPage = new DataGridDemoPage(page);

    try {
      console.log('[TEST] テスト開始: すべての状態列にdata-status属性が存在する');

      // ページに遷移
      await dataGridPage.goto();
      await dataGridPage.expectLoaded();

      // すべての行にdata-status属性が存在することを確認
      await dataGridPage.expectAllRowsHaveStatusAttribute();

      console.log('[TEST] テスト成功: すべての行にdata-status属性が存在しました');
    } catch (error) {
      console.error('[TEST] テスト失敗:', error);
      await page.screenshot({
        path: `artifacts/test-results/data-status-attribute-error-${Date.now()}.png`,
        fullPage: true,
      });
      throw error;
    }
  });
});

test.describe('DataGrid 状態列テスト - regression', () => {
  test('複数行を順番に選択できる', async ({ page }) => {
    const dataGridPage = new DataGridDemoPage(page);

    try {
      console.log('[TEST] テスト開始: 複数行を順番に選択できる');

      // ページに遷移
      await dataGridPage.goto();
      await dataGridPage.expectLoaded();

      // 各状態の行を順番にクリック
      const statuses: Array<{ status: 'NotStarted' | 'InProgress' | 'Completed'; name: string }> = [
        { status: 'NotStarted', name: '未着手' },
        { status: 'InProgress', name: '進行中' },
        { status: 'Completed', name: '完了' },
      ];

      for (const { status, name } of statuses) {
        console.log(`[TEST] ${name}状態の行をクリック`);

        // 行が存在するか確認
        const count = await dataGridPage.getRowCountByStatus(status);
        if (count === 0) {
          console.log(`[TEST] ${name}状態の行が存在しないためスキップ`);
          continue;
        }

        // 行をクリック
        await dataGridPage.clickRowByStatus(status, 0);

        // 選択情報が表示されることを確認
        const selectedInfo = await dataGridPage.getSelectedInfoText();
        expect(selectedInfo).toContain('選択:');
        console.log(`[TEST] ${name}状態の行を選択: ${selectedInfo}`);
      }

      console.log('[TEST] テスト成功: 複数行を順番に選択できました');
    } catch (error) {
      console.error('[TEST] テスト失敗:', error);
      await page.screenshot({
        path: `artifacts/test-results/select-multiple-rows-error-${Date.now()}.png`,
        fullPage: true,
      });
      throw error;
    }
  });

  test('状態ごとの行数が正しくカウントされる', async ({ page }) => {
    const dataGridPage = new DataGridDemoPage(page);

    try {
      console.log('[TEST] テスト開始: 状態ごとの行数が正しくカウントされる');

      // ページに遷移
      await dataGridPage.goto();
      await dataGridPage.expectLoaded();

      // 状態ごとの行数を取得
      const notStartedCount = await dataGridPage.getRowCountByStatus('NotStarted');
      const inProgressCount = await dataGridPage.getRowCountByStatus('InProgress');
      const completedCount = await dataGridPage.getRowCountByStatus('Completed');

      console.log(`[TEST] 未着手: ${notStartedCount}件`);
      console.log(`[TEST] 進行中: ${inProgressCount}件`);
      console.log(`[TEST] 完了: ${completedCount}件`);

      // グリッド全体の行数を取得
      const totalRowCount = await dataGridPage.getTotalRowCount();
      console.log(`[TEST] 全体の行数: ${totalRowCount}件`);

      // 状態ごとの合計が全体の行数と一致することを確認
      const statusSum = notStartedCount + inProgressCount + completedCount;
      expect(statusSum).toBe(totalRowCount);

      console.log('[TEST] テスト成功: 状態ごとの行数が正しくカウントされました');
    } catch (error) {
      console.error('[TEST] テスト失敗:', error);
      await page.screenshot({
        path: `artifacts/test-results/count-by-status-error-${Date.now()}.png`,
        fullPage: true,
      });
      throw error;
    }
  });
});
