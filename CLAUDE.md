# Claude Code - Zenn Content Project
回答は必ず日本語で行ってください。

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