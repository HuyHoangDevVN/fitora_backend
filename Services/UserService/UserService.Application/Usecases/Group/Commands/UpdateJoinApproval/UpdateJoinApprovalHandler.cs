using BuildingBlocks.CQRS;
using BuildingBlocks.DTOs;
using BuildingBlocks.Exceptions;
using BuildingBlocks.RepositoryBase.EntityFramework;
using BuildingBlocks.Security;
using UserService.Domain.Enums;

namespace UserService.Application.Usecases.Group.Commands.UpdateJoinApproval;

public class UpdateJoinApprovalHandler(
    IRepositoryBase<Domain.Models.Group> groupRepo,
    IRepositoryBase<Domain.Models.GroupMember> memberRepo,
    IAuthorizeExtension auth) : ICommandHandler<UpdateJoinApprovalCommand, ResponseDto>
{
    public async Task<ResponseDto> Handle(UpdateJoinApprovalCommand req, CancellationToken ct)
    {
        var callerId = auth.GetUserFromClaimToken().Id;
        var membership = await memberRepo.GetAsync(m => m.GroupId == req.GroupId && m.UserId == callerId, ct);
        if (membership is null) throw new UnAuthorizationException("Bạn không phải thành viên của nhóm");
        if (membership.Role is not (GroupRole.Owner or GroupRole.Admin))
            throw new UnAuthorizationException("Chỉ Owner/Admin được đổi chế độ duyệt tham gia");

        var group = await groupRepo.GetAsync(g => g.Id == req.GroupId, ct)
            ?? throw new NotFoundException("Nhóm không tồn tại");

        group.RequireJoinApproval = req.RequireJoinApproval;
        await groupRepo.UpdateAsync(g => g.Id == group.Id, group, ct);
        var ok = await groupRepo.SaveChangesAsync(ct) > 0;
        return new ResponseDto(ok, IsSuccess: ok, Message: ok ? "Đã cập nhật chế độ duyệt tham gia" : "Cập nhật thất bại");
    }
}
