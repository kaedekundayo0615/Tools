namespace PgDbOperator.Domain.Enums;

/// <summary>
/// アプリケーション種別。
/// 管理対象アプリケーションの分類を表します。
/// </summary>
public enum ApplicationType
{
    WebSystem,
    DesktopApplication,
    CompanyServer,
    HomePage,
    Batch,
    InternalTool,
    Other
}
