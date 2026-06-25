# PostgreSQL 汎用バッチ一式

## 構成

| ファイル | 内容 |
|---|---|
| `pg_common_config.bat` | 接続先、PostgreSQL の bin パス、既定DB、既定フォルダ、共通オプションの設定ファイル |
| `01_pg_dump_backup.bat` | PostgreSQL の Dump を取得するバッチ |
| `02_pg_restore_dump.bat` | Dump / SQL ファイルを指定DBへリストアするバッチ |
| `03_pg_exec_sql_folder.bat` | 指定フォルダ直下の `.sql` をファイル名昇順で全実行するバッチ |
| `backup/` | Dump 出力先の既定フォルダ |
| `sql/` | SQL 実行対象の既定フォルダ |
| `logs/` | 実行ログ出力先 |

## 事前設定

`pg_common_config.bat` を開き、最低限以下を変更してください。

```bat
set "PG_BIN_DIR=C:\Program Files\PostgreSQLin"
set "PG_HOST=localhost"
set "PG_PORT=5432"
set "PG_USER=postgres"
set "PG_PASSWORD="
set "DB_NAME=postgres"
```

`PG_PASSWORD` は空欄のままでも動きます。その場合、PostgreSQL 側の設定によりパスワード入力を求められることがあります。バッチ内にパスワードを直接書く場合、ファイル権限に注意してください。

## Dump取得

既定DBを既定フォルダへ custom 形式でバックアップします。

```bat
01_pg_dump_backup.bat
```

DB名、出力先、形式を指定する場合です。

```bat
01_pg_dump_backup.bat sample_db C:ackup custom
01_pg_dump_backup.bat sample_db C:ackup plain
```

- `custom` は `pg_restore` 用の形式です。拡張子は `.backup` になります。
- `plain` は SQL テキスト形式です。拡張子は `.sql` になります。

## リストア

custom形式などのDumpを指定DBへリストアします。

```bat
02_pg_restore_dump.bat C:ackup\sample_db_20260625_120000.backup sample_db
```

SQLファイルを指定した場合は `psql -f` で実行します。

```bat
02_pg_restore_dump.bat C:ackup\sample_db_20260625_120000.sql sample_db
```

既定では `--clean --if-exists --no-owner --no-privileges` を付けています。対象DB内の既存オブジェクトを削除しながら戻すため、本番DBで実行する場合は必ず事前確認してください。必要に応じて `pg_common_config.bat` の `PG_RESTORE_OPTIONS` を変更してください。

## 指定フォルダ内SQLの名称順実行

既定の `sql/` フォルダ直下にある `.sql` をファイル名昇順で実行します。

```bat
03_pg_exec_sql_folder.bat
```

フォルダとDB名を指定する場合です。

```bat
03_pg_exec_sql_folder.bat C:\work\sql sample_db
```

例として以下のようなファイル名にすると実行順を制御しやすいです。

```text
001_create_table.sql
002_insert_master.sql
003_update_data.sql
```

1ファイルでもエラーになった場合、以降のSQLは実行せず停止します。`psql` には既定で `-v ON_ERROR_STOP=1` を付けています。

## 注意点

- Windows 用の `.bat` です。
- PostgreSQL クライアントツール `pg_dump.exe` / `pg_restore.exe` / `psql.exe` が必要です。
- `PG_BIN_DIR` が空の場合は PATH から各コマンドを探します。
- フォルダSQL実行は「指定フォルダ直下」の `.sql` のみ対象です。サブフォルダ配下は対象外です。
- ログは `logs/` に出力されます。
