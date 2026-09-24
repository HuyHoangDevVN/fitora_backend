using BuildingBlocks.CQRS;
using BuildingBlocks.DTOs;
using BuildingBlocks.Exceptions;
using BuildingBlocks.RepositoryBase.EntityFramework;
using BuildingBlocks.Security;
using UserService.Domain.Enums;
using UserService.Domain.Models;

namespace UserService.Application.Usecases.GroupJoinRequest.Commands.ApproveJoinRequest;

public class ApproveJoinRequestHandler(
    IRepositoryBase<UserService.Domain.Models.GroupJoinRequest> requestRepo,
    IRepositoryBase<Domain.Models.GroupMember> memberRepo,
    IAuthorizeExtension auth) : ICommandHandler<ApproveJoinRequestCommand, ResponseDto>
{
    public async Task<ResponseDto> Handle(ApproveJoinRequestCommand req, CancellationToken ct)
    {
        var callerId = auth.GetUserFromClaimToken().Id;
        var joinRequest = await requestRepo.GetAsync(r => r.Id == req.RequestId, ct)
            ?? throw new NotFoundException("Yêu cầu tham gia không tồn tại");

        var callerMembership = await memberRepo.GetAsync(
            m => m.GroupId == joinRequest.GroupId && m.UserId == callerId, ct);
        if (callerMembership is null || callerMembership.Role is not (GroupRole.Owner or GroupRole.Admin or GroupRole.Moderator))
            throw new UnAuthorizationException("Chỉ Owner/Admin/Moderator được duyệt yêu cầu tham gia");

        if (joinRequest.Status != GroupJoinRequestStatus.Pending)
            return new ResponseDto(null, false, "Yêu cầu đã được xử lý trước đó");

        var alreadyMember = await memberRepo.GetAsync(
            m => m.GroupId == joinRequest.GroupId && m.UserId == joinRequest.UserId, ct);
        if (alreadyMember is null)
        {
            await memberRepo.AddAsync(new Domain.Models.GroupMember
            {
                Id = Guid.NewGuid(),
                GroupId = joinRequest.GroupId,
                UserId = joinRequest.UserId,
                Role = GroupRole.Member,
                JoinedAt = DateTime.UtcNow,
            }, ct);
        }

        joinRequest.Status = GroupJoinRequestStatus.Approved;
        joinRequest.ReviewedAt = DateTime.UtcNow;
        joinRequest.ReviewedBy = callerId;

        await requestRepo.UpdateAsync(r => r.Id == joinRequest.Id, joinRequest, ct);
        var ok = await memberRepo.SaveChangesAsync(ct) >= 0;
        await requestRepo.SaveChangesAsync(ct);
        return new ResponseDto(ok, IsSuccess: true, Message: "Đã duyệt yêu cầu tham gia");
    }
}
