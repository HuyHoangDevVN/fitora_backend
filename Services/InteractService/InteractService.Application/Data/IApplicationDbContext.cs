namespace InteractService.Application.Data;

public interface IApplicationDbContext
{
    DbSet<Post> Posts { get; }
    DbSet<Comment> Comments { get; }
    DbSet<React> Reacts { get; }
    DbSet<Tag> Tags { get; }
    DbSet<Share> Shares { get; }
    DbSet<Report> Reports { get; }
    
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}