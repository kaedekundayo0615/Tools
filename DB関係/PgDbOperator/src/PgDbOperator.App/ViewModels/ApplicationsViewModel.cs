using System.Collections.ObjectModel;
using PgDbOperator.Domain.Enums;
using PgDbOperator.Domain.Models;
using PgDbOperator.Infrastructure;

namespace PgDbOperator.ViewModels;

/// <summary>
/// 対象アプリ管理ViewModel。
/// 管理対象アプリケーションの追加、編集、保存を行います。
/// </summary>
public sealed class ApplicationsViewModel : ObservableObject
{
    private readonly JsonSettingsStore settingsStore;
    private AppSettings settings;
    private ManagedApplication? selectedApplication;
    private string message = string.Empty;

    public ObservableCollection<ManagedApplication> Applications { get; }
    public Array ApplicationTypes => Enum.GetValues(typeof(ApplicationType));
    public RelayCommand AddCommand { get; }
    public RelayCommand SaveCommand { get; }
    public RelayCommand DeleteCommand { get; }

    public ManagedApplication? SelectedApplication
    {
        get => selectedApplication;
        set => SetProperty(ref selectedApplication, value);
    }

    public string Message
    {
        get => message;
        set => SetProperty(ref message, value);
    }

    /// <summary>
    /// 対象アプリ管理ViewModel初期化処理。
    /// 保存済み対象アプリを読み込みます。
    /// </summary>
    public ApplicationsViewModel(JsonSettingsStore settingsStore)
    {
        this.settingsStore = settingsStore;
        settings = settingsStore.Load();
        Applications = new ObservableCollection<ManagedApplication>(settings.Applications);
        AddCommand = new RelayCommand(Add);
        SaveCommand = new RelayCommand(Save);
        DeleteCommand = new RelayCommand(Delete, () => SelectedApplication != null);
    }

    private void Add()
    {
        var app = new ManagedApplication { Name = "新規アプリ", CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now };
        Applications.Add(app);
        SelectedApplication = app;
    }

    private void Save()
    {
        foreach (var app in Applications) app.UpdatedAt = DateTime.Now;
        settings.Applications = Applications.ToList();
        settingsStore.Save(settings);
        Message = "対象アプリ設定を保存しました。";
    }

    private void Delete()
    {
        if (SelectedApplication == null) return;
        Applications.Remove(SelectedApplication);
        SelectedApplication = Applications.FirstOrDefault();
        Save();
    }
}
