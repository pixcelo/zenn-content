# 文字起こし結果のエクスポート形式

このドキュメントでは、Whisper.netサンプルアプリケーションがサポートする文字起こし結果のエクスポート形式について説明します。

## 対応形式一覧

| 形式 | 拡張子 | 用途 | メタデータ |
|------|--------|------|-----------|
| JSON | .json | プログラム処理、API連携 | ✓ |
| テキスト | .txt | 読みやすい・共有しやすい | ✓ |
| CSV | .csv | Excel、データ分析 | ✗ |
| SRT字幕 | .srt | 動画プレーヤー、字幕編集 | ✗ |

## 各形式の詳細

### 1. JSON形式 (.json)

**特徴**: メタデータ付きの構造化データ

**用途**:
- プログラムでの処理
- 他ツールとのAPI連携
- バックアップ・アーカイブ

**形式例**:
```json
{
  "metadata": {
    "created_at": "2026-02-12T15:30:00+09:00",
    "model": "Base",
    "language": "ja",
    "audio_file": "recording_xxx.wav",
    "total_duration": "00:07.20",
    "segment_count": 2
  },
  "segments": [
    {
      "start": "00:00.00",
      "end": "00:03.50",
      "duration": 3.5,
      "text": "これはサンプル音声です。"
    },
    {
      "start": "00:03.50",
      "end": "00:07.20",
      "duration": 3.7,
      "text": "Whisper.netを使用しています。"
    }
  ]
}
```

**メタデータフィールド**:
- `created_at`: 文字起こし実行日時（ISO 8601形式）
- `model`: 使用したWhisperモデル名
- `language`: 言語コード（例: "ja", "en"）
- `audio_file`: 元の音声ファイル名
- `total_duration`: 音声の総再生時間
- `segment_count`: セグメント数

---

### 2. テキスト形式 (.txt)

**特徴**: 人間が読みやすいシンプルなテキスト

**用途**:
- メモ帳やテキストエディタで閲覧・編集
- メール・チャットでの共有
- 議事録作成の下書き

**形式例**:
```
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
文字起こし日時: 2026/02/12 15:30:00
モデル: Base
言語: ja
総時間: 00:07.20
セグメント数: 2
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

00:00.00 - 00:03.50
これはサンプル音声です。

00:03.50 - 00:07.20
Whisper.netを使用しています。
```

---

### 3. CSV形式 (.csv)

**特徴**: 表形式データ（Excel、スプレッドシート対応）

**用途**:
- Excelで開いて編集・分析
- データベースへのインポート
- タイムスタンプ付きデータの集計

**形式例**:
```csv
開始時刻,終了時刻,テキスト,長さ(秒)
00:00.00,00:03.50,"これはサンプル音声です。",3.50
00:03.50,00:07.20,"Whisper.netを使用しています。",3.70
```

**CSVエスケープルール**:
- カンマ、ダブルクォート、改行を含むテキストは `"` で囲む
- テキスト内の `"` は `""` にエスケープ

**Excelでの開き方**:
1. ファイルをダブルクリック（UTF-8対応のExcelなら正しく開ける）
2. 文字化けする場合: Excel → データ → テキストから → UTF-8を選択

---

### 4. SRT字幕形式 (.srt)

**特徴**: 動画プレーヤー・編集ソフト対応の標準字幕形式

**用途**:
- VLC、MPC-HCなどの動画プレーヤー
- YouTube、ニコニコ動画の字幕アップロード
- Premiere Pro、DaVinci Resolveでの字幕編集

**形式例**:
```srt
1
00:00:00,000 --> 00:00:03,500
これはサンプル音声です。

2
00:00:03,500 --> 00:00:07,200
Whisper.netを使用しています。
```

**タイムスタンプ形式**: `HH:MM:SS,mmm` （時:分:秒,ミリ秒）

**動画プレーヤーでの使い方**:
1. VLCなどで動画を開く
2. 字幕 → 字幕トラックを追加 → .srtファイルを選択
3. 自動的に字幕が表示される

---

## エクスポート手順

### 1. 文字起こし実行

1. 音声ファイルを選択 または 録音
2. 「文字起こし実行」ボタンをクリック
3. 結果がDataGridViewに表示される

### 2. エクスポート

1. 「📁 エクスポート」ボタンをクリック
2. 保存形式を選択（複数選択可）
3. 「音声ファイルも一緒に保存」にチェック（任意）
4. 保存先フォルダを選択
5. 「保存」ボタンをクリック

### 3. 保存ファイル

**ファイル名規則**: `YYYYMMDD_HHMMSS_transcript.{拡張子}`

**例**:
```
20260212_153000_transcript.json
20260212_153000_transcript.txt
20260212_153000_transcript.csv
20260212_153000_transcript.srt
20260212_153000_recording.wav  （音声ファイル）
```

---

## デフォルト保存先

**Windowsの場合**:
```
C:\Users\{ユーザー名}\Documents\WhisperNetSample\Transcriptions\
```

**保存先の変更**:
- エクスポート時のフォルダ選択ダイアログで任意のフォルダを指定可能
- 次回以降も前回選択したフォルダが記憶される

---

## トラブルシューティング

### Q. CSVをExcelで開くと文字化けする

**原因**: ExcelがUTF-8を正しく認識していない

**対策**:
1. Excelを開く
2. データタブ → テキストから
3. ファイルを選択 → 元のファイル形式で「65001: Unicode (UTF-8)」を選択
4. 区切り文字で「カンマ」にチェック

### Q. SRT字幕が動画プレーヤーで表示されない

**原因**: タイムスタンプが動画の長さを超えている

**対策**:
- 動画の長さと音声ファイルの長さが一致しているか確認
- 必要に応じてテキストエディタでSRTファイルのタイムスタンプを調整

### Q. JSON形式で日本語が読めない

**原因**: テキストエディタがUTF-8に対応していない

**対策**:
- VSCode、Notepad++、サクラエディタなどUTF-8対応エディタを使用
- Windowsのメモ帳でも読めるが、改行が正しく表示されない場合あり

---

## 開発者向け情報

### カスタムフォーマットの追加

新しいエクスポート形式を追加する場合:

1. `TranscriptionExporter.cs` に新しいメソッドを追加
   ```csharp
   public static void SaveAsCustomFormat(DataTable dt, string filePath)
   {
       // カスタム形式の実装
   }
   ```

2. `Form1.cs` の `btnExport_Click` メソッドに処理を追加

### エクスポートクラスAPI

```csharp
// JSON形式で保存（メタデータ付き）
TranscriptionExporter.SaveAsJson(dataTable, filePath, metadata);

// テキスト形式で保存（メタデータ付き）
TranscriptionExporter.SaveAsText(dataTable, filePath, metadata);

// CSV形式で保存
TranscriptionExporter.SaveAsCsv(dataTable, filePath);

// SRT字幕形式で保存
TranscriptionExporter.SaveAsSrt(dataTable, filePath);

// 音声ファイルをコピー
TranscriptionExporter.CopyAudioFile(sourceFilePath, destinationFolder);
```

---

## 参考リンク

- [JSON形式仕様](https://www.json.org/json-ja.html)
- [CSV形式仕様（RFC 4180）](https://datatracker.ietf.org/doc/html/rfc4180)
- [SRT字幕形式仕様](https://en.wikipedia.org/wiki/SubRip)
- [Whisper.net GitHub](https://github.com/sandrohanea/whisper.net)
