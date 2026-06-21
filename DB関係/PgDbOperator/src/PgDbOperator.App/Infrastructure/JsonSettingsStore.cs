using System.IO;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using PgDbOperator.Domain.Models;

namespace PgDbOperator.Infrastructure;

/// <summary>
/// JSON設定ストア。
/// 対象アプリ、DB接続、PostgreSQLクライアント設定をローカルJSONに保存します。
/// </summary>
public sealed class JsonSettingsStore
{
    private readonly AppStorageContext storageContext;
    private readonly JsonSerializerOptions options;

    /// <summary>
    /// JSON設定ストア初期化処理。
    /// 保存先コンテキストとシリアライズ設定を保持します。
    /// </summary>
    /// <param name="storageContext">アプリ保存先コンテキスト。</param>
    public JsonSettingsStore(AppStorageContext storageContext)
    {
        this.storageContext = storageContext;
        options = CreateOptions();
    }

    /// <summary>
    /// 設定読込処理。
    /// settings.jsonからアプリ設定を読み込みます。
    /// </summary>
    /// <returns>アプリ設定。</returns>
    public AppSettings Load()
    {
        if (!File.Exists(storageContext.SettingsFilePath))
        {
            return new AppSettings();
        }

        var json = File.ReadAllText(storageContext.SettingsFilePath);
        return JsonSerializer.Deserialize<AppSettings>(json, options) ?? new AppSettings();
    }

    /// <summary>
    /// 設定保存処理。
    /// 現在のアプリ設定をsettings.jsonへ保存します。
    /// </summary>
    /// <param name="settings">保存対象設定。</param>
    public void Save(AppSettings settings)
    {
        var json = JsonSerializer.Serialize(settings, options);
        File.WriteAllText(storageContext.SettingsFilePath, json);
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
