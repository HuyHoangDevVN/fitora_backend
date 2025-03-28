using System.Reflection;
using InteractService.Application.Data;
using Microsoft.EntityFrameworkCore;

namespace InteractService.Infrastructure.Data;

public class ApplicationDbContext : DbContext, IApplicationDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<Post> Posts => Set<Post>();
    public DbSet<Comment> Comments => Set<Comment>();
    public DbSet<React> Reacts => Set<React>();
    public DbSet<Tag> Tags => Set<Tag>();
    public DbSet<Share> Shares => Set<Share>();
    public DbSet<Report> Reports => Set<Report>();
    public DbSet<UserVoted> UserVoteds => Set<UserVoted>();
    
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await base.SaveChangesAsync(cancellationToken);
    }
}
