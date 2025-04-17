namespace InteractService.Application.Services.IServices;

public interface IUserApiService
{
    Task<bool> CreateGroupPost(Guid groupId, Guid postId, bool isApproved, CancellationToken cancellationToken);
}