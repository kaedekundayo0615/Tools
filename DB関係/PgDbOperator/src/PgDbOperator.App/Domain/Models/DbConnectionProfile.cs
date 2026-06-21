using PgDbOperator.Domain.Enums;

namespace PgDbOperator.Domain.Models;

/// <summary>
/// DB接続設定。
/// 対象アプリケーションに紐づくPostgreSQL接続情報を表します。
/// </summary>
public sealed class DbConnectionProfile
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ApplicationId { get; set; }
    public string Name { get; set; } = string.Empty;
    public EnvironmentType EnvironmentType { get; set; } = EnvironmentType.Development;
    public string Host { get; set; } = "localhost";
    public int Port { get; set; } = 5432;
    public string DatabaseName { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public PasswordSaveType PasswordSaveType { get; set; } = PasswordSaveType.DoNotSave;
    public string EncryptedPassword { get; set; } = string.Empty;
    public bool UseSsl { get; set; }
    public int TimeoutSeconds { get; set; } = 30;
    public SafetyLevel SafetyLevel { get; set; } = SafetyLevel.Normal;
    public string Description { get; set; } = string.Empty;
    public bool IsEnabled { get; set; } = true;

    /// <summary>
    /// 表示文字列生成処理。
    /// 環境区分と接続名を合わせた表示文字列を返します。
    /// </summary>
    /// <returns>表示名。</returns>
    public override string ToString() => $"{Name} ({EnvironmentType})";
}
