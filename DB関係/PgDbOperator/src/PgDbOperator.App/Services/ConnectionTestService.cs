using Npgsql;
using PgDbOperator.Domain.Enums;
using PgDbOperator.Domain.Models;
using PgDbOperator.Infrastructure;

namespace PgDbOperator.Services;

/// <summary>
/// DB接続テストサービス。
/// DB接続設定に対してNpgsqlで疎通確認を行います。
/// </summary>
public sealed class ConnectionTestService
{
    private readonly DpapiPasswordProtector passwordProtector;

    /// <summary>
    /// 接続テストサービス初期化処理。
    /// パスワード復号サービスを保持します。
    /// </summary>
    /// <param name="passwordProtector">パスワード保護サービス。</param>
    public ConnectionTestService(DpapiPasswordProtector passwordProtector)
    {
        this.passwordProtector = passwordProtector;
    }

    /// <summary>
    /// 接続テスト実行処理。
    /// DBへ接続し、簡易SQLを実行して疎通確認します。
    /// </summary>
    /// <param name="connection">DB接続設定。</param>
    /// <param name="plainPassword">画面入力された平文パスワード。</param>
    /// <returns>接続結果メッセージ。</returns>
    public async Task<string> TestAsync(DbConnectionProfile connection, string plainPassword)
    {
        var password = ResolvePassword(connection, plainPassword);
        var builder = new NpgsqlConnectionStringBuilder
        {
            Host = connection.Host,
            Port = connection.Port,
            Database = connection.DatabaseName,
            Username = connection.UserName,
            Password = password,
            Timeout = connection.TimeoutSeconds,
            SslMode = connection.UseSsl ? SslMode.Prefer : SslMode.Disable
        };

        await using var dbConnection = new NpgsqlConnection(builder.ConnectionString);
        await dbConnection.OpenAsync();
        await using var command = new NpgsqlCommand("select version();", dbConnection);
        var version = Convert.ToString(await command.ExecuteScalarAsync()) ?? string.Empty;
        return $"接続成功: {version}";
    }

    /// <summary>
    /// パスワード解決処理。
    /// 画面入力値が空の場合は暗号化保存値を復号して利用します。
    /// </summary>
    /// <param name="connection">DB接続設定。</param>
    /// <param name="plainPassword">画面入力された平文パスワード。</param>
    /// <returns>DB接続用パスワード。</returns>
    private string ResolvePassword(DbConnectionProfile connection, string plainPassword)
    {
        if (!string.IsNullOrEmpty(plainPassword))
        {
            return plainPassword;
        }

        if (connection.PasswordSaveType == PasswordSaveType.WindowsDpapi && !string.IsNullOrWhiteSpace(connection.EncryptedPassword))
        {
            return passwordProtector.Unprotect(connection.EncryptedPassword);
        }

        return string.Empty;
    }
}
