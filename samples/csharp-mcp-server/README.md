# C# MCP Server サンプルプロジェクト

このプロジェクトは、Zenn記事「[C#でMCPサーバーを作ってみよう](../../contents/articles/csharp-mcp-server.md)」で説明されているMCPサーバーの完全なサンプル実装です。

## 📁 プロジェクト構成

```
SimpleMcpServer/
├── Program.cs              # エントリーポイント・MCPサーバー設定
├── SimpleMcpServer.csproj  # プロジェクトファイル
└── Tools/
    └── SimpleMcpTools.cs   # MCPツール実装（Ping, Echo）
```

## 🚀 セットアップ手順

### 前提条件

- **.NET 10.0 SDK** (Preview 6以上)
- **Claude Code CLI**

### 1. プロジェクトのクローン

```bash
git clone https://github.com/pixcelo/zenn-content.git
cd zenn-content/samples/csharp-mcp-server/SimpleMcpServer
```

### 2. ビルドと実行

```bash
# 依存関係の復元とビルド
dotnet build

# 開発時の実行
dotnet run

# 本番用ビルド（推奨）
dotnet publish -c Release --self-contained true -r win-x64
```

### 3. Claude Code設定

```bash
# プロジェクトディレクトリで実行（開発時）
claude mcp add simple-mcp-server dotnet run

# または本番用実行ファイルを使用
claude mcp add simple-mcp-server "C:/path/to/bin/Release/net8.0/win-x64/publish/SimpleMcpServer.exe"
```

### 4. 動作確認

```bash
# サーバー接続状況を確認
claude mcp list

# 接続が成功していれば ✓ Connected と表示される
```

## 🛠️ 実装されているツール

### 1. Ping ツール
- **機能**: MCPサーバーの基本動作確認
- **パラメータ**: なし
- **戻り値**: `"🏓 Pong from SimpleMcpTools!"`

### 2. Echo ツール
- **機能**: 文字列をそのまま返す
- **パラメータ**: `message` (文字列、デフォルト: "Hello World")
- **戻り値**: `"Echo: {入力文字列}"`

## 🔧 トラブルシューティング

### サーバーに接続できない

1. **パス形式の確認**
   ```bash
   # ❌ 失敗例（Windowsの場合）
   claude mcp add server C:\path\to\server.exe
   
   # ✅ 成功例
   claude mcp add server "C:/path/to/server.exe"
   ```

2. **実行ファイルの存在確認**
   ```bash
   # パブリッシュされているか確認
   ls bin/Release/net8.0/win-x64/publish/SimpleMcpServer.exe
   ```

3. **Claude Code再起動**
   ```bash
   # サーバー登録後、Claude Codeを再起動
   ```

### ビルドエラー

1. **.NET SDK バージョン確認**
   ```bash
   dotnet --version
   # 10.0.100-preview.6.xxx または以降が必要
   ```

2. **依存関係の復元**
   ```bash
   dotnet restore
   dotnet clean
   dotnet build
   ```

## 📚 関連リンク

- [Zenn記事: C#でMCPサーバーを作ってみよう](../../contents/articles/csharp-mcp-server.md)
- [Model Context Protocol公式ドキュメント](https://spec.modelcontextprotocol.io/)
- [Microsoft公式: C#でMCPサーバーを構築する](https://learn.microsoft.com/ja-jp/dotnet/ai/quickstarts/build-mcp-server)

## 📄 ライセンス

このサンプルプロジェクトはMITライセンスの下で公開されています。教育目的での使用を想定しています。

## 🤝 コントリビューション

問題やご提案がありましたら、[Issues](https://github.com/pixcelo/zenn-content/issues)でお知らせください。