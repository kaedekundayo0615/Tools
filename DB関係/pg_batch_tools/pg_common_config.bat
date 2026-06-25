@echo off
rem ================================================================
rem PostgreSQL batch common configuration
rem Copy this folder anywhere and edit only this file for each PC/server.
rem ================================================================

rem PostgreSQL bin directory. If empty, pg_dump/pg_restore/psql are searched from PATH.
set "PG_BIN_DIR=C:\Program Files\PostgreSQL\16\bin"

rem Connection settings.
set "PG_HOST=localhost"
set "PG_PORT=5432"
set "PG_USER=postgres"

rem Optional. Leave empty to let PostgreSQL prompt for the password.
rem Escape percent signs as %% in a batch file.
set "PG_PASSWORD="

rem Default target database.
set "DB_NAME=postgres"

rem Default folders. These paths are relative to this config file.
set "DUMP_DIR=%~dp0backup"
set "SQL_DIR=%~dp0sql"
set "LOG_DIR=%~dp0logs"

rem Dump format: custom or plain
rem custom: recommended for restore with pg_restore. Extension is .backup.
rem plain : SQL text dump. Extension is .sql.
set "DUMP_FORMAT=custom"

rem Extra command options.
set "PG_DUMP_OPTIONS=--no-owner --no-privileges"
set "PG_RESTORE_OPTIONS=--clean --if-exists --no-owner --no-privileges"
set "PSQL_OPTIONS=-v ON_ERROR_STOP=1"
