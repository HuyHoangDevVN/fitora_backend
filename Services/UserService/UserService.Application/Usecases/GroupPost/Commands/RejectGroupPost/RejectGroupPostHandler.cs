using BuildingBlocks.CQRS;
using BuildingBlocks.DTOs;
using BuildingBlocks.Exceptions;
using BuildingBlocks.RepositoryBase.EntityFramework;
using BuildingBlocks.Security;
using UserService.Domain.Enums;

namespace UserService.Application.Usecases.GroupPost.Commands.RejectGroupPost;

public class RejectGroupPostHandler(
    IRepositoryBase<Domain.Models.GroupPost> postRepo,
    IRepositoryBase<Domain.Models.GroupMember> memberRepo,
    IAuthorizeExtension auth) : ICommandHandler<RejectGroupPostCommand, ResponseDto>
{
    public async Task<ResponseDto> Handle(RejectGroupPostCommand req, CancellationToken ct)
    {
        var callerId = auth.GetUserFromClaimToken().Id;
        var post = await postRepo.GetAsync(p => p.PostId == req.PostId, ct)
            ?? throw new NotFoundException("Bài viết trong nhóm không tồn tại");

        var membership = await memberRepo.GetAsync(
            m => m.GroupId == post.GroupId && m.UserId == callerId, ct);
        if (membership is null) throw new UnAuthorizationException("Bạn không phải thành viên của nhóm");
        if (membership.Role is not (GroupRole.Owner or GroupRole.Admin or GroupRole.Moderator))
            throw new UnAuthorizationException("Chỉ Owner/Admin/Moderator được từ chối bài");

        if (post.ApprovalStatus == ApprovalStatus.Rejected)
            return new ResponseDto(null, false, "Bài đã bị từ chối trước đó");

        post.ApprovalStatus = ApprovalStatus.Rejected;
        post.RejectedAt = DateTime.UtcNow;
        post.RejectionReason = req.Reason;
        post.IsApproved = false;

        await postRepo.UpdateAsync(p => p.PostId == post.PostId, post, ct);
        var ok = await postRepo.SaveChangesAsync(ct) > 0;
        return new ResponseDto(ok, IsSuccess: ok, Message: ok ? "Từ chối bài thành công" : "Từ chối bài thất bại");
    }
}
