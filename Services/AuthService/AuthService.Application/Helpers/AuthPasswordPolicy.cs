using System.Text.RegularExpressions;

namespace AuthService.Application.Helpers;

/// <summary>
/// Chính sách mật khẩu 4 yếu tố (Claude.md 13.6.4 / 15.8) — server-side,
/// đồng bộ với FE `src/utils/passwordPolicy.ts`:
/// tối thiểu 8 ký tự, có chữ hoa, chữ thường, chữ số và ký tự đặc biệt.
/// </summary>
public static class AuthPasswordPolicy
{
    private static readonly Regex Upper = new("[A-Z]", RegexOptions.Compiled);
    private static readonly Regex Lower = new("[a-z]", RegexOptions.Compiled);
    private static readonly Regex Digit = new("[0-9]", RegexOptions.Compiled);
    private static readonly Regex Special = new("[^A-Za-z0-9]", RegexOptions.Compiled);

    public static bool Matches(string? password)
    {
        if (string.IsNullOrEmpty(password) || password.Length < 8) return false;
        return Upper.IsMatch(password)
            && Lower.IsMatch(password)
            && Digit.IsMatch(password)
            && Special.IsMatch(password);
    }
}
