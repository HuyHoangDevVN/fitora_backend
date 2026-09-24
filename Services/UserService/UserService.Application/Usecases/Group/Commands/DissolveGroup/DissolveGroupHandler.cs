using BuildingBlocks.CQRS;
using BuildingBlocks.DTOs;
using BuildingBlocks.Exceptions;
using BuildingBlocks.RepositoryBase.EntityFramework;
using BuildingBlocks.Security;
using UserService.Domain.Enums;

namespace UserService.Application.Usecases.Group.Commands.DissolveGroup;

public class DissolveGroupHandler(
    IRepositoryBase<Domain.Models.Group> groupRepo,
    IRepositoryBase<Domain.Models.GroupMember> memberRepo,
    IAuthorizeExtension auth) : ICommandHandler<DissolveGroupCommand, ResponseDto>
{
    public async Task<ResponseDto> Handle(DissolveGroupCommand req, CancellationToken ct)
    {
        var callerId = auth.GetUserFromClaimToken().Id;
        var membership = await memberRepo.GetAsync(
            m => m.GroupId == req.GroupId && m.UserId == callerId, ct);
        if (membership is null || membership.Role != GroupRole.Owner)
            throw new UnAuthorizationException("Chỉ Owner mới được giải tán nhóm");

        var group = await groupRepo.GetAsync(g => g.Id == req.GroupId, ct)
            ?? throw new NotFoundException("Nhóm không tồn tại");

        // Delete members first (FK cascade would also handle but do explicitly)
        await memberRepo.DeleteRangeAsync(m => m.GroupId == req.GroupId, ct);
        await groupRepo.DeleteAsync(g => g.Id == req.GroupId, ct);
        var ok = await groupRepo.SaveChangesAsync(ct) > 0;
        // Also flush members deletion that was staged on memberRepo's context —
        // they share the same DbContext, so one SaveChanges suffices, but call both to be safe
        await memberRepo.SaveChangesAsync(ct);

        return new ResponseDto(ok, IsSuccess: ok, Message: ok ? "Giải tán nhóm thành công" : "Giải tán nhóm thất bại");
    }
}
