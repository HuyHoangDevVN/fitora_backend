namespace UserService.Application.Data;

public interface IApplicationDbContext
{
    DbSet<User> Users { get; }
    DbSet<UserInfo> UserInfos { get; }
    DbSet<FriendRequest> FriendRequests { get; }
    DbSet<FriendShip> FriendShips { get; }
    DbSet<Follow> Follows { get; }
    DbSet<Block> Blocks { get; }
    DbSet<Group> Groups { get; }
    DbSet<GroupInvite> GroupInvites { get; }
    DbSet<GroupMember> GroupMembers { get; }
    
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}