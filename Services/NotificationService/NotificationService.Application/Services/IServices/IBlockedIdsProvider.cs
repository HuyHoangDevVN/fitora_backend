namespace NotificationService.Application.Services.IServices;

public interface IBlockedIdsProvider
{
    Task<IReadOnlySet<Guid>> GetBlockedUserIdsAsync(Guid currentUserId, CancellationToken ct = default);
    Task<IReadOnlySet<Guid>> GetBlockedGroupIdsAsync(Guid currentUserId, CancellationToken ct = default);
    Task<(IReadOnlySet<Guid> userIds, IReadOnlySet<Guid> groupIds)> GetBlockedIdsAsync(Guid currentUserId, CancellationToken ct = default);
}
