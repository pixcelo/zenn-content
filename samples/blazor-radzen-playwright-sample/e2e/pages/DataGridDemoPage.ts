import { type Page, type Locator, expect } from '@playwright/test';

/**
 * 商品の状態（Blazor側のItemStatusと対応）
 */
export type ItemStatus = 'NotStarted' | 'InProgress' | 'Completed';

/**
 * DataGridDemoページのPage Objectクラス
 *
 * Blazor Radzen DataGridの操作と検証を提供します。
 * nexta-smartfのProductionInstructionProcessListPageパターンに従った実装。
 */
export class DataGridDemoPage {
  readonly url = '/datagrid-demo';

  // ページの主要要素
  private readonly grid: Locator;
  private readonly addRowButton: Locator;
  private readonly selectedInfoText: Locator;

  constructor(private readonly page: Page) {
    this.grid = page.locator('div.rz-data-grid');
    this.addRowButton = page.locator('button:has-text("行を追加")');
    this.selectedInfoText = page.locator('.rz-alert-info p');
  }

  /**
   * ページに遷移
   * Blazor特有の問題に対応（ERR_ABORTEDを許容）
   */
  async goto(): Promise<void> {
    console.log(`[DataGridDemoPage] ページに遷移: ${this.url}`);
    try {
      await this.page.goto(this.url, {
        waitUntil: 'domcontentloaded',
        timeout: 60_000,
      });
      console.log('[DataGridDemoPage] ページ遷移完了');
    } catch (e) {
      // Blazor特有のERR_ABORTEDエラーは無視
      if (!String(e).includes('net::ERR_ABORTED')) {
        throw e;
      }
      console.log('[DataGridDemoPage] ERR_ABORTEDを検出したが続行');
    }
  }

  /**
   * ページが読み込まれたことを確認
   */
  async expectLoaded(): Promise<void> {
    console.log('[DataGridDemoPage] ページ読み込み確認開始');
    await expect(this.grid).toBeVisible({ timeout: 30_000 });
    console.log('[DataGridDemoPage] DataGridが表示されました');

    // Radzenのレンダリング完了を待つ
    await this.page.waitForLoadState('networkidle', { timeout: 30_000 });
    console.log('[DataGridDemoPage] ネットワークアイドル状態を確認');
  }

  /**
   * data-status属性で行を取得
   * @param status 状態（NotStarted, InProgress, Completed）
   * @returns 行のLocator
   */
  getRowByStatus(status: ItemStatus): Locator {
    console.log(`[DataGridDemoPage] data-status="${status}"の行を検索`);
    return this.page.locator(`tr:has(span[data-status="${status}"])`);
  }

  /**
   * 特定状態の行数を取得
   * @param status 状態（NotStarted, InProgress, Completed）
   * @returns 行数
   */
  async getRowCountByStatus(status: ItemStatus): Promise<number> {
    const count = await this.getRowByStatus(status).count();
    console.log(`[DataGridDemoPage] data-status="${status}"の行数: ${count}`);
    return count;
  }

  /**
   * 特定状態の行をクリック（選択）
   * @param status 状態（NotStarted, InProgress, Completed）
   * @param index 何番目の行をクリックするか（デフォルト: 0=最初の行）
   */
  async clickRowByStatus(status: ItemStatus, index = 0): Promise<void> {
    console.log(`[DataGridDemoPage] data-status="${status}"の${index}番目の行のチェックボックスをクリック`);
    const row = this.getRowByStatus(status).nth(index);

    // 行が表示されていることを確認
    await expect(row).toBeVisible({ timeout: 10_000 });

    // 行をビューポートにスクロール
    await row.scrollIntoViewIfNeeded();

    // 短時間待機（Radzenのイベントハンドラ登録を待つ）
    await this.page.waitForTimeout(500);

    // 行内のチェックボックスを取得してクリック
    const checkbox = row.locator('input[type="checkbox"]');
    await expect(checkbox).toBeVisible({ timeout: 5_000 });
    await checkbox.click();

    // 選択状態の更新を待つ
    await this.page.waitForTimeout(500);

    console.log(`[DataGridDemoPage] チェックボックスクリック完了`);
  }

  /**
   * 選択情報テキストを取得
   * 例: "選択中の商品ID: 3 (サンプル商品3)"
   * @returns 選択情報テキスト
   */
  async getSelectedInfoText(): Promise<string> {
    const text = await this.selectedInfoText.textContent();
    console.log(`[DataGridDemoPage] 選択情報: ${text}`);
    return text || '';
  }

  /**
   * 行追加ボタンをクリック
   */
  async clickAddRowButton(): Promise<void> {
    console.log('[DataGridDemoPage] 行追加ボタンをクリック');

    // 現在の行数を記録
    const beforeCount = await this.getTotalRowCount();

    await this.addRowButton.click();

    // DataGrid再読み込み待機（より長い待機時間）
    await this.page.waitForLoadState('networkidle', { timeout: 10_000 });

    // 行数が増えるまで待機（最大5秒）
    await this.page.waitForFunction(
      (expectedCount) => {
        const rows = document.querySelectorAll('tbody tr');
        return rows.length >= expectedCount;
      },
      beforeCount + 1,
      { timeout: 5_000 }
    );

    console.log('[DataGridDemoPage] 行追加完了');
  }

  /**
   * すべての行にdata-status属性が存在することを確認
   */
  async expectAllRowsHaveStatusAttribute(): Promise<void> {
    console.log('[DataGridDemoPage] すべての行のdata-status属性を確認');

    // DataGridのすべてのデータ行を取得（ヘッダー行を除く）
    const dataRows = this.page.locator('tbody tr');
    const rowCount = await dataRows.count();

    console.log(`[DataGridDemoPage] データ行数: ${rowCount}`);

    // 各行にdata-status属性が存在することを確認
    for (let i = 0; i < rowCount; i++) {
      const statusSpan = dataRows.nth(i).locator('span[data-status]');
      await expect(statusSpan).toBeVisible({ timeout: 5_000 });

      const statusValue = await statusSpan.getAttribute('data-status');
      console.log(`[DataGridDemoPage] 行${i + 1}のdata-status: ${statusValue}`);

      // 有効な状態値であることを確認
      expect(['NotStarted', 'InProgress', 'Completed']).toContain(statusValue);
    }

    console.log('[DataGridDemoPage] すべての行にdata-status属性が存在しました');
  }

  /**
   * 特定状態の行に正しい日本語テキストが表示されているか確認
   * @param status 状態（NotStarted, InProgress, Completed）
   * @param expectedText 期待される日本語テキスト（未着手、進行中、完了）
   */
  async expectStatusTextByStatus(status: ItemStatus, expectedText: string): Promise<void> {
    console.log(`[DataGridDemoPage] data-status="${status}"の表示テキストを確認: ${expectedText}`);

    const row = this.getRowByStatus(status).first();
    const statusSpan = row.locator('span[data-status]');

    await expect(statusSpan).toBeVisible({ timeout: 5_000 });
    await expect(statusSpan).toHaveText(expectedText);

    console.log(`[DataGridDemoPage] data-status="${status}"の表示テキストが正しいことを確認しました`);
  }

  /**
   * グリッド全体の行数を取得
   * @returns 行数
   */
  async getTotalRowCount(): Promise<number> {
    const count = await this.page.locator('tbody tr').count();
    console.log(`[DataGridDemoPage] グリッド全体の行数: ${count}`);
    return count;
  }
}
