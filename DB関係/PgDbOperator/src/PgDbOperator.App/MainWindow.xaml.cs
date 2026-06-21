using System.Windows;
using PgDbOperator.Infrastructure;
using PgDbOperator.Services;
using PgDbOperator.ViewModels;

namespace PgDbOperator;

/// <summary>
/// メイン画面。
/// アプリケーション全体のナビゲーションと各機能画面の表示を制御します。
/// </summary>
public partial class MainWindow : Window
{
    /// <summary>
    /// メイン画面初期化処理。
    /// 依存サービスを生成し、メインViewModelを設定します。
    /// </summary>
    public MainWindow()
    {
        InitializeComponent();

        var appContext = new AppStorageContext();
        var settingsStore = new JsonSettingsStore(appContext);
        var historyStore = new JsonExecutionHistoryStore(appContext);
        var passwordProtector = new DpapiPasswordProtector();
        var processExecutor = new ProcessExecutor();
        var commandBuilder = new PostgresCommandBuilder();
        var safetyChecker = new SafetyCheckService();
        var connectionTestService = new ConnectionTestService(passwordProtector);
        var historyService = new ExecutionHistoryService(historyStore, appContext);
        var operationService = new DbOperationService(processExecutor, commandBuilder, historyService, safetyChecker, passwordProtector);

        DataContext = new MainWindowViewModel(settingsStore, historyService, operationService, connectionTestService, safetyChecker, passwordProtector);
    }
}
