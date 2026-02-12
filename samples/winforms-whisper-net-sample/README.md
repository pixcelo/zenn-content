# Whisper.net 音声認識サンプル

このプロジェクトは、Windows Forms (.NET Framework 4.8) で Whisper.net を使用した音声認識のサンプルアプリケーションです。

## 概要

OpenAI の Whisper モデルを利用して、WAVファイルからテキストへの変換（音声認識）を行います。

## 機能

### 音声ファイル文字起こし
- **音声ファイル選択**: OpenFileDialogでWAV/MP3ファイルを選択
- **モデル自動ダウンロード**: 初回実行時にWhisperモデル(ggml-base.bin)を自動ダウンロード
- **音声認識実行**: Whisper.netを使用して音声→テキスト変換
- **結果表示**: タイムスタンプ付きで認識結果を表示

### 録音機能（3つのモード）
- **マイク録音**: マイクからの音声のみを録音
- **PCオーディオ録音**: システムで再生中の音声（動画、ミーティングなど）を録音
- **ミックス録音**: マイクとPCオーディオの両方を同時に録音

録音後、自動的に16kHz Monoにリサンプリングされ、文字起こし可能な状態になります。

## 必要な環境

### システム要件
- Windows 10/11
- .NET Framework 4.8
- CPU: AVX、AVX2、FMA、F16C命令セットをサポートするCPU
  - Intel Core i5 4世代以降
  - AMD Ryzen 1000シリーズ以降

**注意**: AVX2をサポートしていない古いCPUの場合は、`Whisper.net.Runtime.NoAvx` パッケージを使用してください。

### 開発環境
- Visual Studio 2017以降（推奨: Visual Studio 2022）

## セットアップ

### 1. プロジェクトのビルド

```bash
# Visual Studioでソリューションを開く
WhisperNetSample.sln

# または、MSBuildを使用
msbuild WhisperNetSample.sln /p:Configuration=Release
```

### 2. NuGetパッケージの復元

Visual Studioでソリューションを開くと自動的にNuGetパッケージが復元されます。

手動で復元する場合:
```bash
nuget restore WhisperNetSample.sln
```

## 使い方

### 初回起動

`WhisperNetSample.exe` を実行します。

初回起動時、Whisperモデル（約140MB）が自動的にダウンロードされます。

### 音声ファイルから文字起こし

1. **音声ファイル選択**ボタンをクリック
2. WAV/MP3ファイルを選択
3. **音声認識開始**ボタンをクリック
4. 結果がDataGridViewに表示されます

### 録音から文字起こし

1. **録音モード選択**
   - マイクのみ / PCオーディオのみ / ミックス のいずれかを選択
2. **録音開始**ボタンをクリック
3. 音声を録音（マイクで話す、または動画を再生など）
4. **録音停止**ボタンをクリック
   - 自動的に16kHz Monoにリサンプリングされます
5. **音声認識開始**ボタンをクリック
6. 結果がDataGridViewに表示されます

### 結果の表示形式

認識結果は以下の形式でDataGridViewに表示されます:

| 開始時刻 | 終了時刻 | テキスト |
|---------|---------|---------|
| 00:00.00 | 00:03.50 | これはサンプル音声です。 |
| 00:03.50 | 00:07.20 | Whisper.netを使用しています。 |

### エクスポート機能

文字起こし結果を複数の形式でエクスポートできます:

1. **📁 エクスポートボタンをクリック**
2. **保存形式を選択**（複数選択可能）:
   - JSON (.json) - 構造化データ、プログラム処理向け
   - テキスト (.txt) - 読みやすい形式、共有向け
   - CSV (.csv) - Excel、データ分析向け
   - SRT (.srt) - 動画字幕形式
3. **音声ファイルも一緒に保存** (オプション)
4. **保存先フォルダを選択**
5. 自動的にフォルダが開きます

**デフォルト保存先**: `ドキュメント\WhisperNetSample\Transcriptions\`

**詳細**: [エクスポート形式仕様書](WhisperNetSample/docs/export-formats.md)

## 対応形式

- **音声ファイル**: WAV、MP3形式（16kHz推奨）
- **録音形式**:
  - マイク: 16kHz Mono（直接録音）
  - PCオーディオ: 48kHz Stereo（ネイティブ）→ 停止時に16kHz Monoへ自動変換
  - ミックス: 48kHz Stereo（ネイティブ）→ 停止時に16kHz Monoへ自動変換
- **言語**: 日本語（コード内で `WithLanguage("ja")` を指定）

他の形式を試す場合は、FFmpegなどを使用してWAV/MP3形式に変換してください。

## トラブルシューティング

### モデルのダウンロードに失敗する

インターネット接続を確認してください。また、Hugging Faceへのアクセスが制限されている場合は、手動でモデルをダウンロードして実行ファイルと同じディレクトリに配置してください。

モデルURL: https://huggingface.co/ggerganov/whisper.cpp

### 「AVXがサポートされていません」エラー

古いCPUの場合、`packages.config` を以下のように変更してください:

```xml
<package id="Whisper.net.Runtime.NoAvx" version="1.9.0" targetFramework="net48" />
```

### 音声が認識されない

- WAVファイルの形式を確認してください（16bit PCM推奨）
- サンプリングレート16kHzで録音されているか確認してください
- 音声が静か過ぎないか確認してください

## 使用ライブラリ

- [Whisper.net](https://github.com/sandrohanea/whisper.net) 1.9.0 - .NET用Whisperライブラリ
- Whisper.net.Runtime 1.9.0 - CPUランタイム（AVX2対応）
- [NAudio](https://github.com/naudio/NAudio) 2.2.1 - .NET用オーディオライブラリ
  - WaveInEvent: マイク入力
  - WasapiLoopbackCapture: システムオーディオキャプチャ
  - MediaFoundationResampler: オーディオリサンプリング

## ライセンス

このサンプルプロジェクトはパブリックドメインです。自由に使用・改変できます。

## 参考リンク

- [Whisper.net GitHub](https://github.com/sandrohanea/whisper.net)
- [OpenAI Whisper](https://github.com/openai/whisper)
- [Whisper.cpp](https://github.com/ggerganov/whisper.cpp)
