using BuildingBlocks.CQRS;
using BuildingBlocks.DTOs;
using BuildingBlocks.Exceptions;
using BuildingBlocks.RepositoryBase.EntityFramework;
using BuildingBlocks.Security;
using UserService.Domain.Enums;

namespace UserService.Application.Usecases.GroupPost.Commands.ApproveGroupPost;

public class ApproveGroupPostHandler(
    IRepositoryBase<Domain.Models.GroupPost> postRepo,
    IRepositoryBase<Domain.Models.GroupMember> memberRepo,
    IAuthorizeExtension auth) : ICommandHandler<ApproveGroupPostCommand, ResponseDto>
{
    public async Task<ResponseDto> Handle(ApproveGroupPostCommand req, CancellationToken ct)
    {
        var callerId = auth.GetUserFromClaimToken().Id;
        var post = await postRepo.GetAsync(p => p.PostId == req.PostId, ct)
            ?? throw new NotFoundException("Bài viết trong nhóm không tồn tại");

        var membership = await memberRepo.GetAsync(
            m => m.GroupId == post.GroupId && m.UserId == callerId, ct);
        if (membership is null) throw new UnAuthorizationException("Bạn không phải thành viên của nhóm");
        if (membership.Role is not (GroupRole.Owner or GroupRole.Admin or GroupRole.Moderator))
            throw new UnAuthorizationException("Chỉ Owner/Admin/Moderator được duyệt bài");

        if (post.ApprovalStatus == ApprovalStatus.Approved)
            return new ResponseDto(null, false, "Bài đã được duyệt trước đó");

        post.ApprovalStatus = ApprovalStatus.Approved;
        post.ApprovedAt = DateTime.UtcNow;
        post.RejectedAt = null;
        post.RejectionReason = null;
        post.IsApproved = true;

        await postRepo.UpdateAsync(p => p.PostId == post.PostId, post, ct);
        var ok = await postRepo.SaveChangesAsync(ct) > 0;
        return new ResponseDto(ok, IsSuccess: ok, Message: ok ? "Duyệt bài thành công" : "Duyệt bài thất bại");
    }
}
