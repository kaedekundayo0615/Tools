namespace PgDbOperator.Domain.Enums;

/// <summary>
/// SQL失敗時動作。
/// 複数SQL実行時にエラーが発生した場合の動作を表します。
/// </summary>
public enum SqlFailureBehavior
{
    StopImmediately,
    Continue,
    Confirm
}
