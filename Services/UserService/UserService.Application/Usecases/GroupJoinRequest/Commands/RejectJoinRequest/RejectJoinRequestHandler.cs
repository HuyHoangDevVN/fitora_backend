using BuildingBlocks.CQRS;
using BuildingBlocks.DTOs;
using BuildingBlocks.Exceptions;
using BuildingBlocks.RepositoryBase.EntityFramework;
using BuildingBlocks.Security;
using UserService.Domain.Enums;

namespace UserService.Application.Usecases.GroupJoinRequest.Commands.RejectJoinRequest;

public class RejectJoinRequestHandler(
    IRepositoryBase<UserService.Domain.Models.GroupJoinRequest> requestRepo,
    IRepositoryBase<Domain.Models.GroupMember> memberRepo,
    IAuthorizeExtension auth) : ICommandHandler<RejectJoinRequestCommand, ResponseDto>
{
    public async Task<ResponseDto> Handle(RejectJoinRequestCommand req, CancellationToken ct)
    {
        var callerId = auth.GetUserFromClaimToken().Id;
        var joinRequest = await requestRepo.GetAsync(r => r.Id == req.RequestId, ct)
            ?? throw new NotFoundException("Yêu cầu tham gia không tồn tại");

        var callerMembership = await memberRepo.GetAsync(
            m => m.GroupId == joinRequest.GroupId && m.UserId == callerId, ct);
        if (callerMembership is null || callerMembership.Role is not (GroupRole.Owner or GroupRole.Admin or GroupRole.Moderator))
            throw new UnAuthorizationException("Chỉ Owner/Admin/Moderator được từ chối yêu cầu tham gia");

        if (joinRequest.Status != GroupJoinRequestStatus.Pending)
            return new ResponseDto(null, false, "Yêu cầu đã được xử lý trước đó");

        joinRequest.Status = GroupJoinRequestStatus.Rejected;
        joinRequest.ReviewedAt = DateTime.UtcNow;
        joinRequest.ReviewedBy = callerId;
        joinRequest.ReviewComment = req.Reason;

        await requestRepo.UpdateAsync(r => r.Id == joinRequest.Id, joinRequest, ct);
        var ok = await requestRepo.SaveChangesAsync(ct) > 0;
        return new ResponseDto(ok, IsSuccess: ok, Message: ok ? "Đã từ chối yêu cầu tham gia" : "Thao tác thất bại");
    }
}
