using System.IO;
namespace PgDbOperator.Domain.Models;

/// <summary>
/// PostgreSQLクライアント設定。
/// pg_dump、pg_restore、psql等の実行ファイルパスを表します。
/// </summary>
public sealed class PostgresClientProfile
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public string BinDirectory { get; set; } = string.Empty;
    public string PgDumpPath { get; set; } = string.Empty;
    public string PgRestorePath { get; set; } = string.Empty;
    public string PsqlPath { get; set; } = string.Empty;
    public string CreateDbPath { get; set; } = string.Empty;
    public string DropDbPath { get; set; } = string.Empty;
    public bool IsDefault { get; set; }
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// binフォルダ反映処理。
    /// binフォルダ配下のPostgreSQL実行ファイルパスを自動設定します。
    /// </summary>
    public void ApplyBinDirectory()
    {
        if (string.IsNullOrWhiteSpace(BinDirectory))
        {
            return;
        }

        PgDumpPath = Path.Combine(BinDirectory, "pg_dump.exe");
        PgRestorePath = Path.Combine(BinDirectory, "pg_restore.exe");
        PsqlPath = Path.Combine(BinDirectory, "psql.exe");
        CreateDbPath = Path.Combine(BinDirectory, "createdb.exe");
        DropDbPath = Path.Combine(BinDirectory, "dropdb.exe");
    }

    /// <summary>
    /// 表示文字列生成処理。
    /// 一覧やコンボボックスに表示する名称を返します。
    /// </summary>
    /// <returns>表示名。</returns>
    public override string ToString() => string.IsNullOrWhiteSpace(Version) ? Name : $"{Name} / {Version}";
}
