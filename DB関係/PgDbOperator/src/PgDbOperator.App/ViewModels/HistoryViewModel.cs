using System.Collections.ObjectModel;
using PgDbOperator.Domain.Models;
using PgDbOperator.Services;

namespace PgDbOperator.ViewModels;

/// <summary>
/// 実行履歴画面ViewModel。
/// 保存済み実行履歴の一覧表示を行います。
/// </summary>
public sealed class HistoryViewModel : ObservableObject
{
    private readonly ExecutionHistoryService historyService;
    private ExecutionHistory? selectedHistory;

    public ObservableCollection<ExecutionHistory> Histories { get; }
    public RelayCommand RefreshCommand { get; }

    public ExecutionHistory? SelectedHistory
    {
        get => selectedHistory;
        set => SetProperty(ref selectedHistory, value);
    }

    /// <summary>
    /// 実行履歴画面ViewModel初期化処理。
    /// 保存済み履歴を読み込みます。
    /// </summary>
    public HistoryViewModel(ExecutionHistoryService historyService)
    {
        this.historyService = historyService;
        Histories = new ObservableCollection<ExecutionHistory>();
        RefreshCommand = new RelayCommand(Refresh);
        Refresh();
    }

    private void Refresh()
    {
        Histories.Clear();
        foreach (var history in historyService.GetHistories())
        {
            Histories.Add(history);
        }
    }
}
