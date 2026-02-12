# アーキテクチャ概要

このドキュメントでは、Whisper.net音声認識サンプルアプリケーションのアーキテクチャと設計判断について説明します。

## システム構成

```
┌─────────────────────────────────────────────────────────────┐
│                     Windows Forms UI                         │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐      │
│  │ ファイル選択  │  │  録音モード  │  │  文字起こし  │      │
│  │    ボタン    │  │  選択ラジオ  │  │    ボタン    │      │
│  └──────────────┘  └──────────────┘  └──────────────┘      │
│  ┌──────────────┐  ┌──────────────┐                        │
│  │  録音開始    │  │  録音停止    │                        │
│  │   ボタン     │  │   ボタン     │                        │
│  └──────────────┘  └──────────────┘                        │
│  ┌────────────────────────────────────────────────┐        │
│  │          DataGridView (結果表示)               │        │
│  └────────────────────────────────────────────────┘        │
└─────────────────────────────────────────────────────────────┘
                            ↓
┌─────────────────────────────────────────────────────────────┐
│                  アプリケーションロジック                    │
│  ┌────────────────┐  ┌────────────────┐                    │
│  │ 音声入力処理    │  │ 音声認識処理    │                    │
│  │ (NAudio)       │  │ (Whisper.net)  │                    │
│  └────────────────┘  └────────────────┘                    │
└─────────────────────────────────────────────────────────────┘
                            ↓
┌─────────────────────────────────────────────────────────────┐
│                    外部ライブラリ                            │
│  ┌────────────────┐  ┌────────────────┐                    │
│  │    NAudio      │  │  Whisper.net   │                    │
│  │    2.2.1       │  │    1.9.0       │                    │
│  └────────────────┘  └────────────────┘                    │
└─────────────────────────────────────────────────────────────┘
```

## 録音処理フロー

### 1. マイク録音

```
ユーザー操作
   ↓
[録音開始] → WaveInEvent初期化 (16kHz Mono)
   ↓
DataAvailableイベント → WAVファイルに直接書き込み
   ↓
[録音停止] → WaveFileWriter.Dispose()
   ↓
文字起こし準備完了
```

**特徴**:
- 直接16kHz Monoで録音
- リサンプリング不要
- シンプルで高速

### 2. PCオーディオ録音

```
ユーザー操作
   ↓
[録音開始] → WasapiLoopbackCapture初期化 (48kHz Stereo)
   ↓
DataAvailableイベント → WAVファイルに直接書き込み (48kHz Stereo)
   ↓
[録音停止] → WaveFileWriter.Dispose()
   ↓
ResampleToWhisperFormat() → 16kHz Monoに変換
   ↓
元ファイル削除
   ↓
文字起こし準備完了
```

**特徴**:
- ネイティブフォーマット（48kHz Stereo）で録音
- 停止時にのみリサンプリング実行
- 録音中の音飛び（choppy audio）を防止

### 3. ミックス録音

```
ユーザー操作
   ↓
[録音開始] → WaveInEvent + WasapiLoopbackCapture初期化
   ↓
各DataAvailableイベント → BufferedWaveProvider経由でミキシング
   ↓
ProcessMixedAudio() → 両方のバッファからデータを取得・合成
   ↓
WAVファイルに書き込み (48kHz Stereo)
   ↓
[録音停止] → WaveFileWriter.Dispose()
   ↓
ResampleToWhisperFormat() → 16kHz Monoに変換
   ↓
元ファイル削除
   ↓
文字起こし準備完了
```

**特徴**:
- マイク（16kHz Mono）とPCオーディオ（48kHz Stereo）を同時録音
- BufferedWaveProviderで一時バッファリング
- イベント駆動でミキシング処理
- 録音停止後にリサンプリング

## リサンプリング戦略

### 設計判断: なぜ録音後リサンプリングなのか？

#### 当初の実装（リアルタイムリサンプリング）

```csharp
// ❌ 問題のあった実装
_wasapiCapture.DataAvailable += (s, args) =>
{
    bufferedProvider.AddSamples(args.Buffer, 0, args.BytesRecorded);

    while (bufferedProvider.BufferedBytes > 0)  // ⚠️ 音飛びの原因
    {
        var buffer = new byte[4096];
        int bytesRead = _resampler.Read(buffer, 0, buffer.Length);
        if (bytesRead > 0)
        {
            _waveFileWriter.Write(buffer, 0, bytesRead);
        }
    }
};
```

**問題点**:
1. DataAvailableイベントは非同期で発火
2. while ループ内でバッファを全部読み切ろうとする
3. しかし同時に新しいDataAvailableイベントが発火
4. → バッファの読み書きタイミングがずれる
5. → 音飛び（choppy audio）が発生

#### 改善後の実装（録音後リサンプリング）

```csharp
// ✅ 録音中（リサンプリングなし）
_wasapiCapture.DataAvailable += (s, args) =>
{
    _waveFileWriter.Write(args.Buffer, 0, args.BytesRecorded);  // シンプル
};

// ✅ 録音停止時（リサンプリング実行）
private string ResampleToWhisperFormat(string sourceFile)
{
    var outputPath = Path.Combine(Path.GetTempPath(), $"resampled_{Guid.NewGuid()}.wav");
    var targetFormat = new WaveFormat(16000, 16, 1);

    using (var reader = new WaveFileReader(sourceFile))
    using (var resampler = new MediaFoundationResampler(reader, targetFormat))
    {
        WaveFileWriter.CreateWaveFile(outputPath, resampler);
    }

    return outputPath;
}
```

**利点**:
1. 録音中はシンプルな直接書き込み
2. 非同期イベントの競合がない
3. 音飛びが発生しない
4. コードが読みやすく保守しやすい

**トレードオフ**:
- 録音中のファイルサイズが大きい（48kHz Stereo）
- 停止時にわずかな処理時間が必要（通常1秒未満）
- → しかし音質の信頼性が優先

## 文字起こし処理フロー

```
WAVファイル (16kHz Mono)
   ↓
WhisperFactory.CreateBuilder()
   ↓
WithLanguage("ja") 設定
   ↓
Build() → WhisperProcessor作成
   ↓
ProcessAsync(audioFilePath)
   ↓
セグメントごとに結果を返す
   ↓
DataGridViewに表示
```

## データフロー図

```
┌─────────────┐
│ 音声入力源   │
│ - マイク     │
│ - PCオーディオ│
└─────────────┘
       ↓
┌─────────────┐
│   NAudio    │
│ 録音処理     │
└─────────────┘
       ↓
┌─────────────┐
│ WAVファイル  │
│ (一時保存)   │
└─────────────┘
       ↓
┌─────────────┐
│リサンプリング │
│ (必要時のみ) │
└─────────────┘
       ↓
┌─────────────┐
│ 16kHz Mono  │
│  WAVファイル │
└─────────────┘
       ↓
┌─────────────┐
│ Whisper.net │
│  音声認識    │
└─────────────┘
       ↓
┌─────────────┐
│ 認識結果     │
│ (セグメント) │
└─────────────┘
       ↓
┌─────────────┐
│DataGridView │
│   表示       │
└─────────────┘
```

## モデル管理

### モデル自動ダウンロード

初回起動時、WhisperFactory.FromPathメソッドがモデルファイルの存在を確認し、存在しない場合は自動的にHugging Faceからダウンロードします。

```csharp
var modelPath = "ggml-base.bin";
_whisperFactory = WhisperFactory.FromPath(modelPath);
// モデルが存在しない場合、自動ダウンロードが開始される
```

### モデル選択

ComboBoxでモデルを選択可能：
- Tiny (75MB) - 高速、精度低
- Base (140MB) - バランス型（デフォルト）
- Small (460MB) - 高精度、やや遅い
- Medium (1.5GB) - より高精度
- Large (2.9GB) - 最高精度、低速

## エラーハンドリング

各処理でtry-catchを実装：

1. **モデルダウンロード失敗**
   - インターネット接続確認メッセージ
   - 手動ダウンロードの案内

2. **録音デバイスなし**
   - マイク/オーディオデバイスの接続確認

3. **ファイル形式エラー**
   - 対応形式（WAV/MP3）の案内

4. **音声認識失敗**
   - 詳細エラーメッセージ表示
   - ファイル形式・音声品質の確認案内

## パフォーマンス考慮

1. **非同期処理**
   - 音声認識はasync/awaitで実行
   - UI応答性を維持

2. **メモリ管理**
   - using文でリソース適切に解放
   - 一時ファイルは録音完了後に削除

3. **ファイルサイズ最適化**
   - リサンプリングでファイルサイズを約1/6に削減
   - 一時ファイルの早期削除

## 参考資料

- [NAudio公式ドキュメント](https://github.com/naudio/NAudio)
- [Whisper.net GitHub](https://github.com/sandrohanea/whisper.net)
- [OpenAI Whisper論文](https://arxiv.org/abs/2212.04356)
