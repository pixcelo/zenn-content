# NAudio 使い方

NAudioは.NET向けの音声処理ライブラリです。
このサンプルでは、MP3変換とマイク録音にNAudioを使用しています。

## 基本概念

### WaveFormat

音声フォーマットを表すクラス：

```csharp
// コンストラクタ: WaveFormat(sampleRate, bits, channels)
var format = new WaveFormat(16000, 16, 1);
// 16kHz, 16bit, Mono
```

**Whisper推奨フォーマット**:
- サンプリングレート: 16000Hz (16kHz)
- ビット深度: 16bit
- チャンネル数: 1 (Mono)

## MP3からWAVへの変換

### 基本的な変換

```csharp
using NAudio.Wave;

using (var reader = new Mp3FileReader("input.mp3"))
{
    WaveFileWriter.CreateWaveFile("output.wav", reader);
}
```

### リサンプリング（サンプリングレート変換）

```csharp
using (var reader = new Mp3FileReader("input.mp3"))
{
    // Whisper推奨の16kHz Monoに変換
    WaveFormat targetFormat = new WaveFormat(16000, 16, 1);

    using (var resampler = new MediaFoundationResampler(reader, targetFormat))
    {
        WaveFileWriter.CreateWaveFile("output.wav", resampler);
    }
}
```

### MediaFoundationResampler

Windows Media Foundationを使用した高品質なリサンプリング：

**メリット**:
- 高品質な変換
- Windows標準機能を利用（追加インストール不要）

**必須条件**:
- Windows Vista以降
- .NET Framework 4.5以降または.NET Core/.NET 5+

### エラーハンドリング

```csharp
try
{
    using (var reader = new Mp3FileReader(mp3FilePath))
    {
        WaveFormat targetFormat = new WaveFormat(16000, 16, 1);
        using (var resampler = new MediaFoundationResampler(reader, targetFormat))
        {
            WaveFileWriter.CreateWaveFile(wavPath, resampler);
        }
    }
}
catch (Exception ex)
{
    Console.WriteLine($"変換エラー: {ex.Message}");
}
```

## マイク録音

### WaveInEvent を使った録音

```csharp
using NAudio.Wave;

private WaveInEvent waveIn;
private WaveFileWriter waveFileWriter;

// 録音開始
private void StartRecording()
{
    // WaveInEventの初期化
    waveIn = new WaveInEvent();
    waveIn.WaveFormat = new WaveFormat(16000, 16, 1); // 16kHz Mono

    // ファイルライターの初期化
    waveFileWriter = new WaveFileWriter("recording.wav", waveIn.WaveFormat);

    // データ受信イベント
    waveIn.DataAvailable += (sender, e) =>
    {
        waveFileWriter.Write(e.Buffer, 0, e.BytesRecorded);
    };

    // 録音開始
    waveIn.StartRecording();
}

// 録音停止
private void StopRecording()
{
    waveIn?.StopRecording();
    waveIn?.Dispose();
    waveFileWriter?.Dispose();
}
```

### 利用可能なマイクデバイスの確認

```csharp
// デバイス数の取得
int deviceCount = WaveInEvent.DeviceCount;

if (deviceCount == 0)
{
    Console.WriteLine("マイクが見つかりません");
    return;
}

// デバイス情報の取得
for (int i = 0; i < deviceCount; i++)
{
    var capabilities = WaveInEvent.GetCapabilities(i);
    Console.WriteLine($"デバイス {i}: {capabilities.ProductName}");
}
```

### 特定のデバイスで録音

```csharp
waveIn = new WaveInEvent();
waveIn.DeviceNumber = 0; // デバイス番号を指定（デフォルトは0）
waveIn.WaveFormat = new WaveFormat(16000, 16, 1);
```

## Windows Formsでの録音実装例

### UIスレッドセーフな録音

```csharp
public partial class Form1 : Form
{
    private WaveInEvent waveIn;
    private WaveFileWriter waveFileWriter;
    private string recordingFilePath;

    private void btnStartRecording_Click(object sender, EventArgs e)
    {
        if (WaveInEvent.DeviceCount == 0)
        {
            MessageBox.Show("マイクが見つかりません");
            return;
        }

        recordingFilePath = Path.Combine(
            Path.GetTempPath(),
            $"recording_{Guid.NewGuid()}.wav"
        );

        waveIn = new WaveInEvent();
        waveIn.WaveFormat = new WaveFormat(16000, 16, 1);
        waveFileWriter = new WaveFileWriter(recordingFilePath, waveIn.WaveFormat);

        waveIn.DataAvailable += (s, args) =>
        {
            waveFileWriter.Write(args.Buffer, 0, args.BytesRecorded);

            // UIスレッドで進捗表示する場合
            this.Invoke(new Action(() =>
            {
                labelStatus.Text = $"録音中: {args.BytesRecorded} bytes";
            }));
        };

        waveIn.StartRecording();

        // UI状態更新
        btnStartRecording.Enabled = false;
        btnStopRecording.Enabled = true;
    }

    private void btnStopRecording_Click(object sender, EventArgs e)
    {
        waveIn?.StopRecording();
        waveIn?.Dispose();
        waveFileWriter?.Dispose();

        // UI状態更新
        btnStartRecording.Enabled = true;
        btnStopRecording.Enabled = false;
        labelStatus.Text = $"録音完了: {recordingFilePath}";
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            // リソースのクリーンアップ
            waveIn?.StopRecording();
            waveIn?.Dispose();
            waveFileWriter?.Dispose();
        }
        base.Dispose(disposing);
    }
}
```

## その他の音声処理

### WAVファイルの読み込み

```csharp
using (var reader = new WaveFileReader("audio.wav"))
{
    Console.WriteLine($"サンプリングレート: {reader.WaveFormat.SampleRate}Hz");
    Console.WriteLine($"チャンネル数: {reader.WaveFormat.Channels}");
    Console.WriteLine($"ビット深度: {reader.WaveFormat.BitsPerSample}bit");
    Console.WriteLine($"長さ: {reader.TotalTime}");
}
```

### 音声の再生

```csharp
using (var audioFile = new AudioFileReader("audio.wav"))
using (var outputDevice = new WaveOutEvent())
{
    outputDevice.Init(audioFile);
    outputDevice.Play();

    // 再生が終わるまで待機
    while (outputDevice.PlaybackState == PlaybackState.Playing)
    {
        Thread.Sleep(100);
    }
}
```

## .NET Framework 4.8での注意点

### パッケージの参照

.NET Framework 4.8では、net472版のNAudioを使用します：

```xml
<Reference Include="NAudio, Version=2.2.1.0, Culture=neutral, PublicKeyToken=e279aa5131008a41">
  <HintPath>..\packages\NAudio.2.2.1\lib\net472\NAudio.dll</HintPath>
</Reference>
```

netstandard2.0版ではなく、**net472版**を参照することが重要です。

### packages.config

```xml
<packages>
  <package id="NAudio" version="2.2.1" targetFramework="net48" />
  <package id="NAudio.Asio" version="2.2.1" targetFramework="net48" />
  <package id="NAudio.Core" version="2.2.1" targetFramework="net48" />
  <package id="NAudio.Wasapi" version="2.2.1" targetFramework="net48" />
  <package id="NAudio.WinMM" version="2.2.1" targetFramework="net48" />
</packages>
```

## トラブルシューティング

### Mp3FileReader が見つからない

**エラー**: `型または名前空間の名前 'Mp3FileReader' が見つかりませんでした`

**原因**: NAudioパッケージが正しく参照されていない

**対策**:
1. NAudioのnet472版を参照
2. `using NAudio.Wave;`を追加
3. NuGetパッケージを復元

### MediaFoundationResampler が使えない

**原因**: Windows Media Foundationが利用できない環境

**対策**:
- Windows Vista以降で実行
- 別のリサンプラー（WdlResamplerやResamplerDmoStream）を検討

### 録音時にノイズが入る

**原因**: バッファサイズが小さすぎる

**対策**:
```csharp
waveIn.BufferMilliseconds = 50; // デフォルトは20ms
```

## パフォーマンス最適化

### バッファサイズの調整

```csharp
// リアルタイム性重視（低レイテンシ）
waveIn.BufferMilliseconds = 20;

// 安定性重視（ノイズ低減）
waveIn.BufferMilliseconds = 100;
```

### 一時ファイルの削除

```csharp
// 処理完了後に一時ファイルを削除
if (File.Exists(tempWavPath))
{
    try
    {
        File.Delete(tempWavPath);
    }
    catch
    {
        // ファイルが使用中の場合があるため、例外を無視
    }
}
```

## 参考リンク

- [NAudio GitHub](https://github.com/naudio/NAudio)
- [NAudio Documentation](https://github.com/naudio/NAudio/tree/master/Docs)
- [NAudio Tutorial](https://markheath.net/category/naudio)
