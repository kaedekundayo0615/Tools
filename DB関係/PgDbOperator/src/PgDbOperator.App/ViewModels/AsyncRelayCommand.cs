using System.Windows.Input;

namespace PgDbOperator.ViewModels;

/// <summary>
/// 非同期汎用コマンド。
/// ViewModelから非同期処理を実行するためのICommand実装です。
/// </summary>
public sealed class AsyncRelayCommand : ICommand
{
    private readonly Func<Task> executeAsync;
    private readonly Func<bool>? canExecute;
    private bool isExecuting;

    public event EventHandler? CanExecuteChanged;

    /// <summary>
    /// 非同期コマンド初期化処理。
    /// 非同期実行処理と実行可否判定を保持します。
    /// </summary>
    /// <param name="executeAsync">非同期実行処理。</param>
    /// <param name="canExecute">実行可否判定。</param>
    public AsyncRelayCommand(Func<Task> executeAsync, Func<bool>? canExecute = null)
    {
        this.executeAsync = executeAsync;
        this.canExecute = canExecute;
    }

    /// <summary>
    /// 実行可否判定処理。
    /// 二重実行を防ぎながらコマンドを実行可能か返します。
    /// </summary>
    /// <param name="parameter">コマンド引数。</param>
    /// <returns>実行可能な場合はtrue。</returns>
    public bool CanExecute(object? parameter) => !isExecuting && (canExecute?.Invoke() ?? true);

    /// <summary>
    /// 非同期コマンド実行処理。
    /// 実行中フラグを制御して非同期処理を呼び出します。
    /// </summary>
    /// <param name="parameter">コマンド引数。</param>
    public async void Execute(object? parameter)
    {
        if (!CanExecute(parameter)) return;
        try
        {
            isExecuting = true;
            RaiseCanExecuteChanged();
            await executeAsync();
        }
        finally
        {
            isExecuting = false;
            RaiseCanExecuteChanged();
        }
    }

    /// <summary>
    /// 実行可否変更通知処理。
    /// 画面にコマンド状態の再評価を通知します。
    /// </summary>
    public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
}
