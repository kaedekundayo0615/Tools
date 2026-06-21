using System.IO;
using System.Windows;
using PgDbOperator.Domain.Enums;
using PgDbOperator.Infrastructure;
using PgDbOperator.Services;

namespace PgDbOperator.ViewModels;

/// <summary>
/// バックアップ画面ViewModel。
/// pg_dumpによるバックアップ作成を制御します。
/// </summary>
public sealed class BackupViewModel : OperationViewModelBase
{
    private readonly DbOperationService operationService;
    private readonly SafetyCheckService safetyCheckService;
    private string outputDirectory = string.Empty;
    private string outputFileName = string.Empty;
    private DumpFormat dumpFormat = DumpFormat.Custom;

    public Array DumpFormats => Enum.GetValues(typeof(DumpFormat));
    public bool NoOwner { get; set; } = true;
    public bool NoPrivileges { get; set; } = true;
    public bool SchemaOnly { get; set; }
    public bool DataOnly { get; set; }
    public AsyncRelayCommand ExecuteCommand { get; }
    public RelayCommand GenerateFileNameCommand { get; }

    public string OutputDirectory { get => outputDirectory; set => SetProperty(ref outputDirectory, value); }
    public string OutputFileName { get => outputFileName; set => SetProperty(ref outputFileName, value); }
    public DumpFormat DumpFormat { get => dumpFormat; set => SetProperty(ref dumpFormat, value); }

    /// <summary>
    /// バックアップ画面ViewModel初期化処理。
    /// バックアップ実行コマンドと初期ファイル名を設定します。
    /// </summary>
    public BackupViewModel(JsonSettingsStore settingsStore, DbOperationService operationService, SafetyCheckService safetyCheckService) : base(settingsStore)
    {
        this.operationService = operationService;
        this.safetyCheckService = safetyCheckService;
        OutputDirectory = Settings.DefaultBackupDirectory;
        GenerateFileName();
        ExecuteCommand = new AsyncRelayCommand(ExecuteAsync);
        GenerateFileNameCommand = new RelayCommand(GenerateFileName);
    }

    private void GenerateFileName()
    {
        var dbName = SelectedConnection?.DatabaseName;
        if (string.IsNullOrWhiteSpace(dbName)) dbName = "database";
        OutputFileName = $"{dbName}_{DateTime.Now:yyyyMMdd_HHmmss}.{(DumpFormat == DumpFormat.Plain ? "sql" : "dump")}";
    }

    private async Task ExecuteAsync()
    {
        try
        {
            var app = RequireApplication();
            var connection = RequireConnection();
            var client = RequireClient();
            if (string.IsNullOrWhiteSpace(OutputDirectory)) throw new InvalidOperationException("出力先フォルダを入力してください。");
            if (string.IsNullOrWhiteSpace(OutputFileName)) GenerateFileName();
            var outputPath = Path.Combine(OutputDirectory, OutputFileName);
            if (safetyCheckService.IsCautionDatabase(connection) && MessageBox.Show($"注意が必要なDBにバックアップを実行します。\n対象DB: {connection.DatabaseName}\n続行しますか？", "実行確認", MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes) return;
            var history = await operationService.BackupAsync(app, connection, client, Password, outputPath, DumpFormat, NoOwner, NoPrivileges, SchemaOnly, DataOnly);
            Message = history.Result == ExecutionResult.Success ? $"バックアップ成功: {outputPath}" : $"バックアップ失敗: {history.StandardError}";
        }
        catch (Exception ex)
        {
            Message = $"バックアップ失敗: {ex.Message}";
        }
    }
}
