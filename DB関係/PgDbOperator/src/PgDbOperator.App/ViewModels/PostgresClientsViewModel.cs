using System.Collections.ObjectModel;
using PgDbOperator.Domain.Models;
using PgDbOperator.Infrastructure;

namespace PgDbOperator.ViewModels;

/// <summary>
/// PostgreSQLクライアント設定ViewModel。
/// pg_dump、pg_restore、psqlのパス設定を管理します。
/// </summary>
public sealed class PostgresClientsViewModel : ObservableObject
{
    private readonly JsonSettingsStore settingsStore;
    private AppSettings settings;
    private PostgresClientProfile? selectedClient;
    private string message = string.Empty;

    public ObservableCollection<PostgresClientProfile> Clients { get; }
    public RelayCommand AddCommand { get; }
    public RelayCommand ApplyBinCommand { get; }
    public RelayCommand SaveCommand { get; }
    public RelayCommand DeleteCommand { get; }

    public PostgresClientProfile? SelectedClient
    {
        get => selectedClient;
        set => SetProperty(ref selectedClient, value);
    }

    public string Message
    {
        get => message;
        set => SetProperty(ref message, value);
    }

    /// <summary>
    /// PostgreSQLクライアント設定ViewModel初期化処理。
    /// 保存済みクライアント設定を読み込みます。
    /// </summary>
    public PostgresClientsViewModel(JsonSettingsStore settingsStore)
    {
        this.settingsStore = settingsStore;
        settings = settingsStore.Load();
        Clients = new ObservableCollection<PostgresClientProfile>(settings.PostgresClients);
        AddCommand = new RelayCommand(Add);
        ApplyBinCommand = new RelayCommand(ApplyBin, () => SelectedClient != null);
        SaveCommand = new RelayCommand(Save);
        DeleteCommand = new RelayCommand(Delete, () => SelectedClient != null);
    }

    private void Add()
    {
        var client = new PostgresClientProfile { Name = "PostgreSQL", Version = "16" };
        Clients.Add(client);
        SelectedClient = client;
    }

    private void ApplyBin()
    {
        SelectedClient?.ApplyBinDirectory();
        OnPropertyChanged(nameof(SelectedClient));
        Message = "binフォルダからEXEパスを反映しました。";
    }

    private void Save()
    {
        if (Clients.Count(x => x.IsDefault) > 1)
        {
            Message = "デフォルト設定は1件のみ選択してください。";
            return;
        }
        settings.PostgresClients = Clients.ToList();
        settingsStore.Save(settings);
        Message = "PostgreSQL設定を保存しました。";
    }

    private void Delete()
    {
        if (SelectedClient == null) return;
        Clients.Remove(SelectedClient);
        SelectedClient = Clients.FirstOrDefault();
        Save();
    }
}
