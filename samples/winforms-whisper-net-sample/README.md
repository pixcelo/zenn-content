# Whisper.net 音声認識サンプル

このプロジェクトは、Windows Forms (.NET Framework 4.8) で Whisper.net を使用した音声認識のサンプルアプリケーションです。

## 概要

OpenAI の Whisper モデルを利用して、WAVファイルからテキストへの変換（音声認識）を行います。

## 機能

- **音声ファイル選択**: OpenFileDialogでWAVファイルを選択
- **モデル自動ダウンロード**: 初回実行時にWhisperモデル(ggml-base.bin)を自動ダウンロード
- **音声認識実行**: Whisper.netを使用して音声→テキスト変換
- **結果表示**: タイムスタンプ付きで認識結果を表示

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

### 1. アプリケーションを起動

`WhisperNetSample.exe` を実行します。

初回起動時、Whisperモデル（約140MB）が自動的にダウンロードされます。

### 2. 音声ファイルを選択

「音声ファイル選択」ボタンをクリックして、WAV形式の音声ファイルを選択します。

### 3. 音声認識を実行

「音声認識開始」ボタンをクリックすると、音声認識が開始されます。

### 4. 結果を確認

認識結果が以下の形式でテキストボックスに表示されます:

```
[00:00:00.000 -> 00:00:03.500] これはサンプル音声です。
[00:00:03.500 -> 00:00:07.200] Whisper.netを使用しています。
```

## 対応形式

- **音声ファイル**: WAV形式（16kHz推奨）
- **言語**: 日本語（コード内で `WithLanguage("ja")` を指定）

他の形式を試す場合は、FFmpegなどを使用してWAV形式に変換してください。

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

- [Whisper.net](https://github.com/sandrohanea/whisper.net) - .NET用Whisperライブラリ
- Whisper.net.Runtime - CPUランタイム（AVX2対応）

## ライセンス

このサンプルプロジェクトはパブリックドメインです。自由に使用・改変できます。

## 参考リンク

- [Whisper.net GitHub](https://github.com/sandrohanea/whisper.net)
- [OpenAI Whisper](https://github.com/openai/whisper)
- [Whisper.cpp](https://github.com/ggerganov/whisper.cpp)
