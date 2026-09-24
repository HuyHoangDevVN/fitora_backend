using InteractService.Application.Services.IServices;
using InteractService.Infrastructure.Grpc;

namespace InteractService.Infrastructure.Services;

public class UserInfoBatchService : IUserInfoBatchService
{
    private readonly UserGrpcClient _userClient;

    public UserInfoBatchService(UserGrpcClient userClient)
    {
        _userClient = userClient;
    }

    public async Task<Dictionary<Guid, UserDisplayInfo>> GetUserDisplayInfosAsync(
        Guid? requestUserId, List<Guid> userIds, CancellationToken cancellationToken)
    {
        if (userIds.Count == 0) return new Dictionary<Guid, UserDisplayInfo>();

        var users = await _userClient.GetUserInfoBatchAsync(requestUserId, userIds.Select(x => x.ToString()).ToList());
        return users.ToDictionary(u => u.Id, u => new UserDisplayInfo(u.Id, u.Username, u.ProfilePictureUrl));
    }
}
