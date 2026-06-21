using System.Security.Cryptography;
using System.Text;

namespace PgDbOperator.Infrastructure;

/// <summary>
/// DPAPIパスワード保護サービス。
/// Windowsユーザー単位でDBパスワードを暗号化・復号します。
/// </summary>
public sealed class DpapiPasswordProtector
{
    /// <summary>
    /// 暗号化処理。
    /// 平文パスワードをWindows DPAPIで暗号化します。
    /// </summary>
    /// <param name="plainText">平文パスワード。</param>
    /// <returns>Base64形式の暗号化文字列。</returns>
    public string Protect(string plainText)
    {
        if (string.IsNullOrEmpty(plainText))
        {
            return string.Empty;
        }

        var bytes = Encoding.UTF8.GetBytes(plainText);
        var protectedBytes = ProtectedData.Protect(bytes, null, DataProtectionScope.CurrentUser);
        return Convert.ToBase64String(protectedBytes);
    }

    /// <summary>
    /// 復号処理。
    /// Base64形式の暗号化文字列を平文パスワードへ復号します。
    /// </summary>
    /// <param name="protectedText">Base64形式の暗号化文字列。</param>
    /// <returns>平文パスワード。</returns>
    public string Unprotect(string protectedText)
    {
        if (string.IsNullOrEmpty(protectedText))
        {
            return string.Empty;
        }

        var protectedBytes = Convert.FromBase64String(protectedText);
        var bytes = ProtectedData.Unprotect(protectedBytes, null, DataProtectionScope.CurrentUser);
        return Encoding.UTF8.GetString(bytes);
    }
}
