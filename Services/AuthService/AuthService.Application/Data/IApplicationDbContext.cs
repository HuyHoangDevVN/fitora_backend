
namespace AuthService.Application.Data;
public interface IApplicationDbContext
{
    DbSet<Key> Keys { get; }
    DbSet<ApplicationUser> Users { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
