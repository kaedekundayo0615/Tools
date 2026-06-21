using System.IO;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using PgDbOperator.Domain.Models;

namespace PgDbOperator.Infrastructure;

/// <summary>
/// JSON実行履歴ストア。
/// DB操作の実行履歴をローカルJSONに保存します。
/// </summary>
public sealed class JsonExecutionHistoryStore
{
    private readonly AppStorageContext storageContext;
    private readonly JsonSerializerOptions options;

    /// <summary>
    /// 実行履歴ストア初期化処理。
    /// 保存先コンテキストとシリアライズ設定を保持します。
    /// </summary>
    /// <param name="storageContext">アプリ保存先コンテキスト。</param>
    public JsonExecutionHistoryStore(AppStorageContext storageContext)
    {
        this.storageContext = storageContext;
        options = CreateOptions();
    }

    /// <summary>
    /// 履歴読込処理。
    /// history.jsonから実行履歴を読み込みます。
    /// </summary>
    /// <returns>実行履歴一覧。</returns>
    public List<ExecutionHistory> Load()
    {
        if (!File.Exists(storageContext.HistoryFilePath))
        {
            return new List<ExecutionHistory>();
        }

        var json = File.ReadAllText(storageContext.HistoryFilePath);
        return JsonSerializer.Deserialize<List<ExecutionHistory>>(json, options) ?? new List<ExecutionHistory>();
    }

    /// <summary>
    /// 履歴保存処理。
    /// 実行履歴一覧をhistory.jsonへ保存します。
    /// </summary>
    /// <param name="histories">保存対象履歴。</param>
    public void Save(List<ExecutionHistory> histories)
    {
        var json = JsonSerializer.Serialize(histories, options);
        File.WriteAllText(storageContext.HistoryFilePath, json);
    }

    /// <summary>
    /// JSON設定生成処理。
    /// Enum文字列化と日本語エスケープ抑制を設定します。
    /// </summary>
    /// <returns>JSONシリアライズ設定。</returns>
    private static JsonSerializerOptions CreateOptions()
    {
        var jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };
        jsonOptions.Converters.Add(new JsonStringEnumConverter());
        return jsonOptions;
    }
}
