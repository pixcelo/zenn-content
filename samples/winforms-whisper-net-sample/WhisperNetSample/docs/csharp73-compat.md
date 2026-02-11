# C# 7.3互換テクニック

.NET Framework 4.8はC# 7.3までしか対応していません。
このドキュメントでは、C# 8.0以降の機能を使えない環境での対処法を説明します。

## C# 8.0+ 機能と C# 7.3 代替策

### 1. await foreach（非同期ストリーム）

#### C# 8.0の書き方

```csharp
await foreach (var item in GetItemsAsync())
{
    Console.WriteLine(item);
}
```

#### C# 7.3の書き方

```csharp
var enumerator = GetItemsAsync().GetAsyncEnumerator();
try
{
    while (await enumerator.MoveNextAsync())
    {
        var item = enumerator.Current;
        Console.WriteLine(item);
    }
}
finally
{
    await enumerator.DisposeAsync();
}
```

**必要なパッケージ**:
```xml
<PackageReference Include="Microsoft.Bcl.AsyncInterfaces" Version="10.0.0" />
```

### 2. using 宣言（using var）

#### C# 8.0の書き方

```csharp
using var stream = File.OpenRead("file.txt");
// streamは自動的にDisposeされる
```

#### C# 7.3の書き方

```csharp
using (var stream = File.OpenRead("file.txt"))
{
    // 処理
}
// ここでstreamがDisposeされる
```

### 3. null合体代入演算子（??=）

#### C# 8.0の書き方

```csharp
string value = null;
value ??= "default";  // valueがnullの場合のみ代入
```

#### C# 7.3の書き方

```csharp
string value = null;
if (value == null)
{
    value = "default";
}

// または
value = value ?? "default";
```

### 4. switch式

#### C# 8.0の書き方

```csharp
var result = value switch
{
    1 => "one",
    2 => "two",
    _ => "other"
};
```

#### C# 7.3の書き方

```csharp
string result;
switch (value)
{
    case 1:
        result = "one";
        break;
    case 2:
        result = "two";
        break;
    default:
        result = "other";
        break;
}
```

## Whisper.netでの具体例

### ProcessAsync の処理

Whisper.netの`ProcessAsync`はIAsyncEnumerable<SegmentData>を返すため、
C# 8.0では`await foreach`が使えますが、C# 7.3では手動で処理する必要があります。

#### C# 8.0+（.NET Core 3.1以降、.NET 5+）

```csharp
using var fileStream = File.OpenRead(audioFile);

await foreach (var result in processor.ProcessAsync(fileStream))
{
    var startTime = result.Start.ToString(@"mm\:ss\.ff");
    var endTime = result.End.ToString(@"mm\:ss\.ff");
    Console.WriteLine($"{startTime} -> {endTime}: {result.Text}");
}
```

#### C# 7.3（.NET Framework 4.8）

```csharp
using (var fileStream = File.OpenRead(audioFile))
{
    var enumerator = processor.ProcessAsync(fileStream).GetAsyncEnumerator();
    try
    {
        while (await enumerator.MoveNextAsync())
        {
            var result = enumerator.Current;
            var startTime = result.Start.ToString(@"mm\:ss\.ff");
            var endTime = result.End.ToString(@"mm\:ss\.ff");
            Console.WriteLine($"{startTime} -> {endTime}: {result.Text}");
        }
    }
    finally
    {
        await enumerator.DisposeAsync();
    }
}
```

## 必要なパッケージ

### Microsoft.Bcl.AsyncInterfaces

IAsyncEnumerable<T>とIAsyncDisposableのサポートを提供：

**packages.config**:
```xml
<package id="Microsoft.Bcl.AsyncInterfaces" version="10.0.0" targetFramework="net48" />
```

**.csproj** (SDK形式の場合):
```xml
<PackageReference Include="Microsoft.Bcl.AsyncInterfaces" Version="10.0.0" />
```

**従来の.csproj**:
```xml
<Reference Include="Microsoft.Bcl.AsyncInterfaces, Version=10.0.0.0, Culture=neutral, PublicKeyToken=cc7b13ffcd2ddd51">
  <HintPath>..\packages\Microsoft.Bcl.AsyncInterfaces.10.0.0\lib\net462\Microsoft.Bcl.AsyncInterfaces.dll</HintPath>
</Reference>
```

### 依存関係

Microsoft.Bcl.AsyncInterfacesは以下のパッケージに依存：

- System.Threading.Tasks.Extensions
- System.Runtime.CompilerServices.Unsafe (間接的)

これらは自動的にインストールされます。

## .NET Framework 4.8での制限事項

### 使えない機能

1. **Nullable参照型**（C# 8.0）
   ```csharp
   string? nullableString = null;  // 使えない
   ```

2. **範囲演算子**（C# 8.0）
   ```csharp
   var slice = array[1..^1];  // 使えない
   ```

3. **インデックスとレンジ**（C# 8.0）
   ```csharp
   var last = array[^1];  // 使えない
   ```

4. **パターンマッチングの拡張**（C# 8.0+）

5. **レコード型**（C# 9.0）

6. **init専用セッター**（C# 9.0）

### 回避策

#### タプル分解

C# 7.3でもタプル分解は使えます：

```csharp
var (start, end) = GetRange();

public (TimeSpan start, TimeSpan end) GetRange()
{
    return (TimeSpan.Zero, TimeSpan.FromSeconds(10));
}
```

#### ローカル関数

C# 7.3でもローカル関数は使えます：

```csharp
public void ProcessData()
{
    void LogMessage(string message)
    {
        Console.WriteLine($"[{DateTime.Now}] {message}");
    }

    LogMessage("処理開始");
    // 処理...
    LogMessage("処理完了");
}
```

## コンパイラエラーと対処法

### CS8370: 機能 'using 宣言' は C# 7.3 では使用できません

**エラーコード**:
```csharp
using var stream = File.OpenRead("file.txt");  // エラー
```

**修正**:
```csharp
using (var stream = File.OpenRead("file.txt"))
{
    // 処理
}
```

### CS8370: 機能 '非同期ストリーム' は C# 7.3 では使用できません

**エラーコード**:
```csharp
await foreach (var item in GetItemsAsync())  // エラー
```

**修正**:
```csharp
var enumerator = GetItemsAsync().GetAsyncEnumerator();
try
{
    while (await enumerator.MoveNextAsync())
    {
        var item = enumerator.Current;
        // 処理
    }
}
finally
{
    await enumerator.DisposeAsync();
}
```

### CS0246: 型または名前空間の名前 'IAsyncEnumerable<>' が見つかりませんでした

**原因**: Microsoft.Bcl.AsyncInterfacesパッケージが参照されていない

**対処**:
1. NuGetで`Microsoft.Bcl.AsyncInterfaces`をインストール
2. .csprojに参照を追加
3. `using System.Collections.Generic;`を追加

## 言語バージョンの確認

### 現在の言語バージョンを確認

```xml
<PropertyGroup>
  <LangVersion>7.3</LangVersion>
</PropertyGroup>
```

### .NET Framework 4.8で使用可能な最新バージョン

C# 7.3が最新です。C# 8.0以降を使うには.NET Core 3.0以降が必要です。

## ベストプラクティス

### 1. usingは必ず波括弧を使う

```csharp
// 良い例
using (var obj = new SomeDisposable())
{
    // 処理
}

// C# 8.0以降でのみ可能
// using var obj = new SomeDisposable();
```

### 2. IAsyncEnumerableの処理は必ずDisposeAsync

```csharp
var enumerator = source.GetAsyncEnumerator();
try
{
    // 処理
}
finally
{
    await enumerator.DisposeAsync();  // 必須
}
```

### 3. async/awaitは積極的に使う

C# 7.3でもasync/awaitは完全にサポートされています：

```csharp
public async Task<string> DownloadAsync(string url)
{
    using (var client = new HttpClient())
    {
        return await client.GetStringAsync(url);
    }
}
```

## 参考資料

### C#バージョン対応表

| C#バージョン | .NETバージョン | 主な新機能 |
|------------|--------------|----------|
| C# 7.0 | .NET Framework 4.6.2+ | タプル、パターンマッチング |
| C# 7.1 | .NET Framework 4.7+ | async Main |
| C# 7.2 | .NET Framework 4.7.1+ | ref struct |
| C# 7.3 | .NET Framework 4.7.2+ | タプル等号演算子 |
| C# 8.0 | .NET Core 3.0+ | await foreach, using宣言 |
| C# 9.0 | .NET 5.0+ | レコード型 |
| C# 10.0 | .NET 6.0+ | グローバルusing |

### 公式ドキュメント

- [C# 言語バージョン管理](https://learn.microsoft.com/ja-jp/dotnet/csharp/language-reference/configure-language-version)
- [C# バージョン履歴](https://learn.microsoft.com/ja-jp/dotnet/csharp/whats-new/csharp-version-history)
- [Microsoft.Bcl.AsyncInterfaces](https://www.nuget.org/packages/Microsoft.Bcl.AsyncInterfaces/)
