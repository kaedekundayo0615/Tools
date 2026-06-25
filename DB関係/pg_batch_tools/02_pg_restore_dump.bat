@echo off
chcp 65001 > nul
setlocal EnableExtensions

rem ================================================================
rem PostgreSQL restore batch
rem Usage:
rem   02_pg_restore_dump.bat dump_file [database_name]
rem Notes:
rem   .sql files are restored by psql.
rem   Other dump files are restored by pg_restore.
rem ================================================================

set "SCRIPT_DIR=%~dp0"
set "CONFIG_FILE=%SCRIPT_DIR%pg_common_config.bat"

if not exist "%CONFIG_FILE%" (
  echo [ERROR] Config file not found: %CONFIG_FILE%
  exit /b 1
)
call "%CONFIG_FILE%"

set "DUMP_FILE=%~1"
set "TARGET_DB=%~2"
if not defined TARGET_DB set "TARGET_DB=%DB_NAME%"

if not defined DUMP_FILE (
  echo [ERROR] Dump file is required.
  echo Usage: %~nx0 dump_file [database_name]
  exit /b 1
)

if not defined TARGET_DB (
  echo [ERROR] Database name is empty. Set DB_NAME in pg_common_config.bat or pass it as the second argument.
  exit /b 1
)

if not exist "%DUMP_FILE%" (
  echo [ERROR] Dump file was not found: %DUMP_FILE%
  exit /b 1
)

if not exist "%LOG_DIR%" mkdir "%LOG_DIR%" > nul 2>&1

if defined PG_BIN_DIR (
  set "PG_RESTORE_EXE=%PG_BIN_DIR%\pg_restore.exe"
  set "PSQL_EXE=%PG_BIN_DIR%\psql.exe"
) else (
  set "PG_RESTORE_EXE=pg_restore"
  set "PSQL_EXE=psql"
)

for %%F in ("%DUMP_FILE%") do set "EXT=%%~xF"

if /i "%EXT%"==".sql" (
  if not exist "%PSQL_EXE%" (
    where psql > nul 2>&1
    if errorlevel 1 (
      echo [ERROR] psql.exe was not found. Check PG_BIN_DIR or PATH.
      exit /b 1
    )
    set "PSQL_EXE=psql"
  )
) else (
  if not exist "%PG_RESTORE_EXE%" (
    where pg_restore > nul 2>&1
    if errorlevel 1 (
      echo [ERROR] pg_restore.exe was not found. Check PG_BIN_DIR or PATH.
      exit /b 1
    )
    set "PG_RESTORE_EXE=pg_restore"
  )
)

for /f %%I in ('powershell -NoProfile -ExecutionPolicy Bypass -Command "Get-Date -Format yyyyMMdd_HHmmss"') do set "NOW=%%I"
if not defined NOW set "NOW=unknown_time"

set "LOG_FILE=%LOG_DIR%\pg_restore_%TARGET_DB%_%NOW%.log"

if defined PG_PASSWORD set "PGPASSWORD=%PG_PASSWORD%"

echo [INFO] Restore started.
echo        database : %TARGET_DB%
echo        host     : %PG_HOST%:%PG_PORT%
echo        user     : %PG_USER%
echo        input    : %DUMP_FILE%
echo        log      : %LOG_FILE%

if /i "%EXT%"==".sql" (
  "%PSQL_EXE%" ^
    -h "%PG_HOST%" ^
    -p "%PG_PORT%" ^
    -U "%PG_USER%" ^
    -d "%TARGET_DB%" ^
    %PSQL_OPTIONS% ^
    -f "%DUMP_FILE%" > "%LOG_FILE%" 2>&1
) else (
  "%PG_RESTORE_EXE%" ^
    -h "%PG_HOST%" ^
    -p "%PG_PORT%" ^
    -U "%PG_USER%" ^
    -d "%TARGET_DB%" ^
    -v ^
    %PG_RESTORE_OPTIONS% ^
    "%DUMP_FILE%" > "%LOG_FILE%" 2>&1
)

if errorlevel 1 (
  echo [ERROR] Restore failed. See log: %LOG_FILE%
  exit /b 1
)

echo [INFO] Restore completed successfully.
exit /b 0
