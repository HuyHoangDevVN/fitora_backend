using Microsoft.AspNetCore.DataProtection;

namespace AuthService.Application.Auths.TwoFactor;

public class TotpSecretProtector : ITotpSecretProtector
{
    private readonly IDataProtector _protector;
    public TotpSecretProtector(IDataProtectionProvider provider)
    {
        _protector = provider.CreateProtector("Fitora.TotpSecret.v1");
    }
    public string Protect(string plaintext) => _protector.Protect(plaintext);
    public string Unprotect(string protectedData)
    {
        try { return _protector.Unprotect(protectedData); }
        catch { return protectedData; } // fallback: legacy plaintext rows before migration
    }
}
