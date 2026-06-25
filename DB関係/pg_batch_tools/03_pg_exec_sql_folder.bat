@echo off
chcp 65001 > nul
setlocal EnableExtensions

rem ================================================================
rem PostgreSQL SQL-folder execution batch
rem Usage:
rem   03_pg_exec_sql_folder.bat [sql_folder] [database_name]
rem Notes:
rem   Executes only *.sql files directly under the specified folder.
rem   File execution order is ascending by file name.
rem   Stops immediately when one SQL file fails.
rem ================================================================

set "SCRIPT_DIR=%~dp0"
set "CONFIG_FILE=%SCRIPT_DIR%pg_common_config.bat"

if not exist "%CONFIG_FILE%" (
  echo [ERROR] Config file not found: %CONFIG_FILE%
  exit /b 1
)
call "%CONFIG_FILE%"

set "TARGET_SQL_DIR=%~1"
if not defined TARGET_SQL_DIR set "TARGET_SQL_DIR=%SQL_DIR%"

set "TARGET_DB=%~2"
if not defined TARGET_DB set "TARGET_DB=%DB_NAME%"

if not defined TARGET_DB (
  echo [ERROR] Database name is empty. Set DB_NAME in pg_common_config.bat or pass it as the second argument.
  exit /b 1
)

if not exist "%TARGET_SQL_DIR%" (
  echo [ERROR] SQL folder was not found: %TARGET_SQL_DIR%
  exit /b 1
)

if not exist "%LOG_DIR%" mkdir "%LOG_DIR%" > nul 2>&1

if defined PG_BIN_DIR (
  set "PSQL_EXE=%PG_BIN_DIR%\psql.exe"
) else (
  set "PSQL_EXE=psql"
)

if not exist "%PSQL_EXE%" (
  where psql > nul 2>&1
  if errorlevel 1 (
    echo [ERROR] psql.exe was not found. Check PG_BIN_DIR or PATH.
    exit /b 1
  )
  set "PSQL_EXE=psql"
)

for /f %%I in ('powershell -NoProfile -ExecutionPolicy Bypass -Command "Get-Date -Format yyyyMMdd_HHmmss"') do set "NOW=%%I"
if not defined NOW set "NOW=unknown_time"

set "LOG_FILE=%LOG_DIR%\psql_folder_%TARGET_DB%_%NOW%.log"
set "LIST_FILE=%TEMP%\pg_sql_list_%RANDOM%_%NOW%.txt"

if defined PG_PASSWORD set "PGPASSWORD=%PG_PASSWORD%"

pushd "%TARGET_SQL_DIR%" > nul 2>&1
if errorlevel 1 (
  echo [ERROR] Failed to open SQL folder: %TARGET_SQL_DIR%
  exit /b 1
)

dir /b /a-d /on "*.sql" > "%LIST_FILE%" 2> nul
popd > nul 2>&1

for %%A in ("%LIST_FILE%") do if %%~zA==0 (
  del "%LIST_FILE%" > nul 2>&1
  echo [WARN] No .sql files found in: %TARGET_SQL_DIR%
  exit /b 0
)

echo [INFO] SQL folder execution started.
echo        database : %TARGET_DB%
echo        host     : %PG_HOST%:%PG_PORT%
echo        user     : %PG_USER%
echo        folder   : %TARGET_SQL_DIR%
echo        order    : file name ascending
echo        log      : %LOG_FILE%
echo. > "%LOG_FILE%"
echo [INFO] SQL folder execution started. >> "%LOG_FILE%"
echo [INFO] Folder: %TARGET_SQL_DIR% >> "%LOG_FILE%"
echo [INFO] Database: %TARGET_DB% >> "%LOG_FILE%"

pushd "%TARGET_SQL_DIR%" > nul 2>&1
for /f "usebackq delims=" %%F in ("%LIST_FILE%") do (
  echo [INFO] Executing: %%F
  echo. >> "%LOG_FILE%"
  echo ================================================================ >> "%LOG_FILE%"
  echo [INFO] Executing: %%F >> "%LOG_FILE%"
  echo ================================================================ >> "%LOG_FILE%"

  "%PSQL_EXE%" ^
    -h "%PG_HOST%" ^
    -p "%PG_PORT%" ^
    -U "%PG_USER%" ^
    -d "%TARGET_DB%" ^
    %PSQL_OPTIONS% ^
    -f "%%~fF" >> "%LOG_FILE%" 2>&1

  if errorlevel 1 (
    popd > nul 2>&1
    del "%LIST_FILE%" > nul 2>&1
    echo [ERROR] SQL execution failed: %%F
    echo         See log: %LOG_FILE%
    exit /b 1
  )
)
popd > nul 2>&1

del "%LIST_FILE%" > nul 2>&1

echo [INFO] SQL folder execution completed successfully.
echo        log: %LOG_FILE%
exit /b 0
