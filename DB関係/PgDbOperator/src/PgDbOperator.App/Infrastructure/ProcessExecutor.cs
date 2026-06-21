using System.Diagnostics;
using System.Text;

namespace PgDbOperator.Infrastructure;

/// <summary>
/// プロセス実行サービス。
/// pg_dump、pg_restore、psqlなどの外部コマンドを安全に実行します。
/// </summary>
public sealed class ProcessExecutor
{
    /// <summary>
    /// 外部プロセス実行処理。
    /// 引数リストを使用してコマンドライン結合による事故を避けながら実行します。
    /// </summary>
    /// <param name="request">プロセス実行要求。</param>
    /// <returns>プロセス実行結果。</returns>
    public async Task<ProcessExecutionResult> ExecuteAsync(ProcessExecutionRequest request)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = request.FileName,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true,
            StandardOutputEncoding = Encoding.UTF8,
            StandardErrorEncoding = Encoding.UTF8
        };

        if (!string.IsNullOrWhiteSpace(request.WorkingDirectory))
        {
            startInfo.WorkingDirectory = request.WorkingDirectory;
        }

        foreach (var argument in request.Arguments)
        {
            startInfo.ArgumentList.Add(argument);
        }

        foreach (var variable in request.EnvironmentVariables)
        {
            startInfo.Environment[variable.Key] = variable.Value;
        }

        using var process = new Process { StartInfo = startInfo };
        process.Start();

        var stdoutTask = process.StandardOutput.ReadToEndAsync();
        var stderrTask = process.StandardError.ReadToEndAsync();
        await process.WaitForExitAsync();

        return new ProcessExecutionResult
        {
            ExitCode = process.ExitCode,
            StandardOutput = await stdoutTask,
            StandardError = await stderrTask
        };
    }
}
