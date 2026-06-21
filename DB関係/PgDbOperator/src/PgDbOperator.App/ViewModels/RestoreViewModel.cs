using System.Windows;
using PgDbOperator.Domain.Enums;
using PgDbOperator.Infrastructure;
using PgDbOperator.Services;

namespace PgDbOperator.ViewModels;

/// <summary>
/// リストア画面ViewModel。
/// pg_restoreまたはpsqlによるDB復元を制御します。
/// </summary>
public sealed class RestoreViewModel : OperationViewModelBase
{
    private readonly DbOperationService operationService;
    private readonly SafetyCheckService safetyCheckService;
    private string inputFilePath = string.Empty;
    public bool Clean { get; set; }
    public bool IfExists { get; set; } = true;
    public bool NoOwner { get; set; } = true;
    public bool NoPrivileges { get; set; } = true;
    public AsyncRelayCommand ExecuteCommand { get; }

    public string InputFilePath { get => inputFilePath; set => SetProperty(ref inputFilePath, value); }

    /// <summary>
    /// リストア画面ViewModel初期化処理。
    /// リストア実行コマンドを設定します。
    /// </summary>
    public RestoreViewModel(JsonSettingsStore settingsStore, DbOperationService operationService, SafetyCheckService safetyCheckService) : base(settingsStore)
    {
        this.operationService = operationService;
        this.safetyCheckService = safetyCheckService;
        ExecuteCommand = new AsyncRelayCommand(ExecuteAsync);
    }

    private async Task ExecuteAsync()
    {
        try
        {
            var app = RequireApplication();
            var connection = RequireConnection();
            if (safetyCheckService.IsCautionDatabase(connection) && MessageBox.Show($"注意が必要なDBにリストアを実行します。\n対象DB: {connection.DatabaseName}\nこの操作は破壊的な変更になる可能性があります。続行しますか？", "リストア確認", MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes) return;
            var history = await operationService.RestoreAsync(app, connection, RequireClient(), Password, InputFilePath, Clean, IfExists, NoOwner, NoPrivileges);
            Message = history.Result == ExecutionResult.Success ? "リストア成功。" : $"リストア失敗: {history.StandardError}";
        }
        catch (Exception ex)
        {
            Message = $"リストア失敗: {ex.Message}";
        }
    }
}
