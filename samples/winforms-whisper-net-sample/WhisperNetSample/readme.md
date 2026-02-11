# Whisper.net サンプル - Windows Forms (.NET Framework 4.8)

Whisper.netを使用した音声文字起こしのWindows Formsサンプルアプリケーションです。

## 機能

### 1. Whisperモデル選択
- 6種類のモデルから選択可能
  - **Tiny (75MB)** - 最小・高速
  - **Base (142MB)** - バランス型（推奨）
  - **Small (466MB)** - 高精度
  - **Medium (1.5GB)** - より高精度
  - **LargeV3 (2.9GB)** - 最高精度
  - **LargeV3Turbo (1.6GB)** - 最新・高精度・高速
- モデルは初回起動時にHugging Faceから自動ダウンロード
- 一度ダウンロードしたモデルは再利用（`ggml-{modeltype}.bin`として保存）

### 2. 音声ファイル文字起こし
- **対応形式**: WAV、MP3
- MP3ファイルは自動的にWAVに変換（16kHz Mono）
- 文字起こし結果をDataGridViewに表示
  - 開始時刻（mm:ss.ff形式）
  - 終了時刻（mm:ss.ff形式）
  - テキスト（発話内容）
- 自動言語検出（日本語対応）

### 3. マイク録音
- 🎤 録音開始 / ⏹ 停止ボタン
- リアルタイム録音状態表示
- 16kHz Monoで録音（Whisper推奨設定）
- 録音後すぐに文字起こし可能

## 技術仕様

- **.NET Framework**: 4.8
- **Whisper.net**: 1.9.0
- **NAudio**: 2.2.1（MP3変換・録音機能）
- **言語バージョン**: C# 7.3
  - `await foreach`の代わりに`GetAsyncEnumerator()`を使用
  - `using var`の代わりに従来の`using`文を使用

## セットアップ

### 必須要件
- Visual Studio 2019以降
- .NET Framework 4.8
- Microsoft Visual C++ 再頒布可能パッケージ（Visual Studio 2022 x64）
  - [ダウンロード](https://learn.microsoft.com/en-us/cpp/windows/latest-supported-vc-redist)

### ビルド手順
1. Visual Studioでソリューションを開く
2. NuGetパッケージを復元
3. ビルド実行

## 使い方

### 音声ファイルから文字起こし
1. モデルを選択（初回は自動ダウンロード）
2. 「ファイル選択」ボタンでWAVまたはMP3ファイルを選択
3. 「文字起こし実行」ボタンをクリック
4. 結果がDataGridViewに表示される

### マイク録音から文字起こし
1. モデルを選択
2. 🎤「録音開始」ボタンをクリック
3. 音声を録音
4. ⏹「停止」ボタンをクリック
5. 「文字起こし実行」ボタンをクリック

## 実装の特徴

### C# 7.3互換
.NET Framework 4.8ではC# 8.0の`await foreach`が使えないため、以下のように実装：

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

### MP3変換
NAudioの`MediaFoundationResampler`を使用して16kHz Monoに変換：

```csharp
using (var reader = new Mp3FileReader(mp3FilePath))
{
    WaveFormat targetFormat = new WaveFormat(16000, 16, 1);
    using (var resampler = new MediaFoundationResampler(reader, targetFormat))
    {
        WaveFileWriter.CreateWaveFile(tempWavPath, resampler);
    }
}
```

### マイク録音
NAudioの`WaveInEvent`を使用：

```csharp
waveIn = new WaveInEvent();
waveIn.WaveFormat = new WaveFormat(16000, 16, 1); // 16kHz Mono
waveFileWriter = new WaveFileWriter(recordingFilePath, waveIn.WaveFormat);

waveIn.DataAvailable += (s, args) =>
{
    waveFileWriter.Write(args.Buffer, 0, args.BytesRecorded);
};

waveIn.StartRecording();
```

## プロジェクト構成

```
WhisperNetSample/
├── Form1.cs                    # メインフォームのロジック
├── Form1.Designer.cs           # UIデザイン定義
├── PopupForm.cs                # サンプル用ポップアップ
├── Program.cs                  # エントリーポイント
├── WhisperNetSample.csproj     # プロジェクトファイル
├── packages.config             # NuGet パッケージ設定
├── README.md                   # このファイル
└── docs/                       # 技術ドキュメント
    ├── whisper-net.md          # Whisper.net使い方
    └── naudio.md               # NAudio使い方
```

## C# 7.3での注意点

.NET Framework 4.8ではC# 8.0の`await foreach`や`using var`が使えません。

### await foreach の代替

```csharp
// ❌ C# 8.0+（使えない）
await foreach (var result in processor.ProcessAsync(fileStream))
{
    // 処理
}

// ✅ C# 7.3（このサンプルで使用）
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

### using var の代替

```csharp
// ❌ C# 8.0+（使えない）
using var stream = File.OpenRead("file.wav");

// ✅ C# 7.3（このサンプルで使用）
using (var stream = File.OpenRead("file.wav"))
{
    // 処理
}
```

詳細は[Microsoft公式ドキュメント](https://learn.microsoft.com/ja-jp/dotnet/csharp/language-reference/configure-language-version)を参照してください。

## トラブルシューティング

### ビルドが終わらない
- bin/objフォルダを削除してクリーンビルド
- Visual Studioを再起動

### 「Native Library not found」エラー
- WhisperNetSample.csprojに`Whisper.net.Runtime.targets`のImportが含まれているか確認
- NuGetパッケージを復元

### MP3が変換できない
- NAudioのnet472版が参照されているか確認
- MediaFoundation が利用可能なWindows環境か確認

## ライセンス

このサンプルコードはMITライセンスです。

## 参考リンク

- [Whisper.net GitHub](https://github.com/sandrohanea/whisper.net)
- [NAudio GitHub](https://github.com/naudio/NAudio)
- [OpenAI Whisper](https://github.com/openai/whisper)
