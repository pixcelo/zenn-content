---
title: "tblsのDBスキーマをGASで社内公開 - GitHub不要&無料"
emoji: "📊"
type: "tech"
topics: ["tbls", "gas", "googleappsscript", "markdown", "スキーマ管理"]
published: false
publication_name: "nexta_" # 企業のPublication名を指定
---

## はじめに

[tbls](https://github.com/k1LoW/tbls)でDBスキーマドキュメントを生成している方、こんな課題を抱えていませんか？

- 非エンジニアはGitHubアカウントを持っていないため、スキーマドキュメントを閲覧できない
- 論理名や物理名をエンジニア以外が自分で確認できるようにしたい
- 社内限定で公開したいが、セキュアな方法がわからない
- コストを抑えつつ、誰でも見やすいドキュメントを提供したい

Google Apps Script(GAS)を使えば、組織内限定で無料公開できます。

## 完成イメージ

![完成したウェブアプリの画面](/images/gas-html-deploy/app-screenshot.png)

- サイドバーでテーブル一覧を検索・表示
- Markdownをリッチに表示（テーブル定義・ER図）
- SVG画像も正しく表示
- Google Workspace内のユーザーのみアクセス可能

## 前提条件

- tbls導入済み（Markdown + SVG形式で出力）
- Google Workspace環境
- Google Driveにスキーマファイルをアップロード済み

## 実装手順

### 1. tblsでスキーマ出力

通常通りtblsでMarkdown形式のドキュメントを生成します。

生成されたファイル（`*.md`と`*.svg`）をGoogle Driveの任意のフォルダにアップロードします。

![Google Driveにスキーマファイルをアップロード](/images/gas-html-deploy/drive-folder.png)

### 2. GASプロジェクト作成

1. [Google Apps Script](https://script.google.com/)にアクセス
2. 「新しいプロジェクト」を作成

![Google Apps Scriptで新しいプロジェクトを作成](/images/gas-html-deploy/gas-new-project.png)

3. `code.gs`と`index.html`の2ファイルを作成

![GASエディタでファイルを作成](/images/gas-html-deploy/gas-editor.png)

### 3. ソースコード実装

※100%、AIが生成したコードです。利用の際は、自己責任でお願いします。

#### code.gs

Google Driveからファイル一覧と内容を取得するバックエンド処理です。

:::details code.gs（クリックで展開）
```javascript
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
  // 例：URLのfolders/より先がID　 https://drive.google.com/drive/u/0/folders/1yrbyDuWgOAyGgd9D0i4mkNfzyD0_3bsW
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

  // キャッシュに保存（最大6時間 = 21600秒、ただし保証されない）
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
```
:::

`YOUR_FOLDER_ID`をGoogle DriveのフォルダIDに置き換えます

#### index.html

Markdownレンダリングとファイル表示を行うフロントエンド実装です。

:::details index.html（クリックで展開）
```html
<!DOCTYPE html>
<html>
<head>
  <base target="_top">
  <meta charset="utf-8">
  <title>DB Schema Documentation</title>
  <style>
    body {
      font-family: 'Segoe UI', Arial, sans-serif;
      margin: 0;
      padding: 0;
      background-color: #f5f5f5;
    }
    .sidebar {
      position: fixed;
      left: 0;
      top: 0;
      width: 280px;
      height: 100vh;
      background: #2c3e50;
      color: white;
      overflow-y: auto;
      box-shadow: 2px 0 5px rgba(0,0,0,0.1);
    }
    .sidebar-header {
      padding: 20px;
      background: #1a252f;
      position: sticky;
      top: 0;
      z-index: 10;
    }
    .sidebar-header h2 {
      margin: 0 0 10px 0;
      font-size: 18px;
    }
    #search {
      width: 100%;
      padding: 8px;
      border: none;
      border-radius: 4px;
      box-sizing: border-box;
    }
    .table-list {
      padding: 10px;
    }
    .table-link {
      padding: 10px 15px;
      margin: 5px 0;
      cursor: pointer;
      border-radius: 4px;
      transition: background 0.2s;
      font-size: 14px;
      word-break: break-word;
    }
    .table-link:hover {
      background: #34495e;
    }
    .table-link.active {
      background: #3498db;
    }
    .content {
      margin-left: 280px;
      padding: 30px;
    }
    .container {
      background: white;
      padding: 30px;
      border-radius: 8px;
      box-shadow: 0 2px 4px rgba(0,0,0,0.1);
      min-height: calc(100vh - 100px);
    }
    .loading {
      text-align: center;
      padding: 50px;
      color: #7f8c8d;
    }
    .spinner {
      border: 4px solid #f3f3f3;
      border-top: 4px solid #3498db;
      border-radius: 50%;
      width: 40px;
      height: 40px;
      animation: spin 1s linear infinite;
      margin: 20px auto;
    }
    @keyframes spin {
      0% { transform: rotate(0deg); }
      100% { transform: rotate(360deg); }
    }
    table {
      width: 100%;
      border-collapse: collapse;
      margin: 20px 0;
    }
    th, td {
      border: 1px solid #ddd;
      padding: 12px;
      text-align: left;
    }
    th {
      background-color: #3498db;
      color: white;
    }
    code {
      background: #f4f4f4;
      padding: 2px 6px;
      border-radius: 3px;
      font-family: 'Consolas', monospace;
    }
    pre {
      background: #f4f4f4;
      padding: 15px;
      border-radius: 5px;
      overflow-x: auto;
    }
    #content a {
      color: #3498db;
      text-decoration: none;
      cursor: pointer;
    }
    #content a:hover {
      text-decoration: underline;
    }
    #content img {
      max-width: 100%;
      height: auto;
      border: 1px solid #ddd;
      border-radius: 4px;
      padding: 10px;
      background: white;
    }
  </style>
  <script src="https://cdn.jsdelivr.net/npm/marked/marked.min.js"></script>
</head>
<body>
  <div class="sidebar">
    <div class="sidebar-header">
      <h2>テーブル一覧</h2>
      <input type="text" id="search" placeholder="テーブル名で検索...">
    </div>
    <div class="table-list" id="table-list">
      <div class="loading">
        <div class="spinner"></div>
        ファイル一覧を読み込み中...<br>
        <small id="load-status">初期化中...</small>
      </div>
    </div>
  </div>

  <div class="content">
    <div class="container">
      <div id="content">
        <div class="loading">
          <p>左側のリストからテーブルを選択してください</p>
        </div>
      </div>
    </div>
  </div>

  <script>
    let allFiles = [];
    let fileIdMap = {};

    function loadFileList() {
      google.script.run
        .withSuccessHandler(function(files) {
          if (!files || files.length === 0) {
            document.getElementById('table-list').innerHTML =
              '<div style="padding: 20px;">ファイルが見つかりません</div>';
            return;
          }

          allFiles = files;
          files.forEach(function(file) {
            fileIdMap[file.name] = file.id;
          });

          const mdFiles = files.filter(f => f.name.endsWith('.md'));
          displayFileList(mdFiles);
        })
        .withFailureHandler(function(error) {
          document.getElementById('table-list').innerHTML =
            '<div style="padding: 20px; color: #e74c3c;">エラー: ' + error.message + '</div>';
        })
        .getFileList();
    }

    function displayFileList(files) {
      const listDiv = document.getElementById('table-list');
      listDiv.innerHTML = '';

      files.forEach(function(file) {
        const div = document.createElement('div');
        div.className = 'table-link';
        div.textContent = file.name.replace('.md', '');
        div.onclick = function() {
          loadMarkdownFile(file.id, file.name, div);
        };
        listDiv.appendChild(div);
      });
    }

    function loadMarkdownFile(fileId, fileName, element) {
      document.querySelectorAll('.table-link').forEach(el => el.classList.remove('active'));
      if (element) element.classList.add('active');

      document.getElementById('content').innerHTML =
        '<div class="loading"><div class="spinner"></div>読み込み中...</div>';

      google.script.run
        .withSuccessHandler(function(result) {
          if (result.success && result.type === 'markdown') {
            displayMarkdown(result.content);
          }
        })
        .getFileContent(fileId);
    }

    function displayMarkdown(markdown) {
      let html = marked.parse(markdown);
      document.getElementById('content').innerHTML = html;
      processLinks();
      processSvgImages();
    }

    function processLinks() {
      const links = document.getElementById('content').querySelectorAll('a');
      links.forEach(function(link) {
        const href = link.getAttribute('href');
        if (href && href.endsWith('.md')) {
          link.onclick = function(e) {
            e.preventDefault();
            if (fileIdMap[href]) {
              loadMarkdownFile(fileIdMap[href], href, null);
            }
            return false;
          };
        }
      });
    }

    function processSvgImages() {
      const images = document.getElementById('content').querySelectorAll('img');
      images.forEach(function(img) {
        const src = img.getAttribute('src');
        if (src && src.endsWith('.svg') && fileIdMap[src]) {
          loadAndDisplaySvg(img, fileIdMap[src]);
        }
      });
    }

    function loadAndDisplaySvg(imgElement, fileId) {
      google.script.run
        .withSuccessHandler(function(result) {
          if (result.success && result.type === 'svg') {
            const base64 = btoa(unescape(encodeURIComponent(result.content)));
            imgElement.src = 'data:image/svg+xml;base64,' + base64;
          }
        })
        .getFileContent(fileId);
    }

    document.getElementById('search').addEventListener('input', function(e) {
      const keyword = e.target.value.toLowerCase();
      const filtered = allFiles
        .filter(f => f.name.endsWith('.md'))
        .filter(file => file.name.toLowerCase().includes(keyword));
      displayFileList(filtered);
    });

    window.onload = function() {
      loadFileList();
    };
  </script>
</body>
</html>
```
:::

- [marked.js](https://marked.js.org/)でMarkdownをHTMLに変換
- SVGをBase64エンコードしてData URIで表示
- Markdown内の相対リンク（`.md`）を動的に処理

### 4. デプロイ&公開設定

1. 右上「デプロイ」→「新しいデプロイ」
2. 種類：「ウェブアプリ」
3. 次のユーザーとして実行：「自分」
4. アクセス権限：「組織内の全員」
5. デプロイ完了後、URLをコピー

これで社内のGoogle Workspaceユーザーのみがアクセスできる状態になります。

## 運用

### スキーマ更新フロー

1. tblsで最新スキーマを生成
2. Google Driveのフォルダを更新
3. GASのスクリプトエディタで`clearCache()`を実行
4. ブラウザをリロード

### アクセス権限管理

- 特定部署のみに公開したい場合：デプロイ設定で「組織内の特定のユーザー」を選択
- 外部共有したくない場合：Google Workspace管理コンソールで外部共有を無効化

## まとめ

tblsとGASの組み合わせで、以下を実現しました：

✅ GitHubアカウント不要でスキーマ共有
✅ Google Workspace組織内の限定公開
✅ 完全無料（Google Driveの容量内）
✅ マークダウン・SVG図解のリッチ表示

ビジネスサイドとの情報共有が格段に改善されます。ぜひお試しください！