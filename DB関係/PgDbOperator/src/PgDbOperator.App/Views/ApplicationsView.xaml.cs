using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using PgDbOperator.Domain.Models;
using PgDbOperator.ViewModels;

namespace PgDbOperator.Views;

/// <summary>
/// ApplicationsView。
/// 対象アプリ管理画面の表示と、画面固有のクリック制御を行います。
/// </summary>
public partial class ApplicationsView : System.Windows.Controls.UserControl
{
    /// <summary>
    /// 画面初期化処理。
    /// XAMLコンポーネントを初期化します。
    /// </summary>
    public ApplicationsView()
    {
        InitializeComponent();
    }


    /// <summary>
    /// 一覧行押下後処理。
    /// 行選択確定後に操作ポップアップを開き直し、同じ行を再クリックした場合も必ず表示します。
    /// </summary>
    /// <param name="sender">イベント発生元。</param>
    /// <param name="e">マウス押下イベント情報。</param>
    private void ApplicationsDataGridRow_PreviewMouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        if (DataContext is not ApplicationsViewModel viewModel || viewModel.IsEditDialogOpen)
        {
            return;
        }

        if (sender is not DataGridRow row || row.DataContext is not ManagedApplication selectedApplication)
        {
            return;
        }

        ApplicationsDataGrid.SelectedItem = selectedApplication;
        Dispatcher.BeginInvoke(
            () =>
            {
                viewModel.ShowActionPopup(selectedApplication);
            },
            DispatcherPriority.Input);
    }

    /// <summary>
    /// ルート押下処理。
    /// 一覧行、操作ポップアップ、編集ダイアログ以外を押下した場合に操作ポップアップを閉じます。
    /// </summary>
    /// <param name="sender">イベント発生元。</param>
    /// <param name="e">マウス押下イベント情報。</param>
    private void AppsRoot_PreviewMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        if (DataContext is not ApplicationsViewModel viewModel || viewModel.IsEditDialogOpen)
        {
            return;
        }

        if (e.OriginalSource is not DependencyObject source)
        {
            viewModel.IsActionPopupOpen = false;
            return;
        }

        if (IsDescendantOfType<DataGridRow>(source) || IsDescendantOf(source, ActionPopupHost.Child))
        {
            return;
        }

        viewModel.IsActionPopupOpen = false;
    }

    /// <summary>
    /// 指定型親要素存在判定処理。
    /// 押下要素の親階層に指定型の要素が存在するか判定します。
    /// </summary>
    /// <typeparam name="T">判定対象の型。</typeparam>
    /// <param name="source">押下要素。</param>
    /// <returns>指定型の親要素が存在する場合はtrue。</returns>
    private static bool IsDescendantOfType<T>(DependencyObject source)
        where T : DependencyObject
    {
        var current = source;
        while (current != null)
        {
            if (current is T)
            {
                return true;
            }

            current = GetParent(current);
        }

        return false;
    }

    /// <summary>
    /// 指定親要素配下判定処理。
    /// 押下要素が指定した親要素の配下に存在するか判定します。
    /// </summary>
    /// <param name="source">押下要素。</param>
    /// <param name="parent">親要素。</param>
    /// <returns>指定親要素配下の場合はtrue。</returns>
    private static bool IsDescendantOf(DependencyObject source, DependencyObject? parent)
    {
        if (parent == null)
        {
            return false;
        }

        var current = source;
        while (current != null)
        {
            if (ReferenceEquals(current, parent))
            {
                return true;
            }

            current = GetParent(current);
        }

        return false;
    }

    /// <summary>
    /// 親要素取得処理。
    /// VisualTreeHelperで取得できない場合はFrameworkElementのParentを返します。
    /// </summary>
    /// <param name="source">対象要素。</param>
    /// <returns>親要素。存在しない場合はnull。</returns>
    private static DependencyObject? GetParent(DependencyObject source)
    {
        if (source is FrameworkElement frameworkElement && frameworkElement.Parent != null)
        {
            return frameworkElement.Parent;
        }

        return VisualTreeHelper.GetParent(source);
    }
}
