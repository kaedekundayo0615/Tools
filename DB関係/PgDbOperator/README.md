# PgDbOperator

PostgreSQL を利用する複数アプリケーション向けの DB 運用支援デスクトップアプリです。

## 主な機能

- 対象アプリ管理
- DB接続設定管理
- PostgreSQLクライアントEXE設定
- バックアップ（pg_dump）
- リストア（pg_restore / psql）
- SQLファイル・SQLフォルダ実行（psql）
- データ入れ替え（移行先バックアップ → 移行元Dump → 移行先Restore → 後処理SQL）
- 実行履歴保存
- 危険操作警告

## 開発環境

- Windows 10/11
- Visual Studio Code
- Visual Studio 2026 / Visual Studio 2022 以降
- .NET 8 SDK
- PostgreSQL Client Tools

## 起動方法

### Visual Studio Code

1. `PgDbOperator` フォルダを Visual Studio Code で開きます。
2. 拡張機能の推奨が表示された場合は、C# / C# Dev Kit をインストールします。
3. `F5` を押下し、`PgDbOperator.App - Debug` を選択します。

デバッグ実行時は `.vscode/tasks.json` の `build PgDbOperator.App` が先に実行され、Debug 構成でビルドした EXE を起動します。

手動で確認する場合は以下を実行してください。

```powershell
cd PgDbOperator
dotnet restore .\PgDbOperator.sln
dotnet build .\src\PgDbOperator.App\PgDbOperator.App.csproj --configuration Debug
dotnet run --project .\src\PgDbOperator.App\PgDbOperator.App.csproj --configuration Debug
```

### Visual Studio 2026 / Visual Studio 2022 以降

1. `PgDbOperator.sln` を開きます。
2. スタートアッププロジェクトが `PgDbOperator.App` になっていることを確認します。
3. `Debug` / `Any CPU` を選択して `F5` で起動します。

`src/PgDbOperator.App/Properties/launchSettings.json` に開発用プロファイルを追加しているため、Visual Studio 側でも `DOTNET_ENVIRONMENT=Development` の状態でデバッグできます。

WPF アプリのため、Windows 環境でビルド・デバッグしてください。

## 注意

この環境では .NET SDK/WPF ビルド環境がないため、ビルド検証は未実施です。  
ソース構成、責務分離、コマンド生成、ログ保存、画面構成を仕様書ベースで実装しています。

## 設定保存先

既定では以下に JSON を保存します。

```text
%APPDATA%\PgDbOperator\settings.json
%APPDATA%\PgDbOperator\history.json
%APPDATA%\PgDbOperator\logs\
```
