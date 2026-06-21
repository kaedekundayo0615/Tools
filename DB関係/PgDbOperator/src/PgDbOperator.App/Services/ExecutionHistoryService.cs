using System.IO;
using System.Text;
using PgDbOperator.Domain.Models;
using PgDbOperator.Infrastructure;

namespace PgDbOperator.Services;

/// <summary>
/// 実行履歴サービス。
/// 実行履歴JSONと詳細ログファイルを保存します。
/// </summary>
public sealed class ExecutionHistoryService
{
    private readonly JsonExecutionHistoryStore historyStore;
    private readonly AppStorageContext storageContext;

    /// <summary>
    /// 実行履歴サービス初期化処理。
    /// 履歴ストアと保存先コンテキストを保持します。
    /// </summary>
    /// <param name="historyStore">履歴ストア。</param>
    /// <param name="storageContext">保存先コンテキスト。</param>
    public ExecutionHistoryService(JsonExecutionHistoryStore historyStore, AppStorageContext storageContext)
    {
        this.historyStore = historyStore;
        this.storageContext = storageContext;
    }

    /// <summary>
    /// 履歴一覧取得処理。
    /// 保存済み実行履歴を新しい順で取得します。
    /// </summary>
    /// <returns>実行履歴一覧。</returns>
    public List<ExecutionHistory> GetHistories()
    {
        return historyStore.Load().OrderByDescending(x => x.StartedAt).ToList();
    }

    /// <summary>
    /// 履歴追加処理。
    /// 実行履歴をJSONへ追加し、詳細ログファイルを保存します。
    /// </summary>
    /// <param name="history">追加する実行履歴。</param>
    public void Add(ExecutionHistory history)
    {
        history.LogFilePath = SaveDetailLog(history);
        var histories = historyStore.Load();
        histories.Add(history);
        historyStore.Save(histories.OrderByDescending(x => x.StartedAt).Take(1000).ToList());
    }

    /// <summary>
    /// 詳細ログ保存処理。
    /// 標準出力、標準エラー、実行コマンドを個別ログファイルへ保存します。
    /// </summary>
    /// <param name="history">実行履歴。</param>
    /// <returns>保存したログファイルパス。</returns>
    private string SaveDetailLog(ExecutionHistory history)
    {
        var fileName = $"{history.StartedAt:yyyyMMdd_HHmmss}_{history.OperationType}_{history.Id:N}.log";
        var path = Path.Combine(storageContext.LogDirectory, fileName);
        var builder = new StringBuilder();
        builder.AppendLine($"StartedAt: {history.StartedAt:yyyy-MM-dd HH:mm:ss}");
        builder.AppendLine($"EndedAt: {history.EndedAt:yyyy-MM-dd HH:mm:ss}");
        builder.AppendLine($"OperationType: {history.OperationType}");
        builder.AppendLine($"Result: {history.Result}");
        builder.AppendLine($"Application: {history.ApplicationName}");
        builder.AppendLine($"Connection: {history.ConnectionName}");
        builder.AppendLine($"Host: {history.Host}");
        builder.AppendLine($"Database: {history.DatabaseName}");
        builder.AppendLine($"User: {history.UserName}");
        builder.AppendLine($"TargetFile: {history.TargetFilePath}");
        builder.AppendLine($"ExitCode: {history.ExitCode}");
        builder.AppendLine("Command:");
        builder.AppendLine(history.CommandText);
        builder.AppendLine("StandardOutput:");
        builder.AppendLine(history.StandardOutput);
        builder.AppendLine("StandardError:");
        builder.AppendLine(history.StandardError);
        builder.AppendLine("Message:");
        builder.AppendLine(history.Message);
        File.WriteAllText(path, builder.ToString(), Encoding.UTF8);
        return path;
    }
}
