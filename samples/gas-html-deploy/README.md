# GAS HTML Deploy Sample

tblsで生成したDBスキーマドキュメントをGoogle Apps Scriptでウェブアプリとして公開するサンプルです。

## ファイル構成

```
gas-html-deploy/
├── code.gs              # GASバックエンド（ファイル取得・キャッシュ管理）
├── index.html           # フロントエンド（Markdownレンダリング・UI）
├── sample-schema/       # サンプルDBスキーマ（架空のECサイト）
│   ├── README.md        # テーブル一覧
│   ├── users.md         # ユーザーマスタ
│   ├── products.md      # 商品マスタ
│   ├── orders.md        # 注文テーブル
│   ├── order_details.md # 注文明細テーブル
│   ├── categories.md    # カテゴリマスタ
│   ├── schema.svg       # 全体ER図
│   └── users.svg        # ユーザーテーブルER図
└── README.md            # このファイル
```

## セットアップ手順

### 1. Google Driveにスキーマファイルをアップロード

`sample-schema/` 内のファイルをGoogle Driveの任意のフォルダにアップロードします。

### 2. Google Apps Scriptプロジェクト作成

1. https://script.google.com/ にアクセス
2. 「新しいプロジェクト」を作成
3. `code.gs` の内容をコピー&ペースト
4. HTMLファイルを追加（ファイル > 新規作成 > HTMLファイル）、名前を `index` にする
5. `index.html` の内容をコピー&ペースト

### 3. フォルダIDの設定

`code.gs` の22行目を編集：

```javascript
const folderId = 'YOUR_FOLDER_ID';  // ← Google DriveのフォルダIDに置き換え
```

フォルダIDの取得方法：
- Google DriveでフォルダURLを開く
- URLの `folders/` 以降の文字列がフォルダID
- 例: `https://drive.google.com/drive/folders/1a2b3c4d5e6f7g8h9` → `1a2b3c4d5e6f7g8h9`

### 4. デプロイ

1. 右上「デプロイ」→「新しいデプロイ」
2. 種類：「ウェブアプリ」を選択
3. 次のユーザーとして実行：「自分」
4. アクセス権限：「組織内の全員」（Google Workspace）または「全員」（個人用）
5. 「デプロイ」をクリック
6. 生成されたURLにアクセス

## 主な機能

- **サイドバー検索**: テーブル名でインクリメンタル検索
- **Markdownレンダリング**: marked.jsでリッチ表示
- **SVG画像表示**: Base64エンコードでSVG図を表示
- **相対リンク処理**: Markdown内の `.md` リンクを動的に処理
- **キャッシュ機能**: 6時間のファイル一覧キャッシュでパフォーマンス向上

## 運用

### キャッシュクリア

スキーマファイルを更新した後は、GASエディタで以下を実行：

1. `clearCache` 関数を選択
2. 「実行」ボタンをクリック
3. ブラウザをリロード

### トラブルシューティング

**ファイルが表示されない**
- フォルダIDが正しいか確認
- Google Driveの共有設定を確認（自分がアクセス権を持っているか）

**画像が表示されない**
- SVGファイルがフォルダに存在するか確認
- ファイル名が正しくリンクされているか確認

**タイムアウトが発生する**
- ファイル数が多すぎる場合は `maxFiles` を調整（デフォルト500）

## 関連記事

詳細な解説は以下の記事をご覧ください：

https://zenn.dev/your-username/articles/tbls-gas-deploy

## ライセンス

このサンプルコードはMITライセンスで公開されています。
