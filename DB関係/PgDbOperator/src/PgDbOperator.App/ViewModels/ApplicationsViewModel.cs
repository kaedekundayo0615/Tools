using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using PgDbOperator.Domain.Enums;
using PgDbOperator.Domain.Models;
using PgDbOperator.Infrastructure;

namespace PgDbOperator.ViewModels;

/// <summary>
/// 対象アプリ管理ViewModel。
/// 管理対象アプリケーションの検索、追加、編集、複製、削除、保存を行います。
/// </summary>
public sealed class ApplicationsViewModel : ObservableObject
{
    private readonly JsonSettingsStore settingsStore;
    private AppSettings settings;
    private ManagedApplication? selectedApplication;
    private ManagedApplication? editingApplication;
    private ApplicationEditMode editMode = ApplicationEditMode.Add;
    private string searchText = string.Empty;
    private string message = string.Empty;
    private string dialogTitle = string.Empty;
    private bool isActionPopupOpen;
    private bool isEditDialogOpen;

    /// <summary>
    /// 対象アプリ管理ViewModel初期化処理。
    /// 保存済み対象アプリを読み込み、画面操作用コマンドを生成します。
    /// </summary>
    /// <param name="settingsStore">JSON設定ストア。</param>
    public ApplicationsViewModel(JsonSettingsStore settingsStore)
    {
        this.settingsStore = settingsStore;
        settings = settingsStore.Load();
        Applications = new ObservableCollection<ManagedApplication>(settings.Applications);
        FilteredApplications = new ObservableCollection<ManagedApplication>();
        ApplicationTypes = CreateApplicationTypes();
        EnabledStatusOptions = CreateEnabledStatusOptions();

        SearchCommand = new RelayCommand(ApplySearch);
        AddCommand = new RelayCommand(OpenAddDialog);
        EditCommand = new RelayCommand(OpenEditDialog, () => SelectedApplication != null);
        CopyCommand = new RelayCommand(OpenCopyDialog, () => SelectedApplication != null);
        SaveEditCommand = new RelayCommand(SaveEditDialog, () => EditingApplication != null);
        CloseEditCommand = new RelayCommand(CloseEditDialog);
        DeleteCommand = new RelayCommand(Delete, () => SelectedApplication != null);
        ToggleEnabledCommand = new RelayCommand(ToggleEnabled, () => SelectedApplication != null);
        SelectBackupDirectoryCommand = new RelayCommand(SelectBackupDirectory, () => EditingApplication != null);
        SelectSqlDirectoryCommand = new RelayCommand(SelectSqlDirectory, () => EditingApplication != null);

        ApplySearch();
    }

    /// <summary>
    /// 対象アプリ全件保持コレクション。
    /// 検索条件に関係なく保存対象の全件を保持します。
    /// </summary>
    public ObservableCollection<ManagedApplication> Applications { get; }

    /// <summary>
    /// 対象アプリ一覧表示コレクション。
    /// 検索条件に一致した対象アプリのみを保持します。
    /// </summary>
    public ObservableCollection<ManagedApplication> FilteredApplications { get; }

    /// <summary>
    /// アプリ種別選択候補。
    /// 編集ダイアログのコンボボックスに表示する日本語名付き候補を保持します。
    /// </summary>
    public IReadOnlyList<ApplicationTypeItem> ApplicationTypes { get; }

    /// <summary>
    /// 有効状態選択候補。
    /// 編集ダイアログのコンボボックスに表示する日本語名付き候補を保持します。
    /// </summary>
    public IReadOnlyList<EnabledStatusItem> EnabledStatusOptions { get; }

    /// <summary>
    /// 一覧選択中対象アプリ。
    /// 選択変更時に行操作ポップアップを表示します。
    /// </summary>
    public ManagedApplication? SelectedApplication
    {
        get => selectedApplication;
        set
        {
            if (!SetProperty(ref selectedApplication, value))
            {
                return;
            }

            if (value != null && !IsEditDialogOpen)
            {
                IsActionPopupOpen = false;
                IsActionPopupOpen = true;
            }
            else
            {
                IsActionPopupOpen = false;
            }

            RaiseSelectionCommandState();
            OnPropertyChanged(nameof(ToggleEnabledButtonText));
        }
    }

    /// <summary>
    /// 編集中対象アプリ。
    /// 追加、編集、コピー時の入力値を保持します。
    /// </summary>
    public ManagedApplication? EditingApplication
    {
        get => editingApplication;
        set => SetProperty(ref editingApplication, value);
    }

    /// <summary>
    /// 検索入力文字列。
    /// アプリ名、種別、フォルダ、説明の部分一致検索に利用します。
    /// </summary>
    public string SearchText
    {
        get => searchText;
        set => SetProperty(ref searchText, value);
    }

    /// <summary>
    /// 操作メッセージ。
    /// 保存結果や入力チェック結果を画面下部に表示します。
    /// </summary>
    public string Message
    {
        get => message;
        set => SetProperty(ref message, value);
    }

    /// <summary>
    /// 編集ダイアログタイトル。
    /// 追加、編集、コピーの操作種別に応じた表示名を保持します。
    /// </summary>
    public string DialogTitle
    {
        get => dialogTitle;
        set => SetProperty(ref dialogTitle, value);
    }

    /// <summary>
    /// 行操作ポップアップ表示状態。
    /// 一覧行選択時に編集、無効化、コピー、削除の操作を表示します。
    /// </summary>
    public bool IsActionPopupOpen
    {
        get => isActionPopupOpen;
        set
        {
            if (SetProperty(ref isActionPopupOpen, value))
            {
                OnPropertyChanged(nameof(ActionPopupVisibility));
            }
        }
    }

    /// <summary>
    /// 編集ダイアログ表示状態。
    /// 追加、編集、コピー操作時に入力ダイアログを表示します。
    /// </summary>
    public bool IsEditDialogOpen
    {
        get => isEditDialogOpen;
        set
        {
            if (SetProperty(ref isEditDialogOpen, value))
            {
                OnPropertyChanged(nameof(EditDialogVisibility));
            }
        }
    }

    /// <summary>
    /// 行操作ポップアップ表示制御値。
    /// XAMLのVisibilityバインド用に表示状態を返します。
    /// </summary>
    public Visibility ActionPopupVisibility => IsActionPopupOpen ? Visibility.Visible : Visibility.Collapsed;

    /// <summary>
    /// 編集ダイアログ表示制御値。
    /// XAMLのVisibilityバインド用に表示状態を返します。
    /// </summary>
    public Visibility EditDialogVisibility => IsEditDialogOpen ? Visibility.Visible : Visibility.Collapsed;

    /// <summary>
    /// 有効切替ボタン表示文字列。
    /// 選択中アプリの状態に応じて無効化または有効化を返します。
    /// </summary>
    public string ToggleEnabledButtonText => SelectedApplication?.IsEnabled == true ? "無効化" : "有効化";

    /// <summary>
    /// 行操作ポップアップ表示処理。
    /// 対象アプリを選択状態にして、操作ポップアップを一度閉じてから再表示します。
    /// </summary>
    /// <param name="application">操作対象アプリ。</param>
    public void ShowActionPopup(ManagedApplication application)
    {
        if (IsEditDialogOpen)
        {
            return;
        }

        if (!ReferenceEquals(SelectedApplication, application))
        {
            SelectedApplication = application;
        }

        IsActionPopupOpen = false;
        IsActionPopupOpen = true;
    }

    public RelayCommand SearchCommand { get; }
    public RelayCommand AddCommand { get; }
    public RelayCommand EditCommand { get; }
    public RelayCommand CopyCommand { get; }
    public RelayCommand SaveEditCommand { get; }
    public RelayCommand CloseEditCommand { get; }
    public RelayCommand DeleteCommand { get; }
    public RelayCommand ToggleEnabledCommand { get; }
    public RelayCommand SelectBackupDirectoryCommand { get; }
    public RelayCommand SelectSqlDirectoryCommand { get; }

    /// <summary>
    /// 検索適用処理。
    /// 検索文字列に一致する対象アプリのみ一覧に再設定します。
    /// </summary>
    private void ApplySearch()
    {
        var keyword = SearchText.Trim();
        var filtered = string.IsNullOrWhiteSpace(keyword)
            ? Applications.ToList()
            : Applications.Where(app => IsMatchSearchKeyword(app, keyword)).ToList();

        FilteredApplications.Clear();
        foreach (var app in filtered)
        {
            FilteredApplications.Add(app);
        }

        if (SelectedApplication != null && !FilteredApplications.Contains(SelectedApplication))
        {
            SelectedApplication = null;
        }

        Message = $"検索結果：{FilteredApplications.Count}件";
    }

    /// <summary>
    /// 追加ダイアログ表示処理。
    /// 初期値を設定した編集用対象アプリを生成します。
    /// </summary>
    private void OpenAddDialog()
    {
        editMode = ApplicationEditMode.Add;
        DialogTitle = "対象アプリ登録";
        EditingApplication = new ManagedApplication
        {
            Name = string.Empty,
            ApplicationType = ApplicationType.WebSystem,
            IsEnabled = true,
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now
        };
        OpenEditDialogCore();
    }

    /// <summary>
    /// 編集ダイアログ表示処理。
    /// 選択中対象アプリの入力用コピーを生成します。
    /// </summary>
    private void OpenEditDialog()
    {
        if (SelectedApplication == null)
        {
            return;
        }

        editMode = ApplicationEditMode.Edit;
        DialogTitle = "対象アプリ編集";
        EditingApplication = CloneApplication(SelectedApplication, false);
        OpenEditDialogCore();
    }

    /// <summary>
    /// コピーダイアログ表示処理。
    /// 選択中対象アプリを複製して新規登録用入力値に設定します。
    /// </summary>
    private void OpenCopyDialog()
    {
        if (SelectedApplication == null)
        {
            return;
        }

        editMode = ApplicationEditMode.Copy;
        DialogTitle = "対象アプリコピー登録";
        EditingApplication = CloneApplication(SelectedApplication, true);
        OpenEditDialogCore();
    }

    /// <summary>
    /// 編集ダイアログ共通表示処理。
    /// 行操作ポップアップを閉じ、編集ダイアログを表示します。
    /// </summary>
    private void OpenEditDialogCore()
    {
        IsActionPopupOpen = false;
        IsEditDialogOpen = true;
        Message = string.Empty;
        SaveEditCommand.RaiseCanExecuteChanged();
        SelectBackupDirectoryCommand.RaiseCanExecuteChanged();
        SelectSqlDirectoryCommand.RaiseCanExecuteChanged();
    }

    /// <summary>
    /// 編集ダイアログ保存処理。
    /// 入力内容を検証し、追加、編集、コピーの操作別に一覧へ反映します。
    /// </summary>
    private void SaveEditDialog()
    {
        if (EditingApplication == null)
        {
            return;
        }

        if (!ValidateEditingApplication(EditingApplication, out var validationMessage))
        {
            Message = validationMessage;
            return;
        }

        NormalizeEditingApplication(EditingApplication);
        EditingApplication.UpdatedAt = DateTime.Now;

        if (editMode == ApplicationEditMode.Edit && SelectedApplication != null)
        {
            ApplyApplicationValues(SelectedApplication, EditingApplication);
        }
        else
        {
            EditingApplication.Id = Guid.NewGuid();
            EditingApplication.CreatedAt = DateTime.Now;
            Applications.Add(EditingApplication);
            SelectedApplication = EditingApplication;
        }

        SaveSettings();
        ApplySearch();
        IsEditDialogOpen = false;
        EditingApplication = null;
        IsActionPopupOpen = SelectedApplication != null;
        Message = "対象アプリ設定を保存しました。";
    }

    /// <summary>
    /// 編集ダイアログ終了処理。
    /// 入力内容を破棄して一覧画面へ戻ります。
    /// </summary>
    private void CloseEditDialog()
    {
        IsEditDialogOpen = false;
        EditingApplication = null;
        IsActionPopupOpen = SelectedApplication != null;
        Message = string.Empty;
    }

    /// <summary>
    /// 削除処理。
    /// 選択中対象アプリを一覧から削除して保存します。
    /// </summary>
    private void Delete()
    {
        if (SelectedApplication == null)
        {
            return;
        }

        var deleteTarget = SelectedApplication;
        Applications.Remove(deleteTarget);
        FilteredApplications.Remove(deleteTarget);
        SelectedApplication = FilteredApplications.FirstOrDefault();
        SaveSettings();
        ApplySearch();
        Message = "対象アプリを削除しました。";
    }

    /// <summary>
    /// 有効状態切替処理。
    /// 選択中対象アプリの有効/無効を反転して保存します。
    /// </summary>
    private void ToggleEnabled()
    {
        if (SelectedApplication == null)
        {
            return;
        }

        SelectedApplication.IsEnabled = !SelectedApplication.IsEnabled;
        SelectedApplication.UpdatedAt = DateTime.Now;
        SaveSettings();
        ApplySearch();
        OnPropertyChanged(nameof(ToggleEnabledButtonText));
        Message = SelectedApplication.IsEnabled ? "対象アプリを有効化しました。" : "対象アプリを無効化しました。";
    }

    /// <summary>
    /// 標準バックアップ先選択処理。
    /// フォルダ選択ダイアログで選択されたパスを編集中対象アプリへ設定します。
    /// </summary>
    private void SelectBackupDirectory()
    {
        SelectDirectory(path =>
        {
            var edited = CloneApplication(EditingApplication!, false);
            edited.DefaultBackupDirectory = path;
            EditingApplication = edited;
        });
    }

    /// <summary>
    /// 実行SQLパス選択処理。
    /// フォルダ選択ダイアログで選択されたパスを編集中対象アプリへ設定します。
    /// </summary>
    private void SelectSqlDirectory()
    {
        SelectDirectory(path =>
        {
            var edited = CloneApplication(EditingApplication!, false);
            edited.DefaultSqlDirectory = path;
            EditingApplication = edited;
        });
    }

    /// <summary>
    /// フォルダ選択共通処理。
    /// Windows標準のフォルダ選択画面を表示し、選択パスを呼び出し元に返します。
    /// </summary>
    /// <param name="applyPath">選択パス反映処理。</param>
    private void SelectDirectory(Action<string> applyPath)
    {
        if (EditingApplication == null)
        {
            return;
        }

        using var dialog = new System.Windows.Forms.FolderBrowserDialog
        {
            Description = "フォルダを選択してください。",
            UseDescriptionForTitle = true,
            ShowNewFolderButton = true
        };

        if (dialog.ShowDialog() != System.Windows.Forms.DialogResult.OK)
        {
            return;
        }

        applyPath(dialog.SelectedPath);
    }

    /// <summary>
    /// 入力チェック処理。
    /// 登録編集ダイアログの必須、文字数、重複、フォルダ存在を検証します。
    /// </summary>
    /// <param name="application">検証対象アプリ。</param>
    /// <param name="validationMessage">検証エラーメッセージ。</param>
    /// <returns>検証結果。正常な場合はtrue。</returns>
    private bool ValidateEditingApplication(ManagedApplication application, out string validationMessage)
    {
        var applicationName = application.Name.Trim();
        var backupDirectory = application.DefaultBackupDirectory.Trim();
        var sqlDirectory = application.DefaultSqlDirectory.Trim();
        var description = application.Description.Trim();

        if (string.IsNullOrWhiteSpace(applicationName))
        {
            validationMessage = "アプリ名称を入力してください。";
            return false;
        }

        if (applicationName.Length > 100)
        {
            validationMessage = "アプリ名称は100文字以内で入力してください。";
            return false;
        }

        if (IsDuplicateApplicationName(applicationName, application.Id))
        {
            validationMessage = "同じアプリ名称が既に登録されています。";
            return false;
        }

        if (!ApplicationTypes.Any(item => item.Value == application.ApplicationType))
        {
            validationMessage = "種別を選択してください。";
            return false;
        }

        if (!ValidateDirectoryPath(backupDirectory, "標準バックアップ", out validationMessage))
        {
            return false;
        }

        if (!ValidateDirectoryPath(sqlDirectory, "実行SQLパス", out validationMessage))
        {
            return false;
        }

        if (description.Length > 1000)
        {
            validationMessage = "説明は1000文字以内で入力してください。";
            return false;
        }

        validationMessage = string.Empty;
        return true;
    }

    /// <summary>
    /// 入力値正規化処理。
    /// 保存前に前後空白の除去とアプリ種別の正規化を行います。
    /// </summary>
    /// <param name="application">正規化対象アプリ。</param>
    private static void NormalizeEditingApplication(ManagedApplication application)
    {
        application.Name = application.Name.Trim();
        application.DefaultBackupDirectory = application.DefaultBackupDirectory.Trim();
        application.DefaultSqlDirectory = application.DefaultSqlDirectory.Trim();
        application.Description = application.Description.Trim();
        application.ApplicationType = NormalizeApplicationType(application.ApplicationType);
    }

    /// <summary>
    /// アプリ名称重複判定処理。
    /// 編集中の同一IDを除外し、同名の対象アプリが既に存在するか判定します。
    /// </summary>
    /// <param name="applicationName">アプリ名称。</param>
    /// <param name="editingApplicationId">編集中アプリID。</param>
    /// <returns>重複している場合はtrue。</returns>
    private bool IsDuplicateApplicationName(string applicationName, Guid editingApplicationId)
    {
        return Applications.Any(app => app.Id != editingApplicationId
            && string.Equals(app.Name.Trim(), applicationName, StringComparison.CurrentCultureIgnoreCase));
    }

    /// <summary>
    /// フォルダパスチェック処理。
    /// 未入力は許容し、入力されている場合はパス形式とフォルダ存在を検証します。
    /// </summary>
    /// <param name="directoryPath">検証対象フォルダパス。</param>
    /// <param name="itemName">画面項目名。</param>
    /// <param name="validationMessage">検証エラーメッセージ。</param>
    /// <returns>検証結果。正常な場合はtrue。</returns>
    private static bool ValidateDirectoryPath(string directoryPath, string itemName, out string validationMessage)
    {
        if (string.IsNullOrWhiteSpace(directoryPath))
        {
            validationMessage = string.Empty;
            return true;
        }

        if (directoryPath.IndexOfAny(Path.GetInvalidPathChars()) >= 0)
        {
            validationMessage = $"{itemName}に使用できない文字が含まれています。";
            return false;
        }

        if (!Path.IsPathFullyQualified(directoryPath))
        {
            validationMessage = $"{itemName}は絶対パスで入力してください。";
            return false;
        }

        if (!Directory.Exists(directoryPath))
        {
            validationMessage = $"{itemName}には存在するフォルダを指定してください。";
            return false;
        }

        validationMessage = string.Empty;
        return true;
    }

    /// <summary>
    /// 設定保存処理。
    /// 画面上の対象アプリ一覧を設定JSONへ保存します。
    /// </summary>
    private void SaveSettings()
    {
        settings.Applications = Applications.ToList();
        settingsStore.Save(settings);
    }

    /// <summary>
    /// 検索条件一致判定処理。
    /// 対象アプリの主要項目に検索文字列が含まれるか判定します。
    /// </summary>
    /// <param name="app">判定対象アプリ。</param>
    /// <param name="keyword">検索文字列。</param>
    /// <returns>一致する場合はtrue。</returns>
    private static bool IsMatchSearchKeyword(ManagedApplication app, string keyword)
    {
        return Contains(app.Name, keyword)
            || Contains(app.ApplicationTypeDisplayName, keyword)
            || Contains(app.DefaultBackupDirectory, keyword)
            || Contains(app.DefaultSqlDirectory, keyword)
            || Contains(app.Description, keyword)
            || Contains(app.IsEnabledDisplayName, keyword);
    }

    /// <summary>
    /// 文字列部分一致判定処理。
    /// 大文字小文字を区別せず対象文字列に検索文字列が含まれるか判定します。
    /// </summary>
    /// <param name="value">対象文字列。</param>
    /// <param name="keyword">検索文字列。</param>
    /// <returns>含まれる場合はtrue。</returns>
    private static bool Contains(string value, string keyword)
    {
        return value.Contains(keyword, StringComparison.CurrentCultureIgnoreCase);
    }

    /// <summary>
    /// 対象アプリ複製処理。
    /// 編集用またはコピー登録用に対象アプリの値を複製します。
    /// </summary>
    /// <param name="source">複製元対象アプリ。</param>
    /// <param name="forCopy">コピー登録用の場合はtrue。</param>
    /// <returns>複製した対象アプリ。</returns>
    private static ManagedApplication CloneApplication(ManagedApplication source, bool forCopy)
    {
        return new ManagedApplication
        {
            Id = source.Id,
            Name = forCopy ? $"{source.Name}_コピー" : source.Name,
            ApplicationType = NormalizeApplicationType(source.ApplicationType),
            Description = source.Description,
            DefaultBackupDirectory = source.DefaultBackupDirectory,
            DefaultSqlDirectory = source.DefaultSqlDirectory,
            IsEnabled = source.IsEnabled,
            CreatedAt = source.CreatedAt,
            UpdatedAt = source.UpdatedAt
        };
    }

    /// <summary>
    /// アプリ種別正規化処理。
    /// 旧分類や画面非表示分類を現在の選択候補へ変換します。
    /// </summary>
    /// <param name="applicationType">変換対象アプリ種別。</param>
    /// <returns>画面選択候補内のアプリ種別。</returns>
    private static ApplicationType NormalizeApplicationType(ApplicationType applicationType)
    {
        return applicationType switch
        {
            ApplicationType.WebSystem => ApplicationType.WebSystem,
            ApplicationType.DesktopApplication => ApplicationType.DesktopApplication,
            ApplicationType.CompanyServer => ApplicationType.CompanyServer,
            ApplicationType.HomePage => ApplicationType.WebSystem,
            _ => ApplicationType.Other
        };
    }

    /// <summary>
    /// 対象アプリ値反映処理。
    /// 編集ダイアログの入力値を既存対象アプリへ上書きします。
    /// </summary>
    /// <param name="target">反映先対象アプリ。</param>
    /// <param name="source">入力値保持対象アプリ。</param>
    private static void ApplyApplicationValues(ManagedApplication target, ManagedApplication source)
    {
        target.Name = source.Name;
        target.ApplicationType = source.ApplicationType;
        target.Description = source.Description;
        target.DefaultBackupDirectory = source.DefaultBackupDirectory;
        target.DefaultSqlDirectory = source.DefaultSqlDirectory;
        target.IsEnabled = source.IsEnabled;
        target.UpdatedAt = source.UpdatedAt;
    }

    /// <summary>
    /// アプリ種別候補生成処理。
    /// 画面に表示する日本語名称と保存するEnum値の対応を生成します。
    /// </summary>
    /// <returns>アプリ種別候補一覧。</returns>
    private static IReadOnlyList<ApplicationTypeItem> CreateApplicationTypes()
    {
        return new List<ApplicationTypeItem>
        {
            new(ApplicationType.WebSystem, "WEBシステム"),
            new(ApplicationType.DesktopApplication, "デスクトップアプリ"),
            new(ApplicationType.CompanyServer, "自社サーバ"),
            new(ApplicationType.Other, "その他")
        };
    }

    /// <summary>
    /// 有効状態候補生成処理。
    /// 画面に表示する日本語名称と保存するbool値の対応を生成します。
    /// </summary>
    /// <returns>有効状態候補一覧。</returns>
    private static IReadOnlyList<EnabledStatusItem> CreateEnabledStatusOptions()
    {
        return new List<EnabledStatusItem>
        {
            new(true, "有効"),
            new(false, "無効")
        };
    }

    /// <summary>
    /// 選択連動コマンド状態更新処理。
    /// 選択中アプリが必要な操作ボタンの実行可否を再評価させます。
    /// </summary>
    private void RaiseSelectionCommandState()
    {
        EditCommand.RaiseCanExecuteChanged();
        CopyCommand.RaiseCanExecuteChanged();
        DeleteCommand.RaiseCanExecuteChanged();
        ToggleEnabledCommand.RaiseCanExecuteChanged();
    }
}

/// <summary>
/// アプリ種別選択候補。
/// 画面表示名と保存用Enum値の対応を表します。
/// </summary>
/// <param name="Value">アプリ種別値。</param>
/// <param name="DisplayName">画面表示名。</param>
public sealed class ApplicationTypeItem
{
    /// <summary>
    /// アプリ種別選択候補初期化処理。
    /// 保存するEnum値と画面表示名を保持します。
    /// </summary>
    /// <param name="value">アプリ種別値。</param>
    /// <param name="displayName">画面表示名。</param>
    public ApplicationTypeItem(ApplicationType value, string displayName)
    {
        Value = value;
        DisplayName = displayName;
    }

    /// <summary>
    /// アプリ種別値。
    /// JSON保存時に利用するEnum値を表します。
    /// </summary>
    public ApplicationType Value { get; }

    /// <summary>
    /// 画面表示名。
    /// コンボボックスに表示する日本語名称を表します。
    /// </summary>
    public string DisplayName { get; }
}

/// <summary>
/// 有効状態選択候補。
/// 画面表示名と保存用bool値の対応を表します。
/// </summary>
public sealed class EnabledStatusItem
{
    /// <summary>
    /// 有効状態選択候補初期化処理。
    /// 保存するbool値と画面表示名を保持します。
    /// </summary>
    /// <param name="value">有効状態値。</param>
    /// <param name="displayName">画面表示名。</param>
    public EnabledStatusItem(bool value, string displayName)
    {
        Value = value;
        DisplayName = displayName;
    }

    /// <summary>
    /// 有効状態値。
    /// JSON保存時に利用するbool値を表します。
    /// </summary>
    public bool Value { get; }

    /// <summary>
    /// 画面表示名。
    /// コンボボックスに表示する日本語名称を表します。
    /// </summary>
    public string DisplayName { get; }
}

/// <summary>
/// 対象アプリ編集モード。
/// 編集ダイアログの保存時に追加、編集、コピーのどの処理かを判定します。
/// </summary>
internal enum ApplicationEditMode
{
    Add,
    Edit,
    Copy
}
