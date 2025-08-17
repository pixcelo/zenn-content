# Claude Code - Zenn Content Project
回答は必ず日本語で行ってください。

## プロジェクト概要
Zennで公開する記事と本を管理するリポジトリです。

## ディレクトリ構造
```
.
├── articles/         # 記事（Zenn連携対象）
├── books/            # 本（Zenn連携対象）
├── images/           # 画像ファイル（Zenn連携対象）
├── samples/          # サンプルコード
├── node_modules/     # Zenn CLI依存関係
├── package.json      # Zenn CLI設定
├── package-lock.json # 依存関係ロック
├── CLAUDE.md         # このファイル
├── README.md
└── .gitignore
```

## Zenn CLI コマンド

### 記事作成
```bash
npx zenn new:article --slug article-name --title "記事タイトル" --type tech --emoji 🤖
```

### 本作成
```bash
npx zenn new:book --slug book-name-12chars-min --title "本のタイトル"
```
**注意**: ブックのスラッグは12-50文字の英数字・ハイフン・アンダースコアのみ

### プレビュー
```bash
npx zenn preview --port 8000
```
プレビューURL: http://localhost:8000

### 一覧表示
```bash
npx zenn list:articles
npx zenn list:books
```

## ワークフロー

### 新規記事作成から公開まで
1. 記事作成: `npx zenn new:article --slug my-article --title "記事タイトル" --type tech --emoji 📝`
2. 記事編集: `articles/my-article.md` を編集
3. プレビュー: `npx zenn preview`
4. コミット & プッシュで自動公開

### 本の作成から公開まで
1. 本作成: `npx zenn new:book --slug my-awesome-book --title "本のタイトル"`
2. 設定編集: `books/my-awesome-book/config.yaml`
3. チャプター編集: `books/my-awesome-book/` 内の `.md` ファイル
4. プレビュー: `npx zenn preview`
5. コミット & プッシュで自動公開

## Git ワークフロー
- メインブランチ: `main`
- コミット時は適切な粒度で分割
- Zennとの同期は GitHub リポジトリ経由で自動

## Zenn記事作成のベストプラクティス

### 図解の追加方法
- **Zenn対応形式**: Mermaid記法のみサポート（PlantUMLは非対応）
- **図解生成**: `mcp__diagram__generate_diagram`を使用してMermaid図を作成
- **対応図種別**: 
  - フローチャート (`flowchart TD`)
  - シーケンス図 (`sequenceDiagram`) 
  - クラス図 (`classDiagram`)
  - アーキテクチャ図 (`graph TB`)

### Mermaid構文注意点
- subgraphには識別子が必要: `subgraph Server["表示名"]`
- ノード名は引用符で囲む: `A["テキスト"]`
- エッジラベルも引用符で囲む: `|"ラベル"`
- `note for` はクラス図では使用不可

### 図解配置の指針
- **アーキテクチャ図**: セクション冒頭で全体像を提示
- **クラス図**: 実装詳細説明の直前
- **シーケンス図**: 動作説明時
- **フローチャート**: プロセス説明時

## 画像の追加方法

### 画像ファイルの配置
- **配置場所**: `/images` ディレクトリ（リポジトリルート）
- **ディレクトリ構造**: サブディレクトリでの整理も可能
- **ファイル制限**: 
  - 最大ファイルサイズ: 3MB
  - 対応形式: `.png`, `.jpg`, `.jpeg`, `.gif`, `.webp`

### 記事での画像参照
```markdown
![説明文](/images/example-image.png)
![図1: システム構成](/images/diagrams/system-overview.png)
```

**重要**: 必ず `/images/` から始まる絶対パスを使用する

### 画像管理のベストプラクティス
- **ファイル名**: わかりやすい名前を使用（例: `alloy-class-diagram.png`）
- **整理**: 記事別やカテゴリ別でサブディレクトリを活用
- **プレビュー**: `npx zenn preview` で画像表示を確認
- **同期**: GitHubにプッシュ後、Zennへの反映は最大1分

### 画像追加のワークフロー
1. 画像ファイルを `/images` ディレクトリに配置
2. 記事で絶対パス参照 `![](/images/filename.png)`
3. プレビューで確認: `npx zenn preview`
4. コミット & プッシュで自動同期