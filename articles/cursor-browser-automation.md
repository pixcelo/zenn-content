---
title: "CursorのAIエージェントにブラウザ操作させる方法"
emoji: "🤖"
type: "tech" # tech: 技術記事 / idea: アイデア
topics: ["cursor", "ai", "agent"]
published: true
publication_name: "nexta_" # 企業のPublication名を指定
---

## はじめに

Cursor Agentに新たに追加されたBrowser機能を使うと、AIエージェントが自動でWebブラウザを操作し、情報収集やWebサイトの動作確認を行ってくれます。本記事では、この機能の使い方を実践的に解説します。

## 全体フロー

1. **Cursor Beta** → Early Access設定に変更
2. **Agent Window起動** → Ctrl+E (または Cmd+E)
3. **Browserボタンクリック** → ブラウザウィンドウ起動
4. **チャットで指示** → AIがブラウジング実行
5. **自動保存** → `.cursor/screenshots` にスクリーンショット保存

## 前提条件

Cursor Agentのブラウザ機能を利用するには、以下の設定が必要です（2025/10/04 時点）：

- Update Accessを**Early Access**に設定

## 使い方

### 1. Early Accessへの変更

Cursor Beta の設定から、Update Access を **Early Access** に変更します。

![Early Access設定画面](/images/cursor-browser-automation/early-access-settings.png)

### 2. Agent Windowを起動

**Ctrl+E** (Windows) または **Cmd+E** (Mac) でAgent Windowを開きます。

![Agent Window](/images/cursor-browser-automation/agent-window.png)

### 3. Browserを起動

Agent Window内の **+ Browser** ボタンをクリックすると、ブラウザウィンドウが開きます。

![Browserボタン](/images/cursor-browser-automation/browser-button.png)

![ブラウザウィンドウ](/images/cursor-browser-automation/browser-window.png)

### 4. 指示を出す

チャット欄で指示を出すと、AIエージェントが自動でブラウジングを実行します。実行中はスクリーンショットを取りながら操作を進めます。

### 5. スクリーンショットの確認

実行結果のスクリーンショットは `.cursor/screenshots` ディレクトリに自動保存されます。

## 実行例：最新AI情報の収集

実際に「最新のAI情報をブラウザで表示して」という指示を出した結果を紹介します。

### AIの実行結果

![AI実行結果](/images/cursor-browser-automation/ai-browsing-result.png)

AIエージェントが以下のサイトを自動で訪問し、スクリーンショットを保存しました：

1. OpenAI公式ブログ - OpenAIの最新発表や研究成果
2. Hacker News - 技術コミュニティでの最新議論
3. VentureBeat AI - AI業界の最新ニュース
4. AI now - 日本のAI技術情報
5. 日経Smart AI - 日本経済新聞のAI特集

## 活用シーン

### 開発・テスト関連

- バグ再現手順の記録: 「このバグを再現して」と指示すれば、操作手順とスクリーンショットを自動記録
- 実装中の動作確認: コード変更後にすぐ「トップページを表示して」で即座に確認
- GitHubのPRレビュー補助: 「このPRで変更されたページを確認して」と指示して関連ページを巡回
- フォーム入力テスト: お問い合わせフォームや会員登録フォームの動作を自然言語で指示してテスト
- エラーページの検証: 404ページやエラー画面が正しく表示されるか確認

### その他の活用場面

- 情報収集の自動化: 複数のニュースサイトやブログを巡回して最新情報を収集
- 競合調査・リサーチ: 競合サイトの機能やUIを調査
- 定期的なモニタリング: 特定のページの状態を定期的にチェック
- 技術記事の収集: 「Reactの最新記事を5つ見つけて」と指示して自動でスクリーンショット保存

## E2Eテスト自動化ツールとの使い分け

一般的なE2Eテスト自動化ツール（Playwright、Selenium、Cypress等）との違いと使い分けについて整理します。

### E2Eテスト自動化ツール（従来型）

- 事前準備: テストシナリオを定義・メンテナンス
- 実行環境: CI/CDパイプラインに組み込んで自動実行
- 検証: 厳密な検証ルールで品質を保証
- 用途: 定型的な回帰テストに最適
- 運用: チーム運用・本番環境での信頼性重視

### Cursor Agent Browser（AI駆動）

- 事前準備: 不要、自然言語で即座に指示
- 実行環境: 開発中にその場で実行
- 検証: 目視確認が中心（スクリーンショット保存）
- 用途: アドホックな動作確認・情報収集
- 運用: 個人開発や探索的テストに向いている

### 使い分けの例

開発フェーズでの使い分け：
- 実装中の即時確認 → Cursor Agent Browser
- リリース前の品質保証 → E2Eテスト自動化ツール

タスク別の使い分け：
- 「このページ、ちゃんと表示される？」 → Cursor
- 「全画面の回帰テストを毎晩実行」 → E2Eテスト自動化ツール
- 「競合サイトの新機能を確認したい」 → Cursor
- 「決済フローが壊れていないか検証」 → E2Eテスト自動化ツール

## 公式リソース

- 機能発表（X/Twitter）: https://x.com/cursor_ai/status/1972778817854067188
- 公式ドキュメント: https://cursor.com/ja/docs/agent/browser

## まとめ

Cursor Agent Browserを使えば、AIエージェントに指示を出すだけで自動的にWebブラウザを操作してくれます。

現在はBeta版で実運用にはまだ課題がありますが、手動で行うと時間もコストもかかる**複数サイトの巡回や、実装後の動作確認をAIに任せられる**可能性を見せてくれています。正式版として安定化すれば、情報収集からスクリーンショット保存まで全自動で、開発者の負担を大きく軽減してくれる強力な機能になるでしょう。