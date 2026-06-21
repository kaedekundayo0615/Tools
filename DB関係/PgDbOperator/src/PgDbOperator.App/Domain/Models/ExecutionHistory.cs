using PgDbOperator.Domain.Enums;

namespace PgDbOperator.Domain.Models;

/// <summary>
/// 実行履歴。
/// バックアップ、リストア、SQL実行、データ入れ替えの作業証跡を表します。
/// </summary>
public sealed class ExecutionHistory
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTime StartedAt { get; set; } = DateTime.Now;
    public DateTime? EndedAt { get; set; }
    public OperationType OperationType { get; set; }
    public ExecutionResult Result { get; set; } = ExecutionResult.Success;
    public string ApplicationName { get; set; } = string.Empty;
    public string ConnectionName { get; set; } = string.Empty;
    public string Host { get; set; } = string.Empty;
    public string DatabaseName { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string TargetFilePath { get; set; } = string.Empty;
    public string CommandText { get; set; } = string.Empty;
    public string StandardOutput { get; set; } = string.Empty;
    public string StandardError { get; set; } = string.Empty;
    public int ExitCode { get; set; }
    public string Message { get; set; } = string.Empty;
    public string LogFilePath { get; set; } = string.Empty;

    /// <summary>
    /// 実行秒数取得処理。
    /// 開始日時と終了日時から実行時間を算出します。
    /// </summary>
    public double ElapsedSeconds => EndedAt.HasValue ? (EndedAt.Value - StartedAt).TotalSeconds : 0;
}
