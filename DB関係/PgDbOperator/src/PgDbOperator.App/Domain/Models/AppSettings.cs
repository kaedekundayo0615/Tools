namespace PgDbOperator.Domain.Models;

/// <summary>
/// アプリ設定。
/// ツール全体で共有する保存先や動作設定を表します。
/// </summary>
public sealed class AppSettings
{
    public List<ManagedApplication> Applications { get; set; } = new();
    public List<DbConnectionProfile> DbConnections { get; set; } = new();
    public List<PostgresClientProfile> PostgresClients { get; set; } = new();
    public string DefaultBackupDirectory { get; set; } = string.Empty;
    public string DefaultSqlDirectory { get; set; } = string.Empty;
    public int LogRetentionDays { get; set; } = 90;
    public bool RequireBackupBeforeRestore { get; set; } = true;
    public bool RequireWarningForProduction { get; set; } = true;
    public bool WarnDangerousSql { get; set; } = true;
}
