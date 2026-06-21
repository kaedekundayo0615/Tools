using System.IO;
using PgDbOperator.Domain.Enums;
using PgDbOperator.Domain.Models;
using PgDbOperator.Infrastructure;

namespace PgDbOperator.Services;

/// <summary>
/// PostgreSQLコマンド生成サービス。
/// pg_dump、pg_restore、psqlの実行要求を生成します。
/// </summary>
public sealed class PostgresCommandBuilder
{
    /// <summary>
    /// バックアップコマンド生成処理。
    /// pg_dump用のプロセス実行要求を生成します。
    /// </summary>
    /// <param name="client">PostgreSQLクライアント設定。</param>
    /// <param name="connection">DB接続設定。</param>
    /// <param name="password">DBパスワード。</param>
    /// <param name="outputFilePath">出力Dumpファイルパス。</param>
    /// <param name="format">Dump形式。</param>
    /// <param name="noOwner">所有者情報を除外する場合はtrue。</param>
    /// <param name="noPrivileges">権限情報を除外する場合はtrue。</param>
    /// <param name="schemaOnly">スキーマのみ出力する場合はtrue。</param>
    /// <param name="dataOnly">データのみ出力する場合はtrue。</param>
    /// <returns>プロセス実行要求。</returns>
    public ProcessExecutionRequest BuildBackupRequest(PostgresClientProfile client, DbConnectionProfile connection, string password, string outputFilePath, DumpFormat format, bool noOwner, bool noPrivileges, bool schemaOnly, bool dataOnly)
    {
        var args = new List<string>
        {
            "--host", connection.Host,
            "--port", connection.Port.ToString(),
            "--username", connection.UserName,
            "--dbname", connection.DatabaseName,
            "--file", outputFilePath,
            "--format", ConvertDumpFormat(format)
        };

        if (noOwner) args.Add("--no-owner");
        if (noPrivileges) args.Add("--no-privileges");
        if (schemaOnly) args.Add("--schema-only");
        if (dataOnly) args.Add("--data-only");

        return CreateRequest(client.PgDumpPath, args, password, Path.GetDirectoryName(outputFilePath) ?? string.Empty);
    }

    /// <summary>
    /// リストアコマンド生成処理。
    /// Dump形式に応じてpg_restoreまたはpsqlの実行要求を生成します。
    /// </summary>
    /// <param name="client">PostgreSQLクライアント設定。</param>
    /// <param name="connection">DB接続設定。</param>
    /// <param name="password">DBパスワード。</param>
    /// <param name="inputFilePath">入力ファイルパス。</param>
    /// <param name="clean">既存オブジェクトを削除する場合はtrue。</param>
    /// <param name="ifExists">存在時のみ削除する場合はtrue。</param>
    /// <param name="noOwner">所有者情報を復元しない場合はtrue。</param>
    /// <param name="noPrivileges">権限情報を復元しない場合はtrue。</param>
    /// <returns>プロセス実行要求。</returns>
    public ProcessExecutionRequest BuildRestoreRequest(PostgresClientProfile client, DbConnectionProfile connection, string password, string inputFilePath, bool clean, bool ifExists, bool noOwner, bool noPrivileges)
    {
        if (IsPlainSql(inputFilePath))
        {
            return BuildPsqlFileRequest(client, connection, password, inputFilePath, true);
        }

        var args = new List<string>
        {
            "--host", connection.Host,
            "--port", connection.Port.ToString(),
            "--username", connection.UserName,
            "--dbname", connection.DatabaseName
        };

        if (clean) args.Add("--clean");
        if (ifExists) args.Add("--if-exists");
        if (noOwner) args.Add("--no-owner");
        if (noPrivileges) args.Add("--no-privileges");
        args.Add(inputFilePath);

        return CreateRequest(client.PgRestorePath, args, password, Path.GetDirectoryName(inputFilePath) ?? string.Empty);
    }

    /// <summary>
    /// SQLファイル実行コマンド生成処理。
    /// psqlでSQLファイルを実行するプロセス実行要求を生成します。
    /// </summary>
    /// <param name="client">PostgreSQLクライアント設定。</param>
    /// <param name="connection">DB接続設定。</param>
    /// <param name="password">DBパスワード。</param>
    /// <param name="sqlFilePath">SQLファイルパス。</param>
    /// <param name="stopOnError">SQLエラー時に停止する場合はtrue。</param>
    /// <returns>プロセス実行要求。</returns>
    public ProcessExecutionRequest BuildPsqlFileRequest(PostgresClientProfile client, DbConnectionProfile connection, string password, string sqlFilePath, bool stopOnError)
    {
        var args = new List<string>
        {
            "--host", connection.Host,
            "--port", connection.Port.ToString(),
            "--username", connection.UserName,
            "--dbname", connection.DatabaseName
        };

        if (stopOnError)
        {
            args.Add("--set");
            args.Add("ON_ERROR_STOP=1");
        }

        args.Add("--file");
        args.Add(sqlFilePath);

        return CreateRequest(client.PsqlPath, args, password, Path.GetDirectoryName(sqlFilePath) ?? string.Empty);
    }

    /// <summary>
    /// 表示用コマンド生成処理。
    /// パスワードを含まない安全な表示用コマンド文字列を生成します。
    /// </summary>
    /// <param name="request">プロセス実行要求。</param>
    /// <returns>表示用コマンド文字列。</returns>
    public string BuildDisplayCommand(ProcessExecutionRequest request)
    {
        return $"\"{request.FileName}\" {string.Join(" ", request.Arguments.Select(EscapeArgument))}";
    }

    /// <summary>
    /// Dump形式変換処理。
    /// アプリ内部のDump形式をpg_dump引数へ変換します。
    /// </summary>
    /// <param name="format">Dump形式。</param>
    /// <returns>pg_dump形式文字列。</returns>
    private static string ConvertDumpFormat(DumpFormat format)
    {
        return format switch
        {
            DumpFormat.Custom => "custom",
            DumpFormat.Plain => "plain",
            DumpFormat.Tar => "tar",
            DumpFormat.Directory => "directory",
            _ => "custom"
        };
    }

    /// <summary>
    /// plain SQL判定処理。
    /// 拡張子からpsqlで実行すべきファイルか判定します。
    /// </summary>
    /// <param name="filePath">判定対象ファイルパス。</param>
    /// <returns>plain SQLの場合はtrue。</returns>
    private static bool IsPlainSql(string filePath) => string.Equals(Path.GetExtension(filePath), ".sql", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// プロセス実行要求作成処理。
    /// 実行ファイル、引数、PGPASSWORD環境変数を設定します。
    /// </summary>
    /// <param name="fileName">実行ファイルパス。</param>
    /// <param name="args">引数一覧。</param>
    /// <param name="password">DBパスワード。</param>
    /// <param name="workingDirectory">作業ディレクトリ。</param>
    /// <returns>プロセス実行要求。</returns>
    private static ProcessExecutionRequest CreateRequest(string fileName, List<string> args, string password, string workingDirectory)
    {
        var env = new Dictionary<string, string>();
        if (!string.IsNullOrEmpty(password))
        {
            env["PGPASSWORD"] = password;
        }

        return new ProcessExecutionRequest
        {
            FileName = fileName,
            Arguments = args,
            EnvironmentVariables = env,
            WorkingDirectory = workingDirectory
        };
    }

    /// <summary>
    /// 引数エスケープ処理。
    /// 表示用コマンドで空白を含む引数を引用符で囲みます。
    /// </summary>
    /// <param name="argument">引数。</param>
    /// <returns>表示用引数。</returns>
    private static string EscapeArgument(string argument)
    {
        return argument.Contains(' ') ? $"\"{argument}\"" : argument;
    }
}
