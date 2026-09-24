using BuildingBlocks.DTOs;
using BuildingBlocks.Exceptions;
using BuildingBlocks.RepositoryBase.EntityFramework;
using BuildingBlocks.Security;

namespace UserService.Application.Usecases.GroupPost.Commands.UpdateGroupPost;

/// <summary>
/// FE counts down "còn MM:SS" (PostBox 13.3) but server is the gate:
/// - Pending (IsApproved==false): author may edit regardless of age.
/// - Approved (IsApproved==true): author only within 15 minutes of GroupPost.CreatedAt.
/// - Caller must be the author (author/role check for delete is elsewhere).
/// </summary>
public class UpdateGroupPostHandler(
    IGroupPostRepository groupPostRepo,
    IRepositoryBase<Domain.Models.GroupPost> groupPostBase,
    IAuthorizeExtension authorize) : ICommandHandler<UpdateGroupPostCommand, ResponseDto>
{
    private static readonly TimeSpan EditWindow = TimeSpan.FromMinutes(15);

    public async Task<ResponseDto> Handle(UpdateGroupPostCommand request, CancellationToken cancellationToken)
    {
        var existing = await groupPostBase.GetAsync(
            x => x.PostId == request.Request.PostId, cancellationToken);
        if (existing is null) throw new NotFoundException("Group post not found");

        var callerId = authorize.GetUserFromClaimToken().Id;
        if (existing.AuthorId != callerId)
            throw new UnAuthorizationException("Chỉ tác giả bài mới được chỉnh sửa.");

        if (existing.IsApproved
            && existing.CreatedAt.HasValue
            && DateTime.UtcNow - existing.CreatedAt.Value > EditWindow)
        {
            throw new BadRequestException(
                "Hết thời gian chỉnh sửa (15 phút). Bài đã được duyệt quá hạn.");
        }

        // Apply only mutable approved flag; GroupId/AuthorId/PostId are identity.
        existing.GroupId = request.Request.GroupId;
        existing.IsApproved = request.Request.IsApproved;

        var ok = await groupPostRepo.UpdateAsync(existing);
        return new ResponseDto(ok, IsSuccess: ok, Message: ok ? "Cập nhật bài trong nhóm thành công" : "Cập nhật thất bại");
    }
}
