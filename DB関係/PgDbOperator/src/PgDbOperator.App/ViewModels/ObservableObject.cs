using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace PgDbOperator.ViewModels;

/// <summary>
/// 変更通知基底クラス。
/// ViewModelのプロパティ変更通知を共通化します。
/// </summary>
public abstract class ObservableObject : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// プロパティ値設定処理。
    /// 値が変更された場合のみ保持値を更新し、変更通知を発火します。
    /// </summary>
    /// <typeparam name="T">プロパティ型。</typeparam>
    /// <param name="field">保持フィールド。</param>
    /// <param name="value">設定値。</param>
    /// <param name="propertyName">プロパティ名。</param>
    /// <returns>変更された場合はtrue。</returns>
    protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
        {
            return false;
        }

        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }

    /// <summary>
    /// プロパティ変更通知処理。
    /// 指定プロパティの変更通知を発火します。
    /// </summary>
    /// <param name="propertyName">プロパティ名。</param>
    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
