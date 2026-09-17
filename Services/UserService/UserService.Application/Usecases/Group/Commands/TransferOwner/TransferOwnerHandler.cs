using BuildingBlocks.CQRS;
using BuildingBlocks.DTOs;
using BuildingBlocks.Exceptions;
using BuildingBlocks.RepositoryBase.EntityFramework;
using BuildingBlocks.Security;
using UserService.Domain.Enums;

namespace UserService.Application.Usecases.Group.Commands.TransferOwner;

public class TransferOwnerHandler(
    IRepositoryBase<Domain.Models.GroupMember> memberRepo,
    IAuthorizeExtension auth) : ICommandHandler<TransferOwnerCommand, ResponseDto>
{
    public async Task<ResponseDto> Handle(TransferOwnerCommand req, CancellationToken ct)
    {
        var callerId = auth.GetUserFromClaimToken().Id;

        var callerMembership = await memberRepo.GetAsync(
            m => m.GroupId == req.GroupId && m.UserId == callerId, ct);
        if (callerMembership is null || callerMembership.Role != GroupRole.Owner)
            throw new UnAuthorizationException("Chỉ Owner mới được chuyển quyền sở hữu");

        if (req.NewOwnerId == callerId)
            return new ResponseDto(null, false, "Bạn đã là Owner");

        var newOwnerMembership = await memberRepo.GetAsync(
            m => m.GroupId == req.GroupId && m.UserId == req.NewOwnerId, ct)
            ?? throw new NotFoundException("Người nhận quyền không phải thành viên của nhóm");

        // Downgrade caller to Admin, promote new owner
        callerMembership.Role = GroupRole.Admin;
        newOwnerMembership.Role = GroupRole.Owner;

        await memberRepo.UpdateAsync(m => m.Id == callerMembership.Id, callerMembership, ct);
        await memberRepo.UpdateAsync(m => m.Id == newOwnerMembership.Id, newOwnerMembership, ct);
        var ok = await memberRepo.SaveChangesAsync(ct) > 0;
        return new ResponseDto(ok, IsSuccess: ok, Message: ok ? "Chuyển quyền sở hữu thành công" : "Chuyển quyền sở hữu thất bại");
    }
}
