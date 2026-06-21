namespace PgDbOperator.Domain.Enums;

/// <summary>
/// 環境区分。
/// DB接続先がどの環境に該当するかを表します。
/// </summary>
public enum EnvironmentType
{
    Local,
    Development,
    Verification,
    Staging,
    Production,
    Other
}
