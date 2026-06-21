using PgDbOperator.Domain.Models;
using PgDbOperator.Infrastructure;

namespace PgDbOperator.ViewModels;

/// <summary>
/// アプリ設定画面ViewModel。
/// デフォルト保存先や安全設定を管理します。
/// </summary>
public sealed class SettingsViewModel : ObservableObject
{
    private readonly JsonSettingsStore settingsStore;
    private AppSettings settings;
    private string message = string.Empty;

    public RelayCommand SaveCommand { get; }

    public string DefaultBackupDirectory
    {
        get => settings.DefaultBackupDirectory;
        set { settings.DefaultBackupDirectory = value; OnPropertyChanged(); }
    }

    public string DefaultSqlDirectory
    {
        get => settings.DefaultSqlDirectory;
        set { settings.DefaultSqlDirectory = value; OnPropertyChanged(); }
    }

    public int LogRetentionDays
    {
        get => settings.LogRetentionDays;
        set { settings.LogRetentionDays = value; OnPropertyChanged(); }
    }

    public bool RequireBackupBeforeRestore
    {
        get => settings.RequireBackupBeforeRestore;
        set { settings.RequireBackupBeforeRestore = value; OnPropertyChanged(); }
    }

    public bool RequireWarningForProduction
    {
        get => settings.RequireWarningForProduction;
        set { settings.RequireWarningForProduction = value; OnPropertyChanged(); }
    }

    public bool WarnDangerousSql
    {
        get => settings.WarnDangerousSql;
        set { settings.WarnDangerousSql = value; OnPropertyChanged(); }
    }

    public string Message
    {
        get => message;
        set => SetProperty(ref message, value);
    }

    /// <summary>
    /// アプリ設定画面ViewModel初期化処理。
    /// 保存済みアプリ設定を読み込みます。
    /// </summary>
    public SettingsViewModel(JsonSettingsStore settingsStore)
    {
        this.settingsStore = settingsStore;
        settings = settingsStore.Load();
        SaveCommand = new RelayCommand(Save);
    }

    private void Save()
    {
        settingsStore.Save(settings);
        Message = "アプリ設定を保存しました。";
    }
}
