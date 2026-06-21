namespace PgDbOperator.Domain.Enums;

/// <summary>
/// パスワード保存方式。
/// DB接続パスワードを保存するかどうかを表します。
/// </summary>
public enum PasswordSaveType
{
    DoNotSave,
    WindowsDpapi
}
