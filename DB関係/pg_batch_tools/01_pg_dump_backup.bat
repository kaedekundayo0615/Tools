@echo off
chcp 65001 > nul
setlocal EnableExtensions

rem ================================================================
rem PostgreSQL dump backup batch
rem Usage:
rem   01_pg_dump_backup.bat [database_name] [output_dir] [custom|plain]
rem ================================================================

set "SCRIPT_DIR=%~dp0"
set "CONFIG_FILE=%SCRIPT_DIR%pg_common_config.bat"

if not exist "%CONFIG_FILE%" (
  echo [ERROR] Config file not found: %CONFIG_FILE%
  exit /b 1
)
call "%CONFIG_FILE%"

set "TARGET_DB=%~1"
if not defined TARGET_DB set "TARGET_DB=%DB_NAME%"

set "OUTPUT_DIR=%~2"
if not defined OUTPUT_DIR set "OUTPUT_DIR=%DUMP_DIR%"

set "BACKUP_FORMAT=%~3"
if not defined BACKUP_FORMAT set "BACKUP_FORMAT=%DUMP_FORMAT%"
if /i "%BACKUP_FORMAT%"=="c" set "BACKUP_FORMAT=custom"
if /i "%BACKUP_FORMAT%"=="p" set "BACKUP_FORMAT=plain"

if /i not "%BACKUP_FORMAT%"=="custom" if /i not "%BACKUP_FORMAT%"=="plain" (
  echo [ERROR] Invalid dump format: %BACKUP_FORMAT%
  echo         Use custom or plain.
  exit /b 1
)

if not defined TARGET_DB (
  echo [ERROR] Database name is empty. Set DB_NAME in pg_common_config.bat or pass it as the first argument.
  exit /b 1
)

if not exist "%OUTPUT_DIR%" mkdir "%OUTPUT_DIR%" > nul 2>&1
if errorlevel 1 (
  echo [ERROR] Failed to create output directory: %OUTPUT_DIR%
  exit /b 1
)

if not exist "%LOG_DIR%" mkdir "%LOG_DIR%" > nul 2>&1

if defined PG_BIN_DIR (
  set "PG_DUMP_EXE=%PG_BIN_DIR%\pg_dump.exe"
) else (
  set "PG_DUMP_EXE=pg_dump"
)

if not exist "%PG_DUMP_EXE%" (
  where pg_dump > nul 2>&1
  if errorlevel 1 (
    echo [ERROR] pg_dump.exe was not found. Check PG_BIN_DIR or PATH.
    exit /b 1
  )
  set "PG_DUMP_EXE=pg_dump"
)

for /f %%I in ('powershell -NoProfile -ExecutionPolicy Bypass -Command "Get-Date -Format yyyyMMdd_HHmmss"') do set "NOW=%%I"
if not defined NOW set "NOW=unknown_time"

if /i "%BACKUP_FORMAT%"=="plain" (
  set "FORMAT_OPTION=p"
  set "EXT=sql"
) else (
  set "FORMAT_OPTION=c"
  set "EXT=backup"
)

set "BACKUP_FILE=%OUTPUT_DIR%\%TARGET_DB%_%NOW%.%EXT%"
set "LOG_FILE=%LOG_DIR%\pg_dump_%TARGET_DB%_%NOW%.log"

if defined PG_PASSWORD set "PGPASSWORD=%PG_PASSWORD%"

 echo [INFO] Backup started.
 echo        database : %TARGET_DB%
 echo        host     : %PG_HOST%:%PG_PORT%
 echo        user     : %PG_USER%
 echo        format   : %BACKUP_FORMAT%
 echo        output   : %BACKUP_FILE%
 echo        log      : %LOG_FILE%

"%PG_DUMP_EXE%" ^
  -h "%PG_HOST%" ^
  -p "%PG_PORT%" ^
  -U "%PG_USER%" ^
  -d "%TARGET_DB%" ^
  -F "%FORMAT_OPTION%" ^
  -b ^
  -v ^
  -f "%BACKUP_FILE%" ^
  %PG_DUMP_OPTIONS% > "%LOG_FILE%" 2>&1

if errorlevel 1 (
  echo [ERROR] Backup failed. See log: %LOG_FILE%
  exit /b 1
)

echo [INFO] Backup completed successfully.
echo        file: %BACKUP_FILE%
exit /b 0
