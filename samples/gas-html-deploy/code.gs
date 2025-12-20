function doGet(e) {
  return HtmlService.createHtmlOutputFromFile('index')
    .setTitle('DB Schema Documentation')
    .setXFrameOptionsMode(HtmlService.XFrameOptionsMode.ALLOWALL);
}

// キャッシュを使ってファイル一覧を取得
function getFileList() {
  const cache = CacheService.getScriptCache();
  const cacheKey = 'file_list_v1';
  
  // キャッシュから取得を試みる（6時間有効）
  const cached = cache.get(cacheKey);
  if (cached) {
    Logger.log('キャッシュからファイル一覧を取得');
    return JSON.parse(cached);
  }
  
  Logger.log('新規にファイル一覧を取得開始');
  
  // キャッシュがない場合は新規取得
  const folderId = 'YOUR_FOLDER_ID';  // ← ここにGoogle DriveのフォルダIDを入れてください
  const folder = DriveApp.getFolderById(folderId);
  const files = folder.getFiles();
  
  const fileList = [];
  let count = 0;
  const maxFiles = 500; // 最大件数を設定
  
  while (files.hasNext() && count < maxFiles) {
    const file = files.next();
    const fileName = file.getName();
    
    // .mdと.svgファイルのみ
    if (fileName.endsWith('.md') || fileName.endsWith('.svg')) {
      fileList.push({
        id: file.getId(),
        name: fileName
      });
      count++;
    }
  }
  
  Logger.log('取得したファイル数: ' + fileList.length);
  
  fileList.sort((a, b) => a.name.localeCompare(b.name));
  
  // キャッシュに保存（6時間 = 21600秒）
  try {
    cache.put(cacheKey, JSON.stringify(fileList), 21600);
    Logger.log('キャッシュに保存しました');
  } catch (e) {
    Logger.log('キャッシュ保存エラー: ' + e.message);
  }
  
  return fileList;
}

// キャッシュをクリア（ファイルを追加・削除した後に実行）
function clearCache() {
  const cache = CacheService.getScriptCache();
  cache.remove('file_list_v1');
  Logger.log('キャッシュをクリアしました');
  return 'キャッシュをクリアしました';
}

// 個別のファイル内容を取得
function getFileContent(fileId) {
  try {
    const file = DriveApp.getFileById(fileId);
    const mimeType = file.getMimeType();
    
    Logger.log('ファイル取得: ' + file.getName() + ' (MIME: ' + mimeType + ')');
    
    // SVGファイルの場合
    if (mimeType === 'image/svg+xml' || file.getName().endsWith('.svg')) {
      return {
        success: true,
        type: 'svg',
        content: file.getBlob().getDataAsString()
      };
    }
    
    // マークダウンファイルの場合
    return {
      success: true,
      type: 'markdown',
      content: file.getBlob().getDataAsString()
    };
  } catch (error) {
    Logger.log('ファイル取得エラー: ' + error.message);
    return {
      success: false,
      error: error.message
    };
  }
}

// テスト用：直接実行してログを確認
function testGetFileList() {
  const result = getFileList();
  Logger.log('取得結果: ' + result.length + '件');
  if (result.length > 0) {
    Logger.log('最初のファイル: ' + result[0].name);
  }
}