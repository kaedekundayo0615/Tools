namespace PgDbOperator.Domain.Enums;

/// <summary>
/// 実行結果。
/// DB操作の終了状態を表します。
/// </summary>
public enum ExecutionResult
{
    Success,
    Failed,
    Cancelled,
    Warning
}
