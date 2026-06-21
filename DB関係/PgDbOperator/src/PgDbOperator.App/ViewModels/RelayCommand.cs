using System.Windows.Input;

namespace PgDbOperator.ViewModels;

/// <summary>
/// 汎用コマンド。
/// ViewModelから画面操作を受け取るためのICommand実装です。
/// </summary>
public sealed class RelayCommand : ICommand
{
    private readonly Action execute;
    private readonly Func<bool>? canExecute;

    public event EventHandler? CanExecuteChanged;

    /// <summary>
    /// コマンド初期化処理。
    /// 実行処理と実行可否判定を保持します。
    /// </summary>
    /// <param name="execute">実行処理。</param>
    /// <param name="canExecute">実行可否判定。</param>
    public RelayCommand(Action execute, Func<bool>? canExecute = null)
    {
        this.execute = execute;
        this.canExecute = canExecute;
    }

    /// <summary>
    /// 実行可否判定処理。
    /// コマンドを実行可能か返します。
    /// </summary>
    /// <param name="parameter">コマンド引数。</param>
    /// <returns>実行可能な場合はtrue。</returns>
    public bool CanExecute(object? parameter) => canExecute?.Invoke() ?? true;

    /// <summary>
    /// コマンド実行処理。
    /// 保持している実行処理を呼び出します。
    /// </summary>
    /// <param name="parameter">コマンド引数。</param>
    public void Execute(object? parameter) => execute();

    /// <summary>
    /// 実行可否変更通知処理。
    /// 画面にコマンド状態の再評価を通知します。
    /// </summary>
    public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
}
