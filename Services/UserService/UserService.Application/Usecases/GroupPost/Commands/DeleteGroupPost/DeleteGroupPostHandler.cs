using BuildingBlocks.DTOs;
using BuildingBlocks.Exceptions;
using BuildingBlocks.RepositoryBase.EntityFramework;
using BuildingBlocks.Security;
using UserService.Domain.Enums;

namespace UserService.Application.Usecases.GroupPost.Commands.DeleteGroupPost;

/// <summary>
/// Chỉ tác giả bài, hoặc Owner/Admin/Moderator của nhóm chứa bài đó, mới được xóa.
/// Trước đây handler này gọi thẳng repo không check gì — bất kỳ user đăng nhập nào
/// biết PostId đều xóa được bài của nhóm bất kỳ.
/// </summary>
public class DeleteGroupPostHandler(
    IGroupPostRepository groupPostRepo,
    IRepositoryBase<Domain.Models.GroupPost> groupPostBase,
    IRepositoryBase<Domain.Models.GroupMember> memberRepo,
    IAuthorizeExtension authorize) : ICommandHandler<DeleteGroupPostCommand, ResponseDto>
{
    public async Task<ResponseDto> Handle(DeleteGroupPostCommand request, CancellationToken cancellationToken)
    {
        var existing = await groupPostBase.GetAsync(
            x => x.PostId == request.Id, cancellationToken);
        if (existing is null) throw new NotFoundException("Group post not found");

        var callerId = authorize.GetUserFromClaimToken().Id;
        if (existing.AuthorId != callerId)
        {
            var membership = await memberRepo.GetAsync(
                m => m.GroupId == existing.GroupId && m.UserId == callerId, cancellationToken);
            if (membership is null || membership.Role is not (GroupRole.Owner or GroupRole.Admin or GroupRole.Moderator))
                throw new UnAuthorizationException("Bạn không có quyền xóa bài viết này.");
        }

        var result = await groupPostRepo.DeleteAsync(request.Id);
        return new ResponseDto(result);
    }
}