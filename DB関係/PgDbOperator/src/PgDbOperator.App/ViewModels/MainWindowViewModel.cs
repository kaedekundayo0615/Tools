using PgDbOperator.Infrastructure;
using PgDbOperator.Services;
using PgDbOperator.Views;
using System.Windows;

namespace PgDbOperator.ViewModels;

/// <summary>
/// メイン画面ViewModel。
/// 画面遷移と共有サービスの受け渡しを管理します。
/// </summary>
public sealed class MainWindowViewModel : ObservableObject
{
    private object? currentView;
    private string currentMenuKey = "Home";
    private bool isSideMenuCollapsed;
    private readonly JsonSettingsStore settingsStore;
    private readonly ExecutionHistoryService historyService;
    private readonly DbOperationService operationService;
    private readonly ConnectionTestService connectionTestService;
    private readonly SafetyCheckService safetyCheckService;
    private readonly DpapiPasswordProtector passwordProtector;

    public RelayCommand NavigateHomeCommand { get; }
    public RelayCommand NavigateApplicationsCommand { get; }
    public RelayCommand NavigateDbConnectionsCommand { get; }
    public RelayCommand NavigatePostgresClientsCommand { get; }
    public RelayCommand NavigateBackupCommand { get; }
    public RelayCommand NavigateRestoreCommand { get; }
    public RelayCommand NavigateSqlExecutionCommand { get; }
    public RelayCommand NavigateDataReplaceCommand { get; }
    public RelayCommand NavigateHistoryCommand { get; }
    public RelayCommand NavigateSettingsCommand { get; }
    public RelayCommand ToggleSideMenuCommand { get; }

    /// <summary>
    /// 現在表示画面。
    /// 右側メイン領域に表示するUserControlを保持します。
    /// </summary>
    public object? CurrentView
    {
        get => currentView;
        private set => SetProperty(ref currentView, value);
    }


    /// <summary>
    /// 現在選択中メニューキー。
    /// サイドメニューの選択状態表示に使用します。
    /// </summary>
    public string CurrentMenuKey
    {
        get => currentMenuKey;
        private set => SetProperty(ref currentMenuKey, value);
    }

    /// <summary>
    /// サイドメニュー折りたたみ状態。
    /// trueの場合はアイコンのみ表示します。
    /// </summary>
    public bool IsSideMenuCollapsed
    {
        get => isSideMenuCollapsed;
        private set
        {
            if (!SetProperty(ref isSideMenuCollapsed, value))
            {
                return;
            }

            OnPropertyChanged(nameof(SideMenuWidth));
            OnPropertyChanged(nameof(MenuTextVisibility));
            OnPropertyChanged(nameof(TitleTextVisibility));
        }
    }

    /// <summary>
    /// サイドメニュー列幅。
    /// 折りたたみ状態に応じて通常幅またはアイコン幅を返します。
    /// </summary>
    public GridLength SideMenuWidth => IsSideMenuCollapsed
        ? new GridLength(56)
        : new GridLength(240);

    /// <summary>
    /// メニュー文字表示状態。
    /// 折りたたみ時は非表示にします。
    /// </summary>
    public Visibility MenuTextVisibility => IsSideMenuCollapsed
        ? Visibility.Collapsed
        : Visibility.Visible;

    /// <summary>
    /// サイドメニュータイトル表示状態。
    /// 折りたたみ時は非表示にします。
    /// </summary>
    public Visibility TitleTextVisibility => IsSideMenuCollapsed
        ? Visibility.Collapsed
        : Visibility.Visible;

    /// <summary>
    /// メイン画面ViewModel初期化処理。
    /// 画面遷移コマンドを生成し、ホーム画面を初期表示します。
    /// </summary>
    public MainWindowViewModel(JsonSettingsStore settingsStore, ExecutionHistoryService historyService, DbOperationService operationService, ConnectionTestService connectionTestService, SafetyCheckService safetyCheckService, DpapiPasswordProtector passwordProtector)
    {
        this.settingsStore = settingsStore;
        this.historyService = historyService;
        this.operationService = operationService;
        this.connectionTestService = connectionTestService;
        this.safetyCheckService = safetyCheckService;
        this.passwordProtector = passwordProtector;

        NavigateHomeCommand = new RelayCommand(NavigateHome);
        NavigateApplicationsCommand = new RelayCommand(NavigateApplications);
        NavigateDbConnectionsCommand = new RelayCommand(NavigateDbConnections);
        NavigatePostgresClientsCommand = new RelayCommand(NavigatePostgresClients);
        NavigateBackupCommand = new RelayCommand(NavigateBackup);
        NavigateRestoreCommand = new RelayCommand(NavigateRestore);
        NavigateSqlExecutionCommand = new RelayCommand(NavigateSqlExecution);
        NavigateDataReplaceCommand = new RelayCommand(NavigateDataReplace);
        NavigateHistoryCommand = new RelayCommand(NavigateHistory);
        NavigateSettingsCommand = new RelayCommand(NavigateSettings);
        ToggleSideMenuCommand = new RelayCommand(ToggleSideMenu);

        NavigateHome();
    }

    /// <summary>
    /// サイドメニュー折りたたみ切替処理。
    /// メニュー幅とタイトル・文字の表示状態を切り替えます。
    /// </summary>
    private void ToggleSideMenu()
    {
        IsSideMenuCollapsed = !IsSideMenuCollapsed;
    }

    /// <summary>
    /// ホーム画面遷移処理。
    /// </summary>
    private void NavigateHome()
    {
        CurrentMenuKey = "Home";
        CurrentView = new HomeView { DataContext = new HomeViewModel(settingsStore, historyService) };
    }

    /// <summary>
    /// 対象アプリ管理画面遷移処理。
    /// </summary>
    private void NavigateApplications()
    {
        CurrentMenuKey = "Applications";
        CurrentView = new ApplicationsView { DataContext = new ApplicationsViewModel(settingsStore) };
    }

    /// <summary>
    /// DB接続管理画面遷移処理。
    /// </summary>
    private void NavigateDbConnections()
    {
        CurrentMenuKey = "DbConnections";
        CurrentView = new DbConnectionsView { DataContext = new DbConnectionsViewModel(settingsStore, connectionTestService, passwordProtector) };
    }

    /// <summary>
    /// PostgreSQL設定画面遷移処理。
    /// </summary>
    private void NavigatePostgresClients()
    {
        CurrentMenuKey = "PostgresClients";
        CurrentView = new PostgresClientsView { DataContext = new PostgresClientsViewModel(settingsStore) };
    }

    /// <summary>
    /// バックアップ画面遷移処理。
    /// </summary>
    private void NavigateBackup()
    {
        CurrentMenuKey = "Backup";
        CurrentView = new BackupView { DataContext = new BackupViewModel(settingsStore, operationService, safetyCheckService) };
    }

    /// <summary>
    /// リストア画面遷移処理。
    /// </summary>
    private void NavigateRestore()
    {
        CurrentMenuKey = "Restore";
        CurrentView = new RestoreView { DataContext = new RestoreViewModel(settingsStore, operationService, safetyCheckService) };
    }

    /// <summary>
    /// SQL実行画面遷移処理。
    /// </summary>
    private void NavigateSqlExecution()
    {
        CurrentMenuKey = "SqlExecution";
        CurrentView = new SqlExecutionView { DataContext = new SqlExecutionViewModel(settingsStore, operationService, safetyCheckService) };
    }

    /// <summary>
    /// データ入れ替え画面遷移処理。
    /// </summary>
    private void NavigateDataReplace()
    {
        CurrentMenuKey = "DataReplace";
        CurrentView = new DataReplaceView { DataContext = new DataReplaceViewModel(settingsStore, operationService, safetyCheckService) };
    }

    /// <summary>
    /// 実行履歴画面遷移処理。
    /// </summary>
    private void NavigateHistory()
    {
        CurrentMenuKey = "History";
        CurrentView = new HistoryView { DataContext = new HistoryViewModel(historyService) };
    }

    /// <summary>
    /// アプリ設定画面遷移処理。
    /// </summary>
    private void NavigateSettings()
    {
        CurrentMenuKey = "Settings";
        CurrentView = new SettingsView { DataContext = new SettingsViewModel(settingsStore) };
    }
}
