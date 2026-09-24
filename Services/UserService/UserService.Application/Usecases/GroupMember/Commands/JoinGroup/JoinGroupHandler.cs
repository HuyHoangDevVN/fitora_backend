using BuildingBlocks.CQRS;
using BuildingBlocks.DTOs;
using BuildingBlocks.Exceptions;
using BuildingBlocks.RepositoryBase.EntityFramework;
using BuildingBlocks.Security;
using UserService.Domain.Enums;

namespace UserService.Application.Usecases.GroupMember.Commands.JoinGroup;

/// <summary>
/// Nếu nhóm bật RequireJoinApproval: mọi yêu cầu tham gia (kể cả Public) đều tạo GroupJoinRequest chờ duyệt.
/// Nếu tắt: tham gia ngay (tạo GroupMember role Member), bất kể Privacy.
/// </summary>
public class JoinGroupHandler(
    IRepositoryBase<Domain.Models.Group> groupRepo,
    IRepositoryBase<Domain.Models.GroupMember> memberRepo,
    IRepositoryBase<Domain.Models.GroupJoinRequest> joinRequestRepo,
    IAuthorizeExtension auth) : ICommandHandler<JoinGroupCommand, ResponseDto>
{
    public async Task<ResponseDto> Handle(JoinGroupCommand req, CancellationToken ct)
    {
        var callerId = auth.GetUserFromClaimToken().Id;

        var group = await groupRepo.GetAsync(g => g.Id == req.GroupId, ct)
            ?? throw new NotFoundException("Nhóm không tồn tại");

        var existingMembership = await memberRepo.GetAsync(
            m => m.GroupId == req.GroupId && m.UserId == callerId, ct);
        if (existingMembership is not null)
            return new ResponseDto(new { alreadyMember = true }, false, "Bạn đã là thành viên của nhóm này");

        if (!group.RequireJoinApproval)
        {
            await memberRepo.AddAsync(new Domain.Models.GroupMember
            {
                Id = Guid.NewGuid(),
                GroupId = req.GroupId,
                UserId = callerId,
                Role = GroupRole.Member,
                JoinedAt = DateTime.UtcNow,
            }, ct);
            var ok = await memberRepo.SaveChangesAsync(ct) > 0;
            return new ResponseDto(new { joined = true }, ok, ok ? "Tham gia nhóm thành công" : "Tham gia nhóm thất bại");
        }

        var pendingRequest = await joinRequestRepo.GetAsync(
            r => r.GroupId == req.GroupId && r.UserId == callerId && r.Status == GroupJoinRequestStatus.Pending, ct);
        if (pendingRequest is not null)
            return new ResponseDto(new { pending = true }, false, "Bạn đã gửi yêu cầu tham gia, vui lòng chờ duyệt");

        await joinRequestRepo.AddAsync(new Domain.Models.GroupJoinRequest
        {
            Id = Guid.NewGuid(),
            GroupId = req.GroupId,
            UserId = callerId,
            Status = GroupJoinRequestStatus.Pending,
            RequestedAt = DateTime.UtcNow,
        }, ct);
        var created = await joinRequestRepo.SaveChangesAsync(ct) > 0;
        return new ResponseDto(new { pending = true }, created, created ? "Đã gửi yêu cầu tham gia, vui lòng chờ duyệt" : "Gửi yêu cầu thất bại");
    }
}
