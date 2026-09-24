using BuildingBlocks.CQRS;
using BuildingBlocks.DTOs;
using BuildingBlocks.Exceptions;
using BuildingBlocks.RepositoryBase.EntityFramework;
using BuildingBlocks.Security;
using UserService.Domain.Enums;

namespace UserService.Application.Usecases.GroupMember.Commands.LeaveGroup;

/// <summary>
/// Owner phải chuyển quyền sở hữu (TransferOwner) trước khi rời — nhất quán với
/// rule Dissolve/TransferOwner: nhóm không được để trống Owner.
/// </summary>
public class LeaveGroupHandler(
    IRepositoryBase<Domain.Models.GroupMember> memberRepo,
    IAuthorizeExtension auth) : ICommandHandler<LeaveGroupCommand, ResponseDto>
{
    public async Task<ResponseDto> Handle(LeaveGroupCommand req, CancellationToken ct)
    {
        var callerId = auth.GetUserFromClaimToken().Id;
        var membership = await memberRepo.GetAsync(
            m => m.GroupId == req.GroupId && m.UserId == callerId, ct)
            ?? throw new NotFoundException("Bạn không phải thành viên của nhóm này");

        if (membership.Role == GroupRole.Owner)
            throw new BadRequestException(
                "Bạn là Owner của nhóm. Hãy chuyển quyền sở hữu cho người khác trước khi rời nhóm.");

        await memberRepo.DeleteAsync(m => m.Id == membership.Id, ct);
        var ok = await memberRepo.SaveChangesAsync(ct) > 0;
        return new ResponseDto(ok, IsSuccess: ok, Message: ok ? "Rời nhóm thành công" : "Rời nhóm thất bại");
    }
}
