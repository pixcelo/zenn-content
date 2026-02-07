# Blazor Radzen Playwright Sample

Blazor と Radzen コンポーネントに対する Playwright E2E テストのサンプルプロジェクトです。

## プロジェクト構成

```
blazor-radzen-playwright-sample/
├── BlazorRadzenPlaywrightSample.sln          # Visual Studio 2022 ソリューション
├── BlazorRadzenPlaywrightSample/             # Blazor アプリ本体 (.NET 8)
│   ├── BlazorRadzenPlaywrightSample.csproj
│   ├── Program.cs
│   ├── Components/
│   │   ├── App.razor
│   │   ├── Routes.razor
│   │   └── Pages/
│   │       └── Home.razor
│   └── wwwroot/
├── e2e/                                       # Playwright E2E テスト
│   ├── package.json
│   ├── playwright.config.ts
│   ├── .env.example
│   ├── .gitignore
│   ├── tests/
│   │   └── example.spec.ts
│   └── README.md
├── build.sh                                   # Linux ビルドスクリプト
├── build.ps1                                  # Windows ビルドスクリプト
├── run.sh                                     # Linux 実行スクリプト
├── run.ps1                                    # Windows 実行スクリプト
└── README.md                                  # このファイル
```

## 技術スタック

### Blazor 側
- **.NET 8** (net8.0)
- **Blazor Web App**
- **Radzen.Blazor 8.7.5**

### E2E テスト側
- **@playwright/test** (^1.57.0)
- **TypeScript**
- **dotenv** (^16.3.1)

## セットアップ

### 前提条件
- .NET 8 SDK
- Node.js (v18 以上推奨)
- Visual Studio 2022 (オプション)

### 1. Blazor アプリケーションのビルド

#### Windows (PowerShell)
```powershell
.\build.ps1
```

#### Linux / macOS
```bash
chmod +x build.sh
./build.sh
```

#### 手動ビルド
```bash
dotnet build BlazorRadzenPlaywrightSample.sln
```

### 2. E2E テストのセットアップ

```bash
cd e2e
npm install
npx playwright install
```

詳細は [e2e/README.md](e2e/README.md) を参照してください。

## 実行方法

### Blazor アプリケーションの起動

#### Windows (PowerShell)
```powershell
.\run.ps1
```

#### Linux / macOS
```bash
chmod +x run.sh
./run.sh
```

#### 手動実行
```bash
cd BlazorRadzenPlaywrightSample
dotnet run
```

アプリケーションは以下の URL で起動します:
- HTTP: http://localhost:5000
- HTTPS: https://localhost:5001

### E2E テストの実行

別のターミナルで以下を実行:

```bash
cd e2e
npm test
```

詳細なテストコマンドは [e2e/README.md](e2e/README.md) を参照してください。

## Visual Studio 2022 での開発

1. `BlazorRadzenPlaywrightSample.sln` を Visual Studio 2022 で開く
2. F5 キーでデバッグ実行
3. アプリケーションがブラウザで開きます

## 次のステップ

このプロジェクトは初期状態です。以下のような拡張が可能です:

1. **Radzen コンポーネントの追加**
   - RadzenButton, RadzenDataGrid, RadzenDialog などを追加
   - Components/Pages/ にサンプルページを作成

2. **Playwright テストの拡張**
   - 各 Radzen コンポーネントのテストを作成
   - インタラクティブな操作のテスト
   - アクセシビリティテスト

3. **CI/CD の統合**
   - GitHub Actions での自動テスト実行
   - テスト結果のレポート生成

## ライセンス

MIT License

## 参考リンク

- [Blazor 公式ドキュメント](https://learn.microsoft.com/ja-jp/aspnet/core/blazor/)
- [Radzen Blazor コンポーネント](https://blazor.radzen.com/)
- [Playwright 公式ドキュメント](https://playwright.dev/)
