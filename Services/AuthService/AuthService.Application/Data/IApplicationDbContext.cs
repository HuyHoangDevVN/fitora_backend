namespace AuthService.Application.Data;

public interface IApplicationDbContext
{
    DbSet<Key> Keys { get; }
    DbSet<ApplicationUser> Users { get; }
    DbSet<PasswordResetToken> PasswordResetTokens { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}