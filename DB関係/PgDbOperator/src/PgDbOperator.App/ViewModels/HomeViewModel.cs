using System.Collections.ObjectModel;
using PgDbOperator.Domain.Models;
using PgDbOperator.Infrastructure;
using PgDbOperator.Services;

namespace PgDbOperator.ViewModels;

/// <summary>
/// ホーム画面ViewModel。
/// 登録済みアプリと直近実行履歴を表示します。
/// </summary>
public sealed class HomeViewModel : ObservableObject
{
    public ObservableCollection<ManagedApplication> Applications { get; }
    public ObservableCollection<ExecutionHistory> RecentHistories { get; }

    /// <summary>
    /// ホーム画面ViewModel初期化処理。
    /// 設定と履歴を読み込み、ダッシュボード表示用コレクションを作成します。
    /// </summary>
    public HomeViewModel(JsonSettingsStore settingsStore, ExecutionHistoryService historyService)
    {
        var settings = settingsStore.Load();
        Applications = new ObservableCollection<ManagedApplication>(settings.Applications.Where(x => x.IsEnabled));
        RecentHistories = new ObservableCollection<ExecutionHistory>(historyService.GetHistories().Take(20));
    }
}
