using PgDbOperator.Domain.Enums;
using PgDbOperator.Domain.Models;

namespace PgDbOperator.Services;

/// <summary>
/// 安全チェックサービス。
/// 本番環境操作、同一DB入れ替え、危険SQLの検知を行います。
/// </summary>
public sealed class SafetyCheckService
{
    private static readonly string[] DangerousKeywords =
    {
        "DROP ", "TRUNCATE ", "DELETE ", "UPDATE ", "ALTER TABLE", "REINDEX", "VACUUM FULL"
    };

    /// <summary>
    /// 注意DB判定処理。
    /// 本番環境または危険レベルの接続か判定します。
    /// </summary>
    /// <param name="connection">DB接続設定。</param>
    /// <returns>注意が必要なDBの場合はtrue。</returns>
    public bool IsCautionDatabase(DbConnectionProfile connection)
    {
        return connection.EnvironmentType == EnvironmentType.Production || connection.SafetyLevel != SafetyLevel.Normal;
    }

    /// <summary>
    /// 同一DB判定処理。
    /// データ入れ替え元と先が同一DBか判定します。
    /// </summary>
    /// <param name="source">移行元DB接続。</param>
    /// <param name="target">移行先DB接続。</param>
    /// <returns>同一DBの場合はtrue。</returns>
    public bool IsSameDatabase(DbConnectionProfile source, DbConnectionProfile target)
    {
        return string.Equals(source.Host, target.Host, StringComparison.OrdinalIgnoreCase)
            && source.Port == target.Port
            && string.Equals(source.DatabaseName, target.DatabaseName, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// 危険SQL検知処理。
    /// SQL本文に破壊的または注意が必要なキーワードが含まれるか判定します。
    /// </summary>
    /// <param name="sqlText">SQL本文。</param>
    /// <returns>検出した危険キーワード一覧。</returns>
    public IReadOnlyList<string> DetectDangerousSql(string sqlText)
    {
        var normalized = sqlText.ToUpperInvariant().ReplaceLineEndings(" ");
        return DangerousKeywords.Where(keyword => normalized.Contains(keyword, StringComparison.OrdinalIgnoreCase)).Distinct().ToList();
    }
}
