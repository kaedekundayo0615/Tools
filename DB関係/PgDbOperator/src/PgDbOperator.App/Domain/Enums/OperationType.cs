namespace PgDbOperator.Domain.Enums;

/// <summary>
/// 操作種別。
/// 実行履歴に保存するDB操作の分類を表します。
/// </summary>
public enum OperationType
{
    Backup,
    Restore,
    SqlExecution,
    DataReplace,
    ConnectionTest
}
