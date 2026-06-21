namespace PgDbOperator.Infrastructure;

/// <summary>
/// プロセス実行結果。
/// 外部EXE実行時の終了コード、標準出力、標準エラーを表します。
/// </summary>
public sealed class ProcessExecutionResult
{
    public int ExitCode { get; init; }
    public string StandardOutput { get; init; } = string.Empty;
    public string StandardError { get; init; } = string.Empty;
}
