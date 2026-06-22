using PgDbOperator.Domain.Enums;

namespace PgDbOperator.Domain.Models;

/// <summary>
/// 対象アプリケーション。
/// DBを利用するWEBシステム、デスクトップアプリ、自社サーバ等の管理単位を表します。
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
    /// アプリ種別表示名取得処理。
    /// 一覧に表示する日本語のアプリ種別名を返します。
    /// </summary>
    public string ApplicationTypeDisplayName => ApplicationType switch
    {
        ApplicationType.WebSystem => "WEBシステム",
        ApplicationType.DesktopApplication => "デスクトップアプリ",
        ApplicationType.CompanyServer => "自社サーバ",
        ApplicationType.HomePage => "WEBシステム",
        ApplicationType.Batch => "その他",
        ApplicationType.InternalTool => "その他",
        _ => "その他"
    };

    /// <summary>
    /// 有効状態表示名取得処理。
    /// 一覧に表示する有効または無効の表示名を返します。
    /// </summary>
    public string IsEnabledDisplayName => IsEnabled ? "有効" : "無効";

    /// <summary>
    /// 表示文字列生成処理。
    /// 一覧やコンボボックスに表示する名称を返します。
    /// </summary>
    /// <returns>表示名。</returns>
    public override string ToString() => Name;
}
