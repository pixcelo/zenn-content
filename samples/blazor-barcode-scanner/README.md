# BlazorBarcodeScanner.ZXing.JS サンプル

BlazorBarcodeScanner.ZXing.JSライブラリを使用したバーコード/QRコードスキャナーのサンプルプロジェクトです。

## 概要

このサンプルは、Blazor WebAssemblyアプリケーションでカメラを使用してバーコードやQRコードをスキャンする方法を示しています。

## 使用技術

- **フレームワーク**: Blazor WebAssembly (.NET 8.0)
- **ライブラリ**: BlazorBarcodeScanner.ZXing.JS 1.0.4
- **バーコードエンジン**: ZXing.js (Zebra Crossing JavaScript版)

## 対応バーコード形式

- QRコード
- EAN-13
- EAN-8
- Code 128
- Code 39
- ITF
- その他ZXing.jsがサポートする形式

## セットアップ

### 1. 依存関係のインストール

```bash
dotnet restore
```

### 2. アプリケーションの実行

```bash
dotnet run
```

または

```bash
dotnet watch
```

ブラウザで `https://localhost:5001` (または表示されたURL) にアクセスします。

## 使い方

1. **スキャン開始**: 「スキャン開始」ボタンをクリック
2. **カメラ許可**: ブラウザがカメラへのアクセス許可を求めたら「許可」を選択
3. **スキャン**: バーコードまたはQRコードをカメラにかざす
4. **結果確認**: スキャン結果が右側のパネルに表示されます
5. **スキャン停止**: 「スキャン停止」ボタンでカメラを停止

## 機能

- ✅ リアルタイムバーコード/QRコードスキャン
- ✅ スキャン結果の表示
- ✅ スキャン履歴（最新10件）
- ✅ エラーハンドリング
- ✅ カメラの開始/停止制御

## 実装のポイント

### 1. JavaScript参照の追加 (wwwroot/index.html)

```html
<script src="_content/BlazorBarcodeScanner.ZXing.JS/zxingjs.index.min.js"></script>
<script src="_content/BlazorBarcodeScanner.ZXing.JS/BlazorBarcodeScanner.js"></script>
```

### 2. BarcodeReaderコンポーネントの使用

```razor
<BarcodeReader
    Title="バーコード/QRコードをかざしてください"
    ShowStart="true"
    OnBarcodeReceived="OnBarcodeDetected"
    OnError="OnError" />
```

### 3. イベントハンドラー

```csharp
private void OnBarcodeDetected(BarcodeReceivedEventArgs args)
{
    // args.BarcodeText でスキャン結果を取得
}

private void OnError(ErrorReceivedEventArgs args)
{
    // args.ErrorMessage でエラー内容を取得
}
```

## 注意事項

### HTTPSが必要

カメラアクセスにはHTTPS接続が必要です。開発環境では自己署名証明書が使用されます。

### ブラウザ対応

- Chrome/Edge: 完全対応
- Firefox: 完全対応
- Safari: iOS 11以降で対応
- モバイルブラウザ: 最新版を推奨

### カメラ権限

初回実行時にブラウザがカメラへのアクセス許可を求めます。許可しないとスキャナーは動作しません。

## トラブルシューティング

### カメラが起動しない場合

1. ブラウザのカメラ権限設定を確認
2. 他のアプリケーションがカメラを使用していないか確認
3. HTTPSで接続されているか確認

### スキャンが遅い場合

- 照明を明るくする
- バーコード/QRコードをカメラに近づける
- カメラのフォーカスが合うまで待つ

## 参考リンク

- [BlazorBarcodeScanner.ZXing.JS - NuGet](https://www.nuget.org/packages/BlazorBarcodeScanner.ZXing.JS/)
- [ZXing.js GitHub](https://github.com/zxing-js/library)
- [Blazor公式ドキュメント](https://learn.microsoft.com/ja-jp/aspnet/core/blazor/)

## ライセンス

このサンプルコードはMITライセンスの下で提供されています。
