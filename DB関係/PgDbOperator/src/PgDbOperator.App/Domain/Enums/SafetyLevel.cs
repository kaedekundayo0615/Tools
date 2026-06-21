namespace PgDbOperator.Domain.Enums;

/// <summary>
/// 安全レベル。
/// 操作前確認の強度を制御するための分類を表します。
/// </summary>
public enum SafetyLevel
{
    Normal,
    Caution,
    Dangerous
}
