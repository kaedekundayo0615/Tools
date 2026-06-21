namespace PgDbOperator.Infrastructure;

/// <summary>
/// プロセス実行要求。
/// 外部EXEのパス、引数、環境変数を表します。
/// </summary>
public sealed class ProcessExecutionRequest
{
    public string FileName { get; init; } = string.Empty;
    public List<string> Arguments { get; init; } = new();
    public Dictionary<string, string> EnvironmentVariables { get; init; } = new();
    public string WorkingDirectory { get; init; } = string.Empty;
}
