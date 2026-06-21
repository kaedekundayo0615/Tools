using System.IO;
namespace PgDbOperator.Infrastructure;

/// <summary>
/// アプリ保存先コンテキスト。
/// 設定ファイル、履歴ファイル、ログファイルの保存先を提供します。
/// </summary>
public sealed class AppStorageContext
{
    public string RootDirectory { get; }
    public string SettingsFilePath { get; }
    public string HistoryFilePath { get; }
    public string LogDirectory { get; }

    /// <summary>
    /// 保存先初期化処理。
    /// %APPDATA%配下にアプリ用ディレクトリを作成します。
    /// </summary>
    public AppStorageContext()
    {
        RootDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "PgDbOperator");
        SettingsFilePath = Path.Combine(RootDirectory, "settings.json");
        HistoryFilePath = Path.Combine(RootDirectory, "history.json");
        LogDirectory = Path.Combine(RootDirectory, "logs");

        Directory.CreateDirectory(RootDirectory);
        Directory.CreateDirectory(LogDirectory);
    }
}
