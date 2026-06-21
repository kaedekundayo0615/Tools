namespace PgDbOperator.Domain.Enums;

/// <summary>
/// Dump形式。
/// pg_dumpで出力するバックアップファイル形式を表します。
/// </summary>
public enum DumpFormat
{
    Custom,
    Plain,
    Tar,
    Directory
}
