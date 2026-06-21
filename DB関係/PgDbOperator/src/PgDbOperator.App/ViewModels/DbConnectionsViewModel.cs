using System.Collections.ObjectModel;
using PgDbOperator.Domain.Enums;
using PgDbOperator.Domain.Models;
using PgDbOperator.Infrastructure;
using PgDbOperator.Services;

namespace PgDbOperator.ViewModels;

/// <summary>
/// DB接続管理ViewModel。
/// 対象アプリに紐づくPostgreSQL接続情報を管理します。
/// </summary>
public sealed class DbConnectionsViewModel : ObservableObject
{
    private readonly JsonSettingsStore settingsStore;
    private readonly ConnectionTestService connectionTestService;
    private readonly DpapiPasswordProtector passwordProtector;
    private AppSettings settings;
    private DbConnectionProfile? selectedConnection;
    private string password = string.Empty;
    private string message = string.Empty;

    public ObservableCollection<ManagedApplication> Applications { get; }
    public ObservableCollection<DbConnectionProfile> Connections { get; }
    public Array EnvironmentTypes => Enum.GetValues(typeof(EnvironmentType));
    public Array SafetyLevels => Enum.GetValues(typeof(SafetyLevel));
    public Array PasswordSaveTypes => Enum.GetValues(typeof(PasswordSaveType));
    public RelayCommand AddCommand { get; }
    public RelayCommand SaveCommand { get; }
    public RelayCommand DeleteCommand { get; }
    public AsyncRelayCommand TestCommand { get; }

    public DbConnectionProfile? SelectedConnection
    {
        get => selectedConnection;
        set => SetProperty(ref selectedConnection, value);
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
    /// DB接続管理ViewModel初期化処理。
    /// 保存済み接続設定と対象アプリを読み込みます。
    /// </summary>
    public DbConnectionsViewModel(JsonSettingsStore settingsStore, ConnectionTestService connectionTestService, DpapiPasswordProtector passwordProtector)
    {
        this.settingsStore = settingsStore;
        this.connectionTestService = connectionTestService;
        this.passwordProtector = passwordProtector;
        settings = settingsStore.Load();
        Applications = new ObservableCollection<ManagedApplication>(settings.Applications);
        Connections = new ObservableCollection<DbConnectionProfile>(settings.DbConnections);
        AddCommand = new RelayCommand(Add);
        SaveCommand = new RelayCommand(Save);
        DeleteCommand = new RelayCommand(Delete, () => SelectedConnection != null);
        TestCommand = new AsyncRelayCommand(TestAsync, () => SelectedConnection != null);
    }

    private void Add()
    {
        var appId = Applications.FirstOrDefault()?.Id ?? Guid.Empty;
        var connection = new DbConnectionProfile { ApplicationId = appId, Name = "新規DB", DatabaseName = "postgres", UserName = "postgres" };
        Connections.Add(connection);
        SelectedConnection = connection;
    }

    private void Save()
    {
        if (SelectedConnection != null && SelectedConnection.PasswordSaveType == PasswordSaveType.WindowsDpapi && !string.IsNullOrEmpty(Password))
        {
            SelectedConnection.EncryptedPassword = passwordProtector.Protect(Password);
            Password = string.Empty;
        }
        settings.DbConnections = Connections.ToList();
        settingsStore.Save(settings);
        Message = "DB接続設定を保存しました。";
    }

    private void Delete()
    {
        if (SelectedConnection == null) return;
        Connections.Remove(SelectedConnection);
        SelectedConnection = Connections.FirstOrDefault();
        Save();
    }

    private async Task TestAsync()
    {
        if (SelectedConnection == null) return;
        try
        {
            Message = await connectionTestService.TestAsync(SelectedConnection, Password);
        }
        catch (Exception ex)
        {
            Message = $"接続失敗: {ex.Message}";
        }
    }
}
