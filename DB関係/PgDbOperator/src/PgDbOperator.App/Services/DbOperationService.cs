using System.IO;
using PgDbOperator.Domain.Enums;
using PgDbOperator.Domain.Models;
using PgDbOperator.Infrastructure;

namespace PgDbOperator.Services;

/// <summary>
/// DB操作サービス。
/// バックアップ、リストア、SQL実行、データ入れ替えのユースケースを実行します。
/// </summary>
public sealed class DbOperationService
{
    private readonly ProcessExecutor processExecutor;
    private readonly PostgresCommandBuilder commandBuilder;
    private readonly ExecutionHistoryService historyService;
    private readonly SafetyCheckService safetyCheckService;
    private readonly DpapiPasswordProtector passwordProtector;

    /// <summary>
    /// DB操作サービス初期化処理。
    /// 外部プロセス実行、コマンド生成、履歴保存等の依存サービスを保持します。
    /// </summary>
    public DbOperationService(ProcessExecutor processExecutor, PostgresCommandBuilder commandBuilder, ExecutionHistoryService historyService, SafetyCheckService safetyCheckService, DpapiPasswordProtector passwordProtector)
    {
        this.processExecutor = processExecutor;
        this.commandBuilder = commandBuilder;
        this.historyService = historyService;
        this.safetyCheckService = safetyCheckService;
        this.passwordProtector = passwordProtector;
    }

    /// <summary>
    /// バックアップ実行処理。
    /// pg_dumpを実行してDumpファイルを生成し、実行履歴を保存します。
    /// </summary>
    public async Task<ExecutionHistory> BackupAsync(ManagedApplication application, DbConnectionProfile connection, PostgresClientProfile client, string password, string outputFilePath, DumpFormat format, bool noOwner, bool noPrivileges, bool schemaOnly, bool dataOnly)
    {
        ValidateRequiredFile(client.PgDumpPath, "pg_dump.exe");
        Directory.CreateDirectory(Path.GetDirectoryName(outputFilePath) ?? throw new InvalidOperationException("出力先フォルダが不正です。"));
        var resolvedPassword = ResolvePassword(connection, password);
        var request = commandBuilder.BuildBackupRequest(client, connection, resolvedPassword, outputFilePath, format, noOwner, noPrivileges, schemaOnly, dataOnly);
        return await ExecuteAndSaveHistoryAsync(application, connection, OperationType.Backup, outputFilePath, request, "バックアップを実行しました。");
    }

    /// <summary>
    /// リストア実行処理。
    /// DumpまたはSQLファイルからDBを復元し、実行履歴を保存します。
    /// </summary>
    public async Task<ExecutionHistory> RestoreAsync(ManagedApplication application, DbConnectionProfile connection, PostgresClientProfile client, string password, string inputFilePath, bool clean, bool ifExists, bool noOwner, bool noPrivileges)
    {
        if (!File.Exists(inputFilePath)) throw new FileNotFoundException("リストア対象ファイルが見つかりません。", inputFilePath);
        var exePath = Path.GetExtension(inputFilePath).Equals(".sql", StringComparison.OrdinalIgnoreCase) ? client.PsqlPath : client.PgRestorePath;
        ValidateRequiredFile(exePath, Path.GetFileName(exePath));
        var resolvedPassword = ResolvePassword(connection, password);
        var request = commandBuilder.BuildRestoreRequest(client, connection, resolvedPassword, inputFilePath, clean, ifExists, noOwner, noPrivileges);
        return await ExecuteAndSaveHistoryAsync(application, connection, OperationType.Restore, inputFilePath, request, "リストアを実行しました。");
    }

    /// <summary>
    /// SQLファイル群実行処理。
    /// 指定されたSQLファイルを順番にpsqlで実行します。
    /// </summary>
    public async Task<List<ExecutionHistory>> ExecuteSqlFilesAsync(ManagedApplication application, DbConnectionProfile connection, PostgresClientProfile client, string password, IReadOnlyList<string> sqlFilePaths, SqlFailureBehavior failureBehavior)
    {
        ValidateRequiredFile(client.PsqlPath, "psql.exe");
        var histories = new List<ExecutionHistory>();
        var resolvedPassword = ResolvePassword(connection, password);

        foreach (var sqlFilePath in sqlFilePaths)
        {
            if (!File.Exists(sqlFilePath)) throw new FileNotFoundException("SQLファイルが見つかりません。", sqlFilePath);
            var request = commandBuilder.BuildPsqlFileRequest(client, connection, resolvedPassword, sqlFilePath, true);
            var history = await ExecuteAndSaveHistoryAsync(application, connection, OperationType.SqlExecution, sqlFilePath, request, "SQLファイルを実行しました。");
            histories.Add(history);

            if (history.Result == ExecutionResult.Failed && failureBehavior == SqlFailureBehavior.StopImmediately)
            {
                break;
            }
        }

        return histories;
    }

    /// <summary>
    /// データ入れ替え実行処理。
    /// 移行先事前バックアップ、移行元Dump、移行先Restore、後処理SQLを順番に実行します。
    /// </summary>
    public async Task<List<ExecutionHistory>> ReplaceDataAsync(ManagedApplication application, DbConnectionProfile sourceConnection, DbConnectionProfile targetConnection, PostgresClientProfile client, string sourcePassword, string targetPassword, string workDirectory, bool backupTargetBeforeRestore, IReadOnlyList<string> afterRestoreSqlFiles)
    {
        if (safetyCheckService.IsSameDatabase(sourceConnection, targetConnection))
        {
            throw new InvalidOperationException("移行元DBと移行先DBが同一のため、データ入れ替えは実行できません。");
        }

        Directory.CreateDirectory(workDirectory);
        var histories = new List<ExecutionHistory>();
        var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");

        if (backupTargetBeforeRestore)
        {
            var targetBackup = Path.Combine(workDirectory, $"before_restore_{targetConnection.DatabaseName}_{timestamp}.dump");
            histories.Add(await BackupAsync(application, targetConnection, client, targetPassword, targetBackup, DumpFormat.Custom, true, true, false, false));
        }

        var sourceDump = Path.Combine(workDirectory, $"source_{sourceConnection.DatabaseName}_{timestamp}.dump");
        histories.Add(await BackupAsync(application, sourceConnection, client, sourcePassword, sourceDump, DumpFormat.Custom, true, true, false, false));
        histories.Add(await RestoreAsync(application, targetConnection, client, targetPassword, sourceDump, true, true, true, true));

        if (afterRestoreSqlFiles.Count > 0)
        {
            histories.AddRange(await ExecuteSqlFilesAsync(application, targetConnection, client, targetPassword, afterRestoreSqlFiles, SqlFailureBehavior.StopImmediately));
        }

        return histories;
    }

    /// <summary>
    /// コマンド実行履歴保存処理。
    /// 外部プロセスを実行し、結果を実行履歴として保存します。
    /// </summary>
    private async Task<ExecutionHistory> ExecuteAndSaveHistoryAsync(ManagedApplication application, DbConnectionProfile connection, OperationType operationType, string targetFilePath, ProcessExecutionRequest request, string successMessage)
    {
        var startedAt = DateTime.Now;
        var result = await processExecutor.ExecuteAsync(request);
        var history = new ExecutionHistory
        {
            StartedAt = startedAt,
            EndedAt = DateTime.Now,
            OperationType = operationType,
            Result = result.ExitCode == 0 ? ExecutionResult.Success : ExecutionResult.Failed,
            ApplicationName = application.Name,
            ConnectionName = connection.Name,
            Host = connection.Host,
            DatabaseName = connection.DatabaseName,
            UserName = connection.UserName,
            TargetFilePath = targetFilePath,
            CommandText = commandBuilder.BuildDisplayCommand(request),
            StandardOutput = result.StandardOutput,
            StandardError = result.StandardError,
            ExitCode = result.ExitCode,
            Message = result.ExitCode == 0 ? successMessage : "処理に失敗しました。標準エラーを確認してください。"
        };
        historyService.Add(history);
        return history;
    }

    /// <summary>
    /// パスワード解決処理。
    /// 画面入力パスワード、暗号化保存パスワードの順で利用する値を決定します。
    /// </summary>
    private string ResolvePassword(DbConnectionProfile connection, string plainPassword)
    {
        if (!string.IsNullOrEmpty(plainPassword)) return plainPassword;
        if (connection.PasswordSaveType == PasswordSaveType.WindowsDpapi && !string.IsNullOrWhiteSpace(connection.EncryptedPassword))
        {
            return passwordProtector.Unprotect(connection.EncryptedPassword);
        }
        return string.Empty;
    }

    /// <summary>
    /// 実行ファイル存在チェック処理。
    /// 指定されたPostgreSQLクライアントEXEが存在するか検証します。
    /// </summary>
    private static void ValidateRequiredFile(string filePath, string displayName)
    {
        if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
        {
            throw new FileNotFoundException($"{displayName} が見つかりません。PostgreSQL設定を確認してください。", filePath);
        }
    }
}
