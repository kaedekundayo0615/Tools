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
- Visual Studio 2022
- .NET 8 SDK
- PostgreSQL Client Tools

## 起動方法

```powershell
cd PgDbOperator
dotnet restore
dotnet build .\PgDbOperator.sln
```

WPF アプリのため、Windows 環境でビルドしてください。

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
