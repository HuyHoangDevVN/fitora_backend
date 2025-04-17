namespace UserService.Application.Services.IServices;

public interface IInteractApiService
{
    Task<bool> ApproveGroupPost(Guid groupId, Guid postId, CancellationToken cancellationToken);
}