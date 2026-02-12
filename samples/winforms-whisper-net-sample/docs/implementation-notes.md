# 実装メモ

このドキュメントでは、開発中に遭遇した問題と解決策、NAudioのベストプラクティスについて記録します。

## NAudio ベストプラクティス

### WasapiLoopbackCapture の正しい使い方

#### ❌ 間違った実装（リアルタイムリサンプリング）

```csharp
// この実装は音飛び（choppy audio）を引き起こす
var bufferedProvider = new BufferedWaveProvider(_wasapiCapture.WaveFormat);
_resampler = new MediaFoundationResampler(bufferedProvider, targetFormat);

_wasapiCapture.DataAvailable += (s, args) =>
{
    // バッファに追加
    bufferedProvider.AddSamples(args.Buffer, 0, args.BytesRecorded);

    // ⚠️ 問題: 全バッファを読み切ろうとする
    while (bufferedProvider.BufferedBytes > 0)
    {
        var buffer = new byte[4096];
        int bytesRead = _resampler.Read(buffer, 0, buffer.Length);
        if (bytesRead > 0)
        {
            _waveFileWriter.Write(buffer, 0, bytesRead);
        }
        else
        {
            break;
        }
    }
};
```

**問題点**:
1. DataAvailableイベントは非同期・並行的に発火
2. while ループが実行中に次のDataAvailableが発火
3. BufferedWaveProviderの読み書きタイミングが競合
4. → 音声データの一部が欠落 → 音飛び発生

#### ✅ 正しい実装（直接書き込み）

```csharp
// NAudio公式ドキュメント推奨パターン
_wasapiCapture = new WasapiLoopbackCapture();
_waveFileWriter = new WaveFileWriter(filePath, _wasapiCapture.WaveFormat);

_wasapiCapture.DataAvailable += (s, args) =>
{
    // シンプルに直接書き込み
    _waveFileWriter.Write(args.Buffer, 0, args.BytesRecorded);
};

_wasapiCapture.StartRecording();
```

**利点**:
- DataAvailableイベントで受け取ったデータをそのまま書き込む
- バッファリングの複雑さがない
- タイミング問題が発生しない
- 音飛びしない

### MediaFoundationResampler の正しい使い方

#### ❌ 間違った実装（イベント内でリサンプリング）

```csharp
// リアルタイムでリサンプリングしようとする
_wasapiCapture.DataAvailable += (s, args) =>
{
    using (var memoryStream = new MemoryStream(args.Buffer, 0, args.BytesRecorded))
    using (var rawSource = new RawSourceWaveStream(memoryStream, _wasapiCapture.WaveFormat))
    using (var resampler = new MediaFoundationResampler(rawSource, targetFormat))
    {
        // ⚠️ 問題: 毎回新しいリサンプラーを作成
        // → オーバーヘッド大、処理が間に合わない
        var buffer = new byte[4096];
        int bytesRead = resampler.Read(buffer, 0, buffer.Length);
        _waveFileWriter.Write(buffer, 0, bytesRead);
    }
};
```

**問題点**:
1. 毎回新しいMediaFoundationResamplerインスタンスを作成
2. 初期化コストが高い
3. DataAvailableイベントの処理時間が長すぎる
4. → 次のイベントが来るまでに処理が終わらない → 音飛び

#### ✅ 正しい実装（録音後にリサンプリング）

```csharp
private string ResampleToWhisperFormat(string sourceFile)
{
    var outputPath = Path.Combine(Path.GetTempPath(), $"resampled_{Guid.NewGuid()}.wav");
    var targetFormat = new WaveFormat(16000, 16, 1);  // 16kHz Mono

    using (var reader = new WaveFileReader(sourceFile))
    using (var resampler = new MediaFoundationResampler(reader, targetFormat))
    {
        // 一度だけリサンプラーを作成し、全データを変換
        WaveFileWriter.CreateWaveFile(outputPath, resampler);
    }

    return outputPath;
}
```

**利点**:
- リサンプラーの初期化は1回だけ
- 録音完了後にまとめて処理
- 時間制約がない
- 確実に全データを変換

## 問題と解決策の記録

### 問題1: PCオーディオ録音で音飛び（choppy audio）

#### 症状
- PCで再生した動画の音声を録音
- 録音されたWAVファイルが「とぎれとぎれ」
- Whisperで認識すると「(言葉)」（音声認識不可トークン）または誤認識

#### 再現手順
1. PCオーディオ録音モードを選択
2. 録音開始
3. YouTubeなどで動画を再生
4. 録音停止
5. WAVファイルを再生 → 音が途切れている

#### 根本原因の調査

**仮説1: リサンプリングバッファサイズの問題**
```csharp
// 間違ったバッファサイズ計算
var resampledBuffer = new byte[args.BytesRecorded];  // ❌
```
- 48kHz Stereo → 16kHz Mono は約1/6に縮小
- しかし元のサイズでバッファを確保
- → 余分な領域にゴミデータ

**修正試行**: 固定4096バイトバッファ + whileループ
```csharp
var buffer = new byte[4096];
while (true)
{
    int bytesRead = _resampler.Read(buffer, 0, buffer.Length);
    if (bytesRead > 0)
        _waveFileWriter.Write(buffer, 0, bytesRead);
    else
        break;
}
```
→ **結果**: まだ音飛びが発生

**仮説2: BufferedWaveProvider + while ループの競合**

NAudio公式ドキュメントとソースコードを調査:
- WasapiLoopbackCaptureの例: 直接書き込みを推奨
- BufferedWaveProviderは再生用途向け
- リアルタイムリサンプリングは推奨されていない

**最終的な根本原因**:
```
DataAvailable(スレッドA) → bufferedProvider.AddSamples()
                         → while (BufferedBytes > 0)
                           ↓
                         resampler.Read() 実行中...
                           ↓
DataAvailable(スレッドB) → bufferedProvider.AddSamples()
                         ⚠️ 同時にバッファが変更される
                           ↓
                         while継続...しかしデータ整合性が崩れる
                           ↓
                         → 音飛び発生
```

#### 解決策

**録音後リサンプリング戦略**:
1. 録音中は48kHz Stereoのまま保存（ネイティブフォーマット）
2. 録音停止時にのみ16kHz Monoへリサンプリング
3. 元ファイルを削除

**実装**:
```csharp
// Phase 1: 録音（シンプル）
private void StartPCAudioRecording()
{
    _wasapiCapture = new WasapiLoopbackCapture();
    _waveFileWriter = new WaveFileWriter(_recordingFilePath, _wasapiCapture.WaveFormat);

    _wasapiCapture.DataAvailable += (s, args) =>
    {
        _waveFileWriter.Write(args.Buffer, 0, args.BytesRecorded);
    };

    _wasapiCapture.StartRecording();
}

// Phase 2: リサンプリング（録音完了後）
private void btnStopRecording_Click(object sender, EventArgs e)
{
    // ... 録音停止処理 ...

    if (_currentRecordingMode == RecordingMode.PCAudio ||
        _currentRecordingMode == RecordingMode.Mix)
    {
        var tempPath = _recordingFilePath;
        _recordingFilePath = ResampleToWhisperFormat(tempPath);
        File.Delete(tempPath);
    }
}
```

**結果**:
- ✅ 音飛び完全に解消
- ✅ コードがシンプルで保守しやすい
- ✅ NAudio公式推奨パターンに準拠

### 問題2: .NET Framework 4.8でのビルドエラー

#### 症状
```
error CS1056: 予期しない文字 '$' です
```

#### 原因
- MSBuild 4.8.9221.0（古いビルドツール）を使用
- C# 7.3のLangVersionを明示的に指定したが、サポートされていない
- 文字列補間（`$"..."`）はC# 6.0からサポートされているが、古いコンパイラで認識されない

#### 解決策
```xml
<PropertyGroup>
  <LangVersion>default</LangVersion>
</PropertyGroup>
```

### 問題3: NuGetパッケージのtargetsファイルエラー

#### 症状1: System.ValueTuple.targets
```
error MSBuild 2003 xmlns が必要
```

#### 解決策
```xml
<Project xmlns="http://schemas.microsoft.com/developer/msbuild/2003">
```

#### 症状2: Whisper.net.Runtime.targets
```
error MSB4066: 要素 <None> の属性 "Visible" は認識されていません
```

#### 原因
- .NET Framework 4.8のMSBuildは`Visible`属性をサポートしていない
- この属性は.NET Core/.NET 5+向け

#### 解決策
```bash
sed -i 's/ Visible="false"//g' Whisper.net.Runtime.targets
sed -i 's/ Visible="false"//g' Whisper.net.Runtime.Cuda.Linux.targets
```

**注意**: これはNuGetパッケージの一時的な修正。プロジェクトクリーン後は再度適用が必要。

## C# 7.3 互換性のヒント

### 使える機能
- 文字列補間: `$"テキスト {variable}"`
- タプル: `(int, string) tuple = (1, "text");`
- ラムダ式: `x => x * 2`
- async/await

### 使えない機能
- デフォルトインターフェース実装（C# 8.0+）
- レコード（C# 9.0+）
- init アクセサー（C# 9.0+）
- パターンマッチング拡張（C# 8.0+）

### 回避策
- ラムダ式でローカル関数を代替
- タプルの代わりに匿名型やクラスを使用可能
- LINQ活用でコード簡潔化

## パフォーマンス最適化

### 録音時のメモリ使用量

#### マイク録音
- フォーマット: 16kHz Mono, 16bit
- データレート: 32 KB/秒
- 1分録音: 約1.9 MB

#### PCオーディオ録音（リサンプリング前）
- フォーマット: 48kHz Stereo, 16bit
- データレート: 192 KB/秒
- 1分録音: 約11.5 MB

#### PCオーディオ録音（リサンプリング後）
- フォーマット: 16kHz Mono, 16bit
- データレート: 32 KB/秒
- 1分録音: 約1.9 MB
- **削減率**: 約83%

### 文字起こし速度

実測値（Intel Core i7-10世代、Base モデル）:
- 1分の音声: 約5-10秒で文字起こし
- リアルタイムの約1/6～1/12の時間

## 今後の改善案

### 機能拡張
1. **WAV以外の出力対応**
   - MP3形式で保存（FFmpeg連携）
   - FLAC形式（ロスレス圧縮）

2. **リアルタイム文字起こし**
   - ストリーミングAPIの活用
   - セグメント単位で逐次処理

3. **ノイズ除去**
   - NAudio.Extras.NoiseReduction
   - プリプロセッシングで音声品質向上

### コード改善
1. **依存性注入**
   - NAudioやWhisper.netをインターフェース化
   - テスト容易性向上

2. **設定の外部化**
   - app.config で録音設定を管理
   - ユーザーがカスタマイズ可能

3. **ロギング強化**
   - NLogやSerilog導入
   - デバッグ情報の詳細化

## 参考資料

### NAudio
- [NAudio GitHub](https://github.com/naudio/NAudio)
- [NAudio Documentation](https://github.com/naudio/NAudio/blob/master/Docs/README.md)
- [Recording Audio](https://github.com/naudio/NAudio/blob/master/Docs/RecordingLevelsWaveIn.md)
- [WasapiLoopbackCapture Examples](https://github.com/naudio/NAudio/blob/master/Docs/WasapiCapture.md)

### Whisper.net
- [Whisper.net GitHub](https://github.com/sandrohanea/whisper.net)
- [API Documentation](https://sandrohanea.github.io/whisper.net/)
- [Examples](https://github.com/sandrohanea/whisper.net/tree/main/examples)

### その他
- [OpenAI Whisper論文](https://arxiv.org/abs/2212.04356)
- [.NET Framework 4.8 C# Language Support](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/configure-language-version)
