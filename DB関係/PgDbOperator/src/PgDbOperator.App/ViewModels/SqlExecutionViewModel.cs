using System.IO;
using System.Collections.ObjectModel;
using System.Windows;
using PgDbOperator.Domain.Enums;
using PgDbOperator.Infrastructure;
using PgDbOperator.Services;

namespace PgDbOperator.ViewModels;

/// <summary>
/// SQL実行画面ViewModel。
/// SQLファイルまたはフォルダ内SQLの実行を制御します。
/// </summary>
public sealed class SqlExecutionViewModel : OperationViewModelBase
{
    private readonly DbOperationService operationService;
    private readonly SafetyCheckService safetyCheckService;
    private string sqlDirectory = string.Empty;
    private string manualSqlFile = string.Empty;
    private SqlFailureBehavior failureBehavior = SqlFailureBehavior.StopImmediately;

    public ObservableCollection<SqlFileItemViewModel> SqlFiles { get; } = new();
    public Array FailureBehaviors => Enum.GetValues(typeof(SqlFailureBehavior));
    public RelayCommand LoadDirectoryCommand { get; }
    public RelayCommand AddManualSqlCommand { get; }
    public AsyncRelayCommand ExecuteCommand { get; }

    public string SqlDirectory { get => sqlDirectory; set => SetProperty(ref sqlDirectory, value); }
    public string ManualSqlFile { get => manualSqlFile; set => SetProperty(ref manualSqlFile, value); }
    public SqlFailureBehavior FailureBehavior { get => failureBehavior; set => SetProperty(ref failureBehavior, value); }

    /// <summary>
    /// SQL実行画面ViewModel初期化処理。
    /// SQL一覧読込、追加、実行コマンドを設定します。
    /// </summary>
    public SqlExecutionViewModel(JsonSettingsStore settingsStore, DbOperationService operationService, SafetyCheckService safetyCheckService) : base(settingsStore)
    {
        this.operationService = operationService;
        this.safetyCheckService = safetyCheckService;
        SqlDirectory = Settings.DefaultSqlDirectory;
        LoadDirectoryCommand = new RelayCommand(LoadDirectory);
        AddManualSqlCommand = new RelayCommand(AddManualSql);
        ExecuteCommand = new AsyncRelayCommand(ExecuteAsync);
    }

    private void LoadDirectory()
    {
        SqlFiles.Clear();
        if (!Directory.Exists(SqlDirectory))
        {
            Message = "SQLフォルダが存在しません。";
            return;
        }
        foreach (var file in Directory.GetFiles(SqlDirectory, "*.sql").OrderBy(x => x))
        {
            SqlFiles.Add(new SqlFileItemViewModel(file));
        }
        Message = $"SQLファイルを{SqlFiles.Count}件読み込みました。";
    }

    private void AddManualSql()
    {
        if (!File.Exists(ManualSqlFile))
        {
            Message = "指定SQLファイルが存在しません。";
            return;
        }
        SqlFiles.Add(new SqlFileItemViewModel(ManualSqlFile));
    }

    private async Task ExecuteAsync()
    {
        try
        {
            var connection = RequireConnection();
            if (safetyCheckService.IsCautionDatabase(connection) && MessageBox.Show($"注意が必要なDBにSQLを実行します。\n対象DB: {connection.DatabaseName}\n続行しますか？", "SQL実行確認", MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes) return;
            var targets = SqlFiles.Where(x => x.IsSelected).Select(x => x.FilePath).ToList();
            if (targets.Count == 0) throw new InvalidOperationException("実行対象SQLを選択してください。");
            foreach (var target in targets)
            {
                var warnings = safetyCheckService.DetectDangerousSql(File.ReadAllText(target));
                if (warnings.Count > 0)
                {
                    Message = $"注意SQLを検出しました: {Path.GetFileName(target)} / {string.Join(", ", warnings)}";
                }
            }
            var histories = await operationService.ExecuteSqlFilesAsync(RequireApplication(), connection, RequireClient(), Password, targets, FailureBehavior);
            Message = $"SQL実行完了: 成功 {histories.Count(x => x.Result == ExecutionResult.Success)} 件 / 失敗 {histories.Count(x => x.Result == ExecutionResult.Failed)} 件";
        }
        catch (Exception ex)
        {
            Message = $"SQL実行失敗: {ex.Message}";
        }
    }
}

/// <summary>
/// SQLファイル項目ViewModel。
/// SQL実行画面に表示するファイル単位の選択状態を表します。
/// </summary>
public sealed class SqlFileItemViewModel : ObservableObject
{
    private bool isSelected = true;
    public string FilePath { get; }
    public string FileName => Path.GetFileName(FilePath);
    public long SizeBytes => new FileInfo(FilePath).Length;
    public DateTime UpdatedAt => File.GetLastWriteTime(FilePath);
    public bool IsSelected { get => isSelected; set => SetProperty(ref isSelected, value); }

    public SqlFileItemViewModel(string filePath)
    {
        FilePath = filePath;
    }
}
