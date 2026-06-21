using System.Collections.ObjectModel;
using PgDbOperator.Domain.Models;
using PgDbOperator.Infrastructure;

namespace PgDbOperator.ViewModels;

/// <summary>
/// 操作画面ViewModel基底。
/// バックアップ、リストア、SQL実行、データ入れ替えで共通利用する選択リストを管理します。
/// </summary>
public abstract class OperationViewModelBase : ObservableObject
{
    protected readonly JsonSettingsStore SettingsStore;
    protected AppSettings Settings;

    public ObservableCollection<ManagedApplication> Applications { get; }
    public ObservableCollection<DbConnectionProfile> Connections { get; }
    public ObservableCollection<PostgresClientProfile> Clients { get; }

    private ManagedApplication? selectedApplication;
    private DbConnectionProfile? selectedConnection;
    private PostgresClientProfile? selectedClient;
    private string password = string.Empty;
    private string message = string.Empty;

    public ManagedApplication? SelectedApplication
    {
        get => selectedApplication;
        set
        {
            if (SetProperty(ref selectedApplication, value))
            {
                RefreshConnectionSelection();
            }
        }
    }

    public DbConnectionProfile? SelectedConnection
    {
        get => selectedConnection;
        set => SetProperty(ref selectedConnection, value);
    }

    public PostgresClientProfile? SelectedClient
    {
        get => selectedClient;
        set => SetProperty(ref selectedClient, value);
    }

    public string Password
    {
        get => password;
        set => SetProperty(ref password, value);
    }

    public string Message
    {
        get => message;
        set => SetProperty(ref message, value);
    }

    /// <summary>
    /// 操作画面ViewModel基底初期化処理。
    /// 設定から対象アプリ、DB接続、PostgreSQLクライアントを読み込みます。
    /// </summary>
    protected OperationViewModelBase(JsonSettingsStore settingsStore)
    {
        SettingsStore = settingsStore;
        Settings = settingsStore.Load();
        Applications = new ObservableCollection<ManagedApplication>(Settings.Applications.Where(x => x.IsEnabled));
        Connections = new ObservableCollection<DbConnectionProfile>(Settings.DbConnections.Where(x => x.IsEnabled));
        Clients = new ObservableCollection<PostgresClientProfile>(Settings.PostgresClients);
        SelectedApplication = Applications.FirstOrDefault();
        SelectedClient = Clients.FirstOrDefault(x => x.IsDefault) ?? Clients.FirstOrDefault();
    }

    /// <summary>
    /// 対象アプリ取得処理。
    /// 未選択の場合は例外を発生させます。
    /// </summary>
    /// <returns>選択中の対象アプリ。</returns>
    protected ManagedApplication RequireApplication() => SelectedApplication ?? throw new InvalidOperationException("対象アプリを選択してください。");

    /// <summary>
    /// DB接続取得処理。
    /// 未選択の場合は例外を発生させます。
    /// </summary>
    /// <returns>選択中のDB接続設定。</returns>
    protected DbConnectionProfile RequireConnection() => SelectedConnection ?? throw new InvalidOperationException("DB接続を選択してください。");

    /// <summary>
    /// PostgreSQLクライアント取得処理。
    /// 未選択の場合は例外を発生させます。
    /// </summary>
    /// <returns>選択中のPostgreSQLクライアント設定。</returns>
    protected PostgresClientProfile RequireClient() => SelectedClient ?? throw new InvalidOperationException("PostgreSQL設定を選択してください。");

    /// <summary>
    /// 接続選択更新処理。
    /// 対象アプリ選択時に関連するDB接続を初期選択します。
    /// </summary>
    private void RefreshConnectionSelection()
    {
        if (SelectedApplication == null) return;
        SelectedConnection = Connections.FirstOrDefault(x => x.ApplicationId == SelectedApplication.Id);
    }
}
