namespace AuthService.Application.Auths.TwoFactor;

/// <summary>Mã hóa/giải mã SecretKey at-rest qua ASP.NET DataProtection.</summary>
public interface ITotpSecretProtector
{
    string Protect(string plaintext);
    string Unprotect(string protectedData);
}
