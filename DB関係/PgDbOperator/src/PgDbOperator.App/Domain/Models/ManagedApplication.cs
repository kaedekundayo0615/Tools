using PgDbOperator.Domain.Enums;

namespace PgDbOperator.Domain.Models;

/// <summary>
/// 対象アプリケーション。
/// DBを利用するWebシステム、デスクトップアプリ、ホームページ等の管理単位を表します。
/// </summary>
public sealed class ManagedApplication
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public ApplicationType ApplicationType { get; set; } = ApplicationType.WebSystem;
    public string Description { get; set; } = string.Empty;
    public string DefaultBackupDirectory { get; set; } = string.Empty;
    public string DefaultSqlDirectory { get; set; } = string.Empty;
    public bool IsEnabled { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime UpdatedAt { get; set; } = DateTime.Now;

    /// <summary>
    /// 表示文字列生成処理。
    /// 一覧やコンボボックスに表示する名称を返します。
    /// </summary>
    /// <returns>表示名。</returns>
    public override string ToString() => Name;
}
