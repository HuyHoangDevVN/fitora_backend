using BuildingBlocks.DTOs;
using BuildingBlocks.RepositoryBase.EntityFramework;

namespace UserService.Application.Usecases.GroupInvite.Commands.CreateGroupInvites;

public class CreateGroupInvitesHandler(
    IGroupInviteRepository groupInviteRepo,
    IRepositoryBase<Domain.Models.Group> groupRepo,
    IRepositoryBase<Domain.Models.GroupMember> memberRepo,
    IRepositoryBase<Domain.Models.GroupJoinRequest> joinRequestRepo)
    : ICommandHandler<CreateGroupInvitesCommand, ResponseDto>
{
    public async Task<ResponseDto> Handle(CreateGroupInvitesCommand command, CancellationToken ct)
    {
        var group = await groupRepo.GetAsync(g => g.Id == command.Request.GroupId, ct);
        if (group is null) return new ResponseDto(null, false, "Nhóm không tồn tại");

        if (!group.RequireJoinApproval)
            return await groupInviteRepo.CreateRangeAsync(command.Request);

        // RequireJoinApproval = true: lời mời cũng phải chờ duyệt -> tạo GroupJoinRequest Pending thay vì GroupInvite trực tiếp
        var toCreate = new List<Domain.Models.GroupJoinRequest>();
        foreach (var receiverId in command.Request.ReceiverUserIds)
        {
            var alreadyMember = await memberRepo.GetAsync(m => m.GroupId == command.Request.GroupId && m.UserId == receiverId, ct);
            if (alreadyMember is not null) continue;
            var alreadyPending = await joinRequestRepo.GetAsync(
                r => r.GroupId == command.Request.GroupId && r.UserId == receiverId && r.Status == Domain.Enums.GroupJoinRequestStatus.Pending, ct);
            if (alreadyPending is not null) continue;
            toCreate.Add(new Domain.Models.GroupJoinRequest
            {
                Id = Guid.NewGuid(),
                GroupId = command.Request.GroupId,
                UserId = receiverId,
                Status = Domain.Enums.GroupJoinRequestStatus.Pending,
                RequestedAt = DateTime.UtcNow,
            });
        }
        if (toCreate.Count == 0) return new ResponseDto(null, false, "Không có lời mời mới (đã là thành viên hoặc đang chờ duyệt)");
        await joinRequestRepo.AddRangeAsync(toCreate, ct);
        var ok = await joinRequestRepo.SaveChangesAsync(ct) > 0;
        return new ResponseDto(toCreate.Select(x => x.Id), ok, ok ? $"Đã tạo {toCreate.Count} yêu cầu chờ duyệt" : "Tạo yêu cầu thất bại");
    }
}