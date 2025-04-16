using System.Reflection;
using Microsoft.EntityFrameworkCore;
using UserService.Application.Data;

namespace UserService.Infrastructure.Data;

public class ApplicationDbContext : DbContext, IApplicationDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }
    
    public DbSet<User> Users => Set<User>();
    public DbSet<UserInfo> UserInfos => Set<UserInfo>();
    public DbSet<FriendRequest> FriendRequests => Set<FriendRequest>();
    public DbSet<FriendShip> FriendShips => Set<FriendShip>();
    public DbSet<Follow> Follows => Set<Follow>();
    public DbSet<Block> Blocks => Set<Block>();
    public DbSet<Group> Groups => Set<Group>();
    public DbSet<GroupInvite> GroupInvites => Set<GroupInvite>();
    public DbSet<GroupMember> GroupMembers => Set<GroupMember>();
    public DbSet<GroupPost> GroupPosts => Set<GroupPost>();
    public DbSet<GroupEvent> GroupEvents => Set<GroupEvent>();
    public DbSet<GroupJoinRequest> GroupJoinRequests => Set<GroupJoinRequest>();
    public DbSet<GroupLog> GroupLogs => Set<GroupLog>();
    public DbSet<GroupNotification> GroupNotifications => Set<GroupNotification>();
    public DbSet<EventRsvp> EventRsvps => Set<EventRsvp>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        builder.Entity<GroupJoinRequest>()
            .HasOne(gjr => gjr.User)               
            .WithMany()                            
            .HasForeignKey(gjr => gjr.UserId)      
            .IsRequired();                         

        
        builder.Entity<GroupJoinRequest>()
            .HasOne(gjr => gjr.ReviewedByUser)     
            .WithMany()                            
            .HasForeignKey(gjr => gjr.ReviewedBy)  
            .IsRequired(false);
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await base.SaveChangesAsync(cancellationToken);
    }
}
