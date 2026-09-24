using System.Security.Cryptography;
using System.Text;

namespace AuthService.Application.Auths.TwoFactor;

public static class TotpHelper
{
    private const string Base32Chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";

    public static string GenerateSecret(int length = 32)
    {
        var bytes = System.Security.Cryptography.RandomNumberGenerator.GetBytes(length * 5 / 8 + 1);
        var sb = new System.Text.StringBuilder();
        int bits = 0, val = 0;
        foreach (var b in bytes)
        {
            val = (val << 8) | b;
            bits += 8;
            while (bits >= 5) { bits -= 5; sb.Append(Base32Chars[(val >> bits) & 31]); }
        }
        while (sb.Length < length) sb.Append(Base32Chars[System.Security.Cryptography.RandomNumberGenerator.GetInt32(32)]);
        return sb.ToString()[..length];
    }

    public static bool Verify(string base32Secret, string code, int window = 1)
    {
        if (code.Length != 6 || !code.All(char.IsDigit)) return false;
        var key = Base32Decode(base32Secret);
        var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds() / 30;
        for (long c = now - window; c <= now + window; c++)
            if (ComputeTotp(key, c) == code) return true;
        return false;
    }

    private static string ComputeTotp(byte[] key, long counter)
    {
        var counterBytes = BitConverter.GetBytes(counter);
        if (BitConverter.IsLittleEndian) Array.Reverse(counterBytes);
        using var hmac = new System.Security.Cryptography.HMACSHA1(key);
        var hash = hmac.ComputeHash(counterBytes);
        int offset = hash[^1] & 0x0F;
        int binary = ((hash[offset] & 0x7F) << 24) | (hash[offset + 1] << 16) | (hash[offset + 2] << 8) | hash[offset + 3];
        return (binary % 1_000_000).ToString("D6");
    }

    private static byte[] Base32Decode(string input)
    {
        input = input.TrimEnd('=').ToUpperInvariant();
        var output = new List<byte>();
        int bits = 0, val = 0;
        foreach (var c in input)
        {
            int idx = Base32Chars.IndexOf(c);
            if (idx < 0) continue;
            val = (val << 5) | idx;
            bits += 5;
            if (bits >= 8) { bits -= 8; output.Add((byte)((val >> bits) & 0xFF)); }
        }
        return output.ToArray();
    }

    public static string BuildQrUrl(string secret, string email, string issuer = "Fitora")
        => $"otpauth://totp/{Uri.EscapeDataString(issuer)}:{Uri.EscapeDataString(email)}?secret={secret}&issuer={Uri.EscapeDataString(issuer)}&algorithm=SHA1&digits=6&period=30";

    public static List<string> GenerateRecoveryCodes(int count = 10)
    {
        const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
        var rng = System.Security.Cryptography.RandomNumberGenerator.Create();
        var codes = new List<string>();
        for (int i = 0; i < count; i++)
        {
            var b = new byte[8];
            rng.GetBytes(b);
            codes.Add(new string(b.Select(x => chars[x % chars.Length]).ToArray()));
        }
        return codes;
    }

    public static string HashCode(string code)
    {
        var bytes = System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(code));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}
