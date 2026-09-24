using BuildingBlocks.DTOs;
using BuildingBlocks.Exceptions;
using BuildingBlocks.RepositoryBase.EntityFramework;
using BuildingBlocks.Security;
using UserService.Domain.Enums;

namespace UserService.Application.Usecases.GroupEvent.Commands.DeleteGroupEvent;

public class DeleteGroupEventHandler(
    IRepositoryBase<Domain.Models.GroupEvent> eventRepo,
    IRepositoryBase<Domain.Models.GroupMember> memberRepo,
    IAuthorizeExtension auth) : ICommandHandler<DeleteGroupEventCommand, ResponseDto>
{
    public async Task<ResponseDto> Handle(DeleteGroupEventCommand request, CancellationToken ct)
    {
        var callerId = auth.GetUserFromClaimToken().Id;
        var ev = await eventRepo.GetAsync(e => e.Id == request.EventId, ct)
            ?? throw new NotFoundException("Sự kiện không tồn tại");

        var membership = await memberRepo.GetAsync(
            m => m.GroupId == ev.GroupId && m.UserId == callerId, ct);
        if (membership is null)
            throw new UnAuthorizationException("Chỉ thành viên mới xóa sự kiện.");
        if (membership.Role is not (GroupRole.Owner or GroupRole.Admin or GroupRole.Moderator))
            throw new UnAuthorizationException("Chỉ Owner/Admin/Moderator được xóa sự kiện.");

        await eventRepo.DeleteAsync(e => e.Id == ev.Id, ct);
        var ok = await eventRepo.SaveChangesAsync(ct) > 0;
        return new ResponseDto(ok, IsSuccess: ok, Message: ok ? "Xóa sự kiện thành công" : "Xóa sự kiện thất bại");
    }
}
