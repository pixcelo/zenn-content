# Claude Code - Zenn Content Project

## プロジェクト概要
Zennで公開する記事と本を管理するリポジトリです。

## ディレクトリ構造
```
.
├── contents/         # Zenn CLIの作業ディレクトリ
│   ├── articles/     # 記事
│   ├── books/        # 本
│   └── package.json  # zenn-cli依存関係
├── CLAUDE.md         # このファイル
├── README.md
└── .gitignore
```

## Zenn CLI コマンド

### 記事作成
```bash
cd contents && npx zenn new:article --slug article-name --title "記事タイトル" --type tech --emoji 🤖
```

### 本作成
```bash
cd contents && npx zenn new:book --slug book-name-12chars-min --title "本のタイトル"
```
**注意**: ブックのスラッグは12-50文字の英数字・ハイフン・アンダースコアのみ

### プレビュー
```bash
cd contents && npx zenn preview --port 8000
```
プレビューURL: http://localhost:8000

### 一覧表示
```bash
cd contents && npx zenn list:articles
cd contents && npx zenn list:books
```

## ワークフロー

### 新規記事作成から公開まで
1. 記事作成: `cd contents && npx zenn new:article --slug my-article --title "記事タイトル" --type tech --emoji 📝`
2. 記事編集: `contents/articles/my-article.md` を編集
3. プレビュー: `cd contents && npx zenn preview`
4. コミット & プッシュで自動公開

### 本の作成から公開まで
1. 本作成: `cd contents && npx zenn new:book --slug my-awesome-book --title "本のタイトル"`
2. 設定編集: `contents/books/my-awesome-book/config.yaml`
3. チャプター編集: `contents/books/my-awesome-book/` 内の `.md` ファイル
4. プレビュー: `cd contents && npx zenn preview`
5. コミット & プッシュで自動公開

## Git ワークフロー
- メインブランチ: `main`
- コミット時は適切な粒度で分割
- Zennとの同期は GitHub リポジトリ経由で自動