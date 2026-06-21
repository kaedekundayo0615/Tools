using System.IO;
using System.Collections.ObjectModel;
using System.Windows;
using PgDbOperator.Domain.Enums;
using PgDbOperator.Domain.Models;
using PgDbOperator.Infrastructure;
using PgDbOperator.Services;

namespace PgDbOperator.ViewModels;

/// <summary>
/// データ入れ替え画面ViewModel。
/// 移行元DBから移行先DBへのDump/Restoreと後処理SQLを制御します。
/// </summary>
public sealed class DataReplaceViewModel : OperationViewModelBase
{
    private readonly DbOperationService operationService;
    private readonly SafetyCheckService safetyCheckService;
    private DbConnectionProfile? sourceConnection;
    private DbConnectionProfile? targetConnection;
    private string sourcePassword = string.Empty;
    private string targetPassword = string.Empty;
    private string workDirectory = string.Empty;
    private string afterRestoreSqlDirectory = string.Empty;

    public ObservableCollection<SqlFileItemViewModel> AfterRestoreSqlFiles { get; } = new();
    public bool BackupTargetBeforeRestore { get; set; } = true;
    public RelayCommand LoadAfterRestoreSqlCommand { get; }
    public AsyncRelayCommand ExecuteCommand { get; }

    public DbConnectionProfile? SourceConnection { get => sourceConnection; set => SetProperty(ref sourceConnection, value); }
    public DbConnectionProfile? TargetConnection { get => targetConnection; set => SetProperty(ref targetConnection, value); }
    public string SourcePassword { get => sourcePassword; set => SetProperty(ref sourcePassword, value); }
    public string TargetPassword { get => targetPassword; set => SetProperty(ref targetPassword, value); }
    public string WorkDirectory { get => workDirectory; set => SetProperty(ref workDirectory, value); }
    public string AfterRestoreSqlDirectory { get => afterRestoreSqlDirectory; set => SetProperty(ref afterRestoreSqlDirectory, value); }

    /// <summary>
    /// データ入れ替え画面ViewModel初期化処理。
    /// データ入れ替え実行と後処理SQL読込コマンドを設定します。
    /// </summary>
    public DataReplaceViewModel(JsonSettingsStore settingsStore, DbOperationService operationService, SafetyCheckService safetyCheckService) : base(settingsStore)
    {
        this.operationService = operationService;
        this.safetyCheckService = safetyCheckService;
        SourceConnection = Connections.FirstOrDefault();
        TargetConnection = Connections.Skip(1).FirstOrDefault();
        WorkDirectory = Settings.DefaultBackupDirectory;
        AfterRestoreSqlDirectory = Settings.DefaultSqlDirectory;
        LoadAfterRestoreSqlCommand = new RelayCommand(LoadAfterRestoreSql);
        ExecuteCommand = new AsyncRelayCommand(ExecuteAsync);
    }

    private void LoadAfterRestoreSql()
    {
        AfterRestoreSqlFiles.Clear();
        if (!Directory.Exists(AfterRestoreSqlDirectory))
        {
            Message = "後処理SQLフォルダが存在しません。";
            return;
        }
        foreach (var file in Directory.GetFiles(AfterRestoreSqlDirectory, "*.sql").OrderBy(x => x))
        {
            AfterRestoreSqlFiles.Add(new SqlFileItemViewModel(file));
        }
        Message = $"後処理SQLを{AfterRestoreSqlFiles.Count}件読み込みました。";
    }

    private async Task ExecuteAsync()
    {
        try
        {
            if (SourceConnection == null) throw new InvalidOperationException("移行元DBを選択してください。");
            if (TargetConnection == null) throw new InvalidOperationException("移行先DBを選択してください。");
            if (safetyCheckService.IsSameDatabase(SourceConnection, TargetConnection)) throw new InvalidOperationException("移行元DBと移行先DBが同一です。");
            if (safetyCheckService.IsCautionDatabase(TargetConnection) && MessageBox.Show($"注意が必要なDBを移行先にしています。\n移行先DB: {TargetConnection.DatabaseName}\n続行しますか？", "データ入れ替え確認", MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes) return;
            var sqlFiles = AfterRestoreSqlFiles.Where(x => x.IsSelected).Select(x => x.FilePath).ToList();
            var histories = await operationService.ReplaceDataAsync(RequireApplication(), SourceConnection, TargetConnection, RequireClient(), SourcePassword, TargetPassword, WorkDirectory, BackupTargetBeforeRestore, sqlFiles);
            Message = $"データ入れ替え完了: 成功 {histories.Count(x => x.Result == ExecutionResult.Success)} 件 / 失敗 {histories.Count(x => x.Result == ExecutionResult.Failed)} 件";
        }
        catch (Exception ex)
        {
            Message = $"データ入れ替え失敗: {ex.Message}";
        }
    }
}
