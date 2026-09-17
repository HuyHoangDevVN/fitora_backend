namespace AuthService.Application.Data;

public interface IApplicationDbContext
{
    DbSet<Key> Keys { get; }
    DbSet<ApplicationUser> Users { get; }
    DbSet<PasswordResetToken> PasswordResetTokens { get; }
    DbSet<TotpSecret> TotpSecrets { get; }
    DbSet<RecoveryCode> RecoveryCodes { get; }
    DbSet<IdentityVerification> IdentityVerifications { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}