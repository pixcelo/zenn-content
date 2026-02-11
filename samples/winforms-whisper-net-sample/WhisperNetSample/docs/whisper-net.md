# Whisper.net 使い方

Whisper.netは、OpenAIのWhisperモデルをC#/.NETで利用するためのライブラリです。
whisper.cpp（C++実装）のバインディングとして動作します。

## 基本的な使い方

### 1. モデルのダウンロード

```csharp
using Whisper.net.Ggml;

// モデルがなければダウンロード
var modelPath = "ggml-base.bin";
if (!File.Exists(modelPath))
{
    using var modelStream = await WhisperGgmlDownloader.Default.GetGgmlModelAsync(GgmlType.Base);
    using var fileWriter = File.OpenWrite(modelPath);
    await modelStream.CopyToAsync(fileWriter);
}
```

### 2. WhisperFactoryの作成

```csharp
using Whisper.net;

using (var whisperFactory = WhisperFactory.FromPath("ggml-base.bin"))
{
    // whisperFactoryを使った処理
}
```

### 3. WhisperProcessorのビルド

```csharp
using (var processor = whisperFactory.CreateBuilder()
    .WithLanguage("auto")  // 自動言語検出
    .Build())
{
    // processorを使った文字起こし処理
}
```

### 4. 音声ファイルの処理

#### C# 8.0以降の場合（await foreach使用）

```csharp
using var fileStream = File.OpenRead("audio.wav");

await foreach (var result in processor.ProcessAsync(fileStream))
{
    Console.WriteLine($"{result.Start}->{result.End}: {result.Text}");
}
```

#### C# 7.3の場合（.NET Framework 4.8など）

```csharp
using (var fileStream = File.OpenRead("audio.wav"))
{
    var enumerator = processor.ProcessAsync(fileStream).GetAsyncEnumerator();
    try
    {
        while (await enumerator.MoveNextAsync())
        {
            var result = enumerator.Current;
            Console.WriteLine($"{result.Start}->{result.End}: {result.Text}");
        }
    }
    finally
    {
        await enumerator.DisposeAsync();
    }
}
```

## 利用可能なモデル

### モデルの種類と特徴

| モデル | ファイルサイズ | 精度 | 速度 | 推奨用途 |
|--------|--------------|------|------|----------|
| Tiny | 75MB | ★☆☆☆☆ | ★★★★★ | デモ・リアルタイム処理 |
| Base | 142MB | ★★☆☆☆ | ★★★★☆ | バランス型（推奨） |
| Small | 466MB | ★★★☆☆ | ★★★☆☆ | 高精度が必要な場合 |
| Medium | 1.5GB | ★★★★☆ | ★★☆☆☆ | さらに高精度が必要 |
| LargeV3 | 2.9GB | ★★★★★ | ★☆☆☆☆ | 最高精度が必要 |
| LargeV3Turbo | 1.6GB | ★★★★★ | ★★☆☆☆ | 最新・高精度・高速 |

### GgmlType列挙型

```csharp
public enum GgmlType
{
    Tiny,
    TinyEn,      // 英語専用
    Base,
    BaseEn,      // 英語専用
    Small,
    SmallEn,     // 英語専用
    Medium,
    MediumEn,    // 英語専用
    LargeV1,     // 古いバージョン
    LargeV2,     // 古いバージョン
    LargeV3,     // 最新の大型モデル
    LargeV3Turbo // 最新の高速大型モデル
}
```

## WhisperProcessorのオプション

### 言語設定

```csharp
.WithLanguage("auto")     // 自動検出
.WithLanguage("ja")       // 日本語
.WithLanguage("en")       // 英語
```

### その他のオプション

```csharp
var processor = whisperFactory.CreateBuilder()
    .WithLanguage("auto")
    .WithThreads(4)                    // 使用するスレッド数
    .WithSingleSegment(false)          // セグメント分割
    .WithTranslate(false)              // 英語への翻訳（falseで無効）
    .Build();
```

## SegmentData（結果データ）

文字起こし結果は`SegmentData`として返されます：

```csharp
public class SegmentData
{
    public TimeSpan Start { get; }     // 開始時刻
    public TimeSpan End { get; }       // 終了時刻
    public string Text { get; }        // テキスト
    public float Probability { get; }  // 確率
    // その他のプロパティ...
}
```

### 時刻の扱い

```csharp
// TimeSpan型で返される
var startTime = result.Start.ToString(@"mm\:ss\.ff");  // "01:23.45"
var endTime = result.End.ToString(@"mm\:ss\.ff");      // "01:25.67"
```

## 音声形式の要件

### 推奨フォーマット

- **サンプリングレート**: 16kHz
- **チャンネル**: Mono（1ch）
- **ビット深度**: 16bit
- **形式**: WAV（PCM）

### サポートされている入力

Whisper.netは内部で音声を16kHz Monoに変換しますが、
最初から16kHz Monoで用意すると処理が高速になります。

## エラーハンドリング

### よくあるエラー

#### 1. Native Library not found

**原因**: ネイティブライブラリ（whisper.dll等）が見つからない

**対策**:
- `Whisper.net.Runtime`パッケージがインストールされているか確認
- .csprojに`Whisper.net.Runtime.targets`のImportがあるか確認

```xml
<Import Project="..\packages\Whisper.net.Runtime.1.9.0\build\Whisper.net.Runtime.targets"
        Condition="Exists('..\packages\Whisper.net.Runtime.1.9.0\build\Whisper.net.Runtime.targets')" />
```

#### 2. モデルダウンロードの失敗

**原因**: ネットワークエラーまたはHugging Faceのレート制限

**対策**:
```csharp
try
{
    using var modelStream = await WhisperGgmlDownloader.Default.GetGgmlModelAsync(GgmlType.Base);
    // ...
}
catch (HttpRequestException ex)
{
    // リトライ処理またはエラー表示
    Console.WriteLine($"ダウンロード失敗: {ex.Message}");
}
```

## パフォーマンス最適化

### 1. スレッド数の調整

```csharp
// CPUコア数に応じて調整
var threadCount = Environment.ProcessorCount / 2;
.WithThreads(threadCount)
```

### 2. モデルの選択

- リアルタイム処理: Tiny または Base
- 高精度が必要: Small または Medium
- 最高精度: LargeV3Turbo（V3より高速）

### 3. GPU加速（オプション）

CUDA対応GPUがある場合、`Whisper.net.Runtime.Cuda`パッケージを使用：

```xml
<PackageReference Include="Whisper.net.Runtime.Cuda.Windows" Version="1.9.0" />
```

## .NET Framework 4.8での注意点

### IAsyncEnumerableの扱い

.NET Framework 4.8では`await foreach`が使えません。
`Microsoft.Bcl.AsyncInterfaces`パッケージを追加し、手動でenumeratorを操作します：

```xml
<PackageReference Include="Microsoft.Bcl.AsyncInterfaces" Version="10.0.0" />
```

```csharp
var enumerator = processor.ProcessAsync(fileStream).GetAsyncEnumerator();
try
{
    while (await enumerator.MoveNextAsync())
    {
        var result = enumerator.Current;
        // 処理
    }
}
finally
{
    await enumerator.DisposeAsync();
}
```

## 参考リンク

- [Whisper.net GitHub](https://github.com/sandrohanea/whisper.net)
- [whisper.cpp](https://github.com/ggerganov/whisper.cpp)
- [OpenAI Whisper](https://github.com/openai/whisper)
- [Hugging Face モデルリポジトリ](https://huggingface.co/ggerganov/whisper.cpp)
